using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class UCI
    {

        PositionController board = new PositionController();
        MoveGenerator moveGen = new MoveGenerator();

        Position position = new Position();
        MoveController moveController = new MoveController();

        Brain brain = new Brain();

        public void Loop()
        {
            // no need to keep track of all the commands that will be happening
            //Console.BufferHeight = 10;

            //Console.WriteLine("id name Onion 0.1");
            //Console.WriteLine("id author Theodore J. Sanchez");

            //Console.WriteLine("uci ok");

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
                    ParseGo(command);
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
                    brain.Test();
                }
            }

        }

        private void ParseGo(string[] command)
        {
            int depth = -1, time = -1, increment = 0;
            int movesToGo = 30, moveTime = -1;

            for (int i = 0; i < command.Length; i++)
            {
                switch (command[i])
                {
                    case "depth":
                        depth = int.Parse(command[i + 1]);
                        break;
                    case "wtime":
                        if (position.side == Color.w)
                            time = int.Parse(command[i + 1]);
                        break;
                    case "btime":
                        if (position.side == Color.b)
                            time = int.Parse(command[i + 1]);
                        break;
                    case "winc":
                        if (position.side == Color.w)
                            increment = int.Parse(command[i + 1]);
                        break;
                    case "binc":
                        if (position.side == Color.b)
                            increment = int.Parse(command[i + 1]);
                        break;
                    case "movetime":
                        moveTime = int.Parse(command[i + 1]);
                        break;
                    case "movestogo":
                        movesToGo = int.Parse(command[i + 1]);
                        break;
                    default:
                        break;
                }
            }

            int[] list = moveGen.GenerateAllMoves(position);
            Console.WriteLine("bestmove " + moveController.PrintMove(list[0]));
            
            board.MakeMove(ref position,list[0]);
        }

        private void ParsePosition(string[] command)
        {
            int i = 0;
            if (command[1] == "startpos")   // set up the starting position
            {
                //position = board.ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1".Split(' '));
                brain.SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                i = 3;
            }
            else if (command[1] == "fen")
            {
                //position = board.ParseFen(string.Format("{0} {1} {2} {3} {4} {5}", command[2], command[3], command[4], command[5], command[6], command[7]).Split(' '));
                brain.SetPosition(command[2] + " " + command[3] + " " + command[4] + " " + command[5] + " " + command[6] + " " + command[7]);
                i = 9;
            }

            // parse any moves that have been made past the initial position
            for (i = i + 0; i < command.Length; i++)
            {
                board.MakeMove(ref position, moveController.ParseMove(position,command[i]));
                position.ply = 0;
            }


            board.PrintPosition(position);
        }

    }
}
