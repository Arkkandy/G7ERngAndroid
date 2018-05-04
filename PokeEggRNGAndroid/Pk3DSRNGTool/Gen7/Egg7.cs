/*MIT License

Copyright(c) 2017 wwwwwwzx

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.*/

using System.Linq;
using Pk3DSRNGTool.Core;

namespace Pk3DSRNGTool
{
    public class Egg7 : EggRNG
    {
        private static uint getrand => RNGPool.getrand;
        private static void Advance(int n) => RNGPool.Advance(n);

        public bool Homogeneous;
        public bool FemaleIsDitto;

        public override RNGResult Generate()
        {
            ResultE7 egg = new ResultE7();

            // Gender
            if (NidoType)
                egg.Gender = (byte)((getrand & 1) + 1);
            else
            {
                if (RandomGender)
                {
                    egg.genderRnum = getrand;
                    egg.Gender = (byte)((int)(egg.genderRnum % 252) >= Gender ? 1 : 2);
                }
                else
                {
                    egg.Gender = Gender;
                }
            }

            // Nature
            egg.Nature = (byte)(getrand % 25);

            // Everstone
            // Chooses which parent if necessary;
            if (Both_Everstone)
                egg.BE_InheritParents = (getrand & 1) == 0;
            else if (EverStone)
                egg.BE_InheritParents = MaleItem == 1;

            // Ability
            egg.abilityRnum = getrand % 100;
            egg.Ability = (byte)getRandomAbility(InheritAbility, egg.abilityRnum);
            egg.CanInheritHidden = canBeHiddenAbility(egg.abilityRnum);

            // PowerItem
            // Chooses which parent if necessary
            if (Both_Power)
            {
                if ((getrand & 1) == 0)
                    egg.InheritMaleIV[M_Power] = true;
                else
                    egg.InheritMaleIV[F_Power] = false;
            }
            else if (Power)
            {
                if (MaleItem > 2)
                    egg.InheritMaleIV[M_Power] = true;
                else
                    egg.InheritMaleIV[F_Power] = false;
            }

            // Inherit IV
            int tmp;
            for (int i = Power ? 1 : 0; i < InheritIVs_Cnt; i++)
            {
                do { tmp = (int)(getrand % 6); }
                while (egg.InheritMaleIV[tmp] != null);
                egg.InheritMaleIV[tmp] = (getrand & 1) == 0;
            }

            // IVs
            egg.IVs = new[] { -1, -1, -1, -1, -1, -1 };
            for (int j = 0; j < 6; j++)
            {
                egg.IVs[j] = (int)(getrand & 0x1F);
                if (egg.InheritMaleIV[j] == null) continue;
                egg.IVs[j] = (egg.InheritMaleIV[j] == true) ? MaleIVs[j] : FemaleIVs[j];
            }

            // Encryption Constant
            egg.EC = getrand;

            // PID
            for (int i = PID_Rerollcount; i > 0; i--)
            {
                egg.PID = getrand;
                if (egg.PSV == TSV) { egg.Shiny = true; break; }
            }

            // Other TSVs
            tmp = (int)egg.PSV;
            if (ConsiderOtherTSV && OtherTSVs.Contains(tmp))
                egg.Shiny = true;

            // Ball
            egg.Ball = (byte)(Homogeneous && getrand % 100 >= 50 || FemaleIsDitto ? 1 : 2);

            // Egg adopt & Egg Clear
            Advance(2);

            return egg;
        }
    }
}
