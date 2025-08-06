using System.Runtime.InteropServices.Marshalling;
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
        int mouseDragSegmentIndex = -1; // Replace your current dragging tracking
        int currentLayer = 1;
        bool isMouseButtonDragging;
        bool isMouseClicked;
        bool isMouseClickReleased;
        bool isMiddleMouseDown;
        Map map;

        Vector2 mapScroll;
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

            map.Draw(_spriteBatch, mapTextures, mapScroll);
            //map.Draw(_spriteBatch, mapTextures, mapScroll);
            DrawMapSegments();
            DrawLayerSwitchButton();
            DrawCursor();

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        #region Private Methods
        private void DrawMapSegments()
        {
            Rectangle sourceRectangle = new Rectangle();
            Rectangle destinationRectangle = new Rectangle();
            text.Size = 1f;
            int windowOffSetX = _graphics.PreferredBackBufferWidth - 290;
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
            UpdateMouseStates();
            HandleMapSegmentDragStart();
            HandlePaletteClick();
            UpdateDraggedSegmentPosition();
            HandleMouseRelease();
            HandleMapScrolling();
        }

        private void UpdateMouseStates()
        {
            previousMouseState = currentMouseState;
            previousMouseX = previousMouseState.X;
            previousMouseY = previousMouseState.Y;
            currentMouseState = Mouse.GetState();
            mouseX = currentMouseState.X;
            mouseY = currentMouseState.Y;
            isMiddleMouseDown = currentMouseState.MiddleButton == ButtonState.Pressed;
            isMouseClicked =
                previousMouseState.LeftButton == ButtonState.Released
                && currentMouseState.LeftButton == ButtonState.Pressed;
            isMouseClickReleased =
                previousMouseState.LeftButton == ButtonState.Pressed
                && currentMouseState.LeftButton == ButtonState.Released;
        }

        private void HandleMapSegmentDragStart()
        {
            int windowOffSetX = _graphics.PreferredBackBufferWidth - 290;
            if (isMouseClicked && !isMouseButtonDragging && mouseX < windowOffSetX)
            {
                int hoveredSegment = map.GetHoveredSegment(mouseX, mouseY, currentLayer, mapScroll);
                if (hoveredSegment != -1)
                {
                    isMouseButtonDragging = true;
                    mouseDragMapSegment = hoveredSegment;
                    mouseDragMapSegmentLayer = currentLayer;
                }
            }
        }

        private void HandlePaletteClick()
        {
            int windowOffSetX = _graphics.PreferredBackBufferWidth - 290;
            int paletteWidth = 280;
            if (
                isMouseClicked
                && !isMouseButtonDragging
                && mouseX >= windowOffSetX
                && mouseX <= windowOffSetX + paletteWidth
            )
            {
                int paletteIndex = (mouseY - 50) / 60;
                if (paletteIndex >= 0 && paletteIndex < 9)
                {
                    int segmentIndex = map.AddMapSegment(currentLayer, paletteIndex);
                    if (segmentIndex >= 0)
                    {
                        isMouseButtonDragging = true;
                        float layerScaler = .5f;
                        if (currentLayer == 0)
                            layerScaler = .375f;
                        else if (currentLayer == 2)
                            layerScaler = .625f;

                        mouseDragMapSegment = segmentIndex;
                        mouseDragMapSegmentLayer = currentLayer;
                        map.MapSegments[currentLayer, mouseDragMapSegment].Location = new Vector2(
                            (currentMouseState.X - 16) * layerScaler,
                            (currentMouseState.Y - 16) * layerScaler
                        );
                    }
                }
            }
        }

        private void UpdateDraggedSegmentPosition()
        {
            if (isMouseButtonDragging && mouseDragMapSegment >= 0 && currentLayer >= 0)
            {
                var segment = map.MapSegments[currentLayer, mouseDragMapSegment];
                if (segment != null)
                {
                    segment.Location = new Vector2(
                        currentMouseState.X - 16,
                        currentMouseState.Y - 16
                    );
                }
            }
        }

        private void HandleMouseRelease()
        {
            if (isMouseButtonDragging && isMouseClickReleased)
            {
                isMouseButtonDragging = false;
                mouseDragMapSegment = -1;
                mouseDragMapSegmentLayer = -1;
            }
        }

        private void HandleMapScrolling()
        {
            if (isMiddleMouseDown)
            {
                mapScroll.X -= (mouseX - previousMouseX) * 3.75f;
                mapScroll.Y -= (mouseY - previousMouseY) * 3.75f;
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

        private void DrawLayerSwitchButton()
        {
            string layerName = "map";
            switch (currentLayer)
            {
                case 0:
                    layerName = "background";
                    break;
                case 1:
                    layerName = "midground";
                    break;
                case 2:
                    layerName = "foreground";
                    break;
            }
            Vector2 layerNameVector = spriteFont.MeasureString("layer: " + layerName);
            Rectangle buttonRectangle = new Rectangle(
                5,
                5,
                (int)layerNameVector.X,
                (int)layerNameVector.Y
            );
            Point mouseCursor = new Point(currentMouseState.X, currentMouseState.Y);
            text.DrawText(new Vector2(5, 5), "layer: " + layerName);
            if (text.IsTextClicked(buttonRectangle, mouseCursor, isMouseClicked))
            {
                currentLayer = (currentLayer + 1) % 3;
            }
        }
        #endregion
    }
}
