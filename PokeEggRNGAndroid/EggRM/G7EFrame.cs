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

    /*public class G7MainEggData {

    }*/

    public class G7EFrame
    {
        private static readonly string[] blinkmarks = { "-", "★", "?", "? ★", "<?>" };


        public ResultE7 egg;

        public G7EFrame(ResultE7 result, int frame = -1, int eggnum = -1)
        {
            egg = result;
            FrameNum = frame;
            EggNum = eggnum;
        }

        public G7EFrame(ResultME7 result, int frame, int time, byte blink) {
            egg = ResultME7.Egg as ResultE7;
            MainPSV = result.PSV;
            MainShiny = result.Shiny;
            FrameNum = frame;
            realTime = time;
            Blink = blink;
            FrameDelayUsed = result.FrameDelayUsed;
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

        // Basic Data
        public int EggNum { get; private set; }
        public int FrameNum { get; private set; }
        private int realTime = -1;
        public int FrameDelayUsed;
        public byte Blink;

        // Egg Advances: including accept/reject, plus and # advances
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

        // MainEggRNG
        public int ShiftF => realTime > -1 ? realTime - 0 : 0; //realTime-standard
        public string RealTime => realTime > -1 ? FuncUtil.Convert2timestr(realTime / 60.0) : string.Empty;
        public string Mark => Blink < 5 ? blinkmarks[Blink] : Blink.ToString();
        public uint MainPSV;
        public bool MainShiny;

        //public string Mark => Blink < 5 ? blinkmarks[Blink] : Blink.ToString();
    }
}