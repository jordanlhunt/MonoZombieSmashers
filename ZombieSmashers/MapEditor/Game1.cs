using System;
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
        Texture2D iconTexture;
        MouseState currentMouseState;
        MouseState previousMouseState;
        int mouseX;
        int mouseY;
        int previousMouseX;
        int previousMouseY;
        int mouseDragMapSegment = -1;
        int mouseDragMapSegmentLayer = -1;
        int currentLayer = 1;
        bool isRightMouseDown;
        bool isMouseButtonDragging;
        bool isMouseClick;
        Map map;
        #endregion
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
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
            iconTexture = Content.Load<Texture2D>(@"MapEditor/Graphics/icons");
        }

        protected override void Update(GameTime gameTime)
        {
            if (
                GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape)
            )
            {
                Exit();
            }
            MouseUpdate();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            GraphicsDevice.Clear(Color.CornflowerBlue);
            DrawMapSegments();
            DrawCursor();
            map.Draw(_spriteBatch, mapTextures, new Vector2());
            _spriteBatch.End();
            base.Draw(gameTime);
        }

        #region Private Methods
        private void DrawMapSegments()
        {
            Rectangle sourceRectangle = new Rectangle();
            Rectangle destinationRectangle = new Rectangle();
            text.Size = 1f;
            const int NUMBER_OF_MAP_ELEMENTS = 9;
            _spriteBatch.Draw(
                nullTexture,
                new Rectangle(500, 20, 280, 550),
                new Color(0, 0, 0, 100)
            );
            for (int i = 0; i < NUMBER_OF_MAP_ELEMENTS; i++)
            {
                MapElement mapElement = map.MapElements[i];
                if (mapElement == null)
                {
                    continue;
                }
                destinationRectangle.X = 500;
                destinationRectangle.Y = 50 + i * 60;
                sourceRectangle = mapElement.SourceRectangle;
                if (sourceRectangle.Width > sourceRectangle.Height)
                {
                    destinationRectangle.Width = 45;
                    destinationRectangle.Height = (int)(
                        (float)sourceRectangle.Height / (float)sourceRectangle.Height * 45f
                    );
                }
                else
                {
                    destinationRectangle.Height = 45;
                    destinationRectangle.Width = (int)(
                        (float)sourceRectangle.Height / (float)sourceRectangle.Height * 45f
                    );
                }
                _spriteBatch.Draw(
                    mapTextures[mapElement.ElementIndex],
                    destinationRectangle,
                    sourceRectangle,
                    Color.White
                );
                text.Color = Color.Gold;
                text.DrawText(
                    new Vector2(destinationRectangle.X + 50, destinationRectangle.Y),
                    mapElement.Name
                );
            }
        }

        private void MouseUpdate()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            mouseX = currentMouseState.X;
            mouseY = currentMouseState.Y;
            bool isMouseClicked =
                previousMouseState.LeftButton == ButtonState.Released
                && currentMouseState.LeftButton == ButtonState.Pressed;
            bool isMouseClickReleased =
                previousMouseState.LeftButton == ButtonState.Pressed
                && currentMouseState.LeftButton == ButtonState.Released;
            if (isMouseClicked && !isMouseButtonDragging)
            {
                if (currentMouseState.X >= 500 && currentMouseState.X <= 780)
                {
                    int paletteIndex = (currentMouseState.Y - 50) / 60;
                    if (paletteIndex >= 0 && paletteIndex < 9)
                    {
                        int segmentIndex = map.AddMapSegment(currentLayer, paletteIndex);
                        if (segmentIndex >= 0)
                        {
                            isMouseButtonDragging = true;
                            mouseDragMapSegment = segmentIndex;
                            mouseDragMapSegmentLayer = currentLayer;
                            map.MapSegments[currentLayer, mouseDragMapSegment].Location =
                                new Vector2(currentMouseState.X - 16, currentMouseState.Y - 16);
                        }
                    }
                }
            }
            if (isMouseButtonDragging && mouseDragMapSegment >= 0 && currentLayer >= 0)
            {
                map.MapSegments[currentLayer, mouseDragMapSegment].Location = new Vector2(
                    currentMouseState.X - 16,
                    currentMouseState.Y - 16
                );
            }
            if (isMouseButtonDragging && isMouseClickReleased)
            {
                isMouseButtonDragging = false;
                mouseDragMapSegment = -1;
                mouseDragMapSegmentLayer = -1;
            }
        }

        private void DrawCursor()
        {
            int iconSize = 32;
            _spriteBatch.Draw(
                iconTexture,
                new Vector2(mouseX, mouseY),
                new Rectangle(0, 0, iconSize, iconSize),
                Color.White,
                0.0f,
                Vector2.Zero,
                1.0f,
                SpriteEffects.None,
                0.0f
            );
        }
        #endregion
    }
}
