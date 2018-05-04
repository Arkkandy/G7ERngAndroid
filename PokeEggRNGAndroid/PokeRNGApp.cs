using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    [Application]
    public class PokeRNGApp : Application
    {
        public static RTStrings Strings { get; private set; }

        private Locale locale = null;

        public PokeRNGApp(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) {

        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Strings = new RTStrings(Context);
        }
    }
}