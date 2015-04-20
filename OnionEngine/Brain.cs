using System;
using System.Collections.Generic;

namespace OnionEngine
{
    class Brain
    {
        #region properties
        PositionController positionController;
        MoveController moveController;
        BitBoards bitboards;

        MoveGenerator moveGen;

        Evaluate evaluation = new Evaluate();

        public Position currentPosition;



        // an ordered list of the five "best" moves
        public int[] bestMoveList = new int[5];

        // a list with a position key and a evaluation score for that position
        Dictionary<ulong, int> evaluationTable = new Dictionary<ulong, int>();  // the evaluation score of a searched position
        Dictionary<int, int> transpositionTable = new Dictionary<int, int>();   // list with information about a searched position

        int[] legalMoves, scores;

        int depth = 0;
        #endregion

        public Brain()
        {
            bitboards = new BitBoards();
            positionController = new PositionController(bitboards);
            moveGen = new MoveGenerator(bitboards);
            moveController = new MoveController();

            currentPosition = positionController.StartPosition();
        }

        // start looking for the best move from the current position
        public int Think()
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

        #region test
        //run perft test on current position
        public void PerftTest(int plyDepth)
        {
            Perft perft = new Perft(bitboards);

            perft.Test(ref currentPosition,plyDepth);
        }
        public void Test(int plyDepth)
        {
            currentPosition = positionController.ParseFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1".Split(' '));
            Perft perft = new Perft(bitboards);

            perft.Test(ref currentPosition, plyDepth);
        }
        #endregion
    }
}
