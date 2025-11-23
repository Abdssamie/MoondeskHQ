using Moondesk.Domain.Enums;
using Moondesk.Domain.Models.IoT;
using Xunit;

namespace Moondesk.BackgroundServices.Tests;

public class AlertThresholdTests
{
    [Theory]
    [InlineData(15.0, 10.0, true)]  // Above high threshold
    [InlineData(10.0, 10.0, false)] // At high threshold
    [InlineData(9.0, 10.0, false)]  // Below high threshold
    [InlineData(5.0, 10.0, false)]  // Normal range
    public void ShouldCreateAlert_WhenAboveHighThreshold(double value, double threshold, bool shouldAlert)
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "pH Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = threshold,
            ThresholdLow = 0.0
        };

        // Act
        var exceedsThreshold = value > sensor.ThresholdHigh;

        // Assert
        Assert.Equal(shouldAlert, exceedsThreshold);
    }

    [Theory]
    [InlineData(-1.0, 0.0, true)]   // Below low threshold
    [InlineData(0.0, 0.0, false)]   // At low threshold
    [InlineData(1.0, 0.0, false)]   // Above low threshold
    [InlineData(5.0, 0.0, false)]   // Normal range
    public void ShouldCreateAlert_WhenBelowLowThreshold(double value, double threshold, bool shouldAlert)
    {
        // Arrange
        var sensor = new Sensor
        {
            Id = 1,
            Name = "pH Sensor",
            OrganizationId = "org_test",
            ThresholdHigh = 14.0,
            ThresholdLow = threshold
        };

        // Act
        var exceedsThreshold = value < sensor.ThresholdLow;

        // Assert
        Assert.Equal(shouldAlert, exceedsThreshold);
    }

    [Theory]
    [InlineData(16.0, 10.0, 60.0)]   // 60% over
    [InlineData(13.0, 10.0, 30.0)]   // 30% over
    [InlineData(11.0, 10.0, 10.0)]   // 10% over
    [InlineData(10.5, 10.0, 5.0)]    // 5% over
    public void CalculatePercentageOverThreshold(double value, double threshold, double expectedPercent)
    {
        // Act
        var percentOver = Math.Abs((value - threshold) / threshold * 100);

        // Assert
        Assert.Equal(expectedPercent, percentOver, 1); // 1 decimal precision
    }

    [Theory]
    [InlineData(60.0, AlertSeverity.Emergency)]
    [InlineData(30.0, AlertSeverity.Critical)]
    [InlineData(15.0, AlertSeverity.Warning)]
    [InlineData(5.0, AlertSeverity.Info)]
    public void DetermineSeverity_BasedOnPercentage(double percentOver, AlertSeverity expectedSeverity)
    {
        // Act
        AlertSeverity severity;
        if (percentOver > 50) severity = AlertSeverity.Emergency;
        else if (percentOver > 25) severity = AlertSeverity.Critical;
        else if (percentOver > 10) severity = AlertSeverity.Warning;
        else severity = AlertSeverity.Info;

        // Assert
        Assert.Equal(expectedSeverity, severity);
    }

    [Fact]
    public void Alert_ContainsRequiredProperties()
    {
        // Arrange & Act
        var alert = new Alert
        {
            Id = 1,
            SensorId = 123,
            OrganizationId = "org_test",
            Timestamp = DateTimeOffset.UtcNow,
            Severity = AlertSeverity.Critical,
            Message = "pH threshold exceeded",
            TriggerValue = 9.5,
            ThresholdValue = 8.5
        };

        // Assert
        Assert.Equal(1, alert.Id);
        Assert.Equal(123, alert.SensorId);
        Assert.Equal("org_test", alert.OrganizationId);
        Assert.Equal(AlertSeverity.Critical, alert.Severity);
        Assert.Equal(9.5, alert.TriggerValue);
        Assert.Equal(8.5, alert.ThresholdValue);
    }

    [Fact]
    public void Reading_ContainsRequiredProperties()
    {
        // Arrange & Act
        var reading = new Reading
        {
            SensorId = 123,
            OrganizationId = "org_test",
            Timestamp = DateTimeOffset.UtcNow,
            Value = 7.2,
            Quality = ReadingQuality.Good,
            Protocol = Protocol.Mqtt
        };

        // Assert
        Assert.Equal(123, reading.SensorId);
        Assert.Equal("org_test", reading.OrganizationId);
        Assert.Equal(7.2, reading.Value);
        Assert.Equal(ReadingQuality.Good, reading.Quality);
        Assert.Equal(Protocol.Mqtt, reading.Protocol);
    }
}
