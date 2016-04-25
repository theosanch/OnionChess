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
        Evaluate evaluation;
        Board board;
        MoveGenerator moveGenerator;

        public Search(Transposition transposition)
        {
            this.transposition = transposition;
            this.board = new Board();
            this.evaluation = new Evaluate();
            this.moveGenerator = new MoveGenerator();
        }


        public int AlphaBetaSearch(ref Position position, int alpha, int beta, int depth, ref SearchSettings searchSettings)
        {
            // end of node - return the evaluation score
            if (depth == 0)
            {
                searchSettings.nodes++;
                //return evaluation.QuickEvaluate(position);
                return evaluation.MediumEvaluation(position);
            }

            // TODO: check for repetition or 50 move

            // generate each move
            int[] moves = moveGenerator.GenerateAllMoves(position);
            //Console.WriteLine("Search AlphaBeta GenerateMoves");

            int legalMoves = 0; // if no legal moves = check mate or stalemate
            int oldAlpha = alpha;

            int score = -999999;
            int bestScore = -999999;

            int bestMove = -1;

            EvaluationEntry entry = new EvaluationEntry(0, 0, depth, ScoreFlag.Empty); // TODO: best move

            // go through each move
            for (int i = 0; i < moves.Length; i++)
            {

                // make the move
                int n = board.MakeMove(ref position, moves[i]);
                if (n != 0) // not a legal move
                {
                    continue;
                }

                legalMoves++;

                // check if move is in transposition table
                entry = transposition.GetPositionData(position.hashKey);
                if (entry.depth >= depth && entry.scoreFlag != ScoreFlag.Empty)
                {
                    if(entry.scoreFlag == ScoreFlag.Exact)
                    {
                        board.UndoMove(ref position);
                        return entry.score;
                    }
                    else if (entry.scoreFlag == ScoreFlag.Alpha && entry.score <= alpha)
                    {
                        board.UndoMove(ref position);
                        return alpha;
                    }
                    else if (entry.scoreFlag == ScoreFlag.Beta && entry.score >= beta)
                    {
                        board.UndoMove(ref position);
                        return beta;
                    }
                }


                if(position.fiftyMoveCounter == 50)
                {
                    score = 0;
                }
                else
                {
                    score = (-1) * AlphaBetaSearch(ref position, (-1) * beta, (-1) * alpha, depth - 1, ref searchSettings);  // next depth
                }
                entry.score = score;

                board.UndoMove(ref position);
                if (score > bestScore)
                {
                    bestScore = score;
                    entry.bestMove = moves[i];
                    bestMove = moves[i];


                    if (score > alpha)
                    {
                        if (score >= beta)
                        {
                            entry.score = beta;
                            entry.scoreFlag = ScoreFlag.Beta;
                            //entry.depth = 0;
                            transposition.AddPositionScore(position.hashKey, entry);

                            return beta;
                        }
                        alpha = score;
                    }
                }

            }

            // mate or stalemate
            if (legalMoves == 0)
            {
                // TODO:
            }

            if (alpha != oldAlpha)
            {
                entry.bestMove = bestMove;
                entry.score = bestScore;
                entry.scoreFlag = ScoreFlag.Exact;
                transposition.AddPositionScore(position.hashKey, entry);
            }
            else
            {
                entry.bestMove = bestMove;
                entry.score = alpha;
                entry.scoreFlag = ScoreFlag.Alpha;
                transposition.AddPositionScore(position.hashKey, entry);
            }

            return alpha;
        }



        public void IterativeSearch(Position position, ref SearchSettings searchData)
        {
            EvaluationEntry entry;


            for (int currentDepth = 1; currentDepth <= searchData.depth; currentDepth++)
            {
                AlphaBetaSearch(ref position, -999999, 999999, currentDepth, ref searchData);

                entry = transposition.GetPositionData(position.hashKey);

                Console.WriteLine(string.Format("depth:{0} score:{1} move:{2} nodes:{3}", currentDepth, entry.score, Move.ToString(entry.bestMove), searchData.nodes));


            }
        }
    }
}
