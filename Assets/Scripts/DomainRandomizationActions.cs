using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class DomainRandomizationActions : MonoBehaviour
    {
        #region public variables
        [Header("Randomize Actions")]
        public bool Randomize = true;
        // Define the distribution of the randomization values.
        [Tooltip("Define the distribution of the randomization values.")]
        public ProbabilityDensityFunction ProbabilityDensityFunction;
        [Tooltip("Amount of decimal places to randomize floats.")]
        public int Precision = 2;
        [Header("Perturb Actions")]
        [Tooltip(@"Percentage range of the action vector to be randomly disturbed. 
                    Example: If the starting value is 1 and the percentage range is 10, the randomization will be between 0.9 and 1.1.")]
        public int PerturbationPercentageRange;
        [Header("Delay Actions")]
        [Tooltip("Define the maximum amount of time an action can be delayed.")]
        [Range(0, 1000f)]
        public int MaxActionDelayInMs;
        [Tooltip("Define the minimum amount of time an action can be delayed.")]
        [Range(0, 1000f)]
        public int MinActionDelayInMs;
        [Tooltip("Define the probability that an action will be delayed.")]
        [Range(0,1f)]
        public double DelayProbability;
        #endregion
        #region private variables

        #endregion
        private void Start()
        {

        }

        /// <summary>
        /// Delay an action based on the defined probability density function, delay probability and defined delay time range.
        /// </summary>
        public void DelayAction() 
        { 
            if (Randomize) 
            {
                // decide if action will be delayed based on delay probability
                System.Random random = new System.Random();
                bool delay = random.NextDouble() < DelayProbability;
                if (delay) 
                {
                    Thread.Sleep((int)Math.Round(RandomizeParameter()));
                }
            }
        }

        /// <summary>
        /// Randomize a parameter based on the defined probability density function.
        /// </summary>
        public float RandomizeParameter()
        {
            float result = 0;
            switch (ProbabilityDensityFunction)
            {
                case ProbabilityDensityFunction.NormalDistributed:
                    result = NormalDistribution(Precision, MaxActionDelayInMs, MinActionDelayInMs);
                    break;
                case ProbabilityDensityFunction.EquallyDistributed:
                    result = EqualDistribution(Precision, MaxActionDelayInMs, MinActionDelayInMs);
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Randomize a parameter based on the defined probability density function and percentage range.
        /// </summary>        
        /// <param name="startingValue"></param>
        public float RandomizeParameter(float startingValue)
        {
            float result = startingValue;
            if (Randomize)
            {
                // calculate the range based on the percentage
                float range = startingValue * PerturbationPercentageRange / 100;
                float max = startingValue + range;
                float min = startingValue - range;
                
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
            }
            return result;
        }

        /// <summary>
        /// Randomize a parameter based on a normal distribution.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        private float EqualDistribution(int precision, float max, float min)
        {
            float result = RandomFromDistribution.RandomRangeLinear(max, min, 0);
            return (float)Math.Round(result, precision);
        }

        /// <summary>  
        /// Randomize a parameter based on a normal distribution.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        private float NormalDistribution(int precision, float max, float min)
        {
            float result = RandomFromDistribution.RandomRangeNormalDistribution(min, max, RandomFromDistribution.ConfidenceLevel_e._999);
            return (float)Math.Round(result, precision);
        }
    }
}

