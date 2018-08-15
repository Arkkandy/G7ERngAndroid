using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Gen7EggRNG.EggRM
{
    public static class SearchConstants {
        public const int ResultLimitLow = 500;
        public const int ResultLimitMedium = 1500;
        public const int ResultLimitHigh = 5000;
        public const int ResultLimitVeryHigh = 10000;

        public const int MaximumFramesSimpleSearch = 50000;
        public const int MaximumFramesPerSearch = 500000;

        public const int MaximumEggTargetFrame = 25000;
        public const int MaximumMainTargetFrame = 2000000;

        public const int ShortestPathHeuristicLow = 750; // Other params
        public const int ShortestPathHeuristicHigh = 1000; // MM + SC + DK

        public const int MaximumTimelineTime = 36000;

        public const int MaximumHistory = 10;
    }

    public static class ColorValues {

        public static readonly Android.Graphics.Color MaleGenderColor = new Android.Graphics.Color(3, 144, 255, 255);
        public static readonly Android.Graphics.Color FemaleGenderColor = Android.Graphics.Color.HotPink;

        public static readonly Android.Graphics.Color PerfectIVColor = Android.Graphics.Color.LawnGreen;
        public static readonly Android.Graphics.Color NoGoodIVColor = new Android.Graphics.Color(255,64,32,255);

        public static readonly Android.Graphics.Color DefaultTextColor = new Android.Graphics.Color(224, 224, 224, 255);

        public static readonly Android.Graphics.Color[] ShinyColor = {
            new Android.Graphics.Color( 16, 32, 190, 196 ),//new Android.Graphics.Color( 137, 206, 250, 196 ), // Blue
            new Android.Graphics.Color( 138,  43, 226, 196 ), //new Android.Graphics.Color( 255, 105, 190, 144 ), // Pink
            new Android.Graphics.Color( 255, 255, 105, 196 ), // Yellow
            new Android.Graphics.Color( 0, 250, 154, 196 ), // Green -> Blueish green
            new Android.Graphics.Color( 196,  114,  32, 196 ), // Red -> Orange
            new Android.Graphics.Color( 128, 128, 128, 196 )  // Gray
        };
    }

    public static class PokemonData {

        public static readonly int[] hpAlphabeticalIndices = new int[16] { 5, 15, 14, 11, 0, 8, 1, 10, 6, 3, 13, 2, 12, 4, 7, 9 };
        public static readonly int[] natureAlphabeticalIndices = new int[25] { 3, 18, 5, 2, 20, 23, 6, 21, 0, 11, 8, 13, 9, 1, 16, 15, 14, 4, 17, 24, 19, 7, 22, 12, 10 };
    }

    /*public class PokemonStrings {

    }*/

    public class StoragePreferences
    {
        /*public string FilterMinIV = "FilterMinIV";
        public string FilterMaxIV = "FilterMaxIV";*/
    }
}