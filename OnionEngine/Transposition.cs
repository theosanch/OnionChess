using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    struct SearchData
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

    class Transposition
    {
        // Transposition manages various large lists of data
        // Its main function is a transposition table with info on previously evaluated positions.

        #region properties
        // hash tables - each table uses a position key as the key for the entry
        private Dictionary<ulong, int> evaluationTable = new Dictionary<ulong, int>();          // the evaluation score of a searched position
        private Dictionary<ulong, int> transpositionTable = new Dictionary<ulong, int>();       // list with information about a searched position
        private Dictionary<ulong, int> princableVariationTable = new Dictionary<ulong, int>();  // the best lines to search first


        MoveGenerator moveGen;
        PositionController positionController;
        #endregion

        public Transposition(PositionController positionController, MoveGenerator moveGen)
        {
            this.moveGen = moveGen;
            this.positionController = positionController;
        }

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
        public int[] GetPVLine(Position copyPosition,int depth)
        {
            int move = GetPV(copyPosition.positionKey);
            int[] PVLine = new int[depth];
            int count = 0;

            while (move != 0 && count < depth)
            {
                if (positionController.MoveExists(copyPosition,move))
                {
                    positionController.MakeMove(ref copyPosition, move);
                    PVLine[count] = move;
                    count++;
                }
                else
                {
                    break;
                }

                move = GetPV(copyPosition.positionKey);
            }

            return PVLine;
        }






    }
}
