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

    public class Board
    {
        public List<List<PIECE[,]>> boards;

        public PIECE this[Point4 p]
        {
            get { return boards[p.c][p.t][p.x, p.y]; }
            set { boards[p.c][p.t][p.x, p.y] = value; }
        }

        public static PIECE[,] getStartingBoard() => new PIECE[,]
        {
            { PIECE.WHITE_ROOK, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_ROOK },
            { PIECE.WHITE_KNIGHT, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KNIGHT },
            { PIECE.WHITE_BISHOP, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_BISHOP },
            { PIECE.WHITE_KING, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KING },
            { PIECE.WHITE_QUEEN, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_QUEEN },
            { PIECE.WHITE_BISHOP, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_BISHOP },
            { PIECE.WHITE_KNIGHT, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_KNIGHT },
            { PIECE.WHITE_ROOK, PIECE.WHITE_PAWN, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_PAWN, PIECE.BLACK_ROOK },
        };
    }

    public struct Point4 : IEquatable<Point4>
    {
        public int x, y, t, c;  //x,y,time,choice

        public Point4(int x, int y, int t, int c)
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
    }

    public struct Move
    {
        public Point4 from, to;

        public Move(Point4 from, Point4 to)
        {
            this.from = from;
            this.to = to;
        }
    }

    public static class Utils
    {
        public static bool PerformMove(Board board, Move move, ref bool isWhiteTurn, ref Move prevMove, ref Vector2 viewOffset)
        {
            if (move.from == move.to || move.from.t != board.boards[move.from.c].Count - 1)
                return false;
            if (IsWhitePiece(board[move.from]) ^ isWhiteTurn)
                return false;

            if (move.from.t == move.to.t && move.from.c == move.to.c)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                move.from.t++;
                move.to.t++;
                viewOffset.X += 9 * Game1.pieceDrawSize;
            }
            else if (move.to.t < move.from.t && move.from.c == move.to.c)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                if (isWhiteTurn)
                {
                    List<PIECE[,]> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Insert(0, newRow);
                    move.from.c++;
                    move.to.c = 0;
                    move.to.t++;
                    viewOffset.Y -= 9 * Game1.pieceDrawSize;
                }
                else
                {
                    List<PIECE[,]> newRow = new();
                    for (int i = 0; i < move.to.t + 1; i++)
                        newRow.Add(null);
                    newRow.Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                    board.boards.Add(newRow);
                    move.to.c = board.boards.Count - 1;
                    move.to.t++;
                    viewOffset.Y += 9 * Game1.pieceDrawSize;
                }
            }
            board[move.to] = board[move.from];
            board[move.from] = PIECE.NONE;
            isWhiteTurn = !isWhiteTurn;

            prevMove = move;

            return true;
        }

        public static bool IsWhitePiece(PIECE p)
        {
            return p >= PIECE.WHITE_PAWN && p <= PIECE.WHITE_KING;
        }
        public static bool IsBlackPiece(PIECE p)
        {
            return p >= PIECE.BLACK_PAWN && p <= PIECE.BLACK_KING;
        }

        public static Vector2 ScreenToWorldSpace(Vector2 p, Game g, Vector2 offset, float zoom)
        {
            p -= g.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2;
            p /= zoom;
            p += g.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2;
            p += offset;
            return p;
        }
    }
}
