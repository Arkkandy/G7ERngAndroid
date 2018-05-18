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
        public int ctimelineTime;
        public int timeleap1, timeleap2;

        public int ShiftStandard;
        public byte Modelnum => (byte)(npcs + 1);
        public bool Raining;
    }

    public struct EggRNGSearchParams {
        public SearchRange eggRange;
        public bool checkOtherTSV;
        public int minFrame, maxFrame;
    }

    public struct SearchParams {
        public SearchType type;

        public bool useFilter;
        public int targetFrame;
        public int aroundTarget;

        public EggRNGSearchParams eggRNG;
        public MainRNGSearchParams mainRNG;
    }
}