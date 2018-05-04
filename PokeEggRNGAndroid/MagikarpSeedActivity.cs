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

namespace Gen7EggRNG
{
    [Activity(Label = "@string/activity_magikarp",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MagikarpSeedActivity : Activity
    {
        TextView seedView;
        TextView magiCount;

        EditText magiPart1;
        EditText magiPart2;
        EditText magiPart3;
        EditText magiPart4;

        Button magiZero;
        Button magiOne;
        Button magiFind;

        ImageButton magiHelp;
        ImageButton magiBackspace;
        ImageButton magiSetSeed;

        int count = 0;
        bool hasResult = false;

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

            SetContentView(Resource.Layout.MagikarpLayout);

            // Fetch widgets

            seedView = (TextView)FindViewById(Resource.Id.magikarpSeedView);
            magiCount = (TextView)FindViewById(Resource.Id.MagikarpCount);

            magiPart1 = (EditText)FindViewById(Resource.Id.MagiPart1);
            magiPart2 = (EditText)FindViewById(Resource.Id.MagiPart2);
            magiPart3 = (EditText)FindViewById(Resource.Id.MagiPart3);
            magiPart4 = (EditText)FindViewById(Resource.Id.MagiPart4);

            magiZero = (Button)FindViewById(Resource.Id.MagikarpButton0);
            magiOne = (Button)FindViewById(Resource.Id.MagikarpButton1);
            magiFind = (Button)FindViewById(Resource.Id.MagikarpFind);

            magiHelp = (ImageButton)FindViewById(Resource.Id.MagikarpHelp);
            magiBackspace = (ImageButton)FindViewById(Resource.Id.magiBackspace);
            magiSetSeed = (ImageButton)FindViewById(Resource.Id.magiSetSeed);

            // Assign functions to widgets

            magiPart1.FocusChange += (sender, args) => {
                if (!args.HasFocus) {
                    UpdateCount();
                }
            };
            magiPart1.TextChanged += (sender, args) => {
                UpdateCount();
            };
            magiPart2.FocusChange += (sender, args) => {
                if (!args.HasFocus)
                {
                    UpdateCount();
                }
            };
            magiPart2.TextChanged += (sender, args) => {
                UpdateCount();
            };
            magiPart3.FocusChange += (sender, args) => {
                if (!args.HasFocus)
                {
                    UpdateCount();
                }
            };
            magiPart3.TextChanged += (sender, args) => {
                UpdateCount();
            };
            magiPart4.FocusChange += (sender, args) => {
                if (!args.HasFocus)
                {
                    UpdateCount();
                }
            };
            magiPart4.TextChanged += (sender, args) => {
                UpdateCount();
            };

            magiZero.Click += delegate {
                AddValue(false);
            };
            magiOne.Click += delegate {
                AddValue(true);
            };
            magiFind.Click += delegate {
                FindSeed();
            };
            magiBackspace.Click += delegate {
                RemoveValue();
            };

            /*magiHelp.Click += delegate {
              // Spawn help dialog explaining how Magikarp Calculator works  
            };*/
            magiSetSeed.Enabled = false;
            magiSetSeed.Click += delegate {
                if (hasResult)
                {
                    string seed = seedView.Text;
                    Intent returnIntent = new Intent();
                    returnIntent.PutExtra("NewSeed", seed);
                    SetResult(Result.Ok, returnIntent);
                    Finish();
                }
            };

            magiHelp.Click += delegate {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle(Resources.GetString(Resource.String.magikarpseed_help_title));
                alert.SetMessage(
                    /*"1. Deposit two Magikarp with different nature and gender. BOTH MUST HOLD EVERSTONES\n" +
                    "2. Breed 127 eggs doing the following:\n" + 
                    "  2.1 Wait until the nursery girl has an egg.\n" +
                    "  2.2 Save before taking the egg.\n" +
                    "  2.3 Take the egg, hatch and write down 0 if it has the Male's nature, 1 the Female's nature.\n"*/
                    Resources.GetString(Resource.String.magikarpseed_help));

                alert.SetPositiveButton(Resources.GetString(Resource.String.okthanks), delegate { });
                alert.Show();
            };
        }

        private string JoinStrings()
        {
            return magiPart1.Text + magiPart2.Text + magiPart3.Text + magiPart4.Text;
        }

        private void UpdateCount() {
            count = magiPart1.Text.Length + magiPart2.Text.Length + magiPart3.Text.Length + magiPart4.Text.Length;
            magiCount.Text = Resources.GetString(Resource.String.profile_magikarp_count) + " " + count.ToString();

            if (count == 127)
            {
                magiFind.Enabled = true;
                magiCount.SetTextColor(Android.Graphics.Color.ForestGreen);
            }
            else {
                magiFind.Enabled = false;
                magiCount.SetTextColor(Android.Graphics.Color.WhiteSmoke);
            }
        }

        private void AddValue(bool v) {
            TextView toAdd = null;
            // Apparently getting the "MaxLength" parameter for EditText widgets is a pain, better leave it hardcoded
            if (magiPart1.Text.Length < 32)
            {
                toAdd = magiPart1;
            }
            else if (magiPart2.Text.Length < 32)
            {
                toAdd = magiPart2;
            }
            else if (magiPart3.Text.Length < 32)
            {
                toAdd = magiPart3;
            }
            else if (magiPart4.Text.Length < 31)
            {
                toAdd = magiPart4;
            }

            if (toAdd == null)
            {
                // Toast.MakeText(this, "Can't add anymore numbers.", ToastLength.Short).Show();
                // Do nothing
            }
            else {
                toAdd.Text += (v ? '1' : '0');
                UpdateCount();
            }
        }

        private void RemoveValue()
        {
            if (magiPart4.Text.Length > 0)
            {
                magiPart4.Text = magiPart4.Text.Substring(0, magiPart4.Text.Count() - 1);
            }
            else if (magiPart3.Text.Length > 0)
            {
                magiPart3.Text = magiPart3.Text.Substring(0, magiPart3.Text.Count() - 1);
            }
            else if (magiPart2.Text.Length > 0)
            {
                magiPart2.Text = magiPart2.Text.Substring(0, magiPart2.Text.Count() - 1);
            }
            else if (magiPart1.Text.Length > 0)
            {
                magiPart1.Text = magiPart1.Text.Substring(0, magiPart1.Text.Count() - 1);
            }
            UpdateCount();
        }

        private void FindSeed() {
            string fullString = JoinStrings();
            if (fullString.Length == 127)
            {
                string seed = Pk3DSRNGTool.MagikarpCalc.calc(fullString);
                seedView.Text = seed;

                hasResult = true;
                magiSetSeed.Enabled = true;
            }
        }
    }

}