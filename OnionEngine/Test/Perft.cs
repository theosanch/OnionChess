using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Perft
    {
        Board board = new Board();
        MoveGenerator moveGenerator = new MoveGenerator();
        MoveController moveController = new MoveController();

        ulong key;

        public ulong leafNodes;



        public Perft()
        {

        }

        public ulong TestPosition(ref Position position, int depth)
        {

            if (depth == 0)
            {
                leafNodes++;
                return leafNodes;
            }

            int[] moves = moveGenerator.GenerateAllMoves(position);

            for (int i = 0; i < moves.Length; i++)
            {
                
                if (moves[i] == 0)
                {
                    break;
                }

                if (board.MakeMove(ref position,moves[i]))
                {
                    //Console.WriteLine("move made");
                    //board.PrintPosition(position);

                    TestPosition(ref position,depth - 1);
                    board.UndoMove(ref position);

                    //Console.WriteLine("move un made");
                    //board.PrintPosition(position);
                }
            }

            return leafNodes;
        }

        public ulong Test(ref Position position, int depth)
        {

            key = position.positionKey;

            board.PrintPosition(position);
            Console.WriteLine("");

            DateTime start = DateTime.Now;

            int[] moveList = moveGenerator.GenerateAllMoves(position);

            for (int moveNumber = 0; moveNumber < moveList.Length; moveNumber++)
            {
                // if out of moves
                if (moveList[moveNumber] == 0)
                {
                    break;
                }
                // if the position is incorrect there is an error in move generation
                if (key != position.positionKey)
                {
                    board.PrintPosition(position);
                    break;
                }


                // is this move legal
                if (!board.MakeMove(ref position,moveList[moveNumber]))
                {
                    continue;
                }

                //Console.WriteLine("move made");
                //board.PrintPosition(position);


                ulong cumulativeNodes = leafNodes;

                TestPosition(ref position, depth - 1);
                board.UndoMove(ref position);

                Console.WriteLine(moveNumber.ToString() + " " + moveController.PrintMove(moveList[moveNumber]) + " : Nodes: " + (leafNodes - cumulativeNodes).ToString("n"));
            }

            Console.WriteLine("Perft Test Complete - Total nodes:" + leafNodes.ToString("n"));
            TimeSpan elapsed = (DateTime.Now - start);
            Console.WriteLine(string.Format("Time: {0}:{1}:{2}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds));

            return leafNodes;
        }

    }
}
