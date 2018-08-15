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
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    [Activity(Label = "@string/activity_filter",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class FilterEditActivity : Activity
    {
        public class StatCombo {
            public SeekBar statMin;
            public SeekBar statMax;
            public TextView statView;
            public TextView statText;
        }


        StatCombo[] filterStats;

        int[] hpIndices;// = new int[16] { 5, 15, 14, 11, 0, 8, 1, 10, 6, 3, 13, 2, 12, 4, 7, 9 };
        int[] natureIndices;// = new int[25] { 3, 18, 5, 2, 20, 23, 6, 21, 0, 11, 8, 13, 9, 1, 16, 15, 14, 4, 17, 24, 19, 7, 22, 12, 10 };

        Widgets.SpinnerCheckboxContainer[] sccsHiddenPower = null;
        Widgets.SpinnerCheckboxContainer[] sccsNature = null;



        Button setIV031Button;
        Button setIV3031Button;
        Button setIV0Button;
        Button setIVTemplateButton;

        Spinner ballSpinner;
        Spinner genderSpinner;
        Spinner abilitySpinner;
        Spinner hiddenPowerSpinner;
        Spinner natureSpinner;

        Spinner nPerfectSpinner;
        CheckBox shinyOnly;
        CheckBox shinyRemind;
        CheckBox safeF;
        CheckBox blinkF;

        Button clearFilterButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.FilterEditLayout);

            string[] hps = Resources.GetStringArray(Resource.Array.HiddenPowerIndexed);
            hpIndices = ArrayUtil.SortArrayIndex(hps);

            string[] natures = Resources.GetStringArray(Resource.Array.NatureIndexed);
            natureIndices = ArrayUtil.SortArrayIndex(natures);

            //Toast.MakeText(this, String.Join(", ", hpIndices), ToastLength.Long).Show();

            // Setup widgets
            int[] resourceIDs = {
                Resource.Id.filterHPMin, Resource.Id.filterHPMax, Resource.Id.filterHPView, Resource.Id.filterHPText,
                Resource.Id.filterAtkMin, Resource.Id.filterAtkMax, Resource.Id.filterAtkView, Resource.Id.filterAtkText,
                Resource.Id.filterDefMin, Resource.Id.filterDefMax, Resource.Id.filterDefView, Resource.Id.filterDefText,
                Resource.Id.filterSpAMin, Resource.Id.filterSpAMax, Resource.Id.filterSpAView, Resource.Id.filterSpAText,
                Resource.Id.filterSpDMin, Resource.Id.filterSpDMax, Resource.Id.filterSpDView, Resource.Id.filterSpDText,
                Resource.Id.filterSpeMin, Resource.Id.filterSpeMax, Resource.Id.filterSpeView, Resource.Id.filterSpeText
            };

            filterStats = new StatCombo[6];

            for (int i = 0; i < 6; ++i) {
                filterStats[i] = new StatCombo();
                SeekBar statBarMin = (SeekBar)FindViewById(resourceIDs[i * 4 + 0]);
                SeekBar statBarMax = (SeekBar)FindViewById(resourceIDs[i * 4 + 1]);
                TextView statView = (TextView)FindViewById(resourceIDs[i * 4 + 2]);
                TextView statText = (TextView)FindViewById(resourceIDs[i * 4 + 3]);

                statBarMin.ProgressChanged += (sender, args) => {
                    if (statBarMax.Progress < statBarMin.Progress)
                    {
                        statBarMax.Progress = statBarMin.Progress;
                    }
                    statView.Text = statBarMin.Progress + " - " + statBarMax.Progress;
                };
                statBarMax.ProgressChanged += (sender, args) => {
                    if (statBarMax.Progress < statBarMin.Progress)
                    {
                        statBarMin.Progress = statBarMax.Progress;
                    }
                    statView.Text = statBarMin.Progress + " - " + statBarMax.Progress;
                };

                statText.Click += delegate {
                    if (statBarMax.Progress > 0)
                    {
                        statBarMax.Progress = statBarMax.Progress - 1;
                    }
                };
                statView.Click += delegate {
                    if (statBarMin.Progress < statBarMin.Max)
                    {
                        statBarMin.Progress = statBarMin.Progress + 1;
                    }
                };

                filterStats[i].statMin = statBarMin;
                filterStats[i].statMax = statBarMax;
                filterStats[i].statView = statView;
                filterStats[i].statText = statText;
            }

            setIV031Button = (Button)FindViewById(Resource.Id.filterIVResetButton);
            setIV3031Button = (Button)FindViewById(Resource.Id.filterIV3031);
            setIV0Button = (Button)FindViewById(Resource.Id.filterIV0);
            setIVTemplateButton = (Button)FindViewById(Resource.Id.filterIVTemplate);

            setIV031Button.Click += delegate {
                for (int i = 0; i < 6; ++i) {
                    filterStats[i].statMin.Progress = 0;
                    filterStats[i].statMax.Progress = 31;
                }
            };
            setIV3031Button.Click += delegate {
                for (int i = 0; i < 6; ++i)
                {
                    filterStats[i].statMax.Progress = 31;
                    filterStats[i].statMin.Progress = 30;
                }
            };
            setIV0Button.Click += delegate {
                PopupMenu menu = new PopupMenu(this, setIV0Button, Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, "0 HP");
                menu.Menu.Add(Menu.None, 2, 2, "0 Atk");
                menu.Menu.Add(Menu.None, 3, 3, "0 Def");
                menu.Menu.Add(Menu.None, 4, 4, "0 SpA");
                menu.Menu.Add(Menu.None, 5, 5, "0 SpD");
                menu.Menu.Add(Menu.None, 6, 6, "0 Spe");

                menu.MenuItemClick += (sender, args) =>
                {
                    // Set only select IVs to 0
                    filterStats[args.Item.ItemId - 1].statMin.Progress = filterStats[args.Item.ItemId - 1].statMax.Progress = 0;
                };
                menu.Show();
            };
            setIVTemplateButton.Click += delegate {
                SpawnTemplateDialog();
            };
            setIVTemplateButton.LongClick += delegate {
                var filterTemplates = FilterIVTemplate.LoadFilterTemplates(this);

                if (filterTemplates.Count > 0) {

                    PopupMenu menu = new PopupMenu(this, setIVTemplateButton, Android.Views.GravityFlags.Center);

                    for (int i = 0; i < filterTemplates.Count; ++i)
                    {
                        menu.Menu.Add(Menu.None, i+1, i+1, filterTemplates[i].name);
                    }

                    menu.MenuItemClick += (sender, args) =>
                    {
                        ApplyIVs(filterTemplates[args.Item.ItemId-1].ivsmin, filterTemplates[args.Item.ItemId-1].ivsmax);
                    };
                    menu.Show();
                }
            };

            // Spinners
            ballSpinner = (Spinner)FindViewById(Resource.Id.filterBall);
            ballSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.male, PokeRNGApp.Strings.female });

            genderSpinner = (Spinner)FindViewById(Resource.Id.filterGender);
            genderSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.male, PokeRNGApp.Strings.female });

            abilitySpinner = (Spinner)FindViewById(Resource.Id.filterAbility);
            abilitySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.abilitySymbols[1], PokeRNGApp.Strings.abilitySymbols[2], PokeRNGApp.Strings.abilitySymbols[3] });

            hiddenPowerSpinner = (Spinner)FindViewById(Resource.Id.filterHiddenPower);
            sccsHiddenPower = new Widgets.SpinnerCheckboxContainer[16];
            for (int i = 0; i < 16; ++i) {
                sccsHiddenPower[i] = new Widgets.SpinnerCheckboxContainer();
                sccsHiddenPower[i].SetTitle(hps[i]);
                sccsHiddenPower[i].SetSelected(false);
            }
            hiddenPowerSpinner.Adapter = new Widgets.SpinnerCheckboxAdapter(this, 0, sccsHiddenPower, PokeRNGApp.Strings.option_any);

            natureSpinner = (Spinner)FindViewById(Resource.Id.filterNature);

            sccsNature = new Widgets.SpinnerCheckboxContainer[25];
            for (int i = 0; i < 25; ++i)
            {
                sccsNature[i] = new Widgets.SpinnerCheckboxContainer();
                sccsNature[i].SetTitle(natures[i]);
                sccsNature[i].SetSelected(false);
            }
            natureSpinner.Adapter = new Widgets.SpinnerCheckboxAdapter(this, 0, sccsNature, PokeRNGApp.Strings.option_any);

            nPerfectSpinner = (Spinner)FindViewById(Resource.Id.filterPerfectIV);
            nPerfectSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new string[] { PokeRNGApp.Strings.option_any, "1", "2", "3", "4", "5", "6" });

            shinyOnly = (CheckBox)FindViewById(Resource.Id.filterShinyOnly);
            shinyRemind = (CheckBox)FindViewById(Resource.Id.filterShinyRemind);
            safeF = (CheckBox)FindViewById(Resource.Id.filterSafeF);
            blinkF = (CheckBox)FindViewById(Resource.Id.filterBlinkF);

            clearFilterButton = (Button)FindViewById(Resource.Id.filterClear);
            clearFilterButton.Click += delegate {
                /*hpBarMin.Progress = atkBarMin.Progress = defBarMin.Progress = spaBarMin.Progress = spdBarMin.Progress = speBarMin.Progress = 0;
                hpBarMax.Progress = atkBarMax.Progress = defBarMax.Progress = spaBarMax.Progress = spdBarMax.Progress = speBarMax.Progress = 31;*/

                ballSpinner.SetSelection(0);
                genderSpinner.SetSelection(0);
                abilitySpinner.SetSelection(0);

                for (int i = 0; i < sccsHiddenPower.Length; ++i)
                {
                    sccsHiddenPower[i].SetSelected(false);
                }
                (hiddenPowerSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).NotifyDataSetChanged();
                for (int i = 0; i < sccsNature.Length; ++i) {
                    sccsNature[i].SetSelected(false);
                }
                (natureSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).NotifyDataSetChanged();

                shinyOnly.Checked = false;
                // Only reset visible UI
                if (shinyRemind.Visibility == ViewStates.Visible) { shinyRemind.Checked = false; }
                if (blinkF.Visibility == ViewStates.Visible) { blinkF.Checked = false; }
                if (safeF.Visibility == ViewStates.Visible) { safeF.Checked = false; }

                nPerfectSpinner.SetSelection(0);
            };

            if (Intent.HasExtra("SearchType"))
            {
                SearchType st = (SearchType) Intent.GetIntExtra("SearchType", 0);
                if (SearchTypeUtil.IsMainRNGSearch(st))
                {
                    shinyRemind.Visibility = ViewStates.Gone;
                    if (Intent.GetIntExtra("NumNPCs",0) > 0)
                    {
                        blinkF.Visibility = ViewStates.Gone;
                        safeF.Visibility = ViewStates.Visible;
                    }
                    else {
                        safeF.Visibility = ViewStates.Gone;
                        blinkF.Visibility = ViewStates.Visible;
                    }
                }
                else {
                    shinyRemind.Visibility = ViewStates.Visible;
                    blinkF.Visibility = ViewStates.Gone;
                    safeF.Visibility = ViewStates.Gone;
                }
            }


            LoadFilterData();
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveFilterData();
        }

        private void ApplyIVs(IVSet min, IVSet max) {
            filterStats[0].statMin.Progress = min.hp; filterStats[0].statMax.Progress = max.hp;
            filterStats[1].statMin.Progress = min.atk; filterStats[1].statMax.Progress = max.atk;
            filterStats[2].statMin.Progress = min.def; filterStats[2].statMax.Progress = max.def;
            filterStats[3].statMin.Progress = min.spa; filterStats[3].statMax.Progress = max.spa;
            filterStats[4].statMin.Progress = min.spd; filterStats[4].statMax.Progress = max.spd;
            filterStats[5].statMin.Progress = min.spe; filterStats[5].statMax.Progress = max.spe;
        }

        private void SaveFilterData() {
            EggRM.FilterData fdata = new EggRM.FilterData();

            fdata.ivMin = new int[6] { filterStats[0].statMin.Progress, filterStats[1].statMin.Progress, filterStats[2].statMin.Progress,
                filterStats[3].statMin.Progress, filterStats[4].statMin.Progress, filterStats[5].statMin.Progress, };
            fdata.ivMax = new int[6] { filterStats[0].statMax.Progress, filterStats[1].statMax.Progress, filterStats[2].statMax.Progress,
                filterStats[3].statMax.Progress, filterStats[4].statMax.Progress, filterStats[5].statMax.Progress, };

            fdata.ball = ballSpinner.SelectedItemPosition;
            fdata.gender = genderSpinner.SelectedItemPosition;
            fdata.ability = abilitySpinner.SelectedItemPosition;

            var listHP = (hiddenPowerSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).GetSelectionValues();
            for (int i = 0; i < listHP.Count; ++i) {
                // Perform index conversion from alphabetical order to in-game order
                fdata.hiddenPowers[hpIndices[listHP[i]]] = true;
            }

            var listNa = (natureSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).GetSelectionValues();
            for (int i = 0; i < listNa.Count; ++i)
            {
                // Perform index conversion from alphabetical order to in-game order
                fdata.natures[natureIndices[listNa[i]]] = true;
            }

            fdata.nPerfects = nPerfectSpinner.SelectedItemPosition;
            
            fdata.shinyOnly = shinyOnly.Checked;
            fdata.shinyRemind = shinyRemind.Checked;
            fdata.blinkFOnly = blinkF.Checked;
            fdata.safeFOnly = safeF.Checked;

            EggRM.FilterData.SaveFilterData(this, fdata);
        }

        private void LoadFilterData()
        {
            EggRM.FilterData fdata = EggRM.FilterData.LoadFilterData(this);

            for (int i = 0; i < 6; ++i) {
                filterStats[i].statMin.Progress = fdata.ivMin[i];
                filterStats[i].statMax.Progress = fdata.ivMax[i];
            }
            
            ballSpinner.SetSelection(fdata.ball);
            genderSpinner.SetSelection(fdata.gender);
            abilitySpinner.SetSelection(fdata.ability);

            for (int i = 0; i < 16; ++i)
            {
                int alphabeticalIndex = hpIndices[i];
                sccsHiddenPower[i].SetSelected(fdata.hiddenPowers[alphabeticalIndex]);
            }
            for (int i = 0; i < 25; ++i)
            {
                int alphabeticalIndex = natureIndices[i];
                sccsNature[i].SetSelected(fdata.natures[alphabeticalIndex]);
            }

            nPerfectSpinner.SetSelection(fdata.nPerfects);
            
            shinyOnly.Checked = fdata.shinyOnly;
            shinyRemind.Checked = fdata.shinyRemind;
            blinkF.Checked = fdata.blinkFOnly;
            safeF.Checked = fdata.safeFOnly;
        }

        private void SpawnTemplateDialog() {
            FilterIVDialog dialog = new FilterIVDialog(this, filterStats);

            dialog.InitializeDialog();

            dialog.Show();
        }
    }
}
 