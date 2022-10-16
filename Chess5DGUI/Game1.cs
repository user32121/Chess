using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Chess5DGUI
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D blank;
        private Texture2D pieceTex;
        private Texture2D selectTex;
        private const int pieceTexSize = 320;
        public const int pieceDrawSize = 64;
        private SpriteFont font;
        private const float colorBorderWidth = 0.3f;

        private readonly GameBoard board = StartingBoards.Standard;
        private Point4? selectedPos;
        private readonly List<Point4> prevMove = new();
        private List<Move> availableMoves;

        private MouseState prevMS;
        private KeyboardState prevKS;

        private Vector2 targetViewOffset, viewOffset;
        private const float offsetSmoothingFactor = 0.92f;
        private const int moveSpeed = 20;
        private float targetZoomValue = defaultZoomValue, zoomValue, zoom;
        private const int defaultZoomValue = -400;
        private const float zoomFactor = 0.999f;
        private const float zoomSmoothingFactor = 0.9f;

        public bool isWhitePerspective = true;

        private const bool algoEnabled = true;
        private Thread algoThread;
        private readonly Stopwatch stopwatch = new();
        private readonly List<(float, Move, long)> algoEval = new();
        private bool resetAlgoEval = true;
        private bool stopAlgo = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            Window.Title = "5D Chess";

            _graphics.PreferredBackBufferHeight = 520;
            _graphics.ApplyChanges();

            SetTargetViewOffset(0, 0);

            if (algoEnabled)
            {
                algoThread = new Thread(AlgoThreadFunction);
                algoThread.Start(this);
            }

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

        private void AlgoThreadFunction(object game1Obj)
        {
            Game1 game1 = (Game1)game1Obj;
            GameBoard workingBoard = null;
            int depth = 0;

            while (!stopAlgo)
            {
                if (resetAlgoEval)
                {
                    resetAlgoEval = false;
                    depth = 0;
                    lock (algoEval)
                        algoEval.Clear();
                    workingBoard = board.Clone();
                }
                if (depth > 200)  //may occur if no valid moves
                    continue;

                int minTurn = workingBoard.boards.Min(timeline => timeline.Count);
                Move move;
                float score;
                stopwatch.Restart();
                if (depth == 0)
                {
                    move = Move.Invalid;
                    score = Algorithm2.GetStaticScore(workingBoard);
                }
                else
                    (move, score) = Algorithm2.GetBestMove(workingBoard, minTurn % 2 == 1, depth, ref resetAlgoEval);
                stopwatch.Stop();
                lock (algoEval)
                    algoEval.Add((score, move, stopwatch.ElapsedMilliseconds));
                depth++;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            availableMoves ??= Utils.GetAllMoves(board);

            MouseState ms = Mouse.GetState();
            if (ms.LeftButton == ButtonState.Pressed && prevMS.LeftButton == ButtonState.Released)
            {
                Point clickPos = Utils.ScreenToWorldSpace(ms.Position.ToVector2(), this, viewOffset, zoom).ToPoint();
                Point4 clickTile;
                if (isWhitePerspective)
                    clickTile = new(board.boards.Count - 1 - clickPos.Y / pieceDrawSize / (board.height + 1), clickPos.X / pieceDrawSize / (board.width + 1), clickPos.X / pieceDrawSize % (board.width + 1), board.height - 1 - clickPos.Y / pieceDrawSize % (board.height + 1));
                else
                    clickTile = new(clickPos.Y / pieceDrawSize / (board.height + 1), clickPos.X / pieceDrawSize / (board.width + 1), board.width - 1 - clickPos.X / pieceDrawSize % (board.width + 1), clickPos.Y / pieceDrawSize % (board.height + 1));
                if (selectedPos.HasValue)
                {
                    Move move = new(selectedPos.Value, clickTile);
                    if (availableMoves.Contains(move))
                        PerformMove(move);
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
                SetTargetViewOffset((board.boards.Count - 1) / 2f, board.boards.Max(b => b.Count) - 1);
                targetZoomValue = defaultZoomValue;
            }
            if (ks.IsKeyDown(Keys.X) && prevKS.IsKeyUp(Keys.X))
            {
                Move move = algoEval.LastOrDefault((0, Move.Invalid, 0)).Item2;
                if (availableMoves.Contains(move))
                    PerformMove(move);
            }
            if (ks.IsKeyDown(Keys.P) && selectedPos.HasValue)
            {
                Move move = new(selectedPos.Value, selectedPos.Value);
                PerformMove(move);
            }
            prevKS = ks;

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

            Point viewWorldPos = Utils.ScreenToWorldSpace(new Vector2(0, 0), this, viewOffset, zoom).ToPoint();
            Rectangle viewWorldRect = new(viewWorldPos, Utils.ScreenToWorldSpace(GraphicsDevice.Viewport.Bounds.Size.ToVector2(), this, viewOffset, zoom).ToPoint() - viewWorldPos);

            for (int c = 0; c < board.boards.Count; c++)
                for (int t = 0; t < board.boards[c].Count; t++)
                    if (board.boards[c][t] != null)
                    {
                        Point drawBoardPos;
                        if (isWhitePerspective)
                            drawBoardPos = new(t * pieceDrawSize * (board.width + 1), (board.boards.Count - c - 1) * pieceDrawSize * (board.height + 1));
                        else
                            drawBoardPos = new(t * pieceDrawSize * (board.width + 1), c * pieceDrawSize * (board.height + 1));
                        Rectangle drawBoardRect = new(drawBoardPos, new Point(pieceDrawSize * (board.width + 1), pieceDrawSize * (board.height + 1)));
                        if (!viewWorldRect.Intersects(drawBoardRect))
                            continue;
                        int borderPixelWidth = (int)(pieceDrawSize * colorBorderWidth);
                        _spriteBatch.Draw(blank, new Rectangle(drawBoardPos.X - borderPixelWidth, drawBoardPos.Y - borderPixelWidth, board.width * pieceDrawSize + borderPixelWidth * 2, board.height * pieceDrawSize + borderPixelWidth * 2), t % 2 == 0 ? Color.White : Color.Black);

                        for (int x = 0; x < board.width; x++)
                        {
                            for (int y = 0; y < board.height; y++)
                            {
                                Point drawPiecePos;
                                if (isWhitePerspective)
                                    drawPiecePos = new(x * pieceDrawSize + drawBoardPos.X, (board.height - y - 1) * pieceDrawSize + drawBoardPos.Y);
                                else
                                    drawPiecePos = new((board.width - x - 1) * pieceDrawSize + drawBoardPos.X, y * pieceDrawSize + drawBoardPos.Y);
                                Point spriteTexPos = Utils.pieceTexIndex[board[c, t, x, y]];

                                //checkerboard
                                _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), (x + y) % 2 == 0 ? Color.RosyBrown : Color.Tan);

                                //previous move
                                if (prevMove.Contains(new Point4(c, t, x, y)))
                                    _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.Yellow * 0.5f);

                                //suggested move
                                (_, Move suggestedMove, _) = algoEval.LastOrDefault((0, Move.Invalid, 0));
                                if (suggestedMove.from == new Point4(c, t, x, y) || suggestedMove.to == new Point4(c, t, x, y))
                                    _spriteBatch.Draw(blank, new Rectangle(drawPiecePos.X, drawPiecePos.Y, pieceDrawSize, pieceDrawSize), Color.LightBlue * 1f);
                                else if (availableMoves != null)
                                    //available moves
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

            //UI
            _spriteBatch.Begin();
            Vector2 drawPos = new(10);
            lock (algoEval)
            {
                for (int i = 0; i < algoEval.Count; i++)
                {
                    (float score, Move move, long milliseconds) = algoEval[i];
                    string text;
                    if (move.from != Move.Invalid.from)
                        text = string.Format("{0}: {1:f} ({2}) ({3}s)", i, score, move, milliseconds / 1000f);
                    else
                        text = string.Format("{0}: {1:f}", i, score);
                    _spriteBatch.DrawString(font, text, drawPos, Color.Black);
                    drawPos.Y += font.LineSpacing;
                }
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            resetAlgoEval = true;
            stopAlgo = true;

            base.OnExiting(sender, args);
        }

        private void SetTargetViewOffset(float c, float t)
        {
            if (isWhitePerspective)
                targetViewOffset = new(t * pieceDrawSize * (board.width + 1), (board.boards.Count - c - 1) * pieceDrawSize * (board.height + 1));
            else
                targetViewOffset = new(t * pieceDrawSize * (board.width + 1), c * pieceDrawSize * (board.height + 1));
        }

        private void PerformMove(Move move)
        {
            Move actualMove = Utils.PerformMove(board, move);

            resetAlgoEval = true;
            availableMoves = null;
            selectedPos = null;

            if (actualMove.to.c == 0 && actualMove.from.c != move.to.c)
                for (int i = 0; i < prevMove.Count; i++)
                {
                    Point4 p = prevMove[i];
                    p.c++;
                    prevMove[i] = p;
                }

            prevMove.Add(actualMove.from);
            prevMove.Add(actualMove.to);

            SetTargetViewOffset(actualMove.to.c, actualMove.to.t);
        }
    }
}