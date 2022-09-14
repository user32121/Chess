using System.Diagnostics;
using static Chess.DllWrapper;

namespace Chess
{
    internal class Program
    {
        static char[] charBoard = new char[64]
        {
            'r','n','b','q','k','b','n','r',
            'p','p','p','p','p','p','p','p',
            '_','_','_','_','_','_','_','_',
            '_','_','_','_','_','_','_','_',
            '_','_','_','_','_','_','_','_',
            '_','_','_','_','_','_','_','_',
            'P','P','P','P','P','P','P','P',
            'R','N','B','Q','K','B','N','R',
        };
        static char[] charBoard2 = new char[64]
        {
'r','_','_','q','k','_','_','r',
'p','p','_','_','_','p','p','p',
'_','_','_','b','_','_','_','_',
'P','_','p','_','_','_','_','_',
'R','_','_','_','b','_','_','_',
'_','P','_','_','_','_','_','_',
'_','_','n','_','P','P','P','P',
'_','_','B','Q','K','B','N','R',
        };
        static Dictionary<PIECE, char> pieceToChar = new Dictionary<PIECE, char>()
        {
            { PIECE.NONE, '_' },
            { PIECE.WHITE_PAWN, 'P' },
            { PIECE.WHITE_ROOK, 'R' },
            { PIECE.WHITE_KNIGHT, 'N' },
            { PIECE.WHITE_BISHOP, 'B' },
            { PIECE.WHITE_QUEEN, 'Q' },
            { PIECE.WHITE_KING, 'K' },
            { PIECE.BLACK_PAWN, 'p' },
            { PIECE.BLACK_ROOK, 'r' },
            { PIECE.BLACK_KNIGHT, 'n' },
            { PIECE.BLACK_BISHOP, 'b' },
            { PIECE.BLACK_QUEEN, 'q' },
            { PIECE.BLACK_KING, 'k' },
        };
        static Dictionary<char, PIECE> charToPiece = new Dictionary<char, PIECE>();
        static Dictionary<int, char> xToFile = new Dictionary<int, char>()
        {
            { 0, 'a' },
            { 1, 'b' },
            { 2, 'c' },
            { 3, 'd' },
            { 4, 'e' },
            { 5, 'f' },
            { 6, 'g' },
            { 7, 'h' },
        };
        static Dictionary<int, char> yToRank = new Dictionary<int, char>()
        {
            { 0, '8' },
            { 1, '7' },
            { 2, '6' },
            { 3, '5' },
            { 4, '4' },
            { 5, '3' },
            { 6, '2' },
            { 7, '1' },
        };
        static Dictionary<char, int> fileToX = new Dictionary<char, int>();
        static Dictionary<char, int> rankToY = new Dictionary<char, int>();

        private static void SetupDicts()
        {
            foreach (KeyValuePair<PIECE, char> item in pieceToChar)
                charToPiece[item.Value] = item.Key;
            foreach (KeyValuePair<int, char> item in xToFile)
                fileToX[item.Value] = item.Key;
            foreach (KeyValuePair<int, char> item in yToRank)
                rankToY[item.Value] = item.Key;
        }

        static PIECE[] board = new PIECE[64];
        static Stopwatch sw = new Stopwatch();

        static void PerformMove(Move move)
        {
            board[move.toY * 8 + move.toX] = board[move.fromY * 8 + move.fromX];
            board[move.fromY * 8 + move.fromX] = PIECE.NONE;
        }

        static void LoadBoard(char[] curCharBoard)
        {
            for (int i = 0; i < 64; i++)
                board[i] = charToPiece[curCharBoard[i]];
        }

        static void PrintBoard()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                    Console.Write(pieceToChar[board[y * 8 + x]] + " ");
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            SetupDicts();
            LoadBoard(charBoard);

            Move prevMove = new Move();

            while (true)
            {
                PrintBoard();
                sw.Restart();
                Move move = DllWrapper.findBestMove(board, prevMove, 5);
                sw.Stop();

                Console.WriteLine("white: {0}{1} > {2}{3} ({4} ms)", xToFile[move.fromX], yToRank[move.fromY], xToFile[move.toX], yToRank[move.toY], sw.ElapsedMilliseconds);

                PerformMove(move);
                PrintBoard();

                while (true)
                {
                    Console.Write("Black: ");
                    string s = Console.ReadLine() ?? "";
                    string[] s2 = s.Split(">");
                    if (s2.Length != 2) { Console.WriteLine("need {from}>{to} format"); continue; }
                    string from = s2[0].Trim();
                    string to = s2[1].Trim();
                    if (from.Length != 2) { Console.WriteLine("{from} should be 2 characters"); continue; }
                    if (to.Length != 2) { Console.WriteLine("{to} should be 2 characters"); continue; }
                    if (!fileToX.TryGetValue(from[0], out int fromX)) { Console.WriteLine("{from} needs a file"); continue; }
                    if (!rankToY.TryGetValue(from[1], out int fromY)) { Console.WriteLine("{from} needs a rank"); continue; }
                    if (!fileToX.TryGetValue(to[0], out int toX)) { Console.WriteLine("{to} needs a file"); continue; }
                    if (!rankToY.TryGetValue(to[1], out int toY)) { Console.WriteLine("{to} needs a rank"); continue; }
                    prevMove = new Move() { fromX = fromX, fromY = fromY, toX = toX, toY = toY };
                    //check whether move is a valid move
                    break;
                }

                PerformMove(prevMove);
            }
        }
    }
}