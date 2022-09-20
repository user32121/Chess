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
        public static void PerformMove(Board board, Move move, ref Move lastMove)
        {
            bool specialMove = false;
            int enPassantFlag = -1;

            switch (board.board[move.fromY * 8 + move.fromX])
            {
                //castle
                case PIECE.WHITE_KING:
                    {
                        if (move.fromX == 4 && move.fromY == 7 && move.toY == 7)
                        {
                            if (move.toX == 2)
                            {
                                board.board[7 * 8 + 4] = board.board[7 * 8 + 0] = PIECE.NONE;
                                board.board[7 * 8 + 2] = PIECE.WHITE_KING;
                                board.board[7 * 8 + 3] = PIECE.WHITE_ROOK;
                                specialMove = true;
                            }
                            else if (move.toX == 6)
                            {
                                board.board[7 * 8 + 4] = board.board[7 * 8 + 7] = PIECE.NONE;
                                board.board[7 * 8 + 6] = PIECE.WHITE_KING;
                                board.board[7 * 8 + 5] = PIECE.WHITE_ROOK;
                                specialMove = true;
                            }
                        }

                        board.whiteCanCastleQueenSide = board.whiteCanCastleKingSide = false;
                        break;
                    }
                case PIECE.WHITE_ROOK:
                    {
                        if (move.fromY == 7)
                            if (move.fromX == 0)
                                board.whiteCanCastleQueenSide = false;
                            else if (move.fromX == 7)
                                board.whiteCanCastleKingSide = false;
                        break;
                    }
                case PIECE.BLACK_KING:
                    {
                        if (move.fromX == 4 && move.fromY == 0 && move.toY == 0)
                        {
                            if (move.toX == 2)
                            {
                                board.board[0 * 8 + 4] = board.board[0 * 8 + 0] = PIECE.NONE;
                                board.board[0 * 8 + 2] = PIECE.BLACK_KING;
                                board.board[0 * 8 + 3] = PIECE.BLACK_ROOK;
                                specialMove = true;
                            }
                            else if (move.toX == 6)
                            {
                                board.board[0 * 8 + 4] = board.board[0 * 8 + 7] = PIECE.NONE;
                                board.board[0 * 8 + 6] = PIECE.BLACK_KING;
                                board.board[0 * 8 + 5] = PIECE.BLACK_ROOK;
                                specialMove = true;
                            }
                        }

                        board.blackCanCastleQueenSide = board.blackCanCastleKingSide = false;
                        break;
                    }
                case PIECE.BLACK_ROOK:
                    {
                        if (move.fromY == 7)
                            if (move.fromX == 0)
                                board.blackCanCastleQueenSide = false;
                            else if (move.fromX == 7)
                                board.blackCanCastleKingSide = false;
                        break;
                    }
                //en passant and promotion
                case PIECE.WHITE_PAWN:
                    {
                        if (move.fromY == 6 && move.toY == 4)
                            enPassantFlag = move.fromX;
                        else if (move.toX == board.enPassantAvailable && move.toY == 2)
                        {
                            board.board[move.toY * 8 + move.toX] = board.board[move.fromY * 8 + move.fromX];
                            board.board[move.fromY * 8 + move.fromX] = board.board[move.fromY * 8 + move.toX] = PIECE.NONE;
                            specialMove = true;
                        }
                        else if (move.toY == 0)
                        {
                            PIECE piece;
                            switch (move.dat)
                            {
                                case MOVE_DATA.NONE:
                                    piece = PIECE.NONE;
                                    break;
                                case MOVE_DATA.PROMOTION_QUEEN:
                                    piece = PIECE.WHITE_QUEEN;
                                    break;
                                case MOVE_DATA.PROMOTION_ROOK:
                                    piece = PIECE.WHITE_ROOK;
                                    break;
                                case MOVE_DATA.PROMOTION_BISHOP:
                                    piece = PIECE.WHITE_BISHOP;
                                    break;
                                case MOVE_DATA.PROMOTION_KNIGHT:
                                    piece = PIECE.WHITE_KNIGHT;
                                    break;
                                default:
                                    throw new NotImplementedException(move.dat.ToString());
                            }
                            board.board[move.toY * 8 + move.toX] = piece;
                            board.board[move.fromY * 8 + move.fromX] = PIECE.NONE;
                            specialMove = true;
                        }
                        break;
                    }
                case PIECE.BLACK_PAWN:
                    {
                        if (move.fromY == 1 && move.toY == 3)
                            enPassantFlag = move.fromX;
                        else if (move.toX == board.enPassantAvailable && move.toY == 5)
                        {
                            board.board[move.toY * 8 + move.toX] = board.board[move.fromY * 8 + move.fromX];
                            board.board[move.fromY * 8 + move.fromX] = board.board[move.fromY * 8 + move.toX] = PIECE.NONE;
                            specialMove = true;
                        }
                        else if (move.toY == 7)
                        {
                            PIECE piece;
                            switch (move.dat)
                            {
                                case MOVE_DATA.NONE:
                                    piece = PIECE.NONE;
                                    break;
                                case MOVE_DATA.PROMOTION_QUEEN:
                                    piece = PIECE.BLACK_QUEEN;
                                    break;
                                case MOVE_DATA.PROMOTION_ROOK:
                                    piece = PIECE.BLACK_ROOK;
                                    break;
                                case MOVE_DATA.PROMOTION_BISHOP:
                                    piece = PIECE.BLACK_BISHOP;
                                    break;
                                case MOVE_DATA.PROMOTION_KNIGHT:
                                    piece = PIECE.BLACK_KNIGHT;
                                    break;
                                default:
                                    throw new NotImplementedException(move.dat.ToString());
                            }
                            board.board[move.toY * 8 + move.toX] = piece;
                            board.board[move.fromY * 8 + move.fromX] = PIECE.NONE;
                            specialMove = true;
                        }
                        break;
                    }
            }

            board.enPassantAvailable = enPassantFlag;

            if (!specialMove)
            {
                board.board[move.toY * 8 + move.toX] = board.board[move.fromY * 8 + move.fromX];
                board.board[move.fromY * 8 + move.fromX] = PIECE.NONE;
            }
            lastMove = move;
        }

        public static bool isWithinBounds(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        public static bool isEmpty(Board board, int x, int y)
        {
            return board.board[y * 8 + x] == PIECE.NONE;
        }
        public static bool isWhitePiece(Board board, int x, int y)
        {
            PIECE piece = board.board[y * 8 + x];
            return piece >= PIECE.WHITE_PAWN && piece <= PIECE.WHITE_KING;
        }
        public static bool isBlackPiece(Board board, int x, int y)
        {
            PIECE piece = board.board[y * 8 + x];
            return piece >= PIECE.BLACK_PAWN && piece <= PIECE.BLACK_KING;
        }
        public static bool isTileUnderAttack(Board board, int x, int y)
        {
            return false;
        }

        public static bool IsValidMove(Board board, Move move, bool isWhiteTurn)
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

            switch (board.board[move.fromY * 8 + move.fromX])
            {
                case PIECE.WHITE_PAWN:
                    {
                        if (move.toY == 0 && move.dat == MOVE_DATA.NONE)
                            return false;

                        switch (move.toY - move.fromY)
                        {
                            case -1:
                                int dx = move.toX - move.fromX;
                                bool candidateTile;
                                if (dx == 0)
                                    candidateTile = isEmpty(board, move.toX, move.toY);
                                else if (Math.Abs(dx) == 1)
                                    candidateTile = isBlackPiece(board, move.toX, move.toY) || board.enPassantAvailable == move.toX;
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
                        //castle
                        if (move.fromX == 4 && move.fromY == 7 && move.toY == 7)
                        {
                            if (move.toX == 2)
                            {
                                if (board.whiteCanCastleQueenSide &&
                                        isEmpty(board, 1, 7) && isEmpty(board, 2, 7) && isEmpty(board, 3, 7) &&
                                        !isTileUnderAttack(board, 4, 7) && !isTileUnderAttack(board, 3, 7) && !isTileUnderAttack(board, 2, 7))
                                    return true;
                                else
                                    return false;
                            }
                            else if (move.toX == 6)
                            {
                                if (board.whiteCanCastleKingSide &&
                                        isEmpty(board, 5, 7) && isEmpty(board, 6, 7) &&
                                        !isTileUnderAttack(board, 4, 7) && !isTileUnderAttack(board, 5, 7) && !isTileUnderAttack(board, 6, 7))
                                    return true;
                                else
                                    return false;
                            }
                        }

                        //normal move
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1 || isWhitePiece(board, move.toX, move.toY))
                            return false;
                        else
                            return true;
                    }
                case PIECE.BLACK_PAWN:
                    {
                        if (move.toY == 7 && move.dat == MOVE_DATA.NONE)
                            return false;

                        switch (move.toY - move.fromY)
                        {
                            case 1:
                                int dx = move.toX - move.fromX;
                                bool candidateTile;
                                if (dx == 0)
                                    candidateTile = isEmpty(board, move.toX, move.toY);
                                else if (Math.Abs(dx) == 1)
                                    candidateTile = isWhitePiece(board, move.toX, move.toY) || board.enPassantAvailable == move.toX;
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
                        //castle
                        if (move.fromX == 4 && move.fromY == 0 && move.toY == 0)
                        {
                            if (move.toX == 2)
                            {
                                if (board.blackCanCastleQueenSide &&
                                        isEmpty(board, 1, 0) && isEmpty(board, 2, 0) && isEmpty(board, 3, 0) &&
                                        !isTileUnderAttack(board, 4, 0) && !isTileUnderAttack(board, 3, 0) && !isTileUnderAttack(board, 2, 0))
                                    return true;
                                else
                                    return false;
                            }
                            else if (move.toX == 6)
                            {
                                if (board.blackCanCastleKingSide &&
                                        isEmpty(board, 5, 0) && isEmpty(board, 6, 0) &&
                                        !isTileUnderAttack(board, 4, 0) && !isTileUnderAttack(board, 5, 0) && !isTileUnderAttack(board, 6, 0))
                                    return true;
                                else
                                    return false;
                            }
                        }

                        //normal move
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
        public static bool IsValidPieceMove(Board board, Move move)
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

            switch (board.board[move.fromY * 8 + move.fromX])
            {
                case PIECE.WHITE_PAWN:
                    {
                        switch (move.toY - move.fromY)
                        {
                            case -1:
                                int dx = move.toX - move.fromX;
                                if (Math.Abs(dx) <= 1)
                                    return true;
                                else
                                    return false;
                            case -2:
                                if (move.fromY == 6 && move.fromX - move.toX == 0)
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
                        else
                            return true;
                    }
                case PIECE.WHITE_KNIGHT:
                    {
                        int dx = Math.Abs(move.toX - move.fromX);
                        int dy = Math.Abs(move.toY - move.fromY);
                        if ((dx == 1 || dy == 1) && dx + dy == 3)
                            return true;
                        else
                            return false;
                    }
                case PIECE.WHITE_BISHOP:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy))
                            return false;
                        else
                            return true;
                    }
                case PIECE.WHITE_QUEEN:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy) && dx != 0 && dy != 0)
                            return false;
                        else
                            return true;
                    }
                case PIECE.WHITE_KING:
                    {
                        //castle
                        if (move.fromX == 4 && move.fromY == 7 && move.toY == 7)
                        {
                            if (move.toX == 2)
                                return board.whiteCanCastleQueenSide;
                            else if (move.toX == 6)
                                return board.whiteCanCastleKingSide;
                        }

                        //normal move
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
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
                                if (Math.Abs(dx) <= 1)
                                    return true;
                                else
                                    return false;
                            case 2:
                                if (move.fromY == 1 && move.fromX - move.toX == 0)
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
                        else
                            return true;
                    }
                case PIECE.BLACK_KNIGHT:
                    {
                        int dx = Math.Abs(move.toX - move.fromX);
                        int dy = Math.Abs(move.toY - move.fromY);
                        if ((dx == 1 || dy == 1) && dx + dy == 3)
                            return true;
                        else
                            return false;
                    }
                case PIECE.BLACK_BISHOP:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy))
                            return false;
                        else
                            return true;
                    }
                case PIECE.BLACK_QUEEN:
                    {
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) != Math.Abs(dy) && dx != 0 && dy != 0)
                            return false;
                        else
                            return true;
                    }
                case PIECE.BLACK_KING:
                    {
                        //castle
                        if (move.fromX == 4 && move.fromY == 0 && move.toY == 0)
                        {
                            if (move.toX == 2)
                                return board.blackCanCastleQueenSide;
                            else if (move.toX == 6)
                                return board.blackCanCastleKingSide;
                        }

                        //normal move
                        int dx = move.toX - move.fromX;
                        int dy = move.toY - move.fromY;
                        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
                            return false;
                        else
                            return true;
                    }
                default:
                    return false;
            }
        }

        public static string MoveToString(Move move)
        {
            if (isWithinBounds(move.fromX, move.fromY) && isWithinBounds(move.toX, move.toY))
                return string.Format("{0}{1} > {2}{3}", (char)(move.fromX + 'a'), (char)(8 - move.fromY + '0'), (char)(move.toX + 'a'), (char)(8 - move.toY + '0'));
            else
                return "out of bounds move";
        }

        public static int GetScore(Board board)
        {
            const int PAWN_VALUE = 1,
                KNIGHT_VALUE = 3,
                BISHOP_VALUE = 3,
                ROOK_VALUE = 5,
                QUEEN_VALUE = 9,
                WIN_VALUE = 1000;

            int score = 0;
            bool hasWhiteKing = false, hasBlackKing = false;
            for (int i = 0; i < 64; i++)
            {
                switch (board.board[i])
                {
                    case PIECE.WHITE_PAWN:
                        score += PAWN_VALUE;
                        break;
                    case PIECE.WHITE_ROOK:
                        score += ROOK_VALUE;
                        break;
                    case PIECE.WHITE_KNIGHT:
                        score += KNIGHT_VALUE;
                        break;
                    case PIECE.WHITE_BISHOP:
                        score += BISHOP_VALUE;
                        break;
                    case PIECE.WHITE_QUEEN:
                        score += QUEEN_VALUE;
                        break;
                    case PIECE.WHITE_KING:
                        hasWhiteKing = true;
                        break;
                    case PIECE.BLACK_PAWN:
                        score -= PAWN_VALUE;
                        break;
                    case PIECE.BLACK_ROOK:
                        score -= ROOK_VALUE;
                        break;
                    case PIECE.BLACK_KNIGHT:
                        score -= KNIGHT_VALUE;
                        break;
                    case PIECE.BLACK_BISHOP:
                        score -= BISHOP_VALUE;
                        break;
                    case PIECE.BLACK_QUEEN:
                        score -= QUEEN_VALUE;
                        break;
                    case PIECE.BLACK_KING:
                        hasBlackKing = true;
                        break;
                    default:
                        break;
                }
            }

            if (!hasWhiteKing)
                score -= WIN_VALUE;
            if (!hasBlackKing)
                score += WIN_VALUE;

            return score;
        }
    }
}
