using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MapEditor
{
    internal class Text
    {
        #region Member Variables
        private float size = 1f;
        private Color currentColor = Color.White;
        SpriteFont spriteFont;
        SpriteBatch spriteBatch;
        #endregion
        #region Properties
        public Color Color
        {
            get { return currentColor; }
            set { currentColor = value; }
        }
        public float Size
        {
            get { return size; }
            set { size = value; }
        }
        #endregion
        #region Constructor
        public Text(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;
        }
        #endregion
        #region Public Methods
        public void DrawText(Vector2 drawCoordinates, string textToDraw)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(
                spriteFont,
                textToDraw,
                drawCoordinates,
                currentColor,
                0f,
                new Vector2(),
                size,
                SpriteEffects.None,
                1f
            );
            spriteBatch.End();
        }

        public Rectangle GetTextBounds(SpriteFont spriteFont, string text, Vector2 position)
        {
            Vector2 textSize = spriteFont.MeasureString(text) * size;
            return new Rectangle(
                (int)position.X,
                (int)position.Y,
                (int)textSize.X,
                (int)textSize.Y
            );
        }

        public bool IsMouseOverText(Rectangle textRectangle, Point mousePosition)
        {
            return textRectangle.Contains(mousePosition);
        }

        public bool IsTextClicked(Rectangle textRectangle, Point mousePosition, bool isMouseClicked)
        {
            return textRectangle.Contains(mousePosition) && isMouseClicked;
        }

        public void DrawHoverableText(Vector2 drawPosition, string textString, Point mousePosition)
        {
            Rectangle textBounds = GetTextBounds(spriteFont, textString, drawPosition);
            Color drawColor = Color.White;
            if (IsMouseOverText(textBounds, mousePosition))
            {
                drawColor = Color.LightYellow;
            }
        }
        #endregion
    }
}
