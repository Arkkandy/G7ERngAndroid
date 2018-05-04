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
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{

    public class ListViewHandler : Handler
    {
        private ArrayAdapter<string> adapter;
        public ListViewHandler(ArrayAdapter<string> ad) { adapter = ad; }

        public override void HandleMessage(Message msg)
        {
            //base.HandleMessage(msg);
            string val = (string)msg.Obj;

            if (msg.What == 1)
            {
                adapter.Add(val);
                adapter.NotifyDataSetChanged();
            }
        }
    };

    [Activity(Label = "@string/activity_tinyseed",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class TinyFinderActivity : Activity
    {

        private enum TinyFinderState {
            Inputting,
            Ready,
            SaveStateReady,
            Active,
            Paused,
            Finished
        }

        TinyFinderState currentProcessingState;

        TextView natureSequence;
        ListView natureSelection;
        ListView results;
        ArrayAdapter<string> resultAdapter;

        Button findButton;
        Button abortButton;
        ImageButton recoveryHelp;
        ImageButton backspaceButton;

        CheckBox tinyShinyCharm;

        ProgressBar tinyProgress;
        TextView tinyClock;

        List<uint> natures;
        List<uint> searchNatures;

        object resultLock = new object();
        List<string> resultList;

        SmartTinyFinder stfinder;
        ListViewHandler resultHandler;

        int[] natureIndices;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.TinyFinderLayout);

            natures = new List<uint>();
            searchNatures = new List<uint>();

            // Fetch Widgets
            natureSequence = (TextView)FindViewById(Resource.Id.tinyNatureSequence);
            natureSelection = (ListView)FindViewById(Resource.Id.tinyNatureSelection);
            results = (ListView)FindViewById(Resource.Id.tinyResults);
            findButton = (Button)FindViewById(Resource.Id.tinyFind);
            recoveryHelp = (ImageButton)FindViewById(Resource.Id.tinyHelp);
            tinyShinyCharm = (CheckBox)FindViewById(Resource.Id.tinyShinyCharm);
            backspaceButton = (ImageButton)FindViewById(Resource.Id.tinyBackspace);
            tinyProgress = (ProgressBar)FindViewById(Resource.Id.tinyProgress);
            tinyClock = (TextView)FindViewById(Resource.Id.tinyClock);
            abortButton = (Button)FindViewById(Resource.Id.tinyAbortButton);

            // Assign functions
            string[] natureStrings = Resources.GetStringArray(Resource.Array.NatureIndexed);
            natureIndices = ArrayUtil.SortArrayIndex(natureStrings);
            for (int i = 0; i < natureStrings.Length; ++i)
            {
                natureStrings[i] += " - " + natureIndices[i];
            }
            natureSelection.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, natureStrings);
            natureSelection.ItemClick += (sender, args) => {
                if (currentProcessingState == TinyFinderState.Inputting)
                {
                    AddNature((uint)natureIndices[args.Position]);
                }
            };

            resultList = new List<string>();
            resultAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new List<string>());
            results.Adapter = resultAdapter;

            findButton.Click += delegate {
                /*if (currentProcessingState == TinyFinderState.Inputting) {
                    //Do nothing
                }*/
                if (currentProcessingState == TinyFinderState.Ready)
                {
                    searchNatures = natures.ConvertAll(x => x);
                    resultList.Clear();
                    resultAdapter.Clear();
                    StartTinySearch();
                    SetTFState(TinyFinderState.Active);
                }
                else if (currentProcessingState == TinyFinderState.SaveStateReady)
                {
                    // Search Natures already been set
                    StartTinySearch();
                    SetTFState(TinyFinderState.Active);
                }
                else if (currentProcessingState == TinyFinderState.Active)
                {
                    PauseSearch();
                    SetTFState(TinyFinderState.Paused);
                }
                else if (currentProcessingState == TinyFinderState.Paused)
                {
                    ResumeSearch();
                    SetTFState(TinyFinderState.Active);
                }
            };

            abortButton.Click += delegate {
                if (currentProcessingState == TinyFinderState.Inputting) {
                    natures.Clear();
                    UpdateSequenceView();
                }
                if (currentProcessingState == TinyFinderState.Ready)
                {
                    natures.Clear();
                    UpdateSequenceView();
                    SetTFState(TinyFinderState.Inputting);
                }
                else if (currentProcessingState == TinyFinderState.SaveStateReady)
                {
                    UpdateSequenceView();
                    EraseData();
                    SetTFState(TinyFinderState.Ready);
                }
                else if (currentProcessingState == TinyFinderState.Active)
                {
                    PauseSearch();
                    stfinder.Abort();
                    EraseData();
                    SetTFState(TinyFinderState.Ready);
                }
                else if (currentProcessingState == TinyFinderState.Paused)
                {
                    stfinder.Abort();
                    EraseData();
                    SetTFState(TinyFinderState.Ready);
                }
                else if (currentProcessingState == TinyFinderState.Finished)
                {
                    EraseData();
                    SetTFState(TinyFinderState.Ready);
                }
            };

            tinyProgress.Max = TinyWorkProducer.MaxValue - 1;
            tinyProgress.Progress = 0;

            backspaceButton.Click += delegate {
                if (currentProcessingState == TinyFinderState.Inputting || currentProcessingState == TinyFinderState.Ready)
                {
                    RemoveNature();
                }
            };

            recoveryHelp.Click += delegate {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle(Resources.GetString(Resource.String.seed_tiny_helptitle));
                alert.SetMessage(Resources.GetString(Resource.String.seed_tiny_helpmessage));

                alert.SetPositiveButton(Resources.GetString(Resource.String.okthanks), delegate { });
                alert.Show();
            };

            results.ItemLongClick += (sender, args) => {
                PopupMenu menu = new PopupMenu(this, results.GetChildAt(args.Position), Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.seed_tiny_copyseed));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.profile_setseed));
                if (resultList.Count > 1)
                {
                    menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.profile_copyall));
                }

                menu.MenuItemClick += (msender, margs) => {

                    if (margs.Item.ItemId == 1)
                    {
                        // Copy checkpoint
                        var seed = resultList[args.Position];

                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("EggSeed", seed);
                        
                        Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.profile_copiedresultseed), seed), ToastLength.Short).Show();
                    }
                    else if (margs.Item.ItemId == 2) {
                        string seed = resultList[args.Position];
                        Intent returnIntent = new Intent();
                        returnIntent.PutExtra("NewSeed", seed);
                        SetResult(Result.Ok, returnIntent);
                        Finish();
                    }
                    else if (margs.Item.ItemId == 3)
                    {
                        string allSeeds = "";
                        for (int i = 0; i < resultList.Count; ++i)
                        {
                            if (i > 0)
                            {
                                allSeeds += "\n";
                            }
                            allSeeds += resultList[i];
                        }

                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("", allSeeds);
                        
                        Toast.MakeText(this, Resources.GetString(Resource.String.profile_copiedall), ToastLength.Long).Show();
                    }
                };

                menu.Show();
            };

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            tinyShinyCharm.Checked = prefs.GetBoolean("ShinyCharm", false);

            // Other initialization
            resultHandler = new ListViewHandler(resultAdapter);

            LoadState();
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Save progress
            if (currentProcessingState == TinyFinderState.Active)
            {
                stfinder.Pause();
                SetTFState(TinyFinderState.Paused);
                SaveState();
            }
            else if (currentProcessingState == TinyFinderState.Paused)
            {
                SaveState();
            }
            else if (currentProcessingState == TinyFinderState.Finished) {
                SaveState();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            // Automatically resume search when returning from another app
            /*if (currentProcessingState == TinyFinderState.Paused)
            {
                stfinder.Resume();
                SetTFState(TinyFinderState.Active);
            }*/
        }

        private void AddNature(uint id)
        {
            if (natures.Count < 8)
            {
                natures.Add(id);

                UpdateSequenceView();

                if (natures.Count == 8)
                {
                    SetTFState(TinyFinderState.Ready);
                }
            }
        }

        private void RemoveNature()
        {
            if (natures.Count > 0)
            {
                if (natures.Count == 8)
                {
                    SetTFState(TinyFinderState.Inputting);
                }

                natures.RemoveAt(natures.Count - 1);

                UpdateSequenceView();
            }
        }

        private void UpdateSequenceView()
        {
            natureSequence.Text = "";
            for (int i = 0; i < natures.Count; ++i)
            {
                if (i > 0) { natureSequence.Text += ","; }
                natureSequence.Text += natures[i].ToString();
            }
        }

        private void StartTinySearch() {
            stfinder.SetFinder(searchNatures.ToArray(), tinyShinyCharm.Checked);
            int numThreads = Java.Lang.Runtime.GetRuntime().AvailableProcessors();
            stfinder.PreStart(numThreads);
            stfinder.Execute();
        }

        public void PauseSearch() {
            stfinder.Pause();
        }
        public void ResumeSearch()
        {
            stfinder.Resume();
        }
        private void LoadState() {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            int lastState = prefs.GetInt("TinyFinderState", 0);

            if (lastState != 0)
            {
                string seeds = prefs.GetString("TinyFinderResults", "");

                if (seeds != "")
                {
                    var seedArr = seeds.Split('|');
                    for (int i = 0; i < seedArr.Length; ++i)
                    {
                        resultAdapter.Add(seedArr[i]);
                        resultList.Add(seedArr[i]);
                    }
                    resultAdapter.NotifyDataSetChanged();
                }

                string nats = prefs.GetString("TinyFinderNatures", "");
                if (nats != "")
                {
                    natures.Clear();
                    searchNatures.Clear();

                    var natArr = nats.Split(',');
                    for (int i = 0; i < natArr.Length; ++i)
                    {
                        uint val = uint.Parse(natArr[i]);
                        natures.Add(val);
                        searchNatures.Add(val);
                    }
                    UpdateSequenceView();
                }

                if (lastState < TinyWorkProducer.MaxValue)
                {
                    SetTFState(TinyFinderState.SaveStateReady);
                }
                else {
                    SetTFState(TinyFinderState.Finished);
                }
            }
            else {
                SetTFState(TinyFinderState.Inputting);
            }

            SetSearchProgress(lastState);

            CreateSTFinder(lastState);
        }
        private void SaveState() {
            int state = stfinder.GetCurrentState();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var prefEditor = prefs.Edit();
            prefEditor.PutInt("TinyFinderState", state);
            lock (resultLock)
            {
                if (resultList.Count > 0)
                {
                    string seeds = "";
                    for (int i = 0; i < resultList.Count; ++i)
                    {
                        if (i > 0) { seeds += "|"; }
                        seeds += resultList[i];
                    }
                    prefEditor.PutString("TinyFinderResults", seeds);
                }
            }

            if (searchNatures.Count == 8) {
                string natString = "";
                for (int i = 0; i < searchNatures.Count; ++i) {
                    if (i > 0) { natString += ","; }
                    natString += natures[i].ToString();
                }
                prefEditor.PutString("TinyFinderNatures", natString);
            }

            prefEditor.Commit();
        }
        private void EraseData() {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var prefEditor = prefs.Edit();

            prefEditor.Remove("TinyFinderState");
            prefEditor.Remove("TinyFinderResults");
            prefEditor.Remove("TinyFinderNatures");
            prefEditor.Commit();

            stfinder.SetState(0);
            SetSearchProgress(0);
        }

        private void CreateSTFinder(int initialState) {
            stfinder = new SmartTinyFinder(initialState);

            stfinder.UpdateProgress += delegate {
                // Time estimation code
                /*int prg = ctiny.Progress;
                long tel = ctiny.timeElapsed_ms;
                tinyProgress.Progress = prg;

                long remaining = ((4096 - prg) * tel) / 1000;

                long min = (remaining / 60) % 60;
                long hr = remaining / 3600;*/

                // % Completed
                int prog = stfinder.GetProgress();
                int maxp = TinyWorkProducer.MaxValue - 1;
                float rate = (float)prog / (float)maxp * 100.0f;
                tinyProgress.Progress = prog;
                tinyClock.Post(delegate { tinyClock.Text = rate.ToString("0.0").PadLeft(5) + "%"; });

                // Absolute completeness
                /*int prog = stfinder.GetProgress();
                tinyClock.Post(delegate { tinyClock.Text = prog.ToString().PadLeft(5); });*/
            };

            stfinder.UpdateSeeds += (s) => {
                resultHandler.ObtainMessage(1, s).SendToTarget();
                AddResult(s);
            };

            stfinder.onFinishAction += delegate
            {
               resultHandler.Post(delegate
               {
                   if (resultList.Count > 0)
                   {
                       // Copy to clipboard
                       if (resultList.Count == 1)
                       {
                           // Copy seed directly to clipboard
                           var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                           clipboard.PrimaryClip = ClipData.NewPlainText("", resultList[0]);
                           
                           Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.seed_tiny_searchfinishedone), resultList[0]), ToastLength.Short).Show();
                       }
                       else
                       {
                           // Copy all seeds into clipboard
                           string allSeeds = "";
                           for (int i = 0; i < resultList.Count; ++i)
                           {
                               if (i > 0)
                               {
                                   allSeeds += "\n";
                               }
                               allSeeds += resultList[i];
                           }

                           var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                           clipboard.PrimaryClip = ClipData.NewPlainText("", allSeeds);

                           Toast.MakeText(this, Resources.GetString(Resource.String.seed_tiny_searchfinishedall), ToastLength.Long).Show();
                       }
                   }
                   else
                   {
                       Toast.MakeText(this, Resources.GetString(Resource.String.seed_tiny_noresult), ToastLength.Long).Show();
                   }
                   stfinder.SetState(TinyWorkProducer.MaxValue);
                   SetTFState(TinyFinderState.Finished);
               });
            };
        }

        private void SetTFState(TinyFinderState st) {
            if (st == TinyFinderState.Inputting)
            {
                natureSelection.SetBackgroundColor(Android.Graphics.Color.Transparent);
                backspaceButton.Enabled = true;
                tinyShinyCharm.Enabled = true;
                findButton.Enabled = false;
                findButton.Text = Resources.GetString(Resource.String.seed_search);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.seed_clear);
            }
            else if (st == TinyFinderState.Ready)
            {
                natureSelection.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                backspaceButton.Enabled = true;
                tinyShinyCharm.Enabled = true;
                findButton.Enabled = true;
                findButton.Text = Resources.GetString(Resource.String.seed_search);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.seed_clear);
            }
            else if (st == TinyFinderState.SaveStateReady)
            {
                natureSelection.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                backspaceButton.Enabled = false;
                tinyShinyCharm.Enabled = false;
                findButton.Enabled = true;
                findButton.Text = Resources.GetString(Resource.String.seed_resume);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.cancel);
            }
            else if (st == TinyFinderState.Active)
            {
                /*if (currentProcessingState == TinyFinderState.Ready) {

                }*/
                natureSelection.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                backspaceButton.Enabled = false;
                tinyShinyCharm.Enabled = false;
                findButton.Enabled = true;
                findButton.Text = Resources.GetString(Resource.String.seed_pause);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.seed_abort);
            }
            else if (st == TinyFinderState.Paused)
            {
                natureSelection.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                backspaceButton.Enabled = false;
                tinyShinyCharm.Enabled = false;
                findButton.Enabled = true;
                findButton.Text = Resources.GetString(Resource.String.seed_resume);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.seed_abort);
            }
            else if (st == TinyFinderState.Finished) {
                natureSelection.SetBackgroundColor(Android.Graphics.Color.DarkGray);
                backspaceButton.Enabled = false;
                tinyShinyCharm.Enabled = false;
                findButton.Enabled = false;
                findButton.Text = Resources.GetString(Resource.String.seed_done);
                abortButton.Enabled = true;
                abortButton.Text = Resources.GetString(Resource.String.seed_end);
            }

            currentProcessingState = st;
        }

        private void SetSearchProgress(int value) {
            tinyProgress.Progress = value;
            float rate = (float)value / (float)tinyProgress.Max * 100.0f;
            tinyClock.Text = rate.ToString("0.0").PadLeft(5) + "%";
        }

        private void ClearResultList() {
            lock (resultLock) {
                resultList.Clear();
            }
        }

        private void AddResult(string seed) {
            lock (resultLock) {
                resultList.Add(seed);
            }
        }
    }
}