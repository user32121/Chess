using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class DllWrapper
    {
        [DllImport("ChessAlgo")]
        public static extern int test();

        [StructLayout(LayoutKind.Sequential)]
        public struct Move
        {
            public int fromX, fromY, toX, toY;
        };
        public enum PIECE : int
        {
            NONE = 0,
            WHITE_PAWN = 10,
            WHITE_ROOK = 11,
            WHITE_KNIGHT = 12,
            WHITE_BISHOP = 13,
            WHITE_QUEEN = 14,
            WHITE_KING = 15,
            BLACK_PAWN = 20,
            BLACK_ROOK = 21,
            BLACK_KNIGHT = 22,
            BLACK_BISHOP = 23,
            BLACK_QUEEN = 24,
            BLACK_KING = 25,
        };
        [DllImport("ChessAlgo")]
        public static extern Move findBestMove(PIECE[] board, Move lastMove, int maxDepth, int maxTime);
    }
}
