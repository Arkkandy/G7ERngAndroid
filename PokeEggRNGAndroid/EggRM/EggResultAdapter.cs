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

using Android.Text;
using Android.Text.Style;
using Pk3DSRNGTool;
using Pk3DSRNGTool.Core;
using Android.Content.Res;

//using Android.Support.V4;

namespace Gen7EggRNG.EggRM
{
    class EggResultAdapter : ArrayAdapter<G7EFrame>
    {
        private G7EFrame[] eggFrames;
        private bool[] checks;

        private FullSearchData searchData;

        private int rowExtraHeight = 0;
        private bool isRandomGender = false;

        private int shinyMethod;

        private Android.Graphics.Color fadedGray;
        private Android.Graphics.Color acceptColor;
        private Android.Graphics.Color rejectColor;
        private Android.Graphics.Color shinyColor;
        private Android.Graphics.Color otherTsvColor;

        public EggResultAdapter(Context context, int layoutResource, G7EFrame[] values, FullSearchData searchData) : base(context, layoutResource, values) {
            eggFrames = values;
            this.searchData = searchData;
            this.isRandomGender = GenderConversion.IsRandomGender((GenderConversion.GenderType)searchData.parents.genderCode);

            shinyMethod = (searchData.parents.isMasuda ? 1 : 0) + (searchData.profile.shinyCharm ? 2 : 0);

            fadedGray = new Android.Graphics.Color(128, 128, 128, 32);
            acceptColor = new Android.Graphics.Color(0, 255, 0, 64);
            rejectColor = new Android.Graphics.Color(255, 0, 0, 64);
            shinyColor = ColorValues.ShinyColor[searchData.preferences.shinyColor];
            otherTsvColor = ColorValues.ShinyColor[searchData.preferences.otherTsvColor];

            rowExtraHeight = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 5.0f * searchData.preferences.rowHeight * searchData.preferences.rowHeight, Context.Resources.DisplayMetrics);

            if (searchData.searchParameters.type == SearchType.ShortestPath) {
                checks = Enumerable.Repeat(false, values.Count()).ToArray();
            }
        }

        public void CheckItem(int pos) {
            checks[pos] = !checks[pos];

            NotifyDataSetChanged();
        }

        /*public override long GetItemId(int position)
        {
            return position;
        }*/

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (searchData.searchParameters.type == SearchType.NormalSearch)
            {
                return GetViewNormalSearch(position, convertView, parent);
            }
            else if (searchData.searchParameters.type == SearchType.EggAccept)
            {
                return GetViewEggAcceptList(position, convertView, parent);
            }
            else if (searchData.searchParameters.type == SearchType.EggAcceptPlus)
            {
                return GetViewEggAcceptPlus(position, convertView, parent);
            }
            else if (searchData.searchParameters.type == SearchType.ShortestPath) {
                return GetViewEggShortestPath(position, convertView, parent);
            }
            else if (searchData.searchParameters.type == SearchType.LeastAdvances)
            {
                return GetViewLeastAdvances(position, convertView, parent);
            }
            else if (searchData.searchParameters.type == SearchType.MainEggRNG)
            {
                return GetViewMainEggRNG(position, convertView, parent);
            }

