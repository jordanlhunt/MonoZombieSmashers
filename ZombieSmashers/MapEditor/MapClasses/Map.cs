using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MapEditor.MapClasses
{
    internal class Map
    {
        #region Constants
        const int MAX_MAP_ELEMENTS = 512;
        const int MAP_SEGMENTS_LAYERS = 3;
        const int MAP_SEGMENTS_PER_LAYER = 64;

        const int LAYER_TYPE_BACKGROUND = 0;
        const int LAYER_TYPE_MIDGROUND = 1;
        const int LAYER_TYPE_FOREGROUND = 2;
        #endregion
        #region Member Variables
        MapElement[] mapElements;
        MapSegment[,] mapSegments;
        #endregion
        #region Properties
        public MapElement[] MapElements
        {
            get { return mapElements; }
        }

        public MapSegment[,] MapSegments
        {
            get { return mapSegments; }
        }
        #endregion
        #region Constructor
        public Map()
        {
            mapElements = new MapElement[MAX_MAP_ELEMENTS];
            mapSegments = new MapSegment[MAP_SEGMENTS_LAYERS, MAP_SEGMENTS_PER_LAYER];
            ReadMapElements();
        }
        #endregion


        #region Public Methods
        public int AddMapSegment(int layer, int index)
        {
            for (int i = 0; i < MAP_SEGMENTS_PER_LAYER; i++)
            {
                if (mapSegments[layer, i] == null)
                {
                    mapSegments[layer, i] = new MapSegment();
                    mapSegments[layer, i].SegmentIndex = index;
                    return i;
                }
            }
            return -1;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D[] mapTextures, Vector2 scroll)
        {
            Rectangle sourceRectangle = new Rectangle();
            Rectangle destinationRectangle = new Rectangle();
            for (int i = 0; i < MAP_SEGMENTS_LAYERS; i++)
            {
                float scale = 1.0f;
                Color layerColor = Color.White;
                if (i == LAYER_TYPE_BACKGROUND)
                {
                    layerColor = Color.DimGray;
                    scale = 0.75f;
                }
                else if (i == LAYER_TYPE_FOREGROUND)
                {
                    layerColor = Color.DarkSlateGray;
                    scale = 1.25f;
                }
                scale *= .5f;
                for (int j = 0; j < MAP_SEGMENTS_PER_LAYER; j++)
                {
                    if (mapSegments[i, j] != null)
                    {
                        sourceRectangle = mapElements[
                            mapSegments[i, j].SegmentIndex
                        ].SourceRectangle;
                        destinationRectangle.X = (int)(
                            mapSegments[i, j].Location.X - scroll.X * scale
                        );
                        destinationRectangle.Y = (int)(
                            mapSegments[i, j].Location.Y - scroll.Y * scale
                        );
                        destinationRectangle.Width = (int)(sourceRectangle.Width * scale);
                        destinationRectangle.Height = (int)(sourceRectangle.Height * scale);
                        spriteBatch.Draw(
                            mapTextures[mapElements[mapSegments[i, j].SegmentIndex].ElementIndex],
                            destinationRectangle,
                            sourceRectangle,
                            layerColor
                        );
                    }
                }
            }
        }

        public int GetHoveredSegment(int mouseX, int mouseY, int layerIndex, Vector2 scroll)
        {
            float layerScaleFactor = CalculateLayerScale(layerIndex);
            for (int i = MAP_SEGMENTS_PER_LAYER - 1; i >= 0; i--)
            {
                if (mapSegments[layerIndex, i] != null)
                {
                    Rectangle sourceRectangle = mapElements[
                        mapSegments[layerIndex, i].SegmentIndex
                    ].SourceRectangle;
                    Rectangle destinationRectangle = new Rectangle(
                        (int)(mapSegments[layerIndex, i].Location.X - scroll.X * layerScaleFactor),
                        (int)(mapSegments[layerIndex, i].Location.Y - scroll.Y * layerScaleFactor),
                        (int)(sourceRectangle.Width * layerScaleFactor),
                        (int)(sourceRectangle.Height * layerScaleFactor)
                    );
                    if (destinationRectangle.Contains(mouseX, mouseY))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        #endregion


        #region Private Methods
        private void ReadMapElements()
        {
            StreamReader streamReader = new StreamReader(@"Content/MapEditor/map_data.txt");
            string currentLine;
            int sourceTextureIndex;
            int currentMapElementIndex = -1;
            int currentTextureIndex = 0;
            Rectangle tempRectangle = new Rectangle();
            string[] rectangleParts;
            currentLine = streamReader.ReadLine();
            while (!streamReader.EndOfStream)
            {
                currentLine = streamReader.ReadLine();
                if (currentLine.StartsWith("#"))
                {
                    if (currentLine.StartsWith("#src"))
                    {
                        rectangleParts = currentLine.Split(' ');
                        if (rectangleParts.Length > 1)
                        {
                            sourceTextureIndex = Convert.ToInt32(rectangleParts[1]);
                            currentTextureIndex = sourceTextureIndex - 1;
                        }
                    }
                }
                else
                {
                    currentMapElementIndex++;
                    string mapElementName = currentLine;
                    currentLine = streamReader.ReadLine();
                    rectangleParts = currentLine.Split(' ');
                    if (rectangleParts.Length > 3)
                    {
                        tempRectangle.X = Convert.ToInt32(rectangleParts[0]);
                        tempRectangle.Y = Convert.ToInt32(rectangleParts[1]);
                        tempRectangle.Width = Convert.ToInt32(rectangleParts[2]) - tempRectangle.X;
                        tempRectangle.Height = Convert.ToInt32(rectangleParts[3]) - tempRectangle.Y;
                    }
                    else
                    {
                        Console.WriteLine("[ERROR] - Map.cs - Read Failure: " + mapElementName);
                    }
                    currentLine = streamReader.ReadLine();
                    int flags = Convert.ToInt32(currentLine);
                    mapElements[currentMapElementIndex] = new MapElement(
                        mapElementName,
                        currentTextureIndex,
                        tempRectangle,
                        flags
                    );
                }
            }
        }

        public float CalculateLayerScale(int layerIndex)
        {
            const float baseScale = 0.5f;

            switch (layerIndex)
            {
                case LAYER_TYPE_BACKGROUND:
                {
                    return 0.75f * baseScale;
                }
                case LAYER_TYPE_FOREGROUND:
                {
                    return 1.25f * baseScale;
                }
                default:
                {
                    return baseScale;
                }
            }
        }

        #endregion
    }
}
