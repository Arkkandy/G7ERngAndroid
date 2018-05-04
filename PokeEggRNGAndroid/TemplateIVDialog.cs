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
    public class FilterIVDialog : Dialog
    {
        private FilterEditActivity.StatCombo[] filterStats;

        private List<FilterIVTemplate> tplList;

        private Spinner templates;
        private TextView tempIVmin;
        private TextView tempIVmax;

        private ImageButton deleteTemplateButton;

        private bool modified = false;

        public FilterIVDialog(Context context, FilterEditActivity.StatCombo[] filterData ) : base(context)
        {
            this.filterStats = filterData;
        }

        public void InitializeDialog()
        {
            SetTitle(Context.Resources.GetString(Resource.String.filter_templates_title));

            SetContentView(Resource.Layout.FilterTemplateDialog);

            templates = (Spinner)base.FindViewById(Resource.Id.filterTemplateSpin);

            tempIVmin = (TextView)base.FindViewById(Resource.Id.filterTemplateMin);
            tempIVmax = (TextView)base.FindViewById(Resource.Id.filterTemplateMax);

            Button saveFilter = (Button)base.FindViewById(Resource.Id.filterTemplateSave);
            Button applyFilter = (Button)base.FindViewById(Resource.Id.filterTemplateApply);

            deleteTemplateButton = (ImageButton)base.FindViewById(Resource.Id.filterTemplateDelete);

            tplList = FilterIVTemplate.LoadFilterTemplates(Context);
            UpdateSpinner();

            templates.ItemSelected += (sender, args) => {
                FilterIVTemplate item;
                if (tplList.Count > 0)
                {
                    item = tplList[args.Position];
                }
                else
                {
                    item = new FilterIVTemplate();
                    item.ivsmin.SetIVs(0, 0, 0, 0, 0, 0);
                    item.ivsmax.SetIVs(31, 31, 31, 31, 31, 31);
                }
                ShowMinStats(item.ivsmin);
                ShowMaxStats(item.ivsmax);
            };
            templates.SetSelection(0);

            deleteTemplateButton.Click += delegate {
                AlertDialog.Builder builder = new AlertDialog.Builder(Context);
                builder.SetTitle(Context.Resources.GetString(Resource.String.filter_templates_delete));
                //builder.SetMessage("If you delete your filter you will be unable to recover it. You will have to remake your filter. Are you sure you want to delete your profile?");
                builder.SetPositiveButton(Context.Resources.GetString(Resource.String.filter_templates_deleteyes),
                    (sender, args) =>
                    {
                        DeleteTemplate();
                    }
                    );
                builder.SetNegativeButton(Context.Resources.GetString(Resource.String.filter_templates_deleteno),
                    (sender, args) =>
                    {

                    }
                    );

                builder.Create().Show();
            };


            applyFilter.Click += delegate {
                if (tplList.Count == 0)
                {
                    SetMinStats(0, 0, 0, 0, 0, 0);
                    SetMaxStats(31, 31, 31, 31, 31, 31);
                }
                else
                {
                    int templateID = templates.SelectedItemPosition;
                    var tpl = tplList[templateID];
                    SetMinStats(tpl.ivsmin);
                    SetMaxStats(tpl.ivsmax);
                }
            };

            saveFilter.Click += delegate
            {
                EnterTextDialog etd = new EnterTextDialog(Context);

                etd.SetTitle(Context.Resources.GetString(Resource.String.filter_templates_newname));
                //etd.SetDefaultText("");
                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Context.Resources.GetString(Resource.String.filter_templates_emptyname));

                etd.InitializeDialog(
                    x =>
                    {
                        AddNewTemplate(x);
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
            };

            this.DismissEvent += delegate {
                if (modified)
                {
                    FilterIVTemplate.SaveFilterTemplates(Context, tplList);
                }
            };
        }

        private void ShowMinStats(IVSet ivs) {
            tempIVmin.Text = Context.Resources.GetString(Resource.String.search_min) +": " + ivs.hp.ToString().PadLeft(2, ' ') + ", " +
                ivs.atk.ToString().PadLeft(2, ' ') + ", " +
                ivs.def.ToString().PadLeft(2, ' ') + ", " +
                ivs.spa.ToString().PadLeft(2, ' ') + ", " +
                ivs.spd.ToString().PadLeft(2, ' ') + ", " +
                ivs.spe.ToString().PadLeft(2, ' ');
        }
        private void ShowMaxStats(IVSet ivs) {
            tempIVmax.Text = Context.Resources.GetString(Resource.String.search_max) +": " + ivs.hp.ToString().PadLeft(2, ' ') + ", " +
                ivs.atk.ToString().PadLeft(2, ' ') + ", " +
                ivs.def.ToString().PadLeft(2, ' ') + ", " +
                ivs.spa.ToString().PadLeft(2, ' ') + ", " +
                ivs.spd.ToString().PadLeft(2, ' ') + ", " +
                ivs.spe.ToString().PadLeft(2, ' ');
        }

        private void SetMinStats(IVSet ivs)
        {
            SetMinStats(ivs.hp, ivs.atk, ivs.def, ivs.spa, ivs.spd, ivs.spe);
        }
        private void SetMaxStats(IVSet ivs)
        {
            SetMaxStats(ivs.hp, ivs.atk, ivs.def, ivs.spa, ivs.spd, ivs.spe);
        }

        private void SetMinStats(int hp, int atk, int def, int spa, int spd, int spe)
        {
            filterStats[0].statMin.Progress = hp;
            filterStats[1].statMin.Progress = atk;
            filterStats[2].statMin.Progress = def;
            filterStats[3].statMin.Progress = spa;
            filterStats[4].statMin.Progress = spd;
            filterStats[5].statMin.Progress = spe;
        }

        private void SetMaxStats(int hp, int atk, int def, int spa, int spd, int spe)
        {
            filterStats[0].statMax.Progress = hp;
            filterStats[1].statMax.Progress = atk;
            filterStats[2].statMax.Progress = def;
            filterStats[3].statMax.Progress = spa;
            filterStats[4].statMax.Progress = spd;
            filterStats[5].statMax.Progress = spe;
        }

        private void AddNewTemplate(string name)
        {
            FilterIVTemplate tpl = new FilterIVTemplate();
            tpl.name = name;

            tpl.ivsmin.SetIVs(filterStats[0].statMin.Progress, filterStats[1].statMin.Progress, filterStats[2].statMin.Progress,
                filterStats[3].statMin.Progress, filterStats[4].statMin.Progress, filterStats[5].statMin.Progress);


            tpl.ivsmax.SetIVs(filterStats[0].statMax.Progress, filterStats[1].statMax.Progress, filterStats[2].statMax.Progress,
                filterStats[3].statMax.Progress, filterStats[4].statMax.Progress, filterStats[5].statMax.Progress);

            tplList.Add(tpl);
            UpdateSpinner();
            templates.SetSelection(tplList.Count - 1);

            modified = true;
        }

        private void DeleteTemplate()
        {
            if (tplList.Count > 0)
            {
                int index = templates.SelectedItemPosition;
                tplList.RemoveAt(index);

                UpdateSpinner();
                templates.SetSelection(Math.Min(index, (tplList.Count - 1)));

                modified = true;
            }
        }

        private void UpdateSpinner()
        {
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