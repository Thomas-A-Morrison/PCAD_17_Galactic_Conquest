// Program:    PCAD_Project_Galactic_Conquest
// Date:       21 APR 2025
// Programmer: Thomas A. Morrison

using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace PCAD_Project_Galactic_Conquest
{
    class Program
    {
        const int NUM_PLANETS = 12; // Maybe make this scalable to the map size. Half the side length?
        const int MAP_SIZE = 24; // Maybe let the player choose the side length (12-25).
        const double BASE_SPEED = 1.5; // Fast enough to get to an adjacent planet in one turn, even if diagonal.

        static void Main(string[] args)
        {
            char[,] map = new char[MAP_SIZE, MAP_SIZE];
            string userName = null;
            char[] planetA = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P' }; // Only 16 at this time...
            int[] planetX = new int[NUM_PLANETS]; // X-coordinate of the planet
            int[] planetY = new int[NUM_PLANETS]; // Y-coordinate of the planet
            int[] planetE = new int[NUM_PLANETS]; // Economy of the planet
            int[] planetD = new int[NUM_PLANETS]; // Defenses of the planet
            int[] planetF = new int[NUM_PLANETS]; // Fleets of the planet
            int[] planetS = new int[NUM_PLANETS]; // Settlers of the planet
            int[] planetP = new int[NUM_PLANETS]; // Probes of the planet
            string[] planetO = new string[NUM_PLANETS]; // Ownership of the planet
            int[] planetT = new int[NUM_PLANETS]; // Technology of the planet
            int[] planetEC = new int[NUM_PLANETS]; // ECON under construction on the planet
            int[] planetDC = new int[NUM_PLANETS]; // Defenses under construction on the planet
            int[] planetFC = new int[NUM_PLANETS]; // Fleets under construction on the planet
            int[] planetPC = new int[NUM_PLANETS]; // Probes under construction on the planet
            int[] planetSC = new int[NUM_PLANETS]; // Settlers under construction on the planet
            var rand = new Random();
            int turn = 0;
            int[] combat = { 0, 0, 0, 0 };
            Random percent = new Random();

            List<string> eventList = new List<string>();
            // Format: turn of arrival, destination system, fleets arriving, probes arriving, settlers arriving, system of origin, player owning.

            Console.Clear();
            Console.WriteLine("Galactic Conquest!\n");
            Console.Write("Please enter your name: ");
            string playerName = Console.ReadLine();
            planetO[0] = playerName;
            Console.WriteLine($"\nAll hail, our great and noble ruler, {planetO[0]}!");
            Console.Write($"The galaxy is yours to conquer.");
            Console.ReadKey();

            InitializeMap(map);
            InitializePlanets(planetE, planetD, planetF, planetO, planetS, planetP, planetT, planetEC, planetDC, planetFC, planetPC, planetSC);
            AssignPlanets(planetA, planetX, planetY, map);
            do
            {
                DisplayMap(map);
                DisplayInterface(planetA, planetO, planetT, planetE, planetD, planetF, planetP, planetS, planetX, planetY, turn, eventList, planetEC, planetDC, planetFC, planetPC, planetSC);
                // Other players issue their orders
                for (int i = 0; i < NUM_PLANETS; i++) 
                {
                    if (planetO[i] != playerName) 
                    {
                        if (planetE[i] > 0) // Computer opponent has ECON to spend
                        {
                            int budget = planetE[i]; 
                            planetEC[i] = planetE[i] / 2 + 1;
                            budget -= planetEC[i];
                            planetFC[i] = 2 * budget/3;
                            budget -= planetFC[i];
                            planetDC[i] = budget;
                        }
                        if (planetF[i] > 20 + (2 * turn)) 
                        {
                            int destination;
                            int number;
                            do
                            {
                                destination = rand.Next(0, NUM_PLANETS - 1);
                            } while (planetO[destination] == planetO[i]);   // Will not attack own planet
                            number = planetF[i] / 2;
                            planetF[i] -= number;
                            eventList.Add($"{TurnOfArrival(planetX[i], planetY[i], planetX[destination], planetY[destination], planetT[i], turn)},{destination},{number},0,0,{i},{i}"); // Format: turn of arrival, destination system, fleets arriving, probes arriving, settlers arriving, system of origin, player owning.
                        }
                    }
                }
                
                foreach (string a in eventList)
                {
                    string[] arrival = a.Split(',');
                    if (Convert.ToInt32(arrival[0]) == turn) 
                    {
                        if (planetO[Convert.ToInt32(arrival[1])] != planetO[Convert.ToInt32(arrival[6])])   // If the owner of the planet is not the same as the owner of the arriving ships, then combat begins.
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write($"  {planetO[Convert.ToInt32(arrival[6])]} attacks {planetO[Convert.ToInt32(arrival[1])]} at System {planetA[Convert.ToInt32(arrival[1])]}!");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                            Console.Write($"  Attacker has: {arrival[2],2} fleets, {arrival[3],2} probes, {arrival[3],2} settlers.");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                            Console.Write($"  Defender has: {planetF[Convert.ToInt32(arrival[1])],2} fleets, {planetD[Convert.ToInt32(arrival[1])],2} defenses.");
                            if ((planetF[Convert.ToInt32(arrival[1])] + planetD[Convert.ToInt32(arrival[1])]) == 0)
                            {
                                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                Console.Write("  Attacker wins without a fight! ");
                                planetO[Convert.ToInt32(arrival[1])] = arrival[6];
                                // Attacker's surviving fleets, probes, and settlers are assigned to the planet
                                planetF[Convert.ToInt32(arrival[1])] = combat[1];
                                planetP[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[3]);
                                planetS[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[4]);
                                planetE[Convert.ToInt32(arrival[1])] /= 2;  // ECON is halved due to battle damage
                                planetD[Convert.ToInt32(arrival[1])] = 0;   // Original owner's defenses destroyed
                                // Anything the original owner had under construction is destroyed: Defenses, ECON, Fleets, Probes, Settlers
                                planetDC[Convert.ToInt32(arrival[1])] = 0;
                                planetEC[Convert.ToInt32(arrival[1])] = 0;
                                planetFC[Convert.ToInt32(arrival[1])] = 0;
                                planetPC[Convert.ToInt32(arrival[1])] = 0;
                                planetSC[Convert.ToInt32(arrival[1])] = 0;
                            }
                            else if (planetT[Convert.ToInt32(arrival[1])] > planetT[Convert.ToInt32(arrival[6])])
                            {
                                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                Console.Write($"  Defender fires first due to higher technology!");
                                combat[0] = planetF[Convert.ToInt32(arrival[1])] + planetD[Convert.ToInt32(arrival[1])];    // Defender
                                combat[1] = Convert.ToInt32(arrival[2]);
                                combat[2] = planetT[Convert.ToInt32(arrival[1])];
                                combat[3] = planetT[Convert.ToInt32(arrival[6])];
                                Thread.Sleep(2000); // 2-second pause
                                ResolveCombat(combat);
                                if (combat[1] == 0) // All attackers destroyed
                                {
                                    // Assign casualties to fleets first, then defenses
                                    planetF[Convert.ToInt32(arrival[1])] = combat[0] - planetD[Convert.ToInt32(arrival[1])];
                                    if (planetF[Convert.ToInt32(arrival[1])] < 0)
                                    {
                                        planetD[Convert.ToInt32(arrival[1])] += planetF[Convert.ToInt32(arrival[1])];
                                        planetF[Convert.ToInt32(arrival[1])] = 0;
                                    }
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  *** Defender wins! ***                        ");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                    Console.Write($"  Attacker has: 0 fleets, 0 probes, 0 settlers.");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                    Console.Write($"  Defender has: {planetF[Convert.ToInt32(arrival[1])],2} fleets, {planetD[Convert.ToInt32(arrival[1])],2} defenses.");
                                }
                                else // All defenders destroyed
                                {
                                    // Attacker's surviving fleets, probes, and settlers are assigned to the planet
                                    planetF[Convert.ToInt32(arrival[1])] = combat[1];
                                    planetP[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[3]);
                                    planetS[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[4]);
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  *** Attacker wins! ***                        ");
                                    planetO[Convert.ToInt32(arrival[1])] = planetO[Convert.ToInt32(arrival[6])];    // Attacker now owns planet
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                    Console.Write($"  Attacker has: {combat[1],2} fleets, {arrival[3],2} probes, {arrival[3],2} settlers.");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                    Console.Write($"  Defender has: 0 fleets, 0 defenses.");
                                    planetE[Convert.ToInt32(arrival[1])] /= 2;  // ECON is halved due to battle damage
                                    planetD[Convert.ToInt32(arrival[1])] = 0;   // Original owner's defenses destroyed
                                    // Anything the original owner had under construction is destroyed: Defenses, ECON, Fleets, Probes, Settlers
                                    planetDC[Convert.ToInt32(arrival[1])] = 0;
                                    planetEC[Convert.ToInt32(arrival[1])] = 0;
                                    planetFC[Convert.ToInt32(arrival[1])] = 0;
                                    planetPC[Convert.ToInt32(arrival[1])] = 0;
                                    planetSC[Convert.ToInt32(arrival[1])] = 0;
                                }
                            }
                            else if ((planetT[Convert.ToInt32(arrival[1])] < planetT[Convert.ToInt32(arrival[6])]))
                            {
                                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                Console.Write($"  Attacker fires first due to higher technology!");
                                combat[0] = Convert.ToInt32(arrival[2]);    // Attacker ships
                                combat[1] = planetF[Convert.ToInt32(arrival[1])] + planetD[Convert.ToInt32(arrival[1])];    // Defender ships + defenses
                                combat[2] = planetT[Convert.ToInt32(arrival[6])];   // Attacker technology
                                combat[3] = planetT[Convert.ToInt32(arrival[1])];   // Defender technology
                                Thread.Sleep(2000); // 2-second pause
                                ResolveCombat(combat);
                                if (combat[0] == 0) // All attackers destroyed
                                {
                                    // Assign casualties to fleets first, then defenses
                                    planetF[Convert.ToInt32(arrival[1])] = combat[1] - planetD[Convert.ToInt32(arrival[1])];
                                    if (planetF[Convert.ToInt32(arrival[1])] < 0)
                                    {
                                        planetD[Convert.ToInt32(arrival[1])] += planetF[Convert.ToInt32(arrival[1])];
                                        planetF[Convert.ToInt32(arrival[1])] = 0;
                                    }
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  *** Defender wins! ***                        ");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                    Console.Write($"  Attacker has: 0 fleets, 0 probes, 0 settlers.");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                    Console.Write($"  Defender has: {planetF[Convert.ToInt32(arrival[1])],2} fleets, {planetD[Convert.ToInt32(arrival[1])],2} defenses.");
                                }
                                else // All defenders destroyed
                                {
                                    // Attacker's surviving fleets, probes, and settlers are assigned to the planet
                                    planetF[Convert.ToInt32(arrival[1])] = combat[0];
                                    planetP[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[3]);
                                    planetS[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[4]);
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  *** Attacker wins! ***                        ");
                                    planetO[Convert.ToInt32(arrival[1])] = planetO[Convert.ToInt32(arrival[6])];    // Attacker now owns planet
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                    Console.Write($"  Attacker has: {combat[0],2} fleets, {arrival[3],2} probes, {arrival[3],2} settlers.");
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                    Console.Write($"  Defender has: 0 fleets, 0 defenses.");
                                    planetE[Convert.ToInt32(arrival[1])] /= 2;  // ECON is halved due to battle damage
                                    planetD[Convert.ToInt32(arrival[1])] = 0;   // Original owner's defenses destroyed
                                    // Anything the original owner had under construction is destroyed: Defenses, ECON, Fleets, Probes, Settlers
                                    planetDC[Convert.ToInt32(arrival[1])] = 0;
                                    planetEC[Convert.ToInt32(arrival[1])] = 0;
                                    planetFC[Convert.ToInt32(arrival[1])] = 0;
                                    planetPC[Convert.ToInt32(arrival[1])] = 0;
                                    planetSC[Convert.ToInt32(arrival[1])] = 0;
                                }
                            }
                            else  // Planet is defended and technology is equal
                            {
                                int chance = percent.Next(1, 101);
                                if (chance < 51)
                                {
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  Defender fires first!");
                                    combat[0] = planetF[Convert.ToInt32(arrival[1])] + planetD[Convert.ToInt32(arrival[1])];    // Defender
                                    combat[1] = Convert.ToInt32(arrival[2]);
                                    combat[2] = planetT[Convert.ToInt32(arrival[1])];
                                    combat[3] = planetT[Convert.ToInt32(arrival[6])];
                                    Thread.Sleep(2000); // 2-second pause
                                    ResolveCombat(combat);
                                    if (combat[1] == 0) // All attackers destroyed
                                    {
                                        // Assign casualties to fleets first, then defenses
                                        planetF[Convert.ToInt32(arrival[1])] = combat[0] - planetD[Convert.ToInt32(arrival[1])];
                                        if (planetF[Convert.ToInt32(arrival[1])] < 0)
                                        {
                                            planetD[Convert.ToInt32(arrival[1])] += planetF[Convert.ToInt32(arrival[1])];
                                            planetF[Convert.ToInt32(arrival[1])] = 0;
                                        }
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                        Console.Write($"  *** Defender wins! ***                        ");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                        Console.Write($"  Attacker has: 0 fleets, 0 probes, 0 settlers.");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                        Console.Write($"  Defender has: {planetF[Convert.ToInt32(arrival[1])],2} fleets, {planetD[Convert.ToInt32(arrival[1])],2} defenses.");
                                    }
                                    else // All defenders destroyed
                                    {
                                        // Attacker's surviving fleets, probes, and settlers are assigned to the planet
                                        planetF[Convert.ToInt32(arrival[1])] = combat[1];
                                        planetP[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[3]);
                                        planetS[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[4]);
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                        Console.Write($"  *** Attacker wins! ***                        ");
                                        planetO[Convert.ToInt32(arrival[1])] = planetO[Convert.ToInt32(arrival[6])];    // Attacker now owns planet
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                        Console.Write($"  Attacker has: {combat[1],2} fleets, {arrival[3],2} probes, {arrival[3],2} settlers.");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                        Console.Write($"  Defender has: 0 fleets, 0 defenses.");
                                        planetE[Convert.ToInt32(arrival[1])] /= 2;  // ECON is halved due to battle damage
                                        planetD[Convert.ToInt32(arrival[1])] = 0;   // Original owner's defenses destroyed
                                        // Anything the original owner had under construction is destroyed: Defenses, ECON, Fleets, Probes, Settlers
                                        planetDC[Convert.ToInt32(arrival[1])] = 0;
                                        planetEC[Convert.ToInt32(arrival[1])] = 0;
                                        planetFC[Convert.ToInt32(arrival[1])] = 0;
                                        planetPC[Convert.ToInt32(arrival[1])] = 0;
                                        planetSC[Convert.ToInt32(arrival[1])] = 0;
                                    }
                                }
                                else
                                {
                                    Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                    Console.Write($"  Attacker fires first!");
                                    combat[0] = Convert.ToInt32(arrival[2]);    // Attacker ships
                                    combat[1] = planetF[Convert.ToInt32(arrival[1])] + planetD[Convert.ToInt32(arrival[1])];    // Defender ships + defenses
                                    combat[2] = planetT[Convert.ToInt32(arrival[6])];   // Attacker technology
                                    combat[3] = planetT[Convert.ToInt32(arrival[1])];   // Defender technology
                                    Thread.Sleep(2000); // 2-second pause
                                    ResolveCombat(combat);
                                    if (combat[0] == 0) // All attackers destroyed
                                    {
                                        // Assign casualties to fleets first, then defenses
                                        planetF[Convert.ToInt32(arrival[1])] = combat[1] - planetD[Convert.ToInt32(arrival[1])];
                                        if (planetF[Convert.ToInt32(arrival[1])] < 0)
                                        {
                                            planetD[Convert.ToInt32(arrival[1])] += planetF[Convert.ToInt32(arrival[1])];
                                            planetF[Convert.ToInt32(arrival[1])] = 0;
                                        }
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                        Console.Write($"  *** Defender wins! ***                        ");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                        Console.Write($"  Attacker has: 0 fleets, 0 probes, 0 settlers.");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                        Console.Write($"  Defender has: {planetF[Convert.ToInt32(arrival[1])],2} fleets, {planetD[Convert.ToInt32(arrival[1])],2} defenses.");
                                    }
                                    else // All defenders destroyed
                                    {
                                        // Attacker's surviving fleets, probes, and settlers are assigned to the planet
                                        planetF[Convert.ToInt32(arrival[1])] = combat[0];
                                        planetP[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[3]);
                                        planetS[Convert.ToInt32(arrival[1])] = Convert.ToInt32(arrival[4]);
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 13);
                                        Console.Write($"  *** Attacker wins! ***                        ");
                                        planetO[Convert.ToInt32(arrival[1])] = planetO[Convert.ToInt32(arrival[6])];    // Attacker now owns planet
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 11);
                                        Console.Write($"  Attacker has: {combat[0],2} fleets, {arrival[3],2} probes, {arrival[4],2} settlers.");
                                        Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 12);
                                        Console.Write($"  Defender has: 0 fleets, 0 defenses.");
                                        planetE[Convert.ToInt32(arrival[1])] /= 2;  // ECON is halved due to battle damage
                                        planetD[Convert.ToInt32(arrival[1])] = 0;   // Original owner's defenses destroyed
                                        // Anything the original owner had under construction is destroyed: Defenses, ECON, Fleets, Probes, Settlers
                                        planetDC[Convert.ToInt32(arrival[1])] = 0;
                                        planetEC[Convert.ToInt32(arrival[1])] = 0;
                                        planetFC[Convert.ToInt32(arrival[1])] = 0;
                                        planetPC[Convert.ToInt32(arrival[1])] = 0;
                                        planetSC[Convert.ToInt32(arrival[1])] = 0;
                                    }
                                }
                            }
                        }
                        else // Destination planet is already owned by arriving vessels
                        {
                            // Arriving Fleets, Probes, and Settlers are assigned to this planet.
                            planetF[Convert.ToInt32(arrival[1])] += Convert.ToInt32(arrival[2]);
                            planetP[Convert.ToInt32(arrival[1])] += Convert.ToInt32(arrival[3]);
                            int settlersArriving = Convert.ToInt32(arrival[4]);
                            for (int i = 0; i < settlersArriving; i++) 
                            {
                                if (planetE[Convert.ToInt32(arrival[1])] < 100) 
                                {
                                    planetE[Convert.ToInt32(arrival[1])]++; // Settlers add to the local ECON
                                    settlersArriving--;
                                }
                            }
                            planetS[Convert.ToInt32(arrival[1])] = settlersArriving;
                        }
                    }
                }
                // Anything under construction is completed: Defenses, ECON, Fleets, Probes, Settlers
                for (int i = 0; i < NUM_PLANETS; i++)
                {
                    planetD[i] += planetDC[i];
                    planetDC[i] = 0;
                    planetE[i] += planetEC[i];
                    planetEC[i] = 0;
                    planetF[i] += planetFC[i];
                    planetFC[i] = 0;
                    planetP[i] += planetPC[i];
                    planetPC[i] = 0;
                    planetS[i] += planetSC[i];
                    planetSC[i] = 0;
                    DetermineTech(planetE, planetT, planetO, i);    // Recalculate tech level
                }
                turn++;
            } while (planetO[0] == playerName); // So long as player owns home world, continue game
        }

        static void InitializeMap(char[,] map) 
        {
            // Initialize map.
            for (int xx = 0; xx < MAP_SIZE; xx++)
            {
                for (int yy = 0; yy < MAP_SIZE; yy++)
                {
                    map[xx, yy] = '.';
                }
            }
        }

        static void InitializePlanets(int[] planetE, int[] planetD, int[] planetF, string[] planetO, int[] planetS, int[]planetP, int[] planetT, int[] planetEC, int[] planetDC, int[] planetFC, int[] planetPC, int[] planetSC) 
        {
            // Initialize planetary economies, defenses, ships.
            var rand = new Random();
            string[] players = { "", "Birds", "Cats", "Dogs", "Eels", "Foxes", "Gnus", "Hawks", "Ibex", "Jays", "Kites", "Lions", "Mice", "Newts", "Owls", "Pigs" };

            for (int e = 1; e < NUM_PLANETS; e++)
            {
                planetE[e] = Convert.ToInt32(rand.Next(1, 21) / rand.Next(1, 5)); // Range of 0-20, weighted toward low end

                DetermineTech(planetE, planetT, planetO, e);

                if (planetE[e] >= 5) // Defenses at start?
                {
                    planetD[e] = planetE[e] / 2; // Half the economy's worth in defenses.
                }
                else
                {
                    planetD[e] = 0; // No defenses because technology is insufficient.
                }
                if (planetE[e] >= 10) // Fleets at start?
                {
                    planetF[e] = planetE[e] / 2; // Half the economy's worth in fleets.
                }
                else
                {
                    planetF[e] = 0; // No fleets because technology is insufficient.
                }
                planetO[e] = players[e];
                planetS[e] = 0; // No settlers until someone scouts a planet.
                if (planetE[e] >= 10) // Probes at start?
                {
                    planetP[e] = 4;
                }
                else
                {
                    planetP[e] = 0; // No probes because technology is insufficient.
                }
                planetEC[e] = 0;    // No ECON under construction
                planetDC[e] = 0;    // No defenses under construction
                planetFC[e] = 0;    // No fleets under construction
                planetPC[e] = 0;    // No probes under construction
                planetSC[e] = 0;    // No settlers under construction
            }
            planetE[0] = 10;    // Player's planet starts at 10.
            planetD[0] = 6;     // Player's planet starts with 1 additional defense.
            planetF[0] = 10;    // Player's planet starts with 5 additional fleets.
            planetP[0] = 4;     // Player's planet starts with 4 additional probes.
            planetT[0] = 3;     // Player starts at Tech Level 3.
        }

        static void AssignPlanets(char[] planetA, int[] planetX, int[] planetY, char[,] map)
        {
            var rand = new Random();
            int count = -1;
            int x = -1;
            int y = -1;
            planetX[0] = -1;
            planetY[0] = -1;
            do
            {
                count++;
                x = rand.Next(MAP_SIZE);
                y = rand.Next(MAP_SIZE);
                for (int i = 0; i < count; i++)
                {
                    if (x == planetX[i] && y == planetY[i]) // Check against duplication
                    {
                        x = rand.Next(MAP_SIZE);
                        y = rand.Next(MAP_SIZE);
                        i = 0;
                    }
                }
                planetA[count] = Convert.ToChar(65 + count);
                planetX[count] = x;
                planetY[count] = y;
                map[x, y] = planetA[count];
                // Console.WriteLine($"Planet {planetA[count]} is {map[x, y]}: Coordinates ({planetX[count]}, {planetY[count]}).");

            } while (count < NUM_PLANETS - 1);
        }

        static void DisplayMap(char[,] map) 
        {
            // Display map.
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write("╔");
            for (int aa = 0; aa < (MAP_SIZE * 2 + 1); aa++) 
            {
                Console.Write("═");
            }
            Console.WriteLine("╦");
            Console.SetCursorPosition(0, 1);
            for (int xx = 0; xx < MAP_SIZE; xx++)
            {
                Console.Write("║ ");
                for (int yy = 0; yy < MAP_SIZE; yy++)
                {
                    Console.Write($"{map[xx, yy]} ");
                }
                Console.WriteLine("║");
            }
            Console.Write("╚");
            for (int aa = 0; aa < (MAP_SIZE * 2 + 1); aa++)
            {
                Console.Write("═");
            }
            Console.WriteLine("╩");
            Console.SetCursorPosition((MAP_SIZE - 4), 0);
            Console.Write("[STAR=MAP]");
        }
        static void DisplayInterface(char[] planetA, string[] planetO, int[] planetT, int[] planetE, int[] planetD, int[] planetF, int[] planetP, int[] planetS, int[] planetX, int[] planetY, int turn, List<string> eventList, int[] planetEC, int[] planetDC, int[] planetFC, int[] planetPC, int[] planetSC) 
        {
            // Declare variables
            int turnOfArrival;
            int destination = -1;
            int spent = 0;
            ConsoleKeyInfo input;
            int econ = 0;
            int defenses = 0;
            int fleets = 0;
            int probes = 0;
            int settlers = 0;
            int number;

            // Display interface
            Console.SetCursorPosition((MAP_SIZE * 2 + 3), 0);
            Console.WriteLine("════════════════════════════[STATUS=BOARD]════════════════════════════╗");
            Console.SetCursorPosition((MAP_SIZE * 2 + 4), 1);
            Console.Write("Owner\tTech\tSystem\tEconomy\tDefense\tFleets\tProbes\tSettlers ║");
            for (int c = 0; c < NUM_PLANETS; c++)
            {
                Console.SetCursorPosition((MAP_SIZE * 2 + 4), 2 + c);
                Console.Write($"{planetO[c], -8}\t{planetT[c], 3}\t{planetA[c], 3}\t{planetE[c],5}\t{planetD[c],5}\t{planetF[c],5}\t{planetP[c],4}\t{planetS[c],4}");
            }
            for (int c = 0; c < MAP_SIZE; c++)
            {
                Console.SetCursorPosition((MAP_SIZE * 2 + 73), 1 + c);
                Console.Write("║");
            }
            Console.SetCursorPosition((MAP_SIZE * 2 + 2), NUM_PLANETS + 2);
            Console.WriteLine("╠════════════════════════════[COMMAND=MENU]════════════════════════════╣");
            Console.SetCursorPosition((MAP_SIZE * 2 + 3), MAP_SIZE + 1);
            Console.WriteLine("══════════════════════════════════════════════════════════════════════╝");
            while (spent < planetE[0])
            {
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 3);
                Console.Write($"System A: {planetE[0],2} ECON | Spent: {spent} | Turn: {turn}");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 4);
                Console.Write("  1. Build 1 ECON (cost: 1)\t\t2. Build 2 Defenses (cost: 1)");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 5);
                Console.Write("  3. Build 1 Fleet (cost: 1)\t\t4. Send Fleets");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 6);
                Console.Write("  5. Build 4 Probes (cost: 1)\t6. Send a Probe");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 7);
                Console.Write("  7. Build 1 Settler (cost: 2)\t8. Send Settlers");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 8);
                Console.Write("  9. End Turn\t\t\t0. Quit Game");
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 9);
                Console.Write("Your choice: "); // Unspent ECON by default is spent on building more ECON.
                input = Console.ReadKey();

                switch (input.Key.ToString())
                {
                    case "D1": // Build ECON
                        if (spent < planetE[0])
                        {
                            spent++;
                            planetEC[0]++;
                            Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                            Console.Write($" ECON under construction: {planetEC[0]}        ");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 30), NUM_PLANETS + 3);
                            Console.Write($"{spent}");
                        }
                        // Make event for this planet to gain that much ECON at the end of the turn if the player still owns it.
                        break;
                    case "D2": // Build defenses
                        if (spent < planetE[0])
                        {
                            spent++;
                            planetDC[0] += 2;   // Gain that many defenses at the end of the turn if the player still owns it.
                            Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                            Console.Write($" Defenses under construction: {planetDC[0]}");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 30), NUM_PLANETS + 3);
                            Console.Write($"{spent}");
                        }
                        break;
                    case "D3": // Build fleets
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  How many? ");
                            number = Convert.ToInt32(Console.ReadLine());
                            if (number > (planetE[0] - spent))
                            {
                                Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                                Console.Write($" That amount exceeds the ECON available!    ");
                            }
                            else 
                            {
                                Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                                Console.Write($"                                            ");
                            }
                        } while (number > (planetE[0] - spent));  // Can't spend more than remaining ECON

                        spent += number;
                        planetFC[0] += number;   // Gain that many fleets at the end of the turn if the player still owns it.
                        Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                        Console.Write($" Fleets under construction: {planetFC[0]}    ");
                        Console.SetCursorPosition((MAP_SIZE * 2 + 30), NUM_PLANETS + 3);
                        Console.Write($"{spent}");
                        break;
                    case "D4": // Send fleets - FROM HERE - TO WHERE? - HOW MANY? Return TurnOfArrival
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  How many? ");
                            number = Convert.ToInt32(Console.ReadLine());
                        } while (number > planetF[0]);  // Can't send more fleets than exist at that planet
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  To which system? ");
                            input = Console.ReadKey();
                            destination = GetDestination(input.Key.ToString());
                        } while (destination == -1);
                        turnOfArrival = TurnOfArrival(planetX[0], planetY[0], planetX[destination], planetY[destination], planetT[0], turn);
                        Console.SetCursorPosition((MAP_SIZE * 2 + 25), NUM_PLANETS + 10);
                        planetF[0] -= number;
                        Console.Write($" Arrives on Turn #{turnOfArrival}");
                        eventList.Add($"{turnOfArrival},{destination},{number},0,0,0,0"); // Format: turn of arrival, destination system, fleets arriving, probes arriving, settlers arriving, system of origin, player owning.
                        break;
                    case "D5": // Build probes
                        if (spent < planetE[0])
                        {
                            spent++;
                            planetPC[0] += 4;      // Gain that many probes at the end of the turn if the player still owns it.
                            Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                            Console.Write($" Probes under construction: {planetPC[0]}   ");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 30), NUM_PLANETS + 3);
                            Console.Write($"{spent}");
                        }
                        break;
                    case "D6": // Send probes - FROM HERE - TO WHERE? - HOW MANY? Return TurnOfArrival
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  To which system? ");
                            input = Console.ReadKey();
                            destination = GetDestination(input.Key.ToString());
                        } while (destination == -1);
                        turnOfArrival = TurnOfArrival(planetX[0], planetY[0], planetX[destination], planetY[destination], 2, turn);
                        Console.SetCursorPosition((MAP_SIZE * 2 + 25), NUM_PLANETS + 10);
                        planetP[0]--;
                        Console.Write($" Arrives on Turn #{turnOfArrival}");
                        eventList.Add($"{turnOfArrival},{destination},0,1,0,0,0"); // Format: turn of arrival, destination system, fleets arriving, probes arriving, settlers arriving, system of origin, player owning.
                        break;
                    case "D7": // Build settlers
                        if (spent - 1 < planetE[0])
                        {
                            spent += 2;
                            planetSC[0]++;     // Gain that many settlers at the end of the turn if the player still owns it.
                            Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 9);
                            Console.Write($" Settlers under construction: {planetSC[0]}");
                            Console.SetCursorPosition((MAP_SIZE * 2 + 30), NUM_PLANETS + 3);
                            Console.Write($"{spent}");
                        }
                        break;
                    case "D8": // Send settlers
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  How many? ");
                            number = Convert.ToInt32(Console.ReadLine());
                        } while (number > planetS[0]);  // Can't send more fleets than exist at that planet
                        do
                        {
                            Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                            Console.Write("  To which system? ");
                            input = Console.ReadKey();
                            destination = GetDestination(input.Key.ToString());
                        } while (destination == -1);

                        turnOfArrival = TurnOfArrival(planetX[0], planetY[0], planetX[destination], planetY[destination], planetT[0], turn);
                        Console.SetCursorPosition((MAP_SIZE * 2 + 25), NUM_PLANETS + 10);
                        planetS[0] -= number;
                        Console.Write($" Arrives on Turn #{turnOfArrival}");
                        eventList.Add($"{turnOfArrival},{destination},0,0,{number},0,0"); // Format: turn of arrival, destination system, fleets arriving, probes arriving, settlers arriving, system of origin, player owning.
                        break;
                    case "D9": // End Turn: all unspent ECON is reinvested into ECON
                        planetEC[0] += planetE[0] - spent;
                        spent = planetE[0];
                        break;
                    case "D0": // Quit Game
                        break;
                    default:
                        Console.SetCursorPosition((MAP_SIZE * 2 + 19), NUM_PLANETS + 10);
                        Console.Write($" Invalid input! '{input.Key.ToString()}'");
                        break;
                }

                Console.SetCursorPosition(0, MAP_SIZE + 4);
            }
        }

        static int GetDestination(string input) 
        {
            input = input.ToUpper();
            char[] chars = input.ToCharArray();
            int system = Convert.ToInt32(chars[0] - 'A');
            if (system < 0 || system > 64 + NUM_PLANETS) // Or if destination == origin...
            {
                Console.SetCursorPosition((MAP_SIZE * 2 + 3), NUM_PLANETS + 10);
                Console.Write("  Invalid input!");
                Thread.Sleep(2000); // 2-second pause
                return -1;
            }
            return system;
        }
        static int TurnOfArrival(int x1, int y1, int x2, int y2, int techLevel, int currentTurn) 
        {
            double distance = Math.Sqrt((Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2)));
            int timeToTravel = (int)Math.Ceiling(distance / (BASE_SPEED * (techLevel - 1)));
            return currentTurn + timeToTravel;
        }

        static void DetermineTech(int[] planetE, int[] planetT, string[] planetO, int e) 
        {
            int sumEcon = 0;
            for (int i = 0; i < NUM_PLANETS; i++) 
            {
                if (planetO[e] == planetO[i]) 
                {
                    sumEcon += planetE[i];
                }
            }
            switch (sumEcon)  // Initial tech level. Recalculate after every turn.
            {
                case < 5:
                    planetT[e] = 0;
                    break;
                case < 10:
                    planetT[e] = 1;
                    break;
                case < 20:
                    planetT[e] = 2;
                    break;
                case < 40:
                    planetT[e] = 3;
                    break;
                case < 80:
                    planetT[e] = 4;
                    break;
                case < 160:
                    planetT[e] = 5;
                    break;
                case < 320:
                    planetT[e] = 6;
                    break;
                case < 640:
                    planetT[e] = 7;
                    break;
                case < 1280:
                    planetT[e] = 8;
                    break;
                case < 2560:
                    planetT[e] = 9;
                    break;
                case < 5120:
                    planetT[e] = 10;
                    break;
                case < 10240:
                    planetT[e] = 11;
                    break;
                case < 20480:
                    planetT[e] = 12;
                    break;
                case < 40960:
                    planetT[e] = 13;
                    break;
            }
        }

        static void ResolveCombat(int[] combat) 
        {
            Random percent = new Random();
            int shot;
            do
            {
                for (int i = 0; i < combat[0]; i++)
                {
                    shot = percent.Next(1, 101) * (combat[2] + 1) / (combat[3] + 1);    // Random % * Shooter's tech / Target's tech. Need +1 to avoid dividing by Tech 0.
                    if (shot > 50)
                    {
                        combat[1]--;
                    }
                }
                if (combat[1] > 0)  // If there are still targets left, targets shoot back; switch roles
                {
                    for (int i = 0; i < combat[1]; i++)
                    {
                        shot = percent.Next(1, 101) * (combat[3] + 1) / (combat[2] + 1);    // Random % * Shooter's tech / Target's tech. Need +1 to avoid dividing by Tech 0.
                        if (shot > 50)
                        {
                            combat[0]--;
                        }
                    }
                }
            } while (combat[0] > 0 && combat[1] > 0);
            if (combat[0] < 0)  // If attacker's strength is less than zero, set it to zero.
            {
                combat[0] = 0;
            }
            if (combat[1] < 0)  // If defender's strength is less than zero, set it to zero.
            {
                combat[1] = 0;
            }
        }
    }
}