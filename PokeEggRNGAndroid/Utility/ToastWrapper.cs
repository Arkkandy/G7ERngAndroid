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

namespace Gen7EggRNG.Util
{
    public class ToastWrapper
    {
        private Toast toast;
        private Context ctx;

        public ToastWrapper(Context context) {
            ctx = context;
            //toast = Toast.MakeText(context, "", ToastLength.Short);
        }

        public void ShowMessage(string message, ToastLength length) {
            Hide();
            toast = Toast.MakeText(ctx, message, length);
            //toast.SetText(message);
            //toast.Duration = length;
            toast.Show();
        }

        public void Hide() {
            if (toast != null)
            {
                toast.Cancel();
            }
        }
    }
}