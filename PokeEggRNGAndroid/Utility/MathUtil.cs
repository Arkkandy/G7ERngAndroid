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

namespace Gen7EggRNG.Util
{
    public static class MathUtil
    {
        public static int GetNumDigits(int value) {
            if (value == 0) { return 0; }
            return (int)Math.Floor(Math.Log10(Math.Abs(value)) + 1);
        }
    }
}