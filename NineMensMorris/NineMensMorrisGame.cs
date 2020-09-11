using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NineMensMorris
{
    public class NineMensMorrisGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        readonly Point tokenSize;
        readonly Point boardSize;
        readonly Point boardLocation;

        Texture2D token;

        Texture2D boardBlack;
        Texture2D line;

        Dictionary<Point, MillPoint> gameBoard;

        bool IsFirstPlayersTurn = true;
        Player CurrentPlayer;

        Player player1;
        Player player2;

        SpriteFont font;
        SpriteFont bigFont;

        GamePhase gamePhase;

        Texture2D resetButton;
        Rectangle resetButtonPosition;

        int nmbTokensToDiscard = 0;

        MouseState previousState;
        bool invalidDiscard;

        TimeSpan messageDuration;
        TimeSpan elapsedTime;

        MillPoint selectedToken;
        Vector2 position;

        Dictionary<int, int> xKoords;
        Dictionary<int, int> yKoords;

        Player winner;

        Texture2D winnerTexture;
        Texture2D playAgain;
        Rectangle playAgainPosition;

        public NineMensMorrisGame()
        { 
            Window.Title = "Nine Men's Morris";

            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 800;
            graphics.PreferredBackBufferWidth = 1300;

            Content.RootDirectory = "Content";

            tokenSize = new Point(50, 50);
            boardSize = new Point(750, 700);
            boardLocation = new Point(20, 30);

            IsMouseVisible = true;

            player1 = new Player(1, Color.OrangeRed);
            player2 = new Player(2, Color.SpringGreen);
            CurrentPlayer = player1;

            gamePhase = GamePhase.Placing;

            invalidDiscard = false;
            messageDuration = TimeSpan.FromSeconds(3);
            elapsedTime = TimeSpan.Zero;

            selectedToken = null;
            position = new Vector2(0, 0);
        }

        protected override void Initialize()
        {
            previousState = Mouse.GetState();

            CreateBoard();

            line = new Texture2D(this.GraphicsDevice, 1, 1);
            line.SetData(new[] { Color.White });

            resetButtonPosition = new Rectangle(860, 30, 200, 50);
            playAgainPosition = new Rectangle(270, 550, 250, 100);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            boardBlack = this.Content.Load<Texture2D>("board_black");
            token = this.Content.Load<Texture2D>("gray_icon");
            font = Content.Load<SpriteFont>("nineMensMorris");
            resetButton = this.Content.Load<Texture2D>("reset_button");
            bigFont = this.Content.Load<SpriteFont>("bigFont");
            winnerTexture = this.Content.Load<Texture2D>("winner");
            playAgain = this.Content.Load<Texture2D>("playAgain");
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (invalidDiscard) elapsedTime += gameTime.ElapsedGameTime;
            if (elapsedTime >= messageDuration)
            {
                invalidDiscard = false;
                elapsedTime = TimeSpan.Zero;
            }

            var mouseState = Mouse.GetState();
            //System.Diagnostics.Debug.WriteLine(mouseState.X.ToString() + "," + mouseState.Y.ToString());

            Mouse.SetCursor(IsInClickableArea(mouseState.Position) ? MouseCursor.Hand : MouseCursor.Arrow);

            if (mouseState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                if (resetButtonPosition.Contains(mouseState.Position) || (gamePhase == GamePhase.END && playAgainPosition.Contains(mouseState.Position)))
                {
                    RestartGame();
                }
                else
                {
                    switch (gamePhase)
                    {
                        case GamePhase.Placing:
                            UpdatePlacingPhase(mouseState);
                            break;
                        case GamePhase.Moving:
                            UpdateMovingPhase();
                            break;
                        case GamePhase.Discarding:
                            UpdateDiscardingPhase(mouseState);
                            break;
                        case GamePhase.END:
                        case GamePhase.Flying:
                        default:
                            gamePhase = GamePhase.END;
                            break;
                    }
                }
            }
            else if (gamePhase == GamePhase.Moving && mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Released)
            {
                //select the token
                SelectTokenToMove(mouseState);
            }
            else if (gamePhase == GamePhase.Moving && mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton == ButtonState.Pressed && selectedToken != null)
            {
                //move the token if one is selected
                UpdatePositionForMovingToken(mouseState);
            }

            base.Update(gameTime);

            previousState = mouseState;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            spriteBatch.Draw(boardBlack, new Rectangle(boardLocation ,boardSize), Color.White);

            spriteBatch.Draw(resetButton, resetButtonPosition, Color.White);

            spriteBatch.DrawString(font, GameText.GetGamePhase(gamePhase), new Vector2(840, 120), Color.Black);

            spriteBatch.DrawString(font, CurrentPlayer.Name + "'s turn", new Vector2(880, 170), CurrentPlayer.Color);

            spriteBatch.Draw(line, new Rectangle(830, 210, 300, 2), Color.LightGray);

            spriteBatch.DrawString(font, player1.Name, new Vector2(850, 260), player1.Color);
            spriteBatch.DrawString(font, " - available tokens: " + player1.NotPlacedTokens.ToString(), new Vector2(860, 290), Color.Black);
            spriteBatch.DrawString(font, " - discarded tokens: " + player1.DiscardedTokens.ToString(), new Vector2(860, 315), Color.Black);

            spriteBatch.DrawString(font, player2.Name, new Vector2(850, 420), player2.Color);
            spriteBatch.DrawString(font, " - available tokens: " + player2.NotPlacedTokens.ToString(), new Vector2(860, 450), Color.Black);
            spriteBatch.DrawString(font, " - discarded tokens: " + player2.DiscardedTokens.ToString(), new Vector2(860, 475), Color.Black);

            foreach (var mp in gameBoard.Values)
            {
                if (mp.IsOccupied && (selectedToken == null || selectedToken.Id != mp.Id))
                {
                    var color = mp.Token.Owner.Color;
                    spriteBatch.Draw(token, new Rectangle(mp.Center.X - tokenSize.X/2, mp.Center.Y - tokenSize.Y/2, tokenSize.X, tokenSize.Y), color);
                }
            }

            if (selectedToken != null)
                spriteBatch.Draw(token, destinationRectangle: new Rectangle(position.ToPoint(), tokenSize), color: selectedToken.Token.Owner.Color, origin: new Vector2(token.Width / 2, token.Height / 2));

            if (invalidDiscard)
            {
                spriteBatch.Draw(line, new Rectangle(145, 200, 500, 120), Color.Beige);
                spriteBatch.DrawString(font, "INVALID MOVE !", new Vector2(300, 220), Color.Red);
                spriteBatch.DrawString(font, "a piece in an opponent's mill can only be removed", new Vector2(170, 250), Color.Red);
                spriteBatch.DrawString(font, "if no other pieces are available", new Vector2(220, 270), Color.Red);
            }

            if (gamePhase == GamePhase.END)
            {
                spriteBatch.Draw(winnerTexture, new Rectangle(200, 100, 400, 400), Color.White);
                spriteBatch.DrawString(bigFont, winner != null ? winner.Name : "", new Vector2(320, 300), Color.Black);
                spriteBatch.Draw(playAgain, playAgainPosition , Color.Green);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void CreateBoard()
        {
            #region Valid mill point ids
            var p00 = new Point(0, 0);
            var p30 = new Point(3, 0);
            var p60 = new Point(6, 0);
            var p11 = new Point(1, 1);
            var p31 = new Point(3, 1);
            var p51 = new Point(5, 1);
            var p22 = new Point(2, 2);
            var p32 = new Point(3, 2);
            var p42 = new Point(4, 2);
            var p03 = new Point(0, 3);
            var p13 = new Point(1, 3);
            var p23 = new Point(2, 3);
            var p43 = new Point(4, 3);
            var p53 = new Point(5, 3);
            var p63 = new Point(6, 3);
            var p24 = new Point(2, 4);
            var p34 = new Point(3, 4);
            var p44 = new Point(4, 4);
            var p15 = new Point(1, 5);
            var p35 = new Point(3, 5);
            var p55 = new Point(5, 5);
            var p06 = new Point(0, 6);
            var p36 = new Point(3, 6);
            var p66 = new Point(6, 6);
            #endregion

            #region Valid koordinates
            xKoords = new Dictionary<int, int>()
            {
                { 0, 51 },
                { 1, 158 },
                { 2, 262 },
                { 3, 396 },
                { 4, 528 },
                { 5, 630 },
                { 6, 738 }
            };
            yKoords = new Dictionary<int, int>()
            {
                { 0, 59 },
                { 1, 158 },
                { 2, 253 },
                { 3, 378 },
                { 4, 503 },
                { 5, 600 },
                { 6, 700 }
            };
            #endregion

            gameBoard = new Dictionary<Point, MillPoint>(){
                { p00, new MillPoint(p00, xKoords[0], yKoords[0])},
                { p30, new MillPoint(p30, xKoords[3], yKoords[0])},
                { p60, new MillPoint(p60, xKoords[6], yKoords[0])},

                { p11, new MillPoint(p11, xKoords[1], yKoords[1])},
                { p31, new MillPoint(p31, xKoords[3], yKoords[1])},
                { p51, new MillPoint(p51, xKoords[5], yKoords[1])},

                { p22, new MillPoint(p22, xKoords[2], yKoords[2])},
                { p32, new MillPoint(p32, xKoords[3], yKoords[2])},
                { p42, new MillPoint(p42, xKoords[4], yKoords[2])},

                { p03, new MillPoint(p03, xKoords[0], yKoords[3])},
                { p13, new MillPoint(p13, xKoords[1], yKoords[3])},
                { p23, new MillPoint(p23, xKoords[2], yKoords[3])},
                { p43, new MillPoint(p43, xKoords[4], yKoords[3])},
                { p53, new MillPoint(p53, xKoords[5], yKoords[3])},
                { p63, new MillPoint(p63, xKoords[6], yKoords[3])},

                { p24, new MillPoint(p24, xKoords[2], yKoords[4])},
                { p34, new MillPoint(p34, xKoords[3], yKoords[4])},
                { p44, new MillPoint(p44, xKoords[4], yKoords[4])},

                { p15, new MillPoint(p15, xKoords[1], yKoords[5])},
                { p35, new MillPoint(p35, xKoords[3], yKoords[5])},
                { p55, new MillPoint(p55, xKoords[5], yKoords[5])},

                { p06, new MillPoint(p06, xKoords[0], yKoords[6])},
                { p36, new MillPoint(p36, xKoords[3], yKoords[6])},
                { p66, new MillPoint(p66, xKoords[6], yKoords[6])}
            };

            #region Neighbourhood
            //                            up   right  down   left
            gameBoard[p00].SetNeighbours(null, gameBoard[p30], gameBoard[p03], null);
            gameBoard[p30].SetNeighbours(null, gameBoard[p60], gameBoard[p31], gameBoard[p00]);
            gameBoard[p60].SetNeighbours(null, null, gameBoard[p63], gameBoard[p30]);

            gameBoard[p11].SetNeighbours(null, gameBoard[p31], gameBoard[p13], null);
            gameBoard[p31].SetNeighbours(gameBoard[p30], gameBoard[p51], gameBoard[p32], gameBoard[p11]);
            gameBoard[p51].SetNeighbours(null, null, gameBoard[p53], gameBoard[p31]);

            gameBoard[p22].SetNeighbours(null, gameBoard[p32], gameBoard[p23], null);
            gameBoard[p32].SetNeighbours(gameBoard[p31], gameBoard[p42], null, gameBoard[p22]);
            gameBoard[p42].SetNeighbours(null, null, gameBoard[p43], gameBoard[p32]);

            gameBoard[p03].SetNeighbours(gameBoard[p00], gameBoard[p13], gameBoard[p06], null);
            gameBoard[p13].SetNeighbours(gameBoard[p11], gameBoard[p23], gameBoard[p15], gameBoard[p03]);
            gameBoard[p23].SetNeighbours(gameBoard[p22], null, gameBoard[p24], gameBoard[p13]);
            gameBoard[p43].SetNeighbours(gameBoard[p42], gameBoard[p53], gameBoard[p44], null);
            gameBoard[p53].SetNeighbours(gameBoard[p51], gameBoard[p63], gameBoard[p55], gameBoard[p43]);
            gameBoard[p63].SetNeighbours(gameBoard[p60], null, gameBoard[p66], gameBoard[p53]);

            gameBoard[p24].SetNeighbours(gameBoard[p23], gameBoard[p34], null, null);
            gameBoard[p34].SetNeighbours(null, gameBoard[p44], gameBoard[p35], gameBoard[p24]);
            gameBoard[p44].SetNeighbours(gameBoard[p43], null, null, gameBoard[p34]);

            gameBoard[p15].SetNeighbours(gameBoard[p13], gameBoard[p35], null, null);
            gameBoard[p35].SetNeighbours(gameBoard[p34], gameBoard[p55], gameBoard[p36], gameBoard[p15]);
            gameBoard[p55].SetNeighbours(gameBoard[p53], null, null, gameBoard[p35]);

            gameBoard[p06].SetNeighbours(gameBoard[p03], gameBoard[p36], null, null);
            gameBoard[p36].SetNeighbours(gameBoard[p35], gameBoard[p66], null, gameBoard[p06]);
            gameBoard[p66].SetNeighbours(gameBoard[p63], null, null, gameBoard[p36]);
            #endregion
        }
        private bool IsInClickableArea(Point mousePosition)
        {
            return resetButtonPosition.Contains(mousePosition) || gameBoard.Any(_ => _.Value.Contains(mousePosition)) || (gamePhase == GamePhase.END && playAgainPosition.Contains(mousePosition));
        }

        private void RestartGame()
        {
            gamePhase = GamePhase.Placing;
            IsFirstPlayersTurn = true;
            CurrentPlayer = player1;
            winner = null;

            foreach (var mp in gameBoard)
            {
                mp.Value.RemoveToken();
            }

            player1.Reset();
            player2.Reset();
        }

        private void UpdatePlacingPhase(MouseState mouseState)
        {
            //clicked non occupied millPoint
            var clickedMillPoint = gameBoard.Values
                .FirstOrDefault(_ => !_.IsOccupied &&  _.Contains(mouseState.Position));

            if (clickedMillPoint != null)
            {
                var playerToken = CurrentPlayer.GetToken();
                if (playerToken != null)
                {
                    clickedMillPoint.PlaceToken(playerToken);

                    CheckForDiscarding(clickedMillPoint);
                }
            }

            if (player1.NotPlacedTokens == 0 && player2.NotPlacedTokens == 0 && gamePhase == GamePhase.Placing)
            {
                gamePhase = GamePhase.Moving;
                CheckForEnd();
            }
        }
        private void CheckForDiscarding(MillPoint millPoint)
        {
            /* check if mill
             *  YES
             *  - same player turn
             *  - enable discarding
             *  NO 
             *  - next player turn
            */

            var mills = millPoint.GetNumberOfMills();
            if (mills > 0)
            {
                nmbTokensToDiscard = mills;
                gamePhase = GamePhase.Discarding;
            }
            else
            {
                IsFirstPlayersTurn = !IsFirstPlayersTurn;
                CurrentPlayer = IsFirstPlayersTurn ? player1 : player2;
            }
        }
        private void UpdateDiscardingPhase(MouseState mouseState)
        {
            //millPoint with an oponent's token
            var clickedMillPoint = gameBoard.Values
                .FirstOrDefault(_ => _.IsOccupied && _.Token.Owner.Id != CurrentPlayer.Id && _.Contains(mouseState.Position));

            if (clickedMillPoint != null)
            {
                //millPoints with tokens of the same user which are not part of any mills
                var freeTokenMillPoints = gameBoard.Values
                    .Any(_ => _.IsOccupied && _.Token.Owner.Id == clickedMillPoint.Token.Owner.Id && _.GetNumberOfMills() == 0);

                //invalid move: a piece in an opponent's mill can only be removed if no other pieces are available
                if (clickedMillPoint.GetNumberOfMills() > 0 && freeTokenMillPoints)
                {
                    invalidDiscard = true;
                }
                else
                {
                    clickedMillPoint.DiscardToken();
                    --nmbTokensToDiscard;
                }
            }

            if (nmbTokensToDiscard == 0)
            {
                IsFirstPlayersTurn = !IsFirstPlayersTurn;
                CurrentPlayer = IsFirstPlayersTurn ? player1 : player2;

                gamePhase = GamePhase.Placing;
                if (player1.NotPlacedTokens == 0 && player2.NotPlacedTokens == 0)
                {
                    gamePhase = GamePhase.Moving;
                    CheckForEnd();
                }
            }
        }
        private void CheckForEnd()
        {
            if (player1.NotPlacedTokens > 0 || player2.NotPlacedTokens > 0)
                return;

            var player1Tokens = gameBoard.Values.Where(_ => _.IsOccupied && player1.Id == _.Token.Owner.Id).ToList();
            var player1TooLittleTokens = player1Tokens.Count <= 2;
            var player1UnableToMove = player1Tokens.TrueForAll(_ => !_.CanMove());

            if (player1TooLittleTokens || player1UnableToMove)
            {
                gamePhase = GamePhase.END;
                winner = player2;
            }

            var player2Tokens = gameBoard.Values.Where(_ => _.IsOccupied && player2.Id == _.Token.Owner.Id).ToList();    
            var player2TooLittleTokens = player2Tokens.Count <= 2;
            var player2UnableToMove = player2Tokens.TrueForAll(_ => !_.CanMove());

            if (player2TooLittleTokens || player2UnableToMove)
            {
                gamePhase = GamePhase.END;
                winner = player1;
            }
        }
        
        private void SelectTokenToMove(MouseState mouseState)
        {
            //millPoint with this user's token
            var clickedMillPoint = gameBoard.Values
                .FirstOrDefault(_ => _.IsOccupied && _.Token.Owner.Id == CurrentPlayer.Id && _.Contains(mouseState.Position));

            if (clickedMillPoint != null)
            {
                selectedToken = clickedMillPoint;
                position.X = selectedToken.Center.X;
                position.Y = selectedToken.Center.Y;
            }
        }

        private void UpdatePositionForMovingToken(MouseState mouseState)
        {
            var diffX = Math.Abs(position.X - mouseState.X);
            var diffY = Math.Abs(position.Y - mouseState.Y);

            if (diffX >= diffY)
            {
                UpdatePositionX(mouseState, position.X < mouseState.X);
                UpdatePositionY(mouseState, position.Y < mouseState.Y);
            }
            else
            {
                UpdatePositionY(mouseState, position.Y < mouseState.Y);
                UpdatePositionX(mouseState, position.X < mouseState.X);
            }
        }
        private void UpdatePositionX(MouseState mouseState, bool moveToRight)
        {
            if ((int)position.Y == selectedToken.Center.Y)
            {
                if (moveToRight)
                {
                    var maxX = selectedToken.Center.X;
                    if (selectedToken.NeighbourRight != null)
                    {
                        maxX = selectedToken.NeighbourRight.IsOccupied ? selectedToken.NeighbourRight.AcceptanceArea.Left : selectedToken.NeighbourRight.Center.X;
                    }
                    position.X = Math.Min(maxX, mouseState.X);
                }
                else 
                {
                    var minX = selectedToken.Center.X;
                    if (selectedToken.NeighbourLeft != null)
                    {
                        minX = selectedToken.NeighbourLeft.IsOccupied ? selectedToken.NeighbourLeft.AcceptanceArea.Right : selectedToken.NeighbourLeft.Center.X;
                    }
                    position.X = Math.Max(minX, mouseState.X);
                }
            }
        }
        private void UpdatePositionY(MouseState mouseState, bool moveDown)
        {
            if ((int)position.X == selectedToken.Center.X)
            {
                if (moveDown)
                {
                    var maxY = selectedToken.Center.Y;
                    if (selectedToken.NeighbourDown != null)
                    {
                        maxY = selectedToken.NeighbourDown.IsOccupied ? selectedToken.NeighbourDown.AcceptanceArea.Top : selectedToken.NeighbourDown.Center.Y;
                    }
                    position.Y = Math.Min(maxY, mouseState.Y);
                }
                else
                {
                    var minY = selectedToken.Center.Y;
                    if (selectedToken.NeighbourUp != null)
                    {
                        minY = selectedToken.NeighbourUp.IsOccupied ? selectedToken.NeighbourUp.AcceptanceArea.Bottom : selectedToken.NeighbourUp.Center.Y;
                    }
                    position.Y = Math.Max(minY, mouseState.Y);
                }
            }
        }

        private void UpdateMovingPhase()
        {
            if (selectedToken == null)
                return;

            //place the token to new spot
            var newMillPoint = gameBoard.Values.FirstOrDefault(_ => !_.IsOccupied && _.Contains(position.ToPoint()));
            if (newMillPoint == null)
            {
                selectedToken = null;
                return;
            }

            var tokenToMove = selectedToken.Token;

            selectedToken.RemoveToken();
            newMillPoint.PlaceToken(tokenToMove);

            selectedToken = null;

            CheckForDiscarding(newMillPoint);
            CheckForEnd();
        }
    }
}
