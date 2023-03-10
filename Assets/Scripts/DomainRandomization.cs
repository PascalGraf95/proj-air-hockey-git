using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class DomainRandomization : MonoBehaviour
    {
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
