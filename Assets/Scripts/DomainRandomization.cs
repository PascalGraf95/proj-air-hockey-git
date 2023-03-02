using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using Mujoco;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;

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

    public class MjGeomClone
    {
        public int MujocoId;
        public float Mass;
        public float Density;
        public MjGeomSettings Settings;
    }

    public class DomainRandomization : MonoBehaviour
    {
        #region public variables
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
        [Range(-100f, 100f)]
        [Space(3)]
        [Header("MuJoCo Geom Parameters")]
        public float MaxMass;
        [Range(-100f, 100f)]
        public float MinMass;
        [Range(-100f, 100f)]
        public float MaxFrictionGeom;
        [Range(-100f, 100f)]
        public float MinFrictionGeom;
        [Range(-100f, 100f)]
        public float MaxDensity;
        [Range(-100f, 100f)]
        public float MinDensity;
        [Range(-100f, 100f)]
        public float SolverSolMixMin;
        [Range(-100f, 100f)]
        public float SolverSolMixMax;
        [Range(-100f, 100f)]
        public float SolRefTimeConstMin;
        [Range(-100f, 100f)]
        public float SolRefTimeConstMax;
        [Range(-100f, 100f)]
        public float SolRefDampingRatioMin;
        [Range(-100f, 100f)]
        public float SolRefDampingRatioMax;
        [Range(-100f, 100f)]
        public float SolRefMixMin;
        [Range(-100f, 100f)]
        public float SolRefMixMax;
        [Range(-100f, 100f)]
        public float SolImpDMinMin;
        [Range(-100f, 100f)]
        public float SolImpDMinMax;
        [Range(-100f, 100f)]
        public float SolImpDMaxMin;
        [Range(-100f, 100f)]
        public float SolImpDMaxMax;
        [Range(-100f, 100f)]
        public float SolImpWidthMin;
        [Range(-100f, 100f)]
        public float SolImpWidthMax;
        [Range(-100f, 100f)]
        public float SolImpMidpointMin;
        [Range(-100f, 100f)]
        public float SolImpMidpointMax;
        [Range(-100f, 100f)]
        public float SolImpPowerMin;
        [Range(-100f, 100f)]
        public float SolImpPowerMax;
        [Range(-100f, 100f)]
        public float GeomSolverMarginMin;
        [Range(-100f, 100f)]
        public float GeomSolverMarginMax;
        [Range(-100f, 100f)]
        public float GeomSolverGapMin;
        [Range(-100f, 100f)]
        public float GeomSolverGapMax;
        [Range(-100f, 100f)]

        [Space(3)]
        [Header("MuJoCo Slide Joint Parameters")]
        public float ArmatureMin;
        [Range(-100f, 100f)]
        public float ArmatureMax;
        [Range(-100f, 100f)]
        public float SpringTimeConstMin;
        [Range(-100f, 100f)]
        public float SpringTimeConstMax;
        [Range(-100f, 100f)]
        public float SpringDampingRatioMin;
        [Range(-100f, 100f)]
        public float SpringDampingRatioMax;
        [Range(-100f, 100f)]
        public float SpringStiffnessMin;
        [Range(-100f, 100f)]
        public float SpringStiffnessMax;
        [Range(-100f, 100f)]
        public float SpringDampingMin;
        [Range(-100f, 100f)]
        public float SpringDampingMax;
        [Range(-100f, 100f)]
        public float SpringEquilibriumPoseMin;
        [Range(-100f, 100f)]
        public float SpringEquilibriumPoseMax;
        [Range(-100f, 100f)]
        public float JointSolverMarginMin;
        [Range(-100f, 100f)]
        public float JointSolverMarginMax;
        [Range(-100f, 100f)]
        public float SolverRefLimitTimeConstMin;
        [Range(-100f, 100f)]
        public float SolverRefLimitTimeConstMax;
        [Range(-100f, 100f)]
        public float SolverRefLimitDampRationMin;
        [Range(-100f, 100f)]
        public float SolverRefLimitDampRationMax;
        [Range(-100f, 100f)]
        public float SolverImpLimitFrictionLossMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitFrictionLossMax;
        [Range(-100f, 100f)]
        public float SolverImpFrictionDMinMin;
        [Range(-100f, 100f)]
        public float SolverImpFrictionDMinMax;
        [Range(-100f, 100f)]
        public float SolverImpFrictionDMaxMin;
        [Range(-100f, 100f)]
        public float SolverImpFrictionDMaxMax;
        [Range(-100f, 100f)]
        public float SolverImpFrictionWidthMin;
        [Range(-100f, 100f)]
        public float SolverImpFrictionWidthMax;
        [Range(-100f, 100f)]
        public float SolverImpFrictionMidpointMin;
        [Range(-100f, 100f)]
        public float SolverImpFrictionMidpointMax;
        [Range(-100f, 100f)]
        public float SolverImpFrictionPowerMin;
        [Range(-100f, 100f)]
        public float SolverImpFrictionPowerMax;
        [Range(-100f, 100f)]
        public float SolverRefFrictionTimeConstMin;
        [Range(-100f, 100f)]
        public float SolverRefFrictionTimeConstMax;
        [Range(-100f, 100f)]
        public float SolverRefFrictionDampRatioMin;
        [Range(-100f, 100f)]
        public float SolverRefFrictionDampRatioMax;
        [Range(-100f, 100f)]

        #endregion
        #region private variables
        private List<GameObject> mjScripts;
        private List<MjGeomClone> mjGeomClones;
        #endregion
        private void Start()
        {
            mjGeomClones = new List<MjGeomClone>();
        }

        private MjGeomClone SearchGameObjectTree(MjGeom mjGeom, int mjId)
        {
            // if object not already in list add the mjGeom to the list
            if (!mjGeomClones.Any(x => x.MujocoId == mjGeom.MujocoId))
            {
                mjGeomClones.Add(new MjGeomClone() { MujocoId = mjGeom.MujocoId, Mass = mjGeom.Mass, Density = mjGeom.Density, Settings = mjGeom.Settings });
            }
            // search for the mjGeom in the list
            MjGeomClone clone = mjGeomClones.Find(x => x.MujocoId == mjId);

            return clone;
        }

        /// <summary>
        /// Recursive function to get all children of a gameobject.
        /// </summary>
        /// <param name="gameObjects"></param>
        public void RandomizeGameObjectTree(List<GameObject> gameObjects)
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
                    RandomizeMjGeom(gameObject);
                }
                else if (gameObject.GetComponent<MjSlideJoint>() != null)
                {

                }
                else if (gameObject.GetComponent<MjActuator>() != null)
                {

                }
            }
        }

        private void RandomizeMjGeom(GameObject gameObject)
        {
            MjGeom mjGeom = gameObject.GetComponent<MjGeom>();
            var parent = gameObject.transform.parent;
            string parentName = parent.gameObject.name;

            if (mjGeom != null)
            {
                switch (RangeSelection)
                {
                    case RangeSelection.Numerical:
                        // set geom parameters to random values
                        mjGeom.Mass = RandomizeParameter(Precision, MaxMass, MinMass);
                        mjGeom.Density = RandomizeParameter(Precision, MaxDensity, MinDensity);
                        mjGeom.Settings.Solver.SolMix = RandomizeParameter(Precision, SolverSolMixMax, SolverSolMixMin);
                        mjGeom.Settings.Solver.SolRef.TimeConst = RandomizeParameter(Precision, SolRefTimeConstMax, SolRefTimeConstMin);
                        mjGeom.Settings.Solver.SolRef.DampRatio = RandomizeParameter(Precision, SolRefDampingRatioMax, SolRefDampingRatioMin);
                        mjGeom.Settings.Solver.SolImp.DMin = RandomizeParameter(Precision, SolImpDMinMax, SolImpDMinMin);
                        mjGeom.Settings.Solver.SolImp.DMax = RandomizeParameter(Precision, SolImpDMaxMax, SolImpDMaxMin);
                        mjGeom.Settings.Solver.SolImp.Width = RandomizeParameter(Precision, SolImpWidthMax, SolImpWidthMin);
                        mjGeom.Settings.Solver.SolImp.Midpoint = RandomizeParameter(Precision, SolImpMidpointMax, SolImpMidpointMin);
                        mjGeom.Settings.Solver.SolImp.Power = RandomizeParameter(Precision, SolImpPowerMax, SolImpPowerMin);
                        mjGeom.Settings.Solver.Margin = RandomizeParameter(Precision, GeomSolverMarginMax, GeomSolverMarginMin);
                        mjGeom.Settings.Solver.Gap = RandomizeParameter(Precision, GeomSolverGapMax, GeomSolverGapMin);
                        mjGeom.Settings.Friction.Rolling = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom);
                        mjGeom.Settings.Friction.Torsional = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom);
                        mjGeom.Settings.Friction.Sliding = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom);
                        // set joint parameters to random values

                        break;
                    case RangeSelection.Percentage:
                        var tempGeom = SearchGameObjectTree(mjGeom, mjGeom.MujocoId);
                        // Randomize all float parameters of the MuJoCo object
                        foreach (FieldInfo field in tempGeom.GetType().GetFields())
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float value = (float)field.GetValue(tempGeom);
                                float result = RandomizeParameter(Precision, value, PercentageRange);
                                // set the new value to the field in the original object
                                SetFieldValue(mjGeom, field.Name, result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static void SetFieldValue(object obj, string fieldName, float value)
        {
            var type = obj.GetType();
            FieldInfo field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(obj, value);
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


