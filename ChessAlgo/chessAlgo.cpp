#include "pch.h"
#include "chessAlgo.h"

using namespace std;

int test() {
    return 5;
}


Move findBestMove(PIECE board[], const Move& lastMove, int maxDepth) {
    return findBestMove(board, lastMove, true, maxDepth);
}

int getBoardScore(PIECE board[], const Move& lastMove, bool myTurn, int maxDepth) {
    if (maxDepth == 0) {
        int score = 0;
        bool hasMyKing = false, hasOpponentKing = false;
        for (size_t i = 0; i < 64; i++)
        {
            switch (board[i])
            {
            case PIECE::PLAYER_PAWN:
                score += PAWN_VALUE;
                break;
            case PIECE::PLAYER_ROOK:
                score += ROOK_VALUE;
                break;
            case PIECE::PLAYER_KNIGHT:
                score += KNIGHT_VALUE;
                break;
            case PIECE::PLAYER_BISHOP:
                score += BISHOP_VALUE;
                break;
            case PIECE::PLAYER_QUEEN:
                score += QUEEN_VALUE;
                break;
            case PIECE::PLAYER_KING:
                hasMyKing = true;
                break;
            case PIECE::OPPONENT_PAWN:
                score -= PAWN_VALUE;
                break;
            case PIECE::OPPONENT_ROOK:
                score -= ROOK_VALUE;
                break;
            case PIECE::OPPONENT_KNIGHT:
                score -= KNIGHT_VALUE;
                break;
            case PIECE::OPPONENT_BISHOP:
                score -= BISHOP_VALUE;
                break;
            case PIECE::OPPONENT_QUEEN:
                score -= QUEEN_VALUE;
                break;
            case PIECE::OPPONENT_KING:
                hasOpponentKing = true;
                break;
            default:
                break;
            }
        }
        if (myTurn)
        {
            if (!hasMyKing)
                score -= LOSE_VALUE;
            if (!hasOpponentKing)
                score += WIN_VALUE;
        }
        else
        {
            if (!hasMyKing)
                score -= LOSE_VALUE;
            if (!hasOpponentKing)
                score += WIN_VALUE;
        }
        return score;
    }
    else
    {
        Move move = findBestMove(board, lastMove, myTurn, maxDepth);
        return getBoardScoreAfterMove(board, move, myTurn, maxDepth);
    }

    return 0;
}

int getBoardScoreAfterMove(PIECE board[], const Move& move, bool myTurn, int maxDepth) {
    PIECE prevPiece = board[move.toY * 8 + move.toX];
    board[move.toY * 8 + move.toX] = board[move.fromY * 8 + move.fromX];
    board[move.fromY * 8 + move.fromX] = PIECE::NONE;
    int score = getBoardScore(board, move, !myTurn, maxDepth - 1);

    board[move.fromY * 8 + move.fromX] = board[move.toY * 8 + move.toX];
    board[move.toY * 8 + move.toX] = prevPiece;
    return score;
}

void inline setBest(PIECE board[], bool myTurn, int maxDepth, int& bestScore, Move& bestMove, Move newMove) {
    int newScore = getBoardScoreAfterMove(board, newMove, myTurn, maxDepth);
    if (myTurn)
    {
        if (newScore > bestScore)
        {
            bestScore = newScore;
            bestMove = newMove;
        }
    }
    else
    {
        if (newScore < bestScore)
        {
            bestScore = newScore;
            bestMove = newMove;
        }
    }
}

