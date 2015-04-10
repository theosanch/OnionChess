﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class MoveController
    {
        // movement related tasks
        // A single move is represented by an int
        // from square, to square, and any other needed data is bitwise or into the int

        private int enPassant = 0x40000, pawnStart = 0x80000, castle = 0x1000000;

        #region move to int
        // a movement is stored in a single int
        public int ToInt(Square from, Square to, Piece captured, Piece promoted)
        {
            return ((int)from) | ((int)to << 7) | ((int)captured << 14) | ((int)promoted << 20);
        }

        // 1 = en passant capture 2 = castle move 3 = pawn start move
        public int ToInt(Square from, Square to, Piece captured, Piece promoted, int n)
        {
            if (n == 1)
            {
                return (ToInt(from, to, captured, promoted)) | (this.enPassant);
            }
            else if (n == 2)
            {
                return (ToInt(from, to, captured, promoted)) | (this.castle);
            }
            else
            {
                return (ToInt(from, to, captured, promoted)) | (this.pawnStart);
            }
        }
        #endregion

        #region int to info
        // get data from a specified move number
        //FROM
        public Square GetFromSquare(int move)
        {
            return (Square)(move & 0x7F);
        }

        // TO
        public Square GetToSquare(int move)
        {
            return (Square)((move >> 7) & 0x7F);
        }

        // CAPTURED
        public Piece GetCapturedPiece(int move)
        {
            return (Piece)((move >> 14) & 0xF);
        }

        // PROMOTED PIECE
        public Piece GetPromotedPiece(int move)
        {
            return (Piece)((move >> 20) & 0xF);
        }

        // was it an en passant capture?
        public bool GetEnPassant(int move)
        {
            return (move & enPassant) > 0;
        }
        // was it a castle move
        public bool GetCastle(int move)
        {
            return (move & castle) > 0;
        }
        // was it a pawn starting position move
        public bool GetPawnStartMove(int move)
        {
            return (move & pawnStart) > 0;
        }
        #endregion

        // parse a standard notation move into its int representation
        public int ParseMove(Position position,string strMove)
        {
            Square from = (Square) Enum.Parse( typeof(Square),(strMove[0].ToString() + strMove[1].ToString()),true);
            Square to = (Square)Enum.Parse(typeof(Square), strMove[2].ToString() + strMove[3].ToString(), true);

            Piece promotion = Piece.EMPTY;
            // is there a promotion piece
            #region promotion
            if (strMove.Length > 4)
            {
                switch (strMove[5])
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

            #region pawn start
            // if it was a pawn initial move
            if (position.pieceTypeBySquare[(int)from] == Piece.wP)
            {
                if ((int)from > 30 && (int)from < 38)
                {
                    return ToInt(from, to, position.pieceTypeBySquare[(int)to], promotion, 3);
                }
            }
            else if (position.pieceTypeBySquare[(int)from] == Piece.bP)
            {
                if ((int)from > 80 && (int)from < 88)
                {
                    return ToInt(from, to, position.pieceTypeBySquare[(int)to], promotion, 3);
                }
            }
            #endregion

            if (to == position.enPas)
            {
                return ToInt(from, to, position.pieceTypeBySquare[(int)to], promotion,1);
            }

            return ToInt(from,to,position.pieceTypeBySquare[(int)to],promotion);
        }
        public string PrintMove(int move)
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
    }
}
