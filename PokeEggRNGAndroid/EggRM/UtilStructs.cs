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

namespace Gen7EggRNG.EggRM
{
    public class FullSearchData {
        public AppPreferences preferences;
        public ProfileData profile;
        public ParentData parents;
        public FilterData filter;
        public SearchParams searchParameters;
        public StationaryData stationary;
        public List<int> otherTSVs;

        /*public FullSearchData() {
            preferences = new AppPreferences();
            profile = new ProfileData();
        }*/

        public FullSearchData Copy() {
            FullSearchData newData = new FullSearchData();
            newData.preferences = new AppPreferences(preferences);
            newData.profile = new ProfileData(profile);
            newData.parents = parents;
            newData.filter = filter;
            newData.stationary = new StationaryData(stationary);
            newData.searchParameters = searchParameters;
            newData.otherTSVs = new List<int>(otherTSVs);

            return newData;
        }
    }

    public class EggSeed
    {
        public uint s0, s1, s2, s3;
        public EggSeed() {}

        public EggSeed(EggSeed es) { s0 = es.s0; s1 = es.s1; s2 = es.s2; s3 = es.s3; }

        public uint[] GetSeedVector() {
            return new uint[] { s0, s1, s2, s3 };
        }

        public void SetSeed(string seed, char separator =',')
        {
            string[] Data = seed.Split(separator);
            /*s3 = Convert.ToUInt32(Data[0], 16);
            s2 = Convert.ToUInt32(Data[1], 16);
            s2 = Convert.ToUInt32(Data[2], 16);
            s0 = Convert.ToUInt32(Data[3], 16);*/
            SetSeed(Data[0], Data[1], Data[2], Data[3]);
        }

        public void SetSeed(string ss3, string ss2, string ss1, string ss0) {
            uint.TryParse(ss3, System.Globalization.NumberStyles.HexNumber, null, out s3);
            uint.TryParse(ss2, System.Globalization.NumberStyles.HexNumber, null, out s2);
            uint.TryParse(ss1, System.Globalization.NumberStyles.HexNumber, null, out s1);
            uint.TryParse(ss0, System.Globalization.NumberStyles.HexNumber, null, out s0);
        }
        public void SetSeed(uint[] statuses) {
            if (statuses != null && statuses.Length == 4)
            {
                s0 = statuses[0];
                s1 = statuses[1];
                s2 = statuses[2];
                s3 = statuses[3];
            }
        }
        public string GetSeedToString( string separator = "," ) {
            return s3.ToString("X") + separator + s2.ToString("X") + separator + s1.ToString("X") + separator + s0.ToString("X");
        }
        public string[] GetSeedToStringArray() {
            return new string[] { s3.ToString("X"), s2.ToString("X"), s1.ToString("X"), s0.ToString("X") };
        }
    }

    public struct IVSet {
        public int hp;
        public int atk;
        public int def;
        public int spa;
        public int spd;
        public int spe;

        public void SetIVs(int hp, int atk, int def, int spa, int spd, int spe) {
            this.hp = hp;
            this.atk = atk;
            this.def = def;
            this.spa = spa;
            this.spd = spd;
            this.spe = spe;
        }

        public void SetFromString(string ivs) {
            var ivarr = ivs.Split(',');
            if (ivarr.Length == 6) {
                hp = int.Parse(ivarr[0]);
                atk = int.Parse(ivarr[1]);
                def = int.Parse(ivarr[2]);
                spa = int.Parse(ivarr[3]);
                spd = int.Parse(ivarr[4]);
                spe = int.Parse(ivarr[5]);
            }
        }
        public string CreateString()
        {
            return hp + "," + atk + "," + def + "," + spa + "," + spd + "," + spe;
        }
        public override string ToString() {
            return CreateString();
        }
    }

    public class SFrameComparer : IComparer<G7SFrame>
    {
        int IComparer<G7SFrame>.Compare(G7SFrame x, G7SFrame y)
        {
            return (x.FrameNum + x.pokemon.FrameDelayUsed).CompareTo(y.FrameNum + y.pokemon.FrameDelayUsed);
        }
    }

    public class SFrameNumComparer : IComparer<G7SFrame> {
        int IComparer<G7SFrame>.Compare(G7SFrame x, G7SFrame y)
        {
            if (x.FrameNum != y.FrameNum)
            {
                return x.FrameNum.CompareTo(y.FrameNum);
            }
            else {
                return y.FrameDelayUsed.CompareTo(x.FrameDelayUsed);
            }
        }
    }
}