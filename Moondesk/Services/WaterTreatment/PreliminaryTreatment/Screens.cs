using System;
/*using Microsoft.Extensions.Logging;*/
using Serilog;
using UnitsNet;

namespace AquaPP.Services.WaterTreatment.PreliminaryTreatment;

public class Screens
{   
    private static readonly Acceleration Gravity = Acceleration.FromMetersPerSecondSquared(9.81);
    
    public struct CoarseScreenDesignParameters
    {
        private string Location { get; set; }
        private double ApproachVelocity { get; set; }
        private double ClearOpenings { get; set; }
        private double HeadLoss { get; set; }
        private string DisposalOfScreenings { get; set; }

        // Optional: Constructor (Structs can have parameterized constructors in C#)
        public  CoarseScreenDesignParameters(string location, 
            double approachVelocity, double clearOpenings, 
            double headLoss, string disposalOfScreenings)
        {
            Location = location;
            ApproachVelocity = approachVelocity;
            ClearOpenings = clearOpenings;
            HeadLoss = headLoss;
            DisposalOfScreenings = disposalOfScreenings;
        }

        // Optional: Methods
        public override string ToString()
        {
            return $"Location: {Location}, Approach Velocity: {ApproachVelocity} m/s, " +
                   $"Clear Openings: {ClearOpenings} m, Head Loss: {HeadLoss} m, Disposal: {DisposalOfScreenings}";
        }
    }
    
    public class TrashRacks
    {
        /// <summary>
        /// Calculates head loss in a trash rack.
        /// </summary>
        /// <param name="flowVelocity">Velocity of the flow in the channel.</param>
        /// <param name="approachVelocity">Velocity of the flow approaching the screen.</param>
        /// <param name="dischargeCoefficient">Discharge coefficient of the screen. Defaults to 0.8 for clean screens and 0.6 for dirty screens.</param>
        /// <param name="gravity">Acceleration due to gravity. Defaults to 9.81 m/s^2.</param>
        /// <param name="screenIsClean">Whether the screen is clean or not. Defaults to <c>true</c>.</param>
        /// <returns>A <see cref="Length"/> of the head loss.</returns>
        /// <remarks>
        /// The formula used is derived from the Bernoulli equation and assumes a horizontal channel.
        /// </remarks>
        public static Length CalcHeadLoss(
            Speed flowVelocity,
            Speed approachVelocity,
            double dischargeCoefficient = 0.8,
            Acceleration gravity = default,
            bool screenIsClean = true)
        {
            if (Equals(gravity, default)) gravity = Gravity;

            if (!screenIsClean) dischargeCoefficient = 0.6;
            try
            {
                return Length.FromMeters((Math.Pow(flowVelocity.MetersPerSecond, 2) - Math.Pow(approachVelocity.MetersPerSecond, 2)) 
                                         / (2 * gravity.MetersPerSecondSquared * dischargeCoefficient));
            } catch (UnitsNetException e)
            {
                Log.Error("Failed calculating head loss with the provided parameters: {e}", e);
                return Length.Zero;
            }
        }

        /// <summary>
        /// Calculates the approximate number of bars in a trash rack
        /// screen according to Davis, 2011.
        /// </summary>
        /// <param name="channelWidth">Width of the channel.</param>
        /// <param name="barSpacing">Spacing between bars.</param>
        /// <param name="barWidth">Width of a bar.</param>
        /// <param name="barSpace">Spacing between a bar and the channel wall.</param>
        /// <returns>An <see cref="int"/> of the approximate number of bars.</returns>
        /// <remarks>
        /// The formula used is derived from Davis, 2011.
        /// </remarks>
        public static int NumberOfBars(
            Length channelWidth,
            Length barSpacing,
            Length barWidth,
            Length barSpace)
        {
            try
            {   
                // Returns the approximate number of bars in the channel according
                // to Davis, 2011
                return (int)((channelWidth.Meters - barSpacing.Meters) / (barSpace.Meters + barWidth.Meters));
            }
            catch (UnitsNetException e)
            {   
                Log.Error("Failed calculating number of bars with the provided parameters: {e}", e);
                return 0;
            }
        }
        
        public static int NumberOfBarSpaces(
            Length channelWidth,
            Length barSpacing,
            Length barWidth,
            Length barSpace)
        {
            try
            {   
                return NumberOfBars(channelWidth, barSpacing, barWidth, barSpace) + 1;
            }
            catch (UnitsNetException e)
            {   
                Log.Error("Failed calculating number of bars with the provided parameters: {e}", e);
                return 0;
            }
        }

        public static Area AreaOfScreenOpenings(
            int numberOfBarSpaces,
            Length barSpacing,
            Length waterDepth)
        {
            try
            {
                return Area.FromSquareMeters(numberOfBarSpaces * barSpacing.Meters * waterDepth.Meters);
            }
            catch (UnitsNetException e)
            {
                Log.Error("Failed calculating area of screen openings with the provided parameters: {e}", e);
                return Area.Zero;
            }
        }
        
        public static Speed FlowRateThroughBarScreenOpenings(
            VolumeFlow flowRate,
            Area areaOfScreenOpenings)
        {
            try
            {
                return Speed.FromMetersPerSecond(flowRate.CubicMetersPerSecond / areaOfScreenOpenings.SquareMeters);
            }
            catch (UnitsNetException e)
            {
                Log.Error("Failed calculating flow rate through bar screen openings with the provided parameters: {e}",e);
                return Speed.Zero;
            }
        }
    }
}