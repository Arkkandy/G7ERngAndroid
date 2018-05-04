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
    [Activity(Label = "@string/activity_preferences",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class PreferencesActivity : Activity
    {
        Spinner rowHeightSpinner;
        Spinner maxResultSpinner;
        Spinner shinyRowSpinner;
        LinearLayout shinyPreview;
        Spinner otherTsvSpinner;
        LinearLayout otherPreview;
        CheckBox autoCheck;
        CheckBox genderCheck;
        CheckBox abilityCheck;
        ImageButton helpButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.PrefsLayout);

            // Initialize widgets
            rowHeightSpinner = (Spinner)FindViewById(Resource.Id.prefsHeight);
            rowHeightSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { Resources.GetString(Resource.String.setting_rowsize_small), Resources.GetString(Resource.String.setting_rowsize_medium), Resources.GetString(Resource.String.setting_rowsize_large) });
            
            maxResultSpinner = (Spinner)FindViewById(Resource.Id.prefMaxResult);
            maxResultSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, new string[] { "500", "1500", "5000", "10000" });

            shinyRowSpinner = (Spinner)FindViewById(Resource.Id.prefsShiny);
            shinyRowSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { Resources.GetString(Resource.String.setting_shinycolors_blue), Resources.GetString(Resource.String.setting_shinycolors_purple),
                    Resources.GetString(Resource.String.setting_shinycolors_yellow), Resources.GetString(Resource.String.setting_shinycolors_green),
                    Resources.GetString(Resource.String.setting_shinycolors_orange), Resources.GetString(Resource.String.setting_shinycolors_gray) });
            shinyPreview = (LinearLayout)FindViewById(Resource.Id.prefsShinyPreview);

            otherTsvSpinner = (Spinner)FindViewById(Resource.Id.prefsOtherTSV);
            otherTsvSpinner.Adapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line,
                new string[] { Resources.GetString(Resource.String.setting_shinycolors_blue), Resources.GetString(Resource.String.setting_shinycolors_purple),
                    Resources.GetString(Resource.String.setting_shinycolors_yellow), Resources.GetString(Resource.String.setting_shinycolors_green),
                    Resources.GetString(Resource.String.setting_shinycolors_orange), Resources.GetString(Resource.String.setting_shinycolors_gray) });
            otherPreview = (LinearLayout)FindViewById(Resource.Id.prefsOtherPreview);

            helpButton = (ImageButton)FindViewById(Resource.Id.prefsHelp);

            autoCheck = (CheckBox)FindViewById(Resource.Id.prefsAuto);
            genderCheck = (CheckBox)FindViewById(Resource.Id.prefsGender);
            abilityCheck = (CheckBox)FindViewById(Resource.Id.prefsAbility);


            shinyRowSpinner.ItemSelected += (sender, args) =>
            {
                shinyPreview.SetBackgroundColor(EggRM.ColorValues.ShinyColor[args.Position]);
            };
            otherTsvSpinner.ItemSelected += (sender, args) =>
            {
                otherPreview.SetBackgroundColor(EggRM.ColorValues.ShinyColor[args.Position]);
            };


            helpButton.Click += delegate {
                AlertDialog.Builder alert = new AlertDialog.Builder(this);

                alert.SetTitle(Resources.GetString(Resource.String.setting_helptitle));
                alert.SetMessage(Resources.GetString(Resource.String.setting_helpmessage));

                alert.SetPositiveButton(Resources.GetString(Resource.String.okthanks), delegate { });
                alert.Show();
            };

            // Initialize functions
            LoadPrefs();
        }

        protected override void OnPause()
        {
            base.OnPause();
            SavePrefs();
        }

        private void LoadPrefs() {
            var prefs = EggRM.AppPreferences.LoadPreferencesData(this);

            rowHeightSpinner.SetSelection(prefs.rowHeight);
            maxResultSpinner.SetSelection(prefs.maxResults);

            shinyRowSpinner.SetSelection(prefs.shinyColor);
            otherTsvSpinner.SetSelection(prefs.otherTsvColor);

            autoCheck.Checked = prefs.autoSearch;
            genderCheck.Checked = prefs.allRandomGender;
            abilityCheck.Checked = prefs.allAbility;
        }

        private void SavePrefs() {
            EggRM.AppPreferences prefs = new EggRM.AppPreferences();
            prefs.rowHeight = rowHeightSpinner.SelectedItemPosition;
            prefs.maxResults = maxResultSpinner.SelectedItemPosition;

            prefs.shinyColor = shinyRowSpinner.SelectedItemPosition;
            prefs.otherTsvColor = otherTsvSpinner.SelectedItemPosition;

            prefs.autoSearch = autoCheck.Checked;
            prefs.allRandomGender = genderCheck.Checked;
            prefs.allAbility = abilityCheck.Checked;

            EggRM.AppPreferences.SavePreferencesData(this, prefs);
        }
    }
}