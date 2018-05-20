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
    public enum GameVersionUI
    {
        Sun,
        Moon,
        UltraSun,
        UltraMoon
    }

    public class GameVersionConversion
    {
        public static int GetGameStartingFrame(GameVersionUI gv, bool idFrame) {
            if ((int)gv < 2) {
                return idFrame ? 1012 : 418;
            }
            return idFrame ? 1132 : 478;
        }

        public static bool IsUltra(GameVersionUI gv) {
            return gv == GameVersionUI.UltraSun || gv == GameVersionUI.UltraMoon;
        }

        public static string GetGameVersionName(GameVersionUI gv) {
            return PokeRNGApp.Strings.gameVersion[(int)gv];
            //return gv == GameVersionUI.Sun ? "Sun" : gv == GameVersionUI.Moon ? "Moon" : gv == GameVersionUI.UltraSun ? "Ultra Sun" : "Ultra Moon";
        }
    }
}