using System;
using System.Collections.Generic;

namespace OnionEngine
{
    class MoveGenerator
    {
        // TODO
        // order moves by move to square. Pieces that move forward should be checked first
        // bit board of all move to square generated to somehow determine if last times generated moves can be reused
        // bug: will castle while in check [fixed]

        // bug
        // position fen 8/PPPk4/8/8/8/8/4Kppp/8 b - - 0 1 moves f2f1q e2e3
        // perft 2
        // king attack check before piece promoted?

        // MoveGenerator has the main task of generating all legal moves for a given position

        // this turn
        private List<int> legalMoves = new List<int>();
        private int[] score = new int[128];
        private int moveCount = 0; // how many moves have been generated so far this move.

        // arrays of pre-generated moves for some pieces as well as move generation helpers. 
        #region PreCalculated Moves
        private ulong[] knightMovesHelper = new ulong[64];
        private ulong[] bishopMovesHelper = new ulong[64];
        private ulong[] rookMovesHelper = new ulong[64];
        private ulong[] kingMovesHelper = new ulong[64];

        private ulong[,] pawnAttacksHelper = new ulong[2, 64];

        // the in between squares from any two squares that make a line or diagonal.
        private ulong[,] intersectLinesHelper = new ulong[64, 64];
        #endregion

        public MoveGenerator()
        {
            // pre calculated move generation helpers
            InitKnightMoves();
            InitBishopMoves();
            InitRookMoves();
            InitKingMoves();

            InitPawnAttacks();

            InitIntersectLines();
        }

        #region Move Generation Initialization
        private void InitKnightMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                ulong square = BitBoard.SquareToBit(i);

                knightMovesHelper[i] = (square & ~BitBoard.files[7]) << 17;               // up right
                knightMovesHelper[i] |= (square & ~BitBoard.files[0]) << 15;             // 
                knightMovesHelper[i] |= (square & ~(BitBoard.files[6] | BitBoard.files[7])) << 10;
                knightMovesHelper[i] |= (square & ~(BitBoard.files[0] | BitBoard.files[1])) << 6;

