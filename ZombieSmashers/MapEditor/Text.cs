using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        #region Constructor
        public Text(SpriteBatch spriteBatch, SpriteFont spriteFont)
        {
            this.spriteBatch = spriteBatch;
            this.spriteFont = spriteFont;
        }
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

        #region Public Methods
        public void DrawText(Vector2 drawCoordinates, string textToDraw)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(
                textToDraw,
                drawCoordinates,
                currentColor,
                0f,
                new Vector2(),
                size,
                SpriteEffects.None,
                1f
            );
        }
        #endregion
    }
}
