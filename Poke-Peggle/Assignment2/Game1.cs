#region File Description
//-----------------------------------------------------------------------------
// Game1.cs
//
// Umer Noor 07 February 2014. 
// Converted original XNA tutorial from 
// "Beginners Guide 2D Games for Windows" Chapter Nine to MonoGame 
// XNA is dead, long live MonoGame!
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
#endregion

namespace GameDynamics
{
    public enum GameState
    {
        SplashScreen,
        Play,
        Win,
        GameOver
    }

    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState previousKeyboardState = Keyboard.GetState();
        GameState gameState;

        Rectangle viewportRect;

        Texture2D backgroundTexture;
        Texture2D dittoTexture;
        Texture2D ballHitTexture;
        Sprite cannon;
        
        const int maxpokeBalls = 1;
        const float restitution = 1.0f;
        const float sfxVolume = 0.6f;
        int livesIndex;
        List<Sprite> pokeBalls = new List<Sprite>();
        List<Sprite> enemies = new List<Sprite>();
        
        int score;
        Vector2 scoreDrawPoint = new Vector2(5f, 5f);
        Vector2 LivesDrawPoint = new Vector2(645f, 5f);
        SpriteFont titleFont, gameFont;


        float fTimeIntervalSecs;
        float elapsed;
        Vector2 gravityForce;

        float tileWidth, tileHeight;
        int enemyHeight;

        Color titleColor, nextColor;
        Random rng = new Random();
        int r, g, b;
        int r2, g2, b2;

        Song song;
        SoundEffect hitBall, hitWall, hitDitto;


        public Game1()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.PreferredBackBufferHeight = 480;
            //graphics.PreferredBackBufferWidth = 800;
            Window.IsBorderless = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            gameState = GameState.SplashScreen;
            fTimeIntervalSecs = 0f; // initialize time step

            tileWidth = graphics.GraphicsDevice.Viewport.Width / 15;
            tileHeight = graphics.GraphicsDevice.Viewport.Height / 10;

            enemyHeight = 34;

            titleColor = getColor();

            gravityForce = new Vector2(0.0f, 500.0f); // set gravity
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            backgroundTexture = Content.Load<Texture2D>("pokemonStadium");
            dittoTexture = Content.Load<Texture2D>("ditto");
            ballHitTexture = Content.Load<Texture2D>("ballLight");

            cannon = new Sprite(Content.Load<Texture2D>("cannon"));
            cannon.Position = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, 50);
            
            
            for (int i = 0; i < maxpokeBalls; i++)
            {
                pokeBalls.Add(new Sprite(Content.Load<Texture2D>("masterBall")));
            }

            SetEnemies();
            
            //drawable area of the game screen.
            viewportRect = new Rectangle(0, 0,
                graphics.GraphicsDevice.Viewport.Width,
                graphics.GraphicsDevice.Viewport.Height);

            hitBall = Content.Load<SoundEffect>("hitWall");
            hitWall = Content.Load<SoundEffect>("hit");
            hitDitto = Content.Load<SoundEffect>("dittoHit");

            song = Content.Load<Song>("PoStadium");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);

            gameFont = Content.Load<SpriteFont>("Fonts/guiFont");
            titleFont = Content.Load<SpriteFont>("Fonts/titleFont");

            base.LoadContent();
        }


        public void UpdatePokeballs()
        {
            foreach (Sprite ball in pokeBalls)
            {

                // calculates the forces acting on the ball
                ball.Force = gravityForce;
                // calculates the acceleration of the ball using f = ma
                ball.Acceleration = ball.Force / ball.Mass;
                // vf = vi + a*t
                ball.Velocity = ball.InitialVelocity + (ball.Acceleration * fTimeIntervalSecs);
                // s = vi*t + 1/2*a*t^2
                ball.Position += (ball.InitialVelocity * fTimeIntervalSecs) + (0.5f * ball.Acceleration * fTimeIntervalSecs * fTimeIntervalSecs);

                // collision detection
                if (ball.Alive)
                {   
                    BoundingSphere pokeBallsphere = new BoundingSphere(new Vector3(ball.Position.X + ball.Center.X, ball.Position.Y + ball.Center.Y, 0f), ball.Image.Height / 2);

                    foreach (Sprite enemy in enemies)
                    {
                        if (pokeBallsphere.Intersects(enemy.bounds))
                        {
                            // calculates collision
                            if (enemy.Ditto)
                            {
                                hitDitto.Play(sfxVolume, 0f, 0f);
                            }
                            else if (!enemy.Ditto)
                            {
                                hitBall.Play(sfxVolume / 2, 0f, 0f);
                            }
                            ball.HasBeenHit = true;
                            enemy.HasBeenHit = true;
                            ball.resolveStaticCollision(enemy, restitution);
                            score++;
                            break;
                        }
                    }
                }

                // rotates the ball
                if (ball.HasBeenHit)
                {
                    ball.Rotation += 0.1f * ball.orientation;
                }

                // Ball bounces off of the side of the screen
                if (ball.Position.X < 0 || ball.Position.X + ball.Image.Width > viewportRect.Width)
                {
                    hitWall.Play(sfxVolume, 0f, 0f);
                    Vector2 velo = (new Vector2(ball.Velocity.X * -1, ball.Velocity.Y)) * restitution;
                    ball.Velocity = velo;
                }
                else if (ball.Position.Y < 0 && ball.Alive)
                {
                    hitWall.Play(sfxVolume, 0f, 0f);
                    Vector2 velo = (new Vector2(ball.Velocity.X, ball.Velocity.Y * -1)) * restitution;
                    ball.Velocity = velo;
                }
                // Ball is killed if it is below the screen
                if (ball.Position.Y > GraphicsDevice.Viewport.Height)
                {
                     ball.Alive = false;

                     if (livesIndex == 0)
                     {
                         gameState = GameState.GameOver;
                     }
                     else if (enemies.Count == 0)
                     {
                         gameState = GameState.Win;
                     }
                }

                // set vi to vf for the next timestep
                ball.InitialVelocity = ball.Velocity;
            }
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (gameState != GameState.Play)
            {
                UpdateSplashScreen();
                elapsed += (float)gameTime.ElapsedGameTime.Milliseconds / 1000.0f;

                if (elapsed > 1 || (r == r2 && g == g2 && b == b2))
                {
                    elapsed = 0f;
                    nextColor = getNewColor();
                }
                else
                {
                    titleColor = transitionColor(r, g, b);
                }
                
            }
            else
            {
                // TODO: Add your update logic here
                KeyboardState keyboardState = Keyboard.GetState();
                if (keyboardState.IsKeyDown(Keys.Left) ||
                    keyboardState.IsKeyDown(Keys.A))
                {
                    cannon.Rotation += 0.1f;
                }
                if (keyboardState.IsKeyDown(Keys.Right) ||
                    keyboardState.IsKeyDown(Keys.D))
                {
                    cannon.Rotation -= 0.1f;
                }
                if (keyboardState.IsKeyDown(Keys.Space) &&
                    previousKeyboardState.IsKeyUp(Keys.Space))
                {
                    FireCannonBall();
                }

                cannon.Rotation = MathHelper.Clamp(cannon.Rotation, 0, MathHelper.Pi);

                fTimeIntervalSecs = ((float)gameTime.ElapsedGameTime.Milliseconds) / 1000.0f;

                UpdatePokeballs();
                UpdateEnemies();

                previousKeyboardState = keyboardState;
            }
            base.Update(gameTime);
        }

        // Draws the grid of pegs
        public void SetEnemies()
        {
            if (enemies.Count > 0)
            {
                enemies = new List<Sprite>();
            }
            else
            {
                for (int x = 1; x < 15; x++)
                {
                    for (int y = 5; y < 10; y++)
                    {
                        Sprite enemy = new Sprite(Content.Load<Texture2D>("ball"), getGridPos(x, y));
                        if (rng.Next(100) > 75)
                        {
                            enemy.Ditto = true;
                        }
                        enemies.Add(enemy);
                    }
                }
            }
        }

        // helper function for set enemies to convert grid coordinates to screen coordinates
        public Vector2 getGridPos(int x, int y)
        {
            return new Vector2(x * tileWidth, (y * tileHeight));
        }

        // update enemy function
        public void UpdateEnemies()
        {
            foreach (Sprite enemy in enemies)
            {
                // if the peg has been hit it will drop off the screen and shrink
                if (enemy.HasBeenHit)
                {
                    if(enemy.Ditto)
                    {
                        enemy.Image = dittoTexture;
                    }
                    else if (!enemy.Ditto)
                    {
                        enemy.Image = ballHitTexture;
                    }
                    enemy.Rotation += 0.05f * enemy.orientation;
                    enemy.Force = gravityForce;
                    if (enemy.Scale > 0.6f)
                    {
                        enemy.Scale -= 0.02f;
                    }
                }

                // calculates the acceleration of the enemy using f = ma
                enemy.Acceleration = enemy.Force / enemy.Mass;
                // vf = vi + a*t
                enemy.Velocity = enemy.InitialVelocity + (enemy.Acceleration * fTimeIntervalSecs);
                // s = vi*t + 1/2*a*t^2
                enemy.Position += (enemy.InitialVelocity * fTimeIntervalSecs) + (0.5f * enemy.Acceleration * fTimeIntervalSecs * fTimeIntervalSecs);

                // set vi to vf for the next timestep
                enemy.InitialVelocity = enemy.Velocity;
            }            

            // when the enemy is below the screen, it is removed from the list
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].Position.Y + enemyHeight > viewportRect.Height)
                {
                    enemies.RemoveAt(i);
                }
            }
        }

        public void FireCannonBall()
        {
            foreach (Sprite ball in pokeBalls)
            {
                if (!ball.Alive && livesIndex != 0)
                {
                    ball.Alive = true;
                    livesIndex--;
                    ball.Position = cannon.Position - ball.Center;
                    ball.InitialVelocity = new Vector2(
                        (float)Math.Cos(cannon.Rotation),
                        (float)Math.Sin(cannon.Rotation)) * 500.0f; // set the force of the shot
                    return;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();

            //Draw the backgroundTexture sized to the width and height of the screen.
            spriteBatch.Draw(backgroundTexture, viewportRect, Color.White);

            if (gameState == GameState.SplashScreen)
            {
                spriteBatch.DrawString(titleFont, "Poke", new Vector2(150, 25), titleColor);
                spriteBatch.DrawString(titleFont, "Peggle", new Vector2(50, 200), titleColor);
            }
            else if (gameState == GameState.Win)
            {
                DrawRectangle(viewportRect, Color.White);
                spriteBatch.DrawString(titleFont, "You Win!", new Vector2(45, viewportRect.Height / 3), titleColor);
            }
            else if (gameState == GameState.GameOver)
            {
                DrawRectangle(viewportRect, Color.Black);
                spriteBatch.DrawString(titleFont, "Game", new Vector2(150, 50), titleColor);
                spriteBatch.DrawString(titleFont, "Over", new Vector2(150, 250), titleColor);
            }
            else if (gameState == GameState.Play)
            {
                foreach (Sprite ball in pokeBalls)
                {
                    if (ball.Alive)
                    {
                        ball.Draw(spriteBatch);
                    }
                }

                spriteBatch.Draw(cannon.Image,
                    cannon.Position,
                    null,
                    Color.White,
                    cannon.Rotation,
                    cannon.Center, 1.0f,
                    SpriteEffects.None, 0);

                foreach (Sprite enemy in enemies)
                {
                    if (enemy.Alive)
                    {
                        enemy.Draw(spriteBatch);
                    }
                }

                spriteBatch.DrawString(gameFont, "Score: " + score.ToString(), scoreDrawPoint, Color.White);
                spriteBatch.DrawString(gameFont, "Balls: " + livesIndex.ToString(), LivesDrawPoint, Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public void UpdateSplashScreen()
        {
            KeyboardState keyState = Keyboard.GetState();
            bool keyPressed = (Keyboard.GetState().GetPressedKeys().Length > 0);

            if (keyPressed)
            {
                if (gameState == GameState.SplashScreen)
                {
                    resetLives();
                    gameState = GameState.Play;
                }
                else if (keyState.IsKeyDown(Keys.Enter) && (gameState == GameState.GameOver || gameState == GameState.Win))
                {
                    score = 0;
                    resetLives();
                    SetEnemies();
                    gameState = GameState.SplashScreen;
                }
            }
        }

        public void resetLives()
        {
            livesIndex = 5;
        }
        
        public Color getColor()
        {
            r = rng.Next(0, 255);
            g = rng.Next(0, 255);
            b = rng.Next(0, 255);

            return new Color(r, g, b);
        }

        public Color getNewColor()
        {
            r2 = rng.Next(0, 255);
            g2 = rng.Next(0, 255);
            b2 = rng.Next(0, 255);

            return new Color(r2, g2, b2);
        }

        public Color transitionColor(int red, int green, int blue)
        {
            if (red - r2 > 0)
                red--;
            else
                red++;

            if (green - g2 > 0)
                green--;
            else
                green++;

            if (blue - b2 > 0)
                blue--;
            else
                blue++;

            r = red;
            g = green;
            b = blue;

            return new Color(red, green, blue);
        }

        private void DrawRectangle(Rectangle coords, Color color)
        {
            var rect = new Texture2D(GraphicsDevice, 1, 1);
            rect.SetData(new[] { color });
            spriteBatch.Draw(rect, coords, color);
        }
    }
}
