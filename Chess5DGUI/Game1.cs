using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Chess5DGUI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D blank;
        private Texture2D pieceTex;
        private Texture2D selectTex;
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

        private Board board = new();
        private bool isWhiteTurn = true;
        private Point4? selectedPos;
        private Move prevMove = new Move() { from = new() { x = -1 }, to = new() { x = -1 } };

        private MouseState prevMS;

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

            board.boards = new() { new(), new(), new() };
            board.boards[0].Add((PIECE[,])Board.getStartingBoard());

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            blank = new Texture2D(GraphicsDevice, 1, 1);
            blank.SetData(new Color[] { Color.White });
            pieceTex = Content.Load<Texture2D>("pieces");
            selectTex = Content.Load<Texture2D>("selectBox");
            font = Content.Load<SpriteFont>("font");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && prevMS.LeftButton == ButtonState.Released)
            {
                Point4 clickPos = new(ms.X / pieceDrawSize % 9, ms.Y / pieceDrawSize % 9, ms.X / pieceDrawSize / 9, ms.Y / pieceDrawSize / 9);
                if (clickPos.x != 8 && clickPos.y != 8 &&
                    clickPos.c >= 0 && clickPos.c < board.boards.Count &&
                    clickPos.t >= 0 && clickPos.t < board.boards[clickPos.c].Count)
                {
                    if (selectedPos.HasValue)
                    {
                        Utils.PerformMove(board, new Move(selectedPos.Value, clickPos));
                        selectedPos = null;
                    }
                    else if (board[clickPos] != PIECE.NONE)
                        selectedPos = clickPos;
                    else
                        selectedPos = null;
                }
                else
                    selectedPos = null;
            }

            if (ms.RightButton == ButtonState.Pressed)
                selectedPos = null;

            prevMS = ms;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            for (int c = 0; c < board.boards.Count; c++)
                for (int t = 0; t < board.boards[c].Count; t++)
                    for (int x = 0; x < 8; x++)
                    {
                        for (int y = 0; y < 8; y++)
                        {
                            Point drawPos = new Point(x * pieceDrawSize + t * pieceDrawSize * 9, y * pieceDrawSize + c * pieceDrawSize * 9);
                            Point spriteTexPos = pieceTexIndex[board[new Point4(x, y, t, c)]];

                            _spriteBatch.Draw(blank, new Rectangle(drawPos.X, drawPos.Y, pieceDrawSize, pieceDrawSize), (x + y) % 2 == 0 ? Color.RosyBrown : Color.Tan);

                            if (prevMove.from == new Point4(x, y, t, c) || prevMove.to == new Point4(x, y, t, c))
                                _spriteBatch.Draw(blank, new Rectangle(drawPos.X, drawPos.Y, pieceDrawSize, pieceDrawSize), Color.LightGreen * 0.5f);

                            _spriteBatch.Draw(pieceTex, new Rectangle(drawPos.X, drawPos.Y, pieceDrawSize, pieceDrawSize), new Rectangle(spriteTexPos, new Point(pieceTexSize, pieceTexSize)), Color.White);

                            if (selectedPos == new Point4(x, y, t, c))
                                _spriteBatch.Draw(selectTex, new Rectangle(drawPos.X, drawPos.Y, pieceDrawSize, pieceDrawSize), Color.White);
                        }
                    }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}