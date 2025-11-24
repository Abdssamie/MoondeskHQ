using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet;
using Moondesk.Edge.Simulator;
using Microsoft.Extensions.Configuration;


var builder = new ConfigurationBuilder()
    // Set the base path to the directory where the application assembly resides
    .SetBasePath(Directory.GetCurrentDirectory())
    
    // 2. Load the main appsettings.json file
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    
    // 4. Overwrite settings with actual Environment Variables
    .AddEnvironmentVariables();

// 5. Build the final configuration object
IConfigurationRoot configuration = builder.Build();
var emqxConfig = configuration.GetRequiredSection("EMQX");

// Helper to get Env Var or Config
string GetConfig(string envVar, string configKey, string defaultValue = "") 
{
    string? env = Environment.GetEnvironmentVariable(envVar);
    if (!string.IsNullOrEmpty(env)) return env;
    return emqxConfig[configKey] ?? defaultValue;
}

string broker = GetConfig("MQTT_BROKER", "BROKER", "localhost");
int port = int.TryParse(Environment.GetEnvironmentVariable("MQTT_PORT"), out int p) ? p : Convert.ToInt32(emqxConfig["PORT"]);
string clientId = GetConfig("MQTT_CLIENTID", "CLIENTID", "moondesk-simulator");
string username = GetConfig("MQTT_USERNAME", "USERNAME", "");
string password = GetConfig("MQTT_PASSWORD", "PASSWORD", "pDIY4ekFrPgRtCl9TYRu");
bool useTls = bool.TryParse(emqxConfig["USETLS"], out var tls) && tls;

// Organization ID Logic: Env Var -> CLI -> Config -> Default
string? envOrgId = Environment.GetEnvironmentVariable("ORG_ID");
var deviceConfig = configuration.GetRequiredSection("Device");
string organizationId = envOrgId ?? deviceConfig["OrganizationId"] ?? "default_org";

// Override from command line args if present
string? cliOrgId = args.FirstOrDefault(a => a.StartsWith("--orgId="));
if (!string.IsNullOrEmpty(cliOrgId))
{
    var parts = cliOrgId.Split('=');
    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
    {
        organizationId = parts[1];
        Console.WriteLine($"Using Organization ID from CLI: {organizationId}");
    }
}
else if (!string.IsNullOrEmpty(envOrgId))
{
    Console.WriteLine($"Using Organization ID from Environment: {organizationId}");
}

if (string.IsNullOrWhiteSpace(organizationId) || organizationId == "default_org")
{
     Console.WriteLine("WARNING: Using default or empty Organization ID. Data may not be visible in your dashboard.");
}
else 
{
    Console.WriteLine($"Simulator configuring for Organization: {organizationId}");
    Console.WriteLine($"Connecting to Broker: {broker}:{port} as Client: {clientId}");
}

var factory = new MqttClientFactory();
var client = factory.CreateMqttClient();

var optionsBuilder = new MqttClientOptionsBuilder()
    .WithTcpServer(broker, port)
    .WithCredentials(username, password)
    .WithClientId(clientId)
    .WithCleanSession();

if (useTls)
{
    optionsBuilder.WithTlsOptions(tlsOptions =>
    {
        string certPath = "/home/abdssamie/Desktop/emqxsl-ca.crt";
        if (File.Exists(certPath))
        {
            string certFile = File.ReadAllText(certPath, Encoding.ASCII);
            
            var caChain = new X509Certificate2Collection();
            caChain.ImportFromPem(certFile);
            
            tlsOptions.WithCertificateValidationHandler(_ => true);
            tlsOptions.WithSslProtocols(SslProtocols.Tls12);
            tlsOptions.WithTrustChain(caChain);
        }
        else 
        {
            Console.WriteLine($"Warning: Certificate file not found at {certPath}. TLS might fail.");
        }
    });
}

var options = optionsBuilder.Build();

try
{
    await client.ConnectAsync(options: options, cancellationToken: CancellationToken.None);
    
    var task = Task.Run((() => SensorReadingSimulator.PublishAsync(client, organizationId)));
    var task1 = Task.Run((() => SensorReadingSimulator.SimulateAlert(client, organizationId)));

    try
    {
        await Task.WhenAll(task, task1);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}
catch (Exception exception)
{
    Console.WriteLine($@"Connection failed: {exception.Message}");
}