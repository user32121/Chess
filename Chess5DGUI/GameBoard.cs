using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess5DGUI
{
    public enum PIECE : byte
    {
        NONE = 0,
        INVALID = 1,
        WHITE_PIECE = 8,
        WHITE_PAWN = 0 | WHITE_PIECE,
        WHITE_ROOK = 1 | WHITE_PIECE,
        WHITE_KNIGHT = 2 | WHITE_PIECE,
        WHITE_BISHOP = 3 | WHITE_PIECE,
        WHITE_QUEEN = 4 | WHITE_PIECE,
        WHITE_KING = 5 | WHITE_PIECE,
        WHITE_UNICORN = 6 | WHITE_PIECE,
        WHITE_DRAGON = 7 | WHITE_PIECE,
        BLACK_PIECE = 16,
        BLACK_PAWN = 0 | BLACK_PIECE,
        BLACK_ROOK = 1 | BLACK_PIECE,
        BLACK_KNIGHT = 2 | BLACK_PIECE,
        BLACK_BISHOP = 3 | BLACK_PIECE,
        BLACK_QUEEN = 4 | BLACK_PIECE,
        BLACK_KING = 5 | BLACK_PIECE,
        BLACK_UNICORN = 6 | BLACK_PIECE,
        BLACK_DRAGON = 7 | BLACK_PIECE,

        COLOR_MASK = WHITE_PIECE | BLACK_PIECE,
        PIECE_MASK = 0b111,
    };

    public struct Point4 : IEquatable<Point4>
    {
        public int x, y, t, c;  //x,y,time,choice

        public Point4(int c, int t, int x, int y)
        {
            this.x = x;
            this.y = y;
            this.t = t;
            this.c = c;
        }

        public static bool operator ==(Point4 p1, Point4 p2)
        {
            return p1.Equals(p2);
        }
        public static bool operator !=(Point4 p1, Point4 p2)
        {
            return !p1.Equals(p2);
        }
        public bool Equals(Point4 other)
        {
            return x == other.x && y == other.y && t == other.t && c == other.c;
        }
        public override bool Equals(object obj)
        {
            return obj is Point4 p && Equals(p);
        }
        public override int GetHashCode()
        {
            return (((17 * 23 + x.GetHashCode()) * 23 + y.GetHashCode()) * 23 + t.GetHashCode()) * 23 + c.GetHashCode();
        }

        public static Point4 operator +(Point4 p1, Point4 p2)
        {
            return new(p1.c + p2.c, p1.t + p2.t, p1.x + p2.x, p1.y + p2.y);
        }
        public static Point4 operator *(Point4 p, int x)
        {
            return new(p.c * x, p.t * x, p.x * x, p.y * x);
        }
    }

    public struct Move
    {
        public Point4 from, to;

        public Move(Point4 from, Point4 to)
        {
            this.from = from;
            this.to = to;
        }
        public Move(int c1, int t1, int x1, int y1, int c2, int t2, int x2, int y2)
        {
            from = new(c1, t1, x1, y1);
            to = new(c2, t2, x2, y2);
        }

        public static readonly Move Invalid = new(new(-1, -1, -1, -1), new(-1, -1, -1, -1));

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}->{4},{5},{6},{7}", from.c, from.t, from.x, from.y, to.c, to.t, to.x, to.y);
        }
    }

    public class GameBoard
    {
        public class Board2D
        {
            public PIECE[,] board;
            public int whitePawnStartY, blackPawnStartY;
            public bool whiteCanCastleKingSide, whiteCanCastleQueenSide, blackCanCastleKingSide, blackCanCastleQueenSide;
            public int enPassantOpportunity = -1;

            public Board2D(PIECE[,] board, int whitePawnStartY, int blackPawnStartY)
            {
                this.board = board;
                this.whitePawnStartY = whitePawnStartY;
                this.blackPawnStartY = blackPawnStartY;
                whiteCanCastleKingSide = whiteCanCastleQueenSide = blackCanCastleKingSide = blackCanCastleQueenSide = true;
            }

            public Board2D Clone()
            {
                return new Board2D(board, whitePawnStartY, blackPawnStartY)
                {
                    board = (PIECE[,])board.Clone(),
                    whiteCanCastleKingSide = whiteCanCastleKingSide,
                    whiteCanCastleQueenSide = whiteCanCastleQueenSide,
                    blackCanCastleKingSide = blackCanCastleKingSide,
                    blackCanCastleQueenSide = blackCanCastleQueenSide
                };
            }

            public PIECE this[int x, int y]
            {
                get { return board[x, y]; }
                set { board[x, y] = value; }
            }

            public static Board2D Board2DFromString(string s)
            {
                if (s == "")
                    return null;

                //based on FEN, but uses _ instead of [0-9]
                //full set: [PNBUDRQKpnbudrqk_]
                string[] lines = s.Split("/");
                foreach (string line in lines)
                    if (line.Length != lines[0].Length)
                        throw new ArgumentException("Rows must be same length");
                PIECE[,] board = new PIECE[lines[0].Length, lines.Length];
                int whitePawnStartY = -1, blackPawnStartY = -1;
                for (int x = 0; x < lines[0].Length; x++)
                    for (int y = 0; y < lines.Length; y++)
                    {
                        board[x, y] = lines[y][x] switch
                        {
                            'P' => PIECE.WHITE_PAWN,
                            'N' => PIECE.WHITE_KNIGHT,
                            'B' => PIECE.WHITE_BISHOP,
                            'U' => PIECE.WHITE_UNICORN,
                            'D' => PIECE.WHITE_DRAGON,
                            'R' => PIECE.WHITE_ROOK,
                            'Q' => PIECE.WHITE_QUEEN,
                            'K' => PIECE.WHITE_KING,
                            'p' => PIECE.BLACK_PAWN,
                            'n' => PIECE.BLACK_KNIGHT,
                            'b' => PIECE.BLACK_BISHOP,
                            'u' => PIECE.BLACK_UNICORN,
                            'd' => PIECE.BLACK_DRAGON,
                            'r' => PIECE.BLACK_ROOK,
                            'q' => PIECE.BLACK_QUEEN,
                            'k' => PIECE.BLACK_KING,
                            '_' => PIECE.NONE,
                            _ => throw new ArgumentException("Invalid character: " + lines[y][x]),
                        };
                        if (board[x, y] == PIECE.WHITE_PAWN)
                            whitePawnStartY = y;
                        if (board[x, y] == PIECE.BLACK_PAWN)
                            blackPawnStartY = y;
                    }

                return new(board, whitePawnStartY, blackPawnStartY);
            }
        }

        public List<List<Board2D>> boards;
        public int timelinesByWhite = 0;
        public int whiteKingCaptured = 0;
        public int width, height;

        private GameBoard() { }
        public GameBoard(List<List<Board2D>> boards)
        {
            this.boards = boards;
            width = boards[0][0].board.GetLength(0);
            height = boards[0][0].board.GetLength(1);
            foreach (List<Board2D> timeline in boards)
                foreach (Board2D board in timeline)
                    if (board.board.GetLength(0) != width || board.board.GetLength(1) != height)
                        throw new ArgumentException("All boards must have same size");
        }

        public GameBoard Clone()
        {
            return new GameBoard()
            {
                boards = boards.Select(l => l.Select(b2d => b2d?.Clone()).ToList()).ToList(),
                timelinesByWhite = timelinesByWhite,
                whiteKingCaptured = whiteKingCaptured,
                width = width,
                height = height,
            };
        }

        public PIECE this[Point4 p]
        {
            get
            {
                try
                {
                    return boards[p.c][p.t][p.x, p.y];
                }
                catch (Exception)
                {
                    return PIECE.INVALID;
                    throw;
                }
            }
            set { boards[p.c][p.t][p.x, p.y] = value; }
        }
        public PIECE this[int c, int t, int x, int y]
        {
            get
            {
                try
                {
                    return boards[c][t]?[x, y] ?? PIECE.INVALID;
                }
                catch (Exception)
                {
                    return PIECE.NONE;
                    throw;
                }
            }
            set { boards[c][t][x, y] = value; }
        }

        public static GameBoard GameBoardFromString(string data, params Move[] moves)
        {
            List<List<Board2D>> boards = data.Split("|").Select(x => x.Split("-").Select(y => Board2D.Board2DFromString(y)).ToList()).ToList();
            GameBoard gameBoard = new(boards);
            foreach (Move move in moves)
                Utils.PerformMove(gameBoard, move);
            return gameBoard;
        }
    }
}
