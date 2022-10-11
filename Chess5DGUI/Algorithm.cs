using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess5DGUI
{
    internal class Algorithm
    {
        public static (Move, float) GetBestMove(GameBoard board, bool optimizingWhite, int maxDepth, ref bool earlyExit)
        {
            List<Move> moves = Utils.GetAllMoves(board);

            if (optimizingWhite)
            {
                float bestScore = float.MinValue;
                int bestMoveIndex = -1;
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].from.t % 2 == 0)
                    {
                        float score = GetScoreAfterMove(board, moves[i], maxDepth, ref earlyExit);
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMoveIndex = i;
                        }
                    }
                }
                if (bestMoveIndex == -1)
                    return (Move.Invalid, 0);
                return (moves[bestMoveIndex], bestScore);
            }
            else
            {
                float bestScore = float.MaxValue;
                int bestMoveIndex = -1;
                for (int i = 0; i < moves.Count; i++)
                {
                    if (moves[i].from.t % 2 == 1)
                    {
                        float score = GetScoreAfterMove(board, moves[i], maxDepth, ref earlyExit);
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMoveIndex = i;
                        }
                    }
                }
                if (bestMoveIndex == -1)
                    return (Move.Invalid, 0);
                return (moves[bestMoveIndex], bestScore);
            }
        }

        private static float GetScoreAfterMove(GameBoard board, Move move, int maxDepth, ref bool earlyExit)
        {
            if (board[move.to] == PIECE.WHITE_KING || board[move.to] == PIECE.BLACK_KING)
                return -Utils.pieceToPointValue[board[move.to]];

            bool removeNewRow = false;
            if (move.from.t == move.to.t && move.from.c == move.to.c)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else if (move.from.t == move.to.t && move.to.t == board.boards[move.to.c].Count - 1)
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                board.boards[move.to.c].Add((PIECE[,])board.boards[move.to.c][move.to.t].Clone());
                move.from.t++;
                move.to.t++;
            }
            else
            {
                board.boards[move.from.c].Add((PIECE[,])board.boards[move.from.c][move.from.t].Clone());
                if (Utils.IsWhitePiece(board[move.from]))
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
                    removeNewRow = true;
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
                    removeNewRow = true;
                }
            }
            board[move.to] = board[move.from];
            board[move.from] = PIECE.NONE;

            int minTurn = board.boards.Min(timeline => timeline.Count);
            float score = GetScore(board, minTurn % 2 == 1, maxDepth - 1, ref earlyExit);

            board.boards[move.from.c].RemoveAt(move.from.t);
            if (move.from.c != move.to.c || move.from.t != move.to.t)
                board.boards[move.to.c].RemoveAt(move.to.t);
            if (removeNewRow)
                board.boards.RemoveAt(move.to.c);

            return score;
        }

        public static float GetScore(GameBoard board, bool optimizingWhite, int maxDepth, ref bool earlyExit)
        {
            if (earlyExit)
                return GetStaticScore(board);
            if (maxDepth <= 0)
                return GetStaticScore(board);
            else
            {
                (_, float score) = GetBestMove(board, optimizingWhite, maxDepth, ref earlyExit);
                return score;
            }
        }

        private static float GetStaticScore(GameBoard board)
        {
            float score = 0;
            int boards = 0;
            for (int c = 0; c < board.boards.Count; c++)
            {
                int t = board.boards[c].Count - 1;
                boards++;
                for (int x = 0; x < 8; x++)
                    for (int y = 0; y < 8; y++)
                        score += Utils.pieceToPointValue[board[c, t, x, y]];
            }
            return score / boards;
        }
    }
}
