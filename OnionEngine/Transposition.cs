using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    struct SearchSettings
    {
        public int startTime;
        public int stopTime;
        public int depth;
        public int depthSet;
        public int timeSet;

        public int movesToGo;
        public bool infinite;  // infinite search mode

        public ulong nodes;

        public bool quite;     // exit the search
        public bool stopped;   // end the search but maintain results
    }

    struct EvaluationEntry
    {
        public int bestMove;
        public int score;
        public int depth;
        public ScoreFlag scoreFlag;

        public EvaluationEntry(int bestMove, int score, int depth, ScoreFlag scoreFlag)
        {
            this.bestMove = bestMove;
            this.score = score;
            this.depth = depth;
            this.scoreFlag = scoreFlag;
        }
    }

    
    enum ScoreFlag
    {
        Empty,  // 
        Exact,  // When a position is evaluated the score is considered exact. 
        Alpha,  // Alpha and Beta cutoff positions get handed an exact score from a position higher in the tree
        Beta
    }

    class Transposition
    {
        // Transposition manages various large lists of data
        // Its main function is a transposition table with info on previously evaluated positions.

        #region properties
        // hash tables - each table uses a position key as the key for the entry
        private Dictionary<ulong, int> evaluationTable = new Dictionary<ulong, int>();                              // the evaluation score of a searched position
        private Dictionary<ulong, EvaluationEntry> transpositionTable = new Dictionary<ulong, EvaluationEntry>();   // list with information about a searched position
        private Dictionary<ulong, int> princableVariationTable = new Dictionary<ulong, int>();                      // the best lines to search first

        Board board = new Board();
        #endregion

        public Transposition()
        {
            
            
        }


        #region principle variation
        public void AddPV(ulong key, int move)
        {
            princableVariationTable.Add(key,move);
        }

        private int GetPV(ulong key)
        {
            int move = 0;
            if (princableVariationTable.TryGetValue(key,out move))
            {
                return move;
            }
            return move;
        }
        // send in a copy of the position
        public int[] GetPVLine(Position copyOfPosition, int depth)
        {
            int move = GetPV(copyOfPosition.positionKey);
            int[] PVLine = new int[depth];
            int count = 0;

            while (move != 0 && count < depth)
            {
                if (board.MoveExists(copyOfPosition,move))
                {
                    board.MakeMove(ref copyOfPosition, move);
                    PVLine[count] = move;
                    count++;
                }
                else
                {
                    break;
                }

                move = GetPV(copyOfPosition.positionKey);
            }

            return PVLine;
        }
        #endregion

        #region Transposition Table
        public EvaluationEntry GetPositionData(ulong positionKey)
        {
            EvaluationEntry entry;
            if (transpositionTable.TryGetValue(positionKey,out entry))
            {
                return entry;
            }

            return new EvaluationEntry(0,0,0,ScoreFlag.Empty);
        }

        public void AddPositionScore(ulong positionKey,EvaluationEntry entry)
        {
            EvaluationEntry existingEntry = GetPositionData(positionKey);

            // add entry if no entry exists
            if (existingEntry.scoreFlag == ScoreFlag.Empty)
            {
                transpositionTable.Add(positionKey,entry);
            }
            // decide whether an entry should be overridden
            else if (existingEntry.scoreFlag == ScoreFlag.Exact)
            {
                
            }
            else
            {
                if(existingEntry.scoreFlag == ScoreFlag.Beta &&
                    entry.score < existingEntry.score)
                {
                    transpositionTable.Remove(positionKey);
                    transpositionTable.Add(positionKey, entry);
                }
                else if (existingEntry.scoreFlag == ScoreFlag.Alpha &&
                    entry.score > existingEntry.score)
                {
                    transpositionTable.Remove(positionKey);
                    transpositionTable.Add(positionKey, entry);
                }
            }
        }
        #endregion
    }
}
