using MapEditor.MapClasses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MapEditor
{
    class MapSegment
    {
        #region Member Variables
        public Vector2 Location;
        int segmentIndex;
        #endregion

        #region Properties
        public int SegmentIndex
        {
            get { return segmentIndex; }
            set { segmentIndex = value; }
        }
        #endregion
    }
}
