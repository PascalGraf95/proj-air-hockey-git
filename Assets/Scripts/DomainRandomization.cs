using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    // TODO: Move to Controller
    public enum ApplyRandomizationAfter
    {
        Episode,
        Steps
    }
    public enum ProbabilityDensityFunction
    {
        EquallyDistributed,
        NormalDistributed
    }
    public enum RangeSelection
    {
        Numerical,
        Percentage
    }
    public class DomainRandomization : MonoBehaviour
    {
        /// <summary>
        /// Randomize a parameter based on the defined probability density function and numerical value range.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        public float RandomizeParameter(int precision, float max, float min, ProbabilityDensityFunction densityFunction)
        {
            float result = 0;
            switch (densityFunction)
            {
                case ProbabilityDensityFunction.NormalDistributed:
                    result = NormalDistribution(precision, max, min);
                    break;
                case ProbabilityDensityFunction.EquallyDistributed:
                    result = EqualDistribution(precision, max, min);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Randomize a parameter based on a normal distribution.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        public float EqualDistribution(int precision, float max, float min)
        {
            float result = RandomFromDistribution.RandomRangeLinear(min, max, 0);
            return (float)Math.Round(result, precision);
        }

        /// <summary>  
        /// Randomize a parameter based on a normal distribution.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        public float NormalDistribution(int precision, float max, float min)
        {
            float result = RandomFromDistribution.RandomRangeNormalDistribution(min, max, RandomFromDistribution.ConfidenceLevel_e._999);
            return (float)Math.Round(result, precision);
        }
    }
}
