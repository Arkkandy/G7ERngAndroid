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

namespace Gen7EggRNG.EggRM
{
    public class AppPreferences
    {
        public int rowHeight;
        public int maxResults;
        public int shinyColor;
        public int otherTsvColor;
        public bool autoSearch;
        public bool allRandomGender;
        public bool allAbility;
        public bool showProfileData;

        public AppPreferences() {

        }

        public AppPreferences(AppPreferences pref) {
            rowHeight = pref.rowHeight;
            maxResults = pref.maxResults;
            shinyColor = pref.shinyColor;
            otherTsvColor = pref.otherTsvColor;
            autoSearch = pref.autoSearch;
            allRandomGender = pref.allRandomGender;
            allAbility = pref.allAbility;
            showProfileData = pref.showProfileData;
        }

        public static AppPreferences LoadPreferencesData(Context context)
        {
            AppPreferences ap = new AppPreferences();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            ap.rowHeight = prefs.GetInt("PrefsRowHeight", 0);
            ap.maxResults = prefs.GetInt("PrefsMaxResults", 0);
            ap.shinyColor = prefs.GetInt("PrefsShiny", 0);
            ap.otherTsvColor = prefs.GetInt("PrefsOtherTSV", 0);
            ap.autoSearch = prefs.GetBoolean("PrefsAutoSearch", false);
            ap.allRandomGender = prefs.GetBoolean("PrefsAllRandomGender", false);
            ap.allAbility = prefs.GetBoolean("PrefsAllAbility", false);
            ap.showProfileData = prefs.GetBoolean("PrefsShowProfile", true);
            return ap;
        }

        public static void SavePreferencesData(Context context, AppPreferences data)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            ISharedPreferencesEditor prefsEdit = prefs.Edit();

            prefsEdit.PutInt("PrefsRowHeight", data.rowHeight);
            prefsEdit.PutInt("PrefsMaxResults", data.maxResults);
            prefsEdit.PutInt("PrefsShiny", data.shinyColor);
            prefsEdit.PutInt("PrefsOtherTSV", data.otherTsvColor);
            prefsEdit.PutBoolean("PrefsAutoSearch", data.autoSearch);
            prefsEdit.PutBoolean("PrefsAllRandomGender", data.allRandomGender);
            prefsEdit.PutBoolean("PrefsAllAbility", data.allAbility);
            prefsEdit.PutBoolean("PrefsShowProfile", data.showProfileData);

            prefsEdit.Commit();
        }

        public int MaxResultValue() {
            if (maxResults == 1)
            {
                return SearchConstants.ResultLimitMedium;
            }
            else if (maxResults == 2)
            {
                return SearchConstants.ResultLimitHigh;
            }
            else if (maxResults == 3) {
                return SearchConstants.ResultLimitVeryHigh;
            }

            return SearchConstants.ResultLimitLow;
        }
    }
}