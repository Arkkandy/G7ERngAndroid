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

using Android.Preferences;
using Pk3DSRNGTool.RNG;
using Pk3DSRNGTool.Core;

namespace Gen7EggRNG
{
    [Activity(Label = "@string/activity_seedrecovery",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class SeedRecoveryActivity : Activity
    {
        private struct SeedFrameCombo {
            public EggSeed seed;
            public int frame;
        }

        private enum SeedRecoverState
        {
            Inputting,
            Finding
        }

        SeedRecoverState currentProcessingState;

        EggSeed currentSeed;

        TextView natureSequence;
        EditText maxFrameEdit;

        ListView natureSelection;

        ListView results;

        Button findButton;
        ImageButton recoveryHelp;
        ImageButton backspaceButton;

        Button recClear;

        ProgressBar recProgress;

        CheckBox recShinyCharm;
        //CheckBox recUseParents;

        List<int> natures;
        List<SeedFrameCombo> resultList;

        System.Threading.Thread recoveryThread = null;

        Handler activityHandler = new Handler();

        //ParentData parents;

        int[] natureIndices;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.SeedRecoveryLayout);

            natures = new List<int>();
            //parents = ParentData.LoadParentData(this);

            // Fetch Widgets
            natureSequence = (TextView)FindViewById(Resource.Id.recNatureSequence);
            maxFrameEdit = (EditText)FindViewById(Resource.Id.recMaxFrame);
            natureSelection = (ListView)FindViewById(Resource.Id.recNatureSelection);
            results = (ListView)FindViewById(Resource.Id.recResults);
            findButton = (Button)FindViewById(Resource.Id.recoverFind);
            recoveryHelp = (ImageButton)FindViewById(Resource.Id.recHelp);
            recShinyCharm = (CheckBox)FindViewById(Resource.Id.recShinyCharm);
            backspaceButton = (ImageButton)FindViewById(Resource.Id.recBackspace);
            recClear = (Button)FindViewById(Resource.Id.recClear);
            recProgress = (ProgressBar)FindViewById(Resource.Id.recBar);
            //recUseParents = (CheckBox)FindViewById(Resource.Id.recUseParents);

            // Assign functions
            string[] natureStrings = Resources.GetStringArray(Resource.Array.NatureIndexed);
            natureIndices = ArrayUtil.SortArrayIndex(natureStrings);
            for (int i = 0; i < natureStrings.Length; ++i)
            {
                natureStrings[i] += " - " + natureIndices[i];
            }
            natureSelection.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, natureStrings);
            natureSelection.ItemClick += (sender, args) => {
                if (currentProcessingState == SeedRecoverState.Inputting)
                {
                    AddNature(natureIndices[args.Position]);
                }
            };

            findButton.Text = Resources.GetString(Resource.String.seed_recover);
            findButton.Click += delegate {
                if (currentProcessingState == SeedRecoverState.Inputting && natures.Count >= 4)
                {
                    if (recoveryThread == null)
                    {
                        ChangeState(SeedRecoverState.Finding);

                        recProgress.Max = int.Parse(maxFrameEdit.Text);
                        recoveryThread = new System.Threading.Thread(() =>
                        {
                            resultList = RecoverSeed(currentSeed);
                            activityHandler.Post(ProcessResults);
                        });

                        recoveryThread.Start();
                        findButton.Text = Resources.GetString(Resource.String.profile_searchingseed);
                        findButton.Enabled = false;
                    }
                }
            };

            backspaceButton.Click += delegate {
                if (currentProcessingState == SeedRecoverState.Inputting)
                {
                    RemoveNature();
                }
            };

            recoveryHelp.Click += delegate {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle(Resources.GetString(Resource.String.profile_seedrecoveryhelptitle));
                alert.SetMessage(Resources.GetString(Resource.String.profile_seedrecoveryhelpmessage));

                alert.SetPositiveButton(Resources.GetString(Resource.String.okthanks), delegate { });
                alert.Show();
            };

            recClear.Click += delegate {
                if (currentProcessingState == SeedRecoverState.Inputting)
                {
                    findButton.Enabled = false;
                    natures.Clear();
                    UpdateSequenceView();
                }
            };

            recProgress.Max = 1000;
            recProgress.Progress = 0;

            results.ItemLongClick += (sender, args) => {
                PopupMenu menu = new PopupMenu(this, results.GetChildAt(args.Position), Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, String.Format(Resources.GetString(Resource.String.profile_copyseedframe), resultList[args.Position].frame));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.profile_setseed));
                if (resultList.Count > 1)
                {
                    menu.Menu.Add(Menu.None, 3, 3, Resources.GetString(Resource.String.profile_copyall));
                }

