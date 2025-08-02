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
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Fonts/RulerGold");
            text = new Text(_spriteBatch, spriteFont);
            nullTexture = Content.Load<Texture2D>(@"Graphics/1x1");
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
            GraphicsDevice.Clear(Color.CornflowerBlue);
            text.Size = 2.25f;
            text.Color = Color.Black;
            for (int i = 0; i < 3; i++)
            {
                if (i == 2)
                {
                    text.Color = Color.White;
                }
                text.DrawText(
                    new Vector2(25 - i * 2, 250 - i * 2),
                    "Zombie Smashers Monogame Stays Winnin'!"
                );
            }
            base.Draw(gameTime);
        }
    }
}
