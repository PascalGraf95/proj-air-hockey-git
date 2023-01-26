using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Mujoco;

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

    public enum RangeSelection
    {
        Numerical,
        Percentage
    }

    public class DomainRandomization : MonoBehaviour
    {
        // Allows the user to define a list of objects with similar randomization parameters to be applied.
        public List<GameObject> GameObjectsToRandomize;
        // Define how often the randomization will be applied.
        public ApplyRandomizationAfter applyRandomization;
        // Define the distribution of the randomization values.
        public ProbabilityDensityFunction probabilityDensityFunction;

        // Define parameter range for MuJoCo objects
        [Space(5)]
        [Header("MuJoCo Object Parameters")]
        [Tooltip("Amount of decimal places to randomize floats.")]
        public int Precision = 2;
        [Tooltip(@"Select if the range is defined in percentage of the starting value defined in the MuJoCo scripts
                   or by numerical min-max-values.")]
        public RangeSelection RangeSelection = RangeSelection.Percentage;
        [Tooltip(@"Percentage of the starting value defined in the MuJoCo scripts to randomize the parameter.
                   Example: If the starting value is 1 and the percentage range is 10, the randomization will be between 0.9 and 1.1.")]
        public int PercentageRange;
        [Range(0f, 100f)]
        public float MaxMass;
        [Range(0f, 100f)]
        public float MinMass;
        [Range(0f, 100f)]
        public float MaxFrictionGeom;
        [Range(0f, 100f)]
        public float MinFrictionGeom;

        
        // private List<GameObject> gameObjectGroups;
        // private List<GameObject> mjBodies;
        private List<GameObject> mjScripts;

        private void Start()
        {
            RandomizeGameObjectTree(GameObjectsToRandomize);
        }

        /// <summary>
        /// Recursive function to get all children of a gameobject.
        /// </summary>
        /// <param name="gameObjects"></param>
        private void RandomizeGameObjectTree(List<GameObject> gameObjects)
        {
            // iterate through all gameobjects
            foreach (GameObject gameObject in gameObjects)
            {
                // check if the gameobject has children gameobjects and a mjBody component
                if (gameObject.transform.childCount > 0 && gameObject.GetComponent<MjBody>() != null)
                {
                    mjScripts = new List<GameObject>();
                    // add all children gameobjects to the list
                    for (int i = 0; i < gameObject.transform.childCount; i++)
                    {
                        Transform child = gameObject.transform.GetChild(i);                        
                        mjScripts.Add(child.gameObject);
                    }
                    // call the function again with the list of children gameobjects
                    RandomizeGameObjectTree(mjScripts);
                }
                else if (gameObject.GetComponent<MjGeom>() != null)
                {
                    RandomizeMjGeom(gameObject.GetComponent<MjGeom>());
                }
                else if (gameObject.GetComponent<MjSlideJoint>() != null)
                {

                }
                else if (gameObject.GetComponent<MjActuator>() != null)
                {

                }                             
            }
        }

        private void RandomizeMjGeom(MjGeom mjGeom)
        {            
            if (mjGeom != null)
            {
                switch (RangeSelection)
                {
                    case RangeSelection.Numerical:
                        mjGeom.Mass = RandomizeParameter(Precision, MaxMass, MinMass);
                        mjGeom.Settings.Friction.Sliding = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom);
                        break;
                    case RangeSelection.Percentage:
                        mjGeom.Mass = RandomizeParameter(Precision, mjGeom.Mass, PercentageRange);
                        mjGeom.Settings.Friction.Sliding = RandomizeParameter(Precision, mjGeom.Settings.Friction.Sliding, PercentageRange);
                        break;
                    default:
                        break;
                }
            }            
        }

        /// <summary>
        /// Randomize a parameter based on the defined probability density function and numerical value range.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="max"></param>
        /// <param name="min"></param>
        public float RandomizeParameter(int precision, float max, float min)
        {
            float result = 0;
            switch (probabilityDensityFunction)
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
        /// Randomize a parameter based on the defined probability density function and percentage range.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="startingValue"></param>
        /// <param name="percentage"></param>
        public float RandomizeParameter(int precision, float startingValue, int percentage)
        {
            // calculate the range based on the percentage
            float range = startingValue * percentage / 100;
            float max = startingValue + range;
            float min = startingValue - range;
            float result = 0;
            switch (probabilityDensityFunction)
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
