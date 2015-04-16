using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Position
    {
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
        // what piece type is on each square
        public Piece[] pieceTypeBySquare = new Piece[64];
        // track location of each piece in a single list. 
        // 13: each piece type including empty.
        // 10: up to ten pieces for that type.
        public Square[,] pieceSquareByType = new Square[12, 10]; // needs to be initialized inside reset board method

        // how many of each piece type?
        public int[] pieceNumber = new int[12];

        // location and attacks of each piece type
        public ulong[] locations = new ulong[12];
        public ulong[] attacks = new ulong[12];

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
        public Position(Position position)
        {

        }


        // when searching through different moves a copy of the previous move is needed
        public Position Clone()
        {
            // this copies all value types
            Position results = (Position)this.MemberwiseClone(); // "this" used just for clarity

            results.pieceTypeBySquare = new Piece[64];
            results.pieceSquareByType = new Square[12, 10];
            results.pieceNumber = new int[12];

            results.materialScore = new int[2];

            results.locations = new ulong[12];
            results.attacks = new ulong[12];

            // ref type copying - arrays are ref type
            for (int i = 0; i < 64; i++)
            {
                results.pieceTypeBySquare[i] = this.pieceTypeBySquare[i];

                if (i < 12)
                {
                    results.pieceNumber[i] = this.pieceNumber[i];

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
                    results.attacks[i] = this.attacks[i];

                }
            }


            return results;
        }
    }
}
