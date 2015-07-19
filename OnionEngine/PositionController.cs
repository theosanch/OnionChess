using System;

namespace OnionEngine
{
    #region enumerators
    // enum for each piece on a square including white or black, an empty square, or an invalid out of bounds square
    enum Piece
    {
        EMPTY, wP, wN, wB, wR, wQ, wK, bP, bN, bB, bR, bQ, bK, INVALID
    }
    // enum for each square including out of bounds square
    enum Square
    {
        A1 = 0, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8,
        INVALID = 64
    }

    enum File
    {
        File_A, File_B, File_C, File_D, File_E, File_F, File_G, File_H, File_NONE
    }
    enum Rank
    {
        Rank_1, Rank_2, Rank_3, Rank_4, Rank_5, Rank_6, Rank_7, Rank_8, Rank_NONE
    }
    enum Color
    {
        w, b, Both
    }

    // helper enum
    enum Castle
    {
        WKCA = 1, BKCA = 2, WQCA = 4, BQCA = 8
    }
    #endregion

    class PositionController
    {
        #region properties
        Random rng = new Random();

        MoveGenerator moveGenerator;
        BitBoards bitboard;

        private Position[] positionHistory = new Position[256];

        private const int C_BoardSquareNumber = 64;
        private const int C_MaxGameMoves = 2048;
        private const string C_StartFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        // unique key for each piece [type,position]
        public ulong[,] pieceKeys = new ulong[13, 64];
        // key for white or black?
        public ulong sideKey;
        // a key for each castle permutation?
        public ulong[] castleKeys = new ulong[16];

        // helper boards
        public ulong[] setMask = new ulong[64];
        public ulong[] clearMask = new ulong[64];

        // value of each piece type
        // used for simple position evaluation  wP   wN   wB   wR    wQ    wK...
        private readonly int[] C_MaterialValues = { 0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000 };

        // Piece movement direction - non bit-board movement
        private readonly int[] C_KnightDirection = { -8, -19, -21, -12, 8, 19, 21, 12 };
        private readonly int[] C_RookDirection = { -1, -10, 1, 10 };
        private readonly int[] C_BishoptDirection = { -9, -11, 11, 9 };
        private readonly int[] C_KingDirection = { -1, -10, 1, 10, -9, -11, 11, 9 };

        private readonly int[] CastlePerm = {
        11, 15, 15, 15, 10, 15, 15, 14,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
         7, 15, 15, 15,  5, 15, 15, 13};
        #endregion

        public PositionController(BitBoards bitboard)
        {
            this.bitboard = bitboard;
            moveGenerator = new MoveGenerator(bitboard);

            InitBitMasks();
            InitHashKeys();
            //InitSq120to64();
            //InitFileRankBoards();
        }

        #region initialization
        private void InitBitMasks()
        {
            // set to 0
            for (int i = 0; i < 64; i++)
            {
                setMask[i] = 0ul;
                clearMask[i] = 0ul;
            }

            // set mask
            for (int i = 0; i < 64; i++)
            {
                setMask[i] |= (1ul << i);
                clearMask[i] = ~setMask[i];

            }
        }
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
        #endregion

        #region position misc
        // reset a given position to an invalid initial state
        private void ResetBoard(Position position)
        {
            #region properties
            for (int i = 0; i < 64; i++)
            {
                // convert from 120 to 64
                position.pieceTypeBySquare[i] = Piece.EMPTY;
            }

            for (int i = 0; i < 12; i++)
            {
                for (int x = 0; x < 10; x++)
                {
                    position.pieceSquareByType[i, x] = Square.INVALID;
                }

            }

            for (int i = 0; i < 2; i++)
            {
                position.materialScore[i] = 0;
            }

            for (int i = 0; i < 12; i++)
            {
                position.pieceNumber[i] = 0;

                position.locations[i] = 0UL;
            }

            for (int i = 0; i < 14; i++)
            {
                position.captures[i] = 0UL;
                position.attacks[i] = 0UL;

            }
            #endregion

            #region meta data
            position.side = Color.Both;
            position.enPassant = Square.INVALID;
            position.fiftyMoveCounter = 0;

            position.ply = 0;

            position.castlePerm = 0;

            position.positionKey = 0ul;
            #endregion
        }
        private void UpdateMaterialLists(Position position)
        {
            for (int i = 0; i < 64; i++)
            {
                Piece piece = position.pieceTypeBySquare[i];
                // is this a piece?
                if (piece != Piece.INVALID && piece != Piece.EMPTY)
                {
                    // what color is this piece
                    // add its material value to the proper sides material score
                    if ((int)piece < 7)
                    {
                        position.materialScore[0] += C_MaterialValues[(int)piece];
                    }
                    else
                    {
                        position.materialScore[1] += C_MaterialValues[(int)piece];
                    }

                    // piece list

                    // add the square to the array
                    // square[13, 10] - [13 types including empty, up to 10 pieces of that type]
                    // however, empties are never tracked in this array
                    position.pieceSquareByType[(int)piece - 1, position.pieceNumber[(int)piece - 1]] = (Square)i;
                    // we now have one more of this piece type.
                    position.pieceNumber[(int)piece - 1]++;

                    // a list of all squares 
                    position.pieceTypeBySquare[i] = piece;
                }
            }
        }

