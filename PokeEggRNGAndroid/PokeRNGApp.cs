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
using Pk3DSRNGTool;

namespace Gen7EggRNG
{
    [Application]
    public class PokeRNGApp : Application
    {
        public static RTStrings Strings { get; private set; }
        public static byte[] personal_ao;
        public static byte[] personal_uu;

        public static PersonalTable ORAS;
        public static PersonalTable USUM;

        public static string[] speciesStrings;

        private Locale locale = null;

        public PokeRNGApp(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership) {

        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            // In case of locale change, update strings.
            Strings = new RTStrings(Context);
            
        }

        public override void OnCreate()
        {
            base.OnCreate();

            Strings = new RTStrings(Context);

            personal_ao = FileHelper.ReadFileBytes(Context.Assets, "personal_ao");
            personal_uu = FileHelper.ReadFileBytes(Context.Assets, "personal_uu");

            ORAS = new PersonalTable(personal_ao, GameVersion.ORAS);
            USUM = new PersonalTable(personal_uu, GameVersion.USUM);

            speciesStrings = Resources.GetStringArray(Resource.Array.Species);
        }
    }
}