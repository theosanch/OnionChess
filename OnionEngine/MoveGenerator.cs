using System;
using System.Collections.Generic;

namespace OnionEngine
{
    class MoveGenerator
    {
        // MoveGenerator has the a main task of generating all legal moves for a given position

        // helper class
        MoveController move = new MoveController();
        BitBoards bitboardController;

        // this turn
        private List<int> legalMoves = new List<int>();
        private int[] score = new int[128];
        private int moveCount = 0; // how many moves have been generated so far this move.

        public MoveGenerator(BitBoards bitboard)
        {
            bitboardController = bitboard;
        }

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
        private void AddDoubleMove(int move)
        {
            legalMoves.Add(move);
            score[moveCount] = 0;
            moveCount++;
        }

        private void AddPawnMove(Square from, Square to, int side)
        {
            int[] rank = { 7, 0 };
            // is it a promotion move
            if ((int)to >> 3 == rank[side])
            {
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wQ));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wR));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wB));
                AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wN));
                return;
            }
            AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
        }
        private void AddPawnCapture(Square from, Square to, Piece captured, int side)
        {
            int[] rank = { 7, 0 };
            // is it a promotion move
            if ((int)to >> 3 == rank[side])
            {
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wQ));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wR));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wB));
                AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wN));
                return;
            }

            AddCaptureMove(move.ToInt(from, to, captured, Piece.EMPTY));
        }

        private ulong CircularLeftShift(ulong bitboard, int shift)
        {
            return (bitboard << shift) | (bitboard >> 64 - shift);
        }

        /// <summary>
        /// Generates all legal moves given a position
        /// </summary>
        /// <param name="position"></param>
        /// <returns>Each int in the array represents a from square and to square</returns>
        public int[] GenerateAllMoves(Position position)
        {
            #region variables
            // clear current arrays
            legalMoves = new List<int>();
            score = new int[128];
            moveCount = 0;

            ulong pawnMoves = 0UL;
            ulong pawnDoubleMoves = 0UL;
            ulong pawnLeftCaptures = 0UL;
            ulong pawnRightCaptures = 0UL;

            ulong bishopMoves = 0UL;
            ulong bishopCaptures = 0UL;

            ulong knightMoves = 0UL;
            ulong knightCaptures = 0UL;

            ulong rookMoves = 0UL;
            ulong rookCaptures = 0UL;

            ulong queenMoves = 0UL;
            ulong queenCaptures = 0UL;

            ulong kingMoves = 0UL;
            ulong kingCaptures = 0UL;

            // king in check not validated

            // a helper to eliminate if statements
            // the first 6 values help with bit shifts

            // 8 and 9 help with capture shift wrapping
            int[] shiftValues = { 8, 64 - 8, 7, 64 - 9, 9, 64 - 7,
                                  2, 5,         // 6 and 7 help with double move rank detection
                                  7, 0,         // A file or H file for side attacks
                                  -8,8,-16,16,  // move direction
                                   7,-9,9,-7};  // attack direction

            // will equal 6(black) or 0(white). 
            // this eliminates the need of having to use if statements because all black pieces are +6 in their respective arrays
            int color = (int)position.side * 6;
            ulong friendlyLocations = (position.locations[color] | position.locations[1 + color] | position.locations[2 + color] | position.locations[3 + color] | position.locations[4 + color] | position.locations[5 + color]);
            ulong enemyLocations = (position.locations[6 - color] | position.locations[7 - color] | position.locations[8 - color] | position.locations[9 - color] | position.locations[10 - color] | position.locations[11 - color]);
            #endregion

            #region generate moves
            // generate all valid pawn moves
            #region pawn moves
            // move forward one         move forward one square         if the position is empty

            pawnMoves = CircularLeftShift(position.locations[color], shiftValues[(int)position.side]) & ~(friendlyLocations | enemyLocations);
            // double move              pieces on the third rank        remove if occupied
            pawnDoubleMoves |= CircularLeftShift(pawnMoves & bitboardController.ranks[shiftValues[(int)position.side + 6]], shiftValues[(int)position.side]) & ~(friendlyLocations | enemyLocations);
            // pawn captures                                        remove proper edge pawns and add en passant

            pawnLeftCaptures = CircularLeftShift(position.locations[color] & ~bitboardController.files[0], shiftValues[(int)position.side + 2]);
            pawnRightCaptures = CircularLeftShift(position.locations[color] & ~bitboardController.files[7], shiftValues[(int)position.side + 4]);

            // attacks tracking
            position.attacks[0 + color] = (pawnLeftCaptures | pawnRightCaptures) & ~(friendlyLocations | enemyLocations);
            position.attacks[12 + (int)position.side] |= (pawnLeftCaptures | pawnRightCaptures) & ~(friendlyLocations | enemyLocations);

            pawnLeftCaptures &= (enemyLocations | bitboardController.SquareToBit((int)position.enPassant));
            pawnRightCaptures &= (enemyLocations | bitboardController.SquareToBit((int)position.enPassant));

            // captures tracking
            position.captures[color] = pawnLeftCaptures | pawnRightCaptures;
            position.captures[12 + (int)position.side] |= pawnLeftCaptures | pawnRightCaptures;

            #region add moves

            while (pawnMoves != 0)
            {
                Square to = (Square)bitboardController.PopBit(ref pawnMoves);
                Square from = to + shiftValues[10 + (int)position.side];

                AddPawnMove(from, to, (int)position.side);
            }
            while (pawnDoubleMoves != 0)
            {
                Square to = (Square)bitboardController.PopBit(ref pawnDoubleMoves);
                Square from = to + shiftValues[12 + (int)position.side];

                AddDoubleMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY, 3));
            }
            while (pawnLeftCaptures != 0)
            {
                Square to = (Square)bitboardController.PopBit(ref pawnLeftCaptures);
                Square from = to - (shiftValues[(int)position.side + 14]);

                if (to == position.enPassant)
                {
                    AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 1));
                    continue;
                }

                AddPawnCapture(from, to, position.pieceTypeBySquare[(int)to], (int)position.side);
            }
            while (pawnRightCaptures != 0)
            {
                Square to = (Square)bitboardController.PopBit(ref pawnRightCaptures);
                Square from = to - (shiftValues[(int)position.side + 16]);

                if (to == position.enPassant)
                {
                    AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 1));
                    continue;
                }

                AddPawnCapture(from, to, position.pieceTypeBySquare[(int)to], (int)position.side);
            }

            #endregion
            #endregion

            // generate all slide moves
            #region rook bishop queen
            ulong bishops = position.locations[2 + color];
            ulong rooks = position.locations[3 + color];
            ulong queens = position.locations[4 + color];
            #region bishop
            while (bishops > 0)
            {
                Square from = (Square)bitboardController.PopBit(ref bishops);
                // moves
                bishopMoves = bitboardController.bishopMoves[(int)from];
                // captures
                bishopCaptures = bishopMoves & enemyLocations;
                // remove captures from moves
                bishopMoves = bishopMoves ^ (bishopCaptures);
                // remove friendly
                bishopMoves = bishopMoves & ~friendlyLocations;


                // add moves
                while (bishopMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref bishopMoves);
                    // if no piece is between the move
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[2 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (bishopCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref bishopCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    // should only intersect with the captured piece
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == bitboardController.SquareToBit((int)to))
                    {
                        position.captures[2 + color] |= bitboardController.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
            #endregion
            #region rook
            while (rooks > 0)
            {
                Square from = (Square)bitboardController.PopBit(ref rooks);

                rookMoves = bitboardController.rookMoves[(int)from];
                // attacks
                rookCaptures = rookMoves & enemyLocations;
                rookMoves = rookMoves ^ (rookCaptures);

                rookMoves = rookMoves & ~friendlyLocations;


                // add moves
                while (rookMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref rookMoves);
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[3 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (rookCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref rookCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == bitboardController.SquareToBit((int)to))
                    {
                        position.captures[3 + color] |= bitboardController.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
            #endregion
            #region queen
            while (queens > 0)
            {
                Square from = (Square)bitboardController.PopBit(ref queens);

                queenMoves = bitboardController.rookMoves[(int)from] | bitboardController.bishopMoves[(int)from];
                //attacks
                queenCaptures = queenMoves & enemyLocations;
                queenMoves = queenMoves ^ (queenCaptures);

                queenMoves = queenMoves & ~friendlyLocations;



                // add moves
                while (queenMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref queenMoves);
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[4 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (queenCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref queenCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == bitboardController.SquareToBit((int)to))
                    {
                        position.captures[4 + color] |= bitboardController.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
            #endregion
            #endregion

            // king and knight
            #region king and knight
            ulong kings = position.locations[5 + color];
            ulong knights = position.locations[1 + color];

            while (kings > 0)
            {
                Square from = (Square)bitboardController.PopBit(ref kings);

                kingMoves = bitboardController.kingMoves[(int)from];
                // captures
                kingCaptures = kingMoves & enemyLocations;
                // remove captures 
                kingMoves = kingMoves ^ (kingCaptures);
                // remove friendly
                kingMoves = kingMoves & ~friendlyLocations;

                #region castle logic
                // castle move
                if (position.castlePerm != 0)
                {
                    int[] colorKey = { 1,2,4,8,     // castle values
                                       4,60,6,62,   // from and to squares for black and white
                                        2,58};
                    // king side
                    // if this castle is still available
                    if ((position.castlePerm & colorKey[(int)position.side]) > 0)
                    {
                        ulong squares = bitboardController.intersectLines[colorKey[(int)position.side + 4], colorKey[(int)position.side + 6]];
                        if ((squares & (enemyLocations | friendlyLocations)) == 0)
                        {
                            // is it attacked
                            int n = 0;
                            while (squares != 0)
                            {
                                int check = bitboardController.PopBit(ref squares);
                                if (!IsSquareAttacked(position, bitboardController.SquareToBit(check), 1 - position.side))
                                {
                                    n++;

                                    if (n > 1)
                                    {
                                        Square to = (Square)(colorKey[(int)position.side + 6]);
                                        AddQuiteMove(move.ToInt(from, (Square)colorKey[(int)position.side + 6], Piece.EMPTY, Piece.EMPTY, 2));
                                    }

                                }
                            }

                            //if ((squares & position.attacks[12 + (1 - (int)position.side)]) == 0)
                            //{
                            //    Square to = (Square)(colorKey[(int)position.side + 6]);
                            //    AddQuiteMove(move.ToInt(from, (Square)colorKey[(int)position.side + 6], Piece.EMPTY, Piece.EMPTY, 2));

                            //}
                        }
                    }
                    // queen side
                    if ((position.castlePerm & colorKey[(int)position.side + 2]) > 0)
                    {
                        ulong squares = bitboardController.intersectLines[colorKey[(int)position.side + 4], colorKey[(int)position.side + 8]];
                        // is the space empty
                        if ((squares & (enemyLocations | friendlyLocations)) == 0)
                        {
                            // is it attacked
                            int n = 0;
                            while (squares != 0)
                            {
                                int check = bitboardController.PopBit(ref squares);
                                if (!IsSquareAttacked(position, bitboardController.SquareToBit(check), 1 - position.side))
                                {
                                    n++;
                                    if (n > 1)
                                    {
                                        Square to = (Square)(colorKey[(int)position.side + 8]);
                                        AddQuiteMove(move.ToInt(from, (Square)(colorKey[(int)position.side + 8]), Piece.EMPTY, Piece.EMPTY, 2));
                                    }
                                }
                            }
                            //if ((squares & position.attacks[12 + (1 - (int)position.side)]) == 0)
                            //{
                            //    Square to = (Square)(colorKey[(int)position.side + 8] + 1);
                            //    AddQuiteMove(move.ToInt(from, (Square)(colorKey[(int)position.side + 8] + 1), Piece.EMPTY, Piece.EMPTY, 2));
                            //}
                        }
                    }
                }
                #endregion

                // add moves
                while (kingMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref kingMoves);

                    position.attacks[5 + color] |= bitboardController.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (kingCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref kingCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.captures[5 + color] |= bitboardController.SquareToBit((int)to);
                    position.captures[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                }
            }
            #region knight
            while (knights > 0)
            {
                Square from = (Square)bitboardController.PopBit(ref knights);

                knightMoves = bitboardController.knightMoves[(int)from];
                // captures
                knightCaptures = knightMoves & enemyLocations;
                // remove captures 
                knightMoves = knightMoves ^ knightCaptures;
                // remove friendly
                knightMoves = knightMoves & ~friendlyLocations;



                // add moves
                while (knightMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref knightMoves);

                    position.attacks[1 + color] |= bitboardController.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (knightCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref knightCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.captures[1 + color] |= bitboardController.SquareToBit((int)to);
                    position.captures[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                }
            }
            #endregion
            #endregion
            #endregion


            return legalMoves.ToArray();
        }

        public void PrintLegalMoves()
        {
            for (int i = 0; i < moveCount; i++)
            {
                Console.WriteLine(move.PrintMove(legalMoves[i]));
            }
        }
        public bool IsSquareAttacked(Position position, ulong bbSquare, Color attackingSide)
        {
            int color = (int)attackingSide * 6;
            //Square pieceSquare = position.pieceSquareByType[5 + ((1 - (int)attackingSide) * 6),0];
            Square attacked = (Square)bitboardController.PopBit(ref bbSquare);

            // pawns
            if ((position.locations[0 + color] & bitboardController.pawnAttacks[1 - (int)attackingSide, (int)attacked]) != 0)
            {
                return true;
            }
            // queen
            ulong queen = (bitboardController.rookMoves[(int)attacked] | bitboardController.bishopMoves[(int)attacked]) & position.locations[4 + color];
            while (queen != 0)
            {
                Square square = (Square)bitboardController.PopBit(ref queen);
                if ((bitboardController.intersectLines[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == bitboardController.SquareToBit((int)square))
                {
                    return true;
                }
            }
            // bishop
            ulong bishop = (bitboardController.bishopMoves[(int)attacked] & position.locations[2 + color]);
            while (bishop != 0)
            {
                Square square = (Square)bitboardController.PopBit(ref bishop);
                if ((bitboardController.intersectLines[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == bitboardController.SquareToBit((int)square))
                {
                    return true;
                }
            }
            // rook
            ulong rook = (bitboardController.rookMoves[(int)attacked] & position.locations[3 + color]);
            while (rook != 0)
            {
                Square square = (Square)bitboardController.PopBit(ref rook);
                if ((bitboardController.intersectLines[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == bitboardController.SquareToBit((int)square))
                {
                    return true;
                }
            }

            // knight
            ulong knight = (bitboardController.knightMoves[(int)attacked] & position.locations[1 + color]);
            if (knight != 0)
            {
                return true;
            }

            //king
            ulong king = (bitboardController.kingMoves[(int)attacked] & position.locations[5 + color]);
            if (king != 0)
            {
                return true;
            }

            return false;
        }

        // convert file and rank to square
        public Square FileRanktoSquare(File file, Rank rank)
        {
            return (Square)((int)file) + (8 * (int)rank);
        }
    }
}