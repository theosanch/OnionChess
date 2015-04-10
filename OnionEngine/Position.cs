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
        public Square enPas = Square.INVALID;
        // what the castle status is. A calculation is done using the Castle enumerator.
        public int castlePerm = 0;

        // fifty move rule counter
        public int fiftyMoveCounter;
        public int ply, hisPly;

        
        #endregion

        #region piece information
        // piece location

        // what piece type is on each square
        public Piece[] pieceTypeBySquare = new Piece[120];
        // track location of each piece in a single list. 
        // 13: each piece type including empty.
        // 10: up to ten pieces for that type.
        public Square[,] pieceSquareByType = new Square[13, 10]; // needs to be initialized inside reset board method

        // how many of each piece type?
        public int[] pieceNumber = new int[13];

        // bit-board location
        public ulong[] pawnBitBoard = new ulong[3];
        

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
            Position results = (Position) this.MemberwiseClone(); // "this" used just for clarity

            results.pieceTypeBySquare = new Piece[120];
            results.pieceNumber = new int[13];
            results.pawnBitBoard = new ulong[3];
            results.materialScore = new int[2];
            results.pieceSquareByType = new Square[13,10];

            // ref type copying - arrays are ref type
            for (int i = 0; i < 120; i++)
            {
                results.pieceTypeBySquare[i] = this.pieceTypeBySquare[i];

                if (i<13)
                {
                    results.pieceNumber[i] = this.pieceNumber[i];

                    if (i < 3)
                    {
                        results.pawnBitBoard[i] = this.pawnBitBoard[i];

                        if (i<2)
                        {
                            results.materialScore[i] = this.materialScore[i];
                        }
                    }

                    // [13,10] array
                    for (int n = 0; n < 10; n++)
                    {
                        results.pieceSquareByType[i,n] = this.pieceSquareByType[i,n];
                    }
                }
            }


            return results;
        }
    }
}
