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
        white, black, both
    }

    // helper enum
    enum Castle
    {
        WKCA = 1, BKCA = 2, WQCA = 4, BQCA = 8
    }
    #endregion

    // some data of each made move to be used when unmaking a move
    struct MoveHistory
    {
        public ulong hashKey;

        public int move;

        public int fiftyMoveCounter;
        public Square enPassantSquare;
        public int castleStatus;
    }

    // TODO
    // Retain top generated moves for the next move generation
    // make global variable to represent pawn kind etc for array positions.


    /// <summary>
    /// Board handles higher level functions and information compared to the Position class
    /// which only handles very basic information of a single position. 
    /// 
    /// The most involved task is making/undoing moves and keeping track of that history
    /// </summary>
    class Board
    {

        #region properties
        

        MoveGenerator moveGenerator;

        // history of moves made   
        private MoveHistory[] moveHistory = new MoveHistory[256];

        private const int C_BOARD_SQUARE_NUMBER = 64;
        private const int C_MAX_GAME_MOVES = 2048;
        private const string C_START_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

       

        // value of each piece type
        // used for simple position evaluation  wP   wN   wB   wR    wQ    wK...
        private readonly int[] C_MaterialValues = { 0, 100, 325, 325, 550, 1000, 50000, 100, 325, 325, 550, 1000, 50000 };

        private readonly int[] CastleHelper = {
        11, 15, 15, 15, 10, 15, 15, 14,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
        15, 15, 15, 15, 15, 15, 15, 15,
         7, 15, 15, 15,  5, 15, 15, 13};
        #endregion

        public Board()
        {
            moveGenerator = new MoveGenerator();
        }

        

        #region position misc
        

        public Position StartPosition()
        {
            return Fen.ParseFen(C_START_FEN.Split(' '));
        }
        #endregion

        

        #region change position

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
            position.hashKey = Hash.HashPiece(position, piece, square);
            position.SetPiece(square, piece);
        }
        private Piece RemovePiece(Position position, Square square)
        {
            Piece piece = position.GetPieceTypeBySquare(square);
            position.hashKey = Hash.HashPiece(position, piece, square);
            position.RemovePiece(square, piece);

            return piece;         
        }
        private Piece MovePiece(Position position, Square from, Square to)
        {
            Piece piece = position.GetPieceTypeBySquare(from);

            position.hashKey = Hash.HashPiece(position, piece, from);

            // bitboard update            
            position.RemovePiece(from, piece);
            position.SetPiece(to, piece);

            return piece;
        }

        #region make unmake
        // return -1 if the king was captured
        // return 0 if the move was made
        // return 1 if the king is in check
        public int MakeMove(ref Position position, int move)
        {
            MoveHistory entry;
            entry.hashKey = position.hashKey;
            entry.move = move;
            entry.fiftyMoveCounter = position.fiftyMoveCounter;
            entry.enPassantSquare = position.enPassantSquare;
            entry.castleStatus = position.castleStatus;
     
            // add move to history
            moveHistory[position.ply] = entry;

            Square from = Move.GetFromSquare(move);
            Square to = Move.GetToSquare(move);
            Piece capture = Move.GetCapturedPiece(move);
            Color side = position.side;



            #region en passant capture or castle move
            // capture an en passant pawn
            if (Move.GetEnPassantCapture(move))
            {
                if (side == Color.white)
                {
                    RemovePiece(position, to - 8);
                }
                else
                {
                    RemovePiece(position, to + 8);
                }
            }
            else if (Move.GetCastle(move))
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
            #endregion

            // castle
            position.hashKey = Hash.HashCastle(position);
            position.castleStatus &= CastleHelper[(int)from];
            position.castleStatus &= CastleHelper[(int)to];
            position.enPassantSquare = Square.INVALID;
            position.hashKey = Hash.HashCastle(position);

            position.fiftyMoveCounter++;

            #region capture
            if (capture != Piece.EMPTY)
            {
                RemovePiece(position, to);
                position.fiftyMoveCounter = 0;
            }
            #endregion

            Piece piece = MovePiece(position, from, to);

            #region pawn
            if (piece == Piece.bP || piece == Piece.wP)
            {
                position.fiftyMoveCounter = 0;
                if (Move.GetPawnDoubleMove(move))
                {
                    if (side == Color.white)
                    {
                        position.enPassantSquare = to - 8;
                    }
                    else
                    {
                        position.enPassantSquare = to + 8;
                    }


                    position.hashKey = Hash.HashEnPassant(position);
                }
                else if (Move.GetPromotedPiece(move) != Piece.EMPTY)
                {
                    RemovePiece(position, to);
                    AddPiece(position, to, Move.GetPromotedPiece(move));
                }

            }
            #endregion


            if (position.enPassantSquare != Square.INVALID)
            {
                position.hashKey = Hash.HashEnPassant(position);
            }

            // update meta info
            position.ply++;
            position.side = 1 - side;

            #region king attacked
            // is the king attacked
            if (side == Color.white)
            {
                if (moveGenerator.IsSquareAttacked(position, position.locations[(int)Piece.wK - 1], Color.black))
                {
                    UndoMove(ref position);
                    return 1;
                }
            }
            else
            {
                if (moveGenerator.IsSquareAttacked(position, position.locations[(int)Piece.bK - 1], Color.white))
                {
                    UndoMove(ref position);
                    return 1;
                }
            }
            #endregion

            return 0;
        }

        public void UndoMove(ref Position position)
        {

            position.ply--;

            // get move from history
            MoveHistory entry = moveHistory[position.ply];

            position.hashKey = entry.hashKey;
            position.fiftyMoveCounter = entry.fiftyMoveCounter;
            position.enPassantSquare = entry.enPassantSquare;
            position.castleStatus = entry.castleStatus;

            Square from = Move.GetFromSquare(entry.move);
            Square to = Move.GetToSquare(entry.move);
            

            //Color side = position.side;
            position.side = 1 - position.side;

            // make move
            //Piece piece = MovePiece(position, to, from);
            Piece piece = position.GetPieceTypeBySquare(to);
            position.RemovePiece(to, piece);
            position.SetPiece(from, piece);

            Piece promoted = Move.GetPromotedPiece(entry.move);
            if (promoted != Piece.EMPTY)
            {
                // we have already reversed the movement
                // we just need to change the piece                
                position.RemovePiece(from, piece);

                // set pawn of the proper color
                position.SetPiece(from, (Piece)(1 + ((int)position.side * 6)));

            }
            else if (Move.GetCastle(entry.move))
            {
                // if it was a castle move
                Piece rookPiece;
                Square rookfrom;
                Square rookto;

                // undo rook move
                switch (to)
                {
                    case Square.C1:
                        //MovePiece(position, Square.D1, Square.A1);
                        rookfrom = Square.D1;
                        rookto = Square.A1;
                        rookPiece = Piece.wR;

                        break;
                    case Square.G1:
                        //MovePiece(position, Square.F1, Square.H1);
                        rookfrom = Square.F1;
                        rookto = Square.H1;
                        rookPiece = Piece.wR;

                        break;
                    case Square.C8:
                        //MovePiece(position, Square.D8, Square.A8);
                        rookfrom = Square.D8;
                        rookto = Square.A8;
                        rookPiece = Piece.bR;

                        break;
                    case Square.G8:
                        //MovePiece(position, Square.F8, Square.H8);
                        rookfrom = Square.F8;
                        rookto = Square.H8;
                        rookPiece = Piece.bR;

                        break;
                    default:
                        rookfrom = Square.INVALID;
                        rookto = Square.INVALID;
                        rookPiece = Piece.INVALID;

                        break;
                }

                position.RemovePiece(rookfrom, rookPiece);
                position.SetPiece(rookto, rookPiece);
            }
            else if (Move.GetEnPassantCapture(entry.move))
            {
                if (position.side == Color.white)
                {
                    position.SetPiece(to - 8,Piece.bP);
                }
                else
                {
                    position.SetPiece(to + 8, Piece.wP);
                }
            }


            Piece capture = Move.GetCapturedPiece(entry.move);
            #region capture
            //if we capture a piece
            if (capture != Piece.EMPTY)
            {
                //AddPiece(position, to, capture);
                position.SetPiece(to,capture);
            }
            #endregion

        }
        #endregion

        #endregion



        #region utility
        private Color PieceToColor(Piece piece)
        {
            if ((int)piece < 7)
            {
                return Color.white;
            }
            else
            {
                return Color.black;
            }
        }
        
        #endregion

        // return true if the move can be made from this position and is legal
        public bool MoveExists(Position position, int move)
        {
            // get a list of all moves for this position
            int[] moves = moveGenerator.GenerateAllMoves(position);
            Console.WriteLine("Board MoveExists GenerateMoves");

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
