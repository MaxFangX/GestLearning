using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KinectLibrary.Contour
{
    public sealed class ContourTracking : IContourTracking
    {
        private HashSet<Vector> duplicateCheck; // Used for fast duplicate checking. This is an unordered collection.
        private List<Vector> contourPoints;

        public ContourTracking()
        {
            EnableScanFromLeft = true;
            EnableScanFromRight = false;

            ScanHeightOffset = 0.2;
            MaxPixelsToBacktrack = 25;
            NumberOfRowsToSkip = 5; // Skip rows when scanning for the initial pixel.
            MaxEdgePixelCount = 700; // If we are about 800 mm - 950 mm from the Kinect sensor we need about minimum 450 pixels to find all the fingers.
        }
        
        /// <summary>
        /// Begin tracking the contour. The event 'ContourDataReady' will fire when the tracking is done.
        /// </summary>
        public IEnumerable<Vector> StartTracking(Pixel[] pixelsInRange, int width, int height)
        {
            ImageWidth = width;
            ImageHeight = height;
            UpperArrayBound = width*height;

            IEnumerable<Vector> contour = FindContour(pixelsInRange);

            ContourDataReady(contour, pixelsInRange);

            return contour;
        }

        /// <summary>
        /// The main tracking loop.
        /// </summary>
        /// <returns>Returns the positions of the pixels that make up the contour.</returns>
        private IEnumerable<Vector> FindContour(Pixel[] rangeData)
        {
            InitializeContourPointList();
            Vector pixelPosition;

            // Initial search direction.
            SearchDirection searchDirection = SearchDirection.UpLeft;
            
            const int gridRadius = 2; // Seeking range in the x direction.
            int heightOffset = (int)(ImageHeight * ScanHeightOffset); // We assume that the wrist is at least this far up on the image.


            if (EnableScanFromLeft)
            {
                // Scan for the initial pixel to begin contour tracking.
                bool foundInitialPixel = ScanFromLeft(heightOffset, rangeData, out pixelPosition);
                if (foundInitialPixel)
                    TrackContour(pixelPosition, rangeData, MaxEdgePixelCount, gridRadius, searchDirection);
            }

            if(EnableScanFromLeft && EnableScanFromRight)
                ResetMaxEdgePixelCount();

            if (EnableScanFromRight)
            {
                if (contourPoints.Count == 0)
                    return contourPoints;

                bool foundInitialPixel = ScanFromRight(heightOffset, rangeData, out pixelPosition);
                if (foundInitialPixel)
                    TrackContour(pixelPosition, rangeData, MaxEdgePixelCount, gridRadius, searchDirection);
            }

            return contourPoints;
        }
        
        private void InitializeContourPointList()
        {
            duplicateCheck = new HashSet<Vector>();
            contourPoints = new List<Vector>(2000);
        }

        private void ResetMaxEdgePixelCount()
        {
            MaxEdgePixelCount += contourPoints.Count;
        }

        /// <summary>
        /// Scan from the bottom left and upwards for a pixel in range.
        /// Returns true if it finds a pixel in range.
        /// </summary>
        private bool ScanFromLeft(int heightOffset, Pixel[] rangeData, out Vector pixelPosition)
        {
            pixelPosition = null;

            for (int y = ImageHeight - 1 - heightOffset; y >= 0; y -= NumberOfRowsToSkip)
            {
                for (int x = 0; x < ImageWidth; x++)
                {
                    if (HasFoundInitialPixel(x, y, rangeData))
                    {
                        pixelPosition = new Vector(x, y, 0d);
                        return true;
                    }
                }
            }

            return false; // Did not find any pixels in range.
        }

        /// <summary>
        /// Scan from the bottom right and upwards for a pixel in range.
        /// When a pixel is found it will try to find the opposite edge and return its position.
        /// Returns true if it finds the pixel.
        /// </summary>
        private bool ScanFromRight(int heightOffset, Pixel[] rangeData, out Vector pixelPosition)
        {
            pixelPosition = null;
            for (int y = ImageHeight - 1 - heightOffset; y >= 0; y -= NumberOfRowsToSkip)
            {
                for (int x = ImageWidth - 1; x >= 0; x--)
                {
                    if (HasFoundInitialPixel(x, y, rangeData))
                    {
                        Vector pixelCoordinate = new Vector(x, y, 0d);

                        if (HasPreviouslyBeenDiscovered(pixelCoordinate))
                        {
                            // The pixel is part of an already discovered contour.
                            // We will try to find a new pixel higher up on the image.
                            y -= 20;
                            break;
                        }

                        // Traverse to the other side (horizontally) so the tracking begins on the left side of the object.
                        // Must do this since the contour tracking algorithm is optimized to begin on the left side of
                        // an object and go upwards.
                        Vector leftSideOfObject = TraverseHorizontally(x, y, rangeData);
                        if (leftSideOfObject != null)
                        {
                            pixelPosition = new Vector(x, y, 0d);
                            return true;
                        }
                    }
                }
            }
            return false; // Did not find any pixels in range.
        }

        /// <summary>
        /// Finds the left end of a horizontal pixel line of in range pixels.
        /// </summary>
        private Vector TraverseHorizontally(int x, int y, Pixel[] pixels)
        {
            for (int _x = x - 1; _x >= 0 ; _x--)
            {
                Pixel pixel = FindPixel(_x, y, pixels);
                if(pixel == Pixel.OutOfRange)
                    return new Vector(_x - 1, y, 0d);
            }

            return null;
        }

        private bool HasFoundInitialPixel(int x, int y, Pixel[] rangeData)
        {
            Pixel currentPixel = FindPixel(x, y, rangeData);
            return currentPixel == Pixel.InRange;
        }

        private void TrackContour(Vector startPosition, Pixel[] rangeData, int maxEdgePixelCount, int gridRadius, SearchDirection searchDirection)
        {
            Vector pixelFoundCoord = startPosition;
            
            while (pixelFoundCoord != null)
            {
                bool newEdgePixel = !HasPreviouslyBeenDiscovered(pixelFoundCoord);

                AddPixelToContourPointList(pixelFoundCoord);

                if (!newEdgePixel)
                    break; // Found duplicate point; abort tracking.
                            
                if (contourPoints.Count > maxEdgePixelCount) 
                    break; // We have reached the edge pixel count limit. We assume that the whole hand is within this range.

                pixelFoundCoord = CheckNearbyForPixels(rangeData, pixelFoundCoord, gridRadius,
                                                       searchDirection, out searchDirection);
                
                UpdateEdgePositionEvent();
                PrintDebugSearchInfo(searchDirection, pixelFoundCoord);
            }
        }

        private void AddPixelToContourPointList(Vector pixelFoundCoord)
        {
            contourPoints.Add(pixelFoundCoord);
            duplicateCheck.Add(pixelFoundCoord);
        }

        [Conditional("DEBUG")]
        private void UpdateEdgePositionEvent()
        {
            EdgePosition = contourPoints;
        }

        [Conditional("DEBUG")]
        private void PrintDebugSearchInfo(SearchDirection searchDirection, Vector pixelCoord)
        {
            if (pixelCoord != null)
            {
                System.Diagnostics.Debug.WriteLine(searchDirection);
                System.Diagnostics.Debug.WriteLine(pixelCoord.X + " : " + pixelCoord.Y);
            }
        }

        
        private Vector CheckNearbyForPixels(Pixel[] pixels, Vector origin, int gridRadius,
                                            SearchDirection searchDirection, out SearchDirection foundDirection)
        {
            Vector foundPixelCoord;

            // Search in the specified quadrant.
            bool pixelFound = SearchInDirection(searchDirection, pixels, origin, gridRadius, out foundPixelCoord,
                                                out foundDirection);
            
            if(HasPreviouslyBeenDiscovered(foundPixelCoord))
                pixelFound = false;

            if (pixelFound)
                return foundPixelCoord;

            // No pixel was found.
            // Search in the next most probable quandrant:
            // This is optimized for finger contours.

            if (searchDirection == SearchDirection.UpLeft)
                pixelFound = SearchInDirection(SearchDirection.UpRight, pixels, origin, gridRadius, out foundPixelCoord,
                                               out foundDirection);

            if (searchDirection == SearchDirection.UpRight && !pixelFound)
                pixelFound = SearchInDirection(SearchDirection.DownRight, pixels, origin, gridRadius, out foundPixelCoord,
                                               out foundDirection);

            if (searchDirection == SearchDirection.DownRight && !pixelFound)
                pixelFound = SearchInDirection(SearchDirection.UpRight, pixels, origin, gridRadius, out foundPixelCoord,
                                               out foundDirection);

            if (searchDirection == SearchDirection.DownLeft && !pixelFound)
                pixelFound = SearchInDirection(SearchDirection.DownRight, pixels, origin, gridRadius, out foundPixelCoord,
                                               out foundDirection);
            
            // If still no pixel was found, search clockwise in all the quadrants:)
            if (!pixelFound) 
                pixelFound = SearchClockwise(searchDirection, pixels, origin, gridRadius, out foundPixelCoord, out foundDirection);

            // If we found a previously discovered edge pixel it might be we are starting the search in the wrong direction.
            // Search counterclockwise in all the quadrants:
            if (HasPreviouslyBeenDiscovered(foundPixelCoord))
                pixelFound = SearchCounterclockwise(searchDirection, pixels, origin, gridRadius, out foundPixelCoord, out foundDirection);

            
            // If we still cant find any new edge pixels we check if we are stuck on a horizontal or vertical single-pixel line:
            if (HasPreviouslyBeenDiscovered(foundPixelCoord))
                foundPixelCoord = SearchForSingleLineEnd(searchDirection, pixels, origin, out foundDirection);

            // If there are absolutley cannot find any new pixel we backtrack and search in a clockwise motion for 
            // a pixel that have not yet been discovered. If we find a new pixel we continue contour tracking from that pixel.
            bool hasBeenDiscovered = HasPreviouslyBeenDiscovered(foundPixelCoord);
            if(hasBeenDiscovered || !pixelFound)
                Backtrack(pixels, origin, MaxPixelsToBacktrack, gridRadius, out foundPixelCoord, out foundDirection);
            
            return foundPixelCoord;
        }

        /// <summary>
        /// Checks if the position have already been discovered.
        /// </summary>
        /// <param name="pixelCoordinate">The pixel position.</param>
        /// <returns>Returns true if it has already been discovered.</returns>
        private bool HasPreviouslyBeenDiscovered(Vector pixelCoordinate)
        {
            return duplicateCheck.Contains(pixelCoordinate) || pixelCoordinate == null;
        }

        /// <summary>
        /// Search for the end of a signle pixel line. This will find the next pixel to continue tracking.
        /// </summary>
        /// <param name="searchDirection">Current search direction.</param>
        /// <param name="pixels">All pixels.</param>
        /// <param name="origin">The position where we will start the search.</param>
        /// <param name="foundDirection">The direction to continue the search in.</param>
        /// <returns>Returns the position to the found pixel.</returns>
        private Vector SearchForSingleLineEnd(SearchDirection searchDirection, Pixel[] pixels, Vector origin, out SearchDirection foundDirection)
        {
            foundDirection = SearchDirection.Undefined;
            Vector foundPixelCoord = null;

            Vector currentPoint = new Vector(origin.X, origin.Y, origin.Z);
            var generalDirection = FindGeneralDirection(searchDirection);

            bool directionIsUp = generalDirection == GeneralDirection.Up;
            bool directionIsDown = generalDirection == GeneralDirection.Down;

            // Search for a vertical line:

            while (directionIsDown && 
                   FoundVerticalLine(currentPoint, pixels, GeneralDirection.Down, searchDirection, out foundPixelCoord, out foundDirection))
            {
                currentPoint.Y--; // Traverse down the line.
            }

            while (directionIsUp &&
                   FoundVerticalLine(currentPoint, pixels, GeneralDirection.Up, searchDirection, out foundPixelCoord, out foundDirection))
            {
                currentPoint.Y++;
            }
                
            if (foundDirection != SearchDirection.Undefined)
                return foundPixelCoord; // We found a vertical line and successfully got to the end.

            // Did not find a vertical line.
            // Search for a horizontal line:

            while (directionIsDown &&
                   FoundHorizontalLine(currentPoint, pixels, GeneralDirection.Down, out foundPixelCoord, out foundDirection))
            {
                currentPoint.X--;
            }

            while (directionIsUp &&
                   FoundHorizontalLine(currentPoint, pixels, GeneralDirection.Up, out foundPixelCoord, out foundDirection))
            {
                currentPoint.X++;
            }

            return foundPixelCoord;
        }

        /// <summary>
        /// Search in a specific quandrant for a pixel in range.
        /// </summary>
        /// <param name="searchDirection">The quadrant to search in.</param>
        /// <param name="pixels">Range data.</param>
        /// <param name="origin">X and Y coordinates to the search start location.</param>
        /// <param name="gridRadius">Search radius in pixels.</param>
        /// <param name="foundPixelCoord">The coordinates to the pixel found.</param>
        /// <param name="foundDirection">The quadrant where the pixel was found.</param>
        /// <returns>True if a pixel was found, otherwise false.</returns>
        private bool SearchInDirection(SearchDirection searchDirection, Pixel[] pixels, Vector origin,
                                         int gridRadius, out Vector foundPixelCoord, out SearchDirection foundDirection)
        {
            foundDirection = SearchDirection.Undefined;
            foundPixelCoord = null;
            bool pixelFound = false;

            // Search in one quadrant.
            switch (searchDirection)
            {
                case SearchDirection.UpLeft:
                    pixelFound = SearchUpLeft(pixels, origin, gridRadius, out foundPixelCoord);
                    foundDirection = SearchDirection.UpLeft;
                    break;
                case SearchDirection.UpRight:
                    pixelFound = SearchUpRight(pixels, origin, gridRadius, out foundPixelCoord);
                    foundDirection = SearchDirection.UpRight;
                    break;
                case SearchDirection.DownRight:
                    pixelFound = SearchDownRight(pixels, origin, gridRadius, out foundPixelCoord);
                    foundDirection = SearchDirection.DownRight;
                    break;
                case SearchDirection.DownLeft:
                    pixelFound = SearchDownLeft(pixels, origin, gridRadius, out foundPixelCoord);
                    foundDirection = SearchDirection.DownLeft;
                    break;

                case SearchDirection.Undefined:
                    break;
                default:
                    break;
            }

            return pixelFound;
        }

        /// <summary>
        /// Search clockwise after a pixel that is in range.
        /// </summary>
        /// <param name="startDirection">The quadrant to begin the search in.</param>
        /// <param name="pixels">Range data.</param>
        /// <param name="origin">X and Y coordinates to the search start location.</param>
        /// <param name="gridRadius">Search radius in pixels.</param>
        /// <param name="foundPixelCoord">The coordinates to the pixel found.</param>
        /// <param name="foundDirection">The quadrant where the pixel was found.</param>
        /// <returns>True if a pixel was found, otherwise false.</returns>
        private bool SearchClockwise(SearchDirection startDirection, Pixel[] pixels, Vector origin,
                                         int gridRadius, out Vector foundPixelCoord, out SearchDirection foundDirection)
        {
            foundPixelCoord = null;
            foundDirection = SearchDirection.Undefined;

            if (startDirection == SearchDirection.Undefined)
                startDirection = SearchDirection.UpLeft;

            // Loop through all SearchDirection enums.
            for (int i = 0, d = (int)startDirection; i < 4; i++, d++)
            {
                if (d == 5)
                    d = 1;

                SearchDirection direction;
                Enum.TryParse(d.ToString(), out direction);

                bool pixelFound = SearchInDirection(direction, pixels, origin, gridRadius, out foundPixelCoord,
                                                    out foundDirection);
                if (pixelFound)
                    return true;
            }

            return false; // No pixel was found.
        }

        /// <summary>
        /// Search counterclockwise after a pixel that is in range.
        /// </summary>
        /// <param name="startDirection">The quadrant to begin the search in.</param>
        /// <param name="pixels">Range data.</param>
        /// <param name="origin">X and Y coordinates to the search start location.</param>
        /// <param name="gridRadius">Search radius in pixels.</param>
        /// <param name="foundPixelCoord">The coordinates to the pixel found.</param>
        /// <param name="foundDirection">The quadrant where the pixel was found.</param>
        /// <returns>True if a pixel was found, otherwise false.</returns>
        private bool SearchCounterclockwise(SearchDirection startDirection, Pixel[] pixels, Vector origin,
                                         int gridRadius, out Vector foundPixelCoord, out SearchDirection foundDirection)
        {
            foundPixelCoord = null;
            foundDirection = SearchDirection.Undefined;

            if (startDirection == SearchDirection.Undefined)
                startDirection = SearchDirection.UpLeft;

            // Loop through all SearchDirection enums.
            for (int i = 4, d = (int)startDirection; i > 0; i--, d--)
            {
                if (d == 0)
                    d = 4;

                SearchDirection direction;
                Enum.TryParse(d.ToString(), out direction);

                bool pixelFound = SearchInDirection(direction, pixels, origin, gridRadius, out foundPixelCoord,
                                                    out foundDirection);
                if (pixelFound)
                    return true;
            }

            return false; // No pixel was found.
        }

        /// <summary>
        /// Checks if the value is not greater than the image width or less than zero.
        /// </summary>
        /// <param name="xValue">The x value to check.</param>
        /// <returns></returns>
        private int ValidateXValue(int xValue)
        {
            if (xValue < 0)
                return 0;
            if (xValue > ImageWidth)
                return ImageWidth;

            return xValue;
        }

        /// <summary>
        /// Checks if the value is not greater than the image height or less than zero.
        /// </summary>
        /// <param name="yValue">The y value to check.</param>
        /// <returns></returns>
        private int ValidateYValue(int yValue)
        {
            if (yValue < 0)
                return 0;
            if (yValue > ImageHeight)
                return ImageHeight;

            return yValue;
        }
        
        private bool SearchDownLeft(Pixel[] pixels, Vector origin, int gridRadius, out Vector foundPixel)
        {
            foundPixel = null;

            const int yOffset = 1;
            int xStart = (int) origin.X;
            int yStart = ValidateYValue((int) origin.Y + yOffset);
            int xMin = ValidateXValue(xStart - gridRadius);
            int yMax = ValidateYValue(yStart + 1);

            for (int y = yStart; y < yMax; y++)
            {
                for (int x = xStart; x > xMin; x--)
                {
                    var currentPixel = FindPixel(x, y, pixels);

                    if (currentPixel == Pixel.InRange)
                    {
                        var prevPixel = FindPixel(x + 1, y, pixels);

                        if (prevPixel == Pixel.OutOfRange)
                        {
                            foundPixel = new Vector(x, y, 0d);
                            return true;
                        }
                    }
                    else
                    {
                        if (x == xStart && y == yStart)
                            continue;

                        var pixelAbove = FindPixel(x, y - 1, pixels);

                        if (pixelAbove == Pixel.InRange)
                        {
                            foundPixel = new Vector(x, y - 1, 0d);
                            return true;
                        }

                        return false; // This direction is no longer valid. Must change direction.
                    }
                }
            }

            return false;
        }
        
        private bool SearchDownRight(Pixel[] pixels, Vector origin, int gridRadius, out Vector foundPixel)
        {
            foundPixel = null;
            
            int xStart = (int)origin.X;
            int yStart = (int)origin.Y;
            int xMax = ValidateXValue(xStart + gridRadius);
            int yMax = ValidateYValue(yStart + 2);

            for (int y = yStart; y < yMax; y++)
            {
                for (int x = xStart; x < xMax; x++)
                {
                    if(x == xStart && y == yStart)
                        continue; // This the previously found pixel. Dont detect it twice.

                    var currentPixel = FindPixel(x, y, pixels);
                    var rightPixel = FindPixel(x + 1, y, pixels);
                    var pixelAbove = FindPixel(x, y - 1, pixels);

                    if (y == yStart && pixelAbove == Pixel.InRange && currentPixel != Pixel.OutOfRange)
                        return false; // The direction of the pixels have changed.

                    if (currentPixel == Pixel.OutOfRange)
                        break; // No valid pixel to detect in the x-direction.

                    if (rightPixel == Pixel.OutOfRange || pixelAbove == Pixel.OutOfRange)
                    {
                        // Edge found.
                        foundPixel = new Vector(x,y,0d);
                        return true;
                    }
                }
            }

            return false;
        }
        
        private bool SearchUpRight(Pixel[] pixels, Vector origin, int gridRadius, out Vector foundPixel)
        {
            foundPixel = null;

            const int yOffset = 1;
            int xStart = (int)origin.X;
            int yStart = ValidateYValue((int)origin.Y - yOffset);
            int xMax = ValidateXValue(xStart + gridRadius);
            int yMin = ValidateYValue(yStart - 1);

            for (int y = yStart; y > yMin; y--)
            {
                for (int x = xStart; x < xMax; x++)
                {
                    var pixel = FindPixel(x, y, pixels);

                    if (pixel == Pixel.InRange)
                    {
                        var leftPixel = FindPixel(x - 1, y, pixels);

                        if (leftPixel == Pixel.OutOfRange)
                        {
                            // Edge detected.
                            foundPixel = new Vector(x, y, 0f);
                            return true;
                        }
                    }
                    else if (x != xStart) // Do not begin right above the pixel we found previously, else we will get a false positive.
                    {
                        var pixelUnder = FindPixel(x, y + 1, pixels); // The pixel under the pixel we are on in the loop.

                        if (pixelUnder == Pixel.InRange)
                        {
                            // Edge detected.
                            foundPixel = new Vector(x, y + 1, 0f);
                            return true;
                        }

                        break; // There is no more valid pixels in the x-direction.
                    }
                }
            }

            return false;
        }
        
        private bool SearchUpLeft(Pixel[] pixels, Vector origin, int gridRadius, out Vector foundPixel)
        {
            foundPixel = null;

            int xStart = (int)origin.X;
            int yStart = (int)origin.Y;
            int xMin = ValidateXValue(xStart - gridRadius);
            int yMin = ValidateYValue(yStart - 2);

            for (int y = yStart; y > yMin; y--)
            {
                for (int x = xStart; x > xMin; x--)
                {
                    if(x == xStart && y == yStart)
                        continue; // This the previously found pixel. Dont detect it twice.

                    var currentPixel = FindPixel(x, y, pixels);

                    if (currentPixel == Pixel.OutOfRange)
                        break; // No valid pixel to detect in the x-direction.

                    var leftPixel = FindPixel(x - 1, y, pixels);
                    var pixelUnder = FindPixel(x, y + 1, pixels); // The pixel under the pixel we are on.

                    bool notThePixelAboveOrigin = y != yStart - 1;
                    if (leftPixel == Pixel.InRange && pixelUnder == Pixel.InRange && notThePixelAboveOrigin)
                        return false; // Pixel direction has changed.

                    if (leftPixel == Pixel.OutOfRange || pixelUnder == Pixel.OutOfRange)
                    {
                        // Edge detected.
                        foundPixel = new Vector(x, y, 0f);
                        return true;
                    }
                }
            }

            return false;
        }
        
        /// <summary>
        /// Finds the general searching direction.
        /// </summary>
        /// <param name="searchDirection">The current searching direction.</param>
        /// <returns>Returns the general direction; either up or down.</returns>
        private GeneralDirection FindGeneralDirection(SearchDirection searchDirection)
        {
            if (searchDirection == SearchDirection.Undefined)
                return GeneralDirection.Undefined;

            if (searchDirection == SearchDirection.UpLeft || searchDirection == SearchDirection.UpRight)
                return GeneralDirection.Up;

            return GeneralDirection.Down;
        }

        /// <summary>
        /// Finds the next pixel to track at the base of a vertical single pixel line.
        /// </summary>
        /// <param name="origin">The position to start tracking from.</param>
        /// <param name="pixels">All pixels.</param>
        /// <param name="generalDirection">The general search direction. Either up or down.</param>
        /// <param name="searchDirection">Current search direction.</param>
        /// <param name="foundPixel">The position of the found pixel.</param>
        /// <param name="foundDirection">The direction to continue tracking.</param>
        /// <returns>Return true if we are on a single pixel line. Otherwise false.</returns>
        private bool FoundVerticalLine(Vector origin, Pixel[] pixels, GeneralDirection generalDirection,
                                       SearchDirection searchDirection, out Vector foundPixel,
                                       out SearchDirection foundDirection)
        {
            foundPixel = null;
            foundDirection = SearchDirection.Undefined;

            Pixel left = FindPixel((int)origin.X - 1, (int)origin.Y, pixels);
            Pixel middle = FindPixel((int) origin.X, (int) origin.Y, pixels);
            Pixel right = FindPixel((int)origin.X + 1, (int)origin.Y, pixels);
            Pixel under = FindPixel((int)origin.X, (int)origin.Y + 1, pixels);
            Pixel above = FindPixel((int) origin.X, (int) origin.Y - 1, pixels);

            if (left == Pixel.OutOfRange && right == Pixel.OutOfRange && middle == Pixel.InRange)
            {
                foundPixel = origin;
                return true;
            }

            bool directionIsLeft = (searchDirection == SearchDirection.DownLeft ||
                                    searchDirection == SearchDirection.UpLeft);

            // If we are searching for a pixel on the left side; continue until we find one.
            if (directionIsLeft && left == Pixel.OutOfRange && above == Pixel.InRange)
            {
                foundPixel = origin;
                return true;
            }

            bool validPixelToLeft = (left == Pixel.InRange && under == Pixel.InRange);

            if (validPixelToLeft && generalDirection == GeneralDirection.Down)
            {
                foundPixel = new Vector(origin.X - 1, origin.Y, 0d);
                foundDirection = SearchDirection.DownLeft;
            }

            bool validPixelToRight = (right == Pixel.InRange && above == Pixel.InRange);

            if (validPixelToRight && generalDirection == GeneralDirection.Up)
            {
                foundPixel = new Vector(origin.X + 1, origin.Y, 0d);
                foundDirection = SearchDirection.UpRight;
            }

            Pixel aboveLeft = FindPixel((int)origin.X - 1, (int)origin.Y - 1, pixels);
            if (left == Pixel.InRange && under == Pixel.InRange && middle == Pixel.InRange && above == Pixel.OutOfRange && aboveLeft ==Pixel.InRange)
            {
                foundPixel = new Vector(origin.X - 1, origin.Y - 1, 0d);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Finds the next pixel to track at the base of a horizontal single pixel line.
        /// </summary>
        /// <param name="origin">The position to start tracking from.</param>
        /// <param name="pixels">All pixels.</param>
        /// <param name="generalDirection">The general search direction. Either up or down.</param>
        /// <param name="foundPixel">The position of the found pixel.</param>
        /// <param name="foundDirection">The direction to continue tracking.</param>
        /// <returns>Return true if we are on a single pixel line. Otherwise false.</returns>
        private bool FoundHorizontalLine(Vector origin, Pixel[] pixels, GeneralDirection generalDirection,
                                            out Vector foundPixel, out SearchDirection foundDirection)
        {
            foundPixel = null;
            foundDirection = SearchDirection.Undefined;

            Pixel above = FindPixel((int)origin.X, (int)origin.Y - 1, pixels);
            Pixel middle = FindPixel((int)origin.X, (int)origin.Y, pixels);
            Pixel under = FindPixel((int)origin.X, (int)origin.Y + 1, pixels);

            if (middle == Pixel.InRange && (under == Pixel.OutOfRange || above == Pixel.OutOfRange))
            {
                foundPixel = origin;
                return true;
            }

            if (above == Pixel.InRange && generalDirection == GeneralDirection.Up)
            {
                foundPixel = new Vector(origin.X, origin.Y - 1, 0d);
                foundDirection = SearchDirection.DownLeft;
            }

            if (under == Pixel.InRange && generalDirection == GeneralDirection.Down)
            {
                foundPixel = new Vector(origin.X, origin.Y + 1, 0d);
                foundDirection = SearchDirection.DownRight;
            }

            return false;
        }
        
        /// <summary>
        /// Backtracks a specified number of pixels to search for a new contour pixel.
        /// It will search in a clockwise motion.
        /// </summary>
        /// <param name="pixels">All pixels.</param>
        /// <param name="origin">The position to start tracking from.</param>
        /// <param name="maxPixelsToBacktrack">Max pixels to backtrack.</param>
        /// <param name="gridRadius">Search radius in pixels.</param>
        /// <param name="foundPixel">The coordinates to the pixel found.</param>
        /// <param name="foundDirection">The quadrant where the pixel was found.</param>
        /// <returns>Returns true if it found a new contour pixel.</returns>
        private bool Backtrack(Pixel[] pixels, Vector origin, int maxPixelsToBacktrack, int gridRadius, out Vector foundPixel, out SearchDirection foundDirection)
        {
            int contourPointIndex = contourPoints.Count - 1;
            int iterationCounter = 0;

            while (iterationCounter < maxPixelsToBacktrack && contourPointIndex > 0)
            {
                bool newPixel = ClockwiseSearchForNewContourPixel(SearchDirection.Undefined, pixels, origin, gridRadius, out foundPixel, out foundDirection);

                if (newPixel)
                    return true; 

                origin = contourPoints[contourPointIndex--];

                iterationCounter++;
            }

            foundDirection = SearchDirection.Undefined;
            foundPixel = null;
            return false;
        }

        /// <summary>
        /// Search clockwise in all search directions for a new contour pixel.
        /// </summary>
        /// <returns>Returns true if a new contour pixel was found; Otherwise false.</returns>
        private bool ClockwiseSearchForNewContourPixel(SearchDirection startDirection, Pixel[] pixels, Vector origin,
                                         int gridRadius, out Vector foundPixelCoord, out SearchDirection foundDirection)
        {
            if (startDirection == SearchDirection.Undefined)
                startDirection = SearchDirection.UpLeft;

            // Loop through all SearchDirection enums.
            for (int i = 0, d = (int)startDirection; i < 4; i++, d++)
            {
                if (d == 5)
                    d = 1;

                SearchDirection direction;
                Enum.TryParse(d.ToString(), out direction);

                bool pixelFound = SearchInDirection(direction, pixels, origin, gridRadius, out foundPixelCoord,
                                                    out foundDirection);
                if (pixelFound)
                {
                    bool newPixel = HasPreviouslyBeenDiscovered(foundPixelCoord);

                    if (!newPixel)
                        return true;
                    
                    continue;
                }
            }

            foundPixelCoord = null;
            foundDirection = SearchDirection.Undefined;
            return false; // No pixel was found.
        }

        /// <summary>
        /// Convert screen coordinates to array index.
        /// </summary>
        /// <param name="x">The x screen coordinate.</param>
        /// <param name="y">The y screen coordinate.</param>
        /// <returns>Array index.</returns>
        private int CoordinateToArrayIndex(int x, int y)
        {
            return (y*(ImageWidth)) + x;
        }

        /// <summary>
        /// Get a pixel from the specified pixel array.
        /// </summary>
        /// <param name="x">The x position. This is zero-based.</param>
        /// <param name="y">The y position. This is zero-based.</param>
        /// <param name="pixels">The pixel array to get the pixel from.</param>
        /// <returns></returns>
        private Pixel FindPixel(int x, int y, Pixel[] pixels)
        {
            int arrayIndex = CoordinateToArrayIndex(x, y);
            if (arrayIndex >= UpperArrayBound || arrayIndex < 0)
                return Pixel.OutOfRange;
            
            return pixels[arrayIndex];
        }
        
        /// <summary>
        /// Search direction in a grid where origin is in the center of the grid.
        /// </summary>
        private enum SearchDirection
        {
            /// <summary>
            /// Right to left and upwards.
            /// </summary>
            UpLeft = 1,
            /// <summary>
            /// Left to right and upwards.
            /// </summary>
            UpRight,
            /// <summary>
            /// Left to right and downwards.
            /// </summary>
            DownRight,
            /// <summary>
            /// Right to left and downwards.
            /// </summary>
            DownLeft,
            /// <summary>
            /// Direction is not defined.
            /// </summary>
            Undefined = 0,
        }

        private enum GeneralDirection
        {
            Undefined = 0,
            Up = 1,
            Down,
        }

        private int ImageWidth { get; set; }
        private int ImageHeight { get; set; }
        private int UpperArrayBound { get; set; }

        public int MaxEdgePixelCount { get; set; }

        /// <summary>
        /// Number of rows to skip when scanning for the inital contour pixel.
        /// </summary>
        public int NumberOfRowsToSkip { get; set; }
        public int MaxPixelsToBacktrack { get; set; }
        public bool EnableScanFromRight { get; set; }
        public bool EnableScanFromLeft { get; set; }
        /// <summary>
        /// Height offset in percentage of image height.
        /// This offsets the height where the scan will start.
        /// </summary>
        public double ScanHeightOffset { get; set; }

        public IEnumerable<Vector> EdgePosition
        {
            set { EdgePointsUpdated(value); }
        }

        public event ContourDataUpdated EdgePointsUpdated = delegate { };
        public event ContourReady ContourDataReady = delegate { };
    }
}