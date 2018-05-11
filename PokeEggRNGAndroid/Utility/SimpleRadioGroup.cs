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

namespace Gen7EggRNG.Utility
{
    public class SimpleRadioGroup
    {
        private List<RadioButton> radioButtons = new List<RadioButton>();

        public SimpleRadioGroup() {}

        public void Add(RadioButton rb) { radioButtons.Add(rb); }
        public void CheckButtonPosition(RadioButton rb)
        {
            if (radioButtons.Contains(rb)) {
                for (int i = 0; i < radioButtons.Count; ++i)
                {
                    radioButtons[i].Checked = (radioButtons[i] == rb);
                }
            }
        }
        public void CheckButtonPosition(int position) {
            if (0 <= position && position < radioButtons.Count)
            {
                for (int i = 0; i < radioButtons.Count; ++i)
                {
                    radioButtons[i].Checked = i == position;
                }
            }
        }
        public void UncheckOthers(RadioButton rb) {
            if (radioButtons.Contains(rb))
            {
                for (int i = 0; i < radioButtons.Count; ++i)
                {
                    if (radioButtons[i] != rb)
                        radioButtons[i].Checked = false;
                }
            }
        }
        public void UncheckOthersPosition(int position)
        {
            if (0 <= position && position < radioButtons.Count)
            {
                for (int i = 0; i < radioButtons.Count; ++i)
                {
                    if (i != position)
                    {
                        radioButtons[i].Checked = false;
                    }
                }
            }
        }
    }
}