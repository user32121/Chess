#include "pch.h"
#include "chessAlgo.h"

using namespace std;

int test() {
    return 5;
}



bool algoRunning = false;
chrono::steady_clock::time_point algoStartTime;
chrono::milliseconds algoMaxTime;

bool isOutOfTime() {
    return chrono::high_resolution_clock::now() - algoStartTime > algoMaxTime;
}

Move apiFindBestMove(Board board, int maxDepth, long long maxTime, int* scoreAfterMove) {
    if (maxDepth <= 0 || algoRunning) {
        return { -1 };
    }
    algoRunning = true;
    algoMaxTime = chrono::milliseconds(maxTime);
    algoStartTime = chrono::high_resolution_clock::now();
    Move move = findBestMove(&board, true, maxDepth, scoreAfterMove);
    algoRunning = false;
    if (isOutOfTime())
        return { -1 };
    else
        return move;
}

int getBoardScore(Board* board, const Move& lastMove, bool myTurn, int maxDepth) {
    if (maxDepth == 0) {
        int score = 0;
        bool hasMyKing = false, hasOpponentKing = false;
        for (size_t i = 0; i < 64; i++)
        {
            switch (board->board[i])
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
        Move move = findBestMove(board, myTurn, maxDepth, nullptr);
        return getBoardScoreAfterMove(board, move, myTurn, maxDepth);
    }

    return 0;
}

int getBoardScoreAfterMove(Board* board, const Move& move, bool myTurn, int maxDepth) {
    if (isOutOfTime())
        return 0;
    int capturedPos = move.toY * 8 + move.toX;

    PIECE movingPiece = board->board[move.fromY * 8 + move.fromX];

    //en passant
    if (move.toX == board->enPassantAvailable)
        if (movingPiece == PIECE::PLAYER_PAWN && move.toY == 2)
            capturedPos += 8;
        else if (movingPiece == PIECE::OPPONENT_PAWN && move.toY == 5)
            capturedPos -= 8;

    int prevEnPassantAvailable = board->enPassantAvailable;
    board->enPassantAvailable = -1;
    if ((movingPiece == PIECE::PLAYER_PAWN || movingPiece == PIECE::OPPONENT_PAWN) && abs(move.toY - move.fromY) == 2)
        board->enPassantAvailable = move.toX;

    bool castleWK = board->whiteCanCastleKingSide,
        castleWQ = board->whiteCanCastleQueenSide,
        castleBK = board->blackCanCastleKingSide,
        castleBQ = board->blackCanCastleQueenSide;

    bool castleMove = false;

    if (movingPiece == PIECE::PLAYER_ROOK)
    {
        if (move.fromX == 0 && move.fromY == 7)
            board->whiteCanCastleQueenSide = false;
        else if (move.fromX == 7 && move.fromY == 7)
            board->whiteCanCastleKingSide = false;
    }
    else if (movingPiece == PIECE::PLAYER_KING)
    {
        if (move.dat == MOVE_DATA::CASTLE_KING_SIDE)
        {
            castleMove = true;
            board->board[7 * 8 + 4] = board->board[7 * 8 + 7] = PIECE::NONE;
            board->board[7 * 8 + 6] = PIECE::PLAYER_KING;
            board->board[7 * 8 + 5] = PIECE::PLAYER_ROOK;
        }
        else if (move.dat == MOVE_DATA::CASTLE_QUEEN_SIDE)
        {
            castleMove = true;
            board->board[7 * 8 + 4] = board->board[7 * 8 + 0] = PIECE::NONE;
            board->board[7 * 8 + 2] = PIECE::PLAYER_KING;
            board->board[7 * 8 + 3] = PIECE::PLAYER_ROOK;
        }
        board->whiteCanCastleKingSide = board->whiteCanCastleQueenSide = false;
    }
    else if (movingPiece == PIECE::OPPONENT_ROOK)
    {
        if (move.fromX == 0 && move.fromY == 0)
            board->blackCanCastleQueenSide = false;
        else if (move.fromX == 7 && move.fromY == 0)
            board->blackCanCastleKingSide = false;
    }
    else if (movingPiece == PIECE::OPPONENT_KING)
    {
        if (move.dat == MOVE_DATA::CASTLE_KING_SIDE)
        {
            castleMove = true;
            board->board[0 * 8 + 4] = board->board[0 * 8 + 7] = PIECE::NONE;
            board->board[0 * 8 + 6] = PIECE::OPPONENT_KING;
            board->board[0 * 8 + 5] = PIECE::OPPONENT_ROOK;
        }
        else if (move.dat == MOVE_DATA::CASTLE_QUEEN_SIDE)
        {
            castleMove = true;
            board->board[0 * 8 + 4] = board->board[0 * 8 + 0] = PIECE::NONE;
            board->board[0 * 8 + 2] = PIECE::OPPONENT_KING;
            board->board[0 * 8 + 3] = PIECE::OPPONENT_ROOK;
        }
        board->blackCanCastleKingSide = board->blackCanCastleQueenSide = false;
    }

    PIECE capturedPiece = board->board[capturedPos];
    switch (move.dat)
    {
    case MOVE_DATA::PROMOTION_QUEEN:
        board->board[capturedPos] = myTurn ? PIECE::PLAYER_QUEEN : PIECE::OPPONENT_QUEEN;
        break;
    case MOVE_DATA::PROMOTION_ROOK:
        board->board[capturedPos] = myTurn ? PIECE::PLAYER_ROOK : PIECE::OPPONENT_ROOK;
        break;
    case MOVE_DATA::PROMOTION_BISHOP:
        board->board[capturedPos] = myTurn ? PIECE::PLAYER_BISHOP : PIECE::OPPONENT_BISHOP;
        break;
    case MOVE_DATA::PROMOTION_KNIGHT:
        board->board[capturedPos] = myTurn ? PIECE::PLAYER_KNIGHT : PIECE::OPPONENT_KNIGHT;
        break;
    default:
        board->board[capturedPos] = movingPiece;
        break;
    }
    board->board[move.fromY * 8 + move.fromX] = PIECE::NONE;

    int score = getBoardScore(board, move, !myTurn, maxDepth - 1);

    board->enPassantAvailable = prevEnPassantAvailable;
    board->whiteCanCastleKingSide = castleWK;
    board->whiteCanCastleQueenSide = castleWQ;
    board->blackCanCastleKingSide = castleBK;
    board->blackCanCastleQueenSide = castleBQ;
    if (castleMove) {
        if (movingPiece == PIECE::PLAYER_KING) {
            if (move.dat == MOVE_DATA::CASTLE_KING_SIDE) {
                board->board[7 * 8 + 5] = board->board[7 * 8 + 6] = PIECE::NONE;
                board->board[7 * 8 + 4] = PIECE::PLAYER_KING;
                board->board[7 * 8 + 7] = PIECE::PLAYER_ROOK;
            }
            else if (move.dat == MOVE_DATA::CASTLE_QUEEN_SIDE) {
                board->board[7 * 8 + 2] = board->board[7 * 8 + 3] = PIECE::NONE;
                board->board[7 * 8 + 4] = PIECE::PLAYER_KING;
                board->board[7 * 8 + 0] = PIECE::PLAYER_ROOK;
            }
        }
        else if (movingPiece == PIECE::OPPONENT_KING) {
            if (move.dat == MOVE_DATA::CASTLE_KING_SIDE) {
                board->board[0 * 8 + 5] = board->board[0 * 8 + 6] = PIECE::NONE;
                board->board[0 * 8 + 4] = PIECE::OPPONENT_KING;
                board->board[0 * 8 + 7] = PIECE::OPPONENT_ROOK;
            }
            else if (move.dat == MOVE_DATA::CASTLE_QUEEN_SIDE) {
                board->board[0 * 8 + 2] = board->board[0 * 8 + 3] = PIECE::NONE;
                board->board[0 * 8 + 4] = PIECE::OPPONENT_KING;
                board->board[0 * 8 + 0] = PIECE::OPPONENT_ROOK;
            }
        }
    }
    else {
        board->board[move.fromY * 8 + move.fromX] = movingPiece;
        board->board[capturedPos] = capturedPiece;
    }
    return score;
}

void setBest(Board* board, bool myTurn, int maxDepth, int& bestScore, Move& bestMove, Move newMove) {
    if (isOutOfTime())
        return;

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

void setBestForPawn(Board* board, bool myTurn, int maxDepth, int& bestScore, Move& bestMove, Move newMove) {
    //promotion
    if (newMove.toY == 0 || newMove.toY == 7) {
        newMove.dat = MOVE_DATA::PROMOTION_QUEEN; setBest(board, myTurn, maxDepth, bestScore, bestMove, newMove);
        newMove.dat = MOVE_DATA::PROMOTION_ROOK; setBest(board, myTurn, maxDepth, bestScore, bestMove, newMove);
        newMove.dat = MOVE_DATA::PROMOTION_BISHOP; setBest(board, myTurn, maxDepth, bestScore, bestMove, newMove);
        newMove.dat = MOVE_DATA::PROMOTION_KNIGHT; setBest(board, myTurn, maxDepth, bestScore, bestMove, newMove);
    }
    else {
        setBest(board, myTurn, maxDepth, bestScore, bestMove, newMove);
    }
}

Move findBestMove(Board* board, bool myTurn, int maxDepth, int* scoreAfterMove) {
    if (myTurn)
    {
        Move bestMove{}, move;
        int bestScore = INT_MIN, score;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                switch (board->board[y * 8 + x])
                {
                case PIECE::PLAYER_PAWN:
                    //move
                    if (isWithinBounds(x, y - 1) && isEmpty(board, x, y - 1))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y - 1 });
                    //double move
                    if (y == 6 && isEmpty(board, x, y - 1) && isEmpty(board, x, y - 2))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y - 2 });
                    //capture left
                    if (isWithinBounds(x - 1, y - 1) && (isOpponentPiece(board, x - 1, y - 1) || (y - 1 == 2 && board->enPassantAvailable == x - 1)))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x - 1, y - 1 });
                    //capture right
                    if (isWithinBounds(x + 1, y - 1) && (isOpponentPiece(board, x + 1, y - 1) || (y - 1 == 2 && board->enPassantAvailable == x + 1)))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + 1, y - 1 });
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
                    //castle
                    if (board->whiteCanCastleKingSide) {
                        if (isEmpty(board, 5, 7) && isEmpty(board, 6, 7) &&
                            !isTileUnderAttack(board, 4, 7) && !isTileUnderAttack(board, 5, 7) && !isTileUnderAttack(board, 6, 7))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, 6, 7, MOVE_DATA::CASTLE_KING_SIDE });
                    }
                    if (board->whiteCanCastleQueenSide) {
                        if (isEmpty(board, 1, 7) && isEmpty(board, 2, 7) && isEmpty(board, 3, 7) &&
                            !isTileUnderAttack(board, 4, 7) && !isTileUnderAttack(board, 3, 7) && !isTileUnderAttack(board, 2, 7))
                            setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, 2, 7, MOVE_DATA::CASTLE_QUEEN_SIDE });
                    }
                    break;
                default:
                    break;
                }
            }
        if (scoreAfterMove != nullptr)
            *scoreAfterMove = bestScore;
        return bestMove;
    }
    else
    {
        Move bestMove{}, move;
        int bestScore = INT_MAX, score;

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                switch (board->board[y * 8 + x])
                {
                case PIECE::OPPONENT_PAWN:
                    //move
                    if (isWithinBounds(x, y + 1) && isEmpty(board, x, y + 1))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y + 1 });
                    //double move
                    if (y == 1 && isEmpty(board, x, y + 1) && isEmpty(board, x, y + 2))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x, y + 2 });
                    //capture left
                    if (isWithinBounds(x - 1, y + 1) && (isMyPiece(board, x - 1, y + 1) || y + 1 == 5 && board->enPassantAvailable == x - 1))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x - 1, y + 1 });
                    //capture right
                    if (isWithinBounds(x + 1, y + 1) && (isMyPiece(board, x + 1, y + 1) || y + 1 == 5 && board->enPassantAvailable == x + 1))
                        setBestForPawn(board, myTurn, maxDepth, bestScore, bestMove, { x, y, x + 1, y + 1 });
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

                        //castle
                        if (board->whiteCanCastleKingSide) {
                            if (isEmpty(board, 5, 0) && isEmpty(board, 6, 0) &&
                                !isTileUnderAttack(board, 4, 0) && !isTileUnderAttack(board, 5, 0) && !isTileUnderAttack(board, 6, 0))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, 6, 0, MOVE_DATA::CASTLE_KING_SIDE });
                        }
                        if (board->whiteCanCastleQueenSide) {
                            if (isEmpty(board, 1, 0) && isEmpty(board, 2, 0) && isEmpty(board, 3, 0) &&
                                !isTileUnderAttack(board, 4, 0) && !isTileUnderAttack(board, 3, 0) && !isTileUnderAttack(board, 2, 0))
                                setBest(board, myTurn, maxDepth, bestScore, bestMove, { x, y, 2, 0, MOVE_DATA::CASTLE_QUEEN_SIDE });
                        }
                    }
                    break;
                default:
                    break;
                }
            }
        if (scoreAfterMove != nullptr)
            *scoreAfterMove = bestScore;
        return bestMove;
    }
}

void printBoard(Board* board) {
    cout << endl;
    for (int y = 0; y < 8; y++)
    {
        for (int x = 0; x < 8; x++)
            cout << pieceToChar[board->board[y * 8 + x]] << ' ';
        cout << endl;
    }
}
