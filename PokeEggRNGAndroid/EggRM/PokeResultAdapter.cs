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
using Pk3DSRNGTool;

namespace Gen7EggRNG.EggRM
{
    class PokeResultAdapter : ArrayAdapter<G7SFrame>
    {
        //private G7SFrame[] pokeFrames;
        //private bool[] checks;

        private FullSearchData searchData;

        private int rowExtraHeight = 0;

        //private int shinyMethod;

        private Android.Graphics.Color shinyColor;

        private bool showStats = false;

        public PokeResultAdapter(Context context, int layoutResource, G7SFrame[] values, FullSearchData searchData, bool showStats) : base(context, layoutResource, values)
        {
            //pokeFrames = values;
            this.searchData = searchData;

            shinyColor = ColorValues.ShinyColor[searchData.preferences.shinyColor];

            this.showStats = showStats;

            rowExtraHeight = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 5.0f * searchData.preferences.rowHeight * searchData.preferences.rowHeight, Context.Resources.DisplayMetrics);
        }

        public void ShowStats(bool showState) {
            showStats = showState;
            NotifyDataSetChanged();
        }

        /*public void CheckItem(int pos)
        {
            checks[pos] = !checks[pos];

            NotifyDataSetChanged();
        }*/

        /*public override long GetItemId(int position)
        {
            return position;
        }*/

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (searchData.searchParameters.type == SearchType.Stationary)
            {
                return GetViewStationary(position, convertView, parent);
            }

            return GetViewStationary(position, convertView, parent);
        }

        private View GetViewStationary(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultStationaryHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemStationary, parent, false);

                holder = new ResultStationaryHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.resstLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;

                holder.frameView = (TextView)view.FindViewById(Resource.Id.resstFrame);
                holder.shiftView = (TextView)view.FindViewById(Resource.Id.resstShift);
                holder.markView = (TextView)view.FindViewById(Resource.Id.resstMark);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.resstHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.resstAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.resstDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.resstSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.resstSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.resstSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.resstHPType);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.resstNature);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.resstGender);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.resstAbility);
                holder.delayView = (TextView)view.FindViewById(Resource.Id.resstDelay);
                holder.realTimeView = (TextView)view.FindViewById(Resource.Id.resstRealTime);

                //holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                view.Tag = holder;
            }
            else
            {
                holder = (ResultStationaryHolder)view.Tag;
            }

            G7SFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                Result7 poke = currentFrame.pokemon;

                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.shiftView.Text = currentFrame.ShiftF.ToString("+#;-#;0");
                holder.delayView.Text = currentFrame.FrameDelayUsed.ToString("+#;-#;0");
                holder.markView.Text = currentFrame.Mark;
                SetGender(holder.genderView, currentFrame.pokemon.Gender);
                holder.natureView.Text = currentFrame.GetNatureStr();
                holder.abilityView.Text = currentFrame.AbilityStr;
                //holder.abilityView.Text = EggDataConversion.GetAbilityString(er.abilityRnum);
                //holder.ballView.Text = currentFrame.Ball;

                if (showStats)
                {
                    if (currentFrame.pokemon.Stats == null) {
                        currentFrame.pokemon.Stats = Pokemon.getStats(currentFrame.pokemon.IVs, currentFrame.pokemon.Nature, currentFrame.pokemon.Level, searchData.stationary.baseStats);
                    }
                    int[] stats = currentFrame.pokemon.Stats;
                    holder.hpView.Text = stats[0].ToString();
                    holder.atkView.Text = stats[1].ToString();
                    holder.defView.Text = stats[2].ToString();
                    holder.spaView.Text = stats[3].ToString();
                    holder.spdView.Text = stats[4].ToString();
                    holder.speView.Text = stats[5].ToString();
                }
                else {
                    holder.hpView.Text = currentFrame.HP.ToString();
                    holder.atkView.Text = currentFrame.Atk.ToString();
                    holder.defView.Text = currentFrame.Def.ToString();
                    holder.spaView.Text = currentFrame.SpA.ToString();
                    holder.spdView.Text = currentFrame.SpD.ToString();
                    holder.speView.Text = currentFrame.Spe.ToString();
                }
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                //holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");
                holder.realTimeView.Text = currentFrame.RealTime;

                // Color Shinies
                if (poke.Shiny)
                {
                    holder.reslayout.SetBackgroundColor(shinyColor);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }

                // Synchronize
                if (searchData.stationary.syncable && currentFrame.pokemon.Synchronize)
                {
                    holder.natureView.SetTextColor(ColorValues.PerfectIVColor);
                    //holder.natureView.SetTypeface(holder.natureView.Typeface, Android.Graphics.TypefaceStyle.Bold);
                }
                else {
                    holder.natureView.SetTextColor(ColorValues.DefaultTextColor);
                    //holder.natureView.SetTypeface(holder.natureView.Typeface, Android.Graphics.TypefaceStyle.Normal);
                }

                // Color Gender
                //ColorGender(holder.genderView, currentFrame);

                // Color Ability
                //ColorAbility(holder.abilityView, currentFrame);

                // Color IVs
                TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                ColorIVs(statViews, currentFrame);

            }

            return view;
        }

        private void SetGender(TextView v, byte gender) {
            v.Text = PokeRNGApp.Strings.genderSymbols[gender];
            if (gender == 1)
            {
                v.SetTextColor(ColorValues.MaleGenderColor);
            }
            else if (gender == 2)
            {
                //v.Text = "♀";
                v.SetTextColor(ColorValues.FemaleGenderColor);
            }
            else
            {
                //v.Text = "-";
                //v.SetText(defaultText)
            }
        }

        private void ColorIVs(TextView[] views, G7SFrame frame)
        {
            for (int i = 0; i < 6; ++i)
            {
                if (frame.pokemon.IVs[i] == 0)
                {
                    views[i].SetTextColor(ColorValues.NoGoodIVColor);
                }
                else if (frame.pokemon.IVs[i] >= 30)
                {
                    views[i].SetTextColor(ColorValues.PerfectIVColor);
                }
                else
                {
                    views[i].SetTextColor(ColorValues.DefaultTextColor);
                }
            }
        }

        public void InflateListGuideline(ViewGroup parent)
        {
            LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
            View view = null;
            if (searchData.searchParameters.type == SearchType.Stationary)
            {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuideStationary, parent, true);
            }
            else
            {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuidePlus, parent, true);
            }
        }
    }

    public class ResultStationaryHolder : Java.Lang.Object
    {
        public LinearLayout reslayout;
        public TextView frameView;
        public TextView shiftView;
        public TextView markView;
        public TextView hpView;
        public TextView atkView;
        public TextView defView;
        public TextView spaView;
        public TextView spdView;
        public TextView speView;
        public TextView hiddenpView;
        public TextView natureView;
        public TextView genderView;
        public TextView abilityView;
        public TextView delayView;
        public TextView realTimeView;
    }
}