        // set up the position given the FEN.split(' ')
        public Position ParseFen(string[] fen)
        {
            Position position = new Position();
            ResetBoard(position);

            if (fen.Length < 4 || fen.Length > 6)
            {
                Console.WriteLine("FEN ERROR: Invalid Format");
                return null;
            }
            // location
            Rank rank = Rank.Rank_8;
            File file = File.File_A;

            // piece type
            Piece piece = Piece.INVALID;

            // how many consecrative empty spaces
            int count = 0;

            #region piece positions
            // piece positions
            foreach (char letter in fen[0])
            {
                // move one square minimum for each character
                count = 1;

                switch (letter)
                {
                    case 'p':
                        piece = Piece.bP;
                        position.locations[6] = AddBit(position.locations[6], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'n':
                        piece = Piece.bN;
                        position.locations[7] = AddBit(position.locations[7], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'b':
                        piece = Piece.bB;
                        position.locations[8] = AddBit(position.locations[8], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'r':
                        piece = Piece.bR;
                        position.locations[9] = AddBit(position.locations[9], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'q':
                        piece = Piece.bQ;
                        position.locations[10] = AddBit(position.locations[10], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'k':
                        piece = Piece.bK;
                        position.locations[11] = AddBit(position.locations[11], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'P':
                        piece = Piece.wP;
                        position.locations[0] = AddBit(position.locations[0], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'N':
                        piece = Piece.wN;
                        position.locations[1] = AddBit(position.locations[1], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'B':
                        piece = Piece.wB;
                        position.locations[2] = AddBit(position.locations[2], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'R':
                        piece = Piece.wR;
                        position.locations[3] = AddBit(position.locations[3], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'Q':
                        piece = Piece.wQ;
                        position.locations[4] = AddBit(position.locations[4], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;
                    case 'K':
                        piece = Piece.wK;
                        position.locations[5] = AddBit(position.locations[5], (int)moveGenerator.FileRanktoSquare(file, rank));
                        break;

                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                        piece = Piece.EMPTY;
                        count = int.Parse(letter.ToString());
                        //count = (int)(letter - '0');
                        break;

                    case '/':
                        rank--;
                        file = File.File_A;
                        continue;

                    default:
                        Console.WriteLine("FEN ERROR: Invalid Character");
                        return null;
                }

                for (int i = 0; i < count; i++)
                {

                    if (piece != Piece.EMPTY)
                    {
                        position.pieceTypeBySquare[(int)moveGenerator.FileRanktoSquare(file, rank)] = piece;

                    }
                    file++;
                }
            }
            #endregion

            #region active color
            if (fen[1] == "w")
            {
                position.side = Color.w;
            }
            else if (fen[1] == "b")
            {
                position.side = Color.b;
            }
            else
            {
                Console.WriteLine("FEN ERROR: Invalid Color");
                return null;
            }
            #endregion

            #region castling availability
            // castle 
            foreach (char character in fen[2])
            {
                switch (character)
                {
                    case 'K': position.castlePerm |= (int)Castle.WKCA; break;
                    case 'Q': position.castlePerm |= (int)Castle.WQCA; break;
                    case 'k': position.castlePerm |= (int)Castle.BKCA; break;
                    case 'q': position.castlePerm |= (int)Castle.BQCA; break;
                    case '-': break;
                    default:
                        Console.WriteLine("FEN ERROR: Invalid Castle");
                        return null;
                }
            }
            #endregion

            #region en passant
            if (fen[3] != "-")
            {
                // convert char to int and cast to enum
                // from char to int to File type
                file = (File)((int)(((fen[3])[0]) - 'a'));
                // from number to Rank
                rank = (Rank)(int.Parse(((fen[3])[1]).ToString()) - 1);

                position.enPassant = moveGenerator.FileRanktoSquare(file, rank);
            }
            #endregion

            #region fifty move
            position.fiftyMoveCounter = int.Parse(fen[4]);
            #endregion

            // generate hash key for this position
            position.positionKey = GeneratePositionKey(position);

            UpdateMaterialLists(position);

            return position;
        }
        public Position StartPosition()
        {
            return ParseFen(C_StartFen.Split(' '));
        }
        #endregion

        #region hash keys
        // return a random 64bit number
        // used for hash key generation
        private ulong Random64bit()
        {
            var buffer = new byte[sizeof(Int64)];
            rng.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }
        // generate unique key for a given position
        private ulong GeneratePositionKey(Position position)
        {
            ulong finalKey = 0;
            Piece piece = Piece.EMPTY;

            // check each square
            for (int squareNumber = 0; squareNumber < C_BoardSquareNumber; squareNumber++)
            {
                piece = position.pieceTypeBySquare[squareNumber];
                if (piece != Piece.INVALID && piece != Piece.EMPTY)
                {
                    // hash key if it is a valid square
                    finalKey ^= pieceKeys[(int)piece, squareNumber];
                }
            }

            // if it is whites turn
            if (position.side == Color.w)
            {
                finalKey ^= sideKey;
            }

            // if there is en passant available
            if (position.enPassant != Square.INVALID)
            {
                finalKey ^= pieceKeys[(int)Piece.EMPTY, (int)position.enPassant];
            }

            // castle status
            finalKey ^= castleKeys[position.castlePerm];

            return finalKey;
        }

        private ulong HashPiece(Position position, Piece piece, Square square)
        {
            return position.positionKey ^= (pieceKeys[(int)piece, (int)square]);
        }
        private ulong HashCastle(Position position)
        {
            return position.positionKey ^= castleKeys[position.castlePerm];
        }
        private ulong HashSide(Position position)
        {
            return position.positionKey ^= sideKey;
        }
        private ulong HashEnPassant(Position position)
        {
            return position.positionKey ^= pieceKeys[(int)Piece.EMPTY, (int)position.enPassant];
        }
        #endregion

        #region change position
        MoveController move = new MoveController();

        #region old make move
        //public void RemovePiece(Position position, Square square)
        //{
        //    Piece piece = position.pieceTypeBySquare[(int)square];
        //    Color color = PieceToColor(piece);

        //    position.positionKey = HashPiece(position, piece, square);

        //    position.pieceTypeBySquare[(int)square] = Piece.EMPTY;
        //    position.materialScore[(int)color] -= C_MaterialValues[(int)piece];

        //    if (piece == Piece.bP || piece == Piece.wP)
        //    {
        //        position.pawnBitBoard[(int)color] = RemoveBit(position.pawnBitBoard[(int)color], Square120To64[(int)square]);
        //        position.pawnBitBoard[(int)Color.Both] = RemoveBit(position.pawnBitBoard[(int)Color.Both], Square120To64[(int)square]);
        //    }

        //    for (int i = 0; i < position.pieceNumber[(int)piece]; i++)
        //    {
        //        if (position.pieceSquareByType[(int)piece, i] == square)
        //        {
        //            position.pieceNumber[(int)piece]--;
        //            position.pieceSquareByType[(int)piece, i] = Square.INVALID; //position.pieceSquareByType[(int)piece, position.pieceNumber[(int)piece]];

        //            // now move each square to the from of its array
        //            for (int x = i; x < 9; x++)
        //            {
        //                position.pieceSquareByType[(int)piece, x] = position.pieceSquareByType[(int)piece, x + 1];
        //            }

        //            break;
        //        }
        //    }
        //}
        //public void AddPiece(Position position, Square square, Piece piece)
        //{
        //    Color color = PieceToColor(piece);

        //    position.positionKey = HashPiece(position, piece, square);

        //    position.pieceTypeBySquare[(int)square] = piece;

        //    if (piece == Piece.bP || piece == Piece.wP)
        //    {
        //        position.pawnBitBoard[(int)color] = AddBit(position.pawnBitBoard[(int)color], (int)square);
        //        position.pawnBitBoard[(int)Color.Both] = AddBit(position.pawnBitBoard[(int)Color.Both], (int)square);
        //    }

        //    position.materialScore[(int)color] += C_MaterialValues[(int)piece];
        //    position.pieceSquareByType[(int)piece, position.pieceNumber[(int)piece]] = square;
        //    position.pieceNumber[(int)piece]++;
        //}
        //public void MovePiece(Position position, Square from, Square to)
        //{
        //    Piece piece = position.pieceTypeBySquare[(int)from];
        //    Color color = PieceToColor(piece);

        //    position.positionKey = HashPiece(position, piece, from);
        //    position.pieceTypeBySquare[(int)from] = Piece.EMPTY;

        //    position.positionKey = HashPiece(position, piece, to);
        //    position.pieceTypeBySquare[(int)to] = piece;





        //    for (int i = 0; i < position.pieceNumber[(int)piece]; i++)
        //    {
        //        if (position.pieceSquareByType[(int)piece, i] == from)
        //        {
        //            position.pieceSquareByType[(int)piece, i] = to;
        //        }
        //    }
        //}

        //public bool MakeMove(ref Position position, int move)
        //{
        //    Square from = this.move.GetFromSquare(move);
        //    Square to = this.move.GetToSquare(move);
        //    Color side = position.side;

        //    positionHistory[position.ply] = position.Clone();
        //    // test
        //    //position.pieceSquareByType[0,0] = Square.A1;



        //    if (this.move.GetEnPassantCapture(move))
        //    {
        //        if (side == Color.w)
        //        {
        //            RemovePiece(position,to - 10);
        //        }
        //        else
        //        {
        //            RemovePiece(position, to + 10);
        //        }
        //    }
        //    else if (this.move.GetCastle(move))
        //    {
        //        switch (to)
        //        {
        //            case Square.C1:
        //                MovePiece(position, Square.A1, Square.D1);
        //                break;
        //            case Square.G1:
        //                MovePiece(position, Square.H1, Square.F1);
        //                break;
        //            case Square.C8:
        //                MovePiece(position, Square.A8, Square.D8);
        //                break;
        //            case Square.G8:
        //                MovePiece(position, Square.H8, Square.F8);
        //                break;
        //            default:
        //                break;
        //        }
        //    }

        //    position.positionKey = HashCastle(position);

        //    position.castlePerm &= CastlePerm[(int)from];
        //    position.castlePerm &= CastlePerm[(int)to];
        //    position.enPassant = Square.INVALID;

        //    position.positionKey = HashCastle(position);

        //    Piece captured = this.move.GetCapturedPiece(move);
        //    position.fiftyMoveCounter++;

        //    if (captured != Piece.EMPTY)
        //    {
        //        RemovePiece(position, to);
        //        position.fiftyMoveCounter = 0;
        //    }

        //    position.ply++;

        //    if (position.pieceTypeBySquare[(int)from] == Piece.bP || position.pieceTypeBySquare[(int)from] == Piece.wP)
        //    {
        //        position.fiftyMoveCounter = 0;
        //        if (this.move.GetPawnDoubleMove(move))
        //        {
        //            if (side == Color.w)
        //            {
        //                position.enPassant = to - 10;
        //            }
        //            else
        //            {
        //                position.enPassant = to + 10;
        //            }


        //            position.positionKey = HashEnPassant(position);
        //        }
        //    }

        //    if (position.enPassant != Square.INVALID)
        //    {
        //        position.positionKey = HashEnPassant(position);
        //    }

        //    MovePiece(position, from, to);

        //    Piece promoted = this.move.GetPromotedPiece(move);

        //    if (promoted != Piece.EMPTY)
        //    {
        //        RemovePiece(position, to);
        //        AddPiece(position, to, promoted);
        //    }


        //    position.side = 1 - side;
        //    position.positionKey = HashSide(position);

        //    // is the king attacked
        //    if (side == Color.w)
        //    {
        //        if (moveGenerator.IsSquareAttacked(position.pieceSquareByType[(int)Piece.wK, 0], Color.b, position))
        //        {
        //            UndoMove(ref position);
        //            return false;
        //        }
        //    }
        //    else
        //    {
        //        if (moveGenerator.IsSquareAttacked(position.pieceSquareByType[(int)Piece.bK, 0], Color.w, position))
        //        {
        //            UndoMove(ref position);
        //            return false;
        //        }
        //    }

        //    return true;
        //}
        //public void UndoMove(ref Position position)
        //{
        //    position = positionHistory[position.ply - 1];
        //}
        #endregion

        private void AddPiece(Position position, Square square, Piece piece)
        {
            Color color = PieceToColor(piece);

            position.positionKey = HashPiece(position, piece, square);

            position.pieceTypeBySquare[(int)square] = piece;

            position.materialScore[(int)color] += C_MaterialValues[(int)piece];
            position.pieceSquareByType[(int)piece - 1, position.pieceNumber[(int)piece - 1]] = square;
            position.pieceNumber[(int)piece - 1]++;

            position.locations[(int)piece - 1] = AddBit(position.locations[(int)piece - 1], (int)square);
        }
        private void RemovePiece(Position position, Square square)
        {
            Piece piece = position.pieceTypeBySquare[(int)square];

            // piece array update
            position.positionKey = HashPiece(position, piece, square);
            position.pieceTypeBySquare[(int)square] = Piece.EMPTY;

            for (int i = 0; i < position.pieceNumber[(int)piece - 1]; i++)
            {
                if (position.pieceSquareByType[(int)piece - 1, i] == square)
                {
                    position.pieceNumber[(int)piece - 1]--;
                    position.pieceSquareByType[(int)piece - 1, i] = Square.INVALID; //position.pieceSquareByType[(int)piece, position.pieceNumber[(int)piece]];

                    // now move each square to the from of its array
                    for (int x = i; x < 9; x++)
                    {
                        position.pieceSquareByType[(int)piece - 1, x] = position.pieceSquareByType[(int)piece - 1, x + 1];
                    }

                    break;
                }
            }

            // bitboard update
            position.locations[(int)piece - 1] = RemoveBit(position.locations[(int)piece - 1], (int)square);
        }
        private void MovePiece(Position position, Square from, Square to)
        {
            Piece piece = position.pieceTypeBySquare[(int)from];

            // error print board
            if (piece == Piece.EMPTY)
            {
                PrintPosition(position);
            }

            // piece array update
            position.positionKey = HashPiece(position, piece, from);
            position.pieceTypeBySquare[(int)from] = Piece.EMPTY;

            position.positionKey = HashPiece(position, piece, to);
            position.pieceTypeBySquare[(int)to] = piece;

            for (int i = 0; i < position.pieceNumber[(int)piece - 1]; i++)
            {
                if (position.pieceSquareByType[(int)piece - 1, i] == from)
                {
                    position.pieceSquareByType[(int)piece - 1, i] = to;
                }
            }

            // bitboard update
            position.locations[(int)piece - 1] = RemoveBit(position.locations[(int)piece - 1], (int)from);
            position.locations[(int)piece - 1] = AddBit(position.locations[(int)piece - 1], (int)to);

        }

        // return 0 if the move was made
        // return 1 if the king is in check
        // return -1 if the king was captured
        public int MakeMove(ref Position position, int move)
        {
            // add move to history
            // check if hash is the same so we don't overwrite the same move over and over
            // clone() is not cheap
            if (positionHistory[position.ply] == null || positionHistory[position.ply].positionKey != position.positionKey)
            {
                positionHistory[position.ply] = position.Clone();
            }

            Square from = this.move.GetFromSquare(move);
            Square to = this.move.GetToSquare(move);
            Piece capture = this.move.GetCapturedPiece(move);
            Color side = position.side;

            //if (capture == Piece.bK || capture == Piece.wK)
            //{
            //    //UndoMove(ref position);
            //    return -1;
            //}

            // capture an en passant pawn
            if (this.move.GetEnPassantCapture(move))
            {
                if (side == Color.w)
                {
                    RemovePiece(position, to - 8);
                }
                else
                {
                    RemovePiece(position, to + 8);
                }
            }
            else if (this.move.GetCastle(move))
            {
                switch (to)
                {
                    case Square.C1:
                        MovePiece(position, Square.A1, Square.D1);
                        break;
                    case Square.G1:
                        MovePiece(position, Square.H1, Square.F1);
                        break;
                    case Square.C8:
                        MovePiece(position, Square.A8, Square.D8);
                        break;
                    case Square.G8:
                        MovePiece(position, Square.H8, Square.F8);
                        break;
                    default:

                        break;
                }
            }

            if (from == Square.H8 && to == Square.H4)
            {
                
            }

            // castle
            position.positionKey = HashCastle(position);
            position.castlePerm &= CastlePerm[(int)from];
            position.castlePerm &= CastlePerm[(int)to];
            position.enPassant = Square.INVALID;
            position.positionKey = HashCastle(position);

            position.fiftyMoveCounter++;

            if (capture != Piece.EMPTY)
            {
                RemovePiece(position, to);
                position.fiftyMoveCounter = 0;
            }

            MovePiece(position, from, to);
            if (position.pieceTypeBySquare[(int)to] == Piece.bP || position.pieceTypeBySquare[(int)to] == Piece.wP)
            {
                position.fiftyMoveCounter = 0;
                if (this.move.GetPawnDoubleMove(move))
                {
                    if (side == Color.w)
                    {
                        position.enPassant = to - 8;
                    }
                    else
                    {
                        position.enPassant = to + 8;
                    }


                    position.positionKey = HashEnPassant(position);
                }
                else if (this.move.GetPromotedPiece(move) != Piece.EMPTY)
                {
                    RemovePiece(position, to);
                    AddPiece(position, to, this.move.GetPromotedPiece(move));
                }

            }

            //if (position.enPassant != Square.INVALID)
            //{
            //    position.positionKey = HashEnPassant(position);
            //}

            // update meta info
            position.ply++;
            position.side = 1 - side;

            // is the king attacked
            if (side == Color.w)
            {
                if (moveGenerator.IsSquareAttacked(position, position.locations[(int)Piece.wK - 1], Color.b))
                {
                    UndoMove(ref position);
                    return 1;
                }
            }
            else
            {
                if (moveGenerator.IsSquareAttacked(position, position.locations[(int)Piece.bK - 1], Color.w))
                {
                    UndoMove(ref position);
                    return 1;
                }
            }

            return 0;
        }
        public void UndoMove(ref Position position)
        {
            position = positionHistory[position.ply - 1].Clone();
        }

        #endregion

        #region bit board manipulation
        // remove from a single position
        public ulong RemoveBit(ulong bitBoard, int square)
        {
            return (bitBoard & clearMask[square]);
        }
        // set a single position
        public ulong AddBit(ulong bitBoard, int square)
        {
            return (bitBoard | setMask[square]);
        }

        public void PrintBitBoard(ulong bitBoard)
        {
            string results = Environment.NewLine;

            ulong shiftMe = 1;
            Square square;
            int sq64 = 0;

            for (Rank rank = Rank.Rank_8; rank >= Rank.Rank_1; rank--)
            {
                for (File file = File.File_A; file <= File.File_H; file++)
                {
                    square = moveGenerator.FileRanktoSquare(file, rank);
                    sq64 = (int)square;

                    if (((shiftMe << sq64) & bitBoard) != 0)
                    {
                        results += "X";
                    }
                    else
                    {
                        results += "-";
                    }
                }

                results += Environment.NewLine;
            }
            Console.WriteLine(results);
        }
        #endregion

        #region utility
        private Color PieceToColor(Piece piece)
        {
            if ((int)piece < 7)
            {
                return Color.w;
            }
            else
            {
                return Color.b;
            }
        }
        public void PrintPosition(Position position)
        {
            Console.WriteLine("Position: " + position.positionKey.ToString("X"));
            Console.WriteLine("");

            for (Rank rank = Rank.Rank_8; rank >= Rank.Rank_1; rank--)
            {
                Console.Write(rank.ToString() + " ");

                for (File file = File.File_A; file <= File.File_H; file++)
                {
                    Square square = moveGenerator.FileRanktoSquare(file, rank);
                    Piece piece = position.pieceTypeBySquare[(int)square];

                    if (piece == Piece.EMPTY)
                    {
                        Console.Write("  . ");
                    }
                    else
                    {
                        Console.Write(" " + piece.ToString() + " ");
                    }
                }
                Console.WriteLine("");
                Console.WriteLine("");
            }

            Console.WriteLine("         A   B   C   D   E   F   G   H");
            Console.WriteLine("");
            Console.Write(string.Format("Side: {0} En Passant: {1} Castle: ", position.side, position.enPassant));

            // castle permission print
            // bit and operation
            if ((position.castlePerm & (int)Castle.WKCA) != 0)
            {
                Console.Write("K");
            }
            if ((position.castlePerm & (int)Castle.WQCA) != 0)
            {
                Console.Write("Q");
            }
            if ((position.castlePerm & (int)Castle.BKCA) != 0)
            {
                Console.Write("k");
            }
            if ((position.castlePerm & (int)Castle.BQCA) != 0)
            {
                Console.Write("q");
            }

            Console.WriteLine("");
            Console.WriteLine("");

        }
        #endregion

        // return true if the move can be made from this position and is legal
        public bool MoveExists(Position position, int move)
        {
            // get a list of all moves for this position
            int[] moves = moveGenerator.GenerateAllMoves(position);

            // look for a matching legal move
            foreach (int n in moves)
            {
                if (MakeMove(ref position, n) != 0)
                {
                    continue;
                }
                else
                {
                    if (n == move)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