            return GetViewNormalSearch(position, convertView, parent);
        }

        private View GetViewNormalSearch(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultNormalHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemNormal, parent, false);

                holder = new ResultNormalHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.resLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.frameView = (TextView)view.FindViewById(Resource.Id.resFrame);
                holder.advView = (TextView)view.FindViewById(Resource.Id.resAdvance);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.resGender);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.resNature);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.resAbility);
                holder.ballView = (TextView)view.FindViewById(Resource.Id.resBall);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.resHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.resAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.resDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.resSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.resSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.resSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.resHiddenP);
                holder.tsvView = (TextView)view.FindViewById(Resource.Id.resTSV);

                holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                view.Tag = holder;
            }
            else {
                holder = (ResultNormalHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                ResultE7 er = currentFrame.egg;

                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.advView.Text = currentFrame.GetAdvance();
                // GENDER
                holder.natureView.Text = currentFrame.GetNatureStr();
                //holder.abilityView.Text = currentFrame.AbilityStr;
                //holder.abilityView.Text = EggDataConversion.GetAbilityString(er.abilityRnum);
                holder.ballView.Text = currentFrame.Ball;
                holder.hpView.Text = currentFrame.HP.ToString();
                holder.atkView.Text = currentFrame.Atk.ToString();
                holder.defView.Text = currentFrame.Def.ToString();
                holder.spaView.Text = currentFrame.SpA.ToString();
                holder.spdView.Text = currentFrame.SpD.ToString();
                holder.speView.Text = currentFrame.Spe.ToString();
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");

                // Color Shinies
                if (er.Shiny)
                {
                    if (er.PSV == (uint)searchData.profile.TSV)
                    {
                        holder.reslayout.SetBackgroundColor(shinyColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(otherTsvColor);
                    }
                    //reslayout.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }

                // Color Gender
                ColorGender(holder.genderView, currentFrame );

                // Color Ability
                ColorAbility(holder.abilityView, currentFrame );

                // Color IVs
                if (er != null)
                {
                    TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                    ColorIVs(statViews, currentFrame );
                }

            }

            return view;
        }

        private View GetViewEggAcceptList(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultPlusHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemPlus, parent, false);

                holder = new ResultPlusHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.respLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.eggnumView = (TextView)view.FindViewById(Resource.Id.respEggN);
                holder.frameView = (TextView)view.FindViewById(Resource.Id.respFrame);
                holder.advView = (TextView)view.FindViewById(Resource.Id.respAdv);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.respGender);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.respNature);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.respAbility);
                holder.ballView = (TextView)view.FindViewById(Resource.Id.respBall);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.respHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.respAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.respDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.respSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.respSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.respSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.respHiddenPower);
                holder.tsvView = (TextView)view.FindViewById(Resource.Id.respTSV);

                holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                view.Tag = holder;
            }
            else
            {
                holder = (ResultPlusHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                ResultE7 er = currentFrame.egg;

                holder.eggnumView.Text = currentFrame.EggNum.ToString();
                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.advView.Text = currentFrame.GetAdvance();
                //holder.genderView.Text = GenderConversion.GetRandomGenderString(currentFrame.EC); //currentFrame.GenderStr;
                holder.natureView.Text = currentFrame.GetNatureStr();
                //holder.abilityView.Text = currentFrame.AbilityStr;
                holder.ballView.Text = currentFrame.Ball;
                holder.hpView.Text = currentFrame.HP.ToString();
                holder.atkView.Text = currentFrame.Atk.ToString();
                holder.defView.Text = currentFrame.Def.ToString();
                holder.spaView.Text = currentFrame.SpA.ToString();
                holder.spdView.Text = currentFrame.SpD.ToString();
                holder.speView.Text = currentFrame.Spe.ToString();
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");

                // Color Shinies
                if (er.Shiny)
                {
                    if (er.PSV == (uint)searchData.profile.TSV)
                    {
                        holder.reslayout.SetBackgroundColor(shinyColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(otherTsvColor);
                    }
                    //reslayout.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }

                // Color Gender
                ColorGender(holder.genderView, currentFrame);

                // Color Ability
                ColorAbility(holder.abilityView, currentFrame);

                // Color IVs
                if (er != null)
                {
                    TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                    ColorIVs(statViews, currentFrame);
                }

            }

            return view;
        }

        private View GetViewEggAcceptPlus(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultPlusHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemPlus, parent, false);

                holder = new ResultPlusHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.respLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.eggnumView = (TextView)view.FindViewById(Resource.Id.respEggN);
                holder.frameView = (TextView)view.FindViewById(Resource.Id.respFrame);
                holder.advView = (TextView)view.FindViewById(Resource.Id.respAdv);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.respGender);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.respNature);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.respAbility);
                holder.ballView = (TextView)view.FindViewById(Resource.Id.respBall);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.respHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.respAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.respDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.respSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.respSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.respSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.respHiddenPower);
                holder.tsvView = (TextView)view.FindViewById(Resource.Id.respTSV);

                holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                view.Tag = holder;
            }
            else
            {
                holder = (ResultPlusHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                ResultE7 er = currentFrame.egg;

                holder.eggnumView.Text = currentFrame.EggNum.ToString();
                holder.frameView.Text = currentFrame.FrameNum.ToString();
                if (currentFrame.AdvType == AdvanceType.Accept)
                {
                    holder.advView.Text = currentFrame.GetAdvance();
                }
                else {
                    holder.advView.Text = currentFrame.AdvanceInfo;
                }
                //holder.genderView.Text = currentFrame.GenderStr;
                holder.natureView.Text = currentFrame.GetNatureStr();
                //holder.abilityView.Text = currentFrame.AbilityStr;
                holder.ballView.Text = currentFrame.Ball;
                holder.hpView.Text = currentFrame.HP.ToString();
                holder.atkView.Text = currentFrame.Atk.ToString();
                holder.defView.Text = currentFrame.Def.ToString();
                holder.spaView.Text = currentFrame.SpA.ToString();
                holder.spdView.Text = currentFrame.SpD.ToString();
                holder.speView.Text = currentFrame.Spe.ToString();
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");

                // Color Shinies
                if (er.Shiny)
                {
                    if (er.PSV == (uint)searchData.profile.TSV)
                    {
                        holder.reslayout.SetBackgroundColor(shinyColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(otherTsvColor);
                    }
                    //reslayout.SetTextColor(Android.Graphics.Color.Black);
                }
                else if (currentFrame.AdvType == AdvanceType.Reject )
                {

                    holder.reslayout.SetBackgroundColor(fadedGray);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }

                // Color Gender
                ColorGender(holder.genderView, currentFrame);

                // Color Ability
                ColorAbility(holder.abilityView, currentFrame);

                // Color IVs
                if (er != null)
                {
                    TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                    ColorIVs(statViews, currentFrame);
                }

            }

            return view;
        }

        private View GetViewEggShortestPath(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultPlusHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemPlus, parent, false);

                holder = new ResultPlusHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.respLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.eggnumView = (TextView)view.FindViewById(Resource.Id.respEggN);
                holder.frameView = (TextView)view.FindViewById(Resource.Id.respFrame);
                holder.advView = (TextView)view.FindViewById(Resource.Id.respAdv);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.respGender);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.respNature);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.respAbility);
                holder.ballView = (TextView)view.FindViewById(Resource.Id.respBall);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.respHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.respAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.respDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.respSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.respSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.respSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.respHiddenPower);
                holder.tsvView = (TextView)view.FindViewById(Resource.Id.respTSV);

                holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                /*holder.reslayout.Click += (sender, args) =>
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                };*/

                view.Tag = holder;
            }
            else
            {
                holder = (ResultPlusHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                ResultE7 er = currentFrame.egg;

                holder.eggnumView.Text = currentFrame.EggNum.ToString();
                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.advView.Text = currentFrame.GetAdvanceTypeString();
                //holder.genderView.Text = currentFrame.GenderStr;
                holder.natureView.Text = currentFrame.GetNatureStr();
                //holder.abilityView.Text = currentFrame.AbilityStr;
                holder.ballView.Text = currentFrame.Ball;
                holder.hpView.Text = currentFrame.HP.ToString();
                holder.atkView.Text = currentFrame.Atk.ToString();
                holder.defView.Text = currentFrame.Def.ToString();
                holder.spaView.Text = currentFrame.SpA.ToString();
                holder.spdView.Text = currentFrame.SpD.ToString();
                holder.speView.Text = currentFrame.Spe.ToString();
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");



                // Color Accepts/Rejects
                if (checks[position])
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                }
                else
                {
                    if (currentFrame.AdvType == AdvanceType.Accept)
                    {
                        holder.reslayout.SetBackgroundColor(acceptColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(rejectColor);
                    }
                }

                // Color Gender
                ColorGender(holder.genderView, currentFrame );

                // Color Ability
                ColorAbility(holder.abilityView, currentFrame );

                // Color IVs
                if (er != null)
                {
                    TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                    ColorIVs(statViews, currentFrame );
                }

            }

            return view;
        }

        private View GetViewLeastAdvances(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultNormalHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemNormal, parent, false);

                holder = new ResultNormalHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.resLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.frameView = (TextView)view.FindViewById(Resource.Id.resFrame);
                holder.advView = (TextView)view.FindViewById(Resource.Id.resAdvance);
                holder.genderView = (TextView)view.FindViewById(Resource.Id.resGender);
                holder.natureView = (TextView)view.FindViewById(Resource.Id.resNature);
                holder.abilityView = (TextView)view.FindViewById(Resource.Id.resAbility);
                holder.ballView = (TextView)view.FindViewById(Resource.Id.resBall);
                holder.hpView = (TextView)view.FindViewById(Resource.Id.resHP);
                holder.atkView = (TextView)view.FindViewById(Resource.Id.resAtk);
                holder.defView = (TextView)view.FindViewById(Resource.Id.resDef);
                holder.spaView = (TextView)view.FindViewById(Resource.Id.resSpA);
                holder.spdView = (TextView)view.FindViewById(Resource.Id.resSpD);
                holder.speView = (TextView)view.FindViewById(Resource.Id.resSpe);
                holder.hiddenpView = (TextView)view.FindViewById(Resource.Id.resHiddenP);
                holder.tsvView = (TextView)view.FindViewById(Resource.Id.resTSV);

                holder.genderView.SetTextSize(Android.Util.ComplexUnitType.Sp, 10);

                view.Tag = holder;
            }
            else
            {
                holder = (ResultNormalHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                ResultE7 er = currentFrame.egg;

                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.advView.Text = currentFrame.AdvanceInfo;
                // GENDER
                holder.natureView.Text = currentFrame.GetNatureStr();
                //holder.abilityView.Text = currentFrame.AbilityStr;
                //holder.abilityView.Text = EggDataConversion.GetAbilityString(er.abilityRnum);
                holder.ballView.Text = currentFrame.Ball;
                holder.hpView.Text = currentFrame.HP.ToString();
                holder.atkView.Text = currentFrame.Atk.ToString();
                holder.defView.Text = currentFrame.Def.ToString();
                holder.spaView.Text = currentFrame.SpA.ToString();
                holder.spdView.Text = currentFrame.SpD.ToString();
                holder.speView.Text = currentFrame.Spe.ToString();
                holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
                holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");

                // Color Shinies
                if (er.Shiny)
                {
                    if (er.PSV == (uint)searchData.profile.TSV)
                    {
                        holder.reslayout.SetBackgroundColor(shinyColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(otherTsvColor);
                    }
                    //reslayout.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }

                // Color Gender
                ColorGender(holder.genderView, currentFrame);

                // Color Ability
                ColorAbility(holder.abilityView, currentFrame);

                // Color IVs
                if (er != null)
                {
                    TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                    ColorIVs(statViews, currentFrame);
                }

            }

            return view;
        }

        private View GetViewMainEggRNG(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            ResultMainEggHolder holder = null;
            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItemMainEgg, parent, false);

                holder = new ResultMainEggHolder();
                holder.reslayout = (LinearLayout)view.FindViewById(Resource.Id.resmeLayout);
                ViewGroup.LayoutParams lparams = holder.reslayout.LayoutParameters;
                lparams.Height += rowExtraHeight;
                holder.reslayout.LayoutParameters = lparams;
                holder.frameView = (TextView)view.FindViewById(Resource.Id.resmeFrame);
                holder.shiftView = (TextView)view.FindViewById(Resource.Id.resmeShift);
                holder.markView = (TextView)view.FindViewById(Resource.Id.resmeMark);
                holder.realtimeView = (TextView)view.FindViewById(Resource.Id.resmeRealtime);
                holder.delayView = (TextView)view.FindViewById(Resource.Id.resmeDelay);
                holder.psvView = (TextView)view.FindViewById(Resource.Id.resmePSV);

                view.Tag = holder;
            }
            else
            {
                holder = (ResultMainEggHolder)view.Tag;
            }

            G7EFrame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                holder.frameView.Text = currentFrame.FrameNum.ToString();
                holder.shiftView.Text = currentFrame.ShiftF.ToString("+#;-#;0");
                holder.markView.Text = currentFrame.Mark;
                holder.realtimeView.Text = currentFrame.RealTime;
                holder.delayView.Text = currentFrame.FrameDelayUsed.ToString("+#;-#;0");
                holder.psvView.Text = currentFrame.MainPSV.ToString("0000");

                // Color Shinies
                if (currentFrame.MainShiny)
                {
                    if (currentFrame.MainPSV == (uint)searchData.profile.TSV)
                    {
                        holder.reslayout.SetBackgroundColor(shinyColor);
                    }
                    else
                    {
                        holder.reslayout.SetBackgroundColor(otherTsvColor);
                    }
                    //reslayout.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    //reslayout.SetTextColor(Android.Graphics.Color.White);
                }
            }

            return view;
        }

        private void ColorGender(TextView v, G7EFrame frame) {
            if (isRandomGender && searchData.preferences.allRandomGender && !searchData.parents.isNidoSpecies)
            {
                string genderstring = GenderConversion.GetRandomGenderString(frame.egg.genderRnum);

                SpannableStringBuilder genderSsb = new SpannableStringBuilder(genderstring);
                var genderVals = GenderConversion.GetRandomGenderBool(frame.egg.genderRnum);
                for (int i = 0; i < genderVals.Count; ++i)
                {
                    genderSsb.SetSpan(new ForegroundColorSpan(genderVals[i] ? ColorValues.MaleGenderColor : ColorValues.FemaleGenderColor), i, i + 1, SpanTypes.ExclusiveExclusive);
                }
                v.TextFormatted = genderSsb;
            }
            else
            {
                if (frame.egg.Gender == 1)
                {
                    v.Text = "♂";
                    v.SetTextColor(ColorValues.MaleGenderColor);
                }
                else if (frame.egg.Gender == 2)
                {
                    v.Text = "♀";
                    v.SetTextColor(ColorValues.FemaleGenderColor);
                }
                else {
                    v.Text = "-";
                }
            }
        }

        private void ColorAbility(TextView v, G7EFrame frame) {
            v.Text = (searchData.preferences.allAbility ? EggDataConversion.GetAbilityString(frame.egg.abilityRnum) : frame.AbilityStr);
            /*if (res.CanInheritHidden)
            {
                v.SetTextColor(Android.Graphics.Color.ForestGreen);
            }
            else
            {
                v.SetTextColor(Android.Graphics.Color.White);
            }*/
        }

        private void ColorIVs(TextView[] views, G7EFrame frame ) {
            for (int i = 0; i < 6; ++i)
            {
                if (frame.egg.InheritMaleIV[i] != null)
                {
                    if (frame.egg.InheritMaleIV[i] == true)
                    {
                        views[i].SetTextColor(ColorValues.MaleGenderColor);
                    }
                    else // (er.InheritMaleIV[i] == false)
                    {
                        views[i].SetTextColor(ColorValues.FemaleGenderColor);
                    }
                }
                else
                {
                    if (frame.egg.IVs[i] == 0)
                    {
                        views[i].SetTextColor(ColorValues.NoGoodIVColor);
                    }
                    else if (frame.egg.IVs[i] >= 30)
                    {
                        views[i].SetTextColor(ColorValues.PerfectIVColor);
                    }
                    else
                    {
                        views[i].SetTextColor(ColorValues.DefaultTextColor);
                    }
                }
            }
        }

        public void InflateListGuideline(ViewGroup parent) {
            LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
            View view = null;
            if (searchData.searchParameters.type == SearchType.NormalSearch)
            {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuideNormal, parent, true);
            }
            else if (searchData.searchParameters.type == SearchType.LeastAdvances)
            {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuideNormal, parent, true);
            }
            else if (searchData.searchParameters.type == SearchType.MainEggRNG)
            {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuideMainEgg, parent, true);
            }
            else {
                view = layoutInflater.Inflate(Resource.Layout.ResultItemGuidePlus, parent, true);
            }
        }


        public void FillNormalHolder(G7EFrame currentFrame, ResultNormalHolder holder) {
            ResultE7 er = currentFrame.egg;

            holder.frameView.Text = currentFrame.FrameNum.ToString();
            holder.advView.Text = currentFrame.GetAdvance();
            // GENDER
            holder.natureView.Text = currentFrame.GetNatureStr();
            //holder.abilityView.Text = currentFrame.AbilityStr;
            //holder.abilityView.Text = EggDataConversion.GetAbilityString(er.abilityRnum);
            holder.ballView.Text = currentFrame.Ball;
            holder.hpView.Text = currentFrame.HP.ToString();
            holder.atkView.Text = currentFrame.Atk.ToString();
            holder.defView.Text = currentFrame.Def.ToString();
            holder.spaView.Text = currentFrame.SpA.ToString();
            holder.spdView.Text = currentFrame.SpD.ToString();
            holder.speView.Text = currentFrame.Spe.ToString();
            holder.hiddenpView.Text = currentFrame.GetHiddenPowerStr();
            holder.tsvView.Text = (shinyMethod != 0 ? currentFrame.PSV.ToString("0000") : "----");

            // Color Shinies
            if (er.Shiny)
            {
                if (er.PSV == (uint)searchData.profile.TSV)
                {
                    holder.reslayout.SetBackgroundColor(shinyColor);
                }
                else
                {
                    holder.reslayout.SetBackgroundColor(otherTsvColor);
                }
                //reslayout.SetTextColor(Android.Graphics.Color.Black);
            }
            else
            {
                holder.reslayout.SetBackgroundColor(Android.Graphics.Color.Transparent);
                //reslayout.SetTextColor(Android.Graphics.Color.White);
            }

            // Color Gender
            ColorGender(holder.genderView, currentFrame);

            // Color Ability
            ColorAbility(holder.abilityView, currentFrame);

            // Color IVs
            if (er != null)
            {
                TextView[] statViews = { holder.hpView, holder.atkView, holder.defView, holder.spaView, holder.spdView, holder.speView };
                ColorIVs(statViews, currentFrame);
            }
        }

        //Fill in cound here, currently 0
        /*public override int Count
        {
            get
            {
                return 0;
            }
        }*/

    }


    public class ResultNormalHolder : Java.Lang.Object
    {
        public LinearLayout reslayout;
        public TextView frameView;
        public TextView advView;
        public TextView genderView;
        public TextView natureView;
        public TextView abilityView;
        public TextView ballView;
        public TextView hpView;
        public TextView atkView;
        public TextView defView;
        public TextView spaView;
        public TextView spdView;
        public TextView speView;
        public TextView hiddenpView;
        public TextView tsvView;
    }

    public class ResultPlusHolder : Java.Lang.Object
    {
        public LinearLayout reslayout;
        public TextView eggnumView;
        public TextView frameView;
        public TextView advView;
        public TextView genderView;
        public TextView natureView;
        public TextView abilityView;
        public TextView ballView;
        public TextView hpView;
        public TextView atkView;
        public TextView defView;
        public TextView spaView;
        public TextView spdView;
        public TextView speView;
        public TextView hiddenpView;
        public TextView tsvView;
    }

    public class ResultShortestHolder : Java.Lang.Object
    {
        public LinearLayout reslayout;
        public CheckedTextView eggnumView;
        public TextView frameView;
        public TextView advView;
        public TextView genderView;
        public TextView natureView;
        public TextView abilityView;
        public TextView ballView;
        public TextView hpView;
        public TextView atkView;
        public TextView defView;
        public TextView spaView;
        public TextView spdView;
        public TextView speView;
        public TextView hiddenpView;
        public TextView tsvView;
    }

    public class ResultMainEggHolder : Java.Lang.Object {
        public LinearLayout reslayout;
        public TextView frameView;
        public TextView shiftView;
        public TextView markView;
        public TextView realtimeView;
        public TextView delayView;
        public TextView psvView;
    }

    /*class EggResultAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }*/
}

