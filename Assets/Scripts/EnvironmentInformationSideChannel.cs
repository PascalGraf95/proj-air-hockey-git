using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Unity.MLAgents.SideChannels;

namespace Assets.Scripts
{
    internal class EnvironmentInformationSideChannel : SideChannel
    {
        public EnvironmentInformationSideChannel()
        {
            ChannelId = new Guid("92744089-f2c0-49f9-ba9e-1968f1944e28");
        }

        protected override void OnMessageReceived(IncomingMessage msg)
        {
            // nothing to do
        }

        public void SendEnvironmentInformation(List<KeyValuePair<string, string>> envInfo)
        {
            // Convert list of key value pairs to a string to make it sendable
            string toSend = string.Empty;
            foreach (var item in envInfo)
            {
                toSend += item.Key;
                toSend += " ";
                toSend += item.Value;
                toSend += ",";
            }
            // Send message
            using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteString(toSend);
                QueueMessageToSend(msgOut);
            }
        }

    }
}