using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace MapEditor.MapClasses
{
    internal class Map
    {
        #region Member Varaibles
        const int MAX_MAP_ELEMENTS = 512;
        MapElement[] mapElements;
        #endregion
        #region Properties
        public MapElement[] MapElements
        {
            get { return mapElements; }
        }
        #endregion
        #region Constructor
        public Map()
        {
            mapElements = new MapElement[MAX_MAP_ELEMENTS];
            ReadMapElements();
        }
        #endregion
        #region Private Methods
        private void ReadMapElements()
        {
            StreamReader streamReader = new StreamReader(@"Content/map_data.txt");
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
        #endregion
    }
}
