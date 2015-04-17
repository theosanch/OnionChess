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
        BitBoards bitboardController;

        // MoveGenerator has a main task of generating all legal moves for a given position

        public File[] SquareToFile = new File[64];
        public Rank[] SquareToRank = new Rank[64];

        // Piece movement direction - non bit-board movement
        private readonly int[] C_KnightDirection = { -8, -19, -21, -12, 8, 19, 21, 12 };
        private readonly int[] C_RookDirection = { -1, -10, 1, 10 };
        private readonly int[] C_BishoptDirection = { -9, -11, 11, 9 };
        private readonly int[] C_KingDirection = { -1, -10, 1, 10, -9, -11, 11, 9 };

        // list of arrays
        // each array represents a single turns worth of piece moves
        private List<int[]> MoveTree = new List<int[]>();
        private List<int[]> ScoreTree = new List<int[]>(); // a score for each move

        // this turn
        private List<int> legalMoves = new List<int>();
        private int[] score = new int[128];
        private int moveCount = 0; // how many moves have been generated so far this move.

        public MoveGenerator(BitBoards bitboard)
        {
            bitboardController = bitboard;
            InitFileRankBoards();
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

        private void InitFileRankBoards()
        {
            for (int i = 0; i < 64; i++)
            {
                SquareToFile[i] = File.File_NONE;
                SquareToRank[i] = Rank.Rank_NONE;
            }

            for (Rank rank = Rank.Rank_1; rank <= Rank.Rank_8; rank++)
            {
                for (File file = File.File_A; file <= File.File_H; file++)
                {
                    Square square = FileRanktoSquare(file, rank);
                    SquareToFile[(int)square] = file;
                    SquareToRank[(int)square] = rank;
                }
            }
        }

        #region old pawn moves
        //private void AddWhitePawnCaptureMove(Square from, Square to, Piece captured)
        //{
        //    // if this is a promotion move
        //    if ((from.ToString())[1] == '7')
        //    {
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wQ));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wR));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wB));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.wN));
        //    }
        //    else
        //    {
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.EMPTY));
        //    }
        //}
        //private void AddBlackPawnCaptureMove(Square from, Square to, Piece captured)
        //{
        //    // if this is a promotion move
        //    if ((from.ToString())[1] == '2')
        //    {
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bQ));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bR));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bB));
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.bN));
        //    }
        //    else
        //    {
        //        AddCaptureMove(this.move.ToInt(from, to, captured, Piece.EMPTY));
        //    }
        //}
        //private void AddWhitePawnMove(Square from, Square to)
        //{
        //    // if this is a promotion move
        //    if ((from.ToString())[1] == '7')
        //    {
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wQ));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wR));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wB));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.wN));
        //    }
        //    else
        //    {
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
        //    }
        //}
        //private void AddBlackPawnMove(Square from, Square to)
        //{
        //    // if this is a promotion move
        //    if ((from.ToString())[1] == '2')
        //    {
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bQ));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bR));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bB));
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.bN));
        //    }
        //    else
        //    {
        //        AddQuiteMove(this.move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
        //    }
        //}


        // return an array with all legal moves from the given position
        #endregion

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

        #region old intersect logic
        // return true if there is a piece between these two squares
        //private bool DiagonalIntersect(Position position, Square from, Square to, Piece piece)
        //{
        //    Rank high, low;
        //    ulong diagonal, results = 0UL;
        //    // get the diagonal
        //    #region get diagonal
        //    if (from > to)
        //    {
        //        high = SquareToRank[(int)from];
        //        low = SquareToRank[(int)to];

        //        if (SquareToFile[(int)from] < SquareToFile[(int)to])
        //        {
        //            diagonal = bitboardController.diagonalsSW[bitboardController.SQ64toDSW[(int)from]];
        //        }
        //        else
        //        {
        //            diagonal = bitboardController.diagonalsNE[bitboardController.SQ64toDNE[(int)from]];
        //        }
        //    }
        //    else
        //    {
        //        high = SquareToRank[(int)to];
        //        low = SquareToRank[(int)from];

        //        if (SquareToFile[(int)to] < SquareToFile[(int)from])
        //        {
        //            diagonal = bitboardController.diagonalsSW[bitboardController.SQ64toDSW[(int)to]];
        //        }
        //        else
        //        {
        //            diagonal = bitboardController.diagonalsNE[bitboardController.SQ64toDNE[(int)to]];
        //        }
        //    }
        //    #endregion

        //    // remove all ranks at high square and above
        //    while (high != low)
        //    {
        //        results |= diagonal & bitboardController.ranks[(int)high];
        //        high--;
        //    }

        //    if ((results & (position.WhitePosition | position.BlackPosition)) > 0)
        //    {
        //        return true;
        //    }
        //    position.attacks[(int)piece] = diagonal;
        //    return false;
        //}
        //private bool Intersect(Position position, Square from, Square to, Piece piece)
        //{
        //    ulong results = 0UL;
        //    // check if diagonal
        //    if (SquareToRank[(int)from] == SquareToRank[(int)to])
        //    {
        //        // same rank
        //        while (from != to)
        //        {
        //            results |= bitboardController.files[(int)SquareToFile[(int)from]] & bitboardController.ranks[(int)SquareToRank[(int)from]];
        //            if (from > to)
        //            {
        //                from -= 8;
        //            }
        //            else
        //            {
        //                from += 8;
        //            }
        //        }
        //    }
        //    else if (SquareToFile[(int)from] == SquareToFile[(int)to])
        //    {
        //        // same file
        //        while (from != to)
        //        {
        //            results |= bitboardController.ranks[(int)SquareToRank[(int)from]] & bitboardController.files[(int)SquareToFile[(int)from]];
        //            if (from > to)
        //            {
        //                from -= 8;
        //            }
        //            else
        //            {
        //                from += 8;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // diagonal
        //        return DiagonalIntersect(position, from, to, piece);
        //    }

        //    if ((results & (position.WhitePosition | position.BlackPosition)) > 0)
        //    {
        //        return true;
        //    }
        //    position.attacks[(int)piece] = results;
        //    return false;
        //}
        #endregion


        private ulong CircularLeftShift(ulong bitboard, int shift)
        {
            return (bitboard << shift) | (bitboard >> 64 - shift);
        }

        public int[] GenerateAllMoves(Position position)
        {
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
            #region generate moves

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

            // generate all valid pawn moves
            #region pawn moves
            // move forward one         move forward one square         if the position is empty

            pawnMoves = CircularLeftShift(position.locations[color], shiftValues[(int)position.side]) & ~(friendlyLocations | enemyLocations);
            // double move              pieces on the third rank        remove if occupied
            pawnDoubleMoves |= CircularLeftShift(pawnMoves & bitboardController.ranks[shiftValues[(int)position.side + 6]], shiftValues[(int)position.side]) & ~(friendlyLocations & enemyLocations);
            // pawn captures                                        remove proper edge pawns and add en passant
            pawnLeftCaptures = CircularLeftShift(position.locations[color] & ~bitboardController.files[0], shiftValues[(int)position.side + 2]) & (enemyLocations | bitboardController.SquareToBit((int)position.enPassant));
            pawnRightCaptures = CircularLeftShift(position.locations[color] & ~bitboardController.files[7], shiftValues[(int)position.side + 4]) & (enemyLocations | bitboardController.SquareToBit((int)position.enPassant));

            position.attacks[color] = pawnLeftCaptures | pawnRightCaptures;
            position.attacks[12 + (int)position.side] |= pawnLeftCaptures | pawnRightCaptures;

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

                AddPawnCapture(from, to, position.pieceTypeBySquare[(int)to], (int)position.side);
            }
            while (pawnRightCaptures != 0)
            {
                Square to = (Square)bitboardController.PopBit(ref pawnRightCaptures);
                Square from = to - (shiftValues[(int)position.side + 16]);

                AddPawnCapture(from, to, position.pieceTypeBySquare[(int)to], (int)position.side);
            }

            #endregion
            #endregion

            // generate all slide moves
            #region rook bishop queen
            ulong bishops = position.locations[2 + color];
            ulong rooks = position.locations[3 + color];
            ulong queens = position.locations[4 + color];

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
                        position.attacks[2 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
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
                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (rookCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref rookCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == bitboardController.SquareToBit((int)to))
                    {
                        position.attacks[3 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
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
                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                    }
                }
                while (queenCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref queenCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];
                    if ((bitboardController.intersectLines[(int)from, (int)to] & (friendlyLocations | enemyLocations)) == bitboardController.SquareToBit((int)to))
                    {
                        position.attacks[4 + color] |= bitboardController.SquareToBit((int)to);
                        position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                        AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                    }
                }
            }
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
                                        1,57};
                    // king side
                    // if this castle is still available
                    if ((position.castlePerm & colorKey[(int)position.side]) > 0)
                    {
                        if ((bitboardController.intersectLines[colorKey[(int)position.side + 4], colorKey[(int)position.side + 6]] & (enemyLocations | friendlyLocations)) == 0)
                        {
                            AddQuiteMove(move.ToInt(from, (Square)colorKey[(int)position.side + 6], Piece.EMPTY, Piece.EMPTY, 2));
                        }

                    }
                    // queen side
                    if ((position.castlePerm & colorKey[(int)position.side + 2]) > 0)
                    {
                        if ((bitboardController.intersectLines[colorKey[(int)position.side + 4], colorKey[(int)position.side + 8]] & (enemyLocations | friendlyLocations)) == 0)
                        {
                            AddQuiteMove(move.ToInt(from, (Square)(colorKey[(int)position.side + 6] + 1), Piece.EMPTY, Piece.EMPTY, 2));
                        }
                    }
                }
                #endregion

                // add moves
                while (kingMoves > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref kingMoves);
                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (kingCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref kingCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.attacks[5 + color] |= bitboardController.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                }
            }

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
                    AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
                }
                while (knightCaptures > 0)
                {
                    Square to = (Square)bitboardController.PopBit(ref knightCaptures);
                    Piece capture = position.pieceTypeBySquare[(int)to];

                    position.attacks[1 + color] |= bitboardController.SquareToBit((int)to);
                    position.attacks[12 + (int)position.side] |= bitboardController.SquareToBit((int)to);
                    AddCaptureMove(move.ToInt(from, to, capture, Piece.EMPTY));
                }
            }
            #endregion
            #endregion

            #region old move generation
            //#region pawn
            //// if it is whites turn
            //if (color == Color.w)
            //{
            //    // for each pawn
            //    for (int i = 0; i < position.pieceNumber[(int)Piece.wP]; i++)
            //    {
            //        // what square is this pawn on
            //        Square from = position.pieceSquareByType[(int)Piece.wP, i];
            //        // is the spot ahead empty?
            //        if (position.pieceTypeBySquare[(int)from + 10] == Piece.EMPTY)
            //        {
            //            AddWhitePawnMove(from, from + 10);
            //            // check for double first move
            //            if ((from.ToString())[1] == '2' && position.pieceTypeBySquare[(int)from + 20] == Piece.EMPTY)
            //            {
            //                AddQuiteMove(this.move.ToInt(from, from + 20, Piece.EMPTY, Piece.EMPTY,3));
            //            }
            //        }

            //        // attack squares
            //        if (position.pieceTypeBySquare[(int)from + 9] != Piece.EMPTY && position.pieceTypeBySquare[(int)from + 9] != Piece.INVALID)
            //        {
            //            // is the piece black
            //            if ((int)position.pieceTypeBySquare[(int)from + 9] > 6)
            //            {
            //                AddWhitePawnCaptureMove(from, from + 9, position.pieceTypeBySquare[(int)from + 9]);
            //            }
            //        }
            //        if (position.pieceTypeBySquare[(int)from + 11] != Piece.EMPTY && position.pieceTypeBySquare[(int)from + 11] != Piece.INVALID)
            //        {
            //            if ((int)position.pieceTypeBySquare[(int)from + 11] > 6)
            //            {
            //                AddWhitePawnCaptureMove(from, from + 11, position.pieceTypeBySquare[(int)from + 11]);
            //            }
            //        }

            //        if (from + 9 == position.enPassant)
            //        {
            //            AddCaptureMove(this.move.ToInt(from, from + 9, position.pieceTypeBySquare[(int)from + 9], Piece.EMPTY,1));
            //        }
            //        else if (from + 11 == position.enPassant)
            //        {
            //            AddCaptureMove(this.move.ToInt(from, from + 11, position.pieceTypeBySquare[(int)from + 11], Piece.EMPTY,1));
            //        }
            //    }
            //}
            //else
            //{
            //    // for each pawn
            //    for (int i = 0; i < position.pieceNumber[(int)Piece.bP]; i++)
            //    {
            //        // what square is this pawn on
            //        Square from = position.pieceSquareByType[(int)Piece.bP, i];
            //        // is the spot ahead empty?
            //        if (position.pieceTypeBySquare[(int)from - 10] == Piece.EMPTY)
            //        {
            //            AddBlackPawnMove(from, from - 10);
            //            // check for double first move
            //            if ((from.ToString())[1] == '7' && position.pieceTypeBySquare[(int)from - 20] == Piece.EMPTY)
            //            {
            //                AddQuiteMove(this.move.ToInt(from, from - 20, Piece.EMPTY, Piece.EMPTY,3));
            //            }
            //        }

            //        // attack squares
            //        if (position.pieceTypeBySquare[(int)from - 9] != Piece.EMPTY && position.pieceTypeBySquare[(int)from - 9] != Piece.INVALID)
            //        {
            //            // is the piece white
            //            if ((int)position.pieceTypeBySquare[(int)from - 9] < 7)
            //            {
            //                AddBlackPawnCaptureMove(from, from - 9, position.pieceTypeBySquare[(int)from - 9]);
            //            }
            //        }
            //        if (position.pieceTypeBySquare[(int)from - 11] != Piece.EMPTY && position.pieceTypeBySquare[(int)from - 11] != Piece.INVALID)
            //        {
            //            if ((int)position.pieceTypeBySquare[(int)from - 11] < 7)
            //            {
            //                AddBlackPawnCaptureMove(from, from - 11, position.pieceTypeBySquare[(int)from - 11]);
            //            }
            //        }

            //        if (from - 9 == position.enPassant)
            //        {
            //            AddCaptureMove(this.move.ToInt(from, from - 9, position.pieceTypeBySquare[(int)from - 9], Piece.EMPTY,1));
            //        }
            //        else if (from - 11 == position.enPassant)
            //        {
            //            AddCaptureMove(this.move.ToInt(from, from - 11, position.pieceTypeBySquare[(int)from - 11], Piece.EMPTY,1));
            //        }
            //    }
            //}
            //#endregion

            //#region other pieces
            //int x = 0;
            //if (color == Color.b)
            //{
            //    // this ensures we only look at black pieces in the piece array
            //    x = 6;
            //}

            //// each piece type
            //for (x = x; x < 12; x++)
            //{
            //    // each piece of this type
            //    for (int y = 0; y < position.pieceNumber[x + 1]; y++)
            //    {
            //        Square from = position.pieceSquareByType[x+1, y];
            //        if (from == Square.INVALID)
            //        {
            //            break;
            //        }

            //        #region rook queen
            //        // if the type is rook or queen
            //        if (x + 1 == 4 || x + 1 == 5 || x + 1 == 10 || x + 1 == 11)
            //        {
            //            // go in this direction
            //            foreach (int i in C_RookDirection)
            //            {
            //                Square to = from + i;
            //                // while we are still on the board
            //                while (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
            //                {
            //                    // is the next square empty
            //                    if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
            //                    {
            //                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            //                    }
            //                    else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
            //                    {
            //                        // add possible capture
            //                        AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
            //                        break;
            //                    }
            //                    else
            //                    {
            //                        break;
            //                    }
            //                    to += i; // move
            //                }
            //            }
            //        }
            //        #endregion

            //        #region bishop queen
            //        if (x + 1 == 3 || x + 1 == 5 || x + 1 == 9 || x + 1 == 11)
            //        {
            //            // go in this direction
            //            foreach (int i in C_BishoptDirection)
            //            {
            //                Square to = from + i;
            //                // while we are still on the board
            //                while (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
            //                {
            //                    // is the next square empty
            //                    if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
            //                    {
            //                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            //                    }
            //                    else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
            //                    {
            //                        // add possible capture
            //                        AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
            //                        break;
            //                    }
            //                    else
            //                    {
            //                        break;
            //                    }
            //                    to += i; // move
            //                }
            //            }
            //        }
            //        #endregion

            //        #region king
            //        if (x+1 == 6 || x+1 == 12)
            //        {
            //            #region directional moves
            //            // go in this direction
            //            foreach (int i in C_KingDirection)
            //            {
            //                Square to = from + i;
            //                // while we are still on the board
            //                if (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
            //                {
            //                    if (!IsSquareAttacked(to,1 - color,position)) // is the next square attacked
            //                    {

            //                        // is the next square empty
            //                        if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
            //                        {
            //                            AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            //                        }
            //                        else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
            //                        {
            //                            // add possible capture
            //                            AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
            //                        }
            //                    }
            //                }
            //            }
            //            #endregion

            //            #region castle move
            //            if (position.castlePerm != 0)
            //            {
            //                if (position.side == Color.w)
            //                {
            //                    // if this castle is still available
            //                    if ((position.castlePerm & (int)Castle.WKCA) > 0)
            //                    {
            //                        // if the spaces are empty
            //                        if (position.pieceTypeBySquare[(int)Square.F1] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.G1] == Piece.EMPTY)
            //                        {
            //                            // if they are not attacked
            //                            if (!IsSquareAttacked(Square.F1,Color.b,position) && !IsSquareAttacked(Square.E1,Color.b,position))
            //                            {
            //                                AddQuiteMove(move.ToInt(Square.E1, Square.G1, Piece.EMPTY, Piece.EMPTY, 2));
            //                            }
            //                        }
            //                    }
            //                    if ((position.castlePerm & (int)Castle.WQCA) > 0)
            //                    {
            //                        // if the spaces are empty
            //                        if (position.pieceTypeBySquare[(int)Square.D1] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.C1] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.B1] == Piece.EMPTY)
            //                        {
            //                            // if they are not attacked
            //                            if (!IsSquareAttacked(Square.D1, Color.b, position) && !IsSquareAttacked(Square.E1, Color.b, position))
            //                            {
            //                                AddQuiteMove(move.ToInt(Square.E1, Square.C1, Piece.EMPTY, Piece.EMPTY,2));
            //                            }
            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    // if this castle is still available
            //                    if ((position.castlePerm & (int)Castle.BKCA) > 0)
            //                    {
            //                        // if the spaces are empty
            //                        if (position.pieceTypeBySquare[(int)Square.F8] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.G8] == Piece.EMPTY)
            //                        {
            //                            // if they are not attacked
            //                            if (!IsSquareAttacked(Square.F8, Color.b, position) && !IsSquareAttacked(Square.E8, Color.b, position))
            //                            {
            //                                AddQuiteMove(move.ToInt(Square.E8, Square.G8, Piece.EMPTY, Piece.EMPTY, 2));
            //                            }
            //                        }
            //                    }
            //                    if ((position.castlePerm & (int)Castle.BQCA) > 0)
            //                    {
            //                        // if the spaces are empty
            //                        if (position.pieceTypeBySquare[(int)Square.D8] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.C8] == Piece.EMPTY &&
            //                            position.pieceTypeBySquare[(int)Square.B8] == Piece.EMPTY)
            //                        {
            //                            // if they are not attacked
            //                            if (!IsSquareAttacked(Square.D8, Color.b, position) && !IsSquareAttacked(Square.E8, Color.b, position))
            //                            {
            //                                AddQuiteMove(move.ToInt(Square.E8, Square.C8, Piece.EMPTY, Piece.EMPTY, 2));
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            #endregion
            //        }
            //        #endregion

            //        #region knight
            //        if (x + 1 == 2 || x + 1 == 8)
            //        {
            //            // go in this direction
            //            foreach (int i in C_KnightDirection)
            //            {
            //                Square to = from + i;
            //                // while we are still on the board
            //                if (position.pieceTypeBySquare[(int)to] != Piece.INVALID)
            //                {
            //                    // is the next square empty
            //                    if (position.pieceTypeBySquare[(int)to] == Piece.EMPTY)
            //                    {
            //                        AddQuiteMove(move.ToInt(from, to, Piece.EMPTY, Piece.EMPTY));
            //                    }
            //                    else if ((position.pieceTypeBySquare[(int)to]).ToString()[0].ToString() != color.ToString()) // is it the opposite color
            //                    {
            //                        // add possible capture
            //                        AddCaptureMove(move.ToInt(from, to, position.pieceTypeBySquare[(int)to], Piece.EMPTY));
            //                    }
            //                }
            //            }
            //        }
            //        #endregion
            //    }

            //    if (x == 5 && color == Color.w)
            //    {   // if all white pieces have been searched end loop
            //        x = 12;
            //    }
            //}

            //#endregion

            //MoveTree.Add(legalMoves);
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
        public bool IsSquareAttacked(Position position, Square square, Color attackingSide)
        {
            #region old check
            //if (attackingSide == Color.w)
            //{
            //    // is there a pawn attacking this square
            //    if (position.pieceTypeBySquare[((int)square) - 11] == Piece.wP ||
            //        position.pieceTypeBySquare[((int)square) - 9] == Piece.wP)
            //    {
            //        return true;
            //    }
            //}
            //else
            //{
            //    if (position.pieceTypeBySquare[((int)square) + 11] == Piece.bP ||
            //        position.pieceTypeBySquare[((int)square) + 9] == Piece.bP)
            //    {
            //        return true;
            //    }
            //}

            //// check for a knight in proper directions
            //foreach (int direction in C_KnightDirection)
            //{
            //    Piece piece = position.pieceTypeBySquare[((int)square) + direction];
            //    if (piece == Piece.wN && attackingSide == Color.w)
            //    {
            //        return true;
            //    }
            //    else if (piece == Piece.bN && attackingSide == Color.b)
            //    {
            //        return true;
            //    }
            //}

            //// check for rook or queen
            //foreach (int direction in C_RookDirection)
            //{
            //    Piece piece = position.pieceTypeBySquare[((int)square) + direction];
            //    int i = direction;
            //    // is it a valid square?
            //    while (piece != Piece.INVALID)
            //    {
            //        // is there a piece on this square
            //        if (piece != Piece.EMPTY)
            //        {
            //            // is it the correct piece
            //            if ((piece == Piece.wR || piece == Piece.wQ) && attackingSide == Color.w)
            //            {
            //                return true;
            //            }
            //            else if ((piece == Piece.bR || piece == Piece.bQ) && attackingSide == Color.b)
            //            {
            //                return true;
            //            }
            //            break; // stop searching in this direction because a piece would be blocking this direction
            //        }
            //        i += direction;
            //        piece = position.pieceTypeBySquare[((int)square) + i];
            //    }
            //}

            //// bishop or queen
            //foreach (int direction in C_BishoptDirection)
            //{
            //    Piece piece = position.pieceTypeBySquare[((int)square) + direction];
            //    int i = direction;
            //    // is it a valid square?
            //    while (piece != Piece.INVALID)
            //    {
            //        // is there a piece on this square
            //        if (piece != Piece.EMPTY)
            //        {
            //            // is it the correct piece
            //            if ((piece == Piece.wB || piece == Piece.wQ) && attackingSide == Color.w)
            //            {
            //                return true;
            //            }
            //            else if ((piece == Piece.bB || piece == Piece.bQ) && attackingSide == Color.b)
            //            {
            //                return true;
            //            }
            //            break; // stop searching in this direction because a piece would be blocking this direction
            //        }
            //        i += direction;
            //        piece = position.pieceTypeBySquare[((int)square) + i];
            //    }
            //}

            //// king
            //foreach (int direction in C_KingDirection)
            //{
            //    Piece piece = position.pieceTypeBySquare[((int)square) + direction];

            //    if (piece == Piece.wK && attackingSide == Color.w)
            //    {
            //        return true;
            //    }
            //    else if (piece == Piece.bK && attackingSide == Color.b)
            //    {
            //        return true;
            //    }

            //}
            #endregion

            int color = (int)attackingSide * 6;

            // each piece type

            // knight
            //if ((bitboardController.knightMoves[(int)square] & position.locations[1 + color]) > 0)
            //{
            //    return true;
            //}
            //else if (((position.locations[0 + color] >> 7) & position.locations[0 + color]) > 0)
            //{

            //}

            if (((position.attacks[12 + (color / 6)]) & bitboardController.SquareToBit((int)square)) != 0)
            {
                return true;
            }

            return false;
        }

        // convert file and rank to square which has a 120 based number
        public Square FileRanktoSquare(File file, Rank rank)
        {
            return (Square)((int)file) + (8 * (int)rank);
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