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

    public enum SearchType
    {
        NormalSearch,
        EggAccept,
        EggAcceptPlus,
        ShortestPath,
        LeastAdvances
    }

    public enum SearchRange
    {
        Simple,
        MinMax,
        AroundTarget

    }

    public struct SearchParams {
        public SearchType type;
        public SearchRange range;

        public bool useFilter;
        public bool checkOtherTSV;
        public int targetFrame;
    }
}