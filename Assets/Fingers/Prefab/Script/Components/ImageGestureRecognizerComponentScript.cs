//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows recognizing gestures that match the pattern of an image
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Gesture/Fingers Image Recognition Gesture", 8)]
    public class ImageGestureRecognizerComponentScript : GestureRecognizerComponentScript<ImageGestureRecognizer>
    {
        /// <summary>The maximum number of distinct paths for each image. Gesture will reset when max path count is hit.</summary>
        [Header("Image gesture properties")]
        [Tooltip("The maximum number of distinct paths for each image. Gesture will reset when max path count is hit.")]
        [Range(1, 5)]
        public int MaximumPathCount = 1;

        /// <summary>The amount that the path must change direction (in radians) to count as a new direction (0.39 is 1.8 of PI).</summary>
        [Tooltip("The amount that the path must change direction (in radians) to count as a new direction (0.39 is 1.8 of PI).")]
        [Range(0.01f, 1.0f)]
        public float DirectionTolerance = 0.3f;

        /// <summary>The distance in units that the touch must move before the gesture begins.</summary>
        [Tooltip("The distance in units that the touch must move before the gesture begins.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.4f;

        /// <summary>Minimum difference beteen points in units to count as a new point.</summary>
        [Tooltip("Minimum difference beteen points in units to count as a new point.")]
        [Range(0.01f, 1.0f)]
        public float MinimumDistanceBetweenPointsUnits = 0.1f;

        /// <summary>The amount that the gesture image must match an image from the set to count as a match (0 - 1).</summary>
        [Tooltip("The amount that the gesture image must match an image from the set to count as a match (0 - 1).")]
        [Range(0.01f, 1.0f)]
        public float SimilarityMinimum = 0.8f;

        /// <summary>The minimum number of points before the gesture will recognize.</summary>
        [Tooltip("The minimum number of points before the gesture will recognize.")]
        [Range(2, 10)]
        public int MinimumPointsToRecognize = 2;

        /// <summary>The images that should be compared against to find a match (can be a file name). The values are a ulong which match the bits of each generated image. See DemoSceneImage &amp; DemoScriptImage.cs for an example.</summary>
        [Tooltip("The images that should be compared against to find a match (can be a file name). The values are a ulong which match the bits of each generated image. See DemoSceneImage & DemoScriptImage.cs for an example.")]
        public List<ImageGestureRecognizerComponentScriptImageEntry> GestureImages;

        /// <summary>
        /// Allows looking up a key from a matched image
        /// </summary>
        public Dictionary<ImageGestureImage, string> GestureImagesToKey { get; private set; }

        /// <summary>
        /// OnEnable
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.MaximumPathCount = MaximumPathCount;
            Gesture.DirectionTolerance = DirectionTolerance;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumDistanceBetweenPointsUnits = MinimumDistanceBetweenPointsUnits;
            Gesture.SimilarityMinimum = SimilarityMinimum;
            Gesture.MinimumPointsToRecognize = MinimumPointsToRecognize;
            ReloadGestureImageEntries();
        }

        /// <summary>
        /// Reload all gesture images from the GestureImages field
        /// </summary>
        public void ReloadGestureImageEntries()
        {
            Gesture.GestureImages = new List<ImageGestureImage>();
            GestureImagesToKey = new Dictionary<ImageGestureImage, string>();
            foreach (ImageGestureRecognizerComponentScriptImageEntry img in GestureImages)
            {
                List<ulong> rows = new List<ulong>();
                string imageText;
                if (File.Exists(img.Images))
                {
                    imageText = File.ReadAllText(img.Images);
                }
                else
                {
                    imageText = img.Images;
                }
                foreach (string ulongs in imageText.Split('\n'))
                {
                    string trimmed = ulongs.Trim();
                    try
                    {
                        // trim out scripting code
                        Match nameMatch = Regex.Match(trimmed, "\"(?<name>[^\"]+)\" ?},?$");
                        string name = (nameMatch.Success ? nameMatch.Groups["name"].Value : img.Key).Replace("\\\\", "\\");
                        trimmed = Regex.Replace(trimmed, @" *?\{ new ImageGestureImage\(new ulong\[\] *?\{ *?", string.Empty);
                        trimmed = Regex.Replace(trimmed, @" *?\}.+$", string.Empty);

                        if (trimmed.Length != 0)
                        {
                            string[] rowStrings = trimmed.Trim().Split(',');
                            foreach (string rowString in rowStrings)
                            {
                                string _rowString = rowString.Trim();
                                if (_rowString.StartsWith("0x"))
                                {
                                    _rowString = _rowString.Substring(2);
                                }
                                rows.Add(ulong.Parse(_rowString, System.Globalization.NumberStyles.HexNumber));
                            }
                            ImageGestureImage image = new ImageGestureImage(rows.ToArray(), ImageGestureRecognizer.ImageColumns, img.ScorePadding);
                            image.Name = name;
                            Gesture.GestureImages.Add(image);
                            GestureImagesToKey[image] = img.Key;
                            rows.Clear();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogFormat("Error parsing image gesture image: {0} - {1}", trimmed, ex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Entry for an image gesture
    /// </summary>
    [System.Serializable]
    public struct ImageGestureRecognizerComponentScriptImageEntry
    {
        /// <summary>Key, should be unique</summary>
        [Tooltip("Key, should be unique")]
        public string Key;

        /// <summary>Score padding, makes it easier to match</summary>
        [Tooltip("Score padding, makes it easier to match")]
        [Range(0.0f, 0.5f)]
        public float ScorePadding;

        /// <summary>Comma separated list of hex format ulong for each row, separated by newlines. Can also be a file name.</summary>
        [TextArea(1, 8)]
        [Tooltip("Comma separated list of hex format ulong for each row, separated by newlines. Can also be a file name.")]
        public string Images;

        /// <summary>Custom description or can be used to store extended information like comma separated numbers, etc.</summary>
        [Tooltip("Custom description or can be used to store extended information like comma separated numbers, etc.")]
        public string Description;
    }
}
