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
using Pk3DSRNGTool.Core;

namespace Gen7EggRNG.EggRM
{
    public class G7SFrame
    {
        public Result7 pokemon;

        public G7SFrame(Result7 result, int frame = -1, int shiftStandard = 0, int time = -1, byte blink = 0)
        {
            pokemon = result;

            FrameNum = frame;
            realTime = time;
            Blink = blink;
            FrameDelayUsed = result.FrameDelayUsed;

            ShiftF = realTime - shiftStandard;

            pokemon.hiddenpower = (byte)HiddenPower.GetHiddenPowerValue(result.IVs);
        }

        public string GetNatureStr()
        {
            return PokeRNGApp.Strings.natures[pokemon.Nature];
        }

        public string GetHiddenPowerStr()
        {
            return PokeRNGApp.Strings.hiddenpowers[pokemon.hiddenpower];
        }

        public int FrameNum { get; private set; }
        public int realTime = -1;
        public int FrameDelayUsed;
        public byte Blink;

        //public int realtime;
        public int ShiftF;
        public string RealTime => realTime > -1 ? FuncUtil.Convert2timestr(realTime / 60.0) : string.Empty;
        public string Mark => Blink < 5 ? G7EFrame.blinkmarks[Blink] : Blink.ToString();
        public ulong Rand64 => pokemon.RandNum;

        public uint EC => pokemon.EC;

        public int HP => pokemon.IVs[0];
        public int Atk => pokemon.IVs[1];
        public int Def => pokemon.IVs[2];
        public int SpA => pokemon.IVs[3];
        public int SpD => pokemon.IVs[4];
        public int Spe => pokemon.IVs[5];

        public int[] GetStats(int[] baseStats) {
            return Pokemon.getStats(pokemon.IVs, pokemon.Nature, pokemon.Level, baseStats);
            //pokemon.Stats = Pokemon.getStats(pokemon.IVs, pokemon.Nature, pokemon.Level, baseStats);
        }

        public string AbilityStr => PokeRNGApp.Strings.abilitySymbols[pokemon.Ability];
    }
}