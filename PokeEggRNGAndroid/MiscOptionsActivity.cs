using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Gen7EggRNG.EggRM;
using Newtonsoft.Json;

namespace Gen7EggRNG
{
    public class TSVThread {
        public int tsv;
        public double postedTime;
        public string url;
    }

    [Activity(Label = "@string/activity_misc",
        ConfigurationChanges = Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.Orientation,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
    public class MiscOptionsActivity : Activity
    {
        Button tsvAdd;
        ListView tsvView;

        Spinner tsvSetSpinner;
        ImageButton tsvSetAdd;
        ImageButton tsvSetDelete;
        ImageButton tsvSetEdit;

        List<MiscTSVData> tsvData = new List<MiscTSVData>();
        List<TSVThread> tsvSVEData = new List<TSVThread>();

        Button sveFetchButton;
        ListView sveList;
        Button sveAddButton;

        bool modified = false;

        int currentSelection = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.MiscOptionsLayout);

            tsvAdd = (Button)FindViewById(Resource.Id.miscAddTSV);
            tsvView = (ListView)FindViewById(Resource.Id.miscTSVs);

            tsvSetSpinner = (Spinner)FindViewById(Resource.Id.otherTSVSpinner);
            tsvSetAdd = (ImageButton)FindViewById(Resource.Id.otherTSVAdd);
            tsvSetDelete = (ImageButton)FindViewById(Resource.Id.otherTSVDelete);
            tsvSetEdit = (ImageButton)FindViewById(Resource.Id.otherTSVEdit);

            sveFetchButton = (Button)FindViewById(Resource.Id.miscGetSVE);
            sveList = (ListView)FindViewById(Resource.Id.miscListSVE);
            sveAddButton = (Button)FindViewById(Resource.Id.miscAddSVE);

            tsvAdd.Click += delegate {
                Dialog dialog = new Dialog(this);

                dialog.SetContentView(Resource.Layout.TSV_Dialog);

                dialog.SetTitle(Resources.GetString(Resource.String.misc_add));
                EditText dialogTSVEdit = (EditText)dialog.FindViewById(Resource.Id.tsvDialogTSV);
                dialogTSVEdit.AddTextChangedListener(new Gen7EggRNG.Util.TSVTextWatcher(dialogTSVEdit));

                Button dialogAdd = (Button)dialog.FindViewById(Resource.Id.tsvDialogAdd);
                dialogAdd.Click += delegate
                {
                    AddTSV(int.Parse(dialogTSVEdit.Text));
                    UpdateListTSVs();
                    //dialog.Dismiss();
                };

                Button dialogCancel = (Button)dialog.FindViewById(Resource.Id.tsvDialogCancel);
                dialogCancel.Click += delegate
                {
                    dialog.Dismiss();
                };

                dialog.Show();
            };

            tsvView.ItemClick += (sender, args) => {
                PopupMenu menu = new PopupMenu(this, tsvView.GetChildAt(args.Position), Android.Views.GravityFlags.Center);
                menu.Menu.Add(Menu.None, 1, 1, String.Format(Resources.GetString(Resource.String.misc_remove_tsv), tsvView.Adapter.GetItem(args.Position)));
                menu.Menu.Add(Menu.None, 2, 2, Resources.GetString(Resource.String.misc_remove_all));

                menu.MenuItemClick += (msender, margs) =>
                {
                    if (margs.Item.ItemId == 1)
                    {
                        modified = true;
                        int value = int.Parse((string)tsvView.Adapter.GetItem(args.Position));
                        tsvData[currentSelection].tsvs.Remove(value);
                        UpdateListTSVs();
                        return;
                    }
                    else if (margs.Item.ItemId == 2)
                    {
                        modified = true;
                        tsvData[currentSelection].tsvs.Clear();
                        UpdateListTSVs();
                        return;
                    }
                };

                menu.Show();
            };

            tsvSetSpinner.ItemSelected += (sender,args) => {
                modified = true;
                currentSelection = args.Position;
                UpdateListTSVs();
            };

            tsvSetAdd.Click += delegate {
                EnterTextDialog etd = new EnterTextDialog(this);
                
                etd.SetTitle(Resources.GetString(Resource.String.misc_newtsvset));
                etd.SetDefaultText(Resources.GetString(Resource.String.misc_defaultsetname));

                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Resources.GetString(Resource.String.misc_emptysetname));

                etd.InitializeDialog(
                    x =>
                    {
                        AddNewTSVSet(x);
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
                //MiscUtility.
            };

            tsvSetDelete.Click += delegate {
                if (tsvData.Count > 1) {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this);
                    builder.SetTitle(Resources.GetString(Resource.String.misc_deleteset));
                    builder.SetMessage(Resources.GetString(Resource.String.misc_deletesetdescription));
                    builder.SetPositiveButton(Resources.GetString(Resource.String.misc_deleteyes),
                        (sender, args) =>
                        {
                            //DeleteTemplate();
                            tsvData.RemoveAt(currentSelection);

                            UpdateSpinner();
                            tsvSetSpinner.SetSelection(Math.Min(currentSelection, tsvData.Count - 1));

                            modified = true;
                        }
                        );
                    builder.SetNegativeButton(Resources.GetString(Resource.String.misc_deleteno),
                        (sender, args) =>
                        {

                        }
                        );

                    builder.Create().Show();
                }
            };

            tsvSetEdit.Click += delegate {
                EnterTextDialog etd = new EnterTextDialog(this);

                etd.SetTitle(Resources.GetString(Resource.String.misc_editsetname));
                etd.SetDefaultText(tsvData[currentSelection].name);
                etd.SetButtonText(PokeRNGApp.Strings.option_ok, PokeRNGApp.Strings.option_cancel);
                etd.SetEmptyFieldMessage(Resources.GetString(Resource.String.misc_emptysetname));

                etd.InitializeDialog(
                    x =>
                    {
                        tsvData[currentSelection].name = x;
                        UpdateSpinner();
                        tsvSetSpinner.SetSelection(currentSelection);
                        modified = true;
                    },
                    delegate
                    {

                    }
                );

                etd.Show();
            };

            sveFetchButton.Click += delegate {
                sveList.Adapter = null;

                var sveTSVs = FetchTSVsFromSVE();

                tsvSVEData = sveTSVs;
                var strs = tsvSVEData.ConvertAll<string>(tsv => tsv.tsv + " " + (int)((DateTime.UtcNow - UnixTimeStampToDateTime(tsv.postedTime)).TotalHours) + " hours ago ");

                sveList.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, strs);
            };

            sveAddButton.Click += delegate {
                if (tsvSVEData.Count > 0) {
                    AddTSVList(tsvSVEData.ConvertAll<int>(tsv => tsv.tsv));
                    UpdateListTSVs();
                    Toast.MakeText(this, "Added new TSVs from SVE.", ToastLength.Short).Show();
                    //Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.misc_tsvadded), tsv.ToString().PadLeft(4, '0')), ToastLength.Short).Show();
                }
            };

            LoadTSVs();
            //UpdateListTSVs();
        }

