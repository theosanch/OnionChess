
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
        public Color side = Color.both;
        // what square is currently available for en passant
        public Square enPassantSquare = Square.INVALID;

        // what the castle status is. A calculation is done using the Castle enumerator.
        // a four bit number is used to store what the castle status is
        public int castleStatus = 0;

        // fifty move rule counter
        public int fiftyMoveCounter;
        // what ply is this move at
        public int ply;
        #endregion

        #region piece information

        #region bitboards
        
        // each location represents a piece type for that color
        // 0 = white pawn, 1 = 
        public ulong[] locations = new ulong[12];

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

        #endregion

        public Position()
        {

        }

        // given a square return what type of piece is on that square
        public Piece GetPieceTypeBySquare(Square square)
        {
            // a bitboard with only the square selected
            ulong bitboardSquare = BitBoard.SquareToBit((int)square);

            // check each piece type
            for (int i = 0; i < 12; i++)
            {
                // if there is a piece in that square
                if((locations[i] & bitboardSquare) != 0)
                {
                    // return that piece type
                    return (Piece)(i + 1);
                }
            }

            // nothing found, return empty
            return Piece.EMPTY;
        }
        // set the piece on the given square - don't set to empty
        public void SetPiece(Square square, Piece piece)
        {
            locations[((int)piece) - 1] = BitBoard.AddBit(locations[((int)piece) - 1], (int)square);
        }

        // remove piece 
        public void RemovePiece(Square square, Piece piece)
        {
            locations[(int)piece - 1] = BitBoard.RemoveBit(locations[(int)piece - 1], (int)square);
        }


        // when searching through different moves a copy of the previous move is needed
        public Position Clone()
        {
            // this copies all value types
            Position results = (Position)this.MemberwiseClone(); // "this" used just for clarity          

            // ref type copying - arrays are ref type
            for (int i = 0; i < 12; i++)
            {
                results.locations[i] = this.locations[i];                
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
                    Piece piece = GetPieceTypeBySquare(square);

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
            result += (string.Format("Side: {0} En Passant: {1} Castle: ", side, enPassantSquare));

            // castle permission print
            // bit and operation
            if ((castleStatus & (int)Castle.WKCA) != 0)
            {
                result += ("K");
            }
            if ((castleStatus & (int)Castle.WQCA) != 0)
            {
                result += ("Q");
            }
            if ((castleStatus & (int)Castle.BKCA) != 0)
            {
                result += ("k");
            }
            if ((castleStatus & (int)Castle.BQCA) != 0)
            {
                result += ("q");
            }

            result += "\n\n";

            return result;

        }
    }
}
