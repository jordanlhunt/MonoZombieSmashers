using MapEditor.MapClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MapEditor
{
    public class Game1 : Game
    {
        #region Member Variables
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Text text;
        private SpriteFont spriteFont;
        Texture2D[] mapTextures;
        Texture2D nullTexture;
        Map map;
        #endregion
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            map = new Map();
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Fonts/RulerGold");
            text = new Text(_spriteBatch, spriteFont);
            nullTexture = Content.Load<Texture2D>(@"MapEditor/Graphics/1x1");
            mapTextures = new Texture2D[1];
            for (int i = 0; i < mapTextures.Length; i++)
            {
                mapTextures[i] = Content.Load<Texture2D>(
                    @"MapEditor/Graphics/maps" + (i + 1).ToString()
                );
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)
            )
                Exit();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawMapElements();
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        #region Private Methods
        private void DrawMapElements()
        {
            Rectangle sourceRectangle = new Rectangle();
            Rectangle destinationRectangle = new Rectangle();
            int windowOffSetX = _graphics.PreferredBackBufferWidth - 256;
            text.Size = 1f;
            const int NUMBER_OF_MAP_ELEMENTS = 9;

            _spriteBatch.Draw(
                nullTexture,
                new Rectangle(windowOffSetX, 20, 280, 550),
                new Color(0, 0, 0, 100)
            );
            for (int i = 0; i < NUMBER_OF_MAP_ELEMENTS; i++)
            {
                MapElement mapElement = map.MapElements[i];
                if (mapElement == null)
                {
                    continue;
                }
                destinationRectangle.X = windowOffSetX;
                destinationRectangle.Y = 50 + i * 60;
                sourceRectangle = mapElement.SourceRectangle;
                if (sourceRectangle.Width > sourceRectangle.Height)
                {
                    destinationRectangle.Width = 45;
                    destinationRectangle.Height = (int)(
                        ((float)sourceRectangle.Height / (float)sourceRectangle.Height) * 45f
                    );
                }
                else
                {
                    destinationRectangle.Height = 45;
                    destinationRectangle.Width = (int)(
                        ((float)sourceRectangle.Height / (float)sourceRectangle.Height) * 45f
                    );
                }
                _spriteBatch.Draw(
                    mapTextures[mapElement.SourceIndex],
                    destinationRectangle,
                    Color.White
                );
                text.Color = Color.Gold;
                text.DrawText(
                    new Vector2(destinationRectangle.X + 50, destinationRectangle.Y),
                    mapElement.Name
                );
            }
        }
        #endregion
    }
}
