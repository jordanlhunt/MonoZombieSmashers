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
        private Vector2 segmentDragOffset;
        MouseState previousMouseState;
        DrawingMode drawingMode;
        int mouseX;
        int mouseY;
        int previousMouseX;
        int previousMouseY;
        int mouseDragMapSegment = -1;
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
            drawingMode = DrawingMode.SegmentSelection;
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
            switch (drawingMode)
            {
                case DrawingMode.SegmentSelection:
                    DrawMapSegments();
                    break;
            }
            DrawMapGrid();
            DrawText();
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
                        sourceRectangle.Height / (float)sourceRectangle.Height * 45f
                    );
                }
                else
                {
                    destinationRectangle.Height = 45;
                    destinationRectangle.Width = (int)(
                        sourceRectangle.Height / (float)sourceRectangle.Height * 45f
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
            if (drawingMode == DrawingMode.SegmentSelection)
            {
                HandleMapSegmentDragStart();
                HandlePaletteClick();
                UpdateDraggedSegmentPosition();
            }
            else if (drawingMode == DrawingMode.CollisionMap && IsAbleToEdit())
            {
                // Convert mouse position to grid coordinates
                int gridX = (mouseX + (int)(mapScroll.X / 2)) / map.GridCellSize;
                int gridY = (mouseY + (int)(mapScroll.Y / 2)) / map.GridCellSize;
                // Ensure coordinates are within valid range
                if (gridX >= 0 && gridY >= 0 && gridX < map.GridSize && gridY < map.GridSize)
                {
                    if (currentMouseState.LeftButton == ButtonState.Pressed)
                    {
                        map.MapGrid[gridX, gridY] = 1;
                    }
                    else if (currentMouseState.RightButton == ButtonState.Pressed)
                    {
                        map.MapGrid[gridX, gridY] = 0;
                    }
                }
            }
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
            if (drawingMode == DrawingMode.SegmentSelection)
            {
                if (isMouseClicked && !isMouseButtonDragging && mouseX < windowOffSetX)
                {
                    int hoveredSegment = map.GetHoveredSegment(
                        mouseX,
                        mouseY,
                        currentLayer,
                        mapScroll
                    );
                    if (hoveredSegment != -1)
                    {
                        isMouseButtonDragging = true;
                        mouseDragMapSegment = hoveredSegment;
                        float layerScaler = GetLayerScaler();
                        Vector2 segmentScreenPos =
                            map.MapSegments[currentLayer, hoveredSegment].Location
                            - mapScroll * layerScaler;
                        segmentDragOffset = new Vector2(
                            segmentScreenPos.X - mouseX,
                            segmentScreenPos.Y - mouseY
                        );
                    }
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
                        float layerScaler = GetLayerScaler();
                        Rectangle sourceRectangle = map.MapElements[paletteIndex].SourceRectangle;
                        Vector2 worldPos = new Vector2(
                            mouseX + mapScroll.X * layerScaler,
                            mouseY + mapScroll.Y * layerScaler
                        );
                        Vector2 segmentLocation = new Vector2(
                            worldPos.X - sourceRectangle.Width * layerScaler / 2,
                            worldPos.Y - sourceRectangle.Height * layerScaler / 2
                        );
                        mouseDragMapSegment = segmentIndex;
                        map.MapSegments[currentLayer, mouseDragMapSegment].Location =
                            segmentLocation;
                        segmentDragOffset = new Vector2(
                            sourceRectangle.Width * layerScaler / 2,
                            sourceRectangle.Height * layerScaler / 2
                        );
                    }
                }
            }
        }

        private void UpdateDraggedSegmentPosition()
        {
            if (isMouseButtonDragging && mouseDragMapSegment >= 0 && currentLayer >= 0)
            {
                float layerScaler = GetLayerScaler();
                Vector2 worldPosition = new Vector2(
                    mouseX + mapScroll.X * layerScaler,
                    mouseY + mapScroll.Y * layerScaler
                );
                map.MapSegments[currentLayer, mouseDragMapSegment].Location =
                    worldPosition - segmentDragOffset;
            }
        }

        private void HandleMouseRelease()
        {
            if (isMouseButtonDragging && isMouseClickReleased)
            {
                isMouseButtonDragging = false;
                mouseDragMapSegment = -1;
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

        private void DrawText()
        {
            DrawLayerButton();
            DrawDrawingModeText();
        }

        private void DrawLayerButton()
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

        private void DrawDrawingModeText()
        {
            Vector2 drawPosition = new Vector2(5, 25);
            string drawingModeText = "";
            switch (drawingMode)
            {
                case DrawingMode.SegmentSelection:
                    drawingModeText = "Draw mode: Select";
                    break;
                case DrawingMode.CollisionMap:
                    drawingModeText = "Draw mode: Column";
                    break;
            }
            text.DrawText(drawPosition, drawingModeText);
            Vector2 drawingModeTextVector = spriteFont.MeasureString(drawingModeText);
            Rectangle textRectangle = new Rectangle(
                (int)drawPosition.X,
                (int)drawPosition.Y,
                (int)drawingModeTextVector.X,
                (int)drawingModeTextVector.Y
            );
            Point mouseCursor = new Point(currentMouseState.X, currentMouseState.Y);
            if (text.IsTextClicked(textRectangle, mouseCursor, isMouseClicked))
            {
                drawingMode = (DrawingMode)((int)(drawingMode + 1) % 2);
            }
        }

        private void DrawMapGrid()
        {
            for (int y = 0; y < map.GridSize; y++)
            {
                for (int x = 0; x < map.GridSize; x++)
                {
                    Rectangle destinationRectangle = new Rectangle(
                        x * map.GridCellSize - (int)(mapScroll.X / 2),
                        y * map.GridCellSize - (int)(mapScroll.Y / 2),
                        map.GridCellSize,
                        map.GridCellSize
                    );
                    if (x < map.GridSize - 1)
                    {
                        _spriteBatch.Draw(
                            nullTexture,
                            new Rectangle(
                                destinationRectangle.X,
                                destinationRectangle.Y,
                                map.GridCellSize,
                                1
                            ),
                            new Color(221, 147, 234, 100)
                        );
                    }
                    if (y < map.GridSize - 1)
                    {
                        _spriteBatch.Draw(
                            nullTexture,
                            new Rectangle(
                                destinationRectangle.X,
                                destinationRectangle.Y,
                                1,
                                map.GridCellSize
                            ),
                            new Color(221, 147, 234, 100)
                        );
                    }
                    if (x < map.GridSize && y < map.GridSize && map.MapGrid[x, y] == 1)
                    {
                        _spriteBatch.Draw(
                            nullTexture,
                            destinationRectangle,
                            new Color(221, 147, 234, 100)
                        );
                    }
                }
            }
        }

        private bool IsAbleToEdit()
        {
            int windowOffSetX = _graphics.PreferredBackBufferWidth - 290;
            // Only allow editing in main map area (avoid UI elements)
            return mouseX < windowOffSetX && mouseY > 50;
        }

        private float GetLayerScaler()
        {
            float layerScaler = .5f;
            if (currentLayer == 0)
            {
                layerScaler = .375f;
            }
            else if (currentLayer == 2)
            {
                layerScaler = .625f;
            }
            return layerScaler;
        }
        #endregion
    }
}
