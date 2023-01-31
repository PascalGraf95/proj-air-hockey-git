using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.MLAgents.SideChannels;
using Unity.MLAgents;
using UnityEngine;

namespace Assets.Scripts
{
    public class AdditionalGameInformationsSideChannel : SideChannel
    {
        public AdditionalGameInformationsSideChannel()
        {
            ChannelId = new Guid("2f487771-440f-4ffc-afd9-486650eb5b7b");
        }
        
        protected override void OnMessageReceived(IncomingMessage msg)
        {
            // nothing to do
        }

        public void SendGameResultToModularRL(int scoreAgent, int scoreHuman, int gamesPlayed)
        {
            List<float> results = new List<float>();
            results.Add(scoreAgent);
            results.Add(scoreHuman);
            results.Add(gamesPlayed);
            using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteFloatList(results);
                QueueMessageToSend(msgOut);
            }
        }

    }
}
