using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using EasyButtons;
using YamlDotNet;
using System.IO;
using YamlDotNet.Serialization;
using System.Collections;

namespace Assets.Scripts
{
    internal class ExportEnvironmentConfiguration : MonoBehaviour
    {
        public AirHockeyAgent airHockeyAgent;
        public string ExportPath = "Builds/";
        
        /// <summary>
        /// Provides a button in the inspector to export the environment configurations.
        /// </summary>
        [Button]
        public void ExportEnvironmentConfig()
        {
            RewardComposition();
            ObservationSpace();
        }

        private void RewardComposition() 
        { 
            Dictionary<string, string> rewardComp = airHockeyAgent.GetRewardComposition();
            // Convert to list of key value pairs to suite the serialization method
            List<KeyValuePair<string, string>> list = rewardComp.Select(kvp => kvp).ToList();
            SerializeDictToYaml(list, "reward_composition");
        }

        private void ObservationSpace()
        {
            List<KeyValuePair<string, string>> observationSpace = airHockeyAgent.GetBehaviorParameters();
            SerializeDictToYaml(observationSpace, "behavior_parameters");
        }

        private void SerializeDictToYaml(List<KeyValuePair<string, string>> dict, string fileName)
        {  
            var serializer = new Serializer();
            using (var writer = new StreamWriter($"{ExportPath}/{fileName}.yaml"))
            {
                serializer.Serialize(writer, dict);
            }
        }
    }
}
