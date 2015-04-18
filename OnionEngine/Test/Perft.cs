using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Perft
    {
        PositionController board;
        MoveGenerator moveGenerator;
        MoveController moveController = new MoveController();

        ulong key;

        public ulong leafNodes;



        public Perft(BitBoards bitboard)
        {
            board = new PositionController(bitboard);
            moveGenerator = new MoveGenerator(bitboard);
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
                    TestPosition(ref position,depth - 1);
                    board.UndoMove(ref position);
                }
            }
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
                int n = board.MakeMove(ref position,moveList[moveNumber]);
                // if the king is in check
                if (n == 1)
                {
                    continue;
                }

                ulong cumulativeNodes = leafNodes;

                TestPosition(ref position, depth - 1);
                board.UndoMove(ref position);

                Console.WriteLine(moveNumber.ToString("D2") + " " + moveController.PrintMove(moveList[moveNumber]) + ": " + (leafNodes - cumulativeNodes).ToString("D"));
            }

            Console.WriteLine("Total nodes:" + leafNodes.ToString("n"));
            TimeSpan elapsed = (DateTime.Now - start);
            Console.WriteLine(string.Format("Time: {0}:{1}:{2}:{3}", elapsed.Hours, elapsed.Minutes, elapsed.Seconds,elapsed.Milliseconds));

            return leafNodes;
        }

    }
}
