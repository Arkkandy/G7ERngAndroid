using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Gen7EggRNG.EggRM
{
    public class MiscTSVData {
        public string name;
        public List<int> tsvs;
    }

    public static class MiscUtility
    {
        public static int GetNumTSVSets(Context context) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            return prefs.GetInt("OtherTSVsNum", 0);
        }

        public static void SelectTSVSet(Context context, int id) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var prefsEditor = prefs.Edit();
            prefsEditor.PutInt("OtherTSVsSelected", id);
        }
        public static int GetSelectedTSVSet(Context context) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            return prefs.GetInt("OtherTSVsSelected", 0);
        }

        public static List<int> LoadTSVs(Context context)
        {
            List<int> tsvList = new List<int>();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            string tsvString = prefs.GetString("OtherTSVs", String.Empty);

            if (tsvString != String.Empty)
            {
                string[] tsvSplit = tsvString.Split(',');
                foreach (string item in tsvSplit)
                {
                    tsvList.Add(int.Parse(item));
                }
            }

            //Toast.MakeText(context, "Loaded TSV: " + tsvString, ToastLength.Long).Show();

            return tsvList;
        }

        public static List<MiscTSVData> LoadAllTSVs(Context context) {
            List<MiscTSVData> tsvData = new List<MiscTSVData>();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numEntries = prefs.GetInt("OtherTSVsNum", 0);

            if (numEntries > 0)
            {
                for (int i = 0; i < numEntries; ++i)
                {
                    MiscTSVData newData = new MiscTSVData();

                    newData.name = prefs.GetString("OtherTSVs" + i + "Name", "OtherTSVs");
                    newData.tsvs = new List<int>();
                    string tsvString = prefs.GetString("OtherTSVs" + i + "TSV", String.Empty);
                    if (tsvString.Length > 0)
                    {
                        string[] tsvSplit = tsvString.Split(',');
                        foreach (string item in tsvSplit)
                        {
                            newData.tsvs.Add(int.Parse(item));
                        }
                    }

                    tsvData.Add(newData);
                }
            }
            else {
                MiscTSVData newData = new MiscTSVData();

                newData.name = "Default Set";
                newData.tsvs = new List<int>();

                tsvData.Add(newData);
            }

            return tsvData;
        }

        public static List<int> LoadCurrentTSVs(Context context)
        {
            List<int> tsvList = new List<int>();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            int selection = prefs.GetInt("OtherTSVsSelected", 0);

            //string tsvName = prefs.GetString("OtherTSVs" + selection + "Name", "OtherTSV");
            string tsvDataString = prefs.GetString("OtherTSVs"+selection+"TSV", String.Empty);

            if (tsvDataString != String.Empty)
            {
                string[] tsvSplit = tsvDataString.Split(',');
                foreach (string item in tsvSplit)
                {
                    tsvList.Add(int.Parse(item));
                }
            }

            //Toast.MakeText(context, "Loaded TSV: " + tsvString, ToastLength.Long).Show();

            return tsvList;
        }

        public static void SaveTSVs(Context context, int id, string name, List<int> tsvs) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var prefEditor = prefs.Edit();

            string tsvString = String.Join(",", tsvs);
            prefEditor.PutString("OtherTSVs" + id + "Name", name);
            prefEditor.PutString("OtherTSVs" + id +"TSV", tsvString);

            prefEditor.Commit();
        }

        public static void SaveTSVs(Context context, List<MiscTSVData> data, int selection) {
            DeleteTSVs(context);
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var prefEditor = prefs.Edit();

            prefEditor.PutInt("OtherTSVsNum", data.Count);
            prefEditor.PutInt("OtherTSVsSelected", selection);
            
            for (int i = 0; i < data.Count; ++i)
            {
                string tsvString = String.Join(",", data[i].tsvs);
                prefEditor.PutString("OtherTSVs" + i + "Name", data[i].name);
                prefEditor.PutString("OtherTSVs" + i + "TSV", tsvString);
            }

            prefEditor.Commit();
        }

        public static int CreateTSVEntry(Context context, string name) {
            int numSets = GetNumTSVSets(context);
            int newID = numSets + 1;

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            var prefEditor = prefs.Edit();

            prefEditor.PutString("OtherTSVs" + newID + "Name", name);

            return newID;
        }

        private static void DeleteTSVs(Context context) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numEntries = prefs.GetInt("OtherTSVsNum", 0);
            var prefsEditor = prefs.Edit();

            prefsEditor.Remove("OtherTSVsSelected");
            prefsEditor.Remove("OtherTSVsNum");

            if (numEntries > 0)
            {

                for (int i = 0; i < numEntries; ++i)
                {
                    prefsEditor.Remove("OtherTSVs" + i + "Name");
                    prefsEditor.Remove("OtherTSVs" + i + "TSV");
                }
            }

            prefsEditor.Commit();
        }
    }
}