using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class MoveGenerator
    {
        // helper class
        MoveController move = new MoveController();

        // Piece movement direction - non bit-board movement
        private static int[] C_KnightDirection = { -8, -19, -21, -12, 8, 19, 21, 12 };
        private static int[] C_RookDirection = { -1, -10, 1, 10 };
        private static int[] C_BishoptDirection = { -9, -11, 11, 9 };
        private static int[] C_KingDirection = { -1, -10, 1, 10, -9, -11, 11, 9 };

        // list of arrays
        // each array represents a single turns worth of piece moves
        private List<int[]> MoveTree = new List<int[]>();
        private List<int[]> ScoreTree = new List<int[]>(); // a score for each move

        // this turn
        private List<int> legalMoves = new List<int>();
        private int[] score = new int[128];
        private int moveCount = 0; // how many moves have been generated so far this move.

        private void AddQuiteMove(int move)
        {
            legalMoves.Add(move);
            score[moveCount] = 0;
            moveCount++;
        }
        private void AddCaptureMove(int move)
        {
            legalMoves.Add(move);
            score[moveCount] = 0;
            moveCount++;
        }
        private void AddEnPassantMove(int move)
        {
            legalMoves.Add(move);
            score[moveCount] = 0;
            moveCount++;
        }

        private void AddWhitePawnCaptureMove(Square from, Square to, Piece captured)
        {
            // if this is a promotion move
            if ((from.ToString())[1] == '7')
            {
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wQ));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wR));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wB));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wN));
            }
            else
            {
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.EMPTY));
            }
        }
        private void AddBlackPawnCaptureMove(Square from, Square to, Piece captured)
        {
            // if this is a promotion move
            if ((from.ToString())[1] == '2')
            {
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bQ));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bR));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bB));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bN));
            }
            else
            {
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.EMPTY));
            }
        }
        private void AddWhitePawnMove(Square from, Square to)
        {
            // if this is a promotion move
            if ((from.ToString())[1] == '7')
            {
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wQ));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wR));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wB));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wN));
            }
            else
            {
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            }
        }
        private void AddBlackPawnMove(Square from, Square to)
        {
            // if this is a promotion move
            if ((from.ToString())[1] == '2')
            {
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bQ));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bR));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bB));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bN));
            }
            else
            {
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            }
        }

        // return an array with all legal moves from the given position
        public int[] GenerateAllMoves(Position position)
        {
            // clear current arrays
            legalMoves = new List<int>();
            score = new int[128];
            moveCount = 0;

            Color color = position.side;

            #region pawn
            // if it is whites turn
            if (color == Color.w)
            {
                // for each pawn
                for (int i = 0; i < position.pieceNumber[(int)Piece.wP]; i++)
                {
                    // what square is this pawn on
                    Square from = position.pieceSquareByType[(int)Piece.wP, i];
                    // is the spot ahead empty?
                    if (position.pieceTypeBySquare[(int)from + 10] == Piece.EMPTY)
                    {
                        AddWhitePawnMove(from, from + 10);
                        // check for double first move
                        if ((from.ToString())[1] == '2' && position.pieceTypeBySquare[(int)from + 20] == Piece.EMPTY)
                        {
                            AddQuiteMove(this.move.ToInt(from, from + 20, Piece.EMPTY, Piece.EMPTY,3));
                        }
                    }

                    // attack squares
                    if (position.pieceTypeBySquare[(int)from + 9] != Piece.EMPTY && position.pieceTypeBySquare[(int)from + 9] != Piece.INVALID)
                    {
                        // is the piece black
                        if ((int)position.pieceTypeBySquare[(int)from + 9] > 6)
                        {
                            AddWhitePawnCaptureMove(from, from + 9, position.pieceTypeBySquare[(int)from + 9]);
                        }
                    }
                    if (position.pieceTypeBySquare[(int)from + 11] != Piece.EMPTY && position.pieceTypeBySquare[(int)from + 11] != Piece.INVALID)
                    {
                        if ((int)position.pieceTypeBySquare[(int)from + 11] > 6)
                        {
                            AddWhitePawnCaptureMove(from, from + 11, position.pieceTypeBySquare[(int)from + 11]);
                        }
                    }

                    if (from + 9 == position.enPas)
                    {
                        AddCaptureMove(this.move.ToInt(from, from + 9, position.pieceTypeBySquare[(int)from + 9], Piece.EMPTY,1));
                    }
                    else if (from + 11 == position.enPas)
                    {
                        AddCaptureMove(this.move.ToInt(from, from + 11, position.pieceTypeBySquare[(int)from + 11], Piece.EMPTY,1));
                    }
                }
            }
            else
            {
                // for each pawn
                for (int i = 0; i < position.pieceNumber[(int)Piece.bP]; i++)
                {
                    // what square is this pawn on
                    Square from = position.pieceSquareByType[(int)Piece.bP, i];
                    // is the spot ahead empty?
                    if (position.pieceTypeBySquare[(int)from - 10] == Piece.EMPTY)
                    {
                        AddBlackPawnMove(from, from - 10);
                        // check for double first move
                        if ((from.ToString())[1] == '7' && position.pieceTypeBySquare[(int)from - 20] == Piece.EMPTY)
                        {
                            AddQuiteMove(this.move.ToInt(from, from - 20, Piece.EMPTY, Piece.EMPTY,3));
                        }
                    }

                    // attack squares
                    if (position.pieceTypeBySquare[(int)from - 9] != Piece.EMPTY && position.pieceTypeBySquare[(int)from - 9] != Piece.INVALID)
                    {
                        // is the piece white
                        if ((int)position.pieceTypeBySquare[(int)from - 9] < 7)
                        {
                            AddBlackPawnCaptureMove(from, from - 9, position.pieceTypeBySquare[(int)from - 9]);
                        }
                    }
                    if (position.pieceTypeBySquare[(int)from - 11] != Piece.EMPTY && position.pieceTypeBySquare[(int)from - 11] != Piece.INVALID)
                    {
                        if ((int)position.pieceTypeBySquare[(int)from - 11] < 7)
                        {
                            AddBlackPawnCaptureMove(from, from - 11, position.pieceTypeBySquare[(int)from - 11]);
                        }
                    }

                    if (from - 9 == position.enPas)
                    {
                        AddCaptureMove(this.move.ToInt(from, from - 9, position.pieceTypeBySquare[(int)from - 9], Piece.EMPTY,1));
                    }
                    else if (from - 11 == position.enPas)
                    {
                        AddCaptureMove(this.move.ToInt(from, from - 11, position.pieceTypeBySquare[(int)from - 11], Piece.EMPTY,1));
                    }
                }
            }
            #endregion

            #region other pieces
            int x = 0;
            if (color == Color.b)
            {
                // this ensures we only look at black pieces in the piece array
                x = 6;
            }

            // each piece type
            for (x = x; x < 12; x++)
            {
                // each piece of this type
                for (int y = 0; y < position.pieceNumber[x + 1]; y++)
                {
                    Square from = position.pieceSquareByType[x+1, y];
                    if (from == Square.INVALID)
                    {
                        break;
                    }

                    #region rook queen
                    // if the type is rook or queen
                    if (x + 1 == 4 || x + 1 == 5 || x + 1 == 10 || x + 1 == 11)
                    {
                        // go in this direction
                        foreach (int i in C_RookDirection)
                        {
                            Square to = from + i;
                            // while we are still on the board
                            while (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
                            {
                                // is the next square empty
                                if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
                                {
                                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                                }
                                else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
                                {
                                    // add possible capture
                                    AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                                to += i; // move
                            }
                        }
                    }
                    #endregion

                    #region bishop queen
                    if (x + 1 == 3 || x + 1 == 5 || x + 1 == 9 || x + 1 == 11)
                    {
                        // go in this direction
                        foreach (int i in C_BishoptDirection)
                        {
                            Square to = from + i;
                            // while we are still on the board
                            while (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
                            {
                                // is the next square empty
                                if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
                                {
                                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                                }
                                else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
                                {
                                    // add possible capture
                                    AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                                to += i; // move
                            }
                        }
                    }
                    #endregion

                    #region king
                    if (x+1 == 6 || x+1 == 12)
                    {
                        #region directional moves
                        // go in this direction
                        foreach (int i in C_KingDirection)
                        {
                            Square to = from + i;
                            // while we are still on the board
                            if (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
                            {
                                if (!IsSquareAttacked(to,1 - color,position)) // is the next square attacked
                                {

                                    // is the next square empty
                                    if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
                                    {
                                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                                    }
                                    else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
                                    {
                                        // add possible capture
                                        AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
                                    }
                                }
                            }
                        }
                        #endregion

                        #region castle move
                        if (position.castlePerm != 0)
                        {
                            if (position.side == Color.w)
                            {
                                // if this castle is still available
                                if ((position.castlePerm & (int)Castle.WKCA) > 0)
                                {
                                    // if the spaces are empty
                                    if (position.pieceTypeBySquare[(int)Square.F1] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.G1] == Piece.EMPTY)
                                    {
                                        // if they are not attacked
                                        if (!IsSquareAttacked(Square.F1,Color.b,position) && !IsSquareAttacked(Square.E1,Color.b,position))
                                        {
                                            AddQuiteMove(move.ToInt(Square.E1, Square.G1, Piece.EMPTY, Piece.EMPTY, 2));
                                        }
                                    }
                                }
                                if ((position.castlePerm & (int)Castle.WQCA) > 0)
                                {
                                    // if the spaces are empty
                                    if (position.pieceTypeBySquare[(int)Square.D1] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.C1] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.B1] == Piece.EMPTY)
                                    {
                                        // if they are not attacked
                                        if (!IsSquareAttacked(Square.D1, Color.b, position) && !IsSquareAttacked(Square.E1, Color.b, position))
                                        {
                                            AddQuiteMove(move.ToInt(Square.E1, Square.C1, Piece.EMPTY, Piece.EMPTY,2));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // if this castle is still available
                                if ((position.castlePerm & (int)Castle.BKCA) > 0)
                                {
                                    // if the spaces are empty
                                    if (position.pieceTypeBySquare[(int)Square.F8] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.G8] == Piece.EMPTY)
                                    {
                                        // if they are not attacked
                                        if (!IsSquareAttacked(Square.F8, Color.b, position) && !IsSquareAttacked(Square.E8, Color.b, position))
                                        {
                                            AddQuiteMove(move.ToInt(Square.E8, Square.G8, Piece.EMPTY, Piece.EMPTY, 2));
                                        }
                                    }
                                }
                                if ((position.castlePerm & (int)Castle.BQCA) > 0)
                                {
                                    // if the spaces are empty
                                    if (position.pieceTypeBySquare[(int)Square.D8] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.C8] == Piece.EMPTY &&
                                        position.pieceTypeBySquare[(int)Square.B8] == Piece.EMPTY)
                                    {
                                        // if they are not attacked
                                        if (!IsSquareAttacked(Square.D8, Color.b, position) && !IsSquareAttacked(Square.E8, Color.b, position))
                                        {
                                            AddQuiteMove(move.ToInt(Square.E8, Square.C8, Piece.EMPTY, Piece.EMPTY, 2));
                                        }
                                    }
                                }
                            }
                        }

                        #endregion
                    }
                    #endregion

                    #region knight
                    if (x + 1 == 2 || x + 1 == 8)
                    {
                        // go in this direction
                        foreach (int i in C_KnightDirection)
                        {
                            Square to = from + i;
                            // while we are still on the board
                            if (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
                            {
                                // is the next square empty
                                if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
                                {
                                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                                }
                                else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
                                {
                                    // add possible capture
                                    AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
                                }
                            }
                        }
                    }
                    #endregion
                }

                if (x == 5 && color == Color.w)
                {   // if all white pieces have been searched end loop
                    x = 12;
                }
            }

            #endregion

            //MoveTree.Add(legalMoves);

            return legalMoves.ToArray();
        }

        public void PrintLegalMoves()
        {
            for (int i = 0; i < moveCount; i++)
            {
                Console.WriteLine(move.PrintMove(legalMoves[i]));
            }
        }
        public bool IsSquareAttacked(Square square, Color attackingSide, Position position)
        {
            if (attackingSide == Color.w)
            {
                // is there a pawn attacking this square
                if (position.pieceTypeBySquare[((int)square) - 11] == Piece.wP ||
                    position.pieceTypeBySquare[((int)square) - 9] == Piece.wP)
                {
                    return true;
                }
            }
            else
            {
                if (position.pieceTypeBySquare[((int)square) + 11] == Piece.bP ||
                    position.pieceTypeBySquare[((int)square) + 9] == Piece.bP)
                {
                    return true;
                }
            }

            // check for a knight in proper directions
            foreach (int direction in C_KnightDirection)
            {
                Piece piece = position.pieceTypeBySquare[((int)square) + direction];
                if (piece == Piece.wN && attackingSide == Color.w)
                {
                    return true;
                }
                else if (piece == Piece.bN && attackingSide == Color.b)
                {
                    return true;
                }
            }

            // check for rook or queen
            foreach (int direction in C_RookDirection)
            {
                Piece piece = position.pieceTypeBySquare[((int)square) + direction];
                int i = direction;
                // is it a valid square?
                while (piece != Piece.INVALID)
                {
                    // is there a piece on this square
                    if (piece != Piece.EMPTY)
                    {
                        // is it the correct piece
                        if ((piece == Piece.wR || piece == Piece.wQ) && attackingSide == Color.w)
                        {
                            return true;
                        }
                        else if ((piece == Piece.bR || piece == Piece.bQ) && attackingSide == Color.b)
                        {
                            return true;
                        }
                        break; // stop searching in this direction because a piece would be blocking this direction
                    }
                    i += direction;
                    piece = position.pieceTypeBySquare[((int)square) + i];
                }
            }

            // bishop or queen
            foreach (int direction in C_BishoptDirection)
            {
                Piece piece = position.pieceTypeBySquare[((int)square) + direction];
                int i = direction;
                // is it a valid square?
                while (piece != Piece.INVALID)
                {
                    // is there a piece on this square
                    if (piece != Piece.EMPTY)
                    {
                        // is it the correct piece
                        if ((piece == Piece.wB || piece == Piece.wQ) && attackingSide == Color.w)
                        {
                            return true;
                        }
                        else if ((piece == Piece.bB || piece == Piece.bQ) && attackingSide == Color.b)
                        {
                            return true;
                        }
                        break; // stop searching in this direction because a piece would be blocking this direction
                    }
                    i += direction;
                    piece = position.pieceTypeBySquare[((int)square) + i];
                }
            }

            // king
            foreach (int direction in C_KingDirection)
            {
                Piece piece = position.pieceTypeBySquare[((int)square) + direction];

                if (piece == Piece.wK && attackingSide == Color.w)
                {
                    return true;
                }
                else if (piece == Piece.bK && attackingSide == Color.b)
                {
                    return true;
                }

            }

            return false;
        }

        //public void PrintAttack(Position position)
        //{
        //    Console.WriteLine("");
        //    for (int i = 63; i >= 0; i--)
        //    {
        //        if (IsSquareAttacked((Square)Square64To120[i], Color.w, position))
        //        {
        //            Console.Write(" X");
        //        }
        //        else
        //        {
        //            Console.Write(" O");
        //        }
        //        if (((i) % 8 == 0) && (i != 1 && i != 0 && i != 63))
        //        {
        //            Console.WriteLine("");
        //            Console.WriteLine("");
        //        }
        //    }
        //    Console.WriteLine("");
        //}


    }
}