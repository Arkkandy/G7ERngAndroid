using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    [Activity(Label = "Edit Stationary Data",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class StationaryEditActivity : Activity
    {
        Spinner categorySpinner;
        Spinner subPokeSpinner;

        EditText levelET;

        EditText bsHPET;
        EditText bsAtkET;
        EditText bsDefET;
        EditText bsSpAET;
        EditText bsSpDET;
        EditText bsSpeET;

        Spinner genderRatioSpinner;

        CheckBox syncableCB;
        CheckBox synchronizerCB;
        Spinner syncNatureSpinner;
        CheckBox alwaysSyncCB;

        CheckBox abilityLockCB;
        Spinner abilitySpinner;

        CheckBox fixed3IVCB;
        CheckBox shinyLockCB;
        CheckBox isForcedShinyCB;
        CheckBox rainingCB;

        EditText recDelayET;
        EditText recNPCET;

        // Hidden parameters
        EditText delayTypeET;
        CheckBox noBlinkCB;

        int[] natureIndices;
        int[] reverseNatureIndices;

        Pk3DSRNGTool.Pokemon.PokemonList[] speciesList;
        Pk3DSRNGTool.GameVersion realVersion;
        GameVersionUI gversion;

        List<int> realSubIndices;

        bool initA = true;
        bool initB = true;

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Down)
            {
                View v = CurrentFocus;
                if (v is EditText)
                {
                    Rect outRect = new Rect();
                    v.GetGlobalVisibleRect(outRect);
                    if (!outRect.Contains((int)ev.RawX, (int)ev.RawY))
                    {
                        v.ClearFocus();
                        InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                        imm.HideSoftInputFromWindow(v.WindowToken, 0);
                    }
                }
            }
            return base.DispatchTouchEvent(ev);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PokemonDetailsLayout);

            // Get game version
            gversion = (GameVersionUI)Intent.GetIntExtra("GameVersion", 0);
            if (GameVersionConversion.IsUltra(gversion))
            {
                speciesList = Pk3DSRNGTool.PKM7.Species_USUM;
                realVersion = gversion == GameVersionUI.UltraSun ? Pk3DSRNGTool.GameVersion.US : Pk3DSRNGTool.GameVersion.UM;
            }
            else {
                speciesList = Pk3DSRNGTool.PKM7.Species_SM;
                realVersion = gversion == GameVersionUI.Sun ? Pk3DSRNGTool.GameVersion.SN : Pk3DSRNGTool.GameVersion.MN;
            }

            // Fetch widgets
            categorySpinner = FindViewById<Spinner>(Resource.Id.sttCategory);
            subPokeSpinner = FindViewById<Spinner>(Resource.Id.sttSubPokemon);

            levelET = FindViewById<EditText>(Resource.Id.sttLevel);

            bsHPET  = FindViewById<EditText>(Resource.Id.sttHP);
            bsAtkET = FindViewById<EditText>(Resource.Id.sttAtk);
            bsDefET = FindViewById<EditText>(Resource.Id.sttDef);
            bsSpAET = FindViewById<EditText>(Resource.Id.sttSpA);
            bsSpDET = FindViewById<EditText>(Resource.Id.sttSpD);
            bsSpeET = FindViewById<EditText>(Resource.Id.sttSpe);

            genderRatioSpinner = FindViewById<Spinner>(Resource.Id.sttGender);

            syncableCB = FindViewById<CheckBox>(Resource.Id.sttSyncable);
            synchronizerCB = FindViewById<CheckBox>(Resource.Id.sttSync);
            syncNatureSpinner = FindViewById<Spinner>(Resource.Id.sttSyncNature);
            alwaysSyncCB = FindViewById<CheckBox>(Resource.Id.sttAlwaysSync);

            abilityLockCB = FindViewById<CheckBox>(Resource.Id.sttAbilityLocked);
            abilitySpinner = FindViewById<Spinner>(Resource.Id.sttAbility);

            fixed3IVCB = FindViewById<CheckBox>(Resource.Id.sttFixed3IV);
            shinyLockCB = FindViewById<CheckBox>(Resource.Id.sttShinyLock);
            isForcedShinyCB = FindViewById<CheckBox>(Resource.Id.sttForceShiny);
            rainingCB = FindViewById<CheckBox>(Resource.Id.sttRaining);

            recDelayET = FindViewById<EditText>(Resource.Id.sttDelay);
            recNPCET = FindViewById<EditText>(Resource.Id.sttNPC);

            delayTypeET = FindViewById<EditText>(Resource.Id.sttDelayType);
            noBlinkCB = FindViewById<CheckBox>(Resource.Id.sttNoBlink);

            // ------- Setup widgets

            // Load Category Spinner
            string[] majorArray = Array.ConvertAll(speciesList, x => x.Text);
            categorySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                majorArray);
            // Load Sub (Pokemon) Spinner
            LoadSubSpinner(0);

            abilitySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.abilitySymbols[1], PokeRNGApp.Strings.abilitySymbols[2], PokeRNGApp.Strings.abilitySymbols[3] });

            syncableCB.CheckedChange += (sender, args) =>
            {
                if (args.IsChecked == true)
                {
                    synchronizerCB.Checked = false;
                    synchronizerCB.Enabled = true;
                    syncNatureSpinner.Enabled = true;
                }
                else {
                    synchronizerCB.Enabled = false;
                    syncNatureSpinner.Enabled = false;
                }
            };

            shinyLockCB.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    isForcedShinyCB.Enabled = true;
                    isForcedShinyCB.Visibility = ViewStates.Visible;
                }
                else {
                    isForcedShinyCB.Enabled = false;
                    isForcedShinyCB.Visibility = ViewStates.Invisible;
                }
            };

            // Gender - In XML

            // Sync Nature
            string[] natures = Resources.GetStringArray(Resource.Array.NatureIndexed);
            natureIndices = ArrayUtil.SortArrayIndex(natures);
            reverseNatureIndices = ArrayUtil.ReverseIndices(natureIndices);
            syncNatureSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, natures );

            // Create spinner handlers ONLY after setting them up so as to not overwrite data accidentally
            categorySpinner.ItemSelected += (sender, args) => {
                if ( initA ) { initA = false; }
                else {
                    int listIndex = args.Position;
                    LoadSubSpinner(listIndex);
                    subPokeSpinner.SetSelection(0);
                }
            };
            subPokeSpinner.ItemSelected += (sender, args) => {
                if (initB) { initB = false; }
                else
                {
                    int listIndex = categorySpinner.SelectedItemPosition;
                    int pokeIndex = realSubIndices[args.Position];
                    SetupPokemonData(speciesList[listIndex].List[pokeIndex]);
                }
            };

            // Load data
            LoadStationaryData();

            //subPokeSpinner.SetSelection(0);

            // #DEBUG = Always update stationary data on return!
            PrepareReturnIntent();
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveStationaryData();
            //SaveSelection();
        }

        private void WriteDataToInterface(StationaryData data) {
            levelET.Text = data.level.ToString();

            bsHPET.Text = data.baseStats[0].ToString();
            bsAtkET.Text = data.baseStats[1].ToString();
            bsDefET.Text = data.baseStats[2].ToString();
            bsSpAET.Text = data.baseStats[3].ToString();
            bsSpDET.Text = data.baseStats[4].ToString();
            bsSpeET.Text = data.baseStats[5].ToString();

            genderRatioSpinner.SetSelection(data.genderType);

            syncableCB.Checked = data.syncable;
            if (data.syncable)
            {
                synchronizerCB.Checked = data.synchronizer;
                alwaysSyncCB.Checked = data.alwaysSync;
                synchronizerCB.Enabled = true;
                syncNatureSpinner.Enabled = true;
            }
            else {
                alwaysSyncCB.Checked = false;
                synchronizerCB.Enabled = false;
                synchronizerCB.Checked = false;
                syncNatureSpinner.Enabled = true;
            }
            syncNatureSpinner.SetSelection(reverseNatureIndices[data.synchronizeNature]);

            abilityLockCB.Checked = data.abilityLocked;
            abilitySpinner.SetSelection(data.ability - 1);

            fixed3IVCB.Checked = data.fixed3IVs;
            shinyLockCB.Checked = data.shinyLocked;
            isForcedShinyCB.Checked = data.isForcedShiny;
            rainingCB.Checked = data.raining;

            recDelayET.Text = data.defaultDelay.ToString();
            recNPCET.Text = data.defaultNPC.ToString();

            delayTypeET.Text = data.delayType.ToString();
            noBlinkCB.Checked = data.noBlink;
        }

        private void LoadStationaryData() {
            StationaryData data = StationaryData.LoadStationaryData(this);

            LoadSelection(data);

            WriteDataToInterface(data);
        }

        private void LoadSelection(StationaryData data) {

            int mainIndex = data.uiMainIndex;
            int subIndex = data.uiSubIndex;

            if (gversion == (GameVersionUI)data.version)
            {
                // Check if indices are ok
                if (mainIndex < speciesList.Length)
                {
                    if (subIndex >= speciesList[mainIndex].List.Length)
                    {
                        subIndex = 0;
                    }
                }
                else
                {
                    mainIndex = 0;
                    subIndex = 0;
                }
            }
            else {
                mainIndex = 0;
                subIndex = 0;
            }

            LoadSubSpinner(mainIndex);
            categorySpinner.SetSelection(mainIndex);

            FetchSubPokemon(mainIndex,subIndex);
            subPokeSpinner.SetSelection(subIndex);

        }

        /*private void SaveSelection() {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            ISharedPreferencesEditor prefsEdit = prefs.Edit();

            prefsEdit.PutInt("SttActivityMainSpinner", categorySpinner.SelectedItemPosition );
            prefsEdit.PutInt("SttActivitySubSpinner", subPokeSpinner.SelectedItemPosition );

            prefsEdit.Commit();
        }*/

        private void SaveStationaryData()
        {
            StationaryData data = new StationaryData();

            //data.category
            //data.subPoke
            data.version = (int)gversion;
            data.uiMainIndex = categorySpinner.SelectedItemPosition;
            data.uiSubIndex = subPokeSpinner.SelectedItemPosition;
            data.mainIndex = categorySpinner.SelectedItemPosition;
            data.subIndex = realSubIndices[subPokeSpinner.SelectedItemPosition];

            data.level = int.Parse(levelET.Text);

            data.baseStats[0] = int.Parse(bsHPET.Text);
            data.baseStats[1] = int.Parse(bsAtkET.Text);
            data.baseStats[2] = int.Parse(bsDefET.Text);
            data.baseStats[3] = int.Parse(bsSpAET.Text);
            data.baseStats[4] = int.Parse(bsSpDET.Text);
            data.baseStats[5] = int.Parse(bsSpeET.Text);

            data.genderType = genderRatioSpinner.SelectedItemPosition;

            data.syncable = syncableCB.Checked;
            data.synchronizer = synchronizerCB.Checked;
            data.synchronizeNature = natureIndices[syncNatureSpinner.SelectedItemPosition];
            data.alwaysSync = alwaysSyncCB.Checked;

            data.abilityLocked = abilityLockCB.Checked;
            data.ability = abilitySpinner.SelectedItemPosition+1;

            data.fixed3IVs = fixed3IVCB.Checked;
            data.shinyLocked = shinyLockCB.Checked;
            data.isForcedShiny = isForcedShinyCB.Checked;
            data.raining = rainingCB.Checked;

            data.defaultDelay = int.Parse(recDelayET.Text);
            data.defaultNPC= int.Parse(recNPCET.Text);

            data.delayType = int.Parse(delayTypeET.Text);
            data.noBlink = noBlinkCB.Checked;

            StationaryData.SaveStationaryData(this, data);
        }

        private void LoadSubSpinner(int index) {
            int listIndex = index;
            int totalIndices = speciesList[listIndex].List.Length;

            realSubIndices = new List<int>();
            List<string> spinnerStrings = new List<string>();
            for (int i = 0; i < totalIndices; ++i) {
                if (speciesList[listIndex].List[i].Version == Pk3DSRNGTool.GameVersion.Gen7 || speciesList[listIndex].List[i].Version == realVersion) {
                    realSubIndices.Add(i);
                    spinnerStrings.Add(speciesList[listIndex].List[i].ToString());
                }
            }

            /*subPokeSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                Array.ConvertAll(speciesList[listIndex].List, x => x.ToString()));*/
            subPokeSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                spinnerStrings);
            //subPokeSpinner.SetSelection(0);
        }
        private void FetchSubPokemon(int mainIndex, int subIndex) {
            int listIndex = mainIndex;
            int pokeIndex = subIndex;
            SetupPokemonData(speciesList[listIndex].List[pokeIndex]);
        }

        private void SetupPokemonData(Pk3DSRNGTool.Pokemon pkm) {
            var pkm7 = (pkm as Pk3DSRNGTool.PKM7);

            StationaryData data = StationaryLoadHelper.LoadDataFromTemplate(pkm7);

            WriteDataToInterface(data);
        }

        private void PrepareReturnIntent()
        {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("UpdateDelay", true);
            SetResult(Result.Ok, returnIntent);
        }
    }
}