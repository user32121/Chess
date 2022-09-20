using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using static Chess.DllWrapper;
using ThreadState = System.Threading.ThreadState;

namespace ChessGUI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D blank;
        private Texture2D pieceTex;
        private Dictionary<PIECE, Point> pieceTexIndex = new Dictionary<PIECE, Point>()
        {
            {PIECE.NONE,new Point(0,640)},
            {PIECE.WHITE_KING,new Point(0,0)},
            {PIECE.WHITE_QUEEN,new Point(320,0)},
            {PIECE.WHITE_BISHOP,new Point(640,0)},
            {PIECE.WHITE_KNIGHT,new Point(960,0)},
            {PIECE.WHITE_ROOK,new Point(1280,0)},
            {PIECE.WHITE_PAWN,new Point(1600,0)},
            {PIECE.BLACK_KING,new Point(0,320)},
            {PIECE.BLACK_QUEEN,new Point(320,320)},
            {PIECE.BLACK_BISHOP,new Point(640,320)},
            {PIECE.BLACK_KNIGHT,new Point(960,320)},
            {PIECE.BLACK_ROOK,new Point(1280,320)},
            {PIECE.BLACK_PAWN,new Point(1600,320)},
        };
        private const int pieceTexSize = 320;
        private const int pieceDrawSize = 64;
        private SpriteFont font;

        private Board board;
        private Move prevMove = new Move() { fromX = -1, toX = -1 };
        private bool isWhiteTurn = true;
        private Thread algoThread;
        private List<(int, Move, int)> algoIterOutputs = new List<(int, Move, int)>();
        private Move algoOutput;
        private bool algoError = false;
        private Stopwatch stopwatch = new Stopwatch();

        private const int maxTime = 1;
        private const int maxDepth = 20;

        private MouseState prevMS;
        private Point? mouseDownOn, promotingPawnAt;
        private List<Move> preMoves = new List<Move>();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 520;
            _graphics.ApplyChanges();

            board = new Board()
            {
                board = new PIECE[64]
                {
                    PIECE.BLACK_ROOK, PIECE.BLACK_KNIGHT, PIECE.BLACK_BISHOP, PIECE.BLACK_QUEEN, PIECE.BLACK_KING, PIECE.BLACK_BISHOP, PIECE.BLACK_KNIGHT, PIECE.BLACK_ROOK,
                    PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN,
                    PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN,
                    PIECE.WHITE_ROOK, PIECE.WHITE_KNIGHT, PIECE.WHITE_BISHOP, PIECE.WHITE_QUEEN, PIECE.WHITE_KING, PIECE.WHITE_BISHOP, PIECE.WHITE_KNIGHT, PIECE.WHITE_ROOK,

                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.BLACK_KING, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    //PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN, PIECE.BLACK_PAWN,
                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                    //PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN, PIECE.WHITE_PAWN,
                    //PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.NONE, PIECE.WHITE_KING, PIECE.NONE, PIECE.NONE, PIECE.NONE,
                }
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new Color[] { Color.White });
            pieceTex = Content.Load<Texture2D>("pieces");
            font = Content.Load<SpriteFont>("font");
        }

        private void GetNextMove()
        {
            stopwatch.Restart();
            Board workingBoard = new Board();
            board.board.CopyTo(workingBoard.board, 0);
            workingBoard.whiteCanCastleQueenSide = board.whiteCanCastleQueenSide;
            workingBoard.whiteCanCastleKingSide = board.whiteCanCastleKingSide;
            workingBoard.blackCanCastleQueenSide = board.blackCanCastleQueenSide;
            workingBoard.blackCanCastleKingSide = board.blackCanCastleKingSide;
            workingBoard.enPassantAvailable = board.enPassantAvailable;

            algoIterOutputs.Clear();
            int depth = 1;
            while (stopwatch.ElapsedMilliseconds <= maxTime * 1000 && depth <= maxDepth)
            {
                Move output = apiFindBestMove(workingBoard, depth, maxTime * 1000 - stopwatch.ElapsedMilliseconds, out int score);
                if (output.fromX == -1)
                    continue;
                if (Utils.IsValidMove(board, output, isWhiteTurn))
                    algoIterOutputs.Add((depth, output, score));
                depth++;
            }
            stopwatch.Stop();
            try
            {
                algoOutput = algoIterOutputs[algoIterOutputs.Count - 1].Item2;
            }
            catch (ArgumentOutOfRangeException)
            {
                algoError = true;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();
            KeyboardState ks = Keyboard.GetState();

            if (promotingPawnAt.HasValue && mouseDownOn.HasValue)
            {
                MOVE_DATA md = MOVE_DATA.NONE;
                if (ks.IsKeyDown(Keys.Escape))
                    promotingPawnAt = mouseDownOn = null;
                else if (ks.IsKeyDown(Keys.Q))
                    md = MOVE_DATA.PROMOTION_QUEEN;
                else if (ks.IsKeyDown(Keys.R))
                    md = MOVE_DATA.PROMOTION_ROOK;
                else if (ks.IsKeyDown(Keys.B))
                    md = MOVE_DATA.PROMOTION_BISHOP;
                else if (ks.IsKeyDown(Keys.N))
                    md = MOVE_DATA.PROMOTION_KNIGHT;

                if (md != MOVE_DATA.NONE)
                {
                    Move move = new Move() { fromX = mouseDownOn.Value.X, fromY = mouseDownOn.Value.Y, toX = promotingPawnAt.Value.X, toY = promotingPawnAt.Value.Y, dat = md };
                    if (Utils.IsValidPieceMove(board, move))
                        preMoves.Add(move);
                    mouseDownOn = null;
                    promotingPawnAt = null;
                }
            }
            else if (ms.LeftButton == ButtonState.Pressed)
            {
                if (prevMS.LeftButton == ButtonState.Released && !mouseDownOn.HasValue)
                {
                    int x = ms.Position.X / pieceDrawSize, y = ms.Position.Y / pieceDrawSize;
                    if (Utils.isWithinBounds(x, y) && Utils.isBlackPiece(board, x, y))
                        mouseDownOn = new Point(x, y);
                }
            }
            else if (mouseDownOn.HasValue)
            {
                int x = ms.Position.X / pieceDrawSize, y = ms.Position.Y / pieceDrawSize;

                Move move = new Move() { fromX = mouseDownOn.Value.X, fromY = mouseDownOn.Value.Y, toX = x, toY = y };
                if (Utils.IsValidPieceMove(board, move))
                {
                    if (y == 0 && board.board[mouseDownOn.Value.Y * 8 + mouseDownOn.Value.X] == PIECE.WHITE_PAWN ||
                        y == 7 && board.board[mouseDownOn.Value.Y * 8 + mouseDownOn.Value.X] == PIECE.BLACK_PAWN)
                        promotingPawnAt = new Point(x, y);
                    else
                    {
                        preMoves.Add(move);
                        mouseDownOn = null;
                    }
                }
                else
                    mouseDownOn = null;
            }

            //right click to cancel premoves
            if (ms.RightButton == ButtonState.Released && prevMS.RightButton == ButtonState.Pressed)
                preMoves.Clear();

            prevMS = ms;

            if (isWhiteTurn)
            {
                if (algoThread == null)
                {
                    algoThread = new Thread(new ThreadStart(GetNextMove));
                    algoThread.Start();
                }
                else if (algoThread.ThreadState == ThreadState.Stopped)
                {
                    if (!algoError)
                    {
                        //thread finished
                        Utils.PerformMove(board, algoOutput, ref prevMove);
                        isWhiteTurn = false;
                        algoThread = null;
                    }
                }
                else if (algoThread.ThreadState != ThreadState.Running)
                    throw new NotImplementedException("ThreadState " + algoThread.ThreadState + " is not implemnted");
            }
            else if (preMoves.Count > 0)
            {
                if (Utils.IsValidMove(board, preMoves[0], isWhiteTurn))
                {
                    Utils.PerformMove(board, preMoves[0], ref prevMove);
                    isWhiteTurn = true;
                    preMoves.RemoveAt(0);
                }
                else
                    preMoves.Clear();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            bool[] hasPremove = new bool[64];
            foreach (Move item in preMoves)
            {
                hasPremove[item.fromY * 8 + item.fromX] = true;
                hasPremove[item.toY * 8 + item.toX] = true;
            }

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    _spriteBatch.Draw(blank, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), (x + y) % 2 == 0 ? Color.RosyBrown : Color.Tan);

                    if (prevMove.fromX == x && prevMove.fromY == y || prevMove.toX == x && prevMove.toY == y)
                        _spriteBatch.Draw(blank, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), Color.LightGreen * 0.5f);

                    if (hasPremove[y * 8 + x])
                        _spriteBatch.Draw(blank, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), Color.LightGray * 0.5f);

                    if (mouseDownOn.HasValue && mouseDownOn == new Point(x, y))
                        continue;

                    Point spritePos = pieceTexIndex[board.board[y * 8 + x]];
                    _spriteBatch.Draw(pieceTex, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), new Rectangle(spritePos, new Point(pieceTexSize, pieceTexSize)), Color.White);
                }
            }

            if (mouseDownOn.HasValue)
            {
                if (promotingPawnAt.HasValue)
                {
                    Point spritePos = pieceTexIndex[board.board[promotingPawnAt.Value.Y * 8 + promotingPawnAt.Value.X]];
                    _spriteBatch.Draw(pieceTex, new Rectangle(promotingPawnAt.Value.X * pieceDrawSize, promotingPawnAt.Value.Y * pieceDrawSize, pieceDrawSize, pieceDrawSize), new Rectangle(spritePos, new Point(pieceTexSize, pieceTexSize)), Color.White);
                }
                else
                {
                    MouseState ms = Mouse.GetState();
                    Point spritePos = pieceTexIndex[board.board[mouseDownOn.Value.Y * 8 + mouseDownOn.Value.X]];
                    _spriteBatch.Draw(pieceTex, new Rectangle(ms.X - pieceDrawSize / 2, ms.Y - pieceDrawSize / 2, pieceDrawSize, pieceDrawSize), new Rectangle(spritePos, new Point(pieceTexSize, pieceTexSize)), Color.White);
                }
            }

            int line = 0;

            _spriteBatch.DrawString(font, Utils.GetScore(board).ToString(), new Vector2(pieceDrawSize * 8 + 10, 10 + line++ * 20), Color.Black);

            if (prevMove.fromX != -1)
                _spriteBatch.DrawString(font, Utils.MoveToString(prevMove), new Vector2(pieceDrawSize * 8 + 10, 10 + line++ * 20), Color.Black);

            _spriteBatch.DrawString(font, (stopwatch.ElapsedMilliseconds / 1000.0).ToString(), new Vector2(pieceDrawSize * 8 + 10, 10 + line++ * 20), Color.Black);
            for (int i = 0; i < algoIterOutputs.Count; i++)
                try
                {
                    _spriteBatch.DrawString(font, String.Format("d={0}: {1} ({2})", algoIterOutputs[i].Item1, Utils.MoveToString(algoIterOutputs[i].Item2), algoIterOutputs[i].Item3), new Vector2(pieceDrawSize * 8 + 10, 10 + line++ * 20), Color.Black);
                }
                catch (ArgumentOutOfRangeException) { }

            if (algoError)
                _spriteBatch.DrawString(font, "Algorithm error", new Vector2(pieceDrawSize * 8 + 10, 10 + line++ * 20), Color.Black);

            if (promotingPawnAt.HasValue)
            {
                _spriteBatch.Draw(blank, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White * 0.5f);
                Vector2 size = font.MeasureString("Press Q, R, B, or N");
                _spriteBatch.DrawString(font, "Press Q, R, B, or N", new Vector2(_graphics.PreferredBackBufferWidth / 2 - size.X / 2, _graphics.PreferredBackBufferHeight / 2 - size.Y / 2), Color.Black);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}