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

//using System.Collections.Generic;

namespace Gen7EggRNG.Widgets
{
    public class SpinnerCheckboxAdapter : ArrayAdapter<SpinnerCheckboxContainer> {

        private string totalSelection;
        private bool isFromView = false;
        private string defaultString;

        public SpinnerCheckboxAdapter(Context context, int resource, SpinnerCheckboxContainer[] list, string defaultMessage) : base(context, resource, list) {
            defaultString = defaultMessage;
            //UpdateSelectionString();
        }

        private void UpdateSelectionString() {
            totalSelection = String.Empty;
            for (int i = 0; i < Count; ++i) {
                if (GetItem(i).IsSelected()) { totalSelection += GetItem(i).GetTitle() + " "; }
            }
            if ( totalSelection == String.Empty ) { totalSelection = defaultString; }
        }

        public List<int> GetSelectionValues() {
            List<int> newList = new List<int>();
            for (int i = 0; i < Count; ++i) {
                if (GetItem(i).IsSelected())
                {
                    newList.Add(i);
                }
            }
            return newList;
        }

        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            return GetCustomView(position, convertView, parent);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //return GetCustomView(position, convertView, parent);
            var view = convertView;

            if (view == null)
            {
                TextView tv = new TextView(Context);
                UpdateSelectionString();
                tv.Text = totalSelection;

                view = tv;
            }
            else {
                UpdateSelectionString();
                (view as TextView).Text = totalSelection;
            }

            return view;
        }

        public View GetCustomView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            // Create new view if none currently exists
            SpinnerCheckboxAdapterViewHolder holder;
            if (view == null)
            {
                LayoutInflater layoutInflator = LayoutInflater.FromContext(Context);
                view = layoutInflator.Inflate(Resource.Layout.SpinnerMulti, null);

                holder = new SpinnerCheckboxAdapterViewHolder();
                holder.Title = (TextView)view.FindViewById(Resource.Id.spinnerMultiText);
                holder.Check = (CheckBox)view.FindViewById(Resource.Id.spinnerMultiCheck);
                view.Tag = holder;

                // CheckBox event handler
                holder.Check.CheckedChange += (sender, args) => {
                    if (!isFromView)
                    {
                        GetItem((int)holder.Check.Tag).SetSelected(args.IsChecked);
                        NotifyDataSetChanged();
                    }
                };
            }
            else {
                holder = view.Tag as SpinnerCheckboxAdapterViewHolder;
            }

            holder.Check.Tag = position;

            // Soft-disable on the CheckBox event handler
            isFromView = true;
            holder.Title.Text = GetItem(position).GetTitle();
            holder.Check.Checked = GetItem(position).IsSelected();
            isFromView = false;

            return view;
        }
    }

    class SpinnerCheckboxAdapterViewHolder : Java.Lang.Object
    {
        public TextView Title { get; set; }
        public CheckBox Check { get; set; }
    }
}