                knightMovesHelper[i] |= (square & ~BitBoard.files[0]) >> 17;               // down left
                knightMovesHelper[i] |= (square & ~BitBoard.files[7]) >> 15;             // down right
                knightMovesHelper[i] |= (square & ~(BitBoard.files[0] | BitBoard.files[1])) >> 10;  // down left
                knightMovesHelper[i] |= (square & ~(BitBoard.files[6] | BitBoard.files[7])) >> 6;
            }
        }
        private void InitBishopMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                bishopMovesHelper[i] = (BitBoard.diagonalsNE[BitBoard.SQ64toDNE[i]] ^ BitBoard.diagonalsSW[BitBoard.SQ64toDSW[i]]);
            }
        }
        private void InitRookMoves()
        {
            int rank = 0, file = 0;
            for (int i = 0; i < 64; i++)
            {
                rookMovesHelper[i] = (BitBoard.ranks[rank] ^ BitBoard.files[file]);
                if (file < 7)
                {
                    file++;
                }
                else
                {
                    file = 0;
                    rank++;
                }
            }
        }
        private void InitKingMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                #region corner positions
                // corner positions
                if (i == 0)
                {
                    kingMovesHelper[0] = 0x302UL;
                    continue;
                }
                else if (i == 7)
                {
                    kingMovesHelper[7] = 0xc040UL;
                    continue;
                }
                else if (i == 63)
                {
                    kingMovesHelper[63] = 0x40c0000000000000UL;
                    continue;
                }
                else if (i == 56)
                {
                    kingMovesHelper[56] = 0x203000000000000UL;
                    continue;
                }
                #endregion

                ulong square = BitBoard.SquareToBit(i);
                // if it is on a border square;
                if ((square & BitBoard.borders) != 0)
                {
                    if ((square & BitBoard.ranks[0]) != 0)
                    {   //                   left         up left           up           up right        right
                        kingMovesHelper[i] = (square >> 1) | (square << 7) | (square << 8) | (square << 9) | (square << 1);
                    }
                    else if ((square & BitBoard.ranks[7]) != 0)
                    {   //                               down left         down         down right
                        kingMovesHelper[i] = (square >> 1) | (square >> 9) | (square >> 8) | (square >> 7) | (square << 1);
                    }
                    else if ((square & BitBoard.files[0]) != 0)
                    {
                        kingMovesHelper[i] = (square << 8) | (square << 9) | (square << 1) | (square >> 7) | (square >> 8);
                    }
                    else if ((square & BitBoard.files[7]) != 0)
                    {
                        kingMovesHelper[i] = (square << 8) | (square << 7) | (square >> 1) | (square >> 9) | (square >> 8);
                    }
                }
                else // normal generation
                {
                    kingMovesHelper[i] = (square << 1) | (square << 9) | (square << 8) | (square << 7) | (square >> 1) | (square >> 9) | (square >> 8) | (square >> 7);
                }

            }
        }

        private void InitPawnAttacks()
        {
            // white
            for (int i = 0; i < 64; i++)
            {
                // left
                pawnAttacksHelper[0, i] = (BitBoard.SquareToBit(i) & ~BitBoard.files[0]) << 7;

                // right
                pawnAttacksHelper[0, i] |= (BitBoard.SquareToBit(i) & ~BitBoard.files[7]) << 9;
            }

            // black
            for (int i = 63; i >= 0; i--)
            {
                // left
                pawnAttacksHelper[1, i] = (BitBoard.SquareToBit(i) & ~BitBoard.files[0]) >> 9;

                // right
                pawnAttacksHelper[1, i] |= (BitBoard.SquareToBit(i) & ~BitBoard.files[7]) >> 7;
            }
        }

        // include destination square and don't include start square
        private void InitIntersectLines()
        {
            for (int from = 0; from < 64; from++)
            {
                for (int to = 0; to < 64; to++)
                {
                    ulong results = 0UL;
                    int start, finish;
                    if (from > to)
                    {
                        start = from;
                        finish = to;
                    }
                    else
                    {
                        start = to;
                        finish = from;
                    }

                    // left bit shift by 3 conveniently gives us the 0-7 rank given a 0-63 square number
                    // the same is true for (bit and 7) with files

                    //int rank = 8 >> 3;
                    //int file = 0 & 7;

                    // same rank
                    if ((from >> 3) == (to >> 3))
                    {
                        while (start >= finish)
                        {
                            results |= BitBoard.ranks[finish >> 3] & BitBoard.files[start & 7];
                            start--;
                        }
                        results &= ~BitBoard.SquareToBit(from);
                        intersectLinesHelper[from, to] = results;
                        continue;
                    }
                    // same file
                    else if ((from & 7) == (to & 7))
                    {
                        while (start >= finish)
                        {
                            results |= BitBoard.files[finish & 7] & BitBoard.ranks[start >> 3];
                            start -= 8;
                        }
                        results &= ~BitBoard.SquareToBit(from);
                        intersectLinesHelper[from, to] = results;
                        continue;
                    }
                    // diagonal
                    else if (BitBoard.SQ64toDSW[from] == BitBoard.SQ64toDSW[to])
                    {
                        while (start >= finish)
                        {
                            results |= BitBoard.diagonalsSW[BitBoard.SQ64toDSW[finish]] & BitBoard.ranks[start >> 3];
                            start -= 9;
                        }
                        results &= ~BitBoard.SquareToBit(from);
                        intersectLinesHelper[from, to] = results;
                        continue;
                    }
                    else if (BitBoard.SQ64toDNE[from] == BitBoard.SQ64toDNE[to])
                    {
                        while (start >= finish)
                        {
                            results |= BitBoard.diagonalsNE[BitBoard.SQ64toDNE[finish]] & BitBoard.ranks[start >> 3];
                            start -= 7;
                        }
                        results &= ~BitBoard.SquareToBit(from);
                        intersectLinesHelper[from, to] = results;
                        continue;
                    }
                    intersectLinesHelper[from, to] = 0UL;
                }

            }
        }
        #endregion

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
                AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.wQ));
                AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.wR));
                AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.wB));
                AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.wN));
                return;
            }
            else
            {
                AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            }
        }
        private void AddPawnCapture(Square from, Square to, Piece captured, int side)
        {
            int[] rank = { 7, 0 };
            // is it a promotion move
            if ((int)to >> 3 == rank[side])
            {
                AddCaptureMove(Move.ToInt(from, to, captured, Piece.wQ));
                AddCaptureMove(Move.ToInt(from, to, captured, Piece.wR));
                AddCaptureMove(Move.ToInt(from, to, captured, Piece.wB));
                AddCaptureMove(Move.ToInt(from, to, captured, Piece.wN));
                return;
            }

            AddCaptureMove(Move.ToInt(from, to, captured, Piece.EMPTY));
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

            // a hack to eliminate if statements
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
            pawnDoubleMoves |= CircularLeftShift(pawnMoves & BitBoard.ranks[shiftValues[(int)position.side + 6]], shiftValues[(int)position.side]) & ~(friendlyLocations | enemyLocations);
            // pawn captures                                        remove proper edge pawns and add en passant

            pawnLeftCaptures = CircularLeftShift(position.locations[color] & ~BitBoard.files[0], shiftValues[(int)position.side + 2]);
            pawnRightCaptures = CircularLeftShift(position.locations[color] & ~BitBoard.files[7], shiftValues[(int)position.side + 4]);

            // attacks tracking
            position.attacks[0 + color] = (pawnLeftCaptures | pawnRightCaptures) & ~(friendlyLocations | enemyLocations);
            position.attacks[12 + (int)position.side] |= (pawnLeftCaptures | pawnRightCaptures) & ~(friendlyLocations | enemyLocations);

            pawnLeftCaptures &= (enemyLocations | BitBoard.SquareToBit((int)position.enPassant));
            pawnRightCaptures &= (enemyLocations | BitBoard.SquareToBit((int)position.enPassant));

            // captures tracking
            position.captures[color] = pawnLeftCaptures | pawnRightCaptures;
            position.captures[12 + (int)position.side] |= pawnLeftCaptures | pawnRightCaptures;

            #region add moves



            //int count = BitBoard.CountBits(pawnMoves);
            //for (int i = 0; i < count; i++)
            while (pawnMoves != 0)
            {
                Square to = (Square)BitBoard.PopBit(ref pawnMoves);
                Square from = to + shiftValues[10 + (int)position.side];

                AddPawnMove(from, to, (int)position.side);
            }
            //count = BitBoard.CountBits(pawnDoubleMoves);
            //for (int i = 0; i < count; i++)
            while (pawnDoubleMoves != 0)
            {
                Square to = (Square)BitBoard.PopBit(ref pawnDoubleMoves);
                Square from = to + shiftValues[12 + (int)position.side];

                AddDoubleMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY, 3));
            }
            //count = BitBoard.CountBits(pawnLeftCaptures);
            //for (int i = 0; i < count; i++)
            while (pawnLeftCaptures != 0)
            {
                Square to = (Square)BitBoard.PopBit(ref pawnLeftCaptures);
                Square from = to - (shiftValues[(int)position.side + 14]);

                if (to == position.enPassant)
                {
                    AddCaptureMove(Move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 1));
                    continue;
                }

                AddPawnCapture(from, to, position.pieceTypeBySquare[(int)to], (int)position.side);
            }
            //count = BitBoard.CountBits(pawnRightCaptures);
            //for (int i = 0; i < count; i++)
            while (pawnRightCaptures != 0)
            {
                Square to = (Square)BitBoard.PopBit(ref pawnRightCaptures);
                Square from = to - (shiftValues[(int)position.side + 16]);

                if (to == position.enPassant)
                {
                    AddCaptureMove(Move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY, 1));
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
                Square from = (Square)BitBoard.PopBit(ref bishops);
                // moves
                bishopMoves = bishopMovesHelper[(int)from];
                // captures
                bishopCaptures = bishopMoves & enemyLocations;
                // remove captures from moves
                bishopMoves = bishopMoves ^ (bishopCaptures);
                // remove friendly
                bishopMoves = bishopMoves & ~friendlyLocations;


                // add moves
                while (bishopMoves > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref bishopMoves);
                    // if no piece is between the move
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[2 + color] |= BitBoard.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (bishopCaptures > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref bishopCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    // should only intersect with the captured piece
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == BitBoard.SquareToBit((int)to))
                    {
                        position.captures[2 + color] |= BitBoard.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddCaptureMove(Move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
            #endregion

            #region rook
            while (rooks > 0)
            {
                Square from = (Square)BitBoard.PopBit(ref rooks);

                rookMoves = rookMovesHelper[(int)from];
                // attacks
                rookCaptures = rookMoves & enemyLocations;
                rookMoves = rookMoves ^ (rookCaptures);

                rookMoves = rookMoves & ~friendlyLocations;


                // add moves
                while (rookMoves > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref rookMoves);
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[3 + color] |= BitBoard.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (rookCaptures > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref rookCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == BitBoard.SquareToBit((int)to))
                    {
                        position.captures[3 + color] |= BitBoard.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddCaptureMove(Move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
            #endregion

            #region queen
            while (queens > 0)
            {
                Square from = (Square)BitBoard.PopBit(ref queens);

                queenMoves = rookMovesHelper[(int)from] | bishopMovesHelper[(int)from];
                //attacks
                queenCaptures = queenMoves & enemyLocations;
                queenMoves = queenMoves ^ (queenCaptures);

                queenMoves = queenMoves & ~friendlyLocations;



                // add moves
                while (queenMoves > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref queenMoves);
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == 0)
                    {
                        position.attacks[4 + color] |= BitBoard.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (queenCaptures > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref queenCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((intersectLinesHelper[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == BitBoard.SquareToBit((int)to))
                    {
                        position.captures[4 + color] |= BitBoard.SquareToBit((int)to);
                        position.captures[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                        AddCaptureMove(Move.ToInt(from, to, capture, Piece.EMPTY));
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
                Square from = (Square)BitBoard.PopBit(ref kings);

                kingMoves = kingMovesHelper[(int)from];
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

                    // is the king in check
                    if (IsSquareAttacked(position, BitBoard.SquareToBit(colorKey[(int)position.side + 4]), 1 - position.side))
                    {

                    }
                    else
                    {
                        // king side
                        // if this castle is still available
                        if ((position.castlePerm & colorKey[(int)position.side]) > 0)
                        {
                            ulong squares = intersectLinesHelper[colorKey[(int)position.side + 4], colorKey[(int)position.side + 6]];
                            if ((squares & (enemyLocations | friendlyLocations)) == 0)
                            {
                                // is it attacked
                                int n = 0;
                                while (squares != 0)
                                {
                                    int check = BitBoard.PopBit(ref squares);
                                    if (!IsSquareAttacked(position, BitBoard.SquareToBit(check), 1 - position.side))
                                    {
                                        n++;

                                        if (n > 1)
                                        {
                                            Square to = (Square)(colorKey[(int)position.side + 6]);
                                            AddQuiteMove(Move.ToInt(from, (Square)colorKey[(int)position.side + 6], Piece.EMPTY, Piece.EMPTY, 2));
                                        }

                                    }
                                }

                            }
                        }
                        // queen side
                        if ((position.castlePerm & colorKey[(int)position.side + 2]) > 0)
                        {

                            ulong squares = intersectLinesHelper[colorKey[(int)position.side + 4], colorKey[(int)position.side + 8]];
                            // is the space empty
                            if ((squares & (enemyLocations | friendlyLocations)) == 0)
                            {
                                // is it attacked
                                int n = 0;
                                while (squares != 0)
                                {
                                    int check = BitBoard.PopBit(ref squares);
                                    if (!IsSquareAttacked(position, BitBoard.SquareToBit(check), 1 - position.side))
                                    {
                                        n++;
                                        if (n > 1)
                                        {
                                            Square to = (Square)(colorKey[(int)position.side + 8]);
                                            AddQuiteMove(Move.ToInt(from, (Square)(colorKey[(int)position.side + 8]), Piece.EMPTY, Piece.EMPTY, 2));
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
                }
                #endregion

                // add moves
                while (kingMoves > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref kingMoves);

                    position.attacks[5 + color] |= BitBoard.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                    AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (kingCaptures > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref kingCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.captures[5 + color] |= BitBoard.SquareToBit((int)to);
                    position.captures[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                    AddCaptureMove(Move.ToInt(from, to, capture, Piece.EMPTY));
                }
            }
            #region knight
            while (knights > 0)
            {
                Square from = (Square)BitBoard.PopBit(ref knights);

                knightMoves = knightMovesHelper[(int)from];
                // captures
                knightCaptures = knightMoves & enemyLocations;
                // remove captures 
                knightMoves = knightMoves ^ knightCaptures;
                // remove friendly
                knightMoves = knightMoves & ~friendlyLocations;



                // add moves
                while (knightMoves > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref knightMoves);

                    position.attacks[1 + color] |= BitBoard.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                    AddQuiteMove(Move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (knightCaptures > 0)
                {
                    Square to = (Square)BitBoard.PopBit(ref knightCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.captures[1 + color] |= BitBoard.SquareToBit((int)to);
                    position.captures[12 + (int)position.side] |= BitBoard.SquareToBit((int)to);
                    AddCaptureMove(Move.ToInt(from, to, capture, Piece.EMPTY));
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
                Console.WriteLine(Move.ToString(legalMoves[i]));
            }
        }
        public bool IsSquareAttacked(Position position, ulong bbSquare, Color attackingSide)
        {
            int color = (int)attackingSide * 6;
            //Square pieceSquare = position.pieceSquareByType[5 + ((1 - (int)attackingSide) * 6),0];
            Square attacked = (Square)BitBoard.PopBit(ref bbSquare);

            // pawns
            if ((position.locations[0 + color] & pawnAttacksHelper[1 - (int)attackingSide, (int)attacked]) != 0)
            {
                return true;
            }
            // queen
            ulong queen = (rookMovesHelper[(int)attacked] | bishopMovesHelper[(int)attacked]) & position.locations[4 + color];
            while (queen != 0)
            {
                Square square = (Square)BitBoard.PopBit(ref queen);
                if ((intersectLinesHelper[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == BitBoard.SquareToBit((int)square))
                {
                    return true;
                }
            }
            // bishop
            ulong bishop = (bishopMovesHelper[(int)attacked] & position.locations[2 + color]);
            while (bishop != 0)
            {
                Square square = (Square)BitBoard.PopBit(ref bishop);
                if ((intersectLinesHelper[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == BitBoard.SquareToBit((int)square))
                {
                    return true;
                }
            }
            // rook
            ulong rook = (rookMovesHelper[(int)attacked] & position.locations[3 + color]);
            while (rook != 0)
            {
                Square square = (Square)BitBoard.PopBit(ref rook);
                if ((intersectLinesHelper[(int)attacked, (int)square] & (position.BlackPosition | position.WhitePosition)) == BitBoard.SquareToBit((int)square))
                {
                    return true;
                }
            }

            // knight
            ulong knight = (knightMovesHelper[(int)attacked] & position.locations[1 + color]);
            if (knight != 0)
            {
                return true;
            }

            //king
            ulong king = (kingMovesHelper[(int)attacked] & position.locations[5 + color]);
            if (king != 0)
            {
                return true;
            }

            return false;
        }


    }
}