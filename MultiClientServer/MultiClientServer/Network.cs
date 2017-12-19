using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiClientServer
{
    class Network
    {
        public void Builder()
        {
            string[] inputports = Console.ReadLine().Split(' ');
            Program.MijnPoort = int.Parse(inputports[0]);

            int[] connectedports = new int[inputports.Length - 1]; // Exclude own port // Possible exception array size[0]
            for (int i = 0; i < inputports.Length - 1; i++)
                connectedports[i] = int.Parse(inputports[i + 1]); // Exclude own port

            foreach (string port in inputports)
                Program.Networkk.Add(int.Parse(port));

            new Server(Program.MijnPoort); // maybe move to another class
            Console.Title = Program.MijnPoort.ToString();

            // Retry connection till it works
            foreach (int port in connectedports)
                Connector(port);

            // Send message with Network list info to all neighbours
            foreach (int neighbour in Program.Buren.Keys) // put // net here (after neighbour)
                foreach (int node in Program.Networkk)
                    Program.Buren[neighbour].Write.Write("// net {0} {1}", node, Program.Networkk.Count);



            ///// Find a way to build the whole network without knowing the network size
            //// List has to merge and resend till everyone has the same copy
            /////// Constantly read input, but stop once last input is transmitted so it can do NetChange algorithm

            string input = Console.ReadLine(); // Just for testing purposes, should be in Program (maybe)
            while (true)
            {
                if (input.StartsWith("// net"))
                {
                    List<int> receivednetwork = new List<int>();
                    receivednetwork.Add(int.Parse(Console.ReadLine()));
                }
            }



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
                        retry = false; // Cannot be reached unless there is no exception         
                    }
                    catch { Thread.Sleep(50); } // Pauses for short period before retrying
                }
            }
        }
    }
}
