using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Gen7EggRNG.Util
{
    public static class DebugUtil
    {
        public static void DumpSharedPreferences(Context context) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var keys = prefs.All;
            foreach (var entry in keys)
            {
                Android.Util.Log.Info("SharedPrefEntry", entry.Key + " : " + entry.Value);
            }
        }
    }
}