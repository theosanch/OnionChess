using System;

namespace OnionEngine
{
    static class Move
    {
        // Movement related tasks

        // A single move is represented by an int
        // 
        // The int represents the following information
        // from square, to square, captured piece, promoted piece, and 
        // en passant or promoted piece or pawn double move
        // the data is bitwise or-ed into and out of the int

        private static int enPassant = 0x40000, doubleMove = 0x80000, castle = 0x1000000;

        #region move to int
        // a movement is stored in a single int
        public static int ToInt(Square from, Square to, Piece captured, Piece promoted)
        {
            return ((int)from) | ((int)to << 7) | ((int)captured << 14) | ((int)promoted << 20);
        }

        // 1 = en passant capture 2 = castle move 3 = double move
        public static int ToInt(Square from, Square to, Piece captured, Piece promoted, int n)
        {
            if (n == 1)
            {
                return (ToInt(from, to, captured, promoted)) | (enPassant);
            }
            else if (n == 2)
            {
                return (ToInt(from, to, captured, promoted)) | (castle);
            }
            else
            {
                return (ToInt(from, to, captured, promoted)) | (doubleMove);
            }
        }
        #endregion

        #region int to info
        // get data from a specified move number
        //FROM
        public static Square GetFromSquare(int move)
        {
            return (Square)(move & 0x7F);
        }

        // TO
        public static Square GetToSquare(int move)
        {
            return (Square)((move >> 7) & 0x7F);
        }

        // CAPTURED
        public static Piece GetCapturedPiece(int move)
        {
            return (Piece)((move >> 14) & 0xF);
        }

        // PROMOTED PIECE
        public static Piece GetPromotedPiece(int move)
        {
            return (Piece)((move >> 20) & 0xF);
        }

        // was it an en passant capture?
        public static bool GetEnPassantCapture(int move)
        {
            return (move & enPassant) > 0;
        }
        // was it a castle move
        public static bool GetCastle(int move)
        {
            return (move & castle) > 0;
        }
        // was it a pawn starting position move
        public static bool GetPawnDoubleMove(int move)
        {
            return (move & doubleMove) > 0;
        }
        #endregion

        // parse a standard notation move into its int representation
        public static int ParseMove(Position position,string strMove)
        {
            Square from = (Square) Enum.Parse( typeof(Square),(strMove[0].ToString() + strMove[1].ToString()),true);
            Square to = (Square)Enum.Parse(typeof(Square), strMove[2].ToString() + strMove[3].ToString(), true);

            #region pawn start
            // if it was a pawn double move
            if (position.pieceTypeBySquare[(int)from] == Piece.wP)
            {
                if (((int)from >> 3) == 1 && ((int)to >> 3) == 3)
                {
                    return ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 3);
                }
            }
            else if (position.pieceTypeBySquare[(int)from] == Piece.bP)
            {
                if (((int)from >> 3) == 6 && ((int)to >> 3) == 4)
                {
                    return ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 3);
                }
            }
            #endregion

            if (to == position.enPassant)
            {
                return ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY,1);
            }

            // is there a promotion piece
            Piece promotion = Piece.EMPTY;
            #region promotion
            if (strMove.Length > 4)
            {
                switch (strMove[4])
                {
                    case 'q':
                        if (position.side == Color.w)
                        {
                            promotion = Piece.wQ;
                        }
                        else
                        {
                            promotion = Piece.bQ;
                        }
                        break;
                    case 'n':
                        if (position.side == Color.w)
                        {
                            promotion = Piece.wN;
                        }
                        else
                        {
                            promotion = Piece.bN;
                        }
                        break;
                    case 'r':
                        if (position.side == Color.w)
                        {
                            promotion = Piece.wR;
                        }
                        else
                        {
                            promotion = Piece.bR;
                        }
                        break;
                    case 'b':
                        if (position.side == Color.w)
                        {
                            promotion = Piece.wB;
                        }
                        else
                        {
                            promotion = Piece.bB;
                        }
                        break;
                    default:
                        break;
                }
            }
            #endregion

            return ToInt(from,to,position.pieceTypeBySquare[(int)to],promotion);
        }

        // get a string of the move in standard notation
        public static string ToString(int move)
        {
            string results = "";

            Square from = GetFromSquare(move);
            Square to = GetToSquare(move);

            results = (GetFromSquare(move).ToString() + GetToSquare(move).ToString()).ToLower();

            Piece promoted = GetPromotedPiece(move);
            if (promoted != Piece.EMPTY)
            {
                results += ((promoted.ToString())[1]).ToString().ToLower();
            }
            

            return results;
        }

        // not related to the move type, but no better place to put yet
        // convert file and rank to square
        public static Square FileRanktoSquare(File file, Rank rank)
        {
            return (Square)((int)file) + (8 * (int)rank);
        }
    }
}
