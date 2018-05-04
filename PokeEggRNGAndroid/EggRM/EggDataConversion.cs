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
    public static class EggDataConversion
    {
        public static int GetRandomAbility(int ability, uint value)
        {
            switch (ability)
            {
                case 0:
                    return value < 0x50 ? 1 : 2;
                case 1:
                    return value < 0x14 ? 1 : 2;
                case 2:
                    if (value < 0x14) return 1;
                    if (value < 0x28) return 2;
                    return 3;
            }
            return 0;
        }

        public static string GetAbilityString(uint value) {
            string abstring = "";
            abstring += GetRandomAbility(0, value).ToString();
            abstring += GetRandomAbility(1, value).ToString();
            int h = GetRandomAbility(2, value);
            abstring += (h == 3 ? "H" : h.ToString());
            return abstring;
        }
    }
}