Move findBestMove(PIECE board[], const Move& lastMove, bool myTurn, int maxDepth) {
    if (myTurn)
    {
        Move bestMove{}, move;
        int bestScore = INT_MIN, score;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                switch (board[y * 8 + x])
                {
                case PIECE::PLAYER_PAWN:
                    //move
                    if (isWithinBounds(x, y - 1) && isEmpty(board, x, y - 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y - 1 });
                    //double move
                    if (y == 6 && isEmpty(board, x, y - 1) && isEmpty(board, x, y - 2))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y - 2 });
                    //capture left
                    if (isWithinBounds(x - 1, y - 1) && isOpponentPiece(board, x - 1, y - 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x - 1, y - 1 });
                    //capture right
                    if (isWithinBounds(x + 1, y - 1) && isOpponentPiece(board, x + 1, y - 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + 1, y - 1 });
                    break;
                case PIECE::PLAYER_ROOK:
                    for (size_t i = 0; i < 4; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * rookDirsX[i], y + d * rookDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * rookDirsX[i], y + d * rookDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * rookDirsX[i], y + d * rookDirsY[i] });
                            else if (isOpponentPiece(board, x + d * rookDirsX[i], y + d * rookDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * rookDirsX[i], y + d * rookDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::PLAYER_KNIGHT:
                    for (size_t i = 0; i < 8; i++)
                    {
                        if (x + knightMovesX[i] >= 0 && x + knightMovesX[i] < 8 && y + knightMovesY[i] >= 0 && y + knightMovesY[i] < 8 &&
                            (isEmpty(board, x + knightMovesX[i], y + knightMovesY[i]) || isOpponentPiece(board, x + knightMovesX[i], y + knightMovesY[i])))
                        {
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + knightMovesX[i], y + knightMovesY[i] });
                        }
                    }
                    break;
                case PIECE::PLAYER_BISHOP:
                    for (size_t i = 0; i < 4; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * bishopDirsX[i], y + d * bishopDirsY[i] });
                            else if (isOpponentPiece(board, x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * bishopDirsX[i], y + d * bishopDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::PLAYER_QUEEN:
                    for (size_t i = 0; i < 8; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * queenDirsX[i], y + d * queenDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * queenDirsX[i], y + d * queenDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * queenDirsX[i], y + d * queenDirsY[i] });
                            else if (isOpponentPiece(board, x + d * queenDirsX[i], y + d * queenDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * queenDirsX[i], y + d * queenDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::PLAYER_KING:
                    for (size_t i = 0; i < 8; i++)
                    {
                        if (!isWithinBounds(x + queenDirsX[i], y + queenDirsY[i]))
                            continue;
                        if (isEmpty(board, x + queenDirsX[i], y + queenDirsY[i]))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + queenDirsX[i], y + queenDirsY[i] });
                        else if (isOpponentPiece(board, x + queenDirsX[i], y + queenDirsY[i]))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + queenDirsX[i], y + queenDirsY[i] });
                    }
                    break;
                default:
                    break;
                }
            }
        return bestMove;
    }
    else
    {
        Move bestMove, move;
        int bestScore = INT_MAX, score;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                switch (board[y * 8 + x])
                {
                case PIECE::OPPONENT_PAWN:
                    //move
                    if (isWithinBounds(x, y + 1) && isEmpty(board, x, y + 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y + 1 });
                    //double move
                    if (y == 1 && isEmpty(board, x, y + 1) && isEmpty(board, x, y + 2))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y + 2 });
                    //capture left
                    if (isWithinBounds(x - 1, y + 1) && isMyPiece(board, x - 1, y + 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x - 1, y + 1 });
                    //capture right
                    if (isWithinBounds(x + 1, y + 1) && isMyPiece(board, x + 1, y + 1))
                        setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + 1, y + 1 });
                    break;
                case PIECE::OPPONENT_ROOK:
                    for (size_t i = 0; i < 4; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * rookDirsX[i], y + d * rookDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * rookDirsX[i], y + d * rookDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * rookDirsX[i], y + d * rookDirsY[i] });
                            else if (isMyPiece(board, x + d * rookDirsX[i], y + d * rookDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * rookDirsX[i], y + d * rookDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::OPPONENT_KNIGHT:
                    for (size_t i = 0; i < 8; i++)
                    {
                        if (x + knightMovesX[i] >= 0 && x + knightMovesX[i] < 8 && y + knightMovesY[i] >= 0 && y + knightMovesY[i] < 8 &&
                            (isEmpty(board, x + knightMovesX[i], y + knightMovesY[i]) || isMyPiece(board, x + knightMovesX[i], y + knightMovesY[i])))
                        {
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + knightMovesX[i], y + knightMovesY[i] });
                        }
                    }
                    break;
                case PIECE::OPPONENT_BISHOP:
                    for (size_t i = 0; i < 4; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * bishopDirsX[i], y + d * bishopDirsY[i] });
                            else if (isMyPiece(board, x + d * bishopDirsX[i], y + d * bishopDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * bishopDirsX[i], y + d * bishopDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::OPPONENT_QUEEN:
                    for (size_t i = 0; i < 8; i++)
                    {
                        for (int d = 1; d < 8; d++)
                        {
                            if (!isWithinBounds(x + d * queenDirsX[i], y + d * queenDirsY[i]))
                                break;
                            if (isEmpty(board, x + d * queenDirsX[i], y + d * queenDirsY[i]))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * queenDirsX[i], y + d * queenDirsY[i] });
                            else if (isMyPiece(board, x + d * queenDirsX[i], y + d * queenDirsY[i]))
                            {
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + d * queenDirsX[i], y + d * queenDirsY[i] });
                                break;
                            }
                            else
                                break;
                        }
                    }
                    break;
                case PIECE::OPPONENT_KING:
                    for (size_t i = 0; i < 8; i++)
                    {
                        if (!isWithinBounds(x + queenDirsX[i], y + queenDirsY[i]))
                            continue;
                        if (isEmpty(board, x + queenDirsX[i], y + queenDirsY[i]))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + queenDirsX[i], y + queenDirsY[i] });
                        else if (isMyPiece(board, x + queenDirsX[i], y + queenDirsY[i]))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + queenDirsX[i], y + queenDirsY[i] });
                    }
                    break;
                default:
                    break;
                }
            }
        return bestMove;
    }
}

void printBoard(PIECE board[]) {
    cout << endl;
    for (int y = 0; y < 8; y++)
    {
        for (int x = 0; x < 8; x++)
            cout << pieceToChar[board[y * 8 + x]] << ' ';
        cout << endl;
    }
}
