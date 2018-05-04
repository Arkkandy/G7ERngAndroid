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
using Pk3DSRNGTool;

namespace Gen7EggRNG.EggRM
{
    public enum AdvanceType {
        Reject,
        Accept
    }

    public class G7EFrame
    {
        public ResultE7 egg;

        public G7EFrame(ResultE7 result, int frame = -1, int eggnum = -1)
        {
            egg = result;
            FrameNum = frame;
            EggNum = eggnum;
        }

        public string GetNatureStr() {
            if (null != egg?.BE_InheritParents) {
                return egg.BE_InheritParents == true ? PokeRNGApp.Strings.male : PokeRNGApp.Strings.female;
            }
            return PokeRNGApp.Strings.natures[egg.Nature];
        }
        public string GetHiddenPowerStr() {
            return PokeRNGApp.Strings.hiddenpowers[egg.hiddenpower];
        }

        public string GetInheritanceString() {
            string inh = "";
            for (int i = 0; i < 6; ++i) {
                if (i > 0) { inh += " "; }
                if (egg.InheritMaleIV[i] != null)
                {
                    inh += ((bool)egg.InheritMaleIV[i] ? "M" : "F");
                }
                else {
                    inh += "O";
                }
            }
            return inh;
        }

        // DataSource Display Block
        public int EggNum { get; private set; }
        public int FrameNum { get; private set; }

        public string GetAdvance() {
            return "+" + egg.FramesUsed.ToString();
        }
        public string AdvanceInfo;
        public AdvanceType AdvType; //
        public string GetAdvanceTypeString() {
            return (AdvType == AdvanceType.Accept ? PokeRNGApp.Strings.accept : PokeRNGApp.Strings.reject);
        }

        public int HP => egg.IVs[0];
        public int Atk => egg.IVs[1];
        public int Def => egg.IVs[2];
        public int SpA => egg.IVs[3];
        public int SpD => egg.IVs[4];
        public int Spe => egg.IVs[5];

        //public char Sync => rt.Synchronize ? 'O' : 'X';
        public uint PSV => egg.PSV;
        public string GenderStr => PokeRNGApp.Strings.genderSymbols[egg.Gender];
        public string AbilityStr => PokeRNGApp.Strings.abilitySymbols[egg.Ability];


        public string Ball => egg.Ball == 1 ? PokeRNGApp.Strings.male : PokeRNGApp.Strings.female;
        //public string Item => (rt as WildResult)?.ItemStr ?? string.Empty;
        public uint Rand => egg.RandNum;
        public ulong Rand64 => egg.EggSeed ?? 0;
        public uint PID => egg.PID;
        public uint EC => egg.EC;
        public string WurmpleEvo => (egg.EC >> 16) % 10 < 5 ? PokeRNGApp.Strings.beautiflyName : PokeRNGApp.Strings.dustoxName;
        //public string State =>  string.Empty;
        //public PRNGState _tinystate;
        public string TinyState => egg.Status.ToString();

    }
}