using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        BLACK_PIECE = 16,
        BLACK_PAWN = 0 | BLACK_PIECE,
        BLACK_ROOK = 1 | BLACK_PIECE,
        BLACK_KNIGHT = 2 | BLACK_PIECE,
        BLACK_BISHOP = 3 | BLACK_PIECE,
        BLACK_QUEEN = 4 | BLACK_PIECE,
        BLACK_KING = 5 | BLACK_PIECE,
        COLOR_MASK = WHITE_PIECE | BLACK_PIECE,
    };

    public class GameBoard
    {
        public List<List<PIECE[,]>> boards;
        public int whitePawnStartY, blackPawnStartY;
        public int timelinesByWhite = 0;
        public bool whiteCanCastleKingSide, whiteCanCastleQueenSide, blackCanCastleKingSide, blackCanCastleQueenSide;
        public int enPassantOpportunity = -1;

        public GameBoard(List<List<PIECE[,]>> boards, int whitePawnStartY, int blackPawnStartY)
        {
            this.boards = boards;
            this.whitePawnStartY = whitePawnStartY;
            this.blackPawnStartY = blackPawnStartY;
            whiteCanCastleKingSide = whiteCanCastleQueenSide = blackCanCastleKingSide = blackCanCastleQueenSide = true;
        }

        public PIECE this[Point4 p]
        {
            get { return boards[p.c][p.t]?[p.x, p.y] ?? PIECE.INVALID; }
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
                catch (Exception e)
                {
                    return PIECE.NONE;
                    throw;
                }
            }
            set { boards[c][t][x, y] = value; }
        }

        public static GameBoard getStartingBoard() => new(new(){ new(){ new PIECE[,]
        {
            { PIECE.WHITE_ROOK, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_ROOK },
            { PIECE.WHITE_KNIGHT, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KNIGHT },
            { PIECE.WHITE_BISHOP, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_BISHOP },
            { PIECE.WHITE_KING, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KING },
            { PIECE.WHITE_QUEEN, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_QUEEN },
            { PIECE.WHITE_BISHOP, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_BISHOP },
            { PIECE.WHITE_KNIGHT, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KNIGHT },
            { PIECE.WHITE_ROOK, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_ROOK },
        } } }, 1, 6);
    }

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

        public static Move Invalid = new(new(-1, -1, -1, -1), new(-1, -1, -1, -1));

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3}->{4},{5},{6},{7}", from.c, from.t, from.x, from.y, to.c, to.t, to.x, to.y);
        }
    }

    public static class Utils
    {
        public static Dictionary<PIECE, Point> pieceTexIndex = new Dictionary<PIECE, Point>()
        {
            {PIECE.NONE,new Point(0,640)},
            {PIECE.WHITE_KING,new Point(0,0)},
            {PIECE.WHITE_QUEEN,new Point(320,0)},
            {PIECE.WHITE_BISHOP,new Point(640,0)},
            {PIECE.WHITE_KNIGHT,new Point(960,0)},
            {PIECE.WHITE_ROOK,new Point(1280,0)},
            {PIECE.WHITE_PAWN,new Point(1600,0)},
            {PIECE.BLACK_KING,new Point(0,320)},
            {PIECE.BLACK_QUEEN,new Point(320,320)},
            {PIECE.BLACK_BISHOP,new Point(640,320)},
            {PIECE.BLACK_KNIGHT,new Point(960,320)},
            {PIECE.BLACK_ROOK,new Point(1280,320)},
            {PIECE.BLACK_PAWN,new Point(1600,320)},
        };
        public static Dictionary<PIECE, int> pieceToPointValue = new Dictionary<PIECE, int>()
        {
            { PIECE.NONE, 0 },
            { PIECE.WHITE_PAWN, 1 },
            { PIECE.WHITE_KNIGHT, 3 },
            { PIECE.WHITE_BISHOP, 3 },
            { PIECE.WHITE_ROOK, 5 },
            { PIECE.WHITE_QUEEN, 9 },
            { PIECE.WHITE_KING, 1000 },
            { PIECE.BLACK_PAWN, -1 },
            { PIECE.BLACK_KNIGHT, -3 },
            { PIECE.BLACK_BISHOP, -3 },
            { PIECE.BLACK_ROOK, -5 },
            { PIECE.BLACK_QUEEN, -9 },
            { PIECE.BLACK_KING, -1000 },
        };

        public static List<Point4> rookDirs = new();
        public static List<Point4> bishopDirs = new();
        public static List<Point4> queenDirs = new();
        public static List<Point4> knightMoves = new();

        static Utils()
        {
            //generate all moves/dirs so I don't have to type 81 combinations of c,t,x,y
            for (int c = -1; c <= 1; c++)
                for (int t = -2; t <= 2; t += 2)
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                        {
                            int ac, at, ax, ay;
                            ac = c != 0 ? 1 : 0;
                            at = t != 0 ? 1 : 0;
                            ax = x != 0 ? 1 : 0;
                            ay = y != 0 ? 1 : 0;
                            if (ac + at + ax + ay == 1)
                                rookDirs.Add(new(c, t, x, y));
                            else if (ac + at + ax + ay == 2)
                            {
                                bishopDirs.Add(new(c, t, x, y));
                                int[] ar = new int[4] { c, t, x, y };
                                int i1 = -1, i2 = -1;
                                for (int i = 0; i < 4; i++)
                                    if (ar[i] != 0)
                                    {
                                        i1 = i2;
                                        i2 = i;
                                    }
                                ar[i1] *= 2;
                                knightMoves.Add(new(ar[0], ar[1], ar[2], ar[3]));
                                ar[i1] /= 2;
                                ar[i2] *= 2;
                                knightMoves.Add(new(ar[0], ar[1], ar[2], ar[3]));
                            }
                            if (ac + at + ax + ay > 0)
                                queenDirs.Add(new(c, t, x, y));
                        }
        }

        public static void PerformMove(GameBoard board, Move move, ref Move prevMove, Action<float, float> setViewOffset)
        {
            if (move.from.t == move.to.t && move.from.c == move.to.c)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else if (move.to.t == board.boards[move.to.c].Count - 1)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                board.boards[move.to.c].Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                if (IsWhitePiece(board[move.from]))
                {
                    List<PIECE[,]> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Insert(0, newRow);
                    move.from.t++;
                    move.from.c++;
                    move.to.c = 0;
                    move.to.t++;
                    board.timelinesByWhite++;
                }
                else
                {
                    List<PIECE[,]> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Add(newRow);
                    move.from.t++;
                    move.to.c = board.boards.Count - 1;
                    move.to.t++;
                    board.timelinesByWhite--;
                }
            }
            board[move.to] = board[move.from];
            board[move.from] = PIECE.NONE;

            prevMove = move;

            setViewOffset(move.to.c, move.to.t);
        }

        public static bool IsInBounds(GameBoard b, Point4 p)
        {
            return p.x >= 0 && p.y >= 0 && p.c >= 0 && p.t >= 0 && p.x < 8 && p.y < 8 && p.c < b.boards.Count && p.t < b.boards[p.c].Count && b.boards[p.c][p.t] != null;
        }
        public static bool IsWhitePiece(PIECE p)
        {
            return p.HasFlag(PIECE.WHITE_PIECE);
        }
        public static bool IsBlackPiece(PIECE p)
        {
            return p.HasFlag(PIECE.BLACK_PIECE);
        }
        public static bool IsFriendlyPiece(PIECE p1, PIECE p2)
        {
            return (p1 & PIECE.COLOR_MASK) == (p2 & PIECE.COLOR_MASK);
        }
        public static bool IsOpponentPiece(PIECE p1, PIECE p2)
        {
            return ((p1 ^ p2) & PIECE.COLOR_MASK) == PIECE.COLOR_MASK;
        }

        public static List<Move> GetAllMoves(GameBoard board)
        {
            List<(int, Move)> res = new();

            int minTurn = board.boards.Min(timeline => timeline.Count);
            bool isWhiteTurn = minTurn % 2 == 1;
            for (int c = 0; c < board.boards.Count; c++)
                if (board.boards[c].Count % 2 == 0 ^ isWhiteTurn)
                {
                    int t = board.boards[c].Count - 1;
                    for (int x = 0; x < 8; x++)
                        for (int y = 0; y < 8; y++)
                        {
                            Point4 pos = new(c, t, x, y);
                            PIECE p = board[pos];
                            if ((board.boards[c].Count % 2 == 1) ^ IsWhitePiece(p))  //cannot move opposite colored pieces on turn
                                continue;
                            int colorMultiplier = IsWhitePiece(p) ? 1 : -1;
                            switch (p)
                            {
                                case PIECE.NONE:
                                    break;
                                case PIECE.WHITE_PAWN:
                                    {
                                        //forward
                                        if (y < 7)
                                        {
                                            if (board[c, t, x, y + 1] == PIECE.NONE)
                                            {
                                                //1 y
                                                res.Add((0, new(pos, new(c, t, x, y + 1))));
                                                if (y == board.whitePawnStartY && board[c, t, x, y + 2] == PIECE.NONE)
                                                    //2 y
                                                    res.Add((0, new(pos, new(c, t, x, y + 2))));
                                            }
                                            //capture x/y
                                            if (x < 7 && IsBlackPiece(board[c, t, x + 1, y + 1]))
                                                res.Add((pieceToPointValue[board[c, t, x + 1, y + 1]], new(pos, new(c, t, x + 1, y + 1))));
                                            if (x > 0 && IsBlackPiece(board[c, t, x - 1, y + 1]))
                                                res.Add((pieceToPointValue[board[c, t, x - 1, y + 1]], new(pos, new(c, t, x - 1, y + 1))));
                                        }
                                        if (c < board.boards.Count - 1 && t < board.boards[c + 1].Count && board[c + 1, t, x, y] == PIECE.NONE)
                                        {
                                            //1 c
                                            if (t == board.boards[c + 1].Count - 1 || board.timelinesByWhite < 1)
                                                res.Add((0, new(pos, new(c + 1, t, x, y))));

                                            //capture c/t
                                            if (t + 1 < board.boards[c + 1].Count - 1 && (t + 1 == board.boards[c + 1].Count - 1 || board.timelinesByWhite < 1) && IsBlackPiece(board[c + 1, t + 1, x, y]))
                                                res.Add((pieceToPointValue[board[c + 1, t + 1, x, y]], new(pos, new(c + 1, t + 1, x, y))));
                                            if (t > 0 && (t - 1 == board.boards[c + 1].Count - 1 || board.timelinesByWhite < 1) && IsBlackPiece(board[c + 1, t - 1, x, y]))
                                                res.Add((pieceToPointValue[board[c + 1, t - 1, x, y]], new(pos, new(c + 1, t - 1, x, y))));
                                        }
                                        break;
                                    }
                                case PIECE.BLACK_PAWN:
                                    {
                                        //forward
                                        if (y > 0)
                                        {
                                            if (board[c, t, x, y - 1] == PIECE.NONE)
                                            {
                                                //1 y
                                                res.Add((0, new(pos, new(c, t, x, y - 1))));
                                                if (y == board.blackPawnStartY && board[c, t, x, y - 2] == PIECE.NONE)
                                                    //2 y
                                                    res.Add((0, new(pos, new(c, t, x, y - 2))));
                                            }
                                            //capture x/y
                                            if (x < 7 && IsWhitePiece(board[c, t, x + 1, y - 1]))
                                                res.Add((pieceToPointValue[board[c, t, x + 1, y - 1]], new(pos, new(c, t, x + 1, y - 1))));
                                            if (x > 0 && IsWhitePiece(board[c, t, x - 1, y - 1]))
                                                res.Add((pieceToPointValue[board[c, t, x - 1, y - 1]], new(pos, new(c, t, x - 1, y - 1))));
                                        }
                                        if (c > 0 && t < board.boards[c - 1].Count && board[c - 1, t, x, y] == PIECE.NONE)
                                        {
                                            //1 c
                                            if (t == board.boards[c - 1].Count - 1 || board.timelinesByWhite > -1)
                                                res.Add((0, new(pos, new(c - 1, t, x, y))));

                                            //capture c/t
                                            if (t < board.boards[c].Count - 1 && (t + 1 == board.boards[c - 1].Count - 1 || board.timelinesByWhite > -1) && IsBlackPiece(board[c - 1, t + 1, x, y]))
                                                res.Add((pieceToPointValue[board[c - 1, t + 1, x, y]], new(pos, new(c - 1, t + 1, x, y))));
                                            if (t > 0 && (t - 1 == board.boards[c - 1].Count - 1 || board.timelinesByWhite > -1) && IsWhitePiece(board[c - 1, t - 1, x, y]))
                                                res.Add((pieceToPointValue[board[c - 1, t - 1, x, y]], new(pos, new(c - 1, t - 1, x, y))));
                                        }
                                        break;
                                    }
                                case PIECE.WHITE_ROOK:
                                case PIECE.BLACK_ROOK:
                                    foreach (Point4 item in rookDirs)
                                    {
                                        for (int i = 1; i < 8; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2) || !(pos2.t == board.boards[pos2.c].Count - 1 || colorMultiplier * board.timelinesByWhite < 1))
                                                break;
                                            PIECE p2 = board[pos2];
                                            if (IsFriendlyPiece(p, p2))
                                                break;
                                            res.Add((pieceToPointValue[board[pos2]], new(pos, pos2)));
                                            if (IsOpponentPiece(p, p2))
                                                break;
                                        }
                                    }
                                    break;
                                case PIECE.WHITE_KNIGHT:
                                case PIECE.BLACK_KNIGHT:
                                    foreach (Point4 item in knightMoves)
                                    {
                                        Point4 pos2 = pos + item;
                                        if (!IsInBounds(board, pos2) || !(pos2.t == board.boards[pos2.c].Count - 1 || colorMultiplier * board.timelinesByWhite < 1))
                                            continue;
                                        PIECE p2 = board[pos2];
                                        if (IsFriendlyPiece(p, p2))
                                            continue;
                                        res.Add((pieceToPointValue[board[pos2]], new(pos, pos2)));
                                    }
                                    break;
                                case PIECE.WHITE_BISHOP:
                                case PIECE.BLACK_BISHOP:
                                    foreach (Point4 item in bishopDirs)
                                    {
                                        for (int i = 1; i < 8; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2) || !(pos2.t == board.boards[pos2.c].Count - 1 || colorMultiplier * board.timelinesByWhite < 1))
                                                break;
                                            PIECE p2 = board[pos2];
                                            if (IsFriendlyPiece(p, p2))
                                                break;
                                            res.Add((pieceToPointValue[board[pos2]], new(pos, pos2)));
                                            if (IsOpponentPiece(p, p2))
                                                break;
                                        }
                                    }
                                    break;
                                case PIECE.WHITE_QUEEN:
                                case PIECE.BLACK_QUEEN:
                                    foreach (Point4 item in queenDirs)
                                    {
                                        for (int i = 1; i < 8; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2) || !(pos2.t == board.boards[pos2.c].Count - 1 || colorMultiplier * board.timelinesByWhite < 1))
                                                break;
                                            PIECE p2 = board[pos2];
                                            if (IsFriendlyPiece(p, p2))
                                                break;
                                            res.Add((pieceToPointValue[board[pos2]], new(pos, pos2)));
                                            if (IsOpponentPiece(p, p2))
                                                break;
                                        }
                                    }
                                    break;
                                case PIECE.WHITE_KING:
                                case PIECE.BLACK_KING:
                                    foreach (Point4 item in queenDirs)
                                    {
                                        Point4 pos2 = pos + item;
                                        if (!IsInBounds(board, pos2) || !(pos2.t == board.boards[pos2.c].Count - 1 || colorMultiplier * board.timelinesByWhite < 1))
                                            continue;
                                        PIECE p2 = board[pos2];
                                        if (IsFriendlyPiece(p, p2))
                                            continue;
                                        res.Add((pieceToPointValue[board[pos2]], new(pos, pos2)));
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                }

            res.Sort((x, y) => Math.Abs(y.Item1) - Math.Abs(x.Item1));
            return res.Select(m => m.Item2).ToList();
        }

        public static Vector2 ScreenToWorldSpace(Vector2 p, Game g, Vector2 offset, float zoom)
        {
            p += new Vector2(-g.GraphicsDevice.Viewport.Bounds.Width / 2, -g.GraphicsDevice.Viewport.Bounds.Height / 2);
            p /= zoom;
            p += new Vector2(g.GraphicsDevice.Viewport.Bounds.Width / 2, g.GraphicsDevice.Viewport.Bounds.Height / 2);
            p += offset;
            return p;
        }
    }
}
