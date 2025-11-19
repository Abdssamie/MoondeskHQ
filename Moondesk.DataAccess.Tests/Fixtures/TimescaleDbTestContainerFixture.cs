using Testcontainers.PostgreSql;

namespace Moondesk.DataAccess.Tests.Fixtures;

public class TimescaleDbTestContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _timescaleDbContainer;

    public TimescaleDbTestContainerFixture()
    {
        _timescaleDbContainer = new PostgreSqlBuilder()
            .WithImage("timescale/timescaledb:latest-pg17")
            .Build();
    }

    public string ConnectionString => _timescaleDbContainer.GetConnectionString();
    
    public Task InitializeAsync() => _timescaleDbContainer.StartAsync();
    
    public Task DisposeAsync() => _timescaleDbContainer.StopAsync();
}