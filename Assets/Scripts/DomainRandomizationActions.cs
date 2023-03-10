using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class DomainRandomizationActions : DomainRandomization
    {
        #region public variables        
        [Header("Randomize Action perturbation")]
        public bool Perturb = true;
        // Define the distribution of the randomization values.
        [Tooltip("Define the distribution of the randomization values.")]
        public ProbabilityDensityFunction ProbabilityDensityFunction;
        [Tooltip("Amount of decimal places to randomize floats.")]
        public int Precision = 2;
        [Header("Perturb Actions")]
        [Tooltip(@"Percentage range of the action vector to be randomly disturbed. 
                    Example: If the starting value is 1 and the percentage range is 10, the randomization will be between 0.9 and 1.1.")]
        public int PerturbationPercentageRange;       
        [Header("Randomize Action delay")]
        public bool Delay = true;
        [Tooltip("Define the minimum amount of actions to be delayed.")]
        [Range(0, 1000)]
        public int MinActionDelay = 0;
        [Tooltip("Define the maximum amount of actions to be delayed.")]
        [Range(0, 1000)]
        public int MaxActionDelay = 0;
        //[Tooltip("Define the maximum amount of time an action can be delayed.")]
        //[Range(0, 1000f)]
        //public int MaxActionDelayInMs;
        //[Tooltip("Define the minimum amount of time an action can be delayed.")]
        //[Range(0, 1000f)]
        //public int MinActionDelayInMs;
        [Tooltip("Define the probability that an action will be delayed.")]
        [Range(0,1f)]
        public double DelayProbability;
        [NonSerialized]
        public bool IsDelayActive; // If true actions are getting delayed
        [NonSerialized]
        public int ActionDelayCount; // Amount of actions to be delayed
        #endregion
        #region private variables

        #endregion                

        /// <summary>
        /// Decide if action will be delayed based on delay probability.
        /// </summary>
        public bool DelayTrigger()
        {            
            System.Random random = new System.Random();
            return random.NextDouble() < DelayProbability;            
        }

        /// <summary>
        /// Return a random int between min and max which represents the amount of actions to be delayed.
        /// </summary>
        public void RandomActionDelayCount()
        {
            System.Random random = new System.Random();
            ActionDelayCount = random.Next(MinActionDelay, MaxActionDelay);
        }

        /// <summary>
        /// Randomize a parameter based on the defined probability density function and percentage range.
        /// </summary>        
        /// <param name="startingValue"></param>
        public float RandomizeParameter(float startingValue)
        {
            bool valuesIsNegative = startingValue < 0;
            // get absolute of value, so that the result from the probability density function is calculated correctly
            startingValue = Math.Abs(startingValue);
            // calculate the range based on the percentage
            float range = startingValue * PerturbationPercentageRange / 100;
            float max = startingValue + range;
            float min = startingValue - range;
            if (valuesIsNegative)
            {
                return -RandomizeParameter(Precision, max, min, ProbabilityDensityFunction);
            }
            else
            {
                return RandomizeParameter(Precision, max, min, ProbabilityDensityFunction);
            }
        }

        /// <summary>
        /// Delay an action based on the defined probability density function, delay probability and defined delay time range.
        /// </summary>
        //public IEnumerator DelayActionTrigger()
        //{
        //    int delayTime = (int)Math.Round(RandomizeParameter());
        //    // decide if action will be delayed based on delay probability
        //    System.Random random = new System.Random();
        //    bool delay = random.NextDouble() < DelayProbability;
        //    if (delay)
        //    {
        //        IsDelayActive = true;
        //        float delayInMs = (float)delayTime / 1000;
        //        yield return new WaitForSeconds(delayInMs);
        //        IsDelayActive = false;
        //    }
        //}

        /// <summary>
        /// Randomize a parameter based on the defined probability density function.
        /// </summary>
        //public float RandomizeParameter()
        //{
        //    float result = 0;
        //    switch (ProbabilityDensityFunction)
        //    {
        //        case ProbabilityDensityFunction.NormalDistributed:
        //            result = NormalDistribution(Precision, MaxActionDelayInMs, MinActionDelayInMs);
        //            break;
        //        case ProbabilityDensityFunction.EquallyDistributed:
        //            result = EqualDistribution(Precision, MaxActionDelayInMs, MinActionDelayInMs);
        //            break;
        //        default:
        //            break;
        //    }
        //    return result;
        //}
    }
}

