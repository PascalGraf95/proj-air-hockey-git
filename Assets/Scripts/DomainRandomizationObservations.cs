using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{    
    public class DomainRandomizationObservations : DomainRandomization
    {
        #region public variables
        [Header("Randomize Observations")]
        public bool Randomize = true;
        // Define the distribution of the randomization values.
        [Tooltip("Define the distribution of the randomization values.")]
        public ProbabilityDensityFunction ProbabilityDensityFunction;
        [Tooltip("Amount of decimal places to randomize floats.")]
        public int Precision = 2;
        [Tooltip(@"Percentage of the starting value defined in the MuJoCo scripts to randomize the parameter.
                   Example: If the starting value is 1 and the percentage range is 10, the randomization will be between 0.9 and 1.1.")]
        public int PercentageRange;
        #endregion
        #region private variables

        #endregion

        /// <summary>
        /// Randomize a parameter based on the defined probability density function and percentage range.
        /// </summary>
        /// <param name="value"></param>
        public float RandomizeParameter(float value)
        {
            bool valueIsNegative = value < 0;
            if (Randomize)
            {
                // get absolute of value, so that the result from the probability density function is calculated correctly
                value = Math.Abs(value);
                // calculate the range based on the percentage
                float range = value * PercentageRange / 100;
                float max = value + range;
                float min = value - range;
                float result = 0;
                switch (ProbabilityDensityFunction)
                {
                    case ProbabilityDensityFunction.NormalDistributed:
                        result = NormalDistribution(Precision, max, min);
                        break;
                    case ProbabilityDensityFunction.EquallyDistributed:
                        result = EqualDistribution(Precision, max, min);
                        break;
                    default:
                        break;
                }
                // if the value was negative, return the negative of the result
                if (valueIsNegative)
                {
                    return -result;
                }
                else
                {
                    return result;
                }                
            }
            else
            {
                return value;
            }
        }
    }
}
