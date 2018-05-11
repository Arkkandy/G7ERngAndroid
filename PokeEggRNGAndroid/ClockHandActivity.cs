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
using Gen7EggRNG.Util;
using Pk3DSRNGTool.RNG;

namespace Gen7EggRNG
{
    public enum SeedType {
        InitialSeed,
        QRFrame,
        IDSeed
    }


    [Activity(Label = "Clock Hands Method",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class ClockHandActivity : Activity
    {
        public class CHResult {
            public uint seed;
            public int frame1;
            public int frame2;
            public int correction;

            public CHResult() { }
            public CHResult(uint initialSeed) { seed = initialSeed; }
            public CHResult(uint initialSeed, int crr) { seed = initialSeed; correction = crr; }
            public CHResult(int qrFrame, int exitFrame) { frame1 = qrFrame; frame2 = exitFrame; }
        }
        List<CHResult> requestResults = new List<CHResult>();
        int startFrame = 0;

        public struct QRFrame {
            public int qrFrame;
            public int exitFrame;
        }


        public class ClockCombo
        {
            public ViewGroup boundingLayout;
            public ImageView image;
            public TextView number;
        }

        private enum SeedReadState {
            Inputting,
            Requesting
        }
        SeedReadState activityState;
        uint currentSeed;

        GameVersionUI gameVersion;

        ClockCombo[] clockData;

        Button requestButton;
        TextView inputView;

        TextView needleCountView;

        ProgressBar requestingBar;

        // Radio group 1
        RadioButton startPositionRB;
        RadioButton endPositionRB;
        Spinner endPositionEdit;

        // Radio group 2
        RadioButton initialSeedRB;
        RadioButton qrSeedRB;
        RadioButton idSeedRB;
        EditText qrMinEdit;
        EditText qrMaxEdit;
        SeedType uiSeedType;

        ImageButton backButton;
        ImageButton clearButton;

        TextView resultInfo;
        ListView resultList;
        SeedType resultType;

        List<int> internalClockhands = new List<int>();
        List<int> uiClockhands = new List<int>();
        int phase = 0;
        int endPhase = 4;
        int minFrame = 478;
        int maxFrame = 50000;

        bool isId = false;
        bool isUltra = false;

        ToastWrapper toastMessage;

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

            SetContentView( Resource.Layout.ClockHandLayout);

            toastMessage = new ToastWrapper(this);

            requestButton = FindViewById<Button>(Resource.Id.requestButton);
            inputView = FindViewById<TextView>(Resource.Id.clockInput);
            requestingBar = FindViewById<ProgressBar>(Resource.Id.requestingBar);

            needleCountView = FindViewById<TextView>(Resource.Id.needleCount);

            startPositionRB = FindViewById<RadioButton>(Resource.Id.startPosition);
            endPositionRB = FindViewById<RadioButton>(Resource.Id.endPosition);
            initialSeedRB = FindViewById<RadioButton>(Resource.Id.initialSeedCheck);
            qrSeedRB = FindViewById<RadioButton>(Resource.Id.qrSeedCheck);
            idSeedRB = FindViewById<RadioButton>(Resource.Id.idSeedCheck);

            resultInfo = FindViewById<TextView>(Resource.Id.resultNote);
            resultList = FindViewById<ListView>(Resource.Id.clockHandResults);

            endPositionEdit = FindViewById<Spinner>(Resource.Id.endPhase);

            qrMinEdit = FindViewById<EditText>(Resource.Id.qrMinFrame);
            qrMaxEdit = FindViewById<EditText>(Resource.Id.qrMinFrame);

            int[] layoutRes = new int[] {
                Resource.Id.clockData0, Resource.Id.clockData1, Resource.Id.clockData2,
                Resource.Id.clockData3, Resource.Id.clockData4, Resource.Id.clockData5,
                Resource.Id.clockData6, Resource.Id.clockData7, Resource.Id.clockData8,
                Resource.Id.clockData9, Resource.Id.clockData10, Resource.Id.clockData11,
                Resource.Id.clockData12, Resource.Id.clockData13, Resource.Id.clockData14,
                Resource.Id.clockData15, Resource.Id.clockData16
            };
            int[] buttonRes = new int[] {
                Resource.Id.clock0, Resource.Id.clock1, Resource.Id.clock2,
                Resource.Id.clock3, Resource.Id.clock4, Resource.Id.clock5,
                Resource.Id.clock6, Resource.Id.clock7, Resource.Id.clock8,
                Resource.Id.clock9, Resource.Id.clock10, Resource.Id.clock11,
                Resource.Id.clock12, Resource.Id.clock13, Resource.Id.clock14,
                Resource.Id.clock15, Resource.Id.clock16
            };
            int[] numRes = new int[] {
                Resource.Id.clockHelp0, Resource.Id.clockHelp1, Resource.Id.clockHelp2,
                Resource.Id.clockHelp3, Resource.Id.clockHelp4, Resource.Id.clockHelp5,
                Resource.Id.clockHelp6, Resource.Id.clockHelp7, Resource.Id.clockHelp8,
                Resource.Id.clockHelp9, Resource.Id.clockHelp10, Resource.Id.clockHelp11,
                Resource.Id.clockHelp12, Resource.Id.clockHelp13, Resource.Id.clockHelp14,
                Resource.Id.clockHelp15, Resource.Id.clockHelp16
            };

            clockData = new ClockCombo[buttonRes.Length];
            for (int i = 0; i < buttonRes.Length; ++i)
            {
                clockData[i] = new ClockCombo();

                ViewGroup bll = FindViewById<ViewGroup>(layoutRes[i]);
                ImageView img = FindViewById<ImageView>(buttonRes[i]);
                TextView imgNum = FindViewById<TextView>(numRes[i]);
                int clockValue = i;

                bll.Click += delegate {
                    AddClockHand(clockValue);
                };

                clockData[i].boundingLayout = bll;
                clockData[i].image = img;
                clockData[i].number = imgNum;
            }

            backButton = FindViewById<ImageButton>(Resource.Id.backButton);
            backButton.Click += delegate {
                EraseClockHand();
            };

            clearButton = FindViewById<ImageButton>(Resource.Id.clearButton);
            clearButton.Click += delegate {
                ClearClockHands();
            };

            startPositionRB.CheckedChange += (sender,args) => {
                if (args.IsChecked)
                {
                    phase = 0;
                    endPositionRB.Checked = false;
                    UpdateNumValues();
                }
            };
            endPositionRB.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    phase = endPhase;
                    startPositionRB.Checked = false;
                    UpdateNumValues();
                }
            };


            initialSeedRB.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    qrSeedRB.Checked = false;
                    idSeedRB.Checked = false;
                    SetSeedType(SeedType.InitialSeed);
                }
            };
            qrSeedRB.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    initialSeedRB.Checked = false;
                    idSeedRB.Checked = false;
                    SetSeedType(SeedType.QRFrame);
                }
            };
            idSeedRB.CheckedChange += (sender, args) => {
                if (args.IsChecked)
                {
                    qrSeedRB.Checked = false;
                    initialSeedRB.Checked = false;
                    SetSeedType(SeedType.IDSeed);
                }
            };

            endPhase = 4;
            endPositionRB.Checked = true;
            initialSeedRB.Checked = true;

            requestButton.Click += delegate {
                {
                    // Button should be disabled during Request, check anyway
                    if (activityState == SeedReadState.Requesting) {
                        return;
                    }

                    // Validate input
                    if (uiSeedType == SeedType.InitialSeed)
                    {                        
                        if (uiClockhands.Count < 8)
                        {
                            toastMessage.ShowMessage("Initial Seed: Enter 8 or more clock hands.", ToastLength.Short);
                            return;
                        }
                        isId = false;
                    }
                    else if (uiSeedType == SeedType.IDSeed)
                    {
                        if (uiClockhands.Count < 9)
                        {
                            toastMessage.ShowMessage("ID Seed: Enter 9 or more clock hands.", ToastLength.Short);
                            return;
                        }
                        isId = true;
                    }
                    else if (uiSeedType == SeedType.QRFrame) {
                        if (uiClockhands.Count < 2)
                        {
                            toastMessage.ShowMessage("QR Frame: Enter 2 or more clock hands.", ToastLength.Short);
                            return;
                        }
                        //isId = false;
                    }

                    // Perform request after validating
                    isUltra = GameVersionConversion.IsUltra(gameVersion);

                    toastMessage.Hide();
                    //resultList.Adapter = null;
                    SetActivityState(SeedReadState.Requesting);

                    internalClockhands = new List<int>(uiClockhands);

                    SeedType type = uiSeedType;
                    var thr = new System.Threading.Thread( () => PerformSeedRequest(type));
                    thr.Start();
                }
            };

            endPositionEdit.Adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerDropDownItem,
                Enumerable.Range(0, 17).ToList().ConvertAll(x => x.ToString()));
            endPositionEdit.SetSelection(4);
            endPositionEdit.ItemSelected += (sender, args) => {
                SetEndPhase(args.Position);
            };

            resultList.ItemLongClick += (sender, args) => {
                if (resultType == SeedType.InitialSeed)
                {
                    PopupMenu menu = new PopupMenu(this, resultList.GetChildAt(args.Position), Android.Views.GravityFlags.Center);

                    //menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.search_popup_copyseed));
                    menu.Menu.Add(Menu.None, 1, 1, "Set initial seed");

                    menu.MenuItemClick += (s2, a2) =>
                    {
                        if (a2.Item.ItemId == 1)
                        {
                            currentSeed = requestResults[args.Position].seed;
                            PrepareReturnIntent(args.Position, SeedType.InitialSeed);
                            BuildTitle(currentSeed.ToString("X"), startFrame);
                        }
                    };

                    menu.Show();
                }
                if (resultType == SeedType.QRFrame)
                {
                    PopupMenu menu = new PopupMenu(this, resultList.GetChildAt(args.Position), Android.Views.GravityFlags.Center);

                    //menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.search_popup_copyseed));
                    menu.Menu.Add(Menu.None, 1, 1, "Use exit frame");

                    menu.MenuItemClick += (s2, a2) =>
                    {
                        if (a2.Item.ItemId == 1)
                        {
                            PrepareReturnIntent(args.Position, SeedType.QRFrame);
                            BuildTitle(currentSeed.ToString("X"), requestResults[args.Position].frame2);
                        }
                    };

                    menu.Show();
                }
                else if (resultType == SeedType.IDSeed)
                {
                    PopupMenu menu = new PopupMenu(this, resultList.GetChildAt(args.Position), Android.Views.GravityFlags.Center);

                    //menu.Menu.Add(Menu.None, 1, 1, Resources.GetString(Resource.String.search_popup_copyseed));
                    menu.Menu.Add(Menu.None, 1, 1, "Set initial seed");

                    menu.MenuItemClick += (s2, a2) =>
                    {
                        if (a2.Item.ItemId == 1)
                        {
                            currentSeed = requestResults[args.Position].seed;
                            PrepareReturnIntent(args.Position, SeedType.IDSeed);
                            BuildTitle(currentSeed.ToString("X"), startFrame);
                        }
                    };

                    menu.Show();
                }
            };

            SetActivityState(SeedReadState.Inputting);

            var pData = ProfileData.LoadCurrentProfileData(this);
            gameVersion = pData.gameVersion;
            currentSeed = pData.initialSeed;
            BuildTitle( seed: currentSeed.ToString("X") );

            qrMinEdit.Text = GameVersionConversion.GetGameStartingFrame(gameVersion, false).ToString();
        }

        // Test needles
        // - Initial Seed: 1,6,15,1,10,4,16,3 (,8 | ,11)
        // - ID: 0,1,2,7,4,10,4,8,9
        private void PerformSeedRequest(SeedType type)
        {
            List<string> results = null;
            string errorMessage = string.Empty;
            string needleString = string.Join(",", internalClockhands);

            // If requesting QR Search
            if (type == SeedType.QRFrame)
            {
                var result = QRSearch(minFrame, maxFrame);

                if (result.Count > 0)
                {
                    results = result.ConvertAll(x => "QR Frame: " + x.qrFrame + " Exit Frame: " + x.exitFrame);
                    requestResults = result.ConvertAll(x => new CHResult(x.qrFrame, x.exitFrame));
                }
            }
            // If requesting Initial Seed or ID Seed
            else
            {
                try
                {
                    var result = Pk3DSRNGTool.SFMTSeedAPI.request(needleString, isId, isUltra);
                    
                    if (result == null)
                    {
                        errorMessage = "Request error.";
                    }
                    else
                    {
                        if (result.Count > 0)
                        {
                            int sFrame = GameVersionConversion.GetGameStartingFrame(gameVersion, type == SeedType.IDSeed) + internalClockhands.Count - 1;
                            if (type == SeedType.InitialSeed)
                            {
                                requestResults = result.ConvertAll(x => new CHResult(uint.Parse(x.seed, System.Globalization.NumberStyles.HexNumber) ));
                                results = result.ConvertAll(x => x.seed.ToUpper());
                            }
                            else if (type == SeedType.IDSeed)
                            {
                                requestResults = result.ConvertAll(x => new CHResult(uint.Parse(x.seed, System.Globalization.NumberStyles.HexNumber), x.add ));
                                results = result.ConvertAll(x => x.seed.ToUpper() + " - Correction: " + x.add);
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    errorMessage = e.Message;
                }
            }

            // Request was finished, update UI
            RunOnUiThread( delegate {
                if (errorMessage != string.Empty)
                {
                    toastMessage.ShowMessage(errorMessage, ToastLength.Long);
                }
                else {
                    if (results == null)
                    {
                        resultInfo.Text = "Results: " + String.Join(",", needleString) + " [" + internalClockhands.Count + "]: No results";
                        resultList.Adapter = null;
                    }
                    else
                    {
                        resultType = type;
                        startFrame = GameVersionConversion.GetGameStartingFrame(gameVersion, resultType == SeedType.IDSeed) + internalClockhands.Count - 1;
                        if (results.Count == 1)
                        {
                            if ( resultType != SeedType.QRFrame )
                            {
                                currentSeed = requestResults[0].seed;
                            }
                            BuildTitle(currentSeed.ToString("X"),
                                       resultType == SeedType.QRFrame ? requestResults[0].frame2 : startFrame );

                            PrepareReturnIntent(0, resultType);
                        }

                        resultInfo.Text = "Results: " + String.Join(",", needleString) + " [" + internalClockhands.Count + "]";

                        // #TODO - Correct starting frames
                        if (resultType != SeedType.QRFrame)
                        {
                            resultInfo.Text += " - Starting frame = " + startFrame.ToString();
                        }

                        var resultAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, results);
                        resultList.Adapter = resultAdapter;
                    }
                }

                SetActivityState(SeedReadState.Inputting);
            });

        }

        private void UpdateNumValues()
        {
            for (int i = 0; i < 17; ++i)
            {
                clockData[i].number.Text = ((i + 17 - phase) % 17).ToString();
            }
        }

        private void ClearClockHands()
        {
            if (activityState == SeedReadState.Inputting)
            {
                uiClockhands.Clear();

                UpdateInputView();
            }
        }

        private void EraseClockHand()
        {
            if (activityState == SeedReadState.Inputting)
            {
                if (uiClockhands.Count > 0)
                {
                    uiClockhands.RemoveAt(uiClockhands.Count - 1);
                    UpdateInputView();
                }
            }
        }

        private void AddClockHand(int value)
        {
            if (activityState == SeedReadState.Inputting)
            {
                uiClockhands.Add((value + 17 - phase) % 17);
                UpdateInputView();
            }
        }

        private void UpdateInputView()
        {
            inputView.Text = string.Join(",", uiClockhands);
            needleCountView.Text = "Needle Count: " + uiClockhands.Count.ToString();
        }

        private void BuildTitle(string seed = "", int frame = 0) {
            string ttl = "Clock Hands Method (" + GameVersionConversion.GetGameVersionName(gameVersion) + ")";
            if (seed != string.Empty) {
                ttl += " - " + seed;
            }
            if (frame != 0) {
                ttl += " - Frame: " + frame;
            }
            Title = ttl;
        }

        private void SetEndPhase(int ph) {
            endPhase = ph;
            if (endPositionRB.Checked) {
                phase = endPhase;
                UpdateNumValues();
            }
        }

        private void SetSeedType(SeedType type) {
            uiSeedType = type;
            VerifySeedTypeConstraints(uiSeedType);
        }
        private void VerifySeedTypeConstraints(SeedType type) {
            if (type == SeedType.InitialSeed)
            {
                startPositionRB.Enabled = true;
                endPositionRB.Enabled = true;

                endPositionRB.Checked = true;
            }
            else
            {
                startPositionRB.Enabled = false;
                endPositionRB.Enabled = false;

                startPositionRB.Checked = true;
            }
        }

        private void SetActivityState(SeedReadState state) {
            if (state == SeedReadState.Inputting)
            {
                initialSeedRB.Enabled = true;
                qrSeedRB.Enabled = true;
                idSeedRB.Enabled = true;
                VerifySeedTypeConstraints(uiSeedType);

                requestButton.Enabled = true;
                requestingBar.Visibility = Android.Views.ViewStates.Invisible;
            }
            else if (state == SeedReadState.Requesting) {
                startPositionRB.Enabled = false;
                endPositionRB.Enabled = false;

                initialSeedRB.Enabled = false;
                qrSeedRB.Enabled = false;
                idSeedRB.Enabled = false;

                requestButton.Enabled = false;
                requestingBar.Visibility = Android.Views.ViewStates.Visible;
            }

            activityState = state;
        }

        private List<QRFrame> QRSearch(int minF, int maxF)
        {
            // Skip frames
            int min = minF;
            int max = maxF;
            SFMT sfmt = new SFMT(currentSeed);
            for (int i = 0; i < min; i++)
                sfmt.Next();

            int length = internalClockhands.Count;

            // Create buffer
            int[] temp_List = new int[length];
            for (int i = 0; i < length; i++)
                temp_List[i] = (int)(sfmt.Nextulong() % 17);

            // Search
            List<QRFrame> res = new List<QRFrame>();
            for (int i = min, head = 0; i <= max; i++)
            {
                int j = 0;
                for (; j < length; j++)
                    if (temp_List[(j + head) % length] != internalClockhands[j])
                        break;
                if (j == length)
                { // Pass compare
                    QRFrame qf;
                    qf.qrFrame = i + internalClockhands.Count - 1;
                    qf.exitFrame = i + internalClockhands.Count + 1;

                    res.Add(qf);
                }
                temp_List[head++] = (int)(sfmt.Nextulong() % 17);
                head = head == length ? 0 : head;
            }

            return res;
        }

        private void PrepareReturnIntent(int index, SeedType type) {
            if (type == SeedType.InitialSeed)
            {
                string seed = requestResults[index].seed.ToString("X");
                Intent returnIntent = new Intent();
                returnIntent.PutExtra("ClockType", 0);
                returnIntent.PutExtra("InitialSeed", seed);
                returnIntent.PutExtra("SeedFrame", startFrame);
                SetResult(Result.Ok, returnIntent);
            }
            else if (type == SeedType.QRFrame)
            {
                Intent returnIntent = new Intent();
                returnIntent.PutExtra("ClockType", 1);
                returnIntent.PutExtra("QRCurrent", requestResults[index].frame1);
                returnIntent.PutExtra("QRExit", requestResults[index].frame2);
                SetResult(Result.Ok, returnIntent);
            }
            else if (type == SeedType.IDSeed) {
                // #TODO: IMPL ID SEED
                string seed = requestResults[index].seed.ToString("X");
                Intent returnIntent = new Intent();
                returnIntent.PutExtra("ClockType", 2);
                returnIntent.PutExtra("InitialSeed", seed);
                returnIntent.PutExtra("SeedFrame", startFrame);
                returnIntent.PutExtra("Correction", requestResults[index].correction);
                SetResult(Result.Ok, returnIntent);
            }
        }
    }
}