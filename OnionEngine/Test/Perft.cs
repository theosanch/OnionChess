using System;
using System.Collections.Generic;
using System.IO;

namespace OnionEngine
{
    class Perft
    {
        Board board;
        MoveGenerator moveGenerator;

        ulong key;

        public ulong leafNodes;



        public Perft()
        {
            board = new Board();
            moveGenerator = new MoveGenerator();
        }

        private void TestPosition(ref Position position, int depth)
        {

            if (depth == 0)
            {
                leafNodes++;
                return;
            }

            int[] moves = moveGenerator.GenerateAllMoves(position);
            for (int i = 0; i < moves.Length; i++)
            {
                int n = board.MakeMove(ref position, moves[i]);
                if (n == 0)
                {
                    if(position.fiftyMoveCounter != 50) // draw
                    {
                        TestPosition(ref position, depth - 1);
                    }
                    board.UndoMove(ref position);
                }
            }
        }

        public ulong Test(Position position, int depth)
        {
            key = position.positionKey;
            Console.Write(position.ToString());
            Console.WriteLine("");
            DateTime start = DateTime.Now;

            int[] moveList = moveGenerator.GenerateAllMoves(position);

            for (int moveNumber = 0; moveNumber < moveList.Length; moveNumber++)
            {
                int n = board.MakeMove(ref position, moveList[moveNumber]);
                // if the king is in check
                if (n != 0)
                {
                    continue;
                }

                ulong cumulativeNodes = leafNodes;
                if (position.fiftyMoveCounter != 50)
                {
                    TestPosition(ref position, depth - 1);
                }
                board.UndoMove(ref position);

                Console.WriteLine((moveNumber + 1).ToString("D2") + " " + Move.ToString(moveList[moveNumber]) + ": " + (leafNodes - cumulativeNodes).ToString("D"));
            }

            Console.WriteLine("Total nodes:" + leafNodes.ToString("n"));
            TimeSpan elapsed = (DateTime.Now - start);
            Console.WriteLine(string.Format("Time: {0}:{1}:{2}:{3}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds, elapsed.Milliseconds));

            return leafNodes;
        }


        // run a series of perft test that can be compared to known perft results
        // save a log file with the results
        public void SuiteTest(int depth)
        {
            if (depth > 6 || depth < 1)
            {
                Console.WriteLine("ERROR depth must be [1,6]");
                return;
            }



            List<string[]> file = new List<string[]>();

            // read in file
            StreamReader reader = new StreamReader(@"D:\[programs]\Chess\OnionEngine\Test\temp_perftsuite.epd");
            while (!reader.EndOfStream)
            {
                file.Add(reader.ReadLine().Split(';'));
            }
            reader.Close();

            // set up stats
            ulong[] accuracy = new ulong[file.Count];
            ulong accuracyTotal = 0;
            TimeSpan[] time = new TimeSpan[file.Count];


            DateTime startTotal = DateTime.Now;
            // run each test
            for (int i = 0; i < file.Count; i++)
            {
                leafNodes = 0;

                // position setup
                string[] fen = ((file[i])[0].Split(' '));
                Position position =  board.ParseFen(fen);

                // test
                DateTime start = DateTime.Now;
                ulong nodes = Test(position, depth);

                // stats                
                time[i] = DateTime.Now - start;

                ulong test = ulong.Parse(((file[i])[depth]));
                accuracy[i] = Diff(nodes , test);
                accuracyTotal += accuracy[i];               
            }



            // write each result
            Console.Clear();
            for (int n = 0; n < file.Count; n++)
            {
                Console.WriteLine(string.Format("{2:000}. Accuracy:{0,10} Elapsed Minutes:{1,10}", accuracy[n], time[n].TotalMinutes, n).PadLeft(20));
            }
            // final results
            TimeSpan elapsedTotal = DateTime.Now - startTotal;
            Console.WriteLine(string.Format("Final - Accuracy:{0,5} Elapsed Minutes:{1,5}", accuracyTotal, elapsedTotal.TotalMinutes));






            // write log file
            string fileName = DateTime.Now.ToString("yyyy-MM-dd-mm") + "_depth-" + depth.ToString() + "_acc-" + accuracyTotal.ToString() + ".log";
            StreamWriter writer = new StreamWriter(@"D:\[programs]\Chess\OnionEngine\Test\" + fileName, true); // true to create the file
            writer.WriteLine(depth.ToString() + ";" + elapsedTotal.TotalMinutes + ";" + accuracyTotal);
            for (int i = 0; i < file.Count; i++)
            {
                writer.WriteLine((file[i])[0] + ";" + time[i].TotalMinutes.ToString() + ";" + accuracy[i].ToString());
            }
            writer.Close();
            Console.WriteLine("File Saved: " + fileName);
        }

        // wrap around prevention
        private ulong Diff(ulong a, ulong b)
        {
            if(a > b)
            {
                return a - b;
            }
            else
            {
                return b - a;
            }
        }
    }
}
