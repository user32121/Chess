using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess5DGUI
{
    public class StartingBoards
    {
        public static GameBoard Standard => GameBoard.GameBoardFromString("RNBQKBNR/PPPPPPPP/________/________/________/________/pppppppp/rnbqkbnr");

        public static GameBoard MiscSmall => GameBoard.GameBoardFromString("KQBNR/PPPPP/_____/ppppp/kqbnr");
        public static GameBoard MiscTimelineInvasion => GameBoard.GameBoardFromString("NBKRB/PPPPP/_____/_____/ppppp|PPPPP/_____/_____/ppppp/nbkrb");
        public static GameBoard MiscTimelineFormations => GameBoard.GameBoardFromString("PPPPP/_____/_____/_____/__k__|__K__/_____/_____/_____/ppppp");

        //solved: white wins
        public static GameBoard FocusedQueens => GameBoard.GameBoardFromString("__K_Q_/______/______/______/______/_q_k__");
        //
        public static GameBoard FocusedRooks => GameBoard.GameBoardFromString("R_KR_/_____/_____/_____/_rk_r");
        //solved?: white wins
        public static GameBoard FocusedKnights => GameBoard.GameBoardFromString("_NK_N/_____/_____/_____/n_kn_");
        //
        public static GameBoard FocusedBishops => GameBoard.GameBoardFromString("_KBB_/_____/_____/_____/_bbk_");
        //
        public static GameBoard FocusedUnicorns => GameBoard.GameBoardFromString("KU_U_/_____/_____/_____/_u_uk");
        //
        public static GameBoard FocusedDragons => GameBoard.GameBoardFromString("KDD__/_____/_____/_____/__ddk");
        //
        public static GameBoard FocusedPawns => GameBoard.GameBoardFromString("KPPPP/_____/_____/_____/ppppk");
        //solved: first to branch timelines loses
        public static GameBoard FocusedKings => GameBoard.GameBoardFromString("K__/___/__k");

        public static GameBoard Test => GameBoard.GameBoardFromString("_P__K/_____/_____/_____/p___k");



        //solved
        public static GameBoard PuzzleRook1 => GameBoard.GameBoardFromString("K_R__/_____/_____/_____/____k", new(0, 0, 2, 0, 0, 0, 4, 0), new(0, 1, 4, 4, 0, 1, 3, 3), new(0, 2, 0, 0, 0, 2, 1, 1), new(0, 3, 3, 3, 0, 3, 2, 3));
        //solved
        public static GameBoard PuzzleRook2 => GameBoard.GameBoardFromString("KR___/_____/_____/_____/__p_k", new(0, 0, 1, 0, 0, 0, 3, 0), new(0, 1, 4, 4, 0, 1, 4, 3), new(0, 2, 3, 0, 0, 2, 4, 0), new(0, 3, 4, 3, 0, 3, 3, 4), new(0, 4, 4, 0, 0, 2, 4, 0), new(0, 3, 4, 3, 1, 5, 4, 3));
        //solved
        public static GameBoard PuzzleRook3 => GameBoard.GameBoardFromString("KR____/_R____/______/______/______/_____k", new(0, 0, 1, 1, 0, 0, 4, 1), new(0, 1, 5, 5, 0, 1, 5, 4));
        //unsolved: need to implement inactive timelines
        public static GameBoard PuzzleRook4 => GameBoard.GameBoardFromString("___K_/__r__/___k_/_____/_____", new Move(0, 0, 3, 0, 0, 0, 4, 0));
        //human solved: would need to process 8 steps
        public static GameBoard PuzzleKnight6 => GameBoard.GameBoardFromString("K_N__/_____/_____/_____/____k");
        //solved
        public static GameBoard PuzzleKing1 => GameBoard.GameBoardFromString("_B___/__K__/_____/_k___/_____", new(0, 0, 2, 1, 0, 0, 1, 1), new(0, 1, 1, 3, 0, 1, 0, 3), new(0, 2, 1, 1, 0, 2, 0, 1), new(0, 3, 0, 3, 0, 3, 1, 3), new(0, 4, 1, 0, 0, 0, 4, 0));
        //solved
        public static GameBoard PuzzleKing2 => GameBoard.GameBoardFromString("K__/___/__k", new(0, 0, 0, 0, 0, 0, 1, 0), new(0, 1, 2, 2, 0, 1, 1, 2), new(0, 2, 1, 0, 0, 2, 0, 0), new(0, 3, 1, 2, 0, 1, 0, 2));
        //solved
        public static GameBoard Combination1 => GameBoard.GameBoardFromString("KN___/_R___/_____/_____/___pk");
        //solved
        public static GameBoard Combination3 => GameBoard.GameBoardFromString("KR____/B_____/______/______/______/_____k", new(0, 0, 1, 0, 0, 0, 5, 0), new(0, 1, 5, 5, 0, 1, 4, 4));
        //solved
        public static GameBoard OpeningTrap1 => GameBoard.GameBoardFromString("RNBQKBNR/PPPPPPPP/________/________/________/________/pppppppp/rnbqkbnr", new(0, 0, 4, 1, 0, 0, 4, 3), new(0, 1, 4, 6, 0, 1, 4, 4), new(0, 2, 3, 0, 0, 2, 7, 4), new(0, 3, 6, 7, 0, 3, 5, 5));
        //solved
        public static GameBoard OpeningTrap2 => GameBoard.GameBoardFromString("RNBQKBNR/PPPPPPPP/________/________/________/________/pppppppp/rnbqkbnr", new(0, 0, 3, 1, 0, 0, 3, 3), new(0, 1, 4, 6, 0, 1, 4, 4), new(0, 2, 3, 3, 0, 2, 4, 4), new(0, 3, 3, 7, 0, 3, 4, 6), new(0, 4, 1, 0, 0, 4, 3, 1));
        //human solved: would need to process 6 steps
        public static GameBoard OpeningTrap3 => GameBoard.GameBoardFromString("RNBQKBNR/PPPPPPPP/________/________/________/________/pppppppp/rnbqkbnr", new(0, 0, 4, 1, 0, 0, 4, 3), new(0, 1, 2, 6, 0, 1, 2, 4), new(0, 2, 3, 0, 0, 2, 7, 4), new(0, 3, 3, 6, 0, 3, 3, 5), new(0, 4, 5, 0, 0, 4, 1, 4), new(0, 5, 1, 7, 0, 5, 2, 5), new(0, 6, 3, 1, 0, 6, 3, 2), new(0, 7, 6, 7, 0, 7, 5, 5));
        //unsolved
        public static GameBoard Tricky1 => GameBoard.GameBoardFromString("PPP_/____/____/p___|_B__/K___/____/___k");
        //solved
        public static GameBoard Tricky2 => GameBoard.GameBoardFromString("____/_BB_/____/__p_|_N__/K___/__p_/___k");
        //human solved: need to process 7 steps
        public static GameBoard PuzzleAdvBranching1 => GameBoard.GameBoardFromString("_K____/PP_N_Q/______/______/_n__pp/____rk", new(0, 0, 0, 1, 0, 0, 0, 3), new(0, 1, 1, 4, 0, 1, 3, 3), new(0, 2, 3, 1, 0, 2, 2, 3), new(0, 3, 5, 4, 0, 3, 5, 3), new(0, 4, 2, 3, 0, 0, 2, 2), new(1, 5, 3, 3, 0, 1, 3, 3));
    }
}
