using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Brain
    {
        PositionController positionController = new PositionController();
        MoveGenerator moveGen = new MoveGenerator();
        Evaluate evaluation = new Evaluate();

        public Position currentPosition;

        // an ordered list of the five "best" moves
        public int[] bestMoveList = new int[5];

        int[] legalMoves, scores;

        int depth = 0;

        public Brain()
        {
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
            Perft perft = new Perft();

            perft.Test(ref currentPosition,plyDepth);
        }

        public void Test()
        {

        }


    }
}
