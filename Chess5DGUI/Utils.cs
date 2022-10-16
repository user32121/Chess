using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static Chess5DGUI.GameBoard;

namespace Chess5DGUI
{
    public static class Utils
    {
        public static readonly Dictionary<PIECE, Point> pieceTexIndex = new()
        {
            {PIECE.NONE,new Point(0,640)},
            {PIECE.WHITE_KING,new Point(0,0)},
            {PIECE.WHITE_QUEEN,new Point(320,0)},
            {PIECE.WHITE_BISHOP,new Point(640,0)},
            {PIECE.WHITE_UNICORN,new Point(320,640)},
            {PIECE.WHITE_DRAGON,new Point(960,640)},
            {PIECE.WHITE_KNIGHT,new Point(960,0)},
            {PIECE.WHITE_ROOK,new Point(1280,0)},
            {PIECE.WHITE_PAWN,new Point(1600,0)},
            {PIECE.BLACK_KING,new Point(0,320)},
            {PIECE.BLACK_QUEEN,new Point(320,320)},
            {PIECE.BLACK_BISHOP,new Point(640,320)},
            {PIECE.BLACK_UNICORN,new Point(640,640)},
            {PIECE.BLACK_DRAGON,new Point(1280,640)},
            {PIECE.BLACK_KNIGHT,new Point(960,320)},
            {PIECE.BLACK_ROOK,new Point(1280,320)},
            {PIECE.BLACK_PAWN,new Point(1600,320)},
        };
        public static readonly Dictionary<PIECE, int> pieceToPointValue = new()
        {
            { PIECE.NONE, 0 },
            { PIECE.WHITE_PAWN, 1 },
            { PIECE.WHITE_KNIGHT, 3 },
            { PIECE.WHITE_BISHOP, 3 },
            { PIECE.WHITE_UNICORN, 3 },
            { PIECE.WHITE_DRAGON, 3 },
            { PIECE.WHITE_ROOK, 5 },
            { PIECE.WHITE_QUEEN, 9 },
            { PIECE.WHITE_KING, 1 },
            { PIECE.BLACK_PAWN, -1 },
            { PIECE.BLACK_KNIGHT, -3 },
            { PIECE.BLACK_BISHOP, -3 },
            { PIECE.BLACK_UNICORN, -3 },
            { PIECE.BLACK_DRAGON, -3 },
            { PIECE.BLACK_ROOK, -5 },
            { PIECE.BLACK_QUEEN, -9 },
            { PIECE.BLACK_KING, -1 },
        };
        public const int WIN_VALUE = 1000;

