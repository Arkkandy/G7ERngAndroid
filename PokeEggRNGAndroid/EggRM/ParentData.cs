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

using Android.Preferences;

namespace Gen7EggRNG.EggRM
{
    public struct ParentData
    {
        public int whoIsDitto;

        public int maleItem;
        public int maleAbility;
        public int[] maleIV;

        public int femaleItem;
        public int femaleAbility;
        public int[] femaleIV;

        public bool isSameDex;
        public int genderCode;
        public bool isMasuda;
        public bool isNidoSpecies;

        public static ParentData LoadParentData(Context context) {
            ParentData pd = new ParentData();

            // Load Profile Data
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            string maleivstring = prefs.GetString("MaleIVs", "31,31,31,31,31,31");
            pd.maleIV = Array.ConvertAll(maleivstring.Split(','), s => int.Parse(s));

            string femaleivstring = prefs.GetString("FemaleIVs", "31,31,31,31,31,31");
            pd.femaleIV = Array.ConvertAll(femaleivstring.Split(','), s => int.Parse(s));

            // Shouldn't happen - but in case things go wrong
            /*if (pd.maleIV.Length != 6) {
                pd.maleIV = new int[6] { 31, 31, 31, 31, 31, 31 };
            }
            if (pd.femaleIV.Length != 6)
            {
                pd.femaleIV = new int[6] { 31, 31, 31, 31, 31, 31 };
            }*/


            // Parent Items
            pd.maleItem = prefs.GetInt("MaleItem", 0);
            pd.femaleItem = prefs.GetInt("FemaleItem", 0);

            // Parent Abilities
            pd.maleAbility = prefs.GetInt("MaleAbility", 0);
            pd.femaleAbility = prefs.GetInt("FemaleAbility", 0);

            // Ditto Status
            pd.whoIsDitto = prefs.GetInt("DittoWho", 0);

            // Gender Ratio
            pd.genderCode = prefs.GetInt("BreedGender", 0);

            // Masuda
            pd.isMasuda = prefs.GetBoolean("IsMasuda", false);

            // Same Dex?
            pd.isSameDex = prefs.GetBoolean("IsSameDex", false);

            // Nidoran?
            pd.isNidoSpecies = prefs.GetBoolean("IsNido", false);

            return pd;
        }

        public static void SaveParentData(Context context, ParentData data)
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
            ISharedPreferencesEditor prefsEdit = prefs.Edit();
            if (data.maleIV.Length == 6) {
                prefsEdit.PutString("MaleIVs", string.Join(",", data.maleIV));
            }
            if (data.maleIV.Length == 6)
            {
                prefsEdit.PutString("FemaleIVs", string.Join(",", data.femaleIV));
            }

            // Parent Items
            prefsEdit.PutInt("MaleItem", data.maleItem );
            prefsEdit.PutInt("FemaleItem", data.femaleItem);


            // Parent Abilities
            prefsEdit.PutInt("MaleAbility", data.maleAbility);
            prefsEdit.PutInt("FemaleAbility", data.femaleAbility);

            // Ditto Status
            prefsEdit.PutInt("DittoWho", data.whoIsDitto);

            // Gender Ratio
            prefsEdit.PutInt("BreedGender", data.genderCode);

            // Masuda
            prefsEdit.PutBoolean("IsMasuda", data.isMasuda);

            // Same Dex?
            prefsEdit.PutBoolean("IsSameDex", data.isSameDex);

            // Nidoran?
            prefsEdit.PutBoolean("IsNido", data.isNidoSpecies);

            prefsEdit.Commit();
        }
    }

    public class ParentIVTemplate {
        public string name;
        public IVSet ivs;


        public static List<ParentIVTemplate> LoadParentTemplates(Context context) {
            List<ParentIVTemplate> list = new List<ParentIVTemplate>();

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numTemps = prefs.GetInt("ParentTempNum", 0);

            if (numTemps > 0) {
                for (int i = 1; i <= numTemps; ++i) {
                    ParentIVTemplate newTpl = new ParentIVTemplate();

                    newTpl.name = prefs.GetString("ParentTemp" + i + "Name", "?????");
                    newTpl.ivs.SetFromString(prefs.GetString("ParentTemp" + i + "IV", "0,0,0,0,0,0"));

                    list.Add(newTpl);
                }                
            }
            
            return list;
        }

        public static void SaveParentTemplates(Context context, List<ParentIVTemplate> templates )
        {
            if (templates != null)
            {
                DeleteParentTemplates(context);

                if (templates.Count > 0)
                {
                    ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);
                    var edit = prefs.Edit();

                    for (int i = 0; i < templates.Count; ++i)
                    {
                        edit.PutString("ParentTemp" + (i + 1) + "Name", templates[i].name);
                        edit.PutString("ParentTemp" + (i + 1) + "IV", templates[i].ivs.ToString());
                    }
                    edit.PutInt("ParentTempNum", templates.Count);
                    edit.Commit();
                }
            }
        }

        public static void DeleteParentTemplates(Context context) {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(context);

            int numFilters = prefs.GetInt("ParentTempNum",0);

            if (numFilters > 0)
            {
                var edit = prefs.Edit();
                edit.Remove("ParentTempNum");

                for (int i = 1; i <= numFilters; ++i) {
                    edit.Remove("ParentTemp" + i + "Name");
                    edit.Remove("ParentTemp" + i + "IV");
                }

                edit.Commit();
            }
        }
    }
}