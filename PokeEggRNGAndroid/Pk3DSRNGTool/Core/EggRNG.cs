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

namespace Pk3DSRNGTool.Core
{
    public abstract class EggRNG : IPKM, IGenerator
    {
        public ushort TSV { get; set; }
        public byte TRV { get; set; }
        public int[] MaleIVs { get; set; }
        public int[] FemaleIVs { get; set; }
        public byte MaleItem { get; set; }
        public byte FemaleItem { get; set; }
        public byte InheritAbility { get; set; }

        public bool ShinyCharm, MMethod, NidoType;

        public bool ConsiderOtherTSV;
        public int[] OtherTSVs;

        protected bool DestinyKnot { get; set; }
        protected bool EverStone { get; set; }
        protected bool Both_Everstone { get; set; }
        protected bool Power { get; set; }
        protected bool Both_Power { get; set; }
        protected byte M_Power { get; set; }
        protected byte F_Power { get; set; }

        protected byte PID_Rerollcount { get; set; }
        protected byte InheritIVs_Cnt { get; set; }
        protected bool RandomGender { get; set; }

        public virtual void Delay() { }
        public abstract RNGResult Generate();

        public void MarkItem()
        {
            EverStone = MaleItem == 1 || FemaleItem == 1;
            Both_Everstone = MaleItem == 1 && FemaleItem == 1;
            DestinyKnot = MaleItem == 2 || FemaleItem == 2;
            Power = MaleItem > 2 || FemaleItem > 2;
            Both_Power = MaleItem > 2 && FemaleItem > 2;
            M_Power = (byte)(MaleItem - 3);
            F_Power = (byte)(FemaleItem - 3);

            if (ShinyCharm)
                PID_Rerollcount += 2;
            if (MMethod)
                PID_Rerollcount += 6;

            InheritIVs_Cnt = (byte)(DestinyKnot ? 5 : 3);
            RandomGender = Gender > 0x0F;
        }

        protected static int getRandomAbility(int ability, uint value)
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

        // Check whether or no this frame can
        protected static bool canBeHiddenAbility(uint value)
        {
            if (value < 0x14) return false;
            if (value < 0x28) return false;
            return true;
        }
    }
}