                menu.MenuItemClick += (msender, margs) => {

                    if (margs.Item.ItemId == 1)
                    {
                        // Copy checkpoint
                        var seed = resultList[args.Position].seed.GetSeedToString();

                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("EggSeed", seed);

                        Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.profile_copiedresultseed), seed), ToastLength.Short).Show();
                    }
                    else if (margs.Item.ItemId == 2)
                    {
                        string seed = resultList[args.Position].seed.GetSeedToString();
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
                            allSeeds += resultList[i].seed;
                        }

                        var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                        clipboard.PrimaryClip = ClipData.NewPlainText("", allSeeds);

                        Toast.MakeText(this, Resources.GetString(Resource.String.profile_copiedall), ToastLength.Long).Show();
                    }
                };

                menu.Show();
            };

            ChangeState(SeedRecoverState.Inputting);

            var pData = ProfileData.LoadCurrentProfileData(this);
            currentSeed = new EggSeed(pData.currentSeed);
            recShinyCharm.Checked = pData.shinyCharm;
        }

        private void AddNature(int id) {
            if (natures.Count < 8)
            {
                natures.Add(id);

                UpdateSequenceView();

                if (natures.Count >= 4) {
                    findButton.Enabled = true;
                }
            }
        }

        private void RemoveNature() {
            if (natures.Count > 0) {
                natures.RemoveAt(natures.Count - 1);

                if (natures.Count < 4) {
                    findButton.Enabled = false;
                }

                UpdateSequenceView();
            }
        }

        private void UpdateSequenceView() {
            natureSequence.Text = "";
            for (int i = 0; i < natures.Count; ++i)
            {
                if (i > 0) { natureSequence.Text += ","; }
                natureSequence.Text += natures[i].ToString();
            }
        }


        private List<SeedFrameCombo> RecoverSeed(EggSeed startSeed)
        {
            var rng = new TinyMT(startSeed.GetSeedVector());
            int max = int.Parse(maxFrameEdit.Text);

            // Prepare
            // Utilize user custom parent data OR simple data
            RNGPool.igenerator = PrepareParentData();
            RNGPool.CreateBuffer(rng, 100);

            Queue<Pk3DSRNGTool.ResultE7> resultCache = new Queue<Pk3DSRNGTool.ResultE7>();

            List<SeedFrameCombo> results = new List<SeedFrameCombo>();
            // Start
            for (int i = 0; i <= max; i++ )
            {
                if (i % 10000 == 0) { recProgress.Progress = i; }

                Pk3DSRNGTool.ResultE7 result = null;

                if (resultCache.Count > 0) { result = resultCache.Dequeue(); }
                else
                {
                    result = RNGPool.GenerateEgg7() as Pk3DSRNGTool.ResultE7;
                    RNGPool.AddNext(rng);
                }
                
                if (result.Nature == natures[0]) {

                    bool success = true;
                    var lastResult = result;
                    // Check nature sequence
                    int adv = lastResult.FramesUsed;
                    for (int j = 1; j < natures.Count; ++j) {

                        if (resultCache.Count >= adv)
                        {
                            lastResult = resultCache.ElementAt(adv-1);
                        }
                        else {
                            int framesToAdd = adv-resultCache.Count;
                            for (int k = 0; k < framesToAdd; ++k) {
                                lastResult = RNGPool.GenerateEgg7() as Pk3DSRNGTool.ResultE7;
                                resultCache.Enqueue(lastResult);
                                RNGPool.AddNext(rng);
                            }
                        }

                        // If any nature is not the same, then give up on this frame
                        if (lastResult.Nature != natures[j]) {
                            success = false; break;
                        }

                        adv += lastResult.FramesUsed;
                    }
                    // If a match is found, add to list
                    if (success) {
                        SeedFrameCombo sfc;
                        sfc.seed = new EggSeed();
                        sfc.seed.SetSeed(result.Status.ToString());
                        sfc.frame = i;
                        results.Add(sfc);
                    }

                }
            }

            recProgress.Progress = max;

            return results;
        }

        private void Advance(TinyMT tmt, int adv) {
            for (int i = 0; i < adv; ++i) {
                RNGPool.AddNext(tmt);
            }
        }

        private EggRNG PrepareParentData()
        {
            var setting = new Pk3DSRNGTool.Egg7();
            setting.FemaleIVs = new int[] { 0, 0, 0, 0, 0, 0 };
            setting.MaleIVs = new int[] { 0, 0, 0, 0, 0, 0 };
            setting.MaleItem = (byte)BreedItem.None;
            setting.FemaleItem = (byte)BreedItem.None;
            setting.ShinyCharm = recShinyCharm.Checked;
            setting.Gender = Pk3DSRNGTool.FuncUtil.getGenderRatio(GenderConversion.ConvertGenderIndexToByte((int)GenderConversion.GenderType.SameRatio));//FuncUtil.getGenderRatio(GenderConversion.ConvertGenderIndexToByte(parents.genderCode));
            //if (setting is Egg7 setting7)
            //{
                setting.Homogeneous = false;
                setting.FemaleIsDitto = true;
            //}
            setting.InheritAbility = 1;
            setting.MMethod = false;
            setting.NidoType = false;

            setting.ConsiderOtherTSV = false;//ConsiderOtherTSV.Checked && (ShinyCharm.Checked || MM.Checked || Gen6 && RB_Accept.Checked);
            setting.OtherTSVs = null;//OtherTSVList.ToArray();

            setting.MarkItem();
            return setting;
        }

        private void ProcessResults() {
            if (resultList.Count > 0)
            {
                List<string> resStrings = resultList.ConvertAll(x => x.frame.ToString().PadRight(6) + " " + x.seed.GetSeedToString());

                // Copy to clipboard
                if (resultList.Count == 1)
                {
                    // Copy seed directly to clipboard
                    var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", resultList[0].seed.GetSeedToString());
                    
                    Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.profile_copiedresultseed), resultList[0].seed), ToastLength.Short).Show();
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
                        allSeeds += "[" + resultList[i].frame + ": " + resultList[i].seed.GetSeedToString() + " ]";
                    }

                    var clipboard = (ClipboardManager)GetSystemService(ClipboardService);
                    clipboard.PrimaryClip = ClipData.NewPlainText("", allSeeds);

                    Toast.MakeText(this, Resources.GetString(Resource.String.profile_copiedall), ToastLength.Long).Show();
                }



                results.Adapter = null;
                results.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, resStrings);
            }
            else
            {
                Toast.MakeText(this, Resources.GetString(Resource.String.profile_noresult), ToastLength.Long).Show();
            }

            findButton.Enabled = true;
            findButton.Text = Resources.GetString(Resource.String.seed_recover);

            recoveryThread = null;

            ChangeState(SeedRecoverState.Inputting);
        }

        private void ChangeState(SeedRecoverState state) {
            if (state == SeedRecoverState.Inputting)
            {
                if (natures.Count >= 4)
                {
                    findButton.Enabled = true;
                }
                else
                {
                    findButton.Enabled = false;
                }
                recClear.Enabled = true;
                backspaceButton.Enabled = true;
            }
            else if (state == SeedRecoverState.Finding) {
                findButton.Enabled = false;
                recClear.Enabled = false;
                backspaceButton.Enabled = false;
            }

            currentProcessingState = state;
        }

        /*private EggRNG PrepareParentDataUser()
        {
            var setting = (EggRNG)new Pk3DSRNGTool.Egg7();
            setting.FemaleIVs = (int[])parents.femaleIV.Clone();
            setting.MaleIVs = (int[])parents.maleIV.Clone();
            setting.MaleItem = (byte)parents.maleItem;
            setting.FemaleItem = (byte)parents.femaleItem;
            setting.ShinyCharm = recShinyCharm.Checked;
            setting.TSV = 0;
            setting.Gender = Pk3DSRNGTool.FuncUtil.getGenderRatio(GenderConversion.ConvertGenderIndexToByte(parents.genderCode));
            if (setting is Pk3DSRNGTool.Egg7 setting7)
            {
                setting7.Homogeneous = parents.isSameDex;
                setting7.FemaleIsDitto = (parents.whoIsDitto == 2);
            }
            setting.InheritAbility = (byte)(parents.whoIsDitto == 2 ? parents.maleAbility : parents.femaleAbility);
            setting.MMethod = parents.isMasuda;
            setting.NidoType = parents.isNidoSpecies;

            setting.ConsiderOtherTSV = false;//ConsiderOtherTSV.Checked && (ShinyCharm.Checked || MM.Checked || Gen6 && RB_Accept.Checked);
            setting.OtherTSVs = null;//OtherTSVList.ToArray();

            setting.MarkItem();
            return setting;
        }*/
    }
}