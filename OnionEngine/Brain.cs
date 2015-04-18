using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Brain
    {
        PositionController positionController;
        MoveController moveController;
        BitBoards bitboards;

        MoveGenerator moveGen;

        Evaluate evaluation = new Evaluate();

        public Position currentPosition;

        // an ordered list of the five "best" moves
        public int[] bestMoveList = new int[5];

        int[] legalMoves, scores;

        int depth = 0;

        public Brain()
        {
            bitboards = new BitBoards();
            positionController = new PositionController(bitboards);
            moveGen = new MoveGenerator(bitboards);
            moveController = new MoveController();

            currentPosition = positionController.StartPosition();
        }

        // start looking for the best move from the current position
        public async Task<int> Think()
        {
            // generate all moves
            legalMoves = moveGen.GenerateAllMoves(currentPosition);
            scores = new int[legalMoves.Length];
            // evaluate each position
            for (int i = 0; i < legalMoves.Length; i++)
            {
                positionController.MakeMove(ref currentPosition,legalMoves[0]);

                scores[i] = evaluation.QuickEvaluate(currentPosition);

                positionController.UndoMove(ref currentPosition);
            }
            // pick best move


            return 0;
        }

        // set the current position
        public void SetPosition(string fen)
        {
            currentPosition = positionController.ParseFen(fen.Split(' '));
        }

        //run perft test on current position
        public void PerftTest(int plyDepth)
        {
            Perft perft = new Perft(bitboards);

            perft.Test(ref currentPosition,plyDepth);
        }

        public void Test(int plyDepth)
        {
            currentPosition = positionController.ParseFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1".Split(' '));
            Perft perft = new Perft(bitboards);

            perft.Test(ref currentPosition, plyDepth);
        }



        internal void Go(string[] command)
        {
            Random rng = new Random();
            int[] moves = moveGen.GenerateAllMoves(currentPosition);
            List<int> legalMoves = new List<int>();
            foreach (int move in moves)
            {
                if (positionController.MakeMove(ref currentPosition,move) == 0)
                {
                    legalMoves.Add(move);
                    positionController.UndoMove(ref currentPosition);
                }
            }
            
            Console.WriteLine("bestmove " + moveController.PrintMove(legalMoves[rng.Next(legalMoves.Count)]));

        }

        internal void MakeMove(string strMove)
        {
            positionController.MakeMove(ref currentPosition, moveController.ParseMove(currentPosition, strMove));
            currentPosition.ply = 0;

            positionController.PrintPosition(currentPosition);
        }
    }
}
