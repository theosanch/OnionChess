
namespace OnionEngine
{
    class Evaluate
    {


        // A quick and simple material evaluation
        public int QuickEvaluate(Position position)
        {
            int score = 0;
            int side = (int)position.side * 6;
            
            // each piece type
            score += position.pieceNumber[0 + side] * 100;
            score += position.pieceNumber[1 + side] * 300;
            score += position.pieceNumber[2 + side] * 300;
            score += position.pieceNumber[3 + side] * 500;
            score += position.pieceNumber[4 + side] * 1000;

            score -= position.pieceNumber[6 - side] * 100;
            score -= position.pieceNumber[7 - side] * 300;
            score -= position.pieceNumber[8 - side] * 300;
            score -= position.pieceNumber[9 - side] * 500;
            score -= position.pieceNumber[10 - side] * 1000;

            return score;
        }

        // more advanced evaluation, but trimmed for speed
        public int MediumEvaluation(Position position)
        {
            return 0;
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
