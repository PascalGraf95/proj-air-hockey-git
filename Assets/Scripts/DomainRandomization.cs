using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts
{
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

    public class DomainRandomization : MonoBehaviour
    {
        // Allows the user to define a list of objects with similar randomization parameters to be applied.
        public List<GameObject> mjGameObjectsToRandomize;
        // Define how often the randomization will be applied.
        public ApplyRandomizationAfter applyRandomization;
        // Define the distribution of the randomization values.
        public ProbabilityDensityFunction probabilityDensityFunction;

        // Define parameter range for MuJoCo objects
        [Space(5)]
        [Header("MuJoCo Object Parameters")]
        [Tooltip("Amount of decimal places to randomize floats.")]
        public int Precision = 2;
        [Range(0f, 100f)]
        public float MaxMass;
        [Range(0f, 100f)]
        public float MinMass;
        [Range(0f, 100f)]
        public float MaxFrictionGeom;
        [Range(0f, 100f)]
        public float MinFrictionGeom;

        
        public void RandomizeEnvironmentParameter()
        {
            switch (probabilityDensityFunction)
            {
                case ProbabilityDensityFunction.NormalDistributed:
                    break;
                case ProbabilityDensityFunction.EquallyDistributed:
                    break;
                default:
                    break;
            }
        }

        private float EqualDistribution(int precision, float max, float min)
        {
            float result = RandomFromDistribution.RandomRangeLinear(max, min, 0);
            return (float)Math.Round(result, precision);
        }

        private float NormalDistribution(int precision, float max, float min) 
        {
            float result = RandomFromDistribution.RandomRangeNormalDistribution(max, min, RandomFromDistribution.ConfidenceLevel_e._999);
            return (float)Math.Round(result, precision);
        }
    }
}