// FAST MODE BACKUP
/*private View GetViewNormalSearch(int position, View convertView, ViewGroup parent) {
            var view = convertView;

            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItem, null);
            }

            Frame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                // Fetch text view
                TextView dataLine = (TextView)view.FindViewById(Resource.Id.resultData);

                EggResult er = currentFrame.rt as EggResult;

                string frameNum = currentFrame.FrameNum.ToString().PadLeft(5) + "  ";
                string frameAdv = currentFrame.FrameUsed + "  ";
                string frameGender = currentFrame.GenderStr + "  ";
                string frameNature = currentFrame.NatureStr.PadRight(7) + "  ";
                string frameAbility = currentFrame.AbilityStr.PadRight(3) + "  ";
                string frameBall = currentFrame.Ball.ToString().PadRight(6) + "  ";
                string frameHP = currentFrame.HP.ToString().PadLeft(2) + " ";
                string frameAtk = currentFrame.Atk.ToString().PadLeft(2) + " ";
                string frameDef = currentFrame.Def.ToString().PadLeft(2) + " ";
                string frameSpA = currentFrame.SpA.ToString().PadLeft(2) + " ";
                string frameSpD = currentFrame.SpD.ToString().PadLeft(2) + " ";
                string frameSpe = currentFrame.Spe.ToString().PadLeft(2) + "  ";
                string frameHiddenPower = HiddenPower.GetHiddenPowerString(er.hiddenpower).PadLeft(8) + "  ";
                string framePSV = currentFrame.PSV.ToString("0000");

                string dataString = frameNum + frameAdv + frameGender + frameNature + frameAbility + frameBall +
                    frameHP + frameAtk + frameDef + frameSpA + frameSpD + frameSpe +
                    frameHiddenPower + framePSV;

                // Color Shinies
                if (er.Shiny)
                {
                    dataLine.SetBackgroundColor(shinyColor);
                    dataLine.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    dataLine.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }


                SpannableStringBuilder ssb = new SpannableStringBuilder(dataString);

                // Color Gender
                if (currentFrame.GenderStr == "M")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }
                else if (currentFrame.GenderStr == "F")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }

                // Color Ability
                if (er.CanInheritHidden)
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexAbility_S, indexAbility_E, SpanTypes.ExclusiveExclusive);
                }

                // Color IVs
                if (er != null)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        if (er.InheritMaleIV[i] != null)
                        {
                            if (er.InheritMaleIV[i] == true)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else // (er.InheritMaleIV[i] == false)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                        else
                        {
                            if (currentFrame.rt.IVs[i] == 0)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Red), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else if (currentFrame.rt.IVs[i] >= 30)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                    }
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 7, 9, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), 10, 12, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 13, 15, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Pink), 16, 18, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 19, 21, SpanTypes.ExclusiveExclusive);
                }

                dataLine.TextFormatted = ssb;
            }

            return view;
        }

        private View GetViewEggAcceptList(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItem, null);
            }

            Frame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                // Fetch text view
                TextView dataLine = (TextView)view.FindViewById(Resource.Id.resultData);

                EggResult er = currentFrame.rt as EggResult;

                string frameNum = currentFrame.FrameNum.ToString().PadLeft(5) + "  ";
                string frameAdv = currentFrame.EggNum.ToString().PadLeft(3) + "  ";
                string frameGender = currentFrame.GenderStr + "  ";
                string frameNature = currentFrame.NatureStr.PadRight(7) + "  ";
                string frameAbility = currentFrame.AbilityStr.PadRight(3) + "  ";
                string frameBall = currentFrame.Ball.ToString().PadRight(6) + "  ";
                string frameHP = currentFrame.HP.ToString().PadLeft(2) + " ";
                string frameAtk = currentFrame.Atk.ToString().PadLeft(2) + " ";
                string frameDef = currentFrame.Def.ToString().PadLeft(2) + " ";
                string frameSpA = currentFrame.SpA.ToString().PadLeft(2) + " ";
                string frameSpD = currentFrame.SpD.ToString().PadLeft(2) + " ";
                string frameSpe = currentFrame.Spe.ToString().PadLeft(2) + "  ";
                string frameHiddenPower = HiddenPower.GetHiddenPowerString(er.hiddenpower).PadLeft(8) + "  ";
                string framePSV = currentFrame.PSV.ToString("0000");

                string dataString = frameNum + frameAdv + frameGender + frameNature + frameAbility + frameBall +
                    frameHP + frameAtk + frameDef + frameSpA + frameSpD + frameSpe +
                    frameHiddenPower + framePSV;

                // Color Shinies
                if (er.Shiny)
                {
                    dataLine.SetBackgroundColor(shinyColor);
                    dataLine.SetTextColor(Android.Graphics.Color.Black);
                }
                else
                {
                    dataLine.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }


                SpannableStringBuilder ssb = new SpannableStringBuilder(dataString);

                // Color Gender
                if (currentFrame.GenderStr == "M")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }
                else if (currentFrame.GenderStr == "F")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }

                // Color Ability
                if (er.CanInheritHidden)
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexAbility_S, indexAbility_E, SpanTypes.ExclusiveExclusive);
                }

                // Color IVs
                if (er != null)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        if (er.InheritMaleIV[i] != null)
                        {
                            if (er.InheritMaleIV[i] == true)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else // (er.InheritMaleIV[i] == false)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                        else
                        {
                            if (currentFrame.rt.IVs[i] == 0)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Red), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else if (currentFrame.rt.IVs[i] >= 30)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                    }
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 7, 9, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), 10, 12, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 13, 15, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Pink), 16, 18, SpanTypes.ExclusiveExclusive);
                    //ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), 19, 21, SpanTypes.ExclusiveExclusive);
                }

                dataLine.TextFormatted = ssb;
            }

            return view;
        }

        private View GetViewEggAcceptPlus(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItem, null);
            }

            Frame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                // Fetch text view
                TextView dataLine = (TextView)view.FindViewById(Resource.Id.resultData);

                EggResult er = currentFrame.rt as EggResult;

                string frameNum = currentFrame.FrameNum.ToString().PadLeft(5) + "  ";
                string frameAdv = currentFrame.EggNum.ToString().PadLeft(3) + "  ";
                string frameGender = currentFrame.GenderStr + "  ";
                string frameNature = currentFrame.NatureStr.PadRight(7) + "  ";
                string frameAbility = currentFrame.AbilityStr.PadRight(3) + "  ";
                string frameBall = currentFrame.Ball.ToString().PadRight(6) + "  ";
                string frameHP = currentFrame.HP.ToString().PadLeft(2) + " ";
                string frameAtk = currentFrame.Atk.ToString().PadLeft(2) + " ";
                string frameDef = currentFrame.Def.ToString().PadLeft(2) + " ";
                string frameSpA = currentFrame.SpA.ToString().PadLeft(2) + " ";
                string frameSpD = currentFrame.SpD.ToString().PadLeft(2) + " ";
                string frameSpe = currentFrame.Spe.ToString().PadLeft(2) + "  ";
                string frameHiddenPower = HiddenPower.GetHiddenPowerString(er.hiddenpower).PadLeft(8) + "  ";
                string framePSV = currentFrame.PSV.ToString("0000");

                string dataString = frameNum + frameAdv + frameGender + frameNature + frameAbility + frameBall +
                    frameHP + frameAtk + frameDef + frameSpA + frameSpD + frameSpe +
                    frameHiddenPower + framePSV;

                // Color Shinies
                if (er.Shiny)
                {
                    dataLine.SetBackgroundColor(shinyColor);
                    dataLine.SetTextColor(Android.Graphics.Color.Black);
                }
                else if (currentFrame.EggNum < 0) {
                    dataLine.SetBackgroundColor(fadedGray);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }
                else
                {
                    dataLine.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }


                SpannableStringBuilder ssb = new SpannableStringBuilder(dataString);

                // Color Gender
                if (currentFrame.GenderStr == "M")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }
                else if (currentFrame.GenderStr == "F")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }

                // Color Ability
                if (er.CanInheritHidden)
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexAbility_S, indexAbility_E, SpanTypes.ExclusiveExclusive);
                }

                // Color IVs
                if (er != null)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        if (er.InheritMaleIV[i] != null)
                        {
                            if (er.InheritMaleIV[i] == true)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else // (er.InheritMaleIV[i] == false)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                        else
                        {
                            if (currentFrame.rt.IVs[i] == 0)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Red), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else if (currentFrame.rt.IVs[i] >= 30)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                    }
                }

                dataLine.TextFormatted = ssb;
            }

            return view;
        }

        private View GetViewEggShortestPath(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;

            if (view == null)
            {
                LayoutInflater layoutInflater = LayoutInflater.FromContext(Context);
                view = layoutInflater.Inflate(Resource.Layout.ResultItem, null);
            }

            Frame currentFrame = GetItem(position);

            if (currentFrame != null)
            {
                // Fetch text view
                TextView dataLine = (TextView)view.FindViewById(Resource.Id.resultData);

                EggResult er = currentFrame.rt as EggResult;

                string frameNum = currentFrame.EggNum.ToString().PadLeft(5) + "  ";
                string frameAdv = currentFrame.FrameUsed + "  ";
                string frameGender = currentFrame.GenderStr + "  ";
                string frameNature = currentFrame.NatureStr.PadRight(7) + "  ";
                string frameAbility = currentFrame.AbilityStr.PadRight(3) + "  ";
                string frameBall = currentFrame.Ball.ToString().PadRight(6) + "  ";
                string frameHP = currentFrame.HP.ToString().PadLeft(2) + " ";
                string frameAtk = currentFrame.Atk.ToString().PadLeft(2) + " ";
                string frameDef = currentFrame.Def.ToString().PadLeft(2) + " ";
                string frameSpA = currentFrame.SpA.ToString().PadLeft(2) + " ";
                string frameSpD = currentFrame.SpD.ToString().PadLeft(2) + " ";
                string frameSpe = currentFrame.Spe.ToString().PadLeft(2) + "  ";
                string frameHiddenPower = HiddenPower.GetHiddenPowerString(er.hiddenpower).PadLeft(8) + "  ";
                string framePSV = currentFrame.PSV.ToString("0000");

                string dataString = frameNum + frameAdv + frameGender + frameNature + frameAbility + frameBall +
                    frameHP + frameAtk + frameDef + frameSpA + frameSpD + frameSpe +
                    frameHiddenPower + framePSV;

                // Color Accepts/Rejects
                if (currentFrame.FrameUsed == "Acc")
                {
                    dataLine.SetBackgroundColor(acceptColor);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }
                else
                {
                    dataLine.SetBackgroundColor(rejectColor);
                    dataLine.SetTextColor(Android.Graphics.Color.White);
                }


                SpannableStringBuilder ssb = new SpannableStringBuilder(dataString);

                // Color Gender
                if (currentFrame.GenderStr == "M")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }
                else if (currentFrame.GenderStr == "F")
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexGender_S, indexGender_E, SpanTypes.ExclusiveExclusive);
                }

                // Color Ability
                if (er.CanInheritHidden)
                {
                    ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexAbility_S, indexAbility_E, SpanTypes.ExclusiveExclusive);
                }

                // Color IVs
                if (er != null)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        if (er.InheritMaleIV[i] != null)
                        {
                            if (er.InheritMaleIV[i] == true)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Blue), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else // (er.InheritMaleIV[i] == false)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.HotPink), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                        else
                        {
                            if (currentFrame.rt.IVs[i] == 0)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Red), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                            else if (currentFrame.rt.IVs[i] >= 30)
                            {
                                ssb.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.ForestGreen), indexIVs[i * 2], indexIVs[i * 2 + 1], SpanTypes.ExclusiveExclusive);
                            }
                        }
                    }
                }

                dataLine.TextFormatted = ssb;
            }

            return view;
        }*/
