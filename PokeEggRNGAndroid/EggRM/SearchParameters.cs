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
        LeastAdvances,
        MainEggRNG
    }

    public enum SearchRange
    {
        Simple,
        MinMax,
        AroundTarget
    }

    public enum MainSearchRange {
        Simple,
        MinMax,
        AroundTarget,
        CreateTimeline,
        TimelineLeap
    }

    public struct MainRNGSearchParams {
        public MainSearchRange mainRange;
        public int startFrame;
        public int minFrame, maxFrame;
        public bool considerDelay;  public int delay;
        public int npcs;
        public int ctimeline;
        public int timeleap1, timeleap2;

        public byte Modelnum => (byte)(npcs + 1);
        public bool Raining;
    }

    public struct SearchParams {
        public SearchType type;
        public SearchRange range;

        public bool useFilter;
        public bool checkOtherTSV;
        public int targetFrame;

        public MainRNGSearchParams mainRNG;
    }
}