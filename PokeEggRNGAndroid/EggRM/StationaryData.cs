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
    public class StationaryData
    {
        public int level;
        public int[] baseStats;
        public int genderType;
        public bool synchronizer;
        public int synchronizeNature;
        public bool alwaysSync;
        public bool abilityLocked;
        public int ability;
        public bool fixed3IVs;
        public bool shinyLocked;
        public bool raining;
        public int defaultDelay;
        public int defaultNPC;

        // Hidden parameters
        public int delayType;
        public bool noBlink;

        public bool BlinkWhenSync => !alwaysSync && !noBlink;

        private static readonly string sttLevel = "SttPokeLevel";
        private static readonly string sttBase = "SttPokeBase";
        private static readonly string sttGender = "SttPokeGender";
        private static readonly string sttSync = "SttPokeSync";
        private static readonly string sttSyncNature = "SttPokeSyncNat";
        private static readonly string sttSyncAlways = "SttPokeSyncAlways";
        private static readonly string sttAbilityLock = "SttPokeAbilityLock";
        private static readonly string sttAbility = "SttPokeAbility";
        private static readonly string sttFixed3IV = "SttPokeFixed3IV";
        private static readonly string sttShinyLock = "SttPokeShinyLock";
        private static readonly string sttRaining = "SttPokeRaining";
        private static readonly string sttDelay = "SttPokeDelay";
        private static readonly string sttNPC = "SttPokeNPC";

        private static readonly string sttDelayType = "SttDelayType";
        private static readonly string sttNoBlink = "SttNoBlink";

        public StationaryData() {
            baseStats = new int[] { 100, 100, 100, 100, 100, 100 };
        }

        public StationaryData(StationaryData sd) {
            level = sd.level;
            baseStats = (int[])sd.baseStats.Clone();
            genderType = sd.genderType;
            synchronizer = sd.synchronizer;
            synchronizeNature = sd.synchronizeNature;
            alwaysSync = sd.alwaysSync;
            abilityLocked = sd.abilityLocked;
            ability = sd.ability;
            fixed3IVs = sd.fixed3IVs;
            shinyLocked = sd.shinyLocked;
            raining = sd.raining;
            defaultDelay = sd.defaultDelay;
            defaultNPC = sd.defaultNPC;

            delayType = sd.delayType;
            noBlink = sd.noBlink;
        }

        public static StationaryData LoadStationaryData(Context context)
        {
            StationaryData stdata = new StationaryData();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            stdata.level = prefs.GetInt(sttLevel, 50);
            stdata.baseStats = Array.ConvertAll(prefs.GetString(sttBase, "100,100,100,100,100,100").Split(','), s => int.Parse(s)).ToArray();
            //if (stdata.baseStats.Length != 6) {stdata.}

            stdata.genderType = prefs.GetInt(sttGender, (int)GenderConversion.GenderType.Genderless );
            stdata.synchronizer = prefs.GetBoolean(sttSync, false);

            stdata.synchronizeNature = prefs.GetInt(sttSyncNature, 0);
            stdata.alwaysSync = prefs.GetBoolean(sttSyncAlways, false);

            stdata.abilityLocked = prefs.GetBoolean(sttAbilityLock, false);
            stdata.ability = prefs.GetInt(sttAbility, 1);

            stdata.fixed3IVs = prefs.GetBoolean(sttFixed3IV, false);
            stdata.shinyLocked = prefs.GetBoolean(sttShinyLock, false);
            stdata.raining = prefs.GetBoolean(sttRaining, false);

            stdata.defaultDelay = prefs.GetInt(sttDelay, 0);
            stdata.defaultNPC = prefs.GetInt(sttNPC, 0 );

            stdata.delayType = prefs.GetInt(sttDelayType, 0);
            stdata.noBlink = prefs.GetBoolean(sttNoBlink, false);
            
            return stdata;
        }

        public static void SaveStationaryData(Context context, StationaryData data)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            ISharedPreferencesEditor prefsEdit = prefs.Edit();

            prefsEdit.PutInt(sttLevel, data.level);
            if (data.baseStats.Length == 6)
            {
                prefsEdit.PutString(sttBase, string.Join(",", data.baseStats));
            }

            prefsEdit.PutInt(sttGender, data.genderType);
            prefsEdit.PutBoolean(sttSync, data.synchronizer);
            prefsEdit.PutInt(sttSyncNature, data.synchronizeNature);
            prefsEdit.PutBoolean(sttSyncAlways, data.alwaysSync);

            prefsEdit.PutBoolean(sttAbilityLock, data.abilityLocked);
            prefsEdit.PutInt(sttAbility, data.ability);

            prefsEdit.PutBoolean(sttFixed3IV, data.fixed3IVs);
            prefsEdit.PutBoolean(sttShinyLock, data.shinyLocked);
            prefsEdit.PutBoolean(sttRaining, data.raining);

            prefsEdit.PutInt(sttDelay, data.defaultDelay);
            prefsEdit.PutInt(sttNPC, data.defaultNPC);

            prefsEdit.PutInt(sttDelayType, data.delayType);
            prefsEdit.PutBoolean(sttNoBlink, data.noBlink);

            prefsEdit.Commit();
        }
    }
}