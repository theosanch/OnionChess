
namespace OnionEngine
{
    class Position
    {
        // TODO 
        // properties that calculate pieceTypeBySquare and pieceSquareByType


        // light weight class to handle a single position
        // a struct might have better performance.

        #region meta information
        // the unique key for this position
        public ulong positionKey;
        // whose turn it is to move.
        public Color side = Color.Both;
        // what square is currently available for en passant
        public Square enPassant = Square.INVALID;
        // what the castle status is. A calculation is done using the Castle enumerator.
        public int castlePerm = 0;

        // fifty move rule counter
        public int fiftyMoveCounter;
        // what ply is this move at
        public int ply;
        #endregion

        #region piece information

        #region piece lists
        // what piece type is on each square
        public Piece[] pieceTypeBySquare = new Piece[64];
        // track location of each piece in a single list. 
        // 12: each piece type.
        // 10: up to ten pieces for that type.
        public Square[,] pieceSquareByType = new Square[12, 10]; // needs to be initialized inside reset board method

        // how many of each piece type?
        public int[] pieceCount = new int[12]; // this list helps with interacting with the pieceSquareByType array
        #endregion

        #region bitboards
        public ulong[] locations = new ulong[12];

        // pseudo legal captures and attacks for each piece type including all for each color [13-14]
        public ulong[] captures = new ulong[14];    // both are generated during move generation
        public ulong[] attacks = new ulong[14];
        #endregion

        public ulong WhitePosition
        {
            get
            {
                return locations[0] | locations[1] | locations[2] | locations[3] | locations[4] | locations[5];
            }
        }
        public ulong BlackPosition
        {
            get
            {
                return locations[6] | locations[7] | locations[8] | locations[9] | locations[10] | locations[11];
            }
        }

        public int[] materialScore = new int[2];

        #endregion

        public Position()
        {

        }


        // when searching through different moves a copy of the previous move is needed
        public Position Clone()
        {
            // this copies all value types
            Position results = (Position)this.MemberwiseClone(); // "this" used just for clarity

            results.pieceTypeBySquare = new Piece[64];
            results.pieceSquareByType = new Square[12, 10];
            results.pieceCount = new int[12];

            results.materialScore = new int[2];

            results.locations = new ulong[12];
            results.captures = new ulong[14];
            results.attacks = new ulong[14];

            // ref type copying - arrays are ref type
            for (int i = 0; i < 64; i++)
            {
                results.pieceTypeBySquare[i] = this.pieceTypeBySquare[i];
                if (i < 14)
                {
                    results.captures[i] = this.captures[i];
                    results.attacks[i] = this.attacks[i];
                }

                if (i < 12)
                {
                    results.pieceCount[i] = this.pieceCount[i];

                    if (i < 2)
                    {
                        results.materialScore[i] = this.materialScore[i];
                    }

                    // [13,10] array
                    for (int n = 0; n < 10; n++)
                    {
                        results.pieceSquareByType[i, n] = this.pieceSquareByType[i, n];
                    }



                    results.locations[i] = this.locations[i];

                }
            }


            return results;
        }

        override public string ToString()
        {
            string result = "";
            result = "Position: " + positionKey.ToString("X");
            result += "\n";

            for (Rank rank = Rank.Rank_8; rank >= Rank.Rank_1; rank--)
            {
                result += (rank.ToString() + " ");

                for (File file = File.File_A; file <= File.File_H; file++)
                {
                    Square square = Move.FileRanktoSquare(file, rank);
                    Piece piece = pieceTypeBySquare[(int)square];

                    if (piece == Piece.EMPTY)
                    {
                        result += ("  . ");
                    }
                    else
                    {
                        result += (" " + piece.ToString() + " ");
                    }
                }
                result += "\n\n";
            }

            result += ("         A   B   C   D   E   F   G   H");
            result += "\n";
            result += (string.Format("Side: {0} En Passant: {1} Castle: ", side, enPassant));

            // castle permission print
            // bit and operation
            if ((castlePerm & (int)Castle.WKCA) != 0)
            {
                result += ("K");
            }
            if ((castlePerm & (int)Castle.WQCA) != 0)
            {
                result += ("Q");
            }
            if ((castlePerm & (int)Castle.BKCA) != 0)
            {
                result += ("k");
            }
            if ((castlePerm & (int)Castle.BQCA) != 0)
            {
                result += ("q");
            }

            result += "\n\n";

            return result;

        }
    }
}
