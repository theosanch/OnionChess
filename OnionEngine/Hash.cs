using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    // read here for the idea behind a hashtable in chess (http://www.netlib.org/utk/lsi/pcwLSI/text/node346.html)
    //
    // 
    class Hash
    {
        Random rng = new Random();

        // unique key for each piece [type,position]
        private ulong[,] pieceKeys = new ulong[13, 64];
        // key for white or black?
        private ulong sideKey;
        // a key for each castle permutation?
        private ulong[] castleKeys = new ulong[16];

        

        public Hash()
        {
            InitHashKeys();
        }



        #region hash keys
        private void InitHashKeys()
        {
            for (int i = 0; i < 13; i++)
            {
                for (int n = 0; n < 64; n++)
                {
                    pieceKeys[i, n] = Random64bit();
                }
            }
            sideKey = Random64bit();

            for (int i = 0; i < 16; i++)
            {
                castleKeys[i] = Random64bit();
            }
        }
        // return a random 64bit number
        // used for hash key generation
        private ulong Random64bit()
        {
            var buffer = new byte[sizeof(Int64)];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        // generate unique key for a given position
        public ulong GeneratePositionKey(Position position)
        {
            ulong finalKey = 0;
            Piece piece = Piece.EMPTY;

            // check each square
            for (int squareNumber = 0; squareNumber < 64; squareNumber++)
            {
                piece = position.GetPieceTypeBySquare((Square)squareNumber);
                if (piece != Piece.EMPTY && piece != Piece.INVALID)
                {
                    // hash key if it is a valid square
                    finalKey ^= pieceKeys[(int)piece, squareNumber];
                }
            }

            // if it is whites turn
            if (position.side == Color.white)
            {
                finalKey ^= sideKey;
            }

            // if there is en passant available
            if (position.enPassantSquare != Square.INVALID)
            {
                finalKey ^= pieceKeys[(int)Piece.EMPTY, (int)position.enPassantSquare];
            }

            // castle status
            finalKey ^= castleKeys[position.castleStatus];

            return finalKey;
        }

        public ulong HashPiece(Position position, Piece piece, Square square)
        {
            return position.positionKey ^= (pieceKeys[(int)piece, (int)square]);
        }
        public ulong HashCastle(Position position)
        {
            return position.positionKey ^= castleKeys[position.castleStatus];
        }
        public ulong HashSide(Position position)
        {
            return position.positionKey ^= sideKey;
        }
        public ulong HashEnPassant(Position position)
        {
            return position.positionKey ^= pieceKeys[(int)Piece.EMPTY, (int)position.enPassantSquare];
        }
        #endregion


    }
}