        public static readonly List<Point4> rookDirs = new();
        public static readonly List<Point4> bishopDirs = new();
        public static readonly List<Point4> unicornDirs = new();
        public static readonly List<Point4> dragonDirs = new();
        public static readonly List<Point4> queenDirs = new();
        public static readonly List<Point4> knightMoves = new();

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
                            else if (ac + at + ax + ay == 3)
                                unicornDirs.Add(new(c, t, x, y));
                            else if (ac + at + ax + ay == 4)
                                dragonDirs.Add(new(c, t, x, y));
                            if (ac + at + ax + ay > 0)
                                queenDirs.Add(new(c, t, x, y));
                        }
        }

        public static Move PerformMove(GameBoard board, Move move)
        {
            int nextEnPassant = -1;
            bool doEnPassant = false;
            if (move.from.t == move.to.t && move.from.c == move.to.c)
            {
                if (IsSamePiece(board[move.from], PIECE.WHITE_PAWN))
                {
                    if (Math.Abs(move.from.y - move.to.y) == 2)
                        nextEnPassant = move.from.x;
                    if (move.from.x != move.to.x && board[move.to] == PIECE.NONE)
                        doEnPassant = true;
                }

                board.boards[move.from.c].Add(board.boards[move.from.c][move.from.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else if (move.to.t == board.boards[move.to.c].Count - 1)
            {
                board.boards[move.from.c].Add(board.boards[move.from.c][move.from.t].Clone());
                board.boards[move.to.c].Add(board.boards[move.to.c][move.to.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else
            {
                board.boards[move.from.c].Add(board.boards[move.from.c][move.from.t].Clone());
                if (IsWhitePiece(board[move.from]))
                {
                    List<Board2D> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add(board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Insert(0, newRow);
                    move.from.t++;
                    move.from.c++;
                    move.to.c = 0;
                    move.to.t++;
                    board.timelinesByWhite++;
                }
                else
                {
                    List<Board2D> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add(board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Add(newRow);
                    move.from.t++;
                    move.to.c = board.boards.Count - 1;
                    move.to.t++;
                    board.timelinesByWhite--;
                }
            }
            if (move.from != move.to)
            {
                if (board[move.to] == PIECE.WHITE_KING)
                    board.whiteKingCaptured++;
                else if (board[move.to] == PIECE.BLACK_KING)
                    board.whiteKingCaptured--;
            }

            PIECE p = board[move.from];

            board.boards[move.to.c][move.to.t].enPassantOpportunity = nextEnPassant;
            if (doEnPassant)
                board[move.to.c, move.to.t, move.to.x, move.from.y] = PIECE.NONE;

            if (p == PIECE.WHITE_PAWN && move.to.y == board.height - 1)
                p = PIECE.WHITE_QUEEN;
            else if (p == PIECE.BLACK_PAWN && move.to.y == 0)
                p = PIECE.BLACK_QUEEN;

            board[move.to] = p;
            board[move.from] = PIECE.NONE;

            return move;
        }

        public static bool IsInBounds(GameBoard b, Point4 p)
        {
            return p.c >= 0 && p.t >= 0 && p.x >= 0 && p.y >= 0 && p.c < b.boards.Count && p.t < b.boards[p.c].Count && b.boards[p.c][p.t] != null && p.x < b.width && p.y < b.height;
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
        public static bool IsSamePiece(PIECE p1, PIECE p2)
        {
            return (p1 & PIECE.PIECE_MASK) == (p2 & PIECE.PIECE_MASK);
        }

        public static List<Move> GetAllMoves(GameBoard board)
        {
            List<(int, Move)> res = new();

            int minTurn = board.boards.Where((timeline, c) => c >= board.timelinesByWhite - 1 && c <= board.boards.Count + board.timelinesByWhite).Min(timeline => timeline.Count);
            bool isWhiteTurn = minTurn % 2 == 1;
            for (int c = 0; c < board.boards.Count; c++)
                if (board.boards[c].Count % 2 == 0 ^ isWhiteTurn)
                {
                    int t = board.boards[c].Count - 1;
                    for (int x = 0; x < board.width; x++)
                        for (int y = 0; y < board.height; y++)
                        {
                            Point4 pos = new(c, t, x, y);
                            PIECE p = board[pos];
                            if ((board.boards[c].Count % 2 == 1) ^ IsWhitePiece(p))  //cannot move opposite colored pieces on turn
                                continue;
                            //int colorMultiplier = IsWhitePiece(p) ? 1 : -1;
                            switch (p)
                            {
                                case PIECE.NONE:
                                    break;
                                case PIECE.WHITE_PAWN:
                                    {
                                        //forward
                                        if (y < board.height - 1)
                                        {
                                            if (board[c, t, x, y + 1] == PIECE.NONE)
                                            {
                                                //1 y
                                                res.Add((0, new(pos, new(c, t, x, y + 1))));
                                                if (y == board.boards[c][t].whitePawnStartY && board[c, t, x, y + 2] == PIECE.NONE)
                                                    //2 y
                                                    res.Add((0, new(pos, new(c, t, x, y + 2))));
                                            }
                                            //capture x/y
                                            if (x < board.width - 1 && (IsBlackPiece(board[c, t, x + 1, y + 1]) || board.boards[c][t].enPassantOpportunity == x + 1))
                                                res.Add((pieceToPointValue[board[c, t, x + 1, y + 1]], new(pos, new(c, t, x + 1, y + 1))));
                                            if (x > 0 && (IsBlackPiece(board[c, t, x - 1, y + 1]) || board.boards[c][t].enPassantOpportunity == x - 1))
                                                res.Add((pieceToPointValue[board[c, t, x - 1, y + 1]], new(pos, new(c, t, x - 1, y + 1))));
                                        }
                                        if (c < board.boards.Count - 1)
                                        {
                                            //1 c
                                            if (t < board.boards[c + 1].Count && board[c + 1, t, x, y] == PIECE.NONE)
                                                res.Add((0, new(pos, new(c + 1, t, x, y))));
                                            //2 c
                                            if (c < board.boards.Count - 2 && t < board.boards[c + 2].Count && board[c + 2, t, x, y] == PIECE.NONE)
                                                res.Add((0, new(pos, new(c + 2, t, x, y))));

                                            //capture c/t
                                            if (t + 2 <= board.boards[c + 1].Count - 1)
                                            {
                                                PIECE p2 = board[c + 1, t + 2, x, y];
                                                if (IsBlackPiece(p2))
                                                    res.Add((pieceToPointValue[board[c + 1, t + 2, x, y]], new(pos, new(c + 1, t + 2, x, y))));
                                            }
                                            if (t - 2 >= 0 && t - 2 <= board.boards[c + 1].Count - 1)
                                            {
                                                PIECE p2 = board[c + 1, t - 2, x, y];
                                                if (IsBlackPiece(p2))
                                                    res.Add((pieceToPointValue[board[c + 1, t - 2, x, y]], new(pos, new(c + 1, t - 2, x, y))));
                                            }
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
                                                if (y == board.boards[c][t].blackPawnStartY && board[c, t, x, y - 2] == PIECE.NONE)
                                                    //2 y
                                                    res.Add((0, new(pos, new(c, t, x, y - 2))));
                                            }
                                            //capture x/y
                                            if (x < board.width - 1 && (IsWhitePiece(board[c, t, x + 1, y - 1]) || board.boards[c][t].enPassantOpportunity == x + 1))
                                                res.Add((pieceToPointValue[board[c, t, x + 1, y - 1]], new(pos, new(c, t, x + 1, y - 1))));
                                            if (x > 0 && (IsWhitePiece(board[c, t, x - 1, y - 1]) || board.boards[c][t].enPassantOpportunity == x - 1))
                                                res.Add((pieceToPointValue[board[c, t, x - 1, y - 1]], new(pos, new(c, t, x - 1, y - 1))));
                                        }
                                        if (c > 0)
                                        {
                                            //1 c
                                            if (t < board.boards[c - 1].Count && board[c - 1, t, x, y] == PIECE.NONE)
                                                res.Add((0, new(pos, new(c - 1, t, x, y))));
                                            //2 c
                                            if (c > 1 && t < board.boards[c - 2].Count && board[c - 2, t, x, y] == PIECE.NONE)
                                                res.Add((0, new(pos, new(c - 2, t, x, y))));

                                            //capture c/t
                                            if (t + 2 <= board.boards[c - 1].Count - 1)
                                            {
                                                PIECE p2 = board[c - 1, t + 2, x, y];
                                                if (IsWhitePiece(p2))
                                                    res.Add((pieceToPointValue[board[c - 1, t + 2, x, y]], new(pos, new(c - 1, t + 2, x, y))));
                                            }
                                            if (t - 2 >= 0 && t - 2 <= board.boards[c - 1].Count - 1)
                                            {
                                                PIECE p2 = board[c - 1, t - 2, x, y];
                                                if (IsWhitePiece(board[c - 1, t - 2, x, y]))
                                                    res.Add((pieceToPointValue[board[c - 1, t - 2, x, y]], new(pos, new(c - 1, t - 2, x, y))));
                                            }
                                        }
                                        break;
                                    }
                                case PIECE.WHITE_ROOK:
                                case PIECE.BLACK_ROOK:
                                    foreach (Point4 item in rookDirs)
                                    {
                                        for (int i = 1; i < 100; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2))
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
                                        if (!IsInBounds(board, pos2))
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
                                        for (int i = 1; i < 100; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2))
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
                                case PIECE.WHITE_UNICORN:
                                case PIECE.BLACK_UNICORN:
                                    foreach (Point4 item in unicornDirs)
                                    {
                                        for (int i = 1; i < 100; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2))
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
                                case PIECE.WHITE_DRAGON:
                                case PIECE.BLACK_DRAGON:
                                    foreach (Point4 item in dragonDirs)
                                    {
                                        for (int i = 1; i < 100; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2))
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
                                        for (int i = 1; i < 100; i++)
                                        {
                                            Point4 pos2 = pos + item * i;
                                            if (!IsInBounds(board, pos2))
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
                                        if (!IsInBounds(board, pos2))
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
