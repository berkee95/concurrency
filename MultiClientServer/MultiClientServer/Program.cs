using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public int[] Network = { 55500, 55501, 55502, 55503, 55504, 55505, 55506, 55507, 55508, 55509, 55510 }; // Find solution to know network size and nodes in it
        static public List<int> Networkk;
        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public Dictionary<int, Dictionary<int, int>> NeighbourTable = new Dictionary<int, Dictionary<int, int>>();
        static public Dictionary<int, int> PersonalTable = new Dictionary<int, int>();
        static public Dictionary<int, int> PrefNeighbour = new Dictionary<int, int>();
        static public object Locker = new object();
        // PrefNeighbour = {destination, neighbour} Dictionary** (key: destination) or List 
        // DistanceNeighbour (D.w[v]) = {destination, distance} * Neighbours (for one destination v) 
        // DistanceYou (D.u[v]) = {destination, distance} Dictionary (key: destination)

        static void Main(string[] args)
        {
            Console.Write("Op welke poort ben ik server? ");
            MijnPoort = int.Parse(Console.ReadLine());
            new Server(MijnPoort);
            Console.Title = MijnPoort.ToString();

            Console.WriteLine("Typ [verbind poortnummer] om verbinding te maken, bijvoorbeeld: verbind 1100");
            Console.WriteLine("Typ [poortnummer bericht] om een bericht te sturen, bijvoorbeeld: 1100 hoi hoi");

            Console.ReadLine();
            ///////////////////////////
            /////// Initialization ///////
            ////////////////////////////

            // TESTING
            //for (int i = 5500; i <= 5510; i++)
            //    Buren.Add(i, new Connection(i));
            //for (int i = 55500; i < 55520; i++)
            //    Network.Add(i);

            ////////////////
            ////////////////

            foreach (int node in Network) // Every Node
            {
                PersonalTable.Add(node, Network.Length);
                PrefNeighbour.Add(node, 404); // Actually null
            }

            foreach (int neighbour in Buren.Keys)
                NeighbourTable.Add(neighbour, PersonalTable);

            PersonalTable[MijnPoort] = 0;
            PrefNeighbour[MijnPoort] = MijnPoort; // local

            //Buren.Add(0, new Connection(i));

            foreach (int neighbour in Buren.Keys) // Send message to all neighbours about itself
                Buren[neighbour].Write.WriteLine(MijnPoort.ToString() + " " + MijnPoort.ToString() + " " + "0");
            // Buren[neighbour].Write.WriteLine("{0} {1} 0", MijnPoort.ToString(), MijnPoort.ToString());
            
            //////////////////////////////
            /////////////////////////
            //////// Recompute /////////
            //////////////////////////

            while (true)
            {
                string input = Console.ReadLine();
                if (input.StartsWith("verbind")) // Remove all this
                {
                    int poort = int.Parse(input.Split()[1]);
                    if (Buren.ContainsKey(poort))
                        Console.WriteLine("Hier is al verbinding naar!");
                    else
                    {
                        // Leg verbinding aan (als client)
                        Buren.Add(poort, new Connection(poort));
                    }
                }
                else if (input == "R") // testing // RouteTable
                {
                    foreach (int node in Network)
                        Console.WriteLine(node.ToString() + " " + PersonalTable[node].ToString());
                }
                else if (input.StartsWith("B ")) // Sending a message 
                {
                    string [] parts = input.Split(' ');
                    int port = int.Parse(parts[1]);
                    if (PersonalTable[port] == Network.Length) // Unreachable destination
                    {
                        Console.WriteLine("Poort {0} is niet bekend", port);
                    }
                    else
                    {
                        string message = parts[2];
                        MessageDeliverer(port, message); // Sends message to correct destination via other neighbours/ports
                    }
                }
                else if (input.StartsWith("C ")) // Makes new connection
                {
                    int port = int.Parse(input.Split(' ')[1]);
                    if (Buren.ContainsKey(port))
                        Console.WriteLine("Al een verbinding.");
                    else
                    {
                        Repair(port);
                        Console.WriteLine("Verbonden: " + port);
                    }
                }
                else if(input.StartsWith("D ")) // Terminates connection
                {
                    int port = int.Parse(input.Split(' ')[1]);
                    if (Buren.ContainsKey(port))
                    {
                        Fail(port);
                        Console.WriteLine("Verbroken: " + port); // Might not be everything
                    }
                    else
                        Console.WriteLine("Poort {0} is niet bekend", port);
                }
                else // Makes entire routing table
                {
                    string[] message = input.Split(' ');
                    MessageReceiver(int.Parse(message[0]), int.Parse(message[1]), int.Parse(message[2]));                 
                }
                //else
                //{
                //    // Stuur berichtje
                //    string[] delen = input.Split(new char[] { ' ' }, 2);
                //    int poort = int.Parse(delen[0]);
                //    if (!Buren.ContainsKey(poort))
                //        Console.WriteLine("Hier is al verbinding naar!");
                //    else
                //        Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                //}
            }
        }

        // Recalculates distance to destination
        private static void Recompute(int destination)
        {
            int old_value = PersonalTable[destination];

            if (destination == MijnPoort) // Own Port
            {
                PersonalTable[destination] = 0;
                PrefNeighbour[destination] = MijnPoort; // local
            }
            else
            {
                Tuple<int, int> distance_and_route = (ShortestRoute(destination));
                int distance = distance_and_route.Item1 + 1;
                int route = distance_and_route.Item2;

                if (distance < Network.Length)
                {
                    lock (Locker)
                    {
                        PersonalTable[destination] = distance;
                        PrefNeighbour[destination] = route;
                    }
                }
                else // Node not connected to network
                {
                    lock (Locker)
                    {
                        PersonalTable[destination] = Network.Length;
                        PrefNeighbour[destination] = 404; // null
                    }
                }
            }

            if (PersonalTable[destination] != old_value) // if distance to destination has changed
            {
                foreach (int neighbour in Buren.Keys) // Send message to all neighbours for destination
                    Buren[neighbour].Write.WriteLine(MijnPoort.ToString() + " " + destination.ToString() + " " + PersonalTable[destination].ToString());
                    // Buren[neighbour].Write.WriteLine("{0} {1} {2}", MijnPoort.ToString(), destination.ToString(), PersonalTable[destination].ToString());

            // afstand message bijgewerkt <>
            }
        }

        // Calculates shortest path from any neighbour to a destination
        private static Tuple<int, int> ShortestRoute(int destination)
        {
            int shortest_distance = Network.Length; // 
            int shortest_route = 404; // null

            foreach (int neighbour in NeighbourTable.Keys)
            {
                int distance = NeighbourTable[neighbour][destination];
                if (distance < shortest_distance)
                {
                    shortest_distance = distance;
                    shortest_route = neighbour; // Neighbour used
                }
            }
            return new Tuple<int, int>(shortest_distance, shortest_route);
        }

        // Receives messages used for making Routing Table
        private static void MessageReceiver(int sender, int destination, int distance)
        {
            NeighbourTable[sender][destination] = distance;
            Recompute(destination);
        }

        // Connects a new neighbour and recalculates the distances
        private static void Repair(int port)
        {
            Buren.Add(port, new Connection(port));
            Dictionary<int, int> clean_dict = new Dictionary<int, int>();
            
            foreach (int node in Network)
                clean_dict.Add(node, Network.Length); 
            NeighbourTable[port] = clean_dict;

            foreach (int node in Network) // Send message for all destinations to new neighbour
                Buren[port].Write.WriteLine(MijnPoort.ToString() + " " + node.ToString() + " " + PersonalTable[node].ToString());
        }

        // Terminates a connection to a neighbour and recalculates the distances
        private static void Fail(int port)
        {
            Buren.Remove(port);
            foreach (int node in Network)
                Recompute(node);
        }

        // Send message to correct port 
        private static void MessageDeliverer(int port, string message)
        {
            if (MijnPoort == port) // message delivered for port
            {
                Console.WriteLine(message);
            }
            else // Send to next neighbour along the route
            {
                int prefneighbour = PrefNeighbour[port];
                Buren[prefneighbour].Write.WriteLine("B {0} {1}", port, message);
                Console.WriteLine("Bericht voor {0} doorgestuurd naar {1}", port, prefneighbour);
            }
        }
    }
}
