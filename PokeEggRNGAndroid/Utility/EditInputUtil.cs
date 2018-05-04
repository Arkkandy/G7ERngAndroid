using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Gen7EggRNG.Util
{
    public static class EditInputUtil
    {
        public static void HideKeyboard(Activity activity)
        {
            InputMethodManager imm = (InputMethodManager)activity.GetSystemService(Activity.InputMethodService);
            //Find the currently focused view, so we can grab the correct window token from it.
            View view = activity.CurrentFocus;
            //If no view currently has focus, create a new one, just so we can grab a window token from it
            if (view == null)
            {
                view = new View(activity);
            }
            imm.HideSoftInputFromWindow(view.WindowToken, HideSoftInputFlags.None);
        }

        public static void HideKeyboardFrom(Context context, View view)
        {
            InputMethodManager imm = (InputMethodManager)context.GetSystemService(Activity.InputMethodService);
            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }


        public static void CloseEditTextOnDone(Context ctx, EditText eview)
        {
            eview.EditorAction += (sender, args) => {
                if (args.ActionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    HideKeyboardFrom(ctx, eview);
                    eview.ClearFocus();
                }
            };
        }
    }
}