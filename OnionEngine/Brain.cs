using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OnionEngine
{
    class Brain
    {
        #region properties

        private Board board;

        private Transposition transposition;
        private Search search;

        private Position currentPosition;


        // an ordered list of the five "best" moves
        public int[] bestMoveList = new int[5];


        //int[] legalMoves, scores;

        //int depth = 0;
        #endregion

        public Brain()
        {
            board = new Board();

            //currentPosition = board.StartPosition();
            currentPosition = Fen.ParseFen("rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1".Split(' '));

            transposition = new Transposition();
            search = new Search(transposition);
        }

        // start looking for the best move from the current position
        public void Think(int plyDepth)
        {
            Console.WriteLine(currentPosition.ToString());

            SearchSettings searchData = new SearchSettings();
            searchData.depth = plyDepth;
            search.IterativeSearch(currentPosition.Clone(), ref searchData);
        }

        // set the current position
        internal void SetPosition(string fen)
        {
            SetPosition(fen.Split(' '));
        }
        // set the current position
        internal void SetPosition(string[] fen)
        {
            currentPosition = Fen.ParseFen(fen);
        }

        //internal void Go(string[] command)
        //{
        //    SearchSettings searchData = new SearchSettings();
        //    searchData.depth = 8;
        //    search.IterativeSearch(currentPosition.Clone(),ref searchData);



        //    int bestMove = transposition.GetPositionData(currentPosition.positionKey).bestMove;
        //    Console.WriteLine("bestmove " + moveController.PrintMove(bestMove));
        //    //positionController.PrintPosition(currentPosition);
        //}

        // receives a previously entered position command with the go command
        internal void Go(string[] position, string[] command)
        {
            ParsePosition(position);

            SearchSettings searchData = new SearchSettings();
            searchData.depth = 6;
            search.IterativeSearch(currentPosition.Clone(), ref searchData);



            int bestMove = transposition.GetPositionData(currentPosition.hashKey).bestMove;
            Console.WriteLine("bestmove " + Move.ToString(bestMove));
        }

        internal void MakeMove(string strMove)
        {
            board.MakeMove(ref currentPosition, Move.ParseMove(currentPosition, strMove));
            currentPosition.ply = 0;

            Console.WriteLine(currentPosition.ToString());
        }

        // set the current position given the "position" command
        private void ParsePosition(string[] command)
        {
            int i = 0;
            if (command[1] == "startpos")   // set up the starting position
            {
                SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                i = 3;
            }
            else if (command[1] == "fen")
            {
                SetPosition(command[2] + " " + command[3] + " " + command[4] + " " + command[5] + " " + command[6] + " " + command[7]);
                i = 9;
            }

            for (i = i + 0; i < command.Length; i++)
            {
                MakeMove(command[i]);
            }
        }

        #region test
        //run perft test on current position
        public void PerftTest(int plyDepth)
        {
            //currentPosition = board.ParseFen("3rk1r1/8/8/8/8/p6p/P6P/R3K2R w KQ - 0 1".Split(' '));
            //currentPosition = board.ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1".Split(' '));
            //currentPosition = board.ParseFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1".Split(' '));
            //currentPosition = board.ParseFen("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8".Split(' '));

            Perft perft = new Perft();

            perft.Test(currentPosition, plyDepth);
        }
        // a method for testing out whatever may need testing
        public void Test(int plyDepth)
        {
            //currentPosition = board.ParseFen("r2qk2r/5Npp/2pb1n2/pp6/4P3/P7/1P1NbPPP/R1BQ1RK1 b kq - 0 1".Split(' '));
            //currentPosition = board.ParseFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1".Split(' '));
            //currentPosition = board.ParseFen("4k3/8/8/8/8/r6r/P6P/R3K2R w KQ - 0 1".Split(' '));
            //currentPosition = board.ParseFen("4k3/8/8/8/8/8/8/4K2R w K - 0 1".Split(' '));
            //currentPosition = board.ParseFen("rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8".Split(' '));           


            //Perft perft = new Perft();
            //perft.SuiteTest(plyDepth);

            ValidatePosition(plyDepth);


        }

        public void PerformanceTest(int plyDepth)
        {
            Perft perft = new Perft();
            perft.SuiteTest(plyDepth);

        }


        // validate move generation with a given position 
        // against a chess engine that is known to have 
        // correct position results
        public void ValidatePosition(int depth)
        {
            string fen = Fen.ToFen(currentPosition);

            Console.WriteLine("");
            Console.WriteLine(fen);

            string move = "";
            
            currentPosition = Fen.ParseFen(fen.Split(' '));

            #region external engine initialization
            Process chessEngine = new Process();
            chessEngine.StartInfo.FileName = @"D:\[downloads]\stockfish-7-win\Windows\stockfish 7 x64.exe";
            chessEngine.StartInfo.UseShellExecute = false;
            chessEngine.StartInfo.CreateNoWindow = true;
            chessEngine.StartInfo.RedirectStandardInput = true;
            chessEngine.StartInfo.RedirectStandardOutput = true;
            chessEngine.Start();
            #endregion


            // external engines perft results logic
            
            chessEngine.StandardOutput.ReadLine();

            string positionCommand = "position fen " + fen;

            // each iteration will advance down a path with an incorrect node count
            for (int n = depth; n > 0; n--)
            {
                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("--------------");

                // get onion perft results
                if (move != "")
                {
                    Console.WriteLine("making move: " + move);
                    MakeMove(move);
                }
                Perft perft = new Perft();
                perft.Test(currentPosition.Clone(), n);

                Console.WriteLine("");
                Console.WriteLine("");

                #region external engine results
                List<string> externalResults = new List<string>();
                // tell external engine what to do
                chessEngine.StandardInput.WriteLine(positionCommand);
                chessEngine.StandardInput.WriteLine("perft " + n.ToString()); // n = decrementing depth
                chessEngine.StandardInput.WriteLine("isready");

                // retrieve input logic
                while (true)
                {
                    string input = "";
                    // read 
                    input = chessEngine.StandardOutput.ReadLine();

                    if (input == "readyok")
                    {
                        break;
                    }
                    else
                    {
                        externalResults.Add(input);
                        Console.WriteLine(input);
                    }
                }
                #endregion


                // comparison logic

                // if all moves are checked and move is not overwrite we
                // know no errors were found
                move = "no discrepancy found"; 

                // for each move
                for (int i = 0; i < perft.info.Count; i++)
                {
                    // find a match
                    int match = FindMatch(externalResults, perft.info[i].Split(' ')[0]);

                    // was a match found
                    if (match != -1)
                    {
                        // are nodes the same
                        if (perft.info[i].Split(' ')[1] == externalResults[match].Split(' ')[1])
                        {

                        }
                        else
                        {
                            // wrong node count found
                            
                            // if this is the first iteration
                            if (n == depth)
                            {
                                // take on "moves" for proper uci protocol
                                positionCommand += " moves ";
                            }

                            // move is used so the move can be made by onion 
                            move = perft.info[i].Split(' ')[0];
                            positionCommand += " " + move;

                            Console.WriteLine("--------------");
                            Console.WriteLine("incorrect NODE count found: " + perft.info[i]);
                            break;
                        }
                    }
                    else
                    {
                        // incorrect move found
                        Console.WriteLine("Incorrect MOVE found: " + perft.info[i]);
                    }
                }

                if (move == "no discrepancy found")
                {
                    break;
                }
            }
        }

        // look for a match and return the spot it is found in
        // return -1 if no match is found
        private int FindMatch(List<string> external, string move)
        {
            // stockfish has a ':' at the end so we add on to make the comparison easier.
            move = move + ":";

            for (int i = 0; i < external.Count; i++)
            {
                if (move == external[i].Split(' ')[0])
                {
                    return i;
                }
            }

            return -1;
        }
        #endregion
    }
}
