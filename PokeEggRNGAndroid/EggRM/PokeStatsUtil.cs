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
    public class PokeStatsUtil
    {
        public static int[] GetMinStats(int[] baseStat, int level) {
            int[] minStats = new int[6];
            minStats[0] = (((baseStat[0] * 2 + 0) * level) / 100) + level + 10;
            for (int i = 1; i < 6; i++)
            {
                minStats[i] = (((baseStat[i] * 2 + 0) * level) / 100) + 5;
                minStats[i] = (int)((float)minStats[i] * 0.9);
            }
            return minStats;
        }
        public static int[] GetMaxStats(int[] baseStat, int level) {
            int[] maxStats = new int[6];
            maxStats[0] = (((baseStat[0] * 2 + 31) * level) / 100) + level + 10;
            for (int i = 1; i < 6; i++)
            {
                maxStats[i] = (((baseStat[i] * 2 + 31) * level) / 100) + 5;
                maxStats[i] = (int)((float)maxStats[i] * 0.9);
            }
            return maxStats;
        }
    }
}