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

namespace Gen7EggRNG.EggRM
{
    public class RTStrings
    {

        public readonly string[] genderSymbols = { "-", "♂", "♀" };
        public readonly string[] abilitySymbols = { "-", "1", "2", "H" };

        public readonly string[] natures;
        public readonly string[] hiddenpowers;

        public readonly string dustoxName;
        public readonly string beautiflyName;

        //public readonly string[] genders; // -, M, F
        //public readonly string[] genderRatios;

        //public readonly string[] abilities; // 1,2,H

        public readonly string accept;
        public readonly string reject;

        public readonly string male;
        public readonly string female;

        public readonly string option_any;
        public readonly string option_none;

        public readonly string option_ok;
        public readonly string option_cancel;

        public readonly string acceptplus_reject_count;

        public readonly string profileinfoseed;
        public readonly string profileinfotsv;

        public RTStrings(Context context) {
            Resources rc = context.Resources;

            natures = rc.GetStringArray(Resource.Array.NatureIndexed);
            hiddenpowers = rc.GetStringArray(Resource.Array.HiddenPowerIndexed);

            accept = rc.GetString(Resource.String.accept);
            reject = rc.GetString(Resource.String.reject);

            male= rc.GetString(Resource.String.male);
            female= rc.GetString(Resource.String.female);

            dustoxName = rc.GetString(Resource.String.dustox);
            beautiflyName = rc.GetString(Resource.String.beautifly);

            option_any = rc.GetString(Resource.String.spinner_option_all);
            option_none = rc.GetString(Resource.String.spinner_option_none);

            option_ok = rc.GetString(Resource.String.ok);
            option_cancel = rc.GetString(Resource.String.cancel);

            acceptplus_reject_count = rc.GetString(Resource.String.search_acceptplus_rcount);

            profileinfoseed = rc.GetString(Resource.String.search_profileinfo_currentseed);
            profileinfotsv = rc.GetString(Resource.String.search_profileinfo_tsv);
        }
    }
}