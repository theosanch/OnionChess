using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class BitBoards
    {
        private readonly int[] BitTable = { 63, 30,  3, 32, 25, 41, 22, 33,
                                            15, 50, 42, 13, 11, 53, 19, 34,
                                            61, 29,  2, 51, 21, 43, 45, 10,
                                            18, 47,  1, 54,  9, 57,  0, 35,
                                            62, 31, 40,  4, 49,  5, 52, 26, 
                                            60,  6, 23, 44, 46, 27, 56, 16,
                                             7, 39, 48, 24, 59, 14, 12, 55,
                                            38, 28, 58, 20, 37, 17, 36,  8 };

        #region General Board Info
        #region file and rank
        public readonly ulong[] ranks = {0x00000000000000FFUL,
                                     0x000000000000FF00UL,
                                     0x0000000000FF0000UL,
                                     0x00000000FF000000UL,
                                     0x000000FF00000000UL,
                                     0x0000FF0000000000UL,
                                     0x00FF000000000000UL,
                                     0xFF00000000000000UL};

        public readonly ulong[] files = { 0x0101010101010101UL,
                                     0x0202020202020202UL,
                                     0x0404040404040404UL,
                                     0x0808080808080808UL,
                                     0x1010101010101010UL,
                                     0x2020202020202020UL,
                                     0x4040404040404040UL,
                                     0x8080808080808080UL};
        #endregion
        #region diagonals
        public readonly ulong[] diagonalsSW = { 
                                         0x0100000000000000UL,      // A8
                                         0x0201000000000000UL,      // A7 to B8
                                         0x0402010000000000UL,
                                         0x0804020100000000UL,
                                         0x1008040201000000UL,
                                         0x2010080402010000UL,
                                         0x4020100804020100UL,      // A2 to G8
                                         0x8040201008040201UL,      // A1 to H8 diagonal
                                         0x0080402010080402UL,      // B1 to H7
                                         0x0000804020100804UL,
                                         0x0000008040201008UL,
                                         0x0000000080402010UL,
                                         0x0000000000804020UL,
                                         0x0000000000008040UL,
                                         0x0000000000000080UL};      // H1
        // given square n (0-63) return number that corresponds to its diagonal inside the SW array above
        // used in bishop move generation to get the diagonals given any square number
        public readonly int[] SQ64toDSW = {
                                           7, 8, 9, 10, 11, 12, 13, 14,
                                           6, 7, 8,  9, 10, 11, 12, 13,
                                           5, 6, 7,  8,  9, 10, 11, 12,
                                           4, 5, 6,  7,  8,  9, 10, 11,
                                           3, 4, 5,  6,  7,  8,  9, 10,
                                           2, 3, 4,  5,  6,  7,  8,  9,
                                           1, 2, 3,  4,  5,  6,  7,  8,
                                           0, 1, 2,  3,  4,  5,  6,  7
                                       };
        public readonly ulong[] diagonalsNE = { 
                                         0x0000000000000001UL,      // A1
                                         0x0000000000000102UL,      // A2 to B1
                                         0x0000000000010204UL,
                                         0x0000000001020408UL,
                                         0x0000000102040810UL,
                                         0x0000010204081020UL,
                                         0x0001020408102040UL,
                                         0x0102040810204080UL,      // A8 to H1 diagonal
                                         0x0204081020408000UL,      // B8 to H2
                                         0x0408102040800000UL,
                                         0x0810204080000000UL,
                                         0x1020408000000000UL,
                                         0x2040800000000000UL,
                                         0x4080000000000000UL,
                                         0x8000000000000000UL};     // H8
        public readonly int[] SQ64toDNE = {
                                           0, 1, 2,  3,  4,  5,  6,  7,
                                           1, 2, 3,  4,  5,  6,  7,  8,
                                           2, 3, 4,  5,  6,  7,  8,  9,
                                           3, 4, 5,  6,  7,  8,  9, 10,
                                           4, 5, 6,  7,  8,  9, 10, 11,
                                           5, 6, 7,  8,  9, 10, 11, 12,
                                           6, 7, 8,  9, 10, 11, 12, 13,
                                           7, 8, 9, 10, 11, 12, 13, 14,
                                       };
        #endregion

        public ulong borders;
        public const ulong corners = 0x8100000000000081UL;

        public const ulong center1 = 0x0000001818000000UL;
        public const ulong center2 = 0x00003c3c3c3c0000UL;
        public ulong center3;

        public const ulong darkSquares = 0xaa55aa55aa55aa55UL;
        public const ulong lightSquares = 0x55aa55aa55aa55aaUL;
        #endregion

        #region PreCalculated Moves
        public ulong[] knightMoves = new ulong[64];
        public ulong[] bishopMoves = new ulong[64];
        public ulong[] rookMoves = new ulong[64];
        public ulong[] kingMoves = new ulong[64];

        public ulong[,] pawnAttacks = new ulong[2,64];

        public ulong[,] intersectLines = new ulong[64, 64];
        #endregion

        public BitBoards()
        {
            borders = ranks[0] | ranks[7] | files[0] | files[7];
            center3 = ~borders;

            InitKnightMoves();
            InitBishopMoves();
            InitRookMoves();
            InitKingMoves();

            InitPawnAttacks();

            InitIntersectLines();
        }


        private void InitKnightMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                ulong square = SquareToBit(i);
                                    
                knightMoves[i]  = (square & ~files[7]) << 17;               // up right
                knightMoves[i] |= (square & ~files[0]) << 15 ;             // 
                knightMoves[i] |= (square & ~(files[6] | files[7])) << 10;
                knightMoves[i] |= (square & ~(files[0] | files[1])) << 6;

                knightMoves[i] |= (square & ~files[0]) >> 17;               // down left
                knightMoves[i] |= (square & ~files[7]) >> 15;             // down right
                knightMoves[i] |= (square & ~(files[0] | files[1])) >> 10;  // down left
                knightMoves[i] |= (square & ~(files[6] | files[7])) >> 6;
            }
        }
        private void InitBishopMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                bishopMoves[i] = (diagonalsNE[SQ64toDNE[i]] ^ diagonalsSW[SQ64toDSW[i]]);
            }
        }
        private void InitRookMoves()
        {
            int rank = 0, file = 0;
            for (int i = 0; i < 64; i++)
            {
                rookMoves[i] = (ranks[rank] ^ files[file]);
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
                    kingMoves[0] = 0x302UL;
                    continue;
                }
                else if (i == 7)
                {
                    kingMoves[7] = 0xc040UL;
                    continue;
                }
                else if (i == 63)
                {
                    kingMoves[63] = 0x40c0000000000000UL;
                    continue;
                }
                else if (i == 56)
                {
                    kingMoves[56] = 0x203000000000000UL;
                    continue;
                }
                #endregion

                ulong square = SquareToBit(i);
                // if it is on a border square;
                if ((square & borders) != 0)
                {
                    if ((square & ranks[0]) != 0)
                    {   //                   left         up left           up           up right        right
                        kingMoves[i] = (square >> 1) | (square << 7) | (square << 8) | (square << 9) | (square << 1);
                    }
                    else if ((square & ranks[7]) != 0)
                    {   //                               down left         down         down right
                        kingMoves[i] = (square >> 1) | (square >> 9) | (square >> 8) | (square >> 7) | (square << 1);
                    }
                    else if ((square & files[0]) != 0)
                    {
                        kingMoves[i] = (square << 8) | (square << 9) | (square << 1) | (square >> 7) | (square >> 8);
                    }
                    else if ((square & files[7]) != 0)
                    {
                        kingMoves[i] = (square << 8) | (square << 7) | (square >> 1) | (square >> 9) | (square >> 8);
                    }
                }
                else // normal generation
                {
                    kingMoves[i] = (square << 1) | (square << 9) | (square << 8) | (square << 7) | (square >> 1) | (square >> 9) | (square >> 8) | (square >> 7);
                }

            }
        }

        private void InitPawnAttacks()
        {
            // white
            for (int i = 0; i < 64; i++)
            {
                // left
                pawnAttacks[0,i] = (SquareToBit(i) & ~files[0]) << 7;

                // right
                pawnAttacks[0, i] |= (SquareToBit(i) & ~files[7]) << 9;
            }

            // black
            for (int i = 63; i >= 0; i--)
            {
                // left
                pawnAttacks[1, i] = (SquareToBit(i) & ~files[0]) >> 9;

                // right
                pawnAttacks[1, i] |= (SquareToBit(i) & ~files[7]) >> 7;
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
                        while (start >= finish )
                        {
                            results |= ranks[finish >> 3] & files[start & 7];
                            start--;
                        }
                        results &= ~SquareToBit(from);
                        intersectLines[from, to] = results;
                        continue;
                    }
                    // same file
                    else if ((from & 7) == (to & 7))
                    {
                        while (start >= finish)
                        {
                            results |= files[finish & 7] & ranks[start >> 3];
                            start -= 8;
                        }
                        results &= ~SquareToBit(from);
                        intersectLines[from, to] = results;
                        continue;
                    }
                    // diagonal
                    else if (SQ64toDSW[from] == SQ64toDSW[to])
                    {
                        while (start >= finish)
                        {
                            results |= diagonalsSW[SQ64toDSW[finish]] & ranks[start >> 3];
                            start -= 9;
                        }
                        results &= ~SquareToBit(from);
                        intersectLines[from, to] = results;
                        continue;
                    }
                    else if (SQ64toDNE[from] == SQ64toDNE[to])
                    {
                        while (start >= finish)
                        {
                            results |= diagonalsNE[SQ64toDNE[finish]] & ranks[start >> 3];
                            start -= 7;
                        }
                        results &= ~SquareToBit(from);
                        intersectLines[from, to] = results;
                        continue;
                    }
                    intersectLines[from, to] = 0UL;
                }

            }
        }


        // how many bits are in this bit board. how many pawns for example
        public int CountBits(ulong bb)
        {
            ulong i;
            for (i = 0; i < bb; i++)
            {
                bb &= bb - 1;
            }

            return (int)i;
        }
        public int PopBit(ref ulong bb)
        {
            ulong b = bb ^ (bb - 1);
            uint fold = (uint)((b & 0xffffffff) ^ (b >> 32));
            bb &= (bb - 1);
            return BitTable[(fold * 0x783a9b23) >> 26];
        }
        // convert a square number to a bitboard with that one square "selected"
        public ulong SquareToBit(int square)
        {
            return 1UL << square;
        }

    }
}
