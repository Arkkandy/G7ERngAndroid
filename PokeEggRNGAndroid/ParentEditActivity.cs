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

using Android.Preferences;
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    [Activity(Label = "@string/activity_parents",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ParentEditActivity : Activity
    {
        public class StatCombo {
            public SeekBar statBar;
            public TextView statView;
        }

        private StatCombo[] maleStats;
        private StatCombo[] femaleStats;

        CheckBox maleDitto;
        CheckBox femaleDitto;

        Spinner maleAbilitySp;
        Spinner femaleAbilitySp;
        Spinner genderRatioSp;

        Spinner maleItemSp;
        Spinner femaleItemSp;
        
        CheckBox sameDexBox;
        CheckBox masudaBox;
        CheckBox nidoBox;

        Button buttonDefault;
        Button parentSwap;
        Button templateButton;

        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ParentDataLayout);

            // Fetch widgets
            int[] maleResourceIDs = new int[] {
                Resource.Id.maleHP, Resource.Id.maleHPView, Resource.Id.maleHPText,
                Resource.Id.maleAtk, Resource.Id.maleAtkView, Resource.Id.maleAtkText,
                Resource.Id.maleDef, Resource.Id.maleDefView, Resource.Id.maleDefText,
                Resource.Id.maleSpA, Resource.Id.maleSpAView, Resource.Id.maleSpAText,
                Resource.Id.maleSpD, Resource.Id.maleSpDView, Resource.Id.maleSpDText,
                Resource.Id.maleSpe, Resource.Id.maleSpeView, Resource.Id.maleSpeText };
            int[] femaleResourceIDs = new int[] {
                Resource.Id.femaleHP, Resource.Id.femaleHPView, Resource.Id.femaleHPText,
                Resource.Id.femaleAtk, Resource.Id.femaleAtkView, Resource.Id.femaleAtkText,
                Resource.Id.femaleDef, Resource.Id.femaleDefView, Resource.Id.femaleDefText,
                Resource.Id.femaleSpA, Resource.Id.femaleSpAView, Resource.Id.femaleSpAText,
                Resource.Id.femaleSpD, Resource.Id.femaleSpDView, Resource.Id.femaleSpDText,
                Resource.Id.femaleSpe, Resource.Id.femaleSpeView, Resource.Id.femaleSpeText };

            maleStats = new StatCombo[6];
            femaleStats = new StatCombo[6];

            // Set Male IVs link
            for (int i = 0; i < 6; ++i) {
                maleStats[i] = new StatCombo();
                SeekBar maleStatI = (SeekBar)FindViewById(maleResourceIDs[i * 3 + 0]);
                TextView maleViewI = (TextView)FindViewById(maleResourceIDs[i * 3 + 1]);
                TextView maleTextI = (TextView)FindViewById(maleResourceIDs[i * 3 + 2]);
                maleStatI.ProgressChanged += (sender, args) => {
                    maleViewI.Text = args.Progress.ToString();
                };
                maleViewI.Click += delegate {
                    if (maleStatI.Progress > 0) { maleStatI.Progress--; }
                };
                maleTextI.Click += delegate {
                    if (maleStatI.Progress < maleStatI.Max) { maleStatI.Progress++; }
                };

                maleStats[i].statBar = maleStatI;
                maleStats[i].statView = maleViewI;
            }
            // Set Female IVs link
            for (int i = 0; i < 6; ++i) {
                femaleStats[i] = new StatCombo();
                SeekBar femaleStatI = (SeekBar)FindViewById(femaleResourceIDs[i * 3 + 0]);
                TextView femaleViewI = (TextView)FindViewById(femaleResourceIDs[i * 3 + 1]);
                TextView femaleTextI = (TextView)FindViewById(femaleResourceIDs[i * 3 + 2]);
                femaleStatI.ProgressChanged += (sender, args) => {
                    femaleViewI.Text = args.Progress.ToString();
                };
                femaleViewI.Click += delegate {
                    if (femaleStatI.Progress < femaleStatI.Max) { femaleStatI.Progress++; }
                };
                femaleTextI.Click += delegate {
                    if (femaleStatI.Progress > 0) { femaleStatI.Progress--; }
                };

                femaleStats[i].statBar = femaleStatI;
                femaleStats[i].statView = femaleViewI;
            }

            maleDitto = (CheckBox)FindViewById(Resource.Id.maleIsDitto);
            femaleDitto = (CheckBox)FindViewById(Resource.Id.femaleIsDitto);

            maleAbilitySp = (Spinner)FindViewById(Resource.Id.maleAbility);
            femaleAbilitySp = (Spinner)FindViewById(Resource.Id.femaleAbility);
            genderRatioSp = (Spinner)FindViewById(Resource.Id.genderSpinner);

            maleItemSp = (Spinner)FindViewById(Resource.Id.maleItem);
            femaleItemSp = (Spinner)FindViewById(Resource.Id.femaleItem);

            sameDexBox = (CheckBox)FindViewById(Resource.Id.isSameDex);
            masudaBox = (CheckBox)FindViewById(Resource.Id.isMasuda);
            nidoBox = (CheckBox)FindViewById(Resource.Id.isNido);

            buttonDefault = (Button)FindViewById(Resource.Id.buttonParentDefault);
            parentSwap = (Button)FindViewById(Resource.Id.buttonParentSwap);
            templateButton = (Button)FindViewById(Resource.Id.buttonParentTemplate);

            // Create handlers

            maleDitto.CheckedChange += (sender, args) => {
                if (args.IsChecked) {
                    if (femaleDitto.Checked) { femaleDitto.Checked = false; }
                    if (sameDexBox.Checked) { sameDexBox.Checked = false; }
                }
            };
            femaleDitto.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    if (maleDitto.Checked) { maleDitto.Checked = false; }
                    if (sameDexBox.Checked) { sameDexBox.Checked = false; }
                }
            };

            genderRatioSp.ItemSelected += (sender, args) => {
                VerifyGenderConstraints();
            };

            sameDexBox.CheckedChange += (sender, args) =>
            {
                if (args.IsChecked)
                {
                    if (maleDitto.Checked) maleDitto.Checked = false;
                    if (femaleDitto.Checked) femaleDitto.Checked = false;
                    if (nidoBox.Checked)
                    {
                        nidoBox.Checked = false;
                    }
                }
            };

            nidoBox.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    if (sameDexBox.Checked)
                    {
                        sameDexBox.Checked = false;
                    }
                }
            };

            buttonDefault.Click += delegate {
                for (int i = 0; i < 6; ++i)
                {
                    maleStats[i].statBar.Progress = 31;
                    femaleStats[i].statBar.Progress = 31;
                }
                maleAbilitySp.SetSelection(0);
                femaleAbilitySp.SetSelection(0);

                maleItemSp.SetSelection(0);
                femaleItemSp.SetSelection(0);

                genderRatioSp.SetSelection(0);

                maleDitto.Checked = false;
                femaleDitto.Checked = false;
                sameDexBox.Checked = false;

                nidoBox.Checked = false;
            };
            buttonDefault.LongClick += delegate {
                PopupMenu menu = new PopupMenu(this, parentSwap, Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.parents_default_ability));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.parents_default_item));
                menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.parents_default_ivs));

                menu.MenuItemClick += (sender, args) =>
                {
                    // Reset only abilities
                    if (args.Item.ItemId == 1)
                    {
                        maleAbilitySp.SetSelection(0);
                        femaleAbilitySp.SetSelection(0);
                    }
                    // Reset only items
                    else if (args.Item.ItemId == 2)
                    {
                        maleItemSp.SetSelection(0);
                        femaleItemSp.SetSelection(0);
                    }
                    // Reset only IVs
                    else if (args.Item.ItemId == 3)
                    {
                        for (int i = 0; i < 6; ++i)
                        {
                            maleStats[i].statBar.Progress = 31;
                            femaleStats[i].statBar.Progress = 31;
                        }
                    }
                };
                menu.Show();
            };

            parentSwap.Click += delegate {
                if (IsRandomGender((GenderConversion.GenderType)genderRatioSp.SelectedItemPosition)) {
                    // Swap ditto
                    bool mD = maleDitto.Checked;
                    bool fD = femaleDitto.Checked;

                    maleDitto.Checked = fD;
                    femaleDitto.Checked = mD;
                }

                // Swap Ability
                int sel1 = maleAbilitySp.SelectedItemPosition;
                int sel2 = femaleAbilitySp.SelectedItemPosition;
                maleAbilitySp.SetSelection(sel2);
                femaleAbilitySp.SetSelection(sel1);

                // Swap Item
                int isel1 = maleItemSp.SelectedItemPosition;
                int isel2 = femaleItemSp.SelectedItemPosition;
                maleItemSp.SetSelection(isel2);
                femaleItemSp.SetSelection(isel1);

                // Swap IVs
                for (int i = 0; i < 6; ++i) {
                    int maleP = maleStats[i].statBar.Progress;
                    int femaleP = femaleStats[i].statBar.Progress;

                    maleStats[i].statBar.Progress = femaleP;
                    femaleStats[i].statBar.Progress = maleP;
                }
            };

            parentSwap.LongClick += delegate {
                PopupMenu menu = new PopupMenu(this, parentSwap, Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.parents_swap_ability));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.parents_swap_item));
                menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.parents_swap_ivs));

                menu.MenuItemClick += (sender, args) =>
                {
                    // Swap only abilities
                    if (args.Item.ItemId == 1)
                    {
                        int sel1 = maleAbilitySp.SelectedItemPosition;
                        int sel2 = femaleAbilitySp.SelectedItemPosition;
                        maleAbilitySp.SetSelection(sel2);
                        femaleAbilitySp.SetSelection(sel1);
                    }
                    // Swap only items
                    else if (args.Item.ItemId == 2)
                    {
                        int isel1 = maleItemSp.SelectedItemPosition;
                        int isel2 = femaleItemSp.SelectedItemPosition;
                        maleItemSp.SetSelection(isel2);
                        femaleItemSp.SetSelection(isel1);
                    }
                    // Swap only IVs
                    else if (args.Item.ItemId == 3)
                    {
                        for (int i = 0; i < 6; ++i)
                        {
                            int maleP = maleStats[i].statBar.Progress;
                            int femaleP = femaleStats[i].statBar.Progress;
                            maleStats[i].statBar.Progress = femaleP;
                            femaleStats[i].statBar.Progress = maleP;
                        }
                    }
                };
                menu.Show();
            };


            templateButton.Click += delegate {
                SpawnTemplateDialog();
            };

            templateButton.LongClick += delegate {
                var tpls = ParentIVTemplate.LoadParentTemplates(this);
                if (tpls.Count > 0) {
                    PopupMenu menu = new PopupMenu(this, templateButton, Android.Views.GravityFlags.Center);
                    menu.Menu.Add(Menu.None, 1, 1, PokeRNGApp.Strings.male);
                    menu.Menu.Add(Menu.None, 2, 2, PokeRNGApp.Strings.female);

                    menu.MenuItemClick += (sender, args) =>
                    {
                        int parentgender = args.Item.ItemId - 1;

                        PopupMenu menu2 = new PopupMenu(this, templateButton, Android.Views.GravityFlags.Center);
                        for (int i = 0; i < tpls.Count; ++i) {
                            menu2.Menu.Add( Menu.None, i+1,i+1, tpls[i].name );
                        }

                        menu2.MenuItemClick += (sender2, args2) => {
                            int index = args2.Item.ItemId-1;
                            if (parentgender == 0)
                            {//MALE
                                maleStats[0].statBar.Progress = tpls[index].ivs.hp;
                                maleStats[1].statBar.Progress = tpls[index].ivs.atk;
                                maleStats[2].statBar.Progress = tpls[index].ivs.def;
                                maleStats[3].statBar.Progress = tpls[index].ivs.spa;
                                maleStats[4].statBar.Progress = tpls[index].ivs.spd;
                                maleStats[5].statBar.Progress = tpls[index].ivs.spe;
                            }
                            else { //FEMALE
                                femaleStats[0].statBar.Progress = tpls[index].ivs.hp;
                                femaleStats[1].statBar.Progress = tpls[index].ivs.atk;
                                femaleStats[2].statBar.Progress = tpls[index].ivs.def;
                                femaleStats[3].statBar.Progress = tpls[index].ivs.spa;
                                femaleStats[4].statBar.Progress = tpls[index].ivs.spd;
                                femaleStats[5].statBar.Progress = tpls[index].ivs.spe;
                            }
                        };

                        menu2.Show();
                    };
                    menu.Show();
                }
            };

            LoadParentStats();

            // 
        }

        private void LoadParentStats() {
            // Read parent data
            ParentData pd = ParentData.LoadParentData(this);

            // Assign values to widgets

            // Set Male IVs
            for (int i = 0; i < 6; ++i)
            {
                maleStats[i].statBar.Progress = pd.maleIV[i];
            }
            // Set Female IVs
            for (int i = 0; i < 6; ++i)
            {
                femaleStats[i].statBar.Progress = pd.femaleIV[i];
            }

            if (pd.whoIsDitto == 0)
            {
                maleDitto.Checked = femaleDitto.Checked = false;
            }
            else if (pd.whoIsDitto == 1) {
                maleDitto.Checked = true;
                femaleDitto.Checked = false;
            }
            else if (pd.whoIsDitto == 2)
            {
                maleDitto.Checked = false;
                femaleDitto.Checked = true;
            }

            maleAbilitySp.SetSelection(pd.maleAbility);
            femaleAbilitySp.SetSelection(pd.femaleAbility);

            maleItemSp.SetSelection(pd.maleItem);
            femaleItemSp.SetSelection(pd.femaleItem);

            genderRatioSp.SetSelection(pd.genderCode);

            sameDexBox.Checked = pd.isSameDex;
            masudaBox.Checked = pd.isMasuda;
            nidoBox.Checked = pd.isNidoSpecies;
        }

        private void SaveParentData() {
            ParentData saveData = new ParentData();

            saveData.maleIV = new int[6];
            saveData.femaleIV = new int[6];

            // Set Male IVs
            for (int i = 0; i < 6; ++i)
            {
                saveData.maleIV[i] = maleStats[i].statBar.Progress;
            }
            // Set Female IVs
            for (int i = 0; i < 6; ++i)
            {
                saveData.femaleIV[i] = femaleStats[i].statBar.Progress;
            }

            saveData.genderCode = genderRatioSp.SelectedItemPosition;

            // WhoIsDitto: 0 = None, 1 = Male, 2 = Female
            saveData.whoIsDitto = 0 + (maleDitto.Checked ? 1 : 0) + (femaleDitto.Checked ? 2 : 0);

            saveData.maleAbility = maleAbilitySp.SelectedItemPosition;
            saveData.femaleAbility = femaleAbilitySp.SelectedItemPosition;

            saveData.maleItem = maleItemSp.SelectedItemPosition;
            saveData.femaleItem = femaleItemSp.SelectedItemPosition;

            saveData.isMasuda = masudaBox.Checked;
            saveData.isSameDex = sameDexBox.Checked;
            saveData.isNidoSpecies = nidoBox.Checked;

            ParentData.SaveParentData(this, saveData);
        }

        private void VerifyGenderConstraints() {
            GenderConversion.GenderType genderRatio = (GenderConversion.GenderType)genderRatioSp.SelectedItemPosition;

            if (genderRatio == GenderConversion.GenderType.SameRatio)
            {
                nidoBox.Enabled = true;
                sameDexBox.Enabled = true;
                maleDitto.Enabled = true;
                femaleDitto.Enabled = true;
            }
            else if (genderRatio == GenderConversion.GenderType.Genderless)
            {
                nidoBox.Enabled = false;
                nidoBox.Checked = false;

                sameDexBox.Enabled = false;
                sameDexBox.Checked = false;

                maleDitto.Enabled = false;
                maleDitto.Checked = false;

                femaleDitto.Enabled = false;
                femaleDitto.Checked = true;
            }
            else if (genderRatio == GenderConversion.GenderType.MOnly)
            {
                nidoBox.Enabled = false;
                nidoBox.Checked = false;

                sameDexBox.Enabled = false;
                sameDexBox.Checked = false;

                maleDitto.Enabled = false;
                maleDitto.Checked = false;

                femaleDitto.Enabled = false;
                femaleDitto.Checked = true;
            }
            else if (genderRatio == GenderConversion.GenderType.FOnly)
            {
                nidoBox.Enabled = false;
                nidoBox.Checked = false;

                sameDexBox.Enabled = false;
                sameDexBox.Checked = false;

                maleDitto.Enabled = true;

                femaleDitto.Enabled = false;
                femaleDitto.Checked = false;
            }
            else /*variable ratios*/ {
                nidoBox.Enabled = false;
                sameDexBox.Enabled = true;
                maleDitto.Enabled = true;
                femaleDitto.Enabled = true;
            }
        }

        private bool IsRandomGender(GenderConversion.GenderType gt) {
            return Pk3DSRNGTool.FuncUtil.getGenderRatio(GenderConversion.ConvertGenderIndexToByte((int)gt)) > 0x0F;
        }

        protected override void OnPause() {
            base.OnPause();

            SaveParentData();
        }

        private void SpawnTemplateDialog() {
            ParentIVDialog dialog = new ParentIVDialog(this, maleStats, femaleStats);

            dialog.InitializeDialog();

            dialog.Show();
        }
    }
}