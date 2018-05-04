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
    public static class HiddenPower
    {
        private static string[] hpStrings = new string[] { "Fighting", "Flying", "Poison", "Ground", "Rock", "Bug", "Ghost", "Steel", "Fire", "Water", "Grass", "Electric", "Psychic", "Ice", "Dragon", "Dark" };

        public static string GetHiddenPowerString(byte id)
        {
            return hpStrings[id];
        }

        public readonly static byte[] Reorder2 = { 0, 1, 2, 4, 5, 3 }; // Normal index to In-Game index
        public static int GetHiddenPowerValue(int[] IVs)
        {
            return 15 * IVs.Select((iv, i) => (iv & 1) << Reorder2[i]).Sum() / 63;
        }
    }
}