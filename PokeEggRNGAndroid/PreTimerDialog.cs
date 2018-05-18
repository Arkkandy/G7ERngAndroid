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
    public class PreTimerDialog : Dialog
    {
        public PreTimerDialog(Context ctx) : base(ctx) {

        }

        public void Initialize(int adjustment) {

            SetContentView(Resource.Layout.AdjustPretimerDialog);

            TextView preTInfo = FindViewById<TextView>(Resource.Id.pretmrInfo);
            TextView preTAdjust = FindViewById<TextView>(Resource.Id.pretmrAdjust);
            TextView preTWarning = FindViewById<TextView>(Resource.Id.pretmrWarning);

            if (Math.Abs(adjustment) >= 2000)
            {
                preTWarning.Visibility = ViewStates.Visible;
                //preTAdjust.Text = (adjustment > 0 ? "> +2000" : "< -2000");
            }
            //else {
                preTAdjust.Text = adjustment.ToString("+#;-#;0");
            //}

            if (adjustment > 0)
            {
                SetTitle("Add to the Pre-Timer");
            }
            else if (adjustment < 0)
            {
                SetTitle("Sub from the Pre-Timer");
            }
            else {
                SetTitle("No adjustment");
                preTInfo.Text = "You hit your target";
            }
        }
    }
}