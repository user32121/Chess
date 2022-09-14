using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using static Chess.DllWrapper;

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

        private PIECE[] board;
        private Move prevMove = new Move();
        private bool isWhiteTurn = true;
        private Thread algoThread;
        private Move algoOutput;

        private const int maxDepth = 5;

        private MouseState prevMS;
        private Point? mouseDownOn;

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

            board = new PIECE[]
            {
                PIECE.BLACK_ROOK,PIECE.BLACK_KNIGHT,PIECE.BLACK_BISHOP,PIECE.BLACK_QUEEN,PIECE.BLACK_KING,PIECE.BLACK_BISHOP,PIECE.BLACK_KNIGHT,PIECE.BLACK_ROOK,
                PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,PIECE.BLACK_PAWN,
                PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,
                PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,
                PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,
                PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,PIECE.NONE,
                PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,PIECE.WHITE_PAWN,
                PIECE.WHITE_ROOK,PIECE.WHITE_KNIGHT,PIECE.WHITE_BISHOP,PIECE.WHITE_QUEEN,PIECE.WHITE_KING,PIECE.WHITE_BISHOP,PIECE.WHITE_KNIGHT,PIECE.WHITE_ROOK,
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new Color[] { Color.White });
            pieceTex = Content.Load<Texture2D>("pieces");
        }

        private void GetNextMove()
        {
            PIECE[] workingBoard = new PIECE[board.Length];
            board.CopyTo(workingBoard, 0);
            algoOutput = findBestMove(workingBoard, prevMove, maxDepth);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState ms = Mouse.GetState();

            if (isWhiteTurn)
            {
                if (algoThread == null)
                {
                    algoThread = new Thread(new ThreadStart(GetNextMove));
                    algoThread.Start();
                }
                else if (algoThread.ThreadState == ThreadState.Stopped)
                {
                    //thread finished
                    Utils.PerformMove(board, algoOutput, ref prevMove);
                    isWhiteTurn = false;
                    algoThread = null;
                }
                else if (algoThread.ThreadState != ThreadState.Running)
                    throw new NotImplementedException("ThreadState " + algoThread.ThreadState + " is not implemnted");
            }
            else
            {
                if (ms.LeftButton == ButtonState.Pressed)
                {
                    if (prevMS.LeftButton == ButtonState.Released && !mouseDownOn.HasValue)
                    {
                        int x = ms.Position.X / pieceDrawSize, y = ms.Position.Y / pieceDrawSize;
                        if (Utils.isWithinBounds(x, y))
                            mouseDownOn = new Point(x, y);
                    }
                }
                else if (mouseDownOn.HasValue)
                {
                    int x = ms.Position.X / pieceDrawSize, y = ms.Position.Y / pieceDrawSize;
                    Move move = new Move() { fromX = mouseDownOn.Value.X, fromY = mouseDownOn.Value.Y, toX = x, toY = y };
                    if (Utils.IsValidMove(board, move, isWhiteTurn))
                    {
                        Utils.PerformMove(board, move, ref prevMove);
                        isWhiteTurn = true;
                    }
                    mouseDownOn = null;
                }
            }

            prevMS = ms;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    _spriteBatch.Draw(blank, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), (x + y) % 2 == 0 ? Color.RosyBrown : Color.Tan);

                    if (prevMove.fromX == x && prevMove.fromY == y || prevMove.toX == x && prevMove.toY == y)
                        _spriteBatch.Draw(blank, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), Color.LightGreen * 0.5f);

                    if (mouseDownOn.HasValue && mouseDownOn == new Point(x, y))
                        continue;

                    Point spritePos = pieceTexIndex[board[y * 8 + x]];
                    _spriteBatch.Draw(pieceTex, new Rectangle(x * pieceDrawSize, y * pieceDrawSize, pieceDrawSize, pieceDrawSize), new Rectangle(spritePos, new Point(pieceTexSize, pieceTexSize)), Color.White);
                }
            }

            if (mouseDownOn.HasValue)
            {
                MouseState ms = Mouse.GetState();
                Point spritePos = pieceTexIndex[board[mouseDownOn.Value.Y * 8 + mouseDownOn.Value.X]];
                _spriteBatch.Draw(pieceTex, new Rectangle(ms.X - pieceDrawSize / 2, ms.Y - pieceDrawSize / 2, pieceDrawSize, pieceDrawSize), new Rectangle(spritePos, new Point(pieceTexSize, pieceTexSize)), Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}