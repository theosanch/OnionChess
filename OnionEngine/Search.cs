using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Search
    {
        Transposition transposition;

        Evaluate evaluation = new Evaluate();

        MoveGenerator moveGenerator;

        MoveController moveController = new MoveController();
        PositionController positionController;

        public Search(Transposition transposition, BitBoards bitboards)
        {
            this.transposition = transposition;
            this.moveGenerator = new MoveGenerator(bitboards);
            this.positionController = new PositionController(bitboards);
        }


        public int AlphaBetaSearch(ref Position position, int alpha, int beta, int depth, ref SearchSettings searchSettings)
        {
            // end of node - return the evaluation score
            if (depth == 0)
            {
                searchSettings.nodes++;
                return evaluation.QuickEvaluate(position);
            }

            // TODO: check for repetition or 50 move

            // generate each move
            int[] moves = moveGenerator.GenerateAllMoves(position);

            int legalMoves = 0; // if no legal moves = check mate or stalemate
            int oldAlpha = alpha;
            int bestMove = 0;
            int score = int.MinValue;

            // go through each move
            for (int i = 0; i < moves.Length; i++)
            {
                EvaluationEntry entry = new EvaluationEntry(0,0,0,ScoreFlag.Empty);

                // make the move
                int n = positionController.MakeMove(ref position,moves[i]);
                if (n != 0) // not a legal move
                {
                    continue;
                }

                legalMoves++;

                // check if move is in transposition table

                score = -AlphaBetaSearch(ref position, -beta, -alpha, depth - 1, ref searchSettings);   // next depth

                positionController.UndoMove(ref position);

                if (score > alpha)
                {
                    if (score > beta)
                    {

                        transposition.AddPositionScore(position.positionKey,);

                        return beta;
                    }
                    alpha = score;
                    bestMove = moves[i];
                }
            }

            // mate or stalemate
            if (legalMoves == 0)
            {
                // TODO:
            }

            if (alpha != oldAlpha)
            {
                //transposition.AddPV(position.positionKey,bestMove);
            }
            else
            {

            }

            return alpha;
        }



        public void IterativeSearch(Position position, ref SearchSettings searchData)
        {
            int bestMove = 0;
            int bestScore = 0;

            int[] pvMoves;



            for (int currentDepth = 1; currentDepth <= searchData.depth; currentDepth++)
            {
                AlphaBetaSearch(ref position, int.MaxValue, int.MinValue, currentDepth, ref searchData);
                pvMoves = transposition.GetPVLine(position,currentDepth);
                bestMove = pvMoves[0];

                Console.WriteLine(string.Format("depth:{0} score:{1} move:{2} nodes:{3}",currentDepth,bestScore,moveController.PrintMove(bestMove),searchData.nodes));


            }
        }
    }
}
