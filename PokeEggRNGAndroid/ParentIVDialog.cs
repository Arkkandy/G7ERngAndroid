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
using Gen7EggRNG.EggRM;

namespace Gen7EggRNG
{
    public class ParentIVDialog : Dialog
    {
        private ParentEditActivity.StatCombo[] maleStats;
        private ParentEditActivity.StatCombo[] femaleStats;

        private List<ParentIVTemplate> tplList;

        private Spinner templates;
        private TextView tempIVs;

        private ImageButton deleteTemplateButton;

        private bool modified = false;

        public ParentIVDialog(Context context, ParentEditActivity.StatCombo[] maleData, ParentEditActivity.StatCombo[] femaleData) : base(context) {
            this.maleStats = maleData;
            this.femaleStats = femaleData;
        }

        public void InitializeDialog() {
            SetTitle(Context.Resources.GetString(Resource.String.parents_templates_title));

            SetContentView(Resource.Layout.ParentTemplateDialog);

            templates = (Spinner)base.FindViewById(Resource.Id.parentTemplateSpin);

            tempIVs = (TextView)base.FindViewById(Resource.Id.parentTemplateIVs);

            Button fromMale = (Button)base.FindViewById(Resource.Id.parentTemplateFromMale);
            Button fromFemale = (Button)base.FindViewById(Resource.Id.parentTemplateFromFemale);
            Button toMale = (Button)base.FindViewById(Resource.Id.parentTemplateToMale);
            Button toFemale = (Button)base.FindViewById(Resource.Id.parentTemplateToFemale);

            deleteTemplateButton = (ImageButton)base.FindViewById(Resource.Id.parentTemplateDelete);

            tplList = ParentIVTemplate.LoadParentTemplates(Context);
            UpdateSpinner();

            templates.ItemSelected += (sender, args) => {
                ParentIVTemplate item;
                if (tplList.Count > 0)
                {
                    item = tplList[args.Position];
                }
                else
                {
                    item = new ParentIVTemplate();
                    item.ivs.hp = item.ivs.atk = item.ivs.def = item.ivs.spa = item.ivs.spd = item.ivs.spe = 31;
                }
                tempIVs.Text = item.ivs.hp.ToString().PadLeft(2, ' ') + ", " +
                    item.ivs.atk.ToString().PadLeft(2, ' ') + ", " +
                    item.ivs.def.ToString().PadLeft(2, ' ') + ", " +
                    item.ivs.spa.ToString().PadLeft(2, ' ') + ", " +
                    item.ivs.spd.ToString().PadLeft(2, ' ') + ", " +
                    item.ivs.spe.ToString().PadLeft(2, ' ');
            };
            templates.SetSelection(0);
            
            deleteTemplateButton.Click += delegate {
                AlertDialog.Builder builder = new AlertDialog.Builder(Context);
                builder.SetTitle(Context.Resources.GetString(Resource.String.parents_templates_delete));
                //builder.SetMessage("If you delete your filter you will be unable to recover it. You will have to remake your filter. Are you sure you want to delete your profile?");
                builder.SetPositiveButton(Context.Resources.GetString(Resource.String.parents_templates_deleteyes),
                    (sender, args) =>
                    {
                        DeleteTemplate();
                    }
                    );
                builder.SetNegativeButton(Context.Resources.GetString(Resource.String.parents_templates_deleteno),
                    (sender, args) =>
                    {

                    }
                    );

                builder.Create().Show();
            };


            toMale.Click += delegate {
                if (tplList.Count == 0)
                {
                    SetMaleStats(31, 31, 31, 31, 31, 31);
                }
                else
                {
                    int templateID = templates.SelectedItemPosition;
                    var tpl = tplList[templateID];
                    SetMaleStats(tpl.ivs);
                }
            };

            toFemale.Click += delegate {
                if (tplList.Count == 0)
                {
                    SetFemaleStats(31, 31, 31, 31, 31, 31);
                }
                else
                {
                    int templateID = templates.SelectedItemPosition;
                    var tpl = tplList[templateID];
                    SetFemaleStats(tpl.ivs);
                }
            };

            fromMale.Click += delegate
            {
                EnterTextDialog etd = new EnterTextDialog(Context);

                etd.SetTitle(Context.Resources.GetString(Resource.String.parents_templates_newname));
                //etd.SetDefaultText("");
                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Context.Resources.GetString(Resource.String.parents_templates_emptyname));

                etd.InitializeDialog(
                    x =>
                    {
                        AddNewTemplate(0, x);
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
            };

            fromFemale.Click += delegate
            {
                EnterTextDialog etd = new EnterTextDialog(Context);

                etd.SetTitle(Context.Resources.GetString(Resource.String.parents_templates_newname));
                //etd.SetDefaultText("");
                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Context.Resources.GetString(Resource.String.parents_templates_emptyname));

                etd.InitializeDialog(
                    x =>
                    {
                        AddNewTemplate(1, x);
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
            };

            this.DismissEvent += delegate {
                if (modified) {
                    ParentIVTemplate.SaveParentTemplates(Context, tplList);
                }
            };
        }

        private void SetMaleStats(IVSet ivs) {
            SetMaleStats(ivs.hp, ivs.atk, ivs.def, ivs.spa, ivs.spd, ivs.spe);
        }
        private void SetFemaleStats(IVSet ivs)
        {
            SetFemaleStats(ivs.hp, ivs.atk, ivs.def, ivs.spa, ivs.spd, ivs.spe);
        }

        private void SetMaleStats(int hp, int atk, int def, int spa, int spd, int spe) {
            maleStats[0].statBar.Progress = hp;
            maleStats[1].statBar.Progress = atk;
            maleStats[2].statBar.Progress = def;
            maleStats[3].statBar.Progress = spa;
            maleStats[4].statBar.Progress = spd;
            maleStats[5].statBar.Progress = spe;
        }

        private void SetFemaleStats(int hp, int atk, int def, int spa, int spd, int spe)
        {
            femaleStats[0].statBar.Progress = hp;
            femaleStats[1].statBar.Progress = atk;
            femaleStats[2].statBar.Progress = def;
            femaleStats[3].statBar.Progress = spa;
            femaleStats[4].statBar.Progress = spd;
            femaleStats[5].statBar.Progress = spe;
        }

        private void AddNewTemplate(int parentID, string name) {

            ParentIVTemplate tpl = new ParentIVTemplate();
            tpl.name = name;

            if (parentID == 0)
            { // Male
                tpl.ivs.SetIVs(maleStats[0].statBar.Progress, maleStats[1].statBar.Progress, maleStats[2].statBar.Progress,
                    maleStats[3].statBar.Progress, maleStats[4].statBar.Progress, maleStats[5].statBar.Progress);
            }
            else if (parentID == 1)
            { // Female
                tpl.ivs.SetIVs(femaleStats[0].statBar.Progress, femaleStats[1].statBar.Progress, femaleStats[2].statBar.Progress,
                    femaleStats[3].statBar.Progress, femaleStats[4].statBar.Progress, femaleStats[5].statBar.Progress);
            }

            tplList.Add(tpl);
            UpdateSpinner();
            templates.SetSelection(tplList.Count - 1);

            modified = true;
        }

        private void DeleteTemplate() {
            if (tplList.Count > 0)
            {
                int index = templates.SelectedItemPosition;
                tplList.RemoveAt(index);

                UpdateSpinner();
                templates.SetSelection(Math.Min(index, (tplList.Count - 1)));

                modified = true;
            }
        }

        private void UpdateSpinner() {
            if (tplList.Count > 0)
            {
                List<string> names = tplList.ConvertAll(x => x.name);
                templates.Adapter = new ArrayAdapter<String>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, names);
                deleteTemplateButton.Enabled = true;
            }
            else
            {
                List<string> names = new List<string>(); names.Add(PokeRNGApp.Strings.option_none);
                templates.Adapter = new ArrayAdapter<String>(Context, Android.Resource.Layout.SimpleDropDownItem1Line, names);
                deleteTemplateButton.Enabled = false;
            }
        }
    }
}