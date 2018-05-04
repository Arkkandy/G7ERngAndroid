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
    public class EnterTextDialog : Dialog
    {
        private Action<string> yesAction;
        private Action noAction;

        private EditText editText;

        private string defaultText = String.Empty;

        private string okText = "OK", cancelText = "Cancel";

        private string emptyFieldMessage = "Text field is empty!";

        public EnterTextDialog(Context context) : base(context) {

        }

        public void SetDefaultText(string text) {
            defaultText = text;
        }

        public void SetButtonText(string okBText, string cancelBText) {
            okText = okBText;
            cancelText = cancelBText;
        }

        public void SetEmptyFieldMessage(string message)
        {
            emptyFieldMessage = message;
        }

        public void InitializeDialog(Action<string> yesAction, Action noAction) {

            this.yesAction = yesAction;
            this.noAction = noAction;

            LinearLayout mainDialogLayout = new LinearLayout(Context);
            mainDialogLayout.Orientation = Orientation.Vertical;

            editText = new EditText(Context);
            editText.Text = defaultText;
            mainDialogLayout.AddView(editText);
            editText.EditorAction += (sender,args) =>{
                if (args.ActionId == Android.Views.InputMethods.ImeAction.Done)
                {
                    YesFunc();
                }
            };

            RelativeLayout buttonLayout = new RelativeLayout(Context);

            Button yesButton = new Button(Context);
            RelativeLayout.LayoutParams yesParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            yesParams.AddRule(LayoutRules.AlignParentLeft);
            yesButton.Text = okText;

            Button noButton = new Button(Context);
            RelativeLayout.LayoutParams noParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, RelativeLayout.LayoutParams.WrapContent);
            noParams.AddRule(LayoutRules.AlignParentRight);
            noButton.Text = cancelText;

            buttonLayout.AddView(yesButton,yesParams);
            buttonLayout.AddView(noButton,noParams);

            mainDialogLayout.AddView(buttonLayout);


            yesButton.Click += delegate { YesFunc(); };

            noButton.Click += delegate { NoFunc(); };
            
            SetContentView(mainDialogLayout);
        }

        private void YesFunc() {
            if (editText.Text.Length > 0)
            {
                yesAction(editText.Text);
                this.Dismiss();
            }
            else
            {
                Toast.MakeText(Context, emptyFieldMessage, ToastLength.Short).Show();
            }
        }

        private void NoFunc() {
            noAction();
            this.Dismiss();
        }
    }
}