using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using Pk3DSRNGTool;
using Pk3DSRNGTool.Core;

namespace Gen7EggRNG.EggRM
{
    public class DetailedFrame : Dialog
    {
        private G7EFrame eggFrame;
        private FullSearchData currentSearchData;

        ImageView dShinyStar;
        TextView dFrameAdv;
        TextView dIvs;
        TextView dHp;
        TextView dGender;
        TextView dAbility;
        TextView dNature;
        TextView dBall;
        TextView dPSV;
        TextView dSeed;
        LinearLayout dStats;
        LinearLayout dExtras;
        Button dDone;
        Button dCopy;

        SpannableStringBuilder ivSsb;
        SpannableStringBuilder ivInherSsb;
        SpannableStringBuilder genderSsb;

        bool ivMode = false;
        int metaMode = 0;
        bool extraMode = false;

        //string ivstring;
        //string ivinherstring;

        public DetailedFrame(G7EFrame frame, FullSearchData currentData, Context context) : base(context)
        {
            eggFrame = frame;
            currentSearchData = currentData;
        }

        public void Initialize() {
            base.SetContentView(Resource.Layout.FrameDetailedDialog);

            string frameString = String.Format(Context.Resources.GetString(Resource.String.search_detailed_frame), eggFrame.FrameNum);
            base.SetTitle(frameString);

            dShinyStar = (ImageView)base.FindViewById(Resource.Id.detailIsShiny);
            dFrameAdv = (TextView)base.FindViewById(Resource.Id.detailFrameAdv);
            dIvs = (TextView)base.FindViewById(Resource.Id.detailIV);
            dHp = (TextView)base.FindViewById(Resource.Id.detailHP);
            dGender = (TextView)base.FindViewById(Resource.Id.detailGender);
            dAbility = (TextView)base.FindViewById(Resource.Id.detailAbility);
            dNature = (TextView)base.FindViewById(Resource.Id.detailNature);
            dBall = (TextView)base.FindViewById(Resource.Id.detailBall);
            dPSV = (TextView)base.FindViewById(Resource.Id.detailPSV);
            dSeed = (TextView)base.FindViewById(Resource.Id.detailSeed);
            dStats = (LinearLayout)base.FindViewById(Resource.Id.detailStatsGroup);
            dExtras = (LinearLayout)base.FindViewById(Resource.Id.detailExtras);
            dDone = (Button)base.FindViewById(Resource.Id.detailDone);
            dCopy = (Button)base.FindViewById(Resource.Id.detailCopySpread);

            ResultE7 eggRes = eggFrame.egg;
            dFrameAdv.Text = "(+" + eggRes.FramesUsed.ToString() + ")";
            string ivstring = "";
            for (int i = 0; i < 6; ++i)
            {
                if (i > 0) { ivstring += ","; }
                ivstring += eggRes.IVs[i].ToString().PadLeft(2, ' ');
            }

            string ivinherstring = "";
            for (int i = 0; i < 6; ++i)
            {
                if (i > 0) { ivinherstring += ","; }
                if (eggRes.InheritMaleIV[i] == null)
                {
                    ivinherstring += eggRes.IVs[i].ToString().PadLeft(2, ' ');
                }
                else if (eggRes.InheritMaleIV[i] == true)
                {
                    ivinherstring += " M";
                }
                else if (eggRes.InheritMaleIV[i] == false)
                {
                    ivinherstring += " F";
                }
            }
            //dIvs.Text = ivstring;
            dHp.Text = eggFrame.GetHiddenPowerStr();
            SetGenderSingle();
            dAbility.Text = eggFrame.AbilityStr;
            dNature.Text = eggFrame.GetNatureStr();
            dBall.Text = eggFrame.Ball;
            if (currentSearchData.searchParameters.type == SearchType.MainEggRNG)
            {
                dPSV.Text = eggFrame.MainPSV.ToString("0000");
            }
            else
            {
                dPSV.Text = (!currentSearchData.parents.isMasuda && !currentSearchData.profile.shinyCharm ? "----" : eggFrame.PSV.ToString("0000"));
            }
            dSeed.Text = eggFrame.TinyState;

            // Set shiny status
            if (eggRes.Shiny)
            {
                dShinyStar.Visibility = ViewStates.Visible;
                if (eggRes.PSV == currentSearchData.profile.TSV)
                {
                    dPSV.SetBackgroundColor(ColorValues.ShinyColor[currentSearchData.preferences.shinyColor]);
                }
                else
                {
                    dPSV.SetBackgroundColor(ColorValues.ShinyColor[currentSearchData.preferences.otherTsvColor]);
                }
            }
            else
            {
                dShinyStar.Visibility = ViewStates.Invisible;
            }

            /*dSeed.Click += delegate {
                Toast.MakeText(this, "Click", ToastLength.Short).Show();
                if (dSeed.Visibility == ViewStates.Invisible)
                    dSeed.Visibility = ViewStates.Visible;
                else if (dSeed.Visibility == ViewStates.Visible)
                {
                    dSeed.Visibility = ViewStates.Invisible;
                }
            };*/

            // Color IVs
            ivSsb = new SpannableStringBuilder(ivstring);
            for (int i = 0; i < 6; ++i)
            {
                if (eggRes.InheritMaleIV[i] != null)
                {
                    if (eggRes.InheritMaleIV[i] == true)
                    {
                        ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.MaleGenderColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                    else // (er.InheritMaleIV[i] == false)
                    {
                        ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.FemaleGenderColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                }
                else
                {
                    if (eggRes.IVs[i] == 0)
                    {
                        ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.NoGoodIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                    else if (eggRes.IVs[i] >= 30)
                    {
                        ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.PerfectIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                }
            }

            ivInherSsb = new SpannableStringBuilder(ivinherstring);
            for (int i = 0; i < 6; ++i)
            {
                if (eggRes.InheritMaleIV[i] != null)
                {
                    if (eggRes.InheritMaleIV[i] == true)
                    {
                        ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.MaleGenderColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                    else // (er.InheritMaleIV[i] == false)
                    {
                        ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.FemaleGenderColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                }
                else
                {
                    if (eggRes.IVs[i] == 0)
                    {
                        ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.NoGoodIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                    else if (eggRes.IVs[i] >= 30)
                    {
                        ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.PerfectIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                    }
                }
            }

            string genderstring = GenderConversion.GetRandomGenderString(eggRes.genderRnum);

            genderSsb = new SpannableStringBuilder(genderstring);
            var genderVals = GenderConversion.GetRandomGenderBool(eggRes.genderRnum);
            for (int i = 0; i < genderVals.Count; ++i)
            {
                genderSsb.SetSpan(new ForegroundColorSpan(genderVals[i] ? ColorValues.MaleGenderColor : ColorValues.FemaleGenderColor), i, i + 1, SpanTypes.ExclusiveExclusive);
            }

            ivMode = false;
            dStats.Click += delegate
            {
                UpdateIVs();
            };

            extraMode = false;
            dExtras.Click += delegate {
                SetExtrasMode();
                //Toast.MakeText(this, "Hello", ToastLength.Short).Show();
            };

            metaMode = 0;
            dSeed.Click += delegate {
                UpdateMetaView();
            };

            dDone.Click += delegate {
                base.Dismiss();
            };

            dCopy.Click += delegate {
                string data = "";
                data += "Gender: " + dGender.Text + "\n";
                data += "IV: " + dIvs.Text + (ivMode ? "\n" : " [" + dHp.Text + "]\n");
                data += "Ability: " + dAbility.Text + "\n";
                data += "Nature: " + dNature.Text + "\n";
                data += "Ball: " + dBall.Text + "\n";
                data += "PSV: " + dPSV.Text + (eggRes.Shiny ? " *\n" : "\n");
                if (metaMode != 4) { data += dSeed.Text + "\n"; }


                var clipboard = (Android.Content.ClipboardManager)Context.GetSystemService(Context.ClipboardService);
                clipboard.PrimaryClip = ClipData.NewPlainText("", data);
                Toast.MakeText(Context, Context.Resources.GetString(Resource.String.search_detailed_spreadcopied), ToastLength.Short).Show();
            };

            dIvs.TextFormatted = ivSsb;
        }

        private void UpdateIVs() {
            ivMode = !ivMode;

            // Set inheritance mode
            if (ivMode)
            {
                dIvs.TextFormatted = ivInherSsb;
                dHp.Visibility = ViewStates.Invisible;
            }
            // Set real IV mode
            else
            {
                dIvs.TextFormatted = ivSsb;
                dHp.Text = eggFrame.GetHiddenPowerStr();
                dHp.Visibility = ViewStates.Visible;
            }
        }

        private void SetGenderSingle() {
            dGender.Text = GenderConversion.GetGenderString((GenderConversion.GenderType)currentSearchData.parents.genderCode);
            if (eggFrame.egg.Gender == 1)
            {
                dGender.SetTextColor(ColorValues.MaleGenderColor);
                dGender.Text += PokeRNGApp.Strings.genderSymbols[1];// "♂";
            }
            else if (eggFrame.egg.Gender == 2)
            {
                dGender.SetTextColor(ColorValues.FemaleGenderColor);
                dGender.Text += PokeRNGApp.Strings.genderSymbols[2];//"♀";
            }
            else
            {
                dGender.Text += PokeRNGApp.Strings.genderSymbols[0];//"-";
            }
        }

        private void SetExtrasMode() {
            extraMode = !extraMode;
            if (extraMode)
            {
                if (GenderConversion.IsRandomGender((GenderConversion.GenderType)currentSearchData.parents.genderCode) && !currentSearchData.parents.isNidoSpecies)
                {
                    dGender.TextFormatted = genderSsb;
                }
                else
                {
                    SetGenderSingle();
                }
                dAbility.Text = EggDataConversion.GetAbilityString(eggFrame.egg.abilityRnum);
            }
            else
            {
                SetGenderSingle();
                dAbility.Text = eggFrame.AbilityStr;
            }
        }

        private void UpdateMetaView() {
            metaMode = (metaMode + 1) % 5;
            if (metaMode == 3 && (currentSearchData.parents.genderCode != (int)GenderConversion.GenderType.SameRatio) && !currentSearchData.parents.isNidoSpecies)
            {
                metaMode++;
            }
            if (metaMode == 0)
            {
                dSeed.Text = eggFrame.TinyState;
            }
            else if (metaMode == 1)
            {
                if (currentSearchData.searchParameters.type == SearchType.MainEggRNG)
                {
                    dSeed.Text = "PID: " + eggFrame.MainPID.ToString("X");
                }
                else
                {
                    dSeed.Text = "PID: " + (!currentSearchData.parents.isMasuda && !currentSearchData.profile.shinyCharm ? "----" : eggFrame.PID.ToString("X"));
                }
            }
            else if (metaMode == 2)
            {
                dSeed.Text = "EC: " + eggFrame.EC.ToString("X");
            }
            else if (metaMode == 3)
            {
                dSeed.Text = Context.Resources.GetString(Resource.String.search_detailed_wurmple) + " " + eggFrame.WurmpleEvo;
            }
            else if (metaMode == 4)
            {
                dSeed.Text = "----";
            }
        }
    }
}