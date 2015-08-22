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

        //internal void Go(string[] command)
        //{
        //    SearchSettings searchData = new SearchSettings();
        //    searchData.depth = 8;
        //    search.IterativeSearch(currentPosition.Clone(),ref searchData);



        //    int bestMove = transposition.GetPositionData(currentPosition.positionKey).bestMove;
        //    Console.WriteLine("bestmove " + moveController.PrintMove(bestMove));
        //    //positionController.PrintPosition(currentPosition);
        //}

        // receives a previously entered position command with the go command
        internal void Go(string[] position, string[] command)
        {
            ParsePosition(position);

            SearchSettings searchData = new SearchSettings();
            searchData.depth = 6;
            search.IterativeSearch(currentPosition.Clone(), ref searchData);



            int bestMove = transposition.GetPositionData(currentPosition.positionKey).bestMove;
            Console.WriteLine("bestmove " + moveController.PrintMove(bestMove));
        }

        internal void MakeMove(string strMove)
        {
            positionController.MakeMove(ref currentPosition, moveController.ParseMove(currentPosition, strMove));
            currentPosition.ply = 0;

            positionController.PrintPosition(currentPosition);
        }

        // set the current position given the "position" command
        private void ParsePosition(string[] command)
        {
            int i = 0;
            if (command[1] == "startpos")   // set up the starting position
            {
                SetPosition("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
                i = 3;
            }
            else if (command[1] == "fen")
            {
                SetPosition(command[2] + " " + command[3] + " " + command[4] + " " + command[5] + " " + command[6] + " " + command[7]);
                i = 9;
            }

            for (i = i + 0; i < command.Length; i++)
            {
                MakeMove(command[i]);
            }
        }

        #region test
        //run perft test on current position
        public void PerftTest(int plyDepth)
        {
            currentPosition = positionController.ParseFen("3rk1r1/8/8/8/8/p6p/P6P/R3K2R w KQ - 0 1".Split(' '));
            Perft perft = new Perft(bitboards);

            perft.Test(ref currentPosition,plyDepth);
        }
        // a method for testing out whatever may need testing
        public void Test(int plyDepth)
        {
            currentPosition = positionController.ParseFen("r2qk2r/5Npp/2pb1n2/pp6/4P3/P7/1P1NbPPP/R1BQ1RK1 b kq - 0 1".Split(' '));
            //currentPosition = positionController.ParseFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1".Split(' '));
            //currentPosition = positionController.ParseFen("4k3/8/8/8/8/r6r/P6P/R3K2R w KQ - 0 1".Split(' '));
            
            
            //Perft perft = new Perft(bitboards);
            //perft.Test(ref currentPosition, plyDepth);



            SearchSettings searchData = new SearchSettings();
            searchData.depth = plyDepth;
            search.IterativeSearch(currentPosition.Clone(), ref searchData);




        }
        #endregion
    }
}
