using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultiClientServer
{
    class Network
    {
        public void Connector()
        {
            string[] inputports = Console.ReadLine().Split(' ');
            Program.MijnPoort = int.Parse(inputports[0]);

            int[] connectedports = new int[inputports.Length - 1]; // Exclude own port
            for (int i = 0; i < inputports.Length - 1; i++)
                connectedports[i] = int.Parse(inputports[i + 1]); // Exclude own port

            foreach (string port in inputports)
                Program.Networkk.Add(int.Parse(port));

            new Server(Program.MijnPoort); // maybe move to another class
            Console.Title = Program.MijnPoort.ToString();

            try
            {
                foreach (int port in connectedports)
                    Program.Buren.Add(port, new Connection(port));
            }
            catch (Exception e) // Continue trying till server is made at port
            {
                Thread.Sleep(50);
                // retry connection till it works
            }
            


            // Send Network list info to all neighbours

            // Merge Network lists >> if Network list != same as before, send message to all neighbours with update

            // Create new thread loop that will continue reading all Network list updates
        }
    }
}
