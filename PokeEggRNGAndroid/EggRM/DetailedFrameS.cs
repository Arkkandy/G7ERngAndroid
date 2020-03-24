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

namespace Gen7EggRNG.EggRM
{
    public class DetailedFrameS : Dialog
    {
        private G7SFrame pokeFrame;
        private FullSearchData currentSearchData;

        ImageView dShinyStar;
        TextView dLevel;
        TextView dIvs;
        TextView dHp;
        TextView dGender;
        TextView dAbility;
        TextView dNature;
        //TextView dSync;
        TextView dDelay;
        TextView dMark;
        TextView dPSV;
        TextView dSeed;
        LinearLayout dStats;
        LinearLayout dExtras;
        Button dDone;
        Button dCopy;

        SpannableStringBuilder ivSsb;
        SpannableStringBuilder ivInherSsb;

        bool statMode = false;
        int metaMode = 0;
        bool extraMode = false;

        //string ivstring;
        //string ivinherstring;

        public DetailedFrameS(G7SFrame frame, FullSearchData currentData, Context context) : base(context)
        {
            pokeFrame = frame;
            currentSearchData = currentData;
        }

        public void Initialize()
        {
            base.SetContentView(Resource.Layout.FrameDetailedSDialog);

            string frameString = String.Format(Context.Resources.GetString(Resource.String.search_detailed_frame), pokeFrame.FrameNum);
            base.SetTitle(frameString);

            dShinyStar = (ImageView)base.FindViewById(Resource.Id.detailsIsShiny);
            dLevel = (TextView)base.FindViewById(Resource.Id.detailsLevel);
            dIvs = (TextView)base.FindViewById(Resource.Id.detailsIV);
            dHp = (TextView)base.FindViewById(Resource.Id.detailsHP);
            dGender = (TextView)base.FindViewById(Resource.Id.detailsGender);
            dAbility = (TextView)base.FindViewById(Resource.Id.detailsAbility);
            dNature = (TextView)base.FindViewById(Resource.Id.detailsNature);
            dDelay = (TextView)base.FindViewById(Resource.Id.detailsDelay);
            dMark = (TextView)base.FindViewById(Resource.Id.detailsMark);
            dPSV = (TextView)base.FindViewById(Resource.Id.detailsPSV);
            dSeed = (TextView)base.FindViewById(Resource.Id.detailsSeed);
            dStats = (LinearLayout)base.FindViewById(Resource.Id.detailsStatsGroup);
            dExtras = (LinearLayout)base.FindViewById(Resource.Id.detailsExtras);
            dDone = (Button)base.FindViewById(Resource.Id.detailsDone);
            dCopy = (Button)base.FindViewById(Resource.Id.detailsCopySpread);

            Result7 pokeRes = pokeFrame.pokemon;
            dLevel.Text = "Lv. " + pokeRes.Level;
            string ivstring = "";
            for (int i = 0; i < 6; ++i)
            {
                if (i > 0) { ivstring += ","; }
                ivstring += pokeRes.IVs[i].ToString().PadLeft(2, ' ');
            }

            string statstring = "";
            int[] pokeStats = pokeFrame.GetStats(currentSearchData.stationary.baseStats);
            for (int i = 0; i < 6; ++i)
            {
                if (i > 0) { statstring += ","; }
                statstring += pokeStats[i].ToString().PadLeft(3, ' ');
            }
            //dIvs.Text = ivstring;
            dHp.Text = pokeFrame.GetHiddenPowerStr();
            SetGenderSingle();
            dAbility.Text = pokeFrame.AbilityStr;
            dNature.Text = pokeFrame.GetNatureStr();
            dDelay.Text = pokeFrame.FrameDelayUsed.ToString();
            dMark.Text = pokeFrame.Mark;
            dPSV.Text = pokeRes.PSV.ToString("0000");
                dPSV.Text += " (" + pokeRes.PRV + ")";
            dSeed.Text = "RandNum: " + pokeFrame.pokemon.RandNum.ToString("X");

            if (pokeFrame.pokemon.Synchronize) {
                dNature.SetTypeface(dNature.Typeface, Android.Graphics.TypefaceStyle.BoldItalic);
            }

            // Set shiny status
            if (pokeRes.Shiny)
            {
                dShinyStar.Visibility = ViewStates.Visible;
                if (pokeRes.PRV == currentSearchData.profile.TRV) {
                    dPSV.SetBackgroundColor(ColorValues.ShinyColor[currentSearchData.preferences.squareShinyColor]);
                }
                else
                {
                    dPSV.SetBackgroundColor(ColorValues.ShinyColor[currentSearchData.preferences.shinyColor]);
                }
            }
            else
            {
                dShinyStar.Visibility = ViewStates.Invisible;
            }

            // Color IVs
            ivSsb = new SpannableStringBuilder(ivstring);
            for (int i = 0; i < 6; ++i)
            {
                if (pokeRes.IVs[i] == 0)
                {
                    ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.NoGoodIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                }
                else if (pokeRes.IVs[i] >= 30)
                {
                    ivSsb.SetSpan(new ForegroundColorSpan(ColorValues.PerfectIVColor), 3 * i, 3 * i + 2, SpanTypes.ExclusiveExclusive);
                }
            }

            ivInherSsb = new SpannableStringBuilder(statstring);
            for (int i = 0; i < 6; ++i)
            {
                if (pokeRes.IVs[i] == 0)
                {
                    ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.NoGoodIVColor), 4 * i, 4 * i + 3, SpanTypes.ExclusiveExclusive);
                }
                else if (pokeRes.IVs[i] >= 30)
                {
                    ivInherSsb.SetSpan(new ForegroundColorSpan(ColorValues.PerfectIVColor), 4 * i, 4 * i + 3, SpanTypes.ExclusiveExclusive);
                }
            }

