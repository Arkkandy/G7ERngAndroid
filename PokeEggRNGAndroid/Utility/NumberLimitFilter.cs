using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;


namespace Gen7EggRNG.Util
{

    public class TSVTextWatcher : Java.Lang.Object, ITextWatcher
    {
        private EditText TsvField;

        private string before;

        public TSVTextWatcher(EditText editText) {
            TsvField = editText;
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            before = s.ToString();
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            // Nothing to do here
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
            TsvField.RemoveTextChangedListener(this);

            string after = s.ToString();

            if (after.Length > 4)
            {
                int value = 0;
                if (int.TryParse(after, out value))
                {
                    if (value <= 4095)
                    {
                        s.Clear();
                        s.Append(value.ToString().PadLeft(4, '0'));
                    }
                    else
                    {
                        s.Delete(s.Length() - 1, s.Length());
                    }
                }
            }
            else if (after.Length <= 4)
            {
                int value = 0;
                if (int.TryParse(after, out value)) {
                    if (value > 4095)
                    {
                        s.Clear();
                        s.Append("4095");
                    }
                    else {
                        s.Clear();
                        s.Append(value.ToString().PadLeft(4, '0'));
                    }
                }
            }

            TsvField.AddTextChangedListener(this);
        }
    }

    public class HexadecimalInputFilter : Java.Lang.Object, IInputFilter {
        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {

            string s = source.ToString();
            string d = "";

            for (int i = start; i < end; i++)
            {
                if (StringUtil.IsCharHex(s[i]))
                {
                    d += Character.ToUpperCase(s[i]);
                }
            }
            return new Java.Lang.String(d);
        }
    }

    /*public class NumberLimitFilter : Java.Lang.Object, IInputFilter
    {
        private int MaxValue;

        public NumberLimitFilter(int max) {
            MaxValue = max;
        }

        Java.Lang.ICharSequence IInputFilter.FilterFormatted(Java.Lang.ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (source.Length() == 0) {
                string val = dest.ToString().Remove(dstart, dend-dstart);
                return new Java.Lang.String(val.PadLeft(4, '0'));
            }
            try
            {
                string val = dest.ToString().Insert(dstart, source.ToString());
                int input = int.Parse(val);
                if (0 <= input && input <= MaxValue)
                {
                    string newVal = input.ToString();
                    return new Java.Lang.String(newVal.PadLeft(4, '0'));
                }
                if (input > 4095) {
                    return new Java.Lang.String("4095");
                }
            }
            catch (Exception ex)
            {
            }

            return new Java.Lang.String(string.Empty);
        }
    }*/
}