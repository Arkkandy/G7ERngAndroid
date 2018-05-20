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
using Pk3DSRNGTool;
using Android.Graphics;
using Android.Views.InputMethods;

namespace Gen7EggRNG
{
    [Activity(Label = "@string/activity_profile",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ProfileEditActivity : Activity
    {
        private const int NoProfileSelected = 0;

        //private bool changesWereMade = false;
        private bool userEditingProfile = false;

        string profileTag;
        int selectedProfileIndex = NoProfileSelected; // 0 = NONE

        //EditText profileTag;
        CheckBox shinyCharmBox;

        EditText tsvField;

        EditText seed3;
        EditText seed2;
        EditText seed1;
        EditText seed0;
        TextView chseed3;
        TextView chseed2;
        TextView chseed1;
        TextView chseed0;
        Button checkToCurrent;
        Button currentToCheck;
        ImageButton seedPlusButton;
        Button findSeedButton;
        EditText iSeed;
        ImageButton clockHandButton;

        Spinner gameVersionSpinner;
        TextView seedFrameView;

        Spinner profileSelectionSpinner;
        ImageButton editProfileButton;
        ImageButton addProfileButton;
        ImageButton deleteProfileButton;

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
            SetContentView(Resource.Layout.ProfileEditLayout);

            // Initialize widgets
            //profileTag = (EditText)FindViewById(Resource.Id.editProfileTag);
            shinyCharmBox = (CheckBox)FindViewById(Resource.Id.hasShinyCharm);

            tsvField = (EditText)FindViewById(Resource.Id.tsvField);

            seed3 = (EditText)FindViewById(Resource.Id.seed3);
            seed2 = (EditText)FindViewById(Resource.Id.seed2);
            seed1 = (EditText)FindViewById(Resource.Id.seed1);
            seed0 = (EditText)FindViewById(Resource.Id.seed0);
            chseed3 = (TextView)FindViewById(Resource.Id.chseed3);
            chseed2 = (TextView)FindViewById(Resource.Id.chseed2);
            chseed1 = (TextView)FindViewById(Resource.Id.chseed1);
            chseed0 = (TextView)FindViewById(Resource.Id.chseed0);
            checkToCurrent = (Button)FindViewById(Resource.Id.buttonSetCurrent);
            currentToCheck = (Button)FindViewById(Resource.Id.buttonSetCheck);
            seedPlusButton = (ImageButton)FindViewById(Resource.Id.profileSeedPlus);
            findSeedButton = (Button)FindViewById(Resource.Id.buttonFindSeed);
            iSeed = (EditText)FindViewById(Resource.Id.initialSeed);
            clockHandButton = (ImageButton)FindViewById(Resource.Id.clockHandButton);

            editProfileButton = (ImageButton)FindViewById(Resource.Id.profileEditButton);
            addProfileButton = (ImageButton)FindViewById(Resource.Id.profileAddButton);
            deleteProfileButton = (ImageButton)FindViewById(Resource.Id.profileDeleteButton);

            profileSelectionSpinner = (Spinner)FindViewById(Resource.Id.profileSelection);

            gameVersionSpinner = (Spinner)FindViewById(Resource.Id.gameVersionSpinner);
            seedFrameView = (TextView)FindViewById(Resource.Id.seedFrame);

            // Set Widget listeners/filters
            tsvField.AddTextChangedListener(new Gen7EggRNG.Util.TSVTextWatcher(tsvField));

            seed3.SetFilters(new Android.Text.IInputFilter[] { new Gen7EggRNG.Util.HexadecimalInputFilter(), new Android.Text.InputFilterLengthFilter(8) });
            seed2.SetFilters(new Android.Text.IInputFilter[] { new Gen7EggRNG.Util.HexadecimalInputFilter(), new Android.Text.InputFilterLengthFilter(8) });
            seed1.SetFilters(new Android.Text.IInputFilter[] { new Gen7EggRNG.Util.HexadecimalInputFilter(), new Android.Text.InputFilterLengthFilter(8) });
            seed0.SetFilters(new Android.Text.IInputFilter[] { new Gen7EggRNG.Util.HexadecimalInputFilter(), new Android.Text.InputFilterLengthFilter(8) });

            iSeed.SetFilters(new Android.Text.IInputFilter[] { new Gen7EggRNG.Util.HexadecimalInputFilter(), new Android.Text.InputFilterLengthFilter(8) });

            // Load Profile Data
            LoadProfile();

            // Initialize interface handlers
            seed3.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };
            seed2.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };
            seed1.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };
            seed0.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };

            iSeed.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };

            /*profileTag.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };*/

            tsvField.FocusChange += (sender, args) =>
            {
                if (!args.HasFocus)
                {
                    SaveProfile();
                }
            };

            shinyCharmBox.CheckedChange += (sender, args) =>
            {
                SaveProfile();
            };


            checkToCurrent.Click += delegate
            {
                SetCurrentSeedFromCheckpoint();
            };


            currentToCheck.Click += delegate
            {
                SetCheckpointSeedFromCurrent();
            };


            seedPlusButton.Click += delegate {
                PopupMenu menu = new PopupMenu(this, seedPlusButton, Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.profile_copycurrent));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.profile_copycheckpoint));
                menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.profile_pastetocurrent));

                menu.MenuItemClick += (sender, args) => {

                    if (args.Item.ItemId == 1)
                    {
                        // Copy current
                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("EggSeed", GetCurrentSeedFromView().GetSeedToString());
                        
                        Toast.MakeText(this, Resources.GetString(Resource.String.profile_copiedcurrent), ToastLength.Short).Show();
                    }
                    else if (args.Item.ItemId == 2) {
                        // Copy checkpoint
                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("EggSeed", GetCheckpointSeedFromView().GetSeedToString());

                        Toast.MakeText(this, Resources.GetString(Resource.String.profile_copiedcheckpoint), ToastLength.Short).Show();
                    }
                    else if (args.Item.ItemId == 3)
                    {
                        // Paste seed to current
                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        if (clipboard.HasPrimaryClip) {
                            string paste = clipboard.PrimaryClip.GetItemAt(0).Text;

                            EggSeed seed = new EggSeed();
                            string[] seedData = paste.Split(',');
                            if (seedData.Length == 4)
                            {
                                try
                                {
                                    uint[] st = new uint[4];
                                    st[3] = Convert.ToUInt32(seedData[0], 16);
                                    st[2] = Convert.ToUInt32(seedData[1], 16);
                                    st[1] = Convert.ToUInt32(seedData[2], 16);
                                    st[0] = Convert.ToUInt32(seedData[3], 16);
                                    seed.SetSeed(st);
                                    SetCurrentSeed(seed);
                                    SaveProfile();
                                    //Toast.MakeText(this, "Current seed replaced.", ToastLength.Short).Show();
                                }
                                catch
                                {
                                    Toast.MakeText(this, Resources.GetString(Resource.String.profile_malformedseed), ToastLength.Short).Show();
                                }
                            }
                            else {
                                Toast.MakeText(this, Resources.GetString(Resource.String.profile_noseed), ToastLength.Short).Show();
                            }
                        }

                    }
                };

                menu.Show();
            };

            findSeedButton.Click += delegate {
                PopupMenu menu = new PopupMenu(this, findSeedButton, Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.profile_magikarp));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.profile_tinyseed));
                menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.profile_seedrecovery));
                menu.Show();


                menu.MenuItemClick += (sender, args) =>
                {

                    if (args.Item.ItemId == 1)
                    {
                        // Hardcoded 0 - "MagikarpRequestCode"
                        var intent = new Intent(this, typeof(MagikarpSeedActivity));
                        StartActivityForResult(intent, 0);
                    }
                    else if (args.Item.ItemId == 2)
                    {
                        var intent = new Intent(this, typeof(TinyFinderActivity));
                        StartActivityForResult(intent, 1);
                    }
                    else if (args.Item.ItemId == 3)
                    {
                        var intent = new Intent(this, typeof(SeedRecoveryActivity));
                        StartActivityForResult(intent, 2);
                    }
                };
            };

            clockHandButton.Click += delegate {
                // Request seed from Clock Hand Activity
                var intent = new Intent(this, typeof(ClockHandActivity));
                StartActivityForResult(intent, 3);
            };

            UpdateSpinner(selectedProfileIndex);
            profileSelectionSpinner.ItemSelected += (sender, args) => {
                if (userEditingProfile)
                {
                    ChangeProfile(args.Position + 1);
                    //Toast.MakeText(this, "Selected " + (args.Position + 1), ToastLength.Short).Show();
                }
                userEditingProfile = true;
            };

            editProfileButton.Click += delegate {
                Dialog editDialog = new Dialog(this);

                editDialog.SetContentView(Resource.Layout.EditProfileDialog);

                editDialog.SetTitle(Resources.GetString(Resource.String.profile_editname));

                EditText profileNameEdit = (EditText)editDialog.FindViewById(Resource.Id.profileTagText);
                Button confirmButton = (Button)editDialog.FindViewById(Resource.Id.profileOkEdit);
                Button cancelButton = (Button)editDialog.FindViewById(Resource.Id.profileNoEdit);

                profileNameEdit.Text = (string)profileSelectionSpinner.SelectedItem;

                confirmButton.Click += delegate {
                    if (profileNameEdit.Text != String.Empty)
                    {
                        UpdateProfileTag(profileNameEdit.Text);

                        UpdateSpinner(selectedProfileIndex);

                        editDialog.Dismiss();
                    }
                    else {
                        Toast.MakeText(this, Resources.GetString(Resource.String.profile_editnameempty), ToastLength.Short).Show();
                    }
                };

                cancelButton.Click += delegate {
                    editDialog.Dismiss();
                };

                editDialog.Show();
            };

            addProfileButton.Click += delegate {
                EnterTextDialog etd = new EnterTextDialog(this);

                etd.SetTitle(Resources.GetString(Resource.String.profile_createprofile));
                etd.SetDefaultText(Resources.GetString(Resource.String.profile_defaultprofile));
                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Resources.GetString(Resource.String.profile_editnameempty));

                etd.InitializeDialog(
                    x =>
                    {
                        AddProfileModel(x);
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
                //Toast.MakeText(this, "New profile created.", ToastLength.Short).Show();
            };

            deleteProfileButton.Click += delegate {
                if (ProfileData.GetNumProfiles(this) > 1)
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    builder.SetTitle(Resources.GetString(Resource.String.profile_delete));
                    builder.SetMessage(Resources.GetString(Resource.String.profile_deletemessage));
                    builder.SetPositiveButton(Resources.GetString(Resource.String.profile_deleteyes),
                        (sender, args) =>
                        {
                            DeleteProfile();
                        }
                        );
                    builder.SetNegativeButton(Resources.GetString(Resource.String.profile_deleteno),
                        (sender, args) =>
                        {

                        }
                        );

                    builder.Create().Show();
                }
            };

            //Gen7EggRNG.AndroidUtil.DebugUtil.DumpSharedPreferences(this);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok && requestCode == 0)
            {
                EggSeed newSeed = new EggSeed();
                string seed = data.GetStringExtra("NewSeed");
                newSeed.SetSeed(FuncUtil.SeedStr2Array(seed));
                SetCurrentSeed(newSeed);
                SaveProfile();
                Toast.MakeText(this, Resources.GetString(Resource.String.profile_magikarp_set), ToastLength.Short).Show();
            }
            else if (resultCode == Result.Ok && requestCode == 1) {
                EggSeed newSeed = new EggSeed();
                string seed = data.GetStringExtra("NewSeed");
                newSeed.SetSeed(FuncUtil.SeedStr2Array(seed));
                SetCurrentSeed(newSeed);
                SaveProfile();
                Toast.MakeText(this, Resources.GetString(Resource.String.profile_tinyfinder_set), ToastLength.Short).Show();
            }
            else if (resultCode == Result.Ok && requestCode == 2)
            {
                EggSeed newSeed = new EggSeed();
                string seed = data.GetStringExtra("NewSeed");
                newSeed.SetSeed(FuncUtil.SeedStr2Array(seed));
                SetCurrentSeed(newSeed);
                SaveProfile();
                Toast.MakeText(this, Resources.GetString(Resource.String.profile_seedrecovery_set), ToastLength.Short).Show();
            }
            else if (resultCode == Result.Ok && requestCode == 3)
            {
                int type = data.GetIntExtra("ClockType", -1);
                if (type == 0) // Initial Seed
                {
                    string seed = data.GetStringExtra("InitialSeed");
                    uint iseed = uint.Parse(seed, System.Globalization.NumberStyles.HexNumber);
                    int frame = data.GetIntExtra("SeedFrame", 0);

                    SetInitialSeed(iseed);
                    seedFrameView.Text = frame.ToString();
                    SaveProfile();
                    //String.Format(Resources.GetString(Resource.String.clock_results), preMinFrame, preMaxFrame);
                    Toast.MakeText(this, Resources.GetString(Resource.String.profile_clock_initial), ToastLength.Short).Show();
                    //Toast.MakeText(this, Resources.GetString(Resource.String.profile_seedrecovery_set), ToastLength.Short).Show();
                }
                else if (type == 1) // QR Frame
                {
                    int frame = data.GetIntExtra("QRExit", 0);
                    seedFrameView.Text = frame.ToString();
                    SaveProfile();
                    Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.profile_clock_qr), frame), ToastLength.Short).Show();
                    //Toast.MakeText(this, Resources.GetString(Resource.String.profile_seedrecovery_set), ToastLength.Short).Show();
                }
                else if (type == 2) { // ID Seed
                    string seed = data.GetStringExtra("InitialSeed");
                    uint iseed = uint.Parse(seed, System.Globalization.NumberStyles.HexNumber);
                    int frame = data.GetIntExtra("SeedFrame", 0);

                    //#TODO: Correction
                    int correction = data.GetIntExtra("Correction", 0);

                    seedFrameView.Text = frame.ToString();
                    SetInitialSeed(iseed);

                    SaveProfile();
                    Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.profile_clock_id), correction), ToastLength.Short).Show();
                    //Toast.MakeText(this, Resources.GetString(Resource.String.profile_seedrecovery_set), ToastLength.Short).Show();
                }

                if (data.HasExtra("SeedFrame")) {
                    PrepareReturnIntent(data.GetIntExtra("SeedFrame", 0));
                }
            }
        }

        // Automatically save data when pausing/exiting Activity
        protected override void OnPause()
        {
            base.OnPause();
            SaveProfile();
        }

        private void LoadProfile() {
            //numProfiles = ProfileData.GetNumProfiles(this);
            ProfileData pData = ProfileData.LoadCurrentProfileData(this);

            selectedProfileIndex = pData.profileIndex;
            gameVersionSpinner.SetSelection((int)pData.gameVersion);
            seedFrameView.Text = pData.seedFrame.ToString();
            profileTag = pData.profileTag;
            shinyCharmBox.Checked = pData.shinyCharm;
            tsvField.Text = pData.TSV.ToString("0000");
            SetCurrentSeed(pData.currentSeed);
            SetCheckpointSeed(pData.checkpointSeed);
            SetInitialSeed(pData.initialSeed);
        }

        private void LoadProfile(int index) {

            ProfileData pData = ProfileData.LoadProfileData(this,index);
            ProfileData.SetCurrentProfile(this, pData.profileIndex);

            selectedProfileIndex = pData.profileIndex;
            gameVersionSpinner.SetSelection((int)pData.gameVersion);
            seedFrameView.Text = pData.seedFrame.ToString();
            profileTag = pData.profileTag;
            shinyCharmBox.Checked = pData.shinyCharm;
            tsvField.Text = pData.TSV.ToString("0000");
            SetCurrentSeed(pData.currentSeed);
            SetCheckpointSeed(pData.checkpointSeed);
            SetInitialSeed(pData.initialSeed);
        }

        private void SaveProfile()
        {
            if (selectedProfileIndex > NoProfileSelected)
            {
                // Take data from the UI
                ProfileData pData = new ProfileData
                {
                    currentSeed = GetCurrentSeedFromView(),
                    gameVersion = (GameVersionUI)gameVersionSpinner.SelectedItemPosition,
                    seedFrame = int.Parse(seedFrameView.Text),
                    checkpointSeed = GetCheckpointSeedFromView(),
                    initialSeed = uint.Parse(iSeed.Text, System.Globalization.NumberStyles.HexNumber),
                    shinyCharm = shinyCharmBox.Checked,
                    TSV = ushort.Parse(tsvField.Text),
                    profileIndex = selectedProfileIndex,
                    profileTag = profileTag
                };

                // Save data
                ProfileData.SaveProfileData(this, pData);

                //Toast.MakeText(this, "Saving profile: " + selectedProfileIndex, ToastLength.Short).Show();
            }
        }

        private void ChangeProfile(int profileId) {
            SaveProfile();
            LoadProfile(profileId);
            PrepareReturnIntent(int.Parse(seedFrameView.Text));
        }

        private EggSeed GetCurrentSeedFromView() {
            EggSeed currSeed = new EggSeed();
            currSeed.SetSeed(seed3.Text, seed2.Text, seed1.Text, seed0.Text);
            return currSeed;
        }
        private EggSeed GetCheckpointSeedFromView()
        {
            EggSeed checkSeed = new EggSeed();
            checkSeed.SetSeed(chseed3.Text, chseed2.Text, chseed1.Text, chseed0.Text);
            return checkSeed;
        }

        private void SetCurrentSeed(EggSeed seed) {

            string[] curseeds = seed.GetSeedToStringArray();
            seed3.Text = curseeds[0];
            seed2.Text = curseeds[1];
            seed1.Text = curseeds[2];
            seed0.Text = curseeds[3];
        }

        private void SetCheckpointSeed(EggSeed seed)
        {
            string[] chseeds = seed.GetSeedToStringArray();
            chseed3.Text = chseeds[0];
            chseed2.Text = chseeds[1];
            chseed1.Text = chseeds[2];
            chseed0.Text = chseeds[3];
        }

        private void SetCurrentSeedFromCheckpoint()
        {
            seed3.Text = chseed3.Text;
            seed2.Text = chseed2.Text;
            seed1.Text = chseed1.Text;
            seed0.Text = chseed0.Text;
            SaveProfile();
        }

        private void SetCheckpointSeedFromCurrent()
        {
            chseed3.Text = seed3.Text;
            chseed2.Text = seed2.Text;
            chseed1.Text = seed1.Text;
            chseed0.Text = seed0.Text;
            SaveProfile();
        }

        private void SetInitialSeed(uint iseed) {
            iSeed.Text = iseed.ToString("X");
        }

        private void EditProfile() {

        }

        private void UpdateSpinner(int profileSelected) {
            // Get names of all profiles
            string[] profileNames = ProfileData.GetProfileNames(this);

            // Initialize spinner
            userEditingProfile = false; // Prevent reloading profile
            profileSelectionSpinner.Adapter = null;
            profileSelectionSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, profileNames );
            profileSelectionSpinner.SetSelection(profileSelected - 1);
        }

        private void AddProfile(string name) {
            int newProfileId = ProfileData.CreateProfile(this,name);
            ChangeProfile(newProfileId);
            UpdateSpinner(selectedProfileIndex);
        }
        private void AddProfileModel(string name)
        {
            ProfileData pData = new ProfileData
            {
                currentSeed = GetCurrentSeedFromView(),
                checkpointSeed = GetCheckpointSeedFromView(),
                initialSeed = uint.Parse(iSeed.Text, System.Globalization.NumberStyles.HexNumber),
                shinyCharm = shinyCharmBox.Checked,
                TSV = ushort.Parse(tsvField.Text),
                profileIndex = selectedProfileIndex,
                profileTag = name
            };
            int newProfileId = ProfileData.CreateProfile(this, pData);
            ChangeProfile(newProfileId);
            UpdateSpinner(selectedProfileIndex);
        }

        private void DeleteProfile() {
            int numProfiles = ProfileData.GetNumProfiles(this);
            if ( numProfiles > 1) {

                ProfileData.DeleteProfile(this, selectedProfileIndex);

                if (selectedProfileIndex == numProfiles) {
                    selectedProfileIndex--;
                }
                LoadProfile(selectedProfileIndex);
                UpdateSpinner(selectedProfileIndex);

                PrepareReturnIntent(int.Parse(seedFrameView.Text));
            }
        }

        private void UpdateProfileTag(string name) {
            profileTag = name;

            // Take data from the UI
            ProfileData pData = new ProfileData
            {
                currentSeed = GetCurrentSeedFromView(),
                gameVersion = (GameVersionUI)gameVersionSpinner.SelectedItemId,
                seedFrame = int.Parse(seedFrameView.Text),
                checkpointSeed = GetCheckpointSeedFromView(),
                initialSeed = uint.Parse(iSeed.Text, System.Globalization.NumberStyles.HexNumber),
                shinyCharm = shinyCharmBox.Checked,
                TSV = ushort.Parse(tsvField.Text),
                profileIndex = selectedProfileIndex,
                profileTag = name
            };

            // Save data
            ProfileData.SaveProfileData(this, pData);
        }

        private void PrepareReturnIntent(int frame) {
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("SeedFrame", frame);
            SetResult(Result.Ok, returnIntent);
        }
    }
}