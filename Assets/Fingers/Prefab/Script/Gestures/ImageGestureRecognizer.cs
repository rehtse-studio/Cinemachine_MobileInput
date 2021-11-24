//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System;
using System.Collections.Generic;

namespace DigitalRubyShared
{
    /// <summary>
    /// Represents an image that can be drawn with a gesture - when you get a valid image you care about, it is good practice to call Reset on this gesture.
    /// </summary>
    public class ImageGestureImage
    {
        #region Constants

        private const ulong m1 = 0x5555555555555555; //binary: 0101...
        private const ulong m2 = 0x3333333333333333; //binary: 00110011..
        private const ulong m4 = 0x0f0f0f0f0f0f0f0f; //binary:  4 zeros,  4 ones ...
        private const ulong m8 = 0x00ff00ff00ff00ff; //binary:  8 zeros,  8 ones ...
        private const ulong m16 = 0x0000ffff0000ffff; //binary: 16 zeros, 16 ones ...
        private const ulong m32 = 0x00000000ffffffff; //binary: 32 zeros, 32 ones
        private const ulong hff = 0xffffffffffffffff; //binary: all ones
        private const ulong h01 = 0x0101010101010101; //the sum of 256 to the power of 0,1,2,3...

        #endregion Constants

        private void ComputeRow(byte[] pixels, int row)
        {
            ulong rowValue = 0;
            int index = row * Width;
            int endIndex = index + Width;
            int shift = 0;

            while (index != endIndex)
            {
                rowValue |= ((ulong)pixels[index++] & 1) << shift++;
            }

            Rows[row] = rowValue;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageGestureImage()
        {
        }

        /// <summary>
        /// Constructor with just the rows for this image, leaving pixels null
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="width">Width</param>
        public ImageGestureImage(ulong[] rows, int width) : this(rows, width, 0.0f)
        {
        }

        /// <summary>
        /// Constructor with just the rows for this image, leaving pixels to null
        /// </summary>
        /// <param name="rows">Rows</param>
        /// <param name="width">Width</param>
        /// <param name="scorePadding">Add this amount to the score to allow fuzzier matches (negative values for more precise matches)</param>
        public ImageGestureImage(ulong[] rows, int width, float scorePadding)
        {
            Width = width;
            Height = rows.Length;
            Size = Width * Height;
            Rows = rows;
            Pixels = null;
            SimilarityPadding = scorePadding;
        }

        /// <summary>
        /// Clone this gesture image
        /// </summary>
        /// <returns>Clone</returns>
        public ImageGestureImage Clone()
        {
            return new ImageGestureImage
            {
                Height = Height,
                Name = Name,
                Rows = Rows.Clone() as ulong[],
                Pixels = (Pixels == null ? null : Pixels.Clone() as byte[]),
                SimilarityPadding = SimilarityPadding,
                Size = Size,
                Width = Width
            };
        }

        /// <summary>
        /// Get hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            if (Rows == null)
            {
                return base.GetHashCode();
            }
            int hashCode = 0;
            unchecked
            {
                foreach (ulong row in Rows)
                {
                    hashCode += (int)row;
                }
            }
            return hashCode;
        }