            statMode = false;
            dStats.Click += delegate
            {
                UpdateIVs();
            };

            extraMode = false;
            dExtras.Click += delegate {
                SetExtrasMode();
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
                data += "IV: " + dIvs.Text + (statMode ? "\n" : " [" + dHp.Text + "]\n");
                data += "Ability: " + dAbility.Text + "\n";
                data += "Nature: " + dNature.Text + "\n";
                //data += "Ball: " + dBall.Text + "\n";
                data += "PSV: " + pokeRes.PSV + (pokeRes.Shiny ? " *\n" : "\n");
                if (metaMode != 3) { data += dSeed.Text + "\n"; }


                var clipboard = (Android.Content.ClipboardManager)Context.GetSystemService(Context.ClipboardService);
                clipboard.PrimaryClip = ClipData.NewPlainText("", data);
                Toast.MakeText(Context, Context.Resources.GetString(Resource.String.search_detailed_spreadcopied), ToastLength.Short).Show();
            };

            dIvs.TextFormatted = ivSsb;
        }

        private void UpdateIVs()
        {
            statMode = !statMode;

            // Show stats
            if (statMode)
            {
                dIvs.TextFormatted = ivInherSsb;
            }
            // Show IVs
            else
            {
                dIvs.TextFormatted = ivSsb;
            }
        }

        private void SetGenderSingle()
        {
            dGender.Text = "";// GenderConversion.GetGenderString((GenderConversion.GenderType)currentSearchData.parents.genderCode);
            if (pokeFrame.pokemon.Gender == 1)
            {
                dGender.SetTextColor(ColorValues.MaleGenderColor);
                dGender.Text += PokeRNGApp.Strings.genderSymbols[1];// "♂";
            }
            else if (pokeFrame.pokemon.Gender == 2)
            {
                dGender.SetTextColor(ColorValues.FemaleGenderColor);
                dGender.Text += PokeRNGApp.Strings.genderSymbols[2];//"♀";
            }
            else
            {
                dGender.Text += PokeRNGApp.Strings.genderSymbols[0];//"-";
            }
        }

        private void SetExtrasMode()
        {
            extraMode = !extraMode;
            if (extraMode)
            {
                dNature.Text = pokeFrame.GetNatureStr();
                // See base nature
                //PokeRNGApp.Strings.natures[pokeFrame.pokemon.]
            }
            else
            {
                dNature.Text = pokeFrame.GetNatureStr();
            }
        }

        private void UpdateMetaView()
        {
            metaMode = (metaMode + 1) % 4;
            if (metaMode == 3 && (currentSearchData.parents.genderCode != (int)GenderConversion.GenderType.SameRatio) && !currentSearchData.parents.isNidoSpecies)
            {
                metaMode++;
            }
            if (metaMode == 0)
            {
                dSeed.Text = "RandNum: " + pokeFrame.pokemon.RandNum.ToString("X");
            }
            else if (metaMode == 1)
            {
                dSeed.Text = "PID: " + pokeFrame.pokemon.PID.ToString("X");
            }
            else if (metaMode == 2)
            {
                dSeed.Text = "EC: " + pokeFrame.pokemon.EC.ToString("X");
            }
            else if (metaMode == 3)
            {
                dSeed.Text = "----";
            }
        }
    }
}