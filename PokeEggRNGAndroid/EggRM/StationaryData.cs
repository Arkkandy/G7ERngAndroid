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
        // Meta
        public int version;
        public int uiMainIndex;
        public int uiSubIndex;
        public int mainIndex;
        public int subIndex;

        // Actual data
        public int level;
        public int[] baseStats;
        public int genderType;
        public bool syncable;
        public bool synchronizer;
        public int synchronizeNature;
        public bool alwaysSync;
        public bool abilityLocked;
        public int ability;
        public bool fixed3IVs;
        public bool shinyLocked;
        public bool isForcedShiny;
        public bool raining;
        public int defaultDelay;
        public int defaultNPC;

        // Hidden parameters
        public int delayType;
        public bool noBlink;

        public bool BlinkWhenSync => !alwaysSync && !noBlink;

        private static readonly string sttMVersion = "SttMetaVersion";
        private static readonly string sttMUiMainIndex = "SttMetaUIMain";
        private static readonly string sttMUiSubIndex = "SttMetaUISub";
        private static readonly string sttMMainIndex = "SttMetaMain";
        private static readonly string sttMSubIndex = "SttMetaSub";

        private static readonly string sttLevel = "SttPokeLevel";
        private static readonly string sttBase = "SttPokeBase";
        private static readonly string sttGender = "SttPokeGender";
        private static readonly string sttSyncable = "SttPokeSyncable";
        private static readonly string sttSync = "SttPokeSync";
        private static readonly string sttSyncNature = "SttPokeSyncNat";
        private static readonly string sttSyncAlways = "SttPokeSyncAlways";
        private static readonly string sttAbilityLock = "SttPokeAbilityLock";
        private static readonly string sttAbility = "SttPokeAbility";
        private static readonly string sttFixed3IV = "SttPokeFixed3IV";
        private static readonly string sttShinyLock = "SttPokeShinyLock";
        private static readonly string sttForceShiny = "SttPokeForceShiny";
        private static readonly string sttRaining = "SttPokeRaining";
        private static readonly string sttDelay = "SttPokeDelay";
        private static readonly string sttNPC = "SttPokeNPC";

        private static readonly string sttDelayType = "SttDelayType";
        private static readonly string sttNoBlink = "SttNoBlink";

        public StationaryData() {
            baseStats = new int[] { 100, 100, 100, 100, 100, 100 };
        }

        public StationaryData(StationaryData sd) {

            version = sd.version;
            uiMainIndex = sd.uiMainIndex;
            uiSubIndex = sd.uiSubIndex;
            mainIndex = sd.mainIndex;
            subIndex = sd.subIndex;

            level = sd.level;
            baseStats = (int[])sd.baseStats.Clone();
            genderType = sd.genderType;
            syncable = sd.syncable;
            synchronizer = sd.synchronizer;
            synchronizeNature = sd.synchronizeNature;
            alwaysSync = sd.alwaysSync;
            abilityLocked = sd.abilityLocked;
            ability = sd.ability;
            fixed3IVs = sd.fixed3IVs;
            shinyLocked = sd.shinyLocked;
            isForcedShiny = sd.isForcedShiny;
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

            stdata.version = prefs.GetInt(sttMVersion, 0);
            stdata.uiMainIndex = prefs.GetInt(sttMUiMainIndex, 0);
            stdata.uiSubIndex = prefs.GetInt(sttMUiSubIndex, 0);
            stdata.mainIndex = prefs.GetInt(sttMMainIndex, 0);
            stdata.subIndex = prefs.GetInt(sttMSubIndex, 0);

            stdata.level = prefs.GetInt(sttLevel, 50);
            stdata.baseStats = Array.ConvertAll(prefs.GetString(sttBase, "100,100,100,100,100,100").Split(','), s => int.Parse(s)).ToArray();
            //if (stdata.baseStats.Length != 6) {stdata.}

            stdata.genderType = prefs.GetInt(sttGender, (int)GenderConversion.GenderType.Genderless );
            stdata.synchronizer = prefs.GetBoolean(sttSync, false);

            stdata.syncable = prefs.GetBoolean(sttSyncable, true);
            stdata.alwaysSync = prefs.GetBoolean(sttSyncAlways, false);
            stdata.synchronizeNature = prefs.GetInt(sttSyncNature, 0);
            stdata.alwaysSync = prefs.GetBoolean(sttSyncAlways, false);

            stdata.abilityLocked = prefs.GetBoolean(sttAbilityLock, false);
            stdata.ability = prefs.GetInt(sttAbility, 1);

            stdata.fixed3IVs = prefs.GetBoolean(sttFixed3IV, false);
            stdata.shinyLocked = prefs.GetBoolean(sttShinyLock, false);
            stdata.isForcedShiny = prefs.GetBoolean(sttForceShiny, false);
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

            prefsEdit.PutInt(sttMVersion, data.version);
            prefsEdit.PutInt(sttMUiMainIndex, data.uiMainIndex);
            prefsEdit.PutInt(sttMUiSubIndex, data.uiSubIndex);
            prefsEdit.PutInt(sttMMainIndex, data.mainIndex);
            prefsEdit.PutInt(sttMSubIndex, data.subIndex);

            prefsEdit.PutInt(sttLevel, data.level);
            if (data.baseStats.Length == 6)
            {
                prefsEdit.PutString(sttBase, string.Join(",", data.baseStats));
            }

            prefsEdit.PutInt(sttGender, data.genderType);

            prefsEdit.PutBoolean(sttSyncable, data.syncable);
            prefsEdit.PutBoolean(sttSync, data.synchronizer);
            prefsEdit.PutInt(sttSyncNature, data.synchronizeNature);
            prefsEdit.PutBoolean(sttSyncAlways, data.alwaysSync);

            prefsEdit.PutBoolean(sttAbilityLock, data.abilityLocked);
            prefsEdit.PutInt(sttAbility, data.ability);

            prefsEdit.PutBoolean(sttFixed3IV, data.fixed3IVs);
            prefsEdit.PutBoolean(sttShinyLock, data.shinyLocked);
            prefsEdit.PutBoolean(sttForceShiny, data.isForcedShiny);
            prefsEdit.PutBoolean(sttRaining, data.raining);

            prefsEdit.PutInt(sttDelay, data.defaultDelay);
            prefsEdit.PutInt(sttNPC, data.defaultNPC);

            prefsEdit.PutInt(sttDelayType, data.delayType);
            prefsEdit.PutBoolean(sttNoBlink, data.noBlink);

            prefsEdit.Commit();
        }
    }

    public class StationaryLoadHelper {

        public static StationaryData LoadDataFromTemplate(Pk3DSRNGTool.PKM7 pkm) {

            var info = pkm.info;

            StationaryData data = new StationaryData();

            // Ignore meta data

            data.level = pkm.Level;

            data.baseStats[0] = info.HP;
            data.baseStats[1] = info.ATK;
            data.baseStats[2] = info.DEF;
            data.baseStats[3] = info.SPA;
            data.baseStats[4] = info.SPD;
            data.baseStats[5] = info.SPE;

            data.genderType = (int)GenderConversion.ConvertByteToGenderType(pkm.GenderRatio);

            data.alwaysSync = pkm.AlwaysSync;


            data.ability = pkm.Ability == 0 && !data.alwaysSync ? (byte)1 : (pkm.Ability == 4 ? 3 : pkm.Ability);
            data.abilityLocked = true;
            if (data.ability == 0xFF || data.ability == 0) // Outlier
            {
                data.abilityLocked = false;
                data.ability = 1;
            }
            /*else {
                data.abilityLocked = true;
                if (pkm.Ability == 0 && !pkm.AlwaysSync)
                {
                    data.ability = 1;
                }
                else
                {
                    data.ability = (pkm.Ability == 4 ? 3 : pkm.Ability);
                }
                //data.ability = pkm.Ability == 0 && (!pkm.AlwaysSync && !pkm7.NoBlink) ? (byte)1 : pkm.Ability;
            }*/

            data.defaultDelay = pkm.Delay;
            data.defaultNPC = pkm.NPC;

            data.delayType = pkm.DelayType;
            data.fixed3IVs = pkm.IV3 || pkm.Totem || pkm.UltraWormhole;
            //data.noBlink = pkm7.NoBlink;
            data.raining = pkm.Raining;
            data.shinyLocked = pkm.ShinyLocked;

            /*if (pkm.shinyLocked) {
                data.isForcedShiny = pkm7.shin
            }*/

            // User data, don't overwrite unless unsyncable
            data.syncable = pkm.Syncable;
            if (!pkm.Syncable)
            {
                data.synchronizer = false;
                //data.synchronizeNature = 
            }
            else
            {
                //data.synchronizeNature

            }//data.synchronizer
            if ( pkm.IsPelago ) { data.syncable = false; }

            return data;
        }

        public static StationaryData LoadStationaryData(Context ctx, GameVersionUI gv) {
            StationaryData storedData = StationaryData.LoadStationaryData(ctx);
            if ((int)gv == storedData.version) {
                return storedData;
            }
            StationaryData replacementData = LoadDataFromTemplate(Pk3DSRNGTool.PKM7.Species_USUM[0].List[0] as Pk3DSRNGTool.PKM7);
            //StationaryData.SaveStationaryData(ctx, replacementData); // Don't overwrite until user goes back to Stationary Edit
            return replacementData;
        }
    }
}