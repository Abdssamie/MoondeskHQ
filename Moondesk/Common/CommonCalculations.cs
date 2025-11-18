using UnitsNet;

namespace AquaPP.Common;

public static class CommonCalculations
{
    public static VolumeFlow CalcVolumeFlow(Volume volume, Duration duration)
    {
        return VolumeFlow.FromCubicMetersPerDay(volume.CubicMeters / duration.Days);
    }

    public static MassFlow CalcMassFlow(VolumeFlow volumeFlow, Density density)
    {
        return MassFlow.FromKilogramsPerDay(volumeFlow.CubicMetersPerDay * density.KilogramsPerCubicMeter);
    }

    public static VolumeFlow CalcVolumeFlow(MassFlow massFlow, Duration duration)
    {
        return VolumeFlow.FromCubicMetersPerDay(massFlow.KilogramsPerDay / duration.Days);
    }

    // Functions for calculating surface overflow rate
    public static Speed CalcSurfaceOverflowRate(VolumeFlow volumeFlow, Area surfaceArea)
    {
        return Speed.FromMetersPerSecond(volumeFlow.CubicMetersPerSecond / surfaceArea.SquareMeters);
    }
    public static Speed CalcSurfaceOverflowRate(VolumeFlow volumeFlow, Length surfaceDiameter)
    {
        return Speed.FromMetersPerSecond(volumeFlow.CubicMetersPerSecond
                                         / Area.FromCircleDiameter(surfaceDiameter).SquareMeters);
    }
    
    // Functions for calculating detention time
    public static Duration CalcDetentionTime(Volume tankVolume, VolumeFlow volumeFlow)
    {
        return Duration.FromHours(tankVolume.CubicMeters / volumeFlow.CubicMetersPerHour);
    }
    public static Duration CalcDetentionTime(Area surfaceArea, Length tankDepth, VolumeFlow volumeFlow)
    {
        return Duration.FromHours(tankDepth.Meters * surfaceArea.SquareMeters / volumeFlow.CentilitersPerHour);
    }

    
    // Reynolds number needed in most calculation
    public static double ReynoldsNumber(
        Length particleDiameter,
        DynamicViscosity waterDynamicViscosity,
        Speed velocity,
        Density waterDensity,
        Ratio shapeFactor)
    {
        double waterDynamicViscositySi = waterDynamicViscosity.NewtonSecondsPerMeterSquared;
        double particleDiameterSi = particleDiameter.Meters;
        double velocitySi = velocity.MetersPerSecond;
        double waterDensitySi = waterDensity.KilogramsPerCubicMeter;
        double shapeFactorSi = shapeFactor.DecimalFractions;
        
        return (
            waterDensitySi * velocitySi * shapeFactorSi * particleDiameterSi
            / waterDynamicViscositySi
        );
    }
}