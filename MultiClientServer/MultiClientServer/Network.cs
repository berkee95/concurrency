using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiClientServer
{
    class Network
    {
        // List<int> receivednetwork = new List<int>();

        public void Builder(string[] inputports) // string [] inputports
        {
            // string[] inputports = Console.ReadLine().Split(' '); // Remove when parameter implemented
            Program.MijnPoort = int.Parse(inputports[0]);

            int[] connectedports = new int[inputports.Length - 1]; // Exclude own port // Possible exception array size[0]
            for (int i = 0; i < inputports.Length - 1; i++)
                connectedports[i] = int.Parse(inputports[i + 1]); // Exclude own port

            foreach (string port in inputports)
                Program.Network.Add(int.Parse(port));

            new Server(Program.MijnPoort); // Should already be able to receive input here
            Console.Title = Program.MijnPoort.ToString();


            // Retry connection till it works
            foreach (int port in connectedports)
                Connector(port);

            //// Build list to string
            //string nodes = ListToString(Program.Network);

            //// Send message with Network list info to all neighbours
            //foreach (int neighbour in Program.Buren.Keys) 
            //    Program.Buren[neighbour].Write.WriteLine("// net " + nodes);


            ///// Find a way to build the whole network without knowing the network size
            //// List has to merge and resend till everyone has the same copy
            /////// Constantly read input, but stop once last input is transmitted so it can do NetChange algorithm


            // Send Network list info to all neighbours

            // Merge Network lists >> if Network list != same as before, send message to all neighbours with update

            // Create new thread loop that will continue reading all Network list updates
        }

        // Retries connections 
        private void Connector(int port)
        {
            if (Program.MijnPoort < port) // Only connect with ports bigger than itself
            {
                bool retry = true;
                while (retry)
                {
                    try
                    {
                        Program.Buren.Add(port, new Connection(port)); // Possible exception is thrown here
                        // Send message info about table
                        // Buren[port].Write.WriteLine(personaltable);
                        retry = false; // Cannot be reached unless there is no exception         
                    }
                    catch { Thread.Sleep(50); } // Pauses for short period before retrying
                }
            }
        }

        // Build entire string from Network list
        private string ListToString(List<int> Networklist)
        {
            string nodes = "";
            for (int t = 0; t < Networklist.Count; t++)
                nodes += Networklist[t].ToString();

            return nodes;
        }
    }
}
