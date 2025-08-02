using Microsoft.Xna.Framework;

namespace MapEditor.MapClasses
{
    internal class MapElement
    {
        #region Member Variables
        private string name;
        private int sourceIndex;
        private Rectangle sourceRectangle;
        private int flags;
        #endregion
        #region Properties
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public int SourceIndex
        {
            get { return sourceIndex; }
            set { sourceIndex = value; }
        }
        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
            set { sourceRectangle = value; }
        }
        public int Flags
        {
            get { return flags; }
            set { flags = value; }
        }
        #endregion
        #region Constructor
        public MapElement(string name, int sourceIndex, Rectangle sourceRectangle, int flags)
        {
            this.name = name;
            this.sourceIndex = sourceIndex;
            this.sourceRectangle = sourceRectangle;
            this.flags = flags;
        }
        #endregion
    }
}
