using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    static class Fen
    {

        // reset a given position to an invalid initial state
        private static void ResetBoard(Position position)
        {
            #region properties
            for (int i = 0; i < 12; i++)
            {
                position.locations[i] = 0UL;
            }
            #endregion

            #region meta data
            position.side = Color.both;
            position.enPassantSquare = Square.INVALID;
            position.fiftyMoveCounter = 0;

            position.ply = 0;

            position.castleStatus = 0;

            position.hashKey = 0ul;
            #endregion
        }

        // set up the position given the FEN.split(' ')
        public static Position ParseFen(string[] fen)
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
                        position.locations[6] = BitBoard.AddBit(position.locations[6], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'n':
                        piece = Piece.bN;
                        position.locations[7] = BitBoard.AddBit(position.locations[7], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'b':
                        piece = Piece.bB;
                        position.locations[8] = BitBoard.AddBit(position.locations[8], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'r':
                        piece = Piece.bR;
                        position.locations[9] = BitBoard.AddBit(position.locations[9], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'q':
                        piece = Piece.bQ;
                        position.locations[10] = BitBoard.AddBit(position.locations[10], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'k':
                        piece = Piece.bK;
                        position.locations[11] = BitBoard.AddBit(position.locations[11], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'P':
                        piece = Piece.wP;
                        position.locations[0] = BitBoard.AddBit(position.locations[0], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'N':
                        piece = Piece.wN;
                        position.locations[1] = BitBoard.AddBit(position.locations[1], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'B':
                        piece = Piece.wB;
                        position.locations[2] = BitBoard.AddBit(position.locations[2], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'R':
                        piece = Piece.wR;
                        position.locations[3] = BitBoard.AddBit(position.locations[3], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'Q':
                        piece = Piece.wQ;
                        position.locations[4] = BitBoard.AddBit(position.locations[4], (int)Move.FileRanktoSquare(file, rank));
                        break;
                    case 'K':
                        piece = Piece.wK;
                        position.locations[5] = BitBoard.AddBit(position.locations[5], (int)Move.FileRanktoSquare(file, rank));
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
                        position.SetPiece(Move.FileRanktoSquare(file, rank), piece);
                    }
                    file++;
                }
            }
            #endregion

            #region active color
            if (fen[1] == "w")
            {
                position.side = Color.white;
            }
            else if (fen[1] == "b")
            {
                position.side = Color.black;
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
                    case 'K': position.castleStatus |= (int)Castle.WKCA; break;
                    case 'Q': position.castleStatus |= (int)Castle.WQCA; break;
                    case 'k': position.castleStatus |= (int)Castle.BKCA; break;
                    case 'q': position.castleStatus |= (int)Castle.BQCA; break;
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

                position.enPassantSquare = Move.FileRanktoSquare(file, rank);
            }
            #endregion

            #region fifty move
            position.fiftyMoveCounter = int.Parse(fen[4]);
            #endregion

            // generate hash key for this position
            position.hashKey = Hash.GeneratePositionKey(position);


            return position;
        }

        public static string ToFen(Position position)
        {

            string locations = "";

            // counter tracks how many empty squares are found
            int counter = 0;
            int rank = 7, file = 7;
            string temp = "";
            for (int i = 63; i >= 0; i--)
            {
                Piece piece = position.GetPieceTypeBySquare((Square)i);

                if (piece != Piece.EMPTY && counter > 0)
                {
                    temp += counter.ToString();
                    counter = 0;
                }


                // what piece is this?
                switch (piece)
                {
                    case Piece.EMPTY:
                        counter++;
                        break;
                    case Piece.wP:
                        temp += "P";
                        break;
                    case Piece.wN:
                        temp += "N";
                        break;
                    case Piece.wB:
                        temp += "B";
                        break;
                    case Piece.wR:
                        temp += "R";
                        break;
                    case Piece.wQ:
                        temp += "Q";
                        break;
                    case Piece.wK:
                        temp += "K";
                        break;
                    case Piece.bP:
                        temp += "p";
                        break;
                    case Piece.bN:
                        temp += "n";
                        break;
                    case Piece.bB:
                        temp += "b";
                        break;
                    case Piece.bR:
                        temp += "r";
                        break;
                    case Piece.bQ:
                        temp += "q";
                        break;
                    case Piece.bK:
                        temp += "k";
                        break;
                    case Piece.INVALID:
                        break;
                    default:
                        break;
                }

                if (file > 0)
                {
                    file--;
                }
                else
                {
                    if (counter > 0)
                    {
                        locations += Reverse(temp + counter.ToString());
                        counter = 0;
                    }
                    else
                    {
                        locations += Reverse(temp);                 
                    }                   

                    temp = "";
                    file = 7;
                    rank--;

                    if (rank == -1)
                    {

                    }
                    else
                    {
                        locations += "/";
                    }
                }
            }
            //locations = locations.Remove(locations.Count() - 1, 1);


            #region side
            string side;
            if (position.side == Color.white)
            {
                side = "w";
            }
            else
            {
                side = "b";
            }
            #endregion

            #region castle
            string castle = "";
            if ((position.castleStatus | (int)Castle.WKCA) != 0)
            {
                castle += "K";
            }

            if ((position.castleStatus | (int)Castle.WQCA) != 0)
            {
                castle += "Q";
            }

            if ((position.castleStatus | (int)Castle.BKCA) != 0)
            {
                castle += "k";
            }

            if ((position.castleStatus | (int)Castle.BQCA) != 0)
            {
                castle += "q";
            }

            // no castle
            if (castle == "")
            {
                castle = "-";
            }
            #endregion

            #region en passant
            string enPassant;
            if (position.enPassantSquare != Square.INVALID)
            {
                enPassant = position.enPassantSquare.ToString().ToLower();
            }
            else
            {
                enPassant = "-";
            }
            #endregion

            return locations + " " + side + " " + castle + " " + enPassant + " " + position.ply.ToString() + " " + ((int)Math.Floor(position.ply / 2.0) + 1.0).ToString();
        }

        // reverse a string from 
        // http://stackoverflow.com/questions/228038/best-way-to-reverse-a-string
        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
