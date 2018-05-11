using Android.App;
using Android.Widget;
using Android.OS;

using Android.Preferences;

using Pk3DSRNGTool;
using Pk3DSRNGTool.Core;
using Pk3DSRNGTool.RNG;

using Gen7EggRNG.EggRM;

using System.Collections.Generic;
using System;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Graphics;
using Android.Views.InputMethods;
using Android.Text;
using Android.Text.Style;

namespace Gen7EggRNG
{
    [Activity(Label = "@string/app_name", MainLauncher = true, //Icon = "@drawable/eggrng7icon",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MainActivity : Activity
    {
        private byte npcModelNum = 5; // #TODO: NPC + 1;

        private FullSearchData uiSearchData = new FullSearchData();
        private FullSearchData currentSearchData = null;
        private List<FullSearchData> previousSearchData = new List<FullSearchData>();
        /*private ProfileData profileData;
        private ParentData parents;
        private FilterData filters;
        private SearchParams uiSearchParams;
        private AppPreferences appPrefs;*/

        // Current data
        /*private SearchParams currentParams;
        private ParentData currentParentData;
        */

        private int maxResults = SearchConstants.ResultLimitLow;
        private int minFrame = 0;
        private int maxFrame = SearchConstants.ResultLimitLow;
        private int aroundTargetFrame = 100;

        private List<G7EFrame> eggFrames = new List<G7EFrame>();

        ImageButton previousButton;
        Button searchButton;
        Button parentsButton;
        Button filterButton;
        Button profileButton;
        Button miscButton;
        ImageButton prefsButton;

        ListView resTab;
        LinearLayout mainLayout;
        Spinner searchParamSpinner;
        LinearLayout resGuide;
        CheckBox checkFilter;
        CheckBox checkOtherTSV;

        EditText editTargetFrame;

        TextView currentSeedDump;
        TextView userTSVDump;

        LinearLayout mainEggRes;

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
            Window.RequestFeature(Android.Views.WindowFeatures.NoTitle);

            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Fetch Widgets
            previousButton = (ImageButton)FindViewById(Resource.Id.mainPreviousButton);
            searchButton = (Button)FindViewById(Resource.Id.buttonCalc);
            parentsButton = (Button)FindViewById(Resource.Id.buttonParents);
            filterButton = (Button)FindViewById(Resource.Id.buttonFilter);
            profileButton = (Button)FindViewById(Resource.Id.buttonProfile);
            miscButton = (Button)FindViewById(Resource.Id.buttonMisc);
            prefsButton = (ImageButton)FindViewById(Resource.Id.buttonPreferences);

            resTab = (ListView)FindViewById(Resource.Id.listView1);

            mainLayout = (LinearLayout)FindViewById(Resource.Id.linearLayout1);
            resGuide = (LinearLayout)FindViewById(Resource.Id.resultGuideline);
            mainEggRes = (LinearLayout)FindViewById(Resource.Id.mainEggStats);

            searchParamSpinner = (Spinner)FindViewById(Resource.Id.searchType);
            editTargetFrame = (EditText)FindViewById(Resource.Id.targetFrame);
            checkFilter = (CheckBox)FindViewById(Resource.Id.checkUseFilter);
            checkOtherTSV = (CheckBox)FindViewById(Resource.Id.mainOtherTSV);
            currentSeedDump = (TextView)FindViewById(Resource.Id.currenSeedDump);
            userTSVDump = (TextView)FindViewById(Resource.Id.tsvDump);

            // Data initialization
            uiSearchData.searchParameters.type = SearchType.NormalSearch;
            uiSearchData.searchParameters.range = SearchRange.Simple;
            uiSearchData.searchParameters.mainRNG.considerDelay = false;
            uiSearchData.searchParameters.mainRNG.delay = 0;
            uiSearchData.searchParameters.mainRNG.ctimeline = 3600;
            uiSearchData.searchParameters.mainRNG.npcs = 0;
            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.MinMax;
            uiSearchData.searchParameters.mainRNG.minFrame = 0;
            uiSearchData.searchParameters.mainRNG.maxFrame = SearchConstants.MaximumFramesPerSearch;

            LoadProfileData();

            // Initialize widget functions
            //Button seedButton = (Button)FindViewById(Resource.Id.buttonSeed);

            /*seedButton.Click += delegate {
                string sString = currentSeed.GetSeedToString();

                var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                clipboard.PrimaryClip = ClipData.NewPlainText("",sString);

                Toast clickFeedback = Toast.MakeText(this, "Seed copied to clipboard\n" + sString, ToastLength.Short);
                clickFeedback.Show();
            };*/

            previousButton.Enabled = false;
            previousButton.Click += delegate {
                PerformPreviousSearch();
            };

            searchButton.Click += delegate {
                PerformSearch();
            };
            searchButton.LongClick += delegate {
                Dialog dialog = new Dialog(this);

                dialog.SetContentView(Resource.Layout.SearchSettingsDialog);

                dialog.SetTitle(Resources.GetString(Resource.String.search_settings));

                LinearLayout simpleLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSSimple);
                LinearLayout rangeLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSRange);
                LinearLayout nearLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSNear);
                LinearLayout delayLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSDelay);
                LinearLayout npcLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSNPC);
                LinearLayout cTimelineLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSCreateTimeline);
                LinearLayout timeLeapLL = dialog.FindViewById<LinearLayout>(Resource.Id.diaSSTimeLeap);

                RadioButton simpleSearchCheck = (RadioButton)dialog.FindViewById(Resource.Id.simpleSearch);
                RadioButton rangeSearchCheck = (RadioButton)dialog.FindViewById(Resource.Id.rangeSearchCheck);
                RadioButton targetSearchCheck = (RadioButton)dialog.FindViewById(Resource.Id.nearTarget);
                CheckBox delayCheck = dialog.FindViewById<CheckBox>(Resource.Id.delayCheck);
                RadioButton timeLeapRB = dialog.FindViewById<RadioButton>(Resource.Id.timelineLeapRB);
                RadioButton cTimelineRB = dialog.FindViewById<RadioButton>(Resource.Id.createTimelineRB);

                EditText minFrameEdit = (EditText)dialog.FindViewById(Resource.Id.minFrame);
                EditText maxFrameEdit = (EditText)dialog.FindViewById(Resource.Id.maxFrame);

                int lastRangeModified = 0;

                EditText nearFrameEdit = (EditText)dialog.FindViewById(Resource.Id.targetNeighbourhood);
                nearFrameEdit.Text = aroundTargetFrame.ToString();
                nearFrameEdit.TextChanged += (sender, args) => {
                    int.TryParse(nearFrameEdit.Text, out aroundTargetFrame);
                };

                EditText delayET = dialog.FindViewById<EditText>(Resource.Id.delay);

                EditText npcNumET = dialog.FindViewById<EditText>(Resource.Id.npcCount);

                EditText cTimelineET = dialog.FindViewById<EditText>(Resource.Id.timelineParam);

                

                Button dialogOk = (Button)dialog.FindViewById(Resource.Id.diaSearchAccept);
                dialogOk.Click += delegate
                {
                    // Save Data
                    dialog.Dismiss();
                };

                Utility.SimpleRadioGroup searchGroup = new Utility.SimpleRadioGroup();


                // Verify constraints
                if (uiSearchData.searchParameters.type != SearchType.MainEggRNG)
                {
                    delayLL.Visibility = ViewStates.Gone;
                    npcLL.Visibility = ViewStates.Gone;
                    cTimelineLL.Visibility = ViewStates.Gone;
                    timeLeapLL.Visibility = ViewStates.Gone;

                    searchGroup.Add(simpleSearchCheck);
                    searchGroup.Add(rangeSearchCheck);
                    searchGroup.Add(targetSearchCheck);
                    /*searchGroup.Add(cTimelineRB);
                    searchGroup.Add(timeLeapRB);*/

                    simpleSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {

                            searchGroup.UncheckOthers(simpleSearchCheck);
                            uiSearchData.searchParameters.range = SearchRange.Simple;
                        }
                    };
                    rangeSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(rangeSearchCheck);
                            uiSearchData.searchParameters.range = SearchRange.MinMax;
                        }
                    };
                    targetSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(targetSearchCheck);
                            uiSearchData.searchParameters.range = SearchRange.AroundTarget;
                        }
                    };

                    // Initialize data
                    if (uiSearchData.searchParameters.range == SearchRange.Simple)
                    {
                        simpleSearchCheck.Checked = true;
                    }
                    else if (uiSearchData.searchParameters.range == SearchRange.MinMax)
                    {
                        rangeSearchCheck.Checked = true;
                    }
                    else if (uiSearchData.searchParameters.range == SearchRange.AroundTarget)
                    {
                        targetSearchCheck.Checked = true;
                    }

                    minFrameEdit.Text = minFrame.ToString();
                    maxFrameEdit.Text = maxFrame.ToString();
                }
                else {
                    //searchGroup.Add(simpleSearchCheck);
                    simpleSearchCheck.Visibility = ViewStates.Gone;
                    timeLeapLL.Visibility = ViewStates.Gone;

                    searchGroup.Add(rangeSearchCheck);
                    searchGroup.Add(targetSearchCheck);
                    searchGroup.Add(cTimelineRB);
                    searchGroup.Add(timeLeapRB);

                    delayCheck.CheckedChange += (sender, args) => {
                        uiSearchData.searchParameters.mainRNG.considerDelay = args.IsChecked;
                        delayET.Enabled = args.IsChecked;
                    };

                    delayET.Text = uiSearchData.searchParameters.mainRNG.delay.ToString();
                    delayCheck.Checked = uiSearchData.searchParameters.mainRNG.considerDelay;

                    npcNumET.Text = uiSearchData.searchParameters.mainRNG.npcs.ToString();

                    simpleSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(simpleSearchCheck);
                            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.Simple;
                        }
                    };
                    rangeSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(rangeSearchCheck);
                            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.MinMax;
                        }
                    };
                    targetSearchCheck.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(targetSearchCheck);
                            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.AroundTarget;
                        }
                    };

                    //#TODO: Implement Timeline searches
                    cTimelineRB.Enabled = false;
                    cTimelineET.Enabled = false;
                    /*cTimelineRB.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(cTimelineRB);
                            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.CreateTimeline;
                        }
                    };
                    timeLeapRB.CheckedChange += (sender, args) => {
                        if (args.IsChecked == true)
                        {
                            searchGroup.UncheckOthers(timeLeapRB);
                            uiSearchData.searchParameters.mainRNG.mainRange = MainSearchRange.TimelineLeap;
                        }
                    };*/

                    /*if (uiSearchData.searchParameters.mainRNG.mainRange == MainSearchRange.Simple)
                    {
                        simpleSearchCheck.Checked = true;
                    }*/
                    if (uiSearchData.searchParameters.mainRNG.mainRange == MainSearchRange.MinMax)
                    {
                        rangeSearchCheck.Checked = true;
                    }
                    else if (uiSearchData.searchParameters.mainRNG.mainRange == MainSearchRange.AroundTarget)
                    {
                        targetSearchCheck.Checked = true;
                    }
                    else if (uiSearchData.searchParameters.mainRNG.mainRange == MainSearchRange.CreateTimeline)
                    {
                        cTimelineRB.Checked = true;
                    }
                    else if (uiSearchData.searchParameters.mainRNG.mainRange == MainSearchRange.TimelineLeap)
                    {
                        timeLeapRB.Checked = true;
                    }

                    minFrameEdit.Text = uiSearchData.searchParameters.mainRNG.minFrame.ToString();
                    maxFrameEdit.Text = uiSearchData.searchParameters.mainRNG.maxFrame.ToString();
                }

                minFrameEdit.AfterTextChanged += (sender, args) =>
                {
                    lastRangeModified = 1;
                };
                maxFrameEdit.AfterTextChanged += (sender, args) =>
                {
                    lastRangeModified = 2;
                };

                dialog.DismissEvent += (sender, args) => {
                    if (uiSearchData.searchParameters.type == SearchType.MainEggRNG)
                    {
                        int preMinFrame = 0;
                        if (int.TryParse(minFrameEdit.Text, out preMinFrame))
                        {

                        }
                        int preMaxFrame = 0;
                        if (int.TryParse(maxFrameEdit.Text, out preMaxFrame))
                        {
                        }
                        if (preMinFrame > preMaxFrame) {
                            int tmp = preMinFrame;
                            preMinFrame = preMaxFrame;
                            preMaxFrame = tmp;
                        }
                        int sFrame = GameVersionConversion.GetGameStartingFrame(uiSearchData.profile.gameVersion, false);
                        preMinFrame = Math.Max(sFrame, preMinFrame);
                        uiSearchData.searchParameters.mainRNG.minFrame = preMinFrame;

                        //#TODO: Reinforce actual limits - #DEBUG for now just test
                        //preMinFrame = Math.Min(preMinFrame, SearchConstants.MaximumFramesPerSearch);
                        //preMinFrame = Math.Min(preMaxFrame, SearchConstants.MaximumFramesPerSearch);

                        uiSearchData.searchParameters.mainRNG.minFrame = preMinFrame;
                        uiSearchData.searchParameters.mainRNG.maxFrame = preMaxFrame;
                    }
                    else
                    {
                        bool warn = false;

                        int preMinFrame = 0;
                        if (int.TryParse(minFrameEdit.Text, out preMinFrame))
                        {

                        }
                        int preMaxFrame = 0;
                        if (int.TryParse(maxFrameEdit.Text, out preMaxFrame))
                        {
                        }

                        if (preMinFrame > SearchConstants.MaximumFramesPerSearch)
                        {
                            preMinFrame = SearchConstants.MaximumFramesPerSearch;
                            warn = true;
                        }
                        if (preMaxFrame > SearchConstants.MaximumFramesPerSearch)
                        {
                            preMaxFrame = SearchConstants.MaximumFramesPerSearch;
                            warn = true;
                        }

                        if (preMinFrame > preMaxFrame)
                        {
                            if (lastRangeModified == 1)
                            {
                                preMaxFrame = preMinFrame;
                            }
                            else if (lastRangeModified == 2)
                            {
                                preMinFrame = preMaxFrame;
                            }
                            else
                            {
                                preMinFrame = 0;
                                preMaxFrame = uiSearchData.preferences.MaxResultValue();
                            }
                            warn = true;
                        }

                        if (warn)
                        {
                            string warningString = String.Format(Resources.GetString(Resource.String.search_range_warning), preMinFrame, preMaxFrame);
                            Toast.MakeText(this, warningString, ToastLength.Short).Show();
                            //Toast.MakeText(this, "Invalid values in range search.\nSetting range to [" + preMinFrame + " : " + preMaxFrame + "]", ToastLength.Short).Show();
                        }

                        minFrame = preMinFrame;
                        maxFrame = preMaxFrame;
                    }

                    if (npcLL.Enabled)
                    {
                        if (npcNumET.Text != string.Empty) {
                            uiSearchData.searchParameters.mainRNG.npcs = int.Parse(npcNumET.Text);
                        }
                    }
                    if (delayLL.Enabled) {
                        if (delayET.Text != string.Empty)
                        {
                            uiSearchData.searchParameters.mainRNG.delay = int.Parse(delayET.Text);
                        }
                    }
                };

                dialog.Show();
            };

            parentsButton.Click += delegate {
                StartActivity(typeof(ParentEditActivity));
            };

            filterButton.Click += delegate {
                StartActivity(typeof(FilterEditActivity));
            };

            profileButton.Click += delegate {
                StartActivity(typeof(ProfileEditActivity));
            };

            miscButton.Click += delegate {
                StartActivity(typeof(MiscOptionsActivity));
            };

            prefsButton.Click += delegate
            {
                StartActivity(typeof(PreferencesActivity));
            };


            // Search parameters
            
            searchParamSpinner.ItemSelected += (sender, args) =>
            {
                SearchType stype = (SearchType)args.Position;
                uiSearchData.searchParameters.type = stype;

                VerifySearchConstraints(stype);
            };
            
            editTargetFrame.TextChanged += (sender, args) =>
            {
                int val = 0;
                int.TryParse(editTargetFrame.Text, out val);
                uiSearchData.searchParameters.targetFrame = val;
            };

            // Set filters to ALWAYS be disabled by default
            checkFilter.CheckedChange += (sender, args) => {
                uiSearchData.searchParameters.useFilter = args.IsChecked;
            };
            checkFilter.Checked = false;
            checkOtherTSV.CheckedChange += (sender, args) => {
                uiSearchData.searchParameters.checkOtherTSV = args.IsChecked;
            };
            checkFilter.Checked = false;


            // Set adapter long click
            resTab.ItemClick += (sender, args) => {
                /*Toast clickFeedback = Toast.MakeText(this, "You've selected Egg Frame " + args.Position + ".", ToastLength.Short);
                clickFeedback.Show();*/
                if (currentSearchData.searchParameters.type == SearchType.ShortestPath)
                {
                    (resTab.Adapter as EggResultAdapter).CheckItem(args.Position);
                }
            };


            resTab.ItemLongClick += (sender, args) => {

                PopupMenu menu = new PopupMenu(this, profileButton, Android.Views.GravityFlags.Center);
                SetupResultMenu(menu,args.Position);

                menu.DismissEvent += (s1, arg1) =>
                {
                    /*Do nothing*/
                };

                menu.Show();
                /*currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[args.Position].TinyState));
                Toast clickFeedback = Toast.MakeText(this, "Set current seed from egg " + eggFrames[args.Position].FrameNum + " " + eggFrames[args.Position].TinyState, ToastLength.Long);
                clickFeedback.Show();*/
            };
        }


        protected override void OnResume()
        {
            base.OnResume();
            LoadProfileData();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok && requestCode == 1) {
                if (data.HasExtra("SeedFrame")) {
                    uiSearchData.searchParameters.mainRNG.minFrame = GetBestStartingFrame(data.GetIntExtra("SeedFrame", 0));
                }
            }
        }


        /*private void AddEntryToResults(LinearLayout resTab) {
            for (int i = 0; i < eggFrames.Count; ++i)
            {
                TextView row = new TextView(this)
                {
                    Text = eggFrames[i].FrameNum + " " + eggFrames[i].FrameUsed + " " + eggFrames[i].NatureStr + " " + eggFrames[i].AbilityStr + " " + eggFrames[i].Ball
                        + " " + eggFrames[i].HP + " " + eggFrames[i].Atk + " " + eggFrames[i].Def + " " + eggFrames[i].SpA + " " + eggFrames[i].SpD+ " " + eggFrames[i].Spe
                        + " " + eggFrames[i].HiddenPower + " " + eggFrames[i].PSV
                };
                LinearLayout.LayoutParams rowParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.MatchParent);
                row.LayoutParameters = rowParams;
                count++;

                // Add row to table
                resTab.AddView(row);
            }
            eggFrames.Clear();
        }*/

        private void PerformSearch() {
            HideMainEggBar();
            resTab.Adapter = null;
            eggFrames.Clear();
            GC.Collect();

            // History of search data
            if (currentSearchData != null) {
                // Maximum of 10 previous search
                if ( previousSearchData.Count == SearchConstants.MaximumHistory)
                {
                    previousSearchData.RemoveAt(0);
                }
                previousSearchData.Add(currentSearchData);

                previousButton.Enabled = true;
            }

            // Grab data from UI and set it to Current Search
            currentSearchData = uiSearchData.Copy();

            if (currentSearchData.searchParameters.type == SearchType.NormalSearch)
            {
                Search7_Egg();
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAccept)
            {
                Search7_EggAcceptOnly();
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAcceptPlus)
            {
                Search7_EggAcceptPlus();
            }
            else if (currentSearchData.searchParameters.type == SearchType.ShortestPath)
            {
                Search7_EggShortestPath();
                //Search7_EggFastSearch();
            }
            else if (currentSearchData.searchParameters.type == SearchType.LeastAdvances)
            {
                //Toast.MakeText(this, "Fastest", ToastLength.Short).Show();
                Search7_EggFastestSearch();
            }
            else if (currentSearchData.searchParameters.type == SearchType.MainEggRNG) {
                Search7_MainEggRNG();
            }

            // IF Search succeeded
            if (eggFrames.Count > 0)
            {
                AddEntryToResults(resTab);
            }
            else {
                currentSearchData = null;
            }
            RNGPool.Clear();
        }

        private void PerformPreviousSearch()
        {
            if ( previousSearchData.Count == 0 ) { return; }

            HideMainEggBar();
            resTab.Adapter = null;
            eggFrames.Clear();
            GC.Collect();

            // History of search data
            /*if (currentSearchData != null) {
                previousSearchData.Add(currentSearchData);
            }*/

            // Grab data from UI and set it to Current Search
            currentSearchData = previousSearchData[previousSearchData.Count - 1];
            previousSearchData.RemoveAt(previousSearchData.Count - 1);

            if (previousSearchData.Count == 0) {
                previousButton.Enabled = false;
            }

            if (currentSearchData.searchParameters.type == SearchType.NormalSearch)
            {
                Search7_Egg();
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAccept)
            {
                Search7_EggAcceptOnly();
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAcceptPlus)
            {
                Search7_EggAcceptPlus();
            }
            else if (currentSearchData.searchParameters.type == SearchType.ShortestPath)
            {
                Search7_EggShortestPath();
                //Search7_EggFastSearch();
            }
            else if (currentSearchData.searchParameters.type == SearchType.LeastAdvances)
            {
                //Toast.MakeText(this, "Fastest", ToastLength.Short).Show();
                Search7_EggFastestSearch();
            }
            else if (currentSearchData.searchParameters.type == SearchType.MainEggRNG)
            {
                // Assume previous search was verified to be correct for Main Egg RNG
                Search7_MainEggRNG();
            }

            // IF Search succeeded
            if (eggFrames.Count > 0)
            {
                AddEntryToResults(resTab);
            }
            else {
                string warningString = Resources.GetString(Resource.String.search_noframes);
                Toast.MakeText(this, warningString, ToastLength.Short).Show();
            }
            RNGPool.Clear();
        }


        private void AddEntryToResults(ListView resTab)
        {
            EggResultAdapter adapter = new EggResultAdapter(this, Android.Resource.Layout.SimpleListItem1, eggFrames.ToArray(), currentSearchData);

            resGuide.RemoveAllViews();
            adapter.InflateListGuideline(resGuide);

            if (currentSearchData.searchParameters.type == SearchType.MainEggRNG) {
                BuildMainEggResult(new G7EFrame(ResultME7.Egg as ResultE7, 0), adapter);
                ShowMainEggBar();
            }

            // Convert data to list
            resTab.Adapter = adapter;
            //eggFrames.Clear();
        }

        /*private bool CheckRandomNumber(uint rn)
        {
            int sv = (int)((rn >> 16) ^ (rn & 0xFFFF)) >> 4;
            return sv == TSV.Value || ConsiderOtherTSV.Checked && OtherTSVList.Contains(sv);
        }*/

        private void Search7_Egg()
        {
            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            int min = 0;
            int max = SearchConstants.MaximumFramesPerSearch;

            if (currentSearchData.searchParameters.range == SearchRange.Simple)
            {
                min = 0;
                max = SearchConstants.MaximumFramesPerSearch;
            }
            else if (currentSearchData.searchParameters.range == SearchRange.MinMax)
            {
                min = minFrame;
                max = maxFrame;
            }
            else if (currentSearchData.searchParameters.range == SearchRange.AroundTarget) {
                min = Math.Max(currentSearchData.searchParameters.targetFrame - aroundTargetFrame, 0);
                max = Math.Min(currentSearchData.searchParameters.targetFrame + aroundTargetFrame, SearchConstants.MaximumFramesPerSearch);
            }

                // Advance
                for (int i = 0; i < min; i++)
                rng.Next();
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);
            // Start
            for (int i = min; i <= max; i++, RNGPool.AddNext(rng))
            {
                var result = RNGPool.GenerateEgg7() as ResultE7;
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                if (currentSearchData.searchParameters.useFilter && (!currentSearchData.filter.VerifyEgg(result) && !currentSearchData.filter.VerifyRemind(result, currentSearchData.profile.TSV, currentSearchData.searchParameters.checkOtherTSV, currentSearchData.otherTSVs)))
                {
                    continue;
                }
                /*if (!(filter.CheckResult(result) || ShinyRemind.Checked && CheckRandomNumber(result.RandNum)))
                    continue;*/
                eggFrames.Add(new G7EFrame(result, frame: i));
                if (eggFrames.Count >= maxResults)
                    return;
            }
        }

        private void Search7_EggCustom(int maxResult, int maxFrame)
        {
            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            int min = 0;
            int max = maxFrame;

            /*if (currentSearchData.searchParameters.range == SearchRange.Simple)
            {
                min = 0;
                max = SearchConstants.MaximumFramesPerSearch;
            }
            else if (currentSearchData.searchParameters.range == SearchRange.MinMax)
            {
                min = minFrame;
                max = maxFrame;
            }
            else if (currentSearchData.searchParameters.range == SearchRange.AroundTarget)
            {
                min = Math.Max(currentSearchData.searchParameters.targetFrame - aroundTargetFrame, 0);
                max = Math.Min(currentSearchData.searchParameters.targetFrame + aroundTargetFrame, SearchConstants.MaximumFramesPerSearch);
            }*/

            // Advance
            /*for (int i = 0; i < min; i++)
                rng.Next();*/
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);
            // Start
            for (int i = min; i <= max; i++, RNGPool.AddNext(rng))
            {
                var result = RNGPool.GenerateEgg7() as ResultE7;
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                if (currentSearchData.searchParameters.useFilter && (!currentSearchData.filter.VerifyEgg(result) && !currentSearchData.filter.VerifyRemind(result, currentSearchData.profile.TSV, currentSearchData.searchParameters.checkOtherTSV, currentSearchData.otherTSVs)))
                {
                    continue;
                }
                /*if (!(filter.CheckResult(result) || ShinyRemind.Checked && CheckRandomNumber(result.RandNum)))
                    continue;*/
                eggFrames.Add(new G7EFrame(result, frame: i));
                if (eggFrames.Count >= maxResult)
                    return;
            }

            /*if (eggFrames.Count == 0)
            {
                Toast.MakeText(this, "No matching frames found.", ToastLength.Short).Show();
            }*/
        }

        private void Search7_EggAcceptOnly()
        {
            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());

            //int min = 0;
            int max = SearchConstants.MaximumFramesPerSearch;

            // Advance
            /*for (int i = 0; i < min; i++)
                rng.Next();*/
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);
            // Start
            int frameCount = 0;
            int eggCount = 1;
            while (frameCount < max)
            {
                var result = RNGPool.GenerateEgg7() as ResultE7;
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                if (!currentSearchData.searchParameters.useFilter || (currentSearchData.searchParameters.useFilter && (currentSearchData.filter.VerifyEgg(result) || currentSearchData.filter.VerifyRemind(result, currentSearchData.profile.TSV, currentSearchData.searchParameters.checkOtherTSV, currentSearchData.otherTSVs))))
                {
                    eggFrames.Add(new G7EFrame(result, frame: frameCount, eggnum: eggCount));
                    if (eggFrames.Count >= maxResults)
                        return;
                }
                eggCount++;
                /*if (!(filter.CheckResult(result) || ShinyRemind.Checked && CheckRandomNumber(result.RandNum)))
                    continue;*/


                // Advance acceptance frames
                int adv = result.FramesUsed;
                while (adv > 0)
                {
                    RNGPool.AddNext(rng);
                    adv--;
                }
                frameCount += result.FramesUsed;
            }
        }


        private void Search7_EggAcceptPlus()
        {
            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            //int min = 0;
            int max = SearchConstants.MaximumFramesPerSearch;

            // Advance
            /*for (int i = 0; i < min; i++)
                rng.Next();*/
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);
            // Start
            int frameCount = 0;
            bool acceptEgg = true;
            int numFollows = 4;
            int advance = 0;
            int eggCount = 1;

            while( frameCount < max)
            {
                var result = RNGPool.GenerateEgg7() as ResultE7;
                int eggnum = 0;
                int framenum = frameCount;
                bool isRej = false;

                if (acceptEgg)
                {
                    eggnum = eggCount;
                    eggCount++;

                    numFollows = 4;
                    frameCount++;
                    RNGPool.AddNext(rng);
                    advance = result.FramesUsed - 1;
                    acceptEgg = false;
                }
                else {
                    isRej = true;
                    eggnum = 1 + (4 - numFollows);
                    numFollows--;
                    if (numFollows == 0)
                    {
                        acceptEgg = true;

                        frameCount += advance;
                        while (advance > 0)
                        {
                            RNGPool.AddNext(rng);
                            advance--;
                        }
                    }
                    else {
                        RNGPool.AddNext(rng);
                        frameCount++;
                        advance--;
                    }
                }
                if (currentSearchData.searchParameters.useFilter && (!currentSearchData.filter.VerifyEgg(result) && !currentSearchData.filter.VerifyRemind(result, currentSearchData.profile.TSV, currentSearchData.searchParameters.checkOtherTSV, currentSearchData.otherTSVs)))
                {
                    continue;
                }
                /*if (!(filter.CheckResult(result) || ShinyRemind.Checked && CheckRandomNumber(result.RandNum)))
                    continue;*/

                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                var newFrame = new G7EFrame(result, frame: framenum, eggnum: eggCount-1);
                if (isRej) { newFrame.AdvanceInfo = PokeRNGApp.Strings.acceptplus_reject_count + eggnum; newFrame.AdvType = AdvanceType.Reject; }
                else { newFrame.AdvType = AdvanceType.Accept; }
                eggFrames.Add(newFrame);
                if (eggFrames.Count >= maxResults)
                    return;
            }
        }

        private void Search7_EggShortestPath() {
            if (currentSearchData.searchParameters.targetFrame > SearchConstants.MaximumTargetFrame)
            {
                Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.search_shortestpath_highframe), SearchConstants.MaximumTargetFrame), ToastLength.Short).Show();
                return;
            }

            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            int max = currentSearchData.searchParameters.targetFrame;
            int rejectcount = 0;
            List<ResultE7> ResultsList = new List<ResultE7>();
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);
            // Start
            for (int i = 0; i <= max; i++, RNGPool.AddNext(rng))
                ResultsList.Add(RNGPool.GenerateEgg7() as ResultE7);
            
            var FrameIndexList = Gen7EggPath.Calc(ResultsList.ConvertAll(egg => egg.FramesUsed).ToArray());
            max = FrameIndexList.Count;
            for (int i = 0; i < max; i++)
            {
                int index = FrameIndexList[i];
                var result = ResultsList[index];
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                var Frame = new G7EFrame(result, frame: index, eggnum: i + 1);
                if (i == max - 1 || FrameIndexList[i + 1] - index > 1)
                    Frame.AdvType = AdvanceType.Accept;
                else
                {
                    Frame.AdvType = AdvanceType.Reject;
                    rejectcount++;
                }
                eggFrames.Add(Frame);
                // Do not limit shortest path
            }
            string warningString = String.Format(Resources.GetString(Resource.String.search_shortestpath_sequence),
                max-rejectcount, rejectcount, max);
            Toast.MakeText(this, warningString, ToastLength.Short).Show();
            //Toast.MakeText(this, "Accept: " + (max - rejectcount) + "  Reject: " + rejectcount + "\nTotal of " + max + " eggs.", ToastLength.Long).Show();
            //Egg_Instruction.Text = getEggListString(max - rejectcount - 1, rejectcount, true);
        }
        private int SilentSearch7_EggShortestPath()
        {
            /*if (currentSearchData.searchParameters.targetFrame > SearchConstants.MaximumTargetFrame)
            {
                Toast.MakeText(this, "Target frame is too high.\nMaximum target frame is: " + SearchConstants.MaximumTargetFrame, ToastLength.Short).Show();
                return -1;
            }*/


            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            int max = eggFrames[eggFrames.Count - 1].FrameNum;

            List<EggResult> ResultsList = new List<EggResult>();
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);

            // Start
            for (int i = 0; i <= max; i++, RNGPool.AddNext(rng))
                ResultsList.Add(RNGPool.GenerateEgg7() as EggResult);

            var resArray = ResultsList.ConvertAll(egg => egg.FramesUsed).ToArray();

            int min = int.MaxValue;
            int index = -1;
            for (int i = 0; i < eggFrames.Count; ++i)
            {
                int numEggs = Gen7EggPath.CountNodes(resArray, eggFrames[i].FrameNum+1);
                eggFrames[i].AdvanceInfo = "#="+numEggs.ToString();
                if (numEggs < min) {
                    min = numEggs;
                    index = i;

                    /*if (min == 1 || min == 2) {
                        break;
                    }*/
                }
            }
            return index;
        }

        private void Search7_EggFastSearch()
        {
            if (currentSearchData.searchParameters.targetFrame > SearchConstants.MaximumTargetFrame)
            {
                Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.search_shortestpath_highframe), SearchConstants.MaximumTargetFrame), ToastLength.Short).Show();
                return;
            }

            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            int max = currentSearchData.searchParameters.targetFrame;
            int rejectcount = 0;

            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);

            // Automatically add accepts until Distance<N frames
            int eggNum = 1;
            int frameCount = 0;
            while (max-frameCount > SearchConstants.ShortestPathHeuristicHigh) {
                var result = RNGPool.GenerateEgg7() as ResultE7;
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
                var newFrame = new G7EFrame(result, frame: frameCount, eggnum: eggNum++);
                newFrame.AdvType = AdvanceType.Accept;
                eggFrames.Add(newFrame);

                // Advance acceptance frames
                int adv = result.FramesUsed;
                
                while (adv > 0)
                {
                    RNGPool.AddNext(rng);
                    adv--;
                }
                frameCount += result.FramesUsed;
            }

            // Perform actual search on the last #ShortestPathHeuristic eggs
            List<ResultE7> resultGraph = new List<ResultE7>();
            for (int i = frameCount; i <= max; i++, RNGPool.AddNext(rng)) {
                resultGraph.Add(RNGPool.GenerateEgg7() as ResultE7);
            }

            var frameIndexList = Gen7EggPath.Calc(resultGraph.ConvertAll(egg => egg.FramesUsed).ToArray());
            max = frameIndexList.Count;
            for (int i = 0; i < max; i++)
            {
                int index = frameIndexList[i];
                var result = resultGraph[index];
                result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);

                var newFrame = new G7EFrame(result, frame: frameCount+index, eggnum: eggNum+i);
                if (i == max - 1 || frameIndexList[i + 1] - index > 1)
                    newFrame.AdvType = AdvanceType.Accept;
                else
                {
                    newFrame.AdvType = AdvanceType.Reject;
                    rejectcount++;
                }
                eggFrames.Add(newFrame);
                // LIMIT ON SHORTEST PATH?
            }

            string warningString = String.Format(Resources.GetString(Resource.String.search_shortestpath_sequence),
                eggFrames.Count-rejectcount, rejectcount, eggFrames.Count);
            Toast.MakeText(this, warningString, ToastLength.Short).Show();
            //Toast.MakeText(this, "Accept " + (eggFrames.Count - rejectcount) + "  Reject: " + rejectcount + "\nTotal of " + eggFrames.Count + " eggs.", ToastLength.Long).Show();
            //Egg_Instruction.Text = getEggListString(max - rejectcount - 1, rejectcount, true);
        }

        private void Search7_EggFastestSearch() {
            //1. Perform filtered search
            //Search7_Egg();
            Search7_EggCustom(maxResults,5000); // Max Results, Max Frames
            //Toast.MakeText(this, "Done Custom", ToastLength.Short).Show();

            if (eggFrames.Count == 0)
            {
                //Toast.MakeText(this, "No matching frames found.", ToastLength.Short).Show();
                return;
            }

            //2. Perform silent short search on each result
            RNGPool.Clear();
            int index = SilentSearch7_EggShortestPath();
            //Toast.MakeText(this, "Done Silent", ToastLength.Short).Show();

            // Make this search act like normal frame-by-frame search
            //uiSearchData.searchParameters.targetFrame = tFrame;
            /*uiSearchData.searchParameters.type = SearchType.NormalSearch;
            currentSearchData = uiSearchData.Copy();
            currentSearchData.searchParameters.useFilter = true;
            currentSearchData.searchParameters.range = SearchRange.Simple;
            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);*/

            //3. Show search for fastest result

            /*int tFrame = eggFrames[index].FrameNum;
            uiSearchData.searchParameters.targetFrame = tFrame;
            uiSearchData.searchParameters.type = SearchType.ShortestPath;
            currentSearchData = uiSearchData.Copy();

            // Show Target frame in UI
            editTargetFrame.Text = tFrame.ToString();
            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);

            eggFrames.Clear();
            RNGPool.Clear();

            Search7_EggShortestPath();*/
        }

        private void Search7_MainEggRNG() {
            if (currentSearchData.profile.shinyCharm || currentSearchData.parents.isMasuda)
            {
                //Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.search_shortestpath_highframe), SearchConstants.MaximumTargetFrame), ToastLength.Short).Show();
                Toast.MakeText(this, "Main Egg RNG requires NO Shiny Charm and NO Masuda Method.", ToastLength.Short).Show();
                return;
            }

            // #DEBUG
            bool blinkOnly = false;
            bool safeOnly = false;

            //#TODO: Range search modes
            SFMT sfmt = new SFMT(currentSearchData.profile.initialSeed);
            int min = currentSearchData.searchParameters.mainRNG.minFrame;
            int max = currentSearchData.searchParameters.mainRNG.maxFrame;// min + SearchConstants.MaximumFramesPerSearch;
            //if (min > max)
                //return;
            // Blinkflag
            FuncUtil.getblinkflaglist(min, max, sfmt, currentSearchData.searchParameters.mainRNG.Modelnum);
            // Skip to min frame
            for (int i = 0; i < min; i++)
                sfmt.Next();
            // Prepare
            ModelStatus status = new ModelStatus(currentSearchData.searchParameters.mainRNG.Modelnum, sfmt);
            ModelStatus stmp = new ModelStatus(currentSearchData.searchParameters.mainRNG.Modelnum, sfmt);
            status.raining = stmp.raining = currentSearchData.searchParameters.mainRNG.Raining;

            PrepareMainEggRNGData(sfmt);

            int frameadvance;
            int realtime = 0;
            int frametime = 0;
            // Start
            for (int i = min; i <= max;)
            {
                do
                {
                    frameadvance = status.NextState();
                    realtime++;
                }
                while (frameadvance == 0); // Keep the starting status of a longlife frame(for npc=0 case)
                do
                {
                    RNGPool.CopyStatus(stmp);
                    var result = RNGPool.Generate7() as Result7;

                    RNGPool.AddNext(sfmt);

                    frameadvance--;
                    i++;
                    if (i > max + 1)
                        continue;
                    byte blinkflag = FuncUtil.blinkflaglist[i - min - 1];
                    if (blinkOnly && blinkflag < 4)
                        continue;
                    if (safeOnly && blinkflag >= 2)
                        continue;
                    if (currentSearchData.searchParameters.useFilter && !result.Shiny)
                    {
                        continue;
                    }
                    /*if (!filter.CheckResult(result))
                        continue;*/
                    eggFrames.Add(new G7EFrame(result as ResultME7, frame: i - 1, time: frametime * 2, blink: blinkflag));
                }
                while (frameadvance > 0);

                if (eggFrames.Count > maxResults)
                    return;
                // Backup current status
                status.CopyTo(stmp);
                frametime = realtime;
            }
        }

        // Use this function to find one specific seed without keeping track of previous seeds
        private G7EFrame SilentSearch7_Egg(EggSeed startSeed, int frame) {
            var rng = new TinyMT(startSeed.GetSeedVector());
            int min = (int)minFrame;
            int max = (int)maxFrame;

            // Advance
            for (int i = 0; i < min; i++)
                rng.Next();
            // Prepare
            //getsetting(rng);
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);

            ResultE7 result = null;
            // Start
            for (int i = 0; i <= frame; i++, RNGPool.AddNext(rng))
            {
                result = RNGPool.GenerateEgg7() as ResultE7;
                // Perform no filtering on silent search
            }

            result.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
            return new G7EFrame(result);
        }

        // Find the egg seed at #advances from the #current seed
        private EggSeed FindNextSeed(EggSeed current, int advances) {
            if ( advances < 0 ) { return current; }
            var res = SilentSearch7_Egg(current, advances);
            EggSeed newSeed = new EggSeed();
            newSeed.SetSeed(FuncUtil.SeedStr2Array(res.TinyState));
            return newSeed;
        }


        private EggRNG PrepareParentData() {
            var setting = (EggRNG)new Egg7();
            setting.FemaleIVs = (int[])currentSearchData.parents.femaleIV.Clone();
            setting.MaleIVs = (int[])currentSearchData.parents.maleIV.Clone();
            setting.MaleItem = (byte)currentSearchData.parents.maleItem;
            setting.FemaleItem = (byte)currentSearchData.parents.femaleItem;
            setting.ShinyCharm = currentSearchData.profile.shinyCharm;
            setting.TSV = (ushort)currentSearchData.profile.TSV;
            setting.Gender = FuncUtil.getGenderRatio(GenderConversion.ConvertGenderIndexToByte(currentSearchData.parents.genderCode));
            if (setting is Egg7 setting7)
            {
                setting7.Homogeneous = currentSearchData.parents.isSameDex;
                setting7.FemaleIsDitto = (currentSearchData.parents.whoIsDitto == 2);
            }
            setting.InheritAbility = (byte)(currentSearchData.parents.whoIsDitto == 2 ? currentSearchData.parents.maleAbility : currentSearchData.parents.femaleAbility);
            setting.MMethod = currentSearchData.parents.isMasuda;
            setting.NidoType = currentSearchData.parents.isNidoSpecies;

            setting.ConsiderOtherTSV = checkOtherTSV.Checked && (currentSearchData.profile.shinyCharm || currentSearchData.parents.isMasuda);
            setting.OtherTSVs = currentSearchData.otherTSVs.ToArray();

            setting.MarkItem();
            return setting;
        }

        private EggResult GenerateFirstEgg() {
            var rng = new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector());
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);

            return RNGPool.GenerateEgg7() as EggResult;
        }

        private void PrepareMainEggRNGData(SFMT sfmt) {

            bool IsUltra = GameVersionConversion.IsUltra(currentSearchData.profile.gameVersion);

            //RNGPool.CreateBuffer(new TinyMT(currentSearchData.profile.currentSeed.GetSeedVector()), 50);
            ResultME7.Egg = GenerateFirstEgg();

            // Determine Shift/F for target frame
            //int standard = FuncUtil.CalcFrame(0, 485, 5000, ?, ?, ?)[0] * 2;

            RNGPool.igenerator = new MainEggRNG()
            {
                TSV = currentSearchData.profile.TSV,
                ConsiderOtherTSV = currentSearchData.searchParameters.checkOtherTSV,
                OtherTSVs = currentSearchData.otherTSVs.ToArray()
            };

            //#TODO: Use interface parameters
            RNGPool.modelnumber = currentSearchData.searchParameters.mainRNG.Modelnum;
            RNGPool.Considerdelay = currentSearchData.searchParameters.mainRNG.considerDelay;
            RNGPool.DelayTime = (int)(currentSearchData.searchParameters.mainRNG.delay / 2) + 2;
            RNGPool.raining = currentSearchData.searchParameters.mainRNG.Raining;
            RNGPool.PreHoneyCorrection = 0;//(int)Correction.Value;
            RNGPool.HoneyDelay = IsUltra ? 63 : 93;
            RNGPool.ultrawild = false;//IsUltra && Method == 2;

            int buffersize = 150;
            // if timeline: buffersize += npcModelNum * 20;
            if (RNGPool.Considerdelay)
            {
                buffersize += RNGPool.modelnumber * RNGPool.DelayTime;
            }
            RNGPool.CreateBuffer(sfmt, buffersize);
        }


        private void LoadProfileData() {
            uiSearchData.parents = ParentData.LoadParentData(this);

            uiSearchData.filter = FilterData.LoadFilterData(this);

            uiSearchData.preferences = AppPreferences.LoadPreferencesData(this);

            uiSearchData.otherTSVs = MiscUtility.LoadCurrentTSVs(this);

            uiSearchData.profile = ProfileData.LoadCurrentProfileData(this);

            maxResults = uiSearchData.preferences.MaxResultValue();

            ShowProfileData();
        }

        private void SaveSeedData()
        {
            ProfileData.SaveCurrentSeed(this, uiSearchData.profile);

            ShowProfileData();
        }
        private void ShowProfileData() {
            currentSeedDump.Text = uiSearchData.profile.profileTag + "  " + PokeRNGApp.Strings.profileinfoseed + " " + uiSearchData.profile.currentSeed.GetSeedToString();
            userTSVDump.Text = "TSV=" + uiSearchData.profile.TSV.ToString("0000");
        }

        private void SetupResultMenu(PopupMenu menu, int frameIndex) {
            if (currentSearchData.searchParameters.type == SearchType.NormalSearch)
            {
                PrepareNormalSearchMenu(menu, frameIndex);
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAccept)
            {
                PrepareEggAcceptMenu(menu, frameIndex);
            }
            else if (currentSearchData.searchParameters.type == SearchType.EggAcceptPlus)
            {
                PrepareEggAcceptPlusMenu(menu, frameIndex);
            }
            else if (currentSearchData.searchParameters.type == SearchType.ShortestPath)
            {
                PrepareShortestPathMenu(menu, frameIndex);
            }
            else if (currentSearchData.searchParameters.type == SearchType.LeastAdvances) {
                PrepareEggLeastAdvanceMenu(menu, frameIndex);
            }
        }

        private void PrepareNormalSearchMenu(PopupMenu menu, int frameIndex) {
            
            menu.Inflate(Resource.Menu.EggResultMenu);
            string titleAccept = String.Format(Resources.GetString(Resource.String.search_popup_accept), eggFrames[frameIndex].FrameNum);
            string titleReject = String.Format(Resources.GetString(Resource.String.search_popup_reject), eggFrames[frameIndex].FrameNum);
            menu.Menu.FindItem(Resource.Id.action_accept).SetTitle(titleAccept);
            menu.Menu.FindItem(Resource.Id.action_reject).SetTitle(titleReject);

            menu.MenuItemClick += (s1, arg1) =>
            {
                int realFrameNum = eggFrames[frameIndex].FrameNum;
                if (arg1.Item.ItemId == Resource.Id.action_accept)
                {
                    int advance = eggFrames[frameIndex].egg.FramesUsed;
                    int nextFrameNum = realFrameNum + advance;

                    int listIndex = eggFrames.FindIndex(x => x.FrameNum == nextFrameNum);
                    if (listIndex >= 0)
                    {
                        uiSearchData.profile.currentSeed.SetSeed(eggFrames[listIndex].TinyState);
                        SaveSeedData();

                        resTab.SetSelection(listIndex);

                        // Jump message
                        string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, false, false, 0, false);
                        Toast.MakeText(this, wString, ToastLength.Long).Show();
                    }
                    else
                    {
                        EggSeed selectionSeed = new EggSeed();
                        selectionSeed.SetSeed(eggFrames[frameIndex].TinyState);
                        uiSearchData.profile.currentSeed = FindNextSeed(selectionSeed, advance);
                        SaveSeedData();

                        // Outside search
                        if (nextFrameNum > eggFrames[eggFrames.Count - 1].FrameNum)
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, false, false, 2, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                                //string wString = String.Format(Resources.GetString(Resource.String.search_popup_accepted_exceedauto), realFrameNum, nextFrameNum);
                                //Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, false, false, 2, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept outside frame message
                        }
                        // Filtered
                        else
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, false, false, 1, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, false, false, 1, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept filtered frame message
                        }
                    }
                }
                else if (arg1.Item.ItemId == Resource.Id.action_reject)
                {
                    int advance = 1;
                    int nextFrameNum = realFrameNum + 1;

                    int listIndex = eggFrames.FindIndex(x => x.FrameNum == nextFrameNum);
                    if (listIndex >= 0)
                    {
                        uiSearchData.profile.currentSeed.SetSeed(eggFrames[listIndex].TinyState);
                        SaveSeedData();

                        resTab.SetSelection(listIndex);

                        // Jump message
                        string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, true, false, 0, false);
                        Toast.MakeText(this, wString, ToastLength.Long).Show();
                    }
                    else
                    {
                        EggSeed selectionSeed = new EggSeed();
                        selectionSeed.SetSeed(eggFrames[frameIndex].TinyState);
                        uiSearchData.profile.currentSeed = FindNextSeed(selectionSeed, advance);
                        SaveSeedData();

                        // Outside search
                        if (nextFrameNum > eggFrames[eggFrames.Count - 1].FrameNum)
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, true, false, 2, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, true, false, 2, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept outside frame message
                        }
                        // Filtered
                        else
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, true, false, 1, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(realFrameNum, nextFrameNum, false, true, false, 1, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept filtered frame message
                        }
                    }
                }
                else if (arg1.Item.ItemId == Resource.Id.action_copyseed) {
                    string sString = eggFrames[frameIndex].TinyState;

                    var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", sString);

                    string wString = Resources.GetString(Resource.String.search_popup_toast_copyseed);
                    Toast.MakeText(this, wString + "\n" + sString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Seed copied to clipboard\n" + sString, ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_setseed)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    string wString = String.Format(Resources.GetString(Resource.String.search_popup_toast_setcurrentseed), realFrameNum );
                    Toast.MakeText(this, wString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Setting current seed from egg " + realFrameNum + ".", ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_target){
                    editTargetFrame.Text = eggFrames[frameIndex].FrameNum.ToString();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_shortest)
                {
                    uiSearchData.searchParameters.type = SearchType.ShortestPath;
                    uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                    searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                    PerformSearch();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_detailed)
                {
                    SpawnDetailedDialog(eggFrames[frameIndex]);
                }
            };
        }

        public void PrepareEggAcceptMenu(PopupMenu menu, int frameIndex) {
            menu.Inflate(Resource.Menu.EggResultMenu);
            string titleAccept = String.Format(Resources.GetString(Resource.String.search_popup_acceptegg), eggFrames[frameIndex].EggNum);
            string titleReject = String.Format(Resources.GetString(Resource.String.search_popup_rejectegg), eggFrames[frameIndex].EggNum);
            menu.Menu.FindItem(Resource.Id.action_accept).SetTitle(titleAccept);
            menu.Menu.FindItem(Resource.Id.action_reject).SetTitle(titleReject);

            menu.MenuItemClick += (s1, arg1) =>
            {
                int eggNum = eggFrames[frameIndex].EggNum;
                int realFrameNum = eggFrames[frameIndex].FrameNum;
                if (arg1.Item.ItemId == Resource.Id.action_accept)
                {
                    int advance = eggFrames[frameIndex].egg.FramesUsed;
                    int nextFrameNum = realFrameNum + advance;

                    int listIndex = eggFrames.FindIndex(x => x.FrameNum == nextFrameNum);
                    if (listIndex >= 0)
                    {
                        uiSearchData.profile.currentSeed.SetSeed(eggFrames[listIndex].TinyState);
                        SaveSeedData();

                        resTab.SetSelection(listIndex);

                        // Jump message
                        string wString = BuildEggMessage(eggNum, eggNum+1, true, false, true, 0, false);
                        Toast.MakeText(this, wString, ToastLength.Long).Show();
                    }
                    else
                    {
                        EggSeed selectionSeed = new EggSeed();
                        selectionSeed.SetSeed(eggFrames[frameIndex].TinyState);
                        uiSearchData.profile.currentSeed = FindNextSeed(selectionSeed, advance);
                        SaveSeedData();

                        // Outside search
                        if (nextFrameNum > eggFrames[eggFrames.Count - 1].FrameNum)
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(eggNum, eggNum + 1, true, false, true, 2, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(eggNum, eggNum + 1, true, false, true, 2, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept outside frame message
                        }
                        // Filtered
                        else
                        {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                PerformSearch();

                                string wString = BuildEggMessage(eggNum, eggNum + 1, true, false, true, 1, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            else
                            {
                                string wString = BuildEggMessage(eggNum, eggNum + 1, true, false, true, 1, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                            }
                            // Accept filtered frame message
                        }
                    }
                }
                else if (arg1.Item.ItemId == Resource.Id.action_reject)
                {
                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - 1 - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    // Find out the seed for the next egg
                    EggSeed selectionSeed = new EggSeed();
                    selectionSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    uiSearchData.profile.currentSeed = FindNextSeed(selectionSeed, 1);
                    SaveSeedData();

                    int nextFrameNum = eggFrames[frameIndex].FrameNum + 1;
                    if (uiSearchData.preferences.autoSearch)
                    {
                        // Perform search
                        PerformSearch();

                        string wString = BuildEggMessage(eggNum, realFrameNum + 1, true, false, false, 0, true);
                        Toast.MakeText(this, wString, ToastLength.Long).Show();
                        //Toast.MakeText(this, "Rejected egg " + eggNum + ".\nSetting seed from frame " + nextFrameNum + "\nRe-doing search from new egg.", ToastLength.Long).Show();
                    }
                    else
                    {
                        string wString = BuildEggMessage(eggNum, realFrameNum + 1, true, false, false, 0, false);
                        Toast.MakeText(this, wString, ToastLength.Long).Show();
                        //Toast.MakeText(this, "Rejected egg " + eggNum + ".\nSetting seed from frame " + nextFrameNum + ".", ToastLength.Long).Show();
                    }
                }
                else if (arg1.Item.ItemId == Resource.Id.action_copyseed)
                {
                    string sString = eggFrames[frameIndex].TinyState;

                    var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", sString);

                    string wString = Resources.GetString(Resource.String.search_popup_toast_copyseed);
                    Toast.MakeText(this, wString + "\n" + sString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Seed copied to clipboard\n" + sString, ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_setseed)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    string wString = String.Format(Resources.GetString(Resource.String.search_popup_toast_setcurrentseed), realFrameNum);
                    Toast.MakeText(this, wString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Setting current seed from egg " + realFrameNum + ".", ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_target)
                {
                    editTargetFrame.Text = eggFrames[frameIndex].FrameNum.ToString();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_shortest)
                {
                    uiSearchData.searchParameters.type = SearchType.ShortestPath;
                    uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                    searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                    PerformSearch();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_detailed)
                {
                    SpawnDetailedDialog(eggFrames[frameIndex]);
                }
            };
        }

        public void PrepareEggAcceptPlusMenu(PopupMenu menu, int frameIndex) {
            bool isRej = eggFrames[frameIndex].AdvType == AdvanceType.Reject;
            string acceptString = (isRej ?
                String.Format(Resources.GetString(Resource.String.search_popup_acceptframe), eggFrames[frameIndex].FrameNum) :
                String.Format(Resources.GetString(Resource.String.search_popup_acceptegg), eggFrames[frameIndex].EggNum));
            string rejectString = (isRej ?
                String.Format(Resources.GetString(Resource.String.search_popup_rejectframe), eggFrames[frameIndex].FrameNum) :
                String.Format(Resources.GetString(Resource.String.search_popup_rejectegg), eggFrames[frameIndex].EggNum));
            menu.Menu.Add(Menu.None, 1, 1, acceptString );
            menu.Menu.Add(Menu.None, 2, 2, rejectString );
            menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.search_popup_copyseed));
            menu.Menu.Add(Menu.None, 4, 4, Resources.GetString(Resource.String.search_popup_currentseed));
            menu.Menu.Add(Menu.None, 5, 5, Resources.GetString(Resource.String.search_popup_targetframe));
            menu.Menu.Add(Menu.None, 6, 6, Resources.GetString(Resource.String.search_popup_shortestpath));
            menu.Menu.Add(Menu.None, 7, 7, Resources.GetString(Resource.String.search_popup_viewdetailed));

            menu.MenuItemClick += (s1, arg1) =>
            {
                int realFrameNum = eggFrames[frameIndex].FrameNum;
                int selectedID = isRej ? eggFrames[frameIndex].FrameNum : eggFrames[frameIndex].EggNum;
                if (arg1.Item.ItemId == 1)
                {
                    int advance = eggFrames[frameIndex].egg.FramesUsed;

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - advance - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    EggSeed selectSeed = new EggSeed();
                    selectSeed.SetSeed(eggFrames[frameIndex].TinyState);
                    
                    int targetFrame = eggFrames[frameIndex].FrameNum + advance;
                    uiSearchData.profile.currentSeed = FindNextSeed(selectSeed, advance);
                    SaveSeedData();
                    // Perform search automatically if accepting isRej

                    G7EFrame targetFrameObj = null;
                    bool isTargetRej = false;
                    bool isOutOfSearch = false;
                    int targetIndex = -1;
                    if (targetFrame > eggFrames[eggFrames.Count - 1].FrameNum) { isOutOfSearch = true; }
                    else {
                        for (int n = frameIndex + 1; n < eggFrames.Count; ++n) {
                            // If we're past the target frame, stop searching
                            if (eggFrames[n].FrameNum > targetFrame) { break; }
                            if (eggFrames[n].FrameNum == targetFrame) {
                                targetFrameObj = eggFrames[n];
                                isTargetRej = eggFrames[n].AdvType == AdvanceType.Reject;
                                targetIndex = n;
                                break;
                            }
                        }
                    }

                    if (isOutOfSearch)
                    {
                        if (uiSearchData.preferences.autoSearch)
                        {
                            uiSearchData.searchParameters.type = SearchType.EggAcceptPlus;
                            //uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                            //editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                            PerformSearch();

                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 2, true);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();

                            //Toast.MakeText(this, "Accepting " + eggId + ". Setting current seed to Egg frame No." + targetFrame.ToString() + ". Frame outside of search limits. Redoing search from new egg.", ToastLength.Long).Show();
                        }
                        else
                        {
                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 2, false);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Accepting " + eggId + ". Setting current seed to Egg frame No." + targetFrame.ToString() + ". Frame is out of this search.", ToastLength.Long).Show();
                        }
                    }
                    else if (targetFrameObj == null)
                    {
                        if (uiSearchData.preferences.autoSearch)
                        {
                            uiSearchData.searchParameters.type = SearchType.EggAcceptPlus;
                            //uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                            //editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                            PerformSearch();

                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 1, true);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();

                            //Toast.MakeText(this, "Accepting " + eggId + ". Setting current seed to Egg frame No." + targetFrame.ToString() + ". Egg not in search, redoing search.", ToastLength.Long).Show();
                        }
                        else
                        {
                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 1, false);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Accepting " + eggId + ". Setting current seed to Egg frame No." + targetFrame.ToString() + ". Frame is out of this search.", ToastLength.Long).Show();
                        }
                    }
                    else {
                        if (isTargetRej) {
                            if (uiSearchData.preferences.autoSearch)
                            {
                                uiSearchData.searchParameters.type = SearchType.EggAcceptPlus;
                                //uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                                searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                                //editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                                PerformSearch();

                                string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 0, true);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                                //Toast.MakeText(this, "Accepting " + eggId + ". Redoing search from Frame= " + targetFrameObj.FrameNum + ".", ToastLength.Long).Show();
                            }
                            else {
                                resTab.SetSelection(targetIndex);
                                string wString = BuildEggMessage(selectedID, targetFrame, !isRej, false, false, 0, false);
                                Toast.MakeText(this, wString, ToastLength.Long).Show();
                                //Toast.MakeText(this, "Accepting " + eggId + ". New frame " + targetFrameObj.FrameNum + " is one of the reject eggs. Redo search for new results.", ToastLength.Long).Show();
                            }
                        }
                        else {
                            // Jump to new egg if in search
                            resTab.SetSelection(targetIndex);

                            string wString = BuildEggMessage(selectedID, targetFrameObj.EggNum, !isRej, false, true, 0, false);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Accepting " + eggId + ". Jumping to Egg " + targetFrameObj.EggNum + ".", ToastLength.Long).Show();
                        }
                    }
                }
                else if (arg1.Item.ItemId == 2)
                {
                    EggSeed selectSeed = new EggSeed();
                    selectSeed.SetSeed(eggFrames[frameIndex].TinyState);

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - 1 - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    int targetFrame = eggFrames[frameIndex].FrameNum + 1;
                    uiSearchData.profile.currentSeed = FindNextSeed(selectSeed, 1);
                    SaveSeedData();

                    G7EFrame targetFrameObj = null;
                    bool isTargetRej = false;
                    bool isOutOfSearch = false;
                    int targetIndex = -1;
                    if (targetFrame > eggFrames[eggFrames.Count - 1].FrameNum) { isOutOfSearch = true; }
                    else
                    {
                        for (int n = frameIndex + 1; n < eggFrames.Count; ++n)
                        {
                            // If we're past the target frame, stop searching
                            if (eggFrames[n].FrameNum > targetFrame) { break; }
                            if (eggFrames[n].FrameNum == targetFrame)
                            {
                                targetFrameObj = eggFrames[n];
                                isTargetRej = eggFrames[n].AdvType == AdvanceType.Reject;
                                targetIndex = n;
                                break;
                            }
                        }
                    }

                    if (targetFrameObj == null)
                    {
                        if (uiSearchData.preferences.autoSearch) {
                            uiSearchData.searchParameters.type = SearchType.EggAcceptPlus;
                            //uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                            //editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                            PerformSearch();


                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, true, false, 2, true);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Rejected " + eggId + ". Redoing search from Frame " + targetFrame + ".", ToastLength.Long).Show();
                        }
                        else
                        {
                            resTab.SetSelection(targetIndex);
                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, true, false, 2, false);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Rejected " + eggId + ". New frame " + targetFrame + " not in search. Redo search for new results.", ToastLength.Long).Show();
                        }
                    }
                    else {
                        if (uiSearchData.preferences.autoSearch)
                        {
                            uiSearchData.searchParameters.type = SearchType.EggAcceptPlus;
                            //uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                            searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                            //editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                            PerformSearch();

                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, true, false, 0, true);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Rejected " + eggId + ". Redoing search from Frame " + targetFrameObj.FrameNum + ".", ToastLength.Long).Show();
                        }
                        else
                        {
                            resTab.SetSelection(targetIndex);

                            string wString = BuildEggMessage(selectedID, targetFrame, !isRej, true, false, 0, false);
                            Toast.MakeText(this, wString, ToastLength.Long).Show();
                            //Toast.MakeText(this, "Rejected " + eggId + ". New frame " + targetFrameObj.FrameNum + " not in search. Redo search for new results.", ToastLength.Long).Show();
                        }
                    }
                }
                else if (arg1.Item.ItemId == 3)
                {
                    string sString = eggFrames[frameIndex].TinyState;

                    var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", sString);

                    string wString = Resources.GetString(Resource.String.search_popup_toast_copyseed);
                    Toast.MakeText(this, wString + "\n" + sString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Seed copied to clipboard\n" + sString, ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == 4)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    string wString = String.Format(Resources.GetString(Resource.String.search_popup_toast_setcurrentseed), realFrameNum);
                    Toast.MakeText(this, wString, ToastLength.Long).Show();
                    //Toast.MakeText(this, "Setting current seed from egg " + realFrameNum + ".", ToastLength.Short).Show();
                }
                else if (arg1.Item.ItemId == 5)
                {
                    editTargetFrame.Text = eggFrames[frameIndex].FrameNum.ToString();
                }
                else if (arg1.Item.ItemId == 6)
                {
                    uiSearchData.searchParameters.type = SearchType.ShortestPath;
                    uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                    searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                    PerformSearch();
                }
                else if (arg1.Item.ItemId == 7)
                {
                    SpawnDetailedDialog(eggFrames[frameIndex]);
                }
            };
        }

        public void PrepareShortestPathMenu(PopupMenu menu, int frameIndex) {
            menu.Inflate(Resource.Menu.ShortestEggResultMenu);
            menu.Menu.FindItem(Resource.Id.action_searchfrom).SetTitle(String.Format(Resources.GetString(Resource.String.search_popup_searchfromnum), eggFrames[frameIndex].EggNum));

            menu.MenuItemClick += (s1, arg1) =>
            {
                if (arg1.Item.ItemId == Resource.Id.action_searchfrom)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    uiSearchData.searchParameters.type = SearchType.ShortestPath;
                    uiSearchData.searchParameters.targetFrame = eggFrames[eggFrames.Count - 1].FrameNum - eggFrames[frameIndex].FrameNum;

                    searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                    PerformSearch();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_searchfromnext)
                {
                    if (frameIndex == eggFrames.Count - 1)
                    {
                        EggSeed nextSeed = new EggSeed();
                        nextSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));

                        var cframe = SilentSearch7_Egg(nextSeed, eggFrames[frameIndex].egg.FramesUsed);

                        uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(cframe.TinyState));
                        SaveSeedData();

                        uiSearchData.searchParameters.type = SearchType.NormalSearch;
                        uiSearchData.searchParameters.targetFrame = 0;

                        searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                        editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                        PerformSearch();

                        Toast.MakeText(this, Resources.GetString(Resource.String.search_toast_shortpathcomplete), ToastLength.Short).Show();
                    }
                    else
                    {
                        uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex + 1].TinyState));
                        SaveSeedData();

                        uiSearchData.searchParameters.type = SearchType.ShortestPath;
                        uiSearchData.searchParameters.targetFrame = eggFrames[eggFrames.Count - 1].FrameNum - eggFrames[frameIndex + 1].FrameNum;

                        searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                        editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                        PerformSearch();
                    }
                }
                else if (arg1.Item.ItemId == Resource.Id.action_copyseed)
                {
                    string sString = eggFrames[frameIndex].TinyState;

                    var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", sString);

                    string wString = Resources.GetString(Resource.String.search_popup_toast_copyseed);
                    Toast.MakeText(this, wString + "\n" + sString, ToastLength.Long).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_setseed)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    string wString = String.Format(Resources.GetString(Resource.String.search_popup_toast_setcurrentseed), eggFrames[frameIndex].FrameNum);
                    Toast.MakeText(this, wString, ToastLength.Long).Show();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_target)
                {
                    editTargetFrame.Text = eggFrames[frameIndex].FrameNum.ToString();
                }
                else if (arg1.Item.ItemId == Resource.Id.action_detailed) {
                    SpawnDetailedDialog(eggFrames[frameIndex]);
                }
            };
        }

        public void PrepareEggLeastAdvanceMenu(PopupMenu menu, int frameIndex) {

            menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.search_popup_copyseed));
            menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.search_popup_currentseed));
            menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.search_popup_targetframe));
            menu.Menu.Add(Menu.None, 4, 4, Resources.GetString(Resource.String.search_popup_shortestpath));
            menu.Menu.Add(Menu.None, 5, 5, Resources.GetString(Resource.String.search_popup_viewdetailed));

            menu.MenuItemClick += (s1, arg1) =>
            {
                int frameNum = eggFrames[frameIndex].FrameNum;

                if (arg1.Item.ItemId == 1)
                {
                    string sString = eggFrames[frameIndex].TinyState;

                    var clipboard = (Android.Content.ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", sString);

                    string wString = Resources.GetString(Resource.String.search_popup_toast_copyseed);
                    Toast.MakeText(this, wString + "\n" + sString, ToastLength.Long).Show();
                }
                else if (arg1.Item.ItemId == 2)
                {
                    uiSearchData.profile.currentSeed.SetSeed(FuncUtil.SeedStr2Array(eggFrames[frameIndex].TinyState));
                    SaveSeedData();

                    uiSearchData.searchParameters.targetFrame = Math.Max(currentSearchData.searchParameters.targetFrame - eggFrames[frameIndex].FrameNum, 0);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();

                    string wString = String.Format(Resources.GetString(Resource.String.search_popup_toast_setcurrentseed), frameNum);
                    Toast.MakeText(this, wString, ToastLength.Long).Show();
                }
                else if (arg1.Item.ItemId == 3)
                {
                    editTargetFrame.Text = eggFrames[frameIndex].FrameNum.ToString();
                }
                else if (arg1.Item.ItemId == 4)
                {
                    uiSearchData.searchParameters.type = SearchType.ShortestPath;
                    uiSearchData.searchParameters.targetFrame = eggFrames[frameIndex].FrameNum;

                    searchParamSpinner.SetSelection((int)uiSearchData.searchParameters.type);
                    editTargetFrame.Text = uiSearchData.searchParameters.targetFrame.ToString();
                    PerformSearch();
                }
                else if (arg1.Item.ItemId == 5)
                {
                    SpawnDetailedDialog(eggFrames[frameIndex]);
                }
                /*menu.Menu.Add(Menu.None, 3, 3, "Copy seed");
                menu.Menu.Add(Menu.None, 4, 4, "Set current seed");
                menu.Menu.Add(Menu.None, 5, 5, "Set target frame");
                menu.Menu.Add(Menu.None, 6, 6, "Shortest Path");
                menu.Menu.Add(Menu.None, 7, 7, "View Detailed");*/
            };
        }

        private void SpawnDetailedDialog(G7EFrame eggFrame) {
            DetailedFrame detailDialog = new DetailedFrame(eggFrame, currentSearchData, this);
            detailDialog.Initialize();
            detailDialog.Show();
        }


        // Presence (0 = In Search, 1 = Filtered, 2 = Out of search)
        public string BuildEggMessage(int last, int next, bool isEgg = false, bool isReject = false, bool nextIsEgg = false, int presence = 0, bool isAuto = false)
        {
            string wString = "";
            if (isEgg)
            {
                wString += String.Format(Resources.GetString(isReject ? Resource.String.search_popup_toast_rejectedegg : Resource.String.search_popup_toast_acceptedegg), last);
            }
            else {
                wString += String.Format(Resources.GetString(isReject ? Resource.String.search_popup_toast_rejectedframe : Resource.String.search_popup_toast_acceptedframe), last);
            }

            wString += "\n";
            if (presence == 0) {
                if (nextIsEgg)
                {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_setnewegg), next);
                }
                else {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_setnewframe), next);
                }
            }
            else if (presence == 1)
            {
                if (nextIsEgg)
                {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_filteregg), next);
                }
                else
                {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_filterframe), next);
                }
            }
            else if (presence == 2)
            {
                if (nextIsEgg)
                {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_eggexceeds), next);
                }
                else
                {
                    wString += String.Format(Resources.GetString(Resource.String.search_popup_toast_frameexceeds), next);
                }
            }

            if (isAuto) {
                wString += "\n";
                if (nextIsEgg)
                {
                    wString += Resources.GetString(Resource.String.search_popup_toast_eggredo);
                }
                else {
                    wString += Resources.GetString(Resource.String.search_popup_toast_frameredo);
                }
            }

            return wString;
        }

        private void HideMainEggBar() {
            mainEggRes.RemoveAllViews();
            mainEggRes.Visibility = ViewStates.Gone;
        }
        private void ShowMainEggBar() {
            mainEggRes.Visibility = ViewStates.Visible;
        }

        private void BuildMainEggResult(G7EFrame frame, EggResultAdapter adapter) {
            LayoutInflater layoutInflater = LayoutInflater.FromContext(this);
            var view = layoutInflater.Inflate(Resource.Layout.ResultItemNormal, mainEggRes, true);

            var holder = new ResultNormalHolder();
            holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.resLayout);
            holder.reslayout.SetGravity(GravityFlags.CenterVertical);
            /*ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
            lparams.Height += rowExtraHeight;
            holder.reslayout.LayoutParameters = lparams;*/
            holder.frameView = (TextView)view.FindViewById(Resource.Id.resFrame);
            holder.advView = (TextView)view.FindViewById(Resource.Id.resAdvance);
            holder.genderView = (TextView)view.FindViewById(Resource.Id.resGender);
            holder.natureView = (TextView)view.FindViewById(Resource.Id.resNature);
            holder.abilityView = (TextView)view.FindViewById(Resource.Id.resAbility);
            holder.ballView = (TextView)view.FindViewById(Resource.Id.resBall);
            holder.hpView = (TextView)view.FindViewById(Resource.Id.resHP);
            holder.atkView = (TextView)view.FindViewById(Resource.Id.resAtk);
            holder.defView = (TextView)view.FindViewById(Resource.Id.resDef);
            holder.spaView = (TextView)view.FindViewById(Resource.Id.resSpA);
            holder.spdView = (TextView)view.FindViewById(Resource.Id.resSpD);
            holder.speView = (TextView)view.FindViewById(Resource.Id.resSpe);
            holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.resHiddenP);
            holder.tsvView = (TextView)view.FindViewById(Resource.Id.resTSV);

            holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

            adapter.FillNormalHolder(frame, holder);
        }

        private void VerifySearchConstraints(SearchType type)
        {
            if (type == SearchType.MainEggRNG) {
                // Automatically Reset Search Parameters
                uiSearchData.searchParameters.mainRNG.minFrame = GetBestStartingFrame();
                uiSearchData.searchParameters.mainRNG.considerDelay = true;
                uiSearchData.searchParameters.mainRNG.delay = 38;
                uiSearchData.searchParameters.mainRNG.npcs = 4;
                uiSearchData.searchParameters.mainRNG.ctimeline = 3600;
                uiSearchData.searchParameters.mainRNG.timeleap1 = 1;
                uiSearchData.searchParameters.mainRNG.timeleap2 = 3;

                // UI modifications
            }
        }

        private int GetBestStartingFrame() {
            return Math.Max(uiSearchData.profile.seedFrame, GameVersionConversion.GetGameStartingFrame(uiSearchData.profile.gameVersion, false));
        }
        private int GetBestStartingFrame(int frame)
        {
            return Math.Max(frame, GameVersionConversion.GetGameStartingFrame(uiSearchData.profile.gameVersion, false));
        }

        /*private void SetCurrentSeed(EggSeed seed) {
            currentSeed = seed;
        }*/
    }
}

