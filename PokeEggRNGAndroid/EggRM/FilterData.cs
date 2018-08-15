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
using Pk3DSRNGTool.Core;

using Android.Preferences;
using Pk3DSRNGTool;

namespace Gen7EggRNG.EggRM
{
    public class FilterData
    {
        public int[] ivMin;
        public int[] ivMax;

        public int ball;
        public int gender;
        public int ability;

        public bool[] hiddenPowers;
        public bool[] natures;

        public bool shinyRemind;
        public bool shinyOnly;

        public int nPerfects;

        public bool blinkFOnly;
        public bool safeFOnly;

        public FilterData() {
            ivMin = new int[6] {  0,  0,  0,  0,  0,  0 };
            ivMax = new int[6] { 31, 31, 31, 31, 31, 31 };

            ball = gender = ability = 0;

            hiddenPowers = Enumerable.Repeat(false, 16).ToArray();
            natures = Enumerable.Repeat(false, 25).ToArray();

            shinyRemind = shinyOnly = blinkFOnly = safeFOnly = false;

            nPerfects = 0;
        }

        public bool VerifyEgg(EggResult egg) {
            return VerifyIVs(egg) && VerifyGender(egg) &&
                   VerifyBall(egg) && VerifyAbility(egg) &&
                   VerifyHiddenPower(egg) && VerifyNature(egg) &&
                   VerifyShininess(egg);
        }

        private bool VerifyIVs(RNGResult res) {
            for (int i = 0; i < 6; ++i) {
                if ( !(ivMin[i] <= res.IVs[i] && res.IVs[i] <= ivMax[i])) {
                    return false;
                }
            }
            if (nPerfects > 0) {
                if (res.IVs.Count(x => x == 31) < nPerfects) { return false; }
            }
            return true;
        }

        private bool VerifyGender(RNGResult egg) {
            if ( gender == 0 ) { return true; }
            else if (egg.Gender == gender) {
                return true;
            }
            return false;
        }

        private bool VerifyBall(EggResult egg) {
            if (ball == 0) { return true; }
            else if (ball == egg.Ball) { return true; }
            return false;
        }

        private bool VerifyAbility(RNGResult egg) {
            if (ability == 0) { return true; }
            // 1 = Slot 1, 2 = Slot 2, 3 = Hidden
            else if (ability == egg.Ability) {
                return true;
            }
            return false;
        }

        private bool VerifyHiddenPower(RNGResult egg)
        {
            if ((hiddenPowers.Count( i => i == true ) == 0 )) { return true; }
            //check if hidden power is contained in array
            return hiddenPowers[egg.hiddenpower];
        }

        private bool VerifyNature(RNGResult egg)
        {
            if ((natures.Count(i => i == true) == 0)) { return true; }
            //check if nature is contained in array
            return natures[egg.Nature];
        }

        private bool VerifyShininess(RNGResult egg)
        {
            if (shinyOnly) {
                return egg.Shiny;
            }
            return true;
        }

        public bool VerifyRemind(EggResult egg, int tsv, bool checkOther, List<int> otherTSV) {
            if (shinyRemind) {
                uint rn = egg.RandNum;
                int sv = (int)((rn >> 16) ^ (rn & 0xFFFF)) >> 4;
                return sv == tsv || (checkOther && otherTSV.Contains(sv));
            }
            return false;
        }



        public bool VerifyStationary(Result7 stationary) {
            return VerifyIVs(stationary) && VerifyGender(stationary) &&
                   VerifyAbility(stationary) &&
                   VerifyHiddenPower(stationary) && VerifyNature(stationary) &&
                   VerifyShininess(stationary);
        }

        /*private bool CheckRandomNumber(uint rn, int tsv, List<int> otherTSV)
        {
            int sv = (int)((rn >> 16) ^ (rn & 0xFFFF)) >> 4;
            return sv == tsv || otherTSV.Contains(sv);
        }*/


