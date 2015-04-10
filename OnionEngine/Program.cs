using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnionEngine
{
    class Program
    {


        static void Main(string[] args)
        {
            //Board board = new Board();
            //MoveGenerator moveGen = new MoveGenerator();
            //Position position;

            //string fen1 = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1";
            ////string fen1 = "n1n5/PPPk4/8/8/8/8/4Kppp/5N1N b - - 0 1";
            ////string fen1 = "r3k2r/p6p/8/1Pp5/8/8/P6P/R3K2R b KQkq - 0 1";

            //position = board.ParseFen(fen1.Split(' '));
            //Perft perft = new Perft();
            ////perft.TestPosition(position, 2);
            //perft.Test(ref position, 1);

            //Console.WriteLine("");


            UCI uci = new UCI();
            uci.Loop();





        }
    }
}
