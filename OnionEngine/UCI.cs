using System;
using System.Collections.Generic;

namespace OnionEngine
{
    class UCI
    {
        Brain brain = new Brain();

        string currentFen;
        string lastmove = "";

        string[] position;

        public void Loop()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly()
                                                                    .GetName()
                                                                    .Version
                                                                    .ToString();

            Console.WriteLine(string.Format("Onion {0} by Theodore J. Sanchez", version));

            bool run = true;

            while (run)
            {
                // get input - separate input by spaces
                string[] command = Console.ReadLine().Split(' ');

                if (command[0] == "\n" || command[0] == "")
                {
                    continue;
                }

                if (command[0] == "isready")
                {
                    Console.WriteLine("readyok"); // are we ready?
                }
                else if (command[0] == "position")
                {
                    
                    position = new string[6];
                    for (int i = 0; i < position.Length; i++)
                    {
                        position[i] = command[i + 2];
                    }

                    brain.SetPosition(position);


                    if(command.Length > 8 && command[8] == "moves")
                    {
                        for (int i = 9; i < command.Length; i++)
                        {
                            brain.MakeMove(command[i]);
                        }
                    }
                }
                else if (command[0] == "ucinewgame")
                {

                }
                else if (command[0] == "go")
                {
                    //brain.Go(command);
                    brain.Go(position, command);
                }
                else if (command[0] == "quit") // close engine
                {
                    run = false;
                }
                else if (command[0] == "uci")
                {
                    Console.WriteLine("id name Onion 0.13");
                    Console.WriteLine("id author Theodore J. Sanchez");

                    Console.WriteLine("uci ok");
                }

                else if (command[0] == "perft")
                {
                    brain.PerftTest(int.Parse(command[1]));
                }
                else if (command[0] == "test")
                {
                    brain.Test(int.Parse(command[1]));
                }
                else
                {

                }
            }
        }
    }
}