        protected override void OnPause()
        {
            base.OnPause();
            if (modified)
            {
                SaveTSVs();
            }
        }

        private void AddNewTSVSet(string name) {
            MiscTSVData newData = new MiscTSVData();
            newData.name = name;
            newData.tsvs = new List<int>();

            tsvData.Add(newData);

            UpdateSpinner();
            tsvSetSpinner.SetSelection(tsvData.Count-1);

            modified = true;
        }


        private void UpdateSpinner() {
            List<string> names = tsvData.ConvertAll(x => x.name);
            tsvSetSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleDropDownItem1Line, names);

            tsvSetDelete.Enabled = names.Count > 1;
        }

        private void AddTSV(int tsv) {
            if (tsvData[currentSelection].tsvs.Contains(tsv))
            {
                Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.misc_tsvalreadyinlist), tsv.ToString().PadLeft(4, '0')), ToastLength.Short).Show();
                //Toast.MakeText(this, "TSV " + tsv.ToString().PadLeft(4, '0') + " is already in the list.", ToastLength.Short).Show();
            }
            else {
                tsvData[currentSelection].tsvs.Add(tsv);
                Toast.MakeText(this, String.Format(Resources.GetString(Resource.String.misc_tsvadded), tsv.ToString().PadLeft(4, '0')), ToastLength.Short).Show();
                //Toast.MakeText(this, "TSV " + tsv.ToString().PadLeft(4, '0') + " added.", ToastLength.Short).Show();
                modified = true;
            }
            /*if (tsvs.Add(tsv))
            {
                Toast.MakeText(this, "TSV " + tsv.ToString().PadLeft(4, '0') + " added.", ToastLength.Short).Show();
            }
            else {
                Toast.MakeText(this, "TSV " + tsv.ToString().PadLeft(4,'0') + " is already in the list.", ToastLength.Short).Show();
            }*/
        }

        private void AddTSVList(List<int> tsvs) {
            for (int i = 0; i < tsvs.Count; ++i)
            {
                if (!tsvData[currentSelection].tsvs.Contains(tsvs[i]))
                {
                    tsvData[currentSelection].tsvs.Add(tsvs[i]);
                    modified = true;
                }
            }
        }

        private void UpdateListTSVs() {
            List<string> tsvStrings = tsvData[currentSelection].tsvs.ConvertAll(x => x.ToString().PadLeft(4,'0'));
            tsvView.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, tsvStrings);
        }

        private void LoadTSVs() {
            //return EggRM.MiscUtility.LoadTSVs(this);
            tsvData = MiscUtility.LoadAllTSVs(this);
            UpdateSpinner();
            tsvSetSpinner.SetSelection( MiscUtility.GetSelectedTSVSet(this) );
        }

        private void SaveTSVs() {
            MiscUtility.SaveTSVs(this, tsvData, currentSelection);

            //Toast.MakeText(this, "Saved TSV: " + tsvString, ToastLength.Long).Show();
        }

        private List<TSVThread> FetchTSVsFromSVE()
        {
            List<TSVThread> list = new List<TSVThread>();

            try
            {
                var cli = new WebClient();
                string sveNewContent = cli.DownloadString("https://www.reddit.com/r/SVExchange/new.json?sort=new&limit=50");

                var xmlDoc = JsonConvert.DeserializeXmlNode(sveNewContent, "data");

                var root = xmlDoc.DocumentElement;
                var postGroup = root.SelectNodes("data/children");

                for (int i = 0; i < postGroup.Count; ++i)
                {
                    var childData = postGroup[i].SelectSingleNode("data");
                    if (childData != null)
                    {
                        var titleNode = childData.SelectSingleNode("title");
                        var timeNode = childData.SelectSingleNode("created_utc");
                        var urlNode = childData.SelectSingleNode("url");

                        if (!IsStringTSV(titleNode.InnerText))
                        {
                            continue;
                        }

                        TSVThread newTSV = new TSVThread();
                        newTSV.tsv = int.Parse(titleNode.InnerText);
                        newTSV.postedTime = double.Parse(timeNode.InnerText);
                        newTSV.url = urlNode.InnerText;

                        list.Add(newTSV);
                    }
                }
            }
            catch ( Exception e) {
                //e.Message;
            }

            return list;
        }

        private bool IsStringTSV(string s)
        {
            if (s.Length != 4)
            {
                return false;
            }
            int tsv = -1;
            if (int.TryParse(s, out tsv) && (0 <= tsv && tsv <= 4095))
            {
                return true;
            }

            return false;
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}