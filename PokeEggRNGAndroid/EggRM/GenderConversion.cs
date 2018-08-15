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
    public static class GenderConversion
    {
        public enum GenderType {
            SameRatio,
            M7to1F,
            M3to1F,
            M1to3F,
            M1to7F,
            MOnly,
            FOnly,
            Genderless
        }
        public static int ConvertGenderIndexToByte(int index)
        {
            switch ((GenderType)index) {
                case GenderType.SameRatio: return 0x7F;
                case GenderType.M7to1F: return 0x1F;
                case GenderType.M3to1F: return 0x3F;
                case GenderType.M1to3F: return 0xBF;
                case GenderType.M1to7F: return 0xE1;
                case GenderType.Genderless: return 0xFF;
                case GenderType.MOnly: return 0x00;
                default: /*case GenderType.FOnly*/ return 0xFE;
            }
            //return 0;
        }

        public static string GetGenderString(GenderType genderCode) {
            switch (genderCode) {
                case GenderType.SameRatio: return "(1:1)";
                case GenderType.M7to1F: return "(7:1)";
                case GenderType.M3to1F: return "(3:1)";
                case GenderType.M1to3F: return "(1:3)";
                case GenderType.M1to7F: return "(1:7)";
                case GenderType.Genderless: return "";
                case GenderType.MOnly: return "";
                case GenderType.FOnly: return "";
            }
            return "";
        }

        public static GenderType ConvertByteToGenderType(byte genderRatio) {
            switch (genderRatio)
            {
                case 0x7F: return GenderType.SameRatio;
                case 0x1F: return GenderType.M7to1F;
                case 0x3F: return GenderType.M3to1F;
                case 0xBF: return GenderType.M1to3F;
                case 0xE1: return GenderType.M1to7F;
                case 0xFF: return GenderType.Genderless;
                case 0x00: return GenderType.MOnly;
                default: /*case 0xFE*/ return GenderType.FOnly;
            }
        }

        // Use the GenderType enum with this function
        public static bool IsRandomGender(GenderType gt)
        {
            return Pk3DSRNGTool.FuncUtil.getGenderRatio(ConvertGenderIndexToByte((int)gt)) > 0x0F;
        }

        // Use the raw gender byte value with this function
        public static bool IsRandomGender(byte gender)
        {
            return gender > 0x0F;
        }

        public static string GetRandomGenderString(uint rnum)
        {
            string gs = "";

            byte[] vals = { 30, 62, 126, 190, 224 };
            int rval = (int)(rnum % 252);
            for (int i = 0; i < vals.Length; ++i)
            {
                byte gender = vals[i];
                gs += (rval >= gender ? "♂" : "♀");
            }

            return gs;
        }

        public static List<bool> GetRandomGenderBool(uint rnum)
        {
            List<bool> lgen = new List<bool>();

            byte[] vals = { 30, 62, 126, 190, 224 };
            for (int i = 0; i < vals.Length; ++i)
            {
                byte gender = vals[i];
                lgen.Add((int)(rnum % 252) >= gender);
            }

            return lgen;
        }

    }
}