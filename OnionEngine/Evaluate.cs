namespace OnionEngine
{
    using System.Collections.Generic;

    class Evaluate
    {

        // Value of a piece based on what square it is located on. example: a knight is not as valuable in a corner square
        // from blacks perspective and must be flipped for white
        #region Location Value

        // general 


        // pawn
        int[] pawnLocationValue = { 0,0,0,0,0,0,0,0,
                                    5,5,5,5,5,5,5,5,
                                    2,3,4,4,4,4,3,2,
                                    1,2,2,3,3,2,2,1,
                                    0,0,0,4,4,3,0,0,
                                    2,1,0,2,2,1,1,2,
                                    4,4,4,1,1,4,4,4,
                                    0,0,0,0,0,0,0,0};

        // knight
        int[] knightLocationValue = { 0,1,1,1,1,1,1,0,
                                      1,1,3,3,3,3,1,1,
                                      2,3,4,5,5,4,3,2,
                                      2,3,5,5,5,5,3,2,
                                      1,2,4,5,5,4,2,1,
                                      1,2,3,4,4,3,2,1,
                                      1,1,1,1,1,1,1,1,
                                      0,1,1,0,0,1,1,0};

        // bishop
        int[] bishopLocationValue = { 0,1,1,1,1,1,1,0,
                                      1,1,3,3,3,3,1,1,
                                      2,3,4,5,5,4,3,2,
                                      2,3,5,5,5,5,3,2,
                                      1,2,4,5,5,4,2,1,
                                      1,2,3,4,4,3,2,1,
                                      1,1,1,1,1,1,1,1,
                                      0,1,1,0,0,1,1,0};

        // rook
        int[] rookLocationValue = { 4,4,4,5,5,4,4,4,
                                    4,5,5,5,5,5,5,4,
                                    1,2,3,4,4,3,2,1,
                                    1,2,3,4,4,3,2,1,
                                    1,2,3,4,4,3,2,1,
                                    1,1,2,2,2,2,1,1,
                                    3,4,4,4,4,4,4,3,
                                    2,0,0,4,3,4,0,2};
        // queen
        int[] queenLocationValue = { 1,1,2,3,3,2,1,1,
                                     2,3,4,5,5,4,3,2,
                                     0,1,1,5,5,1,1,0,
                                     0,1,1,4,4,1,1,2,
                                     3,3,1,4,4,1,3,1,
                                     2,3,0,3,1,2,2,1,
                                     1,2,4,2,4,2,1,0,
                                     0,1,4,5,3,1,5,0};

        // king
        int[] kingLocationValue = { 0,0,0,0,0,0,0,0,
                                    0,0,0,0,0,0,0,0,
                                    0,0,1,1,1,1,0,0,
                                    0,0,1,2,2,1,0,0,
                                    0,0,1,2,2,1,0,0,
                                    0,0,0,1,1,0,0,0,
                                    0,0,0,1,1,2,0,0,
                                    0,0,4,1,3,0,5,0};
        // all arrays in one
        List<int[]> locationValues = new List<int[]>();

        #endregion
        

        // helper that flips an array based on side 
        int[] flip = { 56,57,58,59,60,61,62,63,
                       48,49,50,51,52,53,54,55,
                       40,41,42,43,44,45,46,47,
                       32,33,34,35,36,37,38,39,
                       24,25,26,27,28,29,30,31,
                       16,17,18,19,20,21,22,23,
                       08,09,10,11,12,13,14,15,
                       00,01,02,03,04,05,06,07};

        public Evaluate()
        {
            locationValues.Add(pawnLocationValue);
            locationValues.Add(knightLocationValue);
            locationValues.Add(bishopLocationValue);
            locationValues.Add(rookLocationValue);
            locationValues.Add(queenLocationValue);
            locationValues.Add(kingLocationValue);
        }


        // A quick and simple material evaluation
        public int QuickEvaluate(Position position)
        {
            int score = 0;
            int side = (int)position.side * 6;

            // each piece type
            score += BitBoard.CountBits(position.locations[0 + side]) * 100;
            score += BitBoard.CountBits(position.locations[1 + side]) * 300;
            score += BitBoard.CountBits(position.locations[2 + side]) * 300;
            score += BitBoard.CountBits(position.locations[3 + side]) * 500;
            score += BitBoard.CountBits(position.locations[4 + side]) * 1000;

            score -= BitBoard.CountBits(position.locations[6 + side]) * 100;
            score -= BitBoard.CountBits(position.locations[7 + side]) * 300;
            score -= BitBoard.CountBits(position.locations[8 + side]) * 300;
            score -= BitBoard.CountBits(position.locations[9 + side]) * 500;
            score -= BitBoard.CountBits(position.locations[10 + side]) * 1000;

            return score;
        }

        // more advanced evaluation, but trimmed for speed
        public int MediumEvaluation(Position position)
        {
            int score = QuickEvaluate(position);

            // p n b r q k
            // for white each white piece type
            for (int i = 0; i < 6; i++)
            {
                // get the piece type bitboard
                ulong pieceBitboard = position.locations[i];
                while (pieceBitboard != 0)  // when all bits are removed or there is no piece of this type it will equal 0
                {
                    // returns an int location (0-63) and removes that bit from the bitboard
                    int location = BitBoard.PopBit(ref pieceBitboard);


                    score += (locationValues[i])[flip[location]] * 10;
                }

            }

            // same for black without the flip
            for (int i = 0; i < 6; i++)
            {
                // get the piece type bitboard
                ulong pieceBitboard = position.locations[i+6];
                while (pieceBitboard != 0)  // when all bits are removed or there is no piece of this type it will equal 0
                {
                    // returns an int location (0-63) and removes that bit from the bitboard
                    int location = BitBoard.PopBit(ref pieceBitboard);

                    score -= (locationValues[i])[location] * 10;
                }

            }
            



            return score;
        }


        // a full blown evaluation of the position
        public int LongEvaluate(Position position)
        {

            // how many squares are attacked


            // how many pieces are defended


            // how many pieces are being attacked



            return 0;
        }




        
    }
}
