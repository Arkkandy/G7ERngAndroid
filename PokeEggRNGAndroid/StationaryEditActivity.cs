using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    [Activity(Label = "Edit Stationary Data")]
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

        CheckBox synchronizerCB;
        Spinner syncNatureSpinner;
        CheckBox alwaysSyncCB;

        CheckBox abilityLockCB;
        Spinner abilitySpinner;

        CheckBox fixed3IVCB;
        CheckBox shinyLockCB;
        CheckBox rainingCB;

        EditText recDelayET;
        EditText recNPCET;

        // Hidden parameters
        EditText delayTypeET;
        CheckBox noBlinkCB;

        int[] natureIndices;
        int[] reverseNatureIndices;

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

            synchronizerCB = FindViewById<CheckBox>(Resource.Id.sttSync);
            syncNatureSpinner = FindViewById<Spinner>(Resource.Id.sttSyncNature);
            alwaysSyncCB = FindViewById<CheckBox>(Resource.Id.sttAlwaysSync);

            abilityLockCB = FindViewById<CheckBox>(Resource.Id.sttAbilityLocked);
            abilitySpinner = FindViewById<Spinner>(Resource.Id.sttAbility);

            fixed3IVCB = FindViewById<CheckBox>(Resource.Id.sttFixed3IV);
            shinyLockCB = FindViewById<CheckBox>(Resource.Id.sttShinyLock);
            rainingCB = FindViewById<CheckBox>(Resource.Id.sttRaining);

            recDelayET = FindViewById<EditText>(Resource.Id.sttDelay);
            recNPCET = FindViewById<EditText>(Resource.Id.sttNPC);

            delayTypeET = FindViewById<EditText>(Resource.Id.sttDelayType);
            noBlinkCB = FindViewById<CheckBox>(Resource.Id.sttNoBlink);

            // Setup widgets

            categorySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { "Custom" });
            subPokeSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { "Custom" });

            abilitySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.abilitySymbols[1], PokeRNGApp.Strings.abilitySymbols[2], PokeRNGApp.Strings.abilitySymbols[3] });

            // Gender - In XML

            // Sync Nature
            string[] natures = Resources.GetStringArray(Resource.Array.NatureIndexed);
            natureIndices = ArrayUtil.SortArrayIndex(natures);
            reverseNatureIndices = ArrayUtil.ReverseIndices(natureIndices);
            syncNatureSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, natures );

            // Load data
            LoadStationaryData();
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveStationaryData();
        }

        private void LoadStationaryData() {
            StationaryData data = StationaryData.LoadStationaryData(this);

            categorySpinner.SetSelection(0);
            subPokeSpinner.SetSelection(0);

            levelET.Text = data.level.ToString();

            bsHPET.Text = data.baseStats[0].ToString();
            bsAtkET.Text = data.baseStats[1].ToString();
            bsDefET.Text = data.baseStats[2].ToString();
            bsSpAET.Text = data.baseStats[3].ToString();
            bsSpDET.Text = data.baseStats[4].ToString();
            bsSpeET.Text = data.baseStats[5].ToString();

            genderRatioSpinner.SetSelection(data.genderType);

            synchronizerCB.Checked = data.synchronizer;
            syncNatureSpinner.SetSelection(reverseNatureIndices[data.synchronizeNature]);
            alwaysSyncCB.Checked = data.alwaysSync;

            abilityLockCB.Checked = data.abilityLocked;
            abilitySpinner.SetSelection(data.ability - 1);

            fixed3IVCB.Checked = data.fixed3IVs;
            shinyLockCB.Checked = data.shinyLocked;
            rainingCB.Checked = data.raining;

            recDelayET.Text = data.defaultDelay.ToString();
            recNPCET.Text = data.defaultNPC.ToString();

            delayTypeET.Text = data.delayType.ToString();
            noBlinkCB.Checked = data.noBlink;
        }

        private void SaveStationaryData()
        {
            StationaryData data = new StationaryData();

            //data.category
            //data.subPoke

            data.level = int.Parse(levelET.Text);

            data.baseStats[0] = int.Parse(bsHPET.Text);
            data.baseStats[1] = int.Parse(bsAtkET.Text);
            data.baseStats[2] = int.Parse(bsDefET.Text);
            data.baseStats[3] = int.Parse(bsSpAET.Text);
            data.baseStats[4] = int.Parse(bsSpDET.Text);
            data.baseStats[5] = int.Parse(bsSpeET.Text);

            data.genderType = genderRatioSpinner.SelectedItemPosition;

            data.synchronizer = synchronizerCB.Checked;
            data.synchronizeNature = natureIndices[syncNatureSpinner.SelectedItemPosition];
            data.alwaysSync = alwaysSyncCB.Checked;

            data.abilityLocked = abilityLockCB.Checked;
            data.ability = abilitySpinner.SelectedItemPosition+1;

            data.fixed3IVs = fixed3IVCB.Checked;
            data.shinyLocked = shinyLockCB.Checked;
            data.raining = rainingCB.Checked;

            data.defaultDelay = int.Parse(recDelayET.Text);
            data.defaultNPC= int.Parse(recNPCET.Text);

            data.delayType = int.Parse(delayTypeET.Text);
            data.noBlink = noBlinkCB.Checked;

            StationaryData.SaveStationaryData(this, data);
        }
    }
}