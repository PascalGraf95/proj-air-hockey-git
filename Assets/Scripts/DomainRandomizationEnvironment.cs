﻿using System;
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
    public class MjGeomClone
    {
        public int MujocoId;
        public float Mass;
        public float Density;
        public MjGeomSettings Settings;
    }

    public class MjSlideJointClone
    {
        public int MujocoId;
        public MjJointSettings Settings;
    }

    public class DomainRandomizationEnvironment : DomainRandomization
    {
        #region public variables
        // Allows the user to define a list of objects with similar randomization parameters to be applied.
        public List<GameObject> GameObjectsToRandomize;
        // Define how often the randomization will be applied.
        public ApplyRandomizationAfter applyRandomization;
        // Define the distribution of the randomization values.
        [Tooltip("Define the distribution of the randomization values.")]
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
        public float SolverImpLimitDMinMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitDMinMax;
        [Range(-100f, 100f)]
        public float SolverImpLimitDMaxMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitDMaxMax;
        [Range(-100f, 100f)]
        public float SolverImpLimitWidthMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitWidthMax;
        [Range(-100f, 100f)]
        public float SolverImpLimitMidpointMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitMidpointMax;
        [Range(-100f, 100f)]
        public float SolverImpLimitPowerMin;
        [Range(-100f, 100f)]
        public float SolverImpLimitPowerMax;
        [Range(-100f, 100f)]
        public float SolverFrictionLossMin;
        [Range(-100f, 100f)]
        public float SolverFrictionLossMax;
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
        private List<MjSlideJointClone> mjSlideJointClones;
        #endregion
        private void Start()
        {
            mjGeomClones = new List<MjGeomClone>();
            mjSlideJointClones = new List<MjSlideJointClone>();
        }

        private MjGeomClone GeomSearchGameObjectTree(MjGeom mjGeom, int mjId)
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

        private MjSlideJointClone SlideJointSearchGameObjectTree(MjSlideJoint mjSlideJoint, int mjId)
        {
            // if object not already in list add the mjGeom to the list
            if (!mjSlideJointClones.Any(x => x.MujocoId == mjSlideJoint.MujocoId))
            {
                mjSlideJointClones.Add(new MjSlideJointClone() { MujocoId = mjSlideJoint.MujocoId, Settings = mjSlideJoint.Settings });
            }
            // search for the mjGeom in the list
            MjSlideJointClone clone = mjSlideJointClones.Find(x => x.MujocoId == mjId);

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
                    RandomizeMjSlideJoint(gameObject);
                }
            }
        }

        private void RandomizeMjSlideJoint(GameObject gameObject)
        {
            MjSlideJoint mjSlideJoint = gameObject.GetComponent<MjSlideJoint>();

            if (mjSlideJoint != null)
            {
                switch (RangeSelection)
                {
                    case RangeSelection.Numerical:
                        // set joint parameters to random values
                        mjSlideJoint.Settings.Armature = RandomizeParameter(Precision, ArmatureMax, ArmatureMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Spring.TimeConstant = RandomizeParameter(Precision, SpringTimeConstMax, SpringTimeConstMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Spring.DampingRatio = RandomizeParameter(Precision, SpringDampingRatioMax, SpringDampingRatioMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Spring.Stiffness = RandomizeParameter(Precision, SpringStiffnessMax, SpringStiffnessMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Spring.Damping = RandomizeParameter(Precision, SpringDampingMax, SpringDampingMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Spring.EquilibriumPose = RandomizeParameter(Precision, SpringEquilibriumPoseMax, SpringEquilibriumPoseMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.Margin = RandomizeParameter(Precision, JointSolverMarginMax, JointSolverMarginMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.RefLimit.TimeConst = RandomizeParameter(Precision, SolverRefLimitTimeConstMax, SolverRefLimitTimeConstMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.RefLimit.DampRatio = RandomizeParameter(Precision, SolverRefLimitDampRationMax, SolverRefLimitDampRationMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpLimit.DMin = RandomizeParameter(Precision, SolverImpLimitDMinMax, SolverImpLimitDMinMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpLimit.DMax = RandomizeParameter(Precision, SolverImpLimitDMaxMax, SolverImpLimitDMaxMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpLimit.Width = RandomizeParameter(Precision, SolverImpLimitWidthMax, SolverImpLimitWidthMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpLimit.Midpoint = RandomizeParameter(Precision, SolverImpLimitMidpointMax, SolverImpLimitMidpointMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpLimit.Power = RandomizeParameter(Precision, SolverImpLimitPowerMax, SolverImpLimitPowerMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.FrictionLoss = RandomizeParameter(Precision, SolverFrictionLossMax, SolverFrictionLossMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpFriction.DMin = RandomizeParameter(Precision, SolverImpFrictionDMinMax, SolverImpFrictionDMinMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpFriction.DMax = RandomizeParameter(Precision, SolverImpFrictionDMaxMax, SolverImpFrictionDMaxMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpFriction.Width = RandomizeParameter(Precision, SolverImpFrictionWidthMax, SolverImpFrictionWidthMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpFriction.Midpoint = RandomizeParameter(Precision, SolverImpFrictionMidpointMax, SolverImpFrictionMidpointMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.ImpFriction.Power = RandomizeParameter(Precision, SolverImpFrictionPowerMax, SolverImpFrictionPowerMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.RefFriction.TimeConst = RandomizeParameter(Precision, SolverRefFrictionTimeConstMax, SolverRefFrictionTimeConstMin, probabilityDensityFunction);
                        mjSlideJoint.Settings.Solver.RefFriction.DampRatio = RandomizeParameter(Precision, SolverRefFrictionDampRatioMax, SolverRefFrictionDampRatioMin, probabilityDensityFunction);
                        break;
                    case RangeSelection.Percentage:
                        var tempSliderJoint = SlideJointSearchGameObjectTree(mjSlideJoint, mjSlideJoint.MujocoId);
                        // Randomize all float parameters of the MuJoCo object
                        foreach (FieldInfo field in tempSliderJoint.GetType().GetFields())
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float value = (float)field.GetValue(tempSliderJoint);
                                float result = RandomizeParameterPercentageBased(Precision, value, PercentageRange);
                                // set the new value to the field in the original object
                                SetFieldValue(tempSliderJoint, field.Name, result);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void RandomizeMjGeom(GameObject gameObject)
        {
            MjGeom mjGeom = gameObject.GetComponent<MjGeom>();

            if (mjGeom != null)
            {
                switch (RangeSelection)
                {
                    case RangeSelection.Numerical:
                        // set geom parameters to random values
                        mjGeom.Mass = RandomizeParameter(Precision, MaxMass, MinMass, probabilityDensityFunction);
                        mjGeom.Density = RandomizeParameter(Precision, MaxDensity, MinDensity, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolMix = RandomizeParameter(Precision, SolverSolMixMax, SolverSolMixMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolRef.TimeConst = RandomizeParameter(Precision, SolRefTimeConstMax, SolRefTimeConstMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolRef.DampRatio = RandomizeParameter(Precision, SolRefDampingRatioMax, SolRefDampingRatioMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolImp.DMin = RandomizeParameter(Precision, SolImpDMinMax, SolImpDMinMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolImp.DMax = RandomizeParameter(Precision, SolImpDMaxMax, SolImpDMaxMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolImp.Width = RandomizeParameter(Precision, SolImpWidthMax, SolImpWidthMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolImp.Midpoint = RandomizeParameter(Precision, SolImpMidpointMax, SolImpMidpointMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.SolImp.Power = RandomizeParameter(Precision, SolImpPowerMax, SolImpPowerMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.Margin = RandomizeParameter(Precision, GeomSolverMarginMax, GeomSolverMarginMin, probabilityDensityFunction);
                        mjGeom.Settings.Solver.Gap = RandomizeParameter(Precision, GeomSolverGapMax, GeomSolverGapMin, probabilityDensityFunction);
                        mjGeom.Settings.Friction.Rolling = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom, probabilityDensityFunction);
                        mjGeom.Settings.Friction.Torsional = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom, probabilityDensityFunction);
                        mjGeom.Settings.Friction.Sliding = RandomizeParameter(Precision, MaxFrictionGeom, MinFrictionGeom, probabilityDensityFunction);
                        break;
                    case RangeSelection.Percentage:
                        var tempGeom = GeomSearchGameObjectTree(mjGeom, mjGeom.MujocoId);
                        // Randomize all float parameters of the MuJoCo object
                        foreach (FieldInfo field in tempGeom.GetType().GetFields())
                        {
                            if (field.FieldType == typeof(float))
                            {
                                float value = (float)field.GetValue(tempGeom);
                                float result = RandomizeParameterPercentageBased(Precision, value, PercentageRange);
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
        /// Randomize a parameter based on the defined probability density function and percentage range.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="startingValue"></param>
        /// <param name="percentage"></param>
        public float RandomizeParameterPercentageBased(int precision, float startingValue, int percentage)
        {
            bool valueIsNegative = startingValue < 0;
            // get absolute of value, so that the result from the probability density function is calculated correctly
            startingValue = Math.Abs(startingValue);
            // calculate the range based on the percentage
            float range = startingValue * percentage / 100;
            float max = startingValue + range;
            float min = startingValue - range;
            if (valueIsNegative)
            {
                return -RandomizeParameter(precision, max, min, probabilityDensityFunction);
            }
            else
            {
                return RandomizeParameter(precision, max, min, probabilityDensityFunction);
            }
        }
    }
}