        /// <summary>
        /// Check for equality
        /// </summary>
        /// <param name="obj">Other object</param>
        /// <returns>True if equal, false if not</returns>
        public override bool Equals(object obj)
        {
            if (Rows == null)
            {
                return base.Equals(obj);
            }
            ImageGestureImage other = obj as ImageGestureImage;
            if (other == null || Rows.Length != other.Rows.Length)
            {
                return false;
            }
            for (int i = 0; i < Rows.Length; i++)
            {
                if (Rows[i] != other.Rows[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Recalculate which pixels are on and off into a fast lookup of bits in ulong rows
        /// </summary>
        /// <param name="pixels">Pixels (0 is off, 1 is on)</param>
        /// <param name="width">Width of the image</param>
        public void Initialize(byte[] pixels, int width)
        {
            Pixels = pixels;
            Width = width;
            Height = pixels.Length / Width;
            Size = Width * Height;
            Rows = new ulong[Height];
            int h = Height;
            int row = 0;
            while (h-- != 0)
            {
                ComputeRow(pixels, row++);
            }
        }

        /// <summary>
        /// Compute the similarity with another image. Images must be the same size to compare.
        /// </summary>
        /// <param name="other">Other image</param>
        /// <returns>Similarity (0 - 1)</returns>
        public float Similarity(ImageGestureImage other)
        {
            if (Rows == null || other == null || other.Rows == null || other.Rows.Length != Rows.Length)
            {
                return 0.0f;
            }
            int difference = 0;
            ulong xor;
            for (int i = 0; i < Rows.Length; i++)
            {
                // compute the difference, masking off bits we don't care about
                xor = (Rows[i] ^ other.Rows[i]) & ImageGestureRecognizer.RowBitmask;
                difference += NumberOfBitsSet(xor);
            }

            float similarity = (float)difference / (float)Size;
            similarity = (1.0f - similarity) + SimilarityPadding;

            return similarity;
        }

        /// <summary>
        /// Calculate how different this image is with another image. Images must be the same size to compare.
        /// </summary>
        /// <param name="other">Other image</param>
        /// <returns>Number of different pixels or -1 if unable to compare</returns>
        public int Difference(ImageGestureImage other)
        {
            if (Rows == null || other == null || other.Rows == null || other.Rows.Length != Rows.Length)
            {
                return -1;
            }
            int difference = 0;
            ulong xor;
            for (int i = 0; i < Rows.Length; i++)
            {
                // compute the difference, masking off bits we don't care about
                xor = (Rows[i] ^ other.Rows[i]) & ImageGestureRecognizer.RowBitmask;
                difference += NumberOfBitsSet(xor);
            }

            return difference;
        }

        /// <summary>
        /// Set a pixel to on. Does not recalculate any rows.
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="padding">Padding</param>
        /// <returns>True if pixel set, false if not</returns>
        public bool SetPixelWithPadding(int x, int y, int padding)
        {
            if (Pixels == null || x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }
            int index = (int)x + ((int)y * Width);
            Pixels[index] = 1;
            if (padding == 0)
            {
                return true;
            }
            else if (padding == 1)
            {
                // SetPixelWithPadding(x - 1, y, 0);
                SetPixelWithPadding(x + 1, y, 0);
                // SetPixelWithPadding(x, y - 1, 0);
                SetPixelWithPadding(x, y + 1, 0);
            }
            else if (padding == 2)
            {
                SetPixelWithPadding(x - 1, y - 1, 0);
                SetPixelWithPadding(x, y - 1, 0);
                SetPixelWithPadding(x + 1, y - 1, 0);

                SetPixelWithPadding(x - 1, y, 0);
                SetPixelWithPadding(x + 1, y, 0);

                SetPixelWithPadding(x - 1, y + 1, 0);
                SetPixelWithPadding(x, y + 1, 0);
                SetPixelWithPadding(x + 1, y + 1, 0);
            }
            else
            {
                throw new InvalidOperationException("Padding greater than 2 is not supported right now.");
            }

            return true;
        }

        /// <summary>
        /// Reset all rows to 0 and all pixels to 0
        /// </summary>
        public void Clear()
        {
            if (Rows != null)
            {
                for (int i = 0; i < Rows.Length; i++)
                {
                    Rows[i] = 0;
                }
            }
            if (Pixels != null)
            {
                for (int i = 0; i < Pixels.Length; i++)
                {
                    Pixels[i] = 0;
                }
            }
        }

        /// <summary>
        /// Gets a C# snippet that will create this image and populate the rows
        /// </summary>
        /// <param name="imageName">Image name (can be null)</param>
        /// <returns>C# script</returns>
        public string GetCodeForRowsInitialize(string imageName)
        {
            if (Rows == null || Rows.Length == 0)
            {
                throw new InvalidOperationException("Cannot generate C# script with null rows");
            }
            string prefix = (string.IsNullOrEmpty(imageName) ? string.Empty : "{ ");
            System.Text.StringBuilder b = new System.Text.StringBuilder(prefix + "new ImageGestureImage(new ulong[] { ");
            b.AppendFormat("0x{0:X16}", Rows[0]);
            for (int i = 1; i < Rows.Length; i++)
            {
                b.AppendFormat(", 0x{0:X16}", Rows[i]);
            }
            b.AppendFormat(" }}, imageWidth{0})", SimilarityPadding <= 0.0f ? string.Empty : ", " + SimilarityPadding.ToString("0.00") + "f");
            if (!string.IsNullOrEmpty(imageName))
            {
                b.Append(", \"" + imageName + "\" }");
            }
            return b.ToString();
        }

        /// <summary>
        /// Get the number of bits set in a ulong
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Number of bits set</returns>
        public static int NumberOfBitsSet(ulong value)
        {
            unchecked
            {
                value -= (value >> 1) & m1;                 //put count of each 2 bits into those 2 bits
                value = (value & m2) + ((value >> 2) & m2); //put count of each 4 bits into those 4 bits 
                value = (value + (value >> 4)) & m4;        //put count of each 8 bits into those 8 bits 
                return (int)(value * h01) >> 56;            //returns left 8 bits of x + (x<<8) + (x<<16) + (x<<24) + ... 
            }
        }

        /// <summary>
        /// Check if a bit is set in a ulong
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="bitPos">Bit position</param>
        /// <returns>True if the bit is set, false otherwise</returns>
        public static bool CheckBit(ulong value, int bitPos)
        {
            return (value & ((ulong)1 << bitPos)) != 0;
        }

        /// <summary>
        /// Width of the image
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the image
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Number of pixels in the image
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Each bit is a pixel, each ulong is a row. Least significant bit is x = 0
        /// </summary>
        public ulong[] Rows { get; private set; }

        /// <summary>
        /// Pixels of the image or null if no pixel data in which case just Rows is populated
        /// </summary>
        public byte[] Pixels { get; private set; }

        /// <summary>
        /// Score padding to allow fuzzier (or negative for more precise) matches for this image
        /// </summary>
        public float SimilarityPadding { get; set; }

        /// <summary>
        /// The amount the image matched, where 1.0 is a perfect match
        /// </summary>
        public float Score { get; internal set; }

        /// <summary>
        /// A human readable name for this image
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// Attempts to create an image out of the gesture and compare with a set of images
    /// </summary>
    public class ImageGestureRecognizer : DigitalRubyShared.GestureRecognizer
    {
        #region Constants

        private struct Point
        {
            public int X;
            public int Y;

            public override string ToString()
            {
                return "X: " + X + ", Y: " + Y;
            }
        }

        /// <summary>
        /// Masks to shave off exlusive or'd bits that we don't want to include in a row
        /// </summary>
        public static readonly ulong[] RowBitMasks = new ulong[]
        {
            0x0000000000000000, // 0
            0x0000000000000001, // 1
            0x0000000000000003, // 2
            0x0000000000000007, // 3
            0x000000000000000F, // 4
            0x000000000000001F, // 5
            0x000000000000003F, // 6
            0x000000000000007F, // 7
            0x00000000000000FF, // 8 
            0x00000000000001FF, // 9
            0x00000000000003FF, // 10
            0x00000000000007FF, // 11
            0x0000000000000FFF, // 12
            0x0000000000001FFF, // 13
            0x0000000000003FFF, // 14
            0x0000000000007FFF, // 15
            0x000000000000FFFF, // 16
            0x000000000001FFFF, // 17
            0x000000000003FFFF, // 18
            0x000000000007FFFF, // 19
            0x00000000000FFFFF, // 20
            0x00000000001FFFFF, // 21
            0x00000000003FFFFF, // 22
            0x00000000007FFFFF, // 23
            0x0000000000FFFFFF, // 24
            0x0000000001FFFFFF, // 25
            0x0000000003FFFFFF, // 26
            0x0000000007FFFFFF, // 27
            0x000000000FFFFFFF, // 28
            0x000000001FFFFFFF, // 29
            0x000000003FFFFFFF, // 30
            0x000000007FFFFFFF, // 31
            0x00000000FFFFFFFF, // 32
            0x00000001FFFFFFFF, // 33
            0x00000003FFFFFFFF, // 34
            0x00000007FFFFFFFF, // 35
            0x0000000FFFFFFFFF, // 36
            0x0000001FFFFFFFFF, // 37
            0x0000003FFFFFFFFF, // 38
            0x0000007FFFFFFFFF, // 39
            0x000000FFFFFFFFFF, // 40
            0x000001FFFFFFFFFF, // 41
            0x000003FFFFFFFFFF, // 42
            0x000007FFFFFFFFFF, // 43
            0x00000FFFFFFFFFFF, // 44
            0x00001FFFFFFFFFFF, // 45
            0x00003FFFFFFFFFFF, // 46
            0x00007FFFFFFFFFFF, // 47
            0x0000FFFFFFFFFFFF, // 48
            0x0001FFFFFFFFFFFF, // 49
            0x0003FFFFFFFFFFFF, // 50
            0x0007FFFFFFFFFFFF, // 51
            0x000FFFFFFFFFFFFF, // 52
            0x001FFFFFFFFFFFFF, // 53
            0x003FFFFFFFFFFFFF, // 54
            0x007FFFFFFFFFFFFF, // 55
            0x00FFFFFFFFFFFFFF, // 56
            0x01FFFFFFFFFFFFFF, // 57
            0x03FFFFFFFFFFFFFF, // 58
            0x07FFFFFFFFFFFFFF, // 59
            0x0FFFFFFFFFFFFFFF, // 60
            0x1FFFFFFFFFFFFFFF, // 61
            0x3FFFFFFFFFFFFFFF, // 62
            0x7FFFFFFFFFFFFFFF, // 63
            0xFFFFFFFFFFFFFFFF  // 64
        };

        /// <summary>
        /// Current row bit mask
        /// </summary>
        public static readonly ulong RowBitmask = RowBitMasks[ImageRows];

        /// <summary>
        /// Number of image rows. Should be greater than 4 and less than 65.
        /// </summary>
        public const int ImageRows = 16;

        /// <summary>
        /// Number of columns in the images for gestures. For square images (recommended) set to ImageRows.
        /// </summary>
        public const int ImageColumns = ImageRows;

        /// <summary>
        /// Number of elements in the images for gestures.
        /// </summary>
        public const int ImageSize = ImageRows * ImageColumns;

        /// <summary>
        /// The padding for each pixel converted from lines to the pixel image. Supported values are 0, 1 and 2.
        /// </summary>
        public const int LinePadding = 2;

        #endregion Constants

        private readonly List<List<Point>> points = new List<List<Point>>();
        private List<Point> currentList;
        private int minX;
        private int minY;
        private int maxX;
        private int maxY;

        private void AddPoint(float x, float y)
        {
            if (currentList == null)
            {
                return;
            }

            Point p = new Point { X = (int)x, Y = (int)y };

            if (currentList.Count < 2)
            {
                currentList.Add(p);
            }
            else
            {
                // see if we are within the tolerance for direction changes - if not, we extend the line
                Point last = currentList[currentList.Count - 1];
                Point beforeLast = currentList[currentList.Count - 2];
                float dx2 = last.X - beforeLast.X;
                float dy2 = last.Y - beforeLast.Y;
                float dx = p.X - last.X;
                float dy = p.Y - last.Y;
                float direction = (float)Math.Atan2(dy, dx);
                float direction2 = (float)Math.Atan2(dy2, dx2);
                float magnitudeUnits = DistanceBetweenPoints(last.X, last.Y, beforeLast.X, beforeLast.Y); // (float)Math.Sqrt((dx2 * dx2) + (dy2 * dy2));
                // float magnitudeUnits = DistanceBetweenPoints(p.X, p.Y, last.X, last.Y); // (float)Math.Sqrt((dx * dx) + (dy * dy));
                if (magnitudeUnits < MinimumDistanceBetweenPointsUnits ||
                    CompareFloat(direction, direction2, DirectionTolerance))
                {
                    currentList[currentList.Count - 1] = p;
                }
                else
                {
                    currentList.Add(p);
                }
            }
            minX = Math.Min(p.X, minX);
            minY = Math.Min(p.Y, minY);
            maxX = Math.Max(p.X, maxX);
            maxY = Math.Max(p.Y, maxY);

            // Log("Point: {0},{1}, Bounds: {2},{3},{4},{5}", p.X, p.Y, minX, minY, maxX, maxY);
        }

        private void ProcessTouches()
        {
            if (CurrentTrackedTouches.Count != 0)
            {
                GestureTouch t = CurrentTrackedTouches[0];
                AddPoint(t.X, t.Y);
            }
        }

        private bool CompareFloat(float v1, float v2, float tolerance)
        {
            return (Math.Abs(v1 - v2) < tolerance);
        }

        private void AddLineToGesturedImage(Point point1, Point point2, float s)
        {
            // normalize coordinates to within the rows and columns for the downsized image
            float x1 = (int)(((float)(point1.X - minX) / s) * (float)ImageColumns);
            float y1 = (int)(((float)(point1.Y - minY) / s) * (float)ImageRows);
            float x2 = (int)(((float)(point2.X - minX) / s) * (float)ImageColumns);
            float y2 = (int)(((float)(point2.Y - minY) / s) * (float)ImageRows);

            // calculate the slope
            float xDiff = x2 - x1;
            float yDiff = y2 - y1;
            float xSign = Math.Sign(xDiff);
            float ySign = Math.Sign(yDiff);
            float xIncrement, yIncrement;
            if (xDiff * xSign > yDiff * ySign)
            {
                xIncrement = 1.0f * xSign;
                yIncrement = yDiff / (xDiff * xSign);
            }
            else
            {
                xIncrement = xDiff / (yDiff * ySign);
                yIncrement = 1.0f * ySign;
            }

            // fill in points from point a to point b
            while (true)
            {
                Image.SetPixelWithPadding((int)x1, (int)y1, LinePadding);

                // check if we are at the end
                if (CompareFloat(x1, x2, 0.1f) && CompareFloat(y1, y2, 0.1f))
                {
                    break;
                }
                x1 += xIncrement;
                y1 += yIncrement;
            }
        }

        private void CalculateScores()
        {
            System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();

            // if no images to check we are done
            MatchedGestureImage = null;
            if (GestureImages == null || GestureImages.Count == 0 || currentList.Count < MinimumPointsToRecognize)
            {
                return;
            }

            // find the most similar image
            float lastSimilarity = 0.0f;
            foreach (ImageGestureImage image in GestureImages)
            {
                float similarity = image.Similarity(Image);
                image.Score = similarity;
                if (similarity > lastSimilarity && similarity >= SimilarityMinimum)
                {
                    lastSimilarity = similarity;
                    MatchedGestureImage = image;
                    Image.Score = similarity;
                }
            }

            w.Stop();

            MatchedGestureCalculationTimeMilliseconds = (int)w.Elapsed.TotalMilliseconds;
        }

        private void CheckImages()
        {
            // clear the image and redo all the pixels
            Image.Clear();

            // calculate width and height, with just a little extra to ensure that x and y values are between 0 and n - 1
            float w = (maxX - minX) + 0.05f;
            float h = (maxY - minY) + 0.05f;
            float s = Math.Max(w, h);

            // for each path, add a line for each set of points to the image
            foreach (List<Point> pointList in points)
            {
                for (int i = 1; i < pointList.Count; i++)
                {
                    AddLineToGesturedImage(pointList[i - 1], pointList[i], s);
                }
            }

            // recalculate rows
            Image.Initialize(Image.Pixels, Image.Width);

            // find which image matches the closest to the gestured image
            CalculateScores();
        }

        private void ResetImage()
        {
            Image.Clear();
            PathCount = 0;
            points.Clear();
            currentList = null;
            MatchedGestureImage = null;
            minX = int.MaxValue;
            minY = int.MaxValue;
            maxX = int.MinValue;
            maxY = int.MinValue;
        }

        private bool CanExecute()
        {
            bool executing = (State == GestureRecognizerState.Began || State == GestureRecognizerState.Executing);
            CalculateFocus(CurrentTrackedTouches);
            if (executing)
            {
                ProcessTouches();
                return true;
            }
            else if (State != GestureRecognizerState.Possible)
            {
                return false;
            }

            float distance = Distance(DistanceX, DistanceY);
            if (distance >= ThresholdUnits)
            {
                if (PathCount++ >= MaximumPathCount)
                {
                    PathCount = 1;
                    if (MaximumPathCountExceeded != null)
                    {
                        MaximumPathCountExceeded(this, EventArgs.Empty);
                    }
                }
                currentList = new List<Point>();
                points.Add(currentList);
                SetState(GestureRecognizerState.Began);
                AddPoint(StartFocusX, StartFocusY);
                return true;
            }
            SetState(GestureRecognizerState.Possible);
            return false;
        }

        /// <summary>
        /// TouchesBegan
        /// </summary>
        /// <param name="touches"></param>
        protected override void TouchesBegan(IEnumerable<GestureTouch> touches)
        {
            CalculateFocus(CurrentTrackedTouches, true);
        }

        /// <summary>
        /// TouchesMoved
        /// </summary>
        protected override void TouchesMoved()
        {
            if (!CanExecute())
            {
                return;
            }
            SetState(GestureRecognizerState.Executing);
        }

        /// <summary>
        /// TouchesEnded
        /// </summary>
        protected override void TouchesEnded()
        {
            if (!CanExecute())
            {
                return;
            }
            CheckImages();
            SetState(GestureRecognizerState.Ended);

            if (PathCount == MaximumPathCount)
            {
                // keep number of paths around, we want to fire the max path exceeded the next time the gesture begins
                int tmp = PathCount;
                Reset();
                PathCount = tmp;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ImageGestureRecognizer()
        {
            MaximumPathCount = 1;
            ThresholdUnits = 0.4f;
            DirectionTolerance = 0.3f;
            SimilarityMinimum = 0.8f;
            MinimumDistanceBetweenPointsUnits = 0.1f;
            MinimumPointsToRecognize = 2;
            Image = new ImageGestureImage();
            Image.Initialize(new byte[ImageSize], ImageColumns);
            GestureImages = new List<ImageGestureImage>();
            Reset();
        }

        /// <summary>
        /// Reset the image gesture recognizer
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            ResetImage();
        }

        /// <summary>
        /// The maximum number of distinct paths for each image. Gesture will reset when max path count is hit. The default is 1.
        /// </summary>
        public int MaximumPathCount { get; set; }

        /// <summary>
        /// The amount that the path must change direction (in radians) to count as a new direction (0.39 is 1.8 of PI). Default is 0.3.
        /// </summary>
        public float DirectionTolerance { get; set; }

        /// <summary>
        /// The distance in units that the touch must move before the gesture begins - default is 0.4
        /// </summary>
        public float ThresholdUnits { get; set; }

        /// <summary>
        /// Minimum difference beteen points in units to count as a new point. Default is 0.1.
        /// </summary>
        public float MinimumDistanceBetweenPointsUnits { get; set; }

        /// <summary>
        /// The amount that the gesture image must match an image from the set to count as a match (0 - 1). Default is 0.8.
        /// </summary>
        public float SimilarityMinimum { get; set; }

        /// <summary>
        /// The minimum number of points before the gesture will recognize - default is 2
        /// </summary>
        public int MinimumPointsToRecognize { get; set; }

        /// <summary>
        /// The images that should be compared against to find a match
        /// </summary>
        public List<ImageGestureImage> GestureImages { get; set; }

        /// <summary>
        /// Fires when the maximum path count is exceeded
        /// </summary>
        public event EventHandler MaximumPathCountExceeded;

        /// <summary>
        /// Calculated image from the gesture
        /// </summary>
        public ImageGestureImage Image { get; private set; }

        /// <summary>
        /// The matched gesture image or null if no match
        /// </summary>
        public ImageGestureImage MatchedGestureImage { get; private set; }

        /// <summary>
        /// The last time (in milliseconds) that was needed to calculate the matched gesture
        /// </summary>
        public float MatchedGestureCalculationTimeMilliseconds { get; private set; }

        /// <summary>
        /// This gesture does not reset on end like most gestures - you must reset it manually when you get the image out of it you want
        /// </summary>
        public override bool ResetOnEnd { get { return false; } }

        /// <summary>
        /// The current number of distinct paths that have been drawn
        /// </summary>
        public int PathCount { get; private set; }
    }
}