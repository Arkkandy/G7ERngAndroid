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

namespace Gen7EggRNG
{
    public class TimeCalcDialog : Dialog
    {
        int start;
        int target;
        int npcs;
        bool rain;
        bool fidget;

        uint seed;
        
        EditText startF;
        EditText targetF;
        EditText npcsET;
        CheckBox rainingCB;
        CheckBox fidgetCB;

        TextView resultText;
        Button doneButton;

        public TimeCalcDialog(Context ctx) : base(ctx) {

        }

        public void Initialize(uint iseed, int startFrame, int targetFrame, int numNpcs, bool raining, bool fidgeting) {
            seed = iseed;
            start = startFrame;
            target = targetFrame;
            npcs = numNpcs;
            rain = raining;
            fidget = fidgeting;

            //SetTitle(Context.Resources.GetString(Resource.String.parents_templates_title));
            SetTitle("Time Calculator");

            SetContentView(Resource.Layout.TimeCalculator);

            doneButton = FindViewById<Button>(Resource.Id.tcalcDone);
            startF = FindViewById<EditText>(Resource.Id.tcalcStart);
            targetF = FindViewById<EditText>(Resource.Id.tcalcTarget);
            npcsET = FindViewById<EditText>(Resource.Id.tcalcNpcs);
            rainingCB = FindViewById<CheckBox>(Resource.Id.tcalcRaining);
            fidgetCB = FindViewById<CheckBox>(Resource.Id.tcalcFidget);

            resultText = FindViewById<TextView>(Resource.Id.tcalcResult);

            doneButton.Click += delegate { base.Dismiss(); };

            startF.Text = start.ToString();
            targetF.Text = target.ToString();
            npcsET.Text = npcs.ToString();
            rainingCB.Checked = rain;
            fidgetCB.Checked = fidget;

            startF.TextChanged += (sender,args) => {
                int.TryParse(args.Text.ToString(), out start);
                Recalc();
            };
            targetF.TextChanged += (sender, args) => {
                int.TryParse(args.Text.ToString(), out target);
                Recalc();
            };
            npcsET.TextChanged += (sender, args) => {
                int.TryParse(args.Text.ToString(), out npcs);
                Recalc();
            };
            rainingCB.CheckedChange += (sender, args) => {
                rain = args.IsChecked;
                Recalc();
            };
            fidgetCB.CheckedChange += (sender, args) => {
                fidget = args.IsChecked;
                Recalc();
            };


            doneButton.Click += delegate {
                base.Dismiss();
            };

            Recalc();
        }

        private void Recalc() {
            int[] totaltime = Pk3DSRNGTool.FuncUtil.CalcFrame(seed, start, target, (byte)(npcs + 1), fidget, rain);
            double realtime = totaltime[0] / 30.0;
            string str = $"{totaltime[0] * 2}F ({realtime.ToString("F")}s)";
            if (totaltime[1] > 0)  // To-do should be a good sign for menu method
                str += $" <{totaltime[1] * 2}F>";

            resultText.Text = str;
        }
    }
}