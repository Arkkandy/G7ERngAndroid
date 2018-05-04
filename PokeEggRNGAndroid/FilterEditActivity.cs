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

            Button setIV031Button = (Button)FindViewById(Resource.Id.filterIVResetButton);
            Button setIV3031Button = (Button)FindViewById(Resource.Id.filterIV3031);
            Button setIV0Button = (Button)FindViewById(Resource.Id.filterIV0);
            Button setIVTemplateButton = (Button)FindViewById(Resource.Id.filterIVTemplate);

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
                    if (args.Item.ItemId == 1) { filterStats[0].statMin.Progress = filterStats[0].statMax.Progress = 0; }
                    else if (args.Item.ItemId == 2) { filterStats[1].statMin.Progress = filterStats[1].statMax.Progress = 0; }
                    else if (args.Item.ItemId == 3) { filterStats[2].statMin.Progress = filterStats[2].statMax.Progress = 0; }
                    else if (args.Item.ItemId == 4) { filterStats[3].statMin.Progress = filterStats[3].statMax.Progress = 0; }
                    else if (args.Item.ItemId == 5) { filterStats[4].statMin.Progress = filterStats[4].statMax.Progress = 0; }
                    else if (args.Item.ItemId == 6) { filterStats[5].statMin.Progress = filterStats[5].statMax.Progress = 0; }
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
            Spinner ballSpinner = (Spinner)FindViewById(Resource.Id.filterBall);
            ballSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.male, PokeRNGApp.Strings.female });

            Spinner genderSpinner = (Spinner)FindViewById(Resource.Id.filterGender);
            genderSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.male, PokeRNGApp.Strings.female });

            Spinner abilitySpinner = (Spinner)FindViewById(Resource.Id.filterAbility);
            abilitySpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { PokeRNGApp.Strings.option_any, PokeRNGApp.Strings.abilitySymbols[1], PokeRNGApp.Strings.abilitySymbols[2], PokeRNGApp.Strings.abilitySymbols[3] });

            Spinner hiddenPowerSpinner = (Spinner)FindViewById(Resource.Id.filterHiddenPower);
            sccsHiddenPower = new Widgets.SpinnerCheckboxContainer[16];
            for (int i = 0; i < 16; ++i) {
                sccsHiddenPower[i] = new Widgets.SpinnerCheckboxContainer();
                sccsHiddenPower[i].SetTitle(hps[i]);
                sccsHiddenPower[i].SetSelected(false);
            }
            hiddenPowerSpinner.Adapter = new Widgets.SpinnerCheckboxAdapter(this, 0, sccsHiddenPower, PokeRNGApp.Strings.option_any);

            Spinner natureSpinner = (Spinner)FindViewById(Resource.Id.filterNature);

            sccsNature = new Widgets.SpinnerCheckboxContainer[25];
            for (int i = 0; i < 25; ++i)
            {
                sccsNature[i] = new Widgets.SpinnerCheckboxContainer();
                sccsNature[i].SetTitle(natures[i]);
                sccsNature[i].SetSelected(false);
            }
            natureSpinner.Adapter = new Widgets.SpinnerCheckboxAdapter(this, 0, sccsNature, PokeRNGApp.Strings.option_any);

            Spinner nPerfectSpinner = (Spinner)FindViewById(Resource.Id.filterPerfectIV);
            nPerfectSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new string[] { PokeRNGApp.Strings.option_any, "1", "2", "3", "4", "5", "6" });

            CheckBox shinyOnly = (CheckBox)FindViewById(Resource.Id.filterShinyOnly);
            CheckBox shinyRemind = (CheckBox)FindViewById(Resource.Id.filterShinyRemind);

            Button clearFilterButton = (Button)FindViewById(Resource.Id.filterClear);
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
                shinyRemind.Checked = false;
                nPerfectSpinner.SetSelection(0);
            };

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

            /*SeekBar hpBarMin = (SeekBar)FindViewById(Resource.Id.filterHPMin);
            SeekBar hpBarMax = (SeekBar)FindViewById(Resource.Id.filterHPMax);

            SeekBar atkBarMin = (SeekBar)FindViewById(Resource.Id.filterAtkMin);
            SeekBar atkBarMax = (SeekBar)FindViewById(Resource.Id.filterAtkMax);

            SeekBar defBarMin = (SeekBar)FindViewById(Resource.Id.filterDefMin);
            SeekBar defBarMax = (SeekBar)FindViewById(Resource.Id.filterDefMax);

            SeekBar spaBarMin = (SeekBar)FindViewById(Resource.Id.filterSpAMin);
            SeekBar spaBarMax = (SeekBar)FindViewById(Resource.Id.filterSpAMax);

            SeekBar spdBarMin = (SeekBar)FindViewById(Resource.Id.filterSpDMin);
            SeekBar spdBarMax = (SeekBar)FindViewById(Resource.Id.filterSpDMax);

            SeekBar speBarMin = (SeekBar)FindViewById(Resource.Id.filterSpeMin);
            SeekBar speBarMax = (SeekBar)FindViewById(Resource.Id.filterSpeMax);*/

            fdata.ivMin = new int[6] { filterStats[0].statMin.Progress, filterStats[1].statMin.Progress, filterStats[2].statMin.Progress,
                filterStats[3].statMin.Progress, filterStats[4].statMin.Progress, filterStats[5].statMin.Progress, };
            fdata.ivMax = new int[6] { filterStats[0].statMax.Progress, filterStats[1].statMax.Progress, filterStats[2].statMax.Progress,
                filterStats[3].statMax.Progress, filterStats[4].statMax.Progress, filterStats[5].statMax.Progress, };

            Spinner ballSpinner = (Spinner)FindViewById(Resource.Id.filterBall);
            fdata.ball = ballSpinner.SelectedItemPosition;
            Spinner genderSpinner = (Spinner)FindViewById(Resource.Id.filterGender);
            fdata.gender = genderSpinner.SelectedItemPosition;
            Spinner abilitySpinner = (Spinner)FindViewById(Resource.Id.filterAbility);
            fdata.ability = abilitySpinner.SelectedItemPosition;

            Spinner hiddenPowerSpinner = (Spinner)FindViewById(Resource.Id.filterHiddenPower);
            var listHP = (hiddenPowerSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).GetSelectionValues();
            for (int i = 0; i < listHP.Count; ++i) {
                // Perform index conversion from alphabetical order to in-game order
                fdata.hiddenPowers[hpIndices[listHP[i]]] = true;
            }

            Spinner natureSpinner = (Spinner)FindViewById(Resource.Id.filterNature);
            var listNa = (natureSpinner.Adapter as Widgets.SpinnerCheckboxAdapter).GetSelectionValues();
            for (int i = 0; i < listNa.Count; ++i)
            {
                // Perform index conversion from alphabetical order to in-game order
                fdata.natures[natureIndices[listNa[i]]] = true;
            }

            Spinner nPerfectSpinner = (Spinner)FindViewById(Resource.Id.filterPerfectIV);
            fdata.nPerfects = nPerfectSpinner.SelectedItemPosition;

            CheckBox shinyOnly = (CheckBox)FindViewById(Resource.Id.filterShinyOnly);
            fdata.shinyOnly = shinyOnly.Checked;
            CheckBox shinyRemind = (CheckBox)FindViewById(Resource.Id.filterShinyRemind);
            fdata.shinyRemind = shinyRemind.Checked;

            EggRM.FilterData.SaveFilterData(this, fdata);
        }

        private void LoadFilterData()
        {
            EggRM.FilterData fdata = EggRM.FilterData.LoadFilterData(this);

            for (int i = 0; i < 6; ++i) {
                filterStats[i].statMin.Progress = fdata.ivMin[i];
                filterStats[i].statMax.Progress = fdata.ivMax[i];
            }

            Spinner ballSpinner = (Spinner)FindViewById(Resource.Id.filterBall);
            ballSpinner.SetSelection(fdata.ball);
            Spinner genderSpinner = (Spinner)FindViewById(Resource.Id.filterGender);
            genderSpinner.SetSelection(fdata.gender);
            Spinner abilitySpinner = (Spinner)FindViewById(Resource.Id.filterAbility);
            abilitySpinner.SetSelection(fdata.ability);

            Spinner hiddenPowerSpinner = (Spinner)FindViewById(Resource.Id.filterHiddenPower);
            for (int i = 0; i < 16; ++i)
            {
                int alphabeticalIndex = hpIndices[i];
                sccsHiddenPower[i].SetSelected(fdata.hiddenPowers[alphabeticalIndex]);
            }
            Spinner natureSpinner = (Spinner)FindViewById(Resource.Id.filterNature);
            for (int i = 0; i < 25; ++i)
            {
                int alphabeticalIndex = natureIndices[i];
                sccsNature[i].SetSelected(fdata.natures[alphabeticalIndex]);
            }

            Spinner nPerfectSpinner = (Spinner)FindViewById(Resource.Id.filterPerfectIV);
            nPerfectSpinner.SetSelection(fdata.nPerfects);

            CheckBox shinyOnly = (CheckBox)FindViewById(Resource.Id.filterShinyOnly);
            shinyOnly.Checked = fdata.shinyOnly;
            CheckBox shinyRemind = (CheckBox)FindViewById(Resource.Id.filterShinyRemind);
            shinyRemind.Checked = fdata.shinyRemind;
        }

        private void SpawnTemplateDialog() {
            FilterIVDialog dialog = new FilterIVDialog(this, filterStats);

            dialog.InitializeDialog();

            dialog.Show();
        }
    }
}
 