        public static FilterData LoadFilterData(Context context) {
            FilterData fd = new FilterData();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            fd.ivMin = Array.ConvertAll(prefs.GetString("FilterMinIV", "0,0,0,0,0,0").Split(','), s => int.Parse(s));
            fd.ivMax = Array.ConvertAll(prefs.GetString("FilterMaxIV", "31,31,31,31,31,31").Split(','), s => int.Parse(s));

            fd.ball = prefs.GetInt("FilterBall", 0);
            fd.gender = prefs.GetInt("FilterGender", 0);
            fd.ability = prefs.GetInt("FilterAbility", 0);

            string hps = prefs.GetString("FilterHiddenPower", String.Empty);
            string natures = prefs.GetString("FilterNature", String.Empty);
            if (hps != String.Empty && hps.Length == 16) { fd.hiddenPowers = Array.ConvertAll(hps.ToCharArray(), x => (x == '0' ? false : true)); }
            if (natures != String.Empty && natures.Length == 25) { fd.natures = Array.ConvertAll(natures.ToCharArray(), x => (x == '0' ? false : true)); }

            fd.shinyOnly = prefs.GetBoolean("FilterShinyOnly", false);
            fd.shinyRemind = prefs.GetBoolean("FilterShinyRemind", false);

            fd.blinkFOnly = prefs.GetBoolean("FilterBlinkF", false);
            fd.safeFOnly = prefs.GetBoolean("FilterSafeF", false);

            fd.nPerfects = prefs.GetInt("FilterNPerfects", 0);

            return fd;
        }

        public static void SaveFilterData(Context context, FilterData data )
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            ISharedPreferencesEditor prefsEdit = prefs.Edit();
            if (data.ivMin.Length == 6)
            {
                prefsEdit.PutString("FilterMinIV", string.Join(",", data.ivMin));
            }
            if (data.ivMax.Length == 6)
            {
                prefsEdit.PutString("FilterMaxIV", string.Join(",", data.ivMax));
            }

            prefsEdit.PutInt("FilterBall", data.ball);
            prefsEdit.PutInt("FilterGender", data.gender);
            prefsEdit.PutInt("FilterAbility", data.ability);
            
            string hps = new string(Array.ConvertAll(data.hiddenPowers, x => (x == true ? '1' : '0')));
            prefsEdit.PutString("FilterHiddenPower", hps );

            string nats = new string(Array.ConvertAll(data.natures, x => (x == true ? '1' : '0')));
            prefsEdit.PutString("FilterNature", nats);

            prefsEdit.PutBoolean("FilterShinyOnly", data.shinyOnly);
            prefsEdit.PutBoolean("FilterShinyRemind", data.shinyRemind);

            prefsEdit.PutBoolean("FilterBlinkF", data.blinkFOnly);
            prefsEdit.PutBoolean("FilterSafeF", data.safeFOnly);

            prefsEdit.PutInt("FilterNPerfects", data.nPerfects);

            prefsEdit.Commit();
        }
    }


    public class FilterIVTemplate
    {
        public string name;
        public IVSet ivsmin;
        public IVSet ivsmax;


        public static List<FilterIVTemplate> LoadFilterTemplates(Context context)
        {
            List<FilterIVTemplate> list = new List<FilterIVTemplate>();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numTemps = prefs.GetInt("FilterTempNum", 0);

            if (numTemps > 0)
            {
                for (int i = 1; i <= numTemps; ++i)
                {
                    FilterIVTemplate newTpl = new FilterIVTemplate();

                    newTpl.name = prefs.GetString("FilterTemp" + i + "Name", "?????");
                    newTpl.ivsmin.SetFromString(prefs.GetString("FilterTemp" + i + "Min", "0,0,0,0,0,0"));
                    newTpl.ivsmax.SetFromString(prefs.GetString("FilterTemp" + i + "Max", "0,0,0,0,0,0"));

                    list.Add(newTpl);
                }
            }

            return list;
        }

        public static void SaveFilterTemplates(Context context, List<FilterIVTemplate> templates)
        {
            if (templates != null)
            {
                DeleteFilterTemplates(context);

                if (templates.Count > 0)
                {
                    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
                    var edit = prefs.Edit();

                    for (int i = 0; i < templates.Count; ++i)
                    {
                        edit.PutString("FilterTemp" + (i + 1) + "Name", templates[i].name);
                        edit.PutString("FilterTemp" + (i + 1) + "Min", templates[i].ivsmin.ToString());
                        edit.PutString("FilterTemp" + (i + 1) + "Max", templates[i].ivsmax.ToString());
                    }
                    edit.PutInt("FilterTempNum", templates.Count);
                    edit.Commit();
                }
            }
        }

        public static void DeleteFilterTemplates(Context context)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numFilters = prefs.GetInt("FilterTempNum", 0);

            if (numFilters > 0)
            {
                var edit = prefs.Edit();
                edit.Remove("FilterTempNum");

                for (int i = 1; i <= numFilters; ++i)
                {
                    edit.Remove("FilterTemp" + i + "Name");
                    edit.Remove("FilterTemp" + i + "Min");
                    edit.Remove("FilterTemp" + i + "Max");
                }

                edit.Commit();
            }
        }
    }
}