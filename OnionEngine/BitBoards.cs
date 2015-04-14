using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class BitBoards
    {
        private readonly int[] BitTable = {
            63, 30, 3, 32, 25, 41, 22, 33, 15, 50, 42, 13, 11, 53, 19, 34, 61, 29, 2,
            51, 21, 43, 45, 10, 18, 47, 1, 54, 9, 57, 0, 35, 62, 31, 40, 4, 49, 5, 52,
            26, 60, 6, 23, 44, 46, 27, 56, 16, 7, 39, 48, 24, 59, 14, 12, 55, 38, 28,
            58, 20, 37, 17, 36, 8
            };

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
                                         0x1008040201000000UL,
                                         0x0804020100000000UL,
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
                                           2, 2, 3,  4,  5,  6,  7,  8,
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
        #endregion

        public BitBoards()
        {
            borders = ranks[0] | ranks[7] | files[0] | files[7];
            center3 = ~borders;

            InitKnightMoves();
            InitBishopMoves();
            InitRookMoves();
            InitKingMoves();
        }

        private void InitKnightMoves()
        {
            for (ulong i = 0; i < 64; i++)
            {
                knightMoves[i] = (i << 17) & ~files[7];
                knightMoves[i] |= (i << 15) & ~(files[0]); // 
                knightMoves[i] |= (i << 10) & ~(files[6] | files[7]);
                knightMoves[i] |= (i << 6) & ~(files[0] | files[1]);

                knightMoves[i] |= (i >> 17) & ~files[0];
                knightMoves[i] |= (i >> 15) & ~(files[7]); // 
                knightMoves[i] |= (i >> 10) & ~(files[0] | files[1]);
                knightMoves[i] |= (i >> 6) & ~(files[6] | files[7]);
            }
        }
        private void InitBishopMoves()
        {
            for (int i = 0; i < 64; i++)
            {
                bishopMoves[i] = (diagonalsSW[SQ64toDSW[i]] ^ diagonalsNE[SQ64toDNE[i]]);
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
            for (ulong i = 0; i < 64; i++)
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
                    kingMoves[63] = 0x40c0000000000000;
                    continue;
                }
                else if (i == 56)
                {
                    kingMoves[56] = 0x203000000000000;
                    continue;
                }
                #endregion

                // if it is on a border square;
                if ((i & borders) != 0)
                {
                    if ((i & ranks[0]) != 0)
                    {
                        kingMoves[i] = (ulong)((i << 1) | (i << 9) | (i << 8) | (i << 7) | (i >> 1));
                    }
                    else if ((i & ranks[7]) != 0)
                    {
                        kingMoves[i] = (ulong)((i << 1) | (i >> 1) | (i >> 9) | (i >> 8) | (i >> 7));
                    }
                    else if ((i & files[0]) != 0)
                    {
                        kingMoves[i] = (ulong)((i << 8) | (i << 7) | (i >> 1) | (i >> 9) | (i >> 8));
                    }
                    else if ((i & files[7]) != 0)
                    {
                        kingMoves[i] = (ulong)((i << 1) | (i << 9) | (i << 8) | (i >> 8) | (i >> 7));
                    }
                }
                else // normal generation
                {
                    kingMoves[i] = (ulong)((i << 1) | (i << 9) | (i << 8) | (i << 7) | (i >> 1) | (i >> 9) | (i >> 8) | (i >> 7));
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
    }
}
