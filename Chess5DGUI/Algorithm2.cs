using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Chess5DGUI.GameBoard;

namespace Chess5DGUI
{
    internal class Algorithm2
    {
        public static (Move, float) GetBestMove(GameBoard board, bool optimizingWhite, int maxDepth, ref bool earlyExit)
        {
            return AlphaBeta(board, optimizingWhite, maxDepth, float.NegativeInfinity, float.PositiveInfinity, ref earlyExit);
        }

        //based on fail-soft alpha-beta from https://en.wikipedia.org/wiki/Alpha%E2%80%93beta_pruning
        private static (Move, float) AlphaBeta(GameBoard board, bool optimizingWhite, int maxDepth, float a, float b, ref bool earlyExit)
        {
            List<Move> moves = Utils.GetAllMoves(board);

            if (optimizingWhite)
            {
                float value = float.NegativeInfinity;
                int moveIndex = -1;

                for (int i = 0; i < moves.Count; i++)
                {
                    float nodeValue = GetScoreAfterMove(board, moves[i], maxDepth, a, b, ref earlyExit);
                    if (nodeValue > value)
                    {
                        value = nodeValue;
                        moveIndex = i;
                    }
                    a = Math.Max(a, value);
                    if (value >= b)
                        break;
                }
                if (moveIndex == -1)
                    return (Move.Invalid, 0);
                return (moves[moveIndex], value);
            }
            else
            {
                float value = float.PositiveInfinity;
                int moveIndex = -1;

                for (int i = 0; i < moves.Count; i++)
                {
                    float nodeValue = GetScoreAfterMove(board, moves[i], maxDepth, a, b, ref earlyExit);
                    if (nodeValue < value)
                    {
                        value = nodeValue;
                        moveIndex = i;
                    }
                    a = Math.Min(a, value);
                    if (value >= b)
                        break;
                }
                if (moveIndex == -1)
                    return (Move.Invalid, 0);
                return (moves[moveIndex], value);
            }
        }

        private static float GetScoreAfterMove(GameBoard board, Move move, int maxDepth, float a, float b, ref bool earlyExit)
        {
            if (board[move.to] == PIECE.WHITE_KING)
                return -Utils.WIN_VALUE;
            if (board[move.to] == PIECE.BLACK_KING)
                return Utils.WIN_VALUE;

            int nextEnPassant = -1;
            bool doEnPassant = false;
            bool removeNewRow = false;
            if (move.from.t == move.to.t && move.from.c == move.to.c)
            {
                if (Utils.IsSamePiece(board[move.from], PIECE.WHITE_PAWN))
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
                if (Utils.IsWhitePiece(board[move.from]))
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
                    removeNewRow = true;
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
                    removeNewRow = true;
                }
            }
            int kingCaptures = 0;
            if (board[move.to] == PIECE.WHITE_KING)
                kingCaptures++;
            else if (board[move.to] == PIECE.BLACK_KING)
                kingCaptures--;
            board.whiteKingCaptured += kingCaptures;

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

            int minTurn = board.boards.Where((timeline, c) => c >= board.timelinesByWhite - 1 && c <= board.boards.Count + board.timelinesByWhite).Min(timeline => timeline.Count);
            float score = GetScore(board, minTurn % 2 == 1, maxDepth - 1, a, b, ref earlyExit);

            board.boards[move.from.c].RemoveAt(move.from.t);
            if (move.from.c != move.to.c || move.from.t != move.to.t)
                board.boards[move.to.c].RemoveAt(move.to.t);
            if (removeNewRow)
            {
                board.boards.RemoveAt(move.to.c);
                if (move.to.c == 0)
                    board.timelinesByWhite--;
                else
                    board.timelinesByWhite++;
            }
            board.whiteKingCaptured -= kingCaptures;

            return score;
        }

        public static float GetScore(GameBoard board, bool optimizingWhite, int maxDepth, float a, float b, ref bool earlyExit)
        {
            if (earlyExit)
                return GetStaticScore(board);
            if (maxDepth <= 0)
                return GetStaticScore(board);
            else
            {
                (_, float score) = AlphaBeta(board, optimizingWhite, maxDepth, a, b, ref earlyExit);
                return score;
            }
        }

        public static float GetStaticScore(GameBoard board)
        {
            float score = 0;
            int boards = 0;
            for (int c = 0; c < board.boards.Count; c++)
            {
                int t = board.boards[c].Count - 1;
                boards++;
                for (int x = 0; x < board.width; x++)
                    for (int y = 0; y < board.height; y++)
                        score += Utils.pieceToPointValue[board[c, t, x, y]];
            }
            score /= boards;
            score -= board.timelinesByWhite * 10;
            score -= board.whiteKingCaptured * Utils.WIN_VALUE;
            return score;
        }
    }
}
