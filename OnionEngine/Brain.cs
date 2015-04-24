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

        Transposition transposition;
        Search search;

        public Position currentPosition;



        // an ordered list of the five "best" moves
        public int[] bestMoveList = new int[5];

        
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

            transposition = new Transposition(positionController,moveGen);
            search = new Search(transposition,bitboards);
        }

        // start looking for the best move from the current position
        public int Think()
        {
            
            return 0;
        }

        // set the current position
        internal void SetPosition(string fen)
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
            positionController.PrintPosition(currentPosition);
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
        // a method for testing out whatever may need testing
        public void Test(int plyDepth)
        {
            currentPosition = positionController.ParseFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1".Split(' '));
            //Perft perft = new Perft(bitboards);

            //perft.Test(ref currentPosition, plyDepth);
            SearchSettings searchData = new SearchSettings();
            searchData.depth = plyDepth;
            search.IterativeSearch(currentPosition.Clone(),ref searchData);




        }
        #endregion
    }
}
