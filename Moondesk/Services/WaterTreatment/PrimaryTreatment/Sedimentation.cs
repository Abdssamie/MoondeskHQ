using System.Security.Cryptography;

namespace AquaPP.Services.WaterTreatment.PrimaryTreatment;

using System;
using UnitsNet;
using AquaPP.Common;

public abstract class PrimarySedimentation  // Class is abstract temporarily
{
    // CultureInfo _usEnglish = new CultureInfo("en-US");=
    
    public static Speed CalculateTerminalSettlingVelocity(
        Length particleDiameter, 
        Density particleDensity,
        Density waterDensity,
        Acceleration gravity,
        DynamicViscosity waterDynamicViscosity,
        Temperature temperature,
        Ratio? shapeFactor,
        double tolerance = 1e-4,
        int maxIterations = 300
        )
    {
        shapeFactor ??= Ratio.FromDecimalFractions(1);
        
        double particleDiameterSi = particleDiameter.Meters;
        double particleDensitySi = particleDensity.KilogramsPerCubicMeter;
        double waterDensitySi = waterDensity.KilogramsPerCubicMeter;
        double gravitySi = gravity.MetersPerSecondSquared;
        double waterDynamicViscositySi = waterDynamicViscosity.NewtonSecondsPerMeterSquared;
        
        
        // Initial guess
        var velocityStokes = (
            (gravitySi * (particleDensitySi - waterDensitySi) * Math.Pow(particleDiameterSi,2))
            / (18 * waterDynamicViscositySi)
        );

        var velocityT = Speed.FromMetersPerSecond(velocityStokes);

        for (int i = 0; i < maxIterations; i++)
        {
            var reynoldsNumber = CommonCalculations.ReynoldsNumber(
                particleDiameter,
                waterDynamicViscosity,
                velocityT,
                waterDensity,
                shapeFactor.Value);

            double velocityTNewSi;
            
            if (reynoldsNumber < 1)
            {
                velocityTNewSi = velocityStokes;
                return Speed.FromMetersPerSecond(velocityTNewSi);
            }
            
            double dragCoefficient;
            if (reynoldsNumber >= 1 && reynoldsNumber < 1000)
            { 
                dragCoefficient = 24/reynoldsNumber + 3/Math.Sqrt(reynoldsNumber) + 0.34;  
            }
            else
            {
                dragCoefficient = 0.4;
            }

            velocityTNewSi = Math.Sqrt(
                (4*gravitySi*particleDiameterSi*(particleDensitySi-waterDensitySi))
                / (3*dragCoefficient*waterDensitySi)
                );
            
            var relativeDifference = Math.Abs(
                (velocityTNewSi - velocityT.MetersPerSecond) 
                / velocityT.MetersPerSecond
                );

            if (relativeDifference < tolerance)
            {
                return velocityT;
            }

            velocityT = Speed.FromMetersPerSecond(velocityTNewSi);
        }
        
        throw new Exception($"Settling Velocity Calculation did not converge after {maxIterations} iterations");
    }


    public MassFlow MassFlowOfSolidsRemoved(VolumeFlow volumeFlow, MassConcentration massConcentration,
        Ratio massRemovedFraction)
    {
        return MassFlow.FromKilogramsPerDay(volumeFlow.CubicMetersPerDay*
                                  massConcentration.KilogramsPerCubicMeter*
                                  massRemovedFraction.DecimalFractions);
    }
}
