using AquaPP.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AquaPP.Services.Units;
using ILogger = Serilog.ILogger;
// ReSharper disable InconsistentNaming

namespace AquaPP.Services.Csv;

public class WaterQualityCsvService
{
    private readonly ILogger _logger;
    private readonly IUnitConversionService _unitConversionService;

    public WaterQualityCsvService(ILogger logger, IUnitConversionService unitConversionService)
    {
        _logger = logger;
        _unitConversionService = unitConversionService;
    }

    public async Task<bool> ExportWaterQualityData(IEnumerable<WaterQualityReading> readings, string filePath, string conductivityUnit, string turbidityUnit, string chlorineUnit)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            await using var writer = new StreamWriter(filePath);
            await using var csv = new CsvWriter(writer, config);

            // Write header manually
            csv.WriteField("SampleId");
            csv.WriteField("Timestamp");
            csv.WriteField("Location");
            csv.WriteField("PH");
            csv.WriteField($"Conductivity ({conductivityUnit})");
            csv.WriteField($"Turbidity ({turbidityUnit})");
            csv.WriteField($"ChlorineResidual ({chlorineUnit})");
            csv.WriteField("Notes");
            await csv.NextRecordAsync();

            // Write records
            foreach (var reading in readings)
            {
                csv.WriteField(reading.SampleId);
                csv.WriteField(reading.Timestamp);
                csv.WriteField(reading.Location);
                csv.WriteField(reading.PH);
                csv.WriteField(reading.Conductivity);
                csv.WriteField(reading.Turbidity);
                csv.WriteField(reading.ChlorineResidual);
                csv.WriteField(reading.Notes);
                await csv.NextRecordAsync();
            }
            _logger.Information("Successfully exported water quality data to {FilePath}", filePath);
            return true;
        }
        catch (IOException ex)
        {
            _logger.Error(ex, "Error writing to file {FilePath}: {ExMessage}", filePath, ex.Message);
            return false;
        }
        catch (CsvHelperException ex)
        {
            _logger.Error(ex, "Error during CSV export to {FilePath}: {ExMessage}", filePath, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An unexpected error occurred during export to {FilePath}: {ExMessage}", filePath, ex.Message);
            return false;
        }
    }
    
    [Experimental("DiagnosticId")]
    public Task<IEnumerable<WaterQualityReading>> ImportWaterQualityData(string filePath)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord;
            if (headers is null)
            {
                throw new InvalidDataException("CSV file does not contain a header row.");
            }

            // Parse units from headers
            var conductivityHeader = headers.FirstOrDefault(h => h.StartsWith("Conductivity")) ?? "Conductivity";
            var turbidityHeader = headers.FirstOrDefault(h => h.StartsWith("Turbidity")) ?? "Turbidity";
            var chlorineHeader = headers.FirstOrDefault(h => h.StartsWith("ChlorineResidual")) ?? "ChlorineResidual";

            var conductivityUnit = ParseUnitFromHeader(conductivityHeader, UnitConversionService.BaseConductivityUnit);
            var turbidityUnit = ParseUnitFromHeader(turbidityHeader, UnitConversionService.BaseTurbidityUnit);
            var chlorineUnit = ParseUnitFromHeader(chlorineHeader, UnitConversionService.BaseChlorineUnit);

            var records = csv.GetRecords<dynamic>().ToList();
            var readings = new List<WaterQualityReading>();

            foreach (var record in records)
            {
                var dict = (IDictionary<string, object>)record;
                var reading = new WaterQualityReading
                {
                    SampleId = dict["SampleId"].ToString() ?? string.Empty,
                    Timestamp = DateTime.TryParse(dict["Timestamp"].ToString(), out var timestamp) ? timestamp : DateTime.Now,
                    Location = dict["Location"].ToString() ?? string.Empty,
                    PH = double.TryParse(dict["PH"].ToString(), out var ph) ? ph : 0,
                    Notes = dict["Notes"].ToString() ?? string.Empty
                };

                var conductivityValue = double.TryParse(dict[conductivityHeader].ToString(), out var cond) ? cond : 0;
                var turbidityValue = double.TryParse(dict[turbidityHeader].ToString(), out var turb) ? turb : 0;
                var chlorineValue = double.TryParse(dict[chlorineHeader].ToString(), out var chl) ? chl : 0;

                reading.Conductivity = _unitConversionService.Convert(conductivityValue, conductivityUnit, UnitConversionService.BaseConductivityUnit, Parameter.Conductivity);
                reading.Turbidity = _unitConversionService.Convert(turbidityValue, turbidityUnit, UnitConversionService.BaseTurbidityUnit, Parameter.Turbidity);
                reading.ChlorineResidual = _unitConversionService.Convert(chlorineValue, chlorineUnit, UnitConversionService.BaseChlorineUnit, Parameter.Chlorine);

                readings.Add(reading);
            }

            _logger.Information($"Successfully imported {readings.Count} water quality readings from {filePath}");
            return Task.FromResult<IEnumerable<WaterQualityReading>>(readings);
        }
        catch (FileNotFoundException ex)
        {
            _logger.Error(ex, $"File not found at {filePath}: {ex.Message}");
            return Task.FromResult<IEnumerable<WaterQualityReading>>(new List<WaterQualityReading>());
        }
        catch (IOException ex)
        {
            _logger.Error(ex, $"Error reading from file {filePath}: {ex.Message}");
            return Task.FromResult<IEnumerable<WaterQualityReading>>(new List<WaterQualityReading>());
        }
        catch (CsvHelperException ex)
        {
            _logger.Error(ex, $"Error during CSV import from {filePath}: {ex.Message}");
            return Task.FromResult<IEnumerable<WaterQualityReading>>(new List<WaterQualityReading>());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"An unexpected error occurred during import from {filePath}: {ex.Message}");
            return Task.FromResult<IEnumerable<WaterQualityReading>>(new List<WaterQualityReading>());
        }
    }

    private string ParseUnitFromHeader(string header, string defaultUnit)
    {
        if (string.IsNullOrEmpty(header)) return defaultUnit;
        var match = Regex.Match(header, @"\((.*?)\)");
        return match.Success ? match.Groups[1].Value : defaultUnit;
    }
}
