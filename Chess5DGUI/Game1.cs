using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess5DGUI
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D blank;
        private Texture2D pieceTex;
        private Texture2D selectTex;
        private const int pieceTexSize = 320;
        public const int pieceDrawSize = 64;
        private SpriteFont font;
        private const float colorBorderWidth = 0.3f;

        private Board board = Board.getStartingBoard();
        private Point4? selectedPos;
        private Move prevMove = new Move() { from = new() { x = -1 }, to = new() { x = -1 } };
        private List<Move> availableMoves;

        private MouseState prevMS;

        private Vector2 targetViewOffset, viewOffset;
        private const float offsetSmoothingFactor = 0.9f;
        private const int moveSpeed = 20;
        private float targetZoomValue, zoomValue, zoom;
        private const float zoomFactor = 0.999f;
        private const float zoomSmoothingFactor = 0.9f;

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

            availableMoves ??= Utils.GetAllMoves(board);

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && prevMS.LeftButton == ButtonState.Released)
            {
                Point clickPos = Utils.ScreenToWorldSpace(ms.Position.ToVector2(), this, viewOffset, zoom).ToPoint();
                Point4 clickTile = new(clickPos.Y / pieceDrawSize / 9, clickPos.X / pieceDrawSize / 9, clickPos.X / pieceDrawSize % 9, clickPos.Y / pieceDrawSize % 9);
                if (selectedPos.HasValue)
                {
                    Move move = new Move(selectedPos.Value, clickTile);
                    if (availableMoves.Contains(move))
                    {
                        Utils.PerformMove(board, move, ref prevMove, ref targetViewOffset);
                        availableMoves = null;
                    }
                    selectedPos = null;
                }
                else if (availableMoves.Any(m => m.from == clickTile))
                    selectedPos = clickTile;
                else
                    selectedPos = null;
            }

            if (ms.RightButton == ButtonState.Pressed)
                selectedPos = null;

            if (ms.ScrollWheelValue != prevMS.ScrollWheelValue)
                targetZoomValue += ms.ScrollWheelValue - prevMS.ScrollWheelValue;
            zoomValue = zoomSmoothingFactor * zoomValue + (1 - zoomSmoothingFactor) * targetZoomValue;
            zoom = MathF.Pow(zoomFactor, -zoomValue);

            prevMS = ms;

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.W))
                targetViewOffset.Y -= moveSpeed / zoom;
            if (ks.IsKeyDown(Keys.S))
                targetViewOffset.Y += moveSpeed / zoom;
            if (ks.IsKeyDown(Keys.A))
                targetViewOffset.X -= moveSpeed / zoom;
            if (ks.IsKeyDown(Keys.D))
                targetViewOffset.X += moveSpeed / zoom;
            if (ks.IsKeyDown(Keys.Z))
            {
                targetViewOffset = Vector2.Zero;
                targetZoomValue = 0;
            }
            viewOffset = viewOffset * offsetSmoothingFactor + targetViewOffset * (1 - offsetSmoothingFactor);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            Matrix mat = Matrix.CreateTranslation(-viewOffset.X, -viewOffset.Y, 0);
            mat *= Matrix.CreateTranslation(-GraphicsDevice.Viewport.Bounds.Width / 2, -GraphicsDevice.Viewport.Bounds.Height / 2, 0);
            mat *= Matrix.CreateScale(zoom);
            mat *= Matrix.CreateTranslation(GraphicsDevice.Viewport.Bounds.Width / 2, GraphicsDevice.Viewport.Bounds.Height / 2, 0);
            _spriteBatch.Begin(transformMatrix: mat);

            for (int c = 0; c < board.boards.Count; c++)
                for (int t = 0; t < board.boards[c].Count; t++)
                    if (board.boards[c][t] != null)
                    {
                        Point drawBoardPos = new(t * pieceDrawSize * 9, c * pieceDrawSize * 9);
                        int borderPixelWidth = (int)(pieceDrawSize * colorBorderWidth);
                        _spriteBatch.Draw(blank, new Rectangle(drawBoardPos.X - borderPixelWidth, drawBoardPos.Y - borderPixelWidth, 8 * pieceDrawSize + borderPixelWidth * 2, 8 * pieceDrawSize + borderPixelWidth * 2), t % 2 == 0 ? Color.White : Color.Black);

                        for (int x = 0; x < 8; x++)
                        {
                            for (int y = 0; y < 8; y++)
                            {
                                Point drawPiecePos = new(x * pieceDrawSize + drawBoardPos.X, y * pieceDrawSize + drawBoardPos.Y);
                                Point spriteTexPos = Utils.pieceTexIndex[board[c, t, x, y]];

                                //checkerboard
                                _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), (x + y) % 2 == 0 ? Color.RosyBrown : Color.Tan);

                                //previous move
                                if (prevMove.from == new Point4(c, t, x, y) || prevMove.to == new Point4(c, t, x, y))
                                    _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.Yellow * 0.5f);

                                //available moves
                                if (availableMoves != null)
                                    if (selectedPos.HasValue)
                                    {
                                        if (availableMoves.Contains(new(selectedPos.Value, new(c, t, x, y))))
                                            _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.LightGreen * 0.5f);
                                    }
                                    else
                                    {
                                        if (availableMoves.Any(m => m.from == new Point4(c, t, x, y)))
                                            _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.LightGreen * 0.5f);
                                    }

                                //current piece
                                _spriteBatch.Draw(pieceTex, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), new Rectangle(spriteTexPos, new Point(pieceTexSize, pieceTexSize)), Color.White);

                                //selected piece
                                if (selectedPos == new Point4(c, t, x, y))
                                    _spriteBatch.Draw(selectTex, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.White);
                            }
                        }
                    }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}