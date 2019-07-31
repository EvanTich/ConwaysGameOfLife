using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace ConwaysGameOfLife {

    /// <summary>
    /// Conway's Game of Life. Simple concept.
    /// </summary>
    public class GameOfLife : Game {

        private const int MAP = 32;
        private const int SIZE = 16;
        private const string HELP = @"Help:
            Space to pause/unpause
            Left click to create life
            Right click to remove life
            Comma(,) to make each step faster
            Period(.) to make each step slower
            H to show/hide help";

        private static readonly Color MAP_COLOR = Color.White;
        private static readonly Color BOX_COLOR = Color.Black;
        private static readonly Color STEP_PROGRESS_COLOR = Color.Blue;
        private static readonly Color STEP_BACKGROUND_COLOR = Color.Gray;
        private static readonly Color STRING_COLOR = Color.Gray;

        private SpriteFont font;

        private bool[,] prevState;
        private bool[,] map;

        private Texture2D box;
        private Vector2 pos;

        private bool helpMenu;
        private bool paused;
        private Vector2 pausedPos;

        private double delta; // current time in this step
        private Rectangle deltaRect;
        private double timescale; // time per step

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public GameOfLife() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Initialize all needed values.
        /// </summary>
        protected override void Initialize() {
            map = new bool[MAP, MAP];
            prevState = new bool[MAP, MAP];

            box = new Texture2D(graphics.GraphicsDevice, SIZE, SIZE);
            pos = new Vector2(0, 0);

            // setup white box that can be used for anything
            Color[] data = new Color[SIZE * SIZE];
            for(int i = 0; i < data.Length; i++)
                data[i] = Color.White;
            box.SetData(data);

            helpMenu = true;
            paused = true;
            pausedPos = new Vector2(2, SIZE);

            delta = 0;
            deltaRect = new Rectangle(2, 2, SIZE * 4 - 4, SIZE - 4);
            timescale = .5;

            this.IsMouseVisible = true;

            // set window size too
            graphics.PreferredBackBufferHeight = graphics.PreferredBackBufferWidth = SIZE * MAP;
            graphics.ApplyChanges();

            base.Initialize();
        }

        /// <summary>
        /// Load the font.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font"); // default font
        }

        /// <summary>
        /// This does nothing.
        /// </summary>
        protected override void UnloadContent() { }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            InputHelper.Update();

            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            if(InputHelper.IsNewKeyPress(Keys.Space)) {
                paused = !paused;
            } else if(InputHelper.IsNewKeyPress(Keys.OemComma)) {
                timescale /= 1.25;
            } else if(InputHelper.IsNewKeyPress(Keys.OemPeriod)) {
                timescale *= 1.25;
            } else if(InputHelper.IsNewKeyPress(Keys.H)) {
                helpMenu = !helpMenu;
            }

            // mouse clicking
            if(InputHelper.IsNewMouseClick(MouseButton.LeftClick)) {
                // set y, x position to true
                int x = Mouse.GetState().Position.X / SIZE,
                    y = Mouse.GetState().Position.Y / SIZE;
                if(x >= 0 && x < MAP && y >= 0 && y < MAP) // bounds check
                    map[y, x] = true;
            } else if(InputHelper.IsNewMouseClick(MouseButton.RightClick)) {
                // set y, x position to false
                int x = Mouse.GetState().Position.X / SIZE,
                    y = Mouse.GetState().Position.Y / SIZE;
                if(x >= 0 && x < MAP && y >= 0 && y < MAP) // bounds check
                    map[y, x] = false;
            }

            if(!paused) {
                delta += gameTime.ElapsedGameTime.TotalSeconds;
                if(delta >= timescale) {
                    delta -= timescale;
                    Step();
                }

                deltaRect.Width = (int)((SIZE * 4 - 4) * delta / timescale);
            }
        }

        /// <summary>
        /// Draws the cells, step progress, paused notification, and help menu if applicable.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(MAP_COLOR);

            spriteBatch.Begin();

            pos.X = 0;
            pos.Y = 0;
            for(int i = 0; i < MAP; i++, pos.Y += SIZE)
                for(int j = 0; j < MAP; j++, pos.X = (pos.X + SIZE) % (MAP * SIZE))
                    if(map[i, j])
                        spriteBatch.Draw(box, pos, BOX_COLOR);

            spriteBatch.Draw(box, new Rectangle(0, 0, SIZE * 4, SIZE), new Color(STEP_BACKGROUND_COLOR, 200));
            spriteBatch.Draw(box, deltaRect, STEP_PROGRESS_COLOR);

            if(paused)
                spriteBatch.DrawString(font, "PAUSED", pausedPos, STRING_COLOR);

            if(helpMenu)
                spriteBatch.DrawString(font, HELP, Mouse.GetState().Position.ToVector2(), STRING_COLOR);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Population of surrounding cells, excluding the center cell.
        /// </summary>
        /// <param name="state">State of the map to check</param>
        /// <param name="y">Row position</param>
        /// <param name="x">Column position</param>
        /// <returns>Surrounding cell population, can be 0 to 8.</returns>
        private int Population(bool[,] state, int y, int x) {
            int count = 0;

            for(int i = -1; i <= 1; i++)
                for(int j = -1; j <= 1; j++)
                    if((y + i >= 0 && y + i < MAP) && // y + i in bounds
                       (x + j >= 0 && x + j < MAP) && // x + j in bounds
                       (i != 0 || j != 0) && // i and j are not 0
                       state[y + i, x + j]) // cell is alive
                        count++;

            return count;
        }

        /// <summary>
        /// Creates a new state from the previous state, following Conway's Game of Life rules:
        ///     1. Any live cell with fewer than two live neighbours dies, as if by underpopulation.
        ///     2. Any live cell with two or three live neighbours lives on to the next generation.
        ///     3. Any live cell with more than three live neighbours dies, as if by overpopulation.
        ///     4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
        /// See https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life for more information.
        /// </summary>
        private void Step() {
            // copy current state to previous state
            for(int i = 0; i < MAP; i++)
                for(int j = 0; j < MAP; j++)
                    prevState[i, j] = map[i, j];

            // do game of life
            for(int i = 0; i < MAP; i++)
                for(int j = 0; j < MAP; j++) {

                    // get population of surrounding cells
                    int pop = Population(prevState, i, j);
                    if(prevState[i, j] && (pop < 2 || pop > 3)) {
                        // underpopulation or overpopulation
                        map[i, j] = false;
                    } else if(!prevState[i, j] && pop == 3) { // techincally doesn't need !prevState[..] stuff, but its there
                        // reproduction
                        map[i, j] = true;
                    }
                }
        }
    }
}
