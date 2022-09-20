#pragma once

#ifdef CHESSALGO_EXPORTS
#define CHESSALGO_API __declspec(dllexport)
#else
#define CHESSALGO_API __declspec(dllimport)
#endif



enum class PIECE : int
{
    NONE = 0,
    PLAYER_PAWN = 10,
    PLAYER_ROOK = 11,
    PLAYER_KNIGHT = 12,
    PLAYER_BISHOP = 13,
    PLAYER_QUEEN = 14,
    PLAYER_KING = 15,
    OPPONENT_PAWN = 20,
    OPPONENT_ROOK = 21,
    OPPONENT_KNIGHT = 22,
    OPPONENT_BISHOP = 23,
    OPPONENT_QUEEN = 24,
    OPPONENT_KING = 25,
};

const int PAWN_VALUE = 1;
const int ROOK_VALUE = 5;
const int KNIGHT_VALUE = 3;
const int BISHOP_VALUE = 3;
const int QUEEN_VALUE = 9;
//const int KING_VALUE = 1000;
const int WIN_VALUE = 1000;
const int LOSE_VALUE = 10000;

enum class MOVE_DATA : int
{
    NONE = 0,
    PROMOTION_QUEEN = 1,
    PROMOTION_ROOK = 2,
    PROMOTION_BISHOP = 3,
    PROMOTION_KNIGHT = 4,
    CASTLE_KING_SIDE = 5,
    CASTLE_QUEEN_SIDE = 6,
};

struct Move
{
    int fromX, fromY, toX, toY;
    MOVE_DATA dat;
};
struct Board
{
    PIECE board[64]{};
    bool whiteCanCastleQueenSide = true, whiteCanCastleKingSide = true, blackCanCastleQueenSide = true, blackCanCastleKingSide = true;
    int enPassantAvailable = -1;
};


extern "C" CHESSALGO_API int test();
//board is flattened, y first array [y*w + x]
//0,0 is at the top left corner
extern "C" CHESSALGO_API Move apiFindBestMove(Board board, int maxDepth, long long maxTime, int* scoreAfterMove);

Move findBestMove(Board* board, bool myturn, int maxDepth, int* scoreAfterMove);
int getBoardScoreAfterMove(Board* board, const Move& move, bool myTurn, int maxDepth);
void printBoard(Board* board);

inline bool isWithinBounds(int x, int y) {
    return x >= 0 && x < 8 && y >= 0 && y < 8;
}
inline bool isMyPiece(Board* board, int x, int y) {
    return board->board[y * 8 + x] >= PIECE::PLAYER_PAWN && board->board[y * 8 + x] <= PIECE::PLAYER_KING;
}
inline bool isOpponentPiece(Board* board, int x, int y) {
    return board->board[y * 8 + x] >= PIECE::OPPONENT_PAWN && board->board[y * 8 + x] <= PIECE::OPPONENT_KING;
}
inline bool isEmpty(Board* board, int x, int y) {
    return board->board[y * 8 + x] == PIECE::NONE;
}
inline bool isTileUnderAttack(Board* board, int x, int y) {
    return false;
}

int knightMovesX[]{ 1, 2, 2, 1,-1,-2,-2,-1 };
int knightMovesY[]{ 2, 1,-1,-2,-2,-1, 1, 2 };
int rookDirsX[]{ 1,-1, 0, 0 };
int rookDirsY[]{ 0, 0, 1,-1 };
int bishopDirsX[]{ 1,-1, -1, 1 };
int bishopDirsY[]{ 1, 1, -1,-1 };
int queenDirsX[]{ 1, 1, 1, 0,-1,-1,-1, 0 };  //also used for king
int queenDirsY[]{ -1,0, 1, 1, 1, 0,-1,-1 };

std::map<PIECE, char> pieceToChar{
    std::make_pair(PIECE::NONE, '_'),
    std::make_pair(PIECE::PLAYER_PAWN, 'P'),
    std::make_pair(PIECE::PLAYER_ROOK, 'R'),
    std::make_pair(PIECE::PLAYER_KNIGHT, 'N'),
    std::make_pair(PIECE::PLAYER_BISHOP, 'B'),
    std::make_pair(PIECE::PLAYER_QUEEN, 'Q'),
    std::make_pair(PIECE::PLAYER_KING, 'K'),
    std::make_pair(PIECE::OPPONENT_PAWN, 'p'),
    std::make_pair(PIECE::OPPONENT_ROOK, 'r'),
    std::make_pair(PIECE::OPPONENT_KNIGHT, 'n'),
    std::make_pair(PIECE::OPPONENT_BISHOP, 'b'),
    std::make_pair(PIECE::OPPONENT_QUEEN, 'q'),
    std::make_pair(PIECE::OPPONENT_KING, 'k'),
};
