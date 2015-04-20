using System;
using System.Collections.Generic;

namespace OnionEngine
{
    class UCI
    {
        Brain brain = new Brain();
            
        string currentFen;
        List<string> currentMoves = new List<string>();

        public void Loop()
        {
            // no need to keep track of all the commands that will be happening
            //Console.BufferHeight = 10;

            //Console.WriteLine("id name Onion 0.1");
            //Console.WriteLine("id author Theodore J. Sanchez");

            Console.WriteLine("Onion v0.1 by Theodore J. Sanchez");


            while (true)
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
                    ParsePosition(command);
                }
                else if (command[0] == "ucinewgame")
                {

                }
                else if (command[0] == "go")
                {
                    brain.Go(command);
                }
                else if (command[0] == "quit") // close engine
                {
                    break;
                }
                else if (command[0] == "uci")
                {
                    Console.WriteLine("id name Onion 0.1");
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
            }
        }

        private void ParsePosition(string[] command)
        {
            int i = 0;
            if (command[1] == "startpos")   // set up the starting position
            {
                //position = board.ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1".Split(' '));
                brain.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                //currentFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                i = 3;
            }
            else if (command[1] == "fen")
            {
                //position = board.ParseFen(string.Format("{0} {1} {2} {3} {4} {5}", command[2], command[3], command[4], command[5], command[6], command[7]).Split(' '));
                brain.SetPosition(command[2] + " " + command[3] + " " + command[4] + " " + command[5] + " " + command[6] + " " + command[7]);
                //currentFen = command[2] + " " + command[3] + " " + command[4] + " " + command[5] + " " + command[6] + " " + command[7];
                i = 9;
            }

            // does the previous move match the previous move received
            if (currentMoves.Count > 0 && command[command.Length - 2] == currentMoves[currentMoves.Count - 1])
            {
                brain.MakeMove(command[command.Length - 1]);
                currentMoves.Add(command[i]);
            }
            else
            {
                //parse any moves that have been made past the initial position
                for (i = i + 0; i < command.Length; i++)
                {
                    brain.MakeMove(command[i]);
                    currentMoves.Add(command[i]);
                }
            }
        }
    }
}
