using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chess.DllWrapper;

namespace ChessGUI
{
    internal class Utils
    {
        public static void PerformMove(PIECE[] board, Move move, ref Move lastMove)
        {
            board[move.toY * 8 + move.toX] = board[move.fromY * 8 + move.fromX];
            board[move.fromY * 8 + move.fromX] = PIECE.NONE;
            lastMove = move;
        }

        public static bool isWithinBounds(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        public static bool isEmpty(PIECE[] board, int x, int y)
        {
            return board[y * 8 + x] == PIECE.NONE;
        }
        public static bool isWhitePiece(PIECE[] board, int x, int y)
        {
            PIECE piece = board[y * 8 + x];
            return piece >= PIECE.WHITE_PAWN && piece <= PIECE.WHITE_KING;
        }
        public static bool isBlackPiece(PIECE[] board, int x, int y)
        {
            PIECE piece = board[y * 8 + x];
            return piece >= PIECE.BLACK_PAWN && piece <= PIECE.BLACK_KING;
        }

        public static bool IsValidMove(PIECE[] board, Move move, bool isWhiteTurn)
        {
            //cannot move to own tile
            if (move.fromX == move.toX && move.fromY == move.toY)
                return false;

            //must be within bounds
            if (!isWithinBounds(move.fromX, move.fromY) || !isWithinBounds(move.toX, move.toY))
                return false;

            //cannot move from empty tile
            if (isEmpty(board, move.fromX, move.fromY))
                return false;

            //must move white piece on white turn
            if (isWhitePiece(board, move.fromX, move.fromY) ^ isWhiteTurn)
                return false;

            switch (board[move.fromY * 8 + move.fromX])
            {
                case PIECE.WHITE_PAWN:
                    {
                        switch (move.toY - move.fromY)
                        {
                            case -1:
                                int dx = move.toX - move.fromX;
                                bool candidateTile;
                                if (dx == 0)
                                    candidateTile = isEmpty(board, move.toX, move.toY);
                                else if (Math.Abs(dx) == 1)
                                    candidateTile = isBlackPiece(board, move.toX, move.toY);
                                else
                                    return false;
                                if (candidateTile)
                                    return true;
                                else
                                    return false;
                            case -2:
                                if (move.fromY == 6 && move.fromX - move.toX == 0 && isEmpty(board, move.toX, 5) && isEmpty(board, move.toX, 4))
                                    return true;
                                else
                                    return false;
                            default:
                                return false;
                        }
                    }
                case PIECE.WHITE_ROOK:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (dx != 0 && dy != 0)
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isBlackPiece(board, x, y))
                                return true;
                            else if (isWhitePiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.WHITE_KNIGHT:
                    {
                        int dx = Math.Abs(move.toX - move.fromX);
                        int dy = Math.Abs(move.toY - move.fromY);
                        if ((dx == 1 || dy == 1) && dx + dy == 3)
                            return !isWhitePiece(board, move.toX, move.toY);
                        else
                            return false;
                    }
                case PIECE.WHITE_BISHOP:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy))
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isBlackPiece(board, x, y))
                                return true;
                            else if (isWhitePiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.WHITE_QUEEN:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy) && dx != 0 && dy != 0)
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isBlackPiece(board, x, y))
                                return true;
                            else if (isWhitePiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.WHITE_KING:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || isWhitePiece(board, move.toX, move.toY))
                            return false;
                        else
                            return true;
                    }
                case PIECE.BLACK_PAWN:
                    {
                        switch (move.toY - move.fromY)
                        {
                            case 1:
                                int dx = move.toX - move.fromX;
                                bool candidateTile;
                                if (dx == 0)
                                    candidateTile = isEmpty(board, move.toX, move.toY);
                                else if (Math.Abs(dx) == 1)
                                    candidateTile = isWhitePiece(board, move.toX, move.toY);
                                else
                                    return false;
                                if (candidateTile)
                                    return true;
                                else
                                    return false;
                            case 2:
                                if (move.fromY == 1 && move.fromX - move.toX == 0 && isEmpty(board, move.toX, 2) && isEmpty(board, move.toX, 3))
                                    return true;
                                else
                                    return false;
                            default:
                                return false;
                        }
                    }
                case PIECE.BLACK_ROOK:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (dx != 0 && dy != 0)
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isWhitePiece(board, x, y))
                                return true;
                            else if (isBlackPiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.BLACK_KNIGHT:
                    {
                        int dx = Math.Abs(move.toX - move.fromX);
                        int dy = Math.Abs(move.toY - move.fromY);
                        if ((dx == 1 || dy == 1) && dx + dy == 3)
                            return !isBlackPiece(board, move.toX, move.toY);
                        else
                            return false;
                    }
                case PIECE.BLACK_BISHOP:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy))
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isWhitePiece(board, x, y))
                                return true;
                            else if (isBlackPiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.BLACK_QUEEN:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy) && dx != 0 && dy != 0)
                            return false;
                        dx = Math.Sign(dx);
                        dy = Math.Sign(dy);
                        int x = move.fromX;
                        int y = move.fromY;
                        while (x != move.toX || y != move.toY)
                        {
                            x += dx;
                            y += dy;
                            if (isWhitePiece(board, x, y))
                                return true;
                            else if (isBlackPiece(board, x, y))
                                return false;
                        }
                        return true;
                    }
                case PIECE.BLACK_KING:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || isBlackPiece(board, move.toX, move.toY))
                            return false;
                        else
                            return true;
                    }
                default:
                    return false;
            }
        }
    }
}
