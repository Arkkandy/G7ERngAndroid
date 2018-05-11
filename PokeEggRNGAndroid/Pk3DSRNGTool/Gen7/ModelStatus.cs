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

using Pk3DSRNGTool.RNG;

namespace Pk3DSRNGTool
{
    internal class ModelStatus
    {
        private SFMT sfmt;
        private int cnt;
        private ulong getrand { get { cnt++; return sfmt.Nextulong(); } }

        public byte Modelnumber;
        public int[] remain_frame;
        public bool phase;

        public bool IsBoy;
        public int fidget_cd = -1; // fidget cooldown. -1 to ignore fidget

        public bool raining;

        public ModelStatus(byte n, SFMT st)
        {
            sfmt = (SFMT)st.DeepCopy();
            Modelnumber = n;
            remain_frame = new int[n];
        }

        public int NextState()
        {
            cnt = 0;
            if (fidget_cd > 0 && --fidget_cd == 0)
                fidget_cd = fidget();
            for (int i = 0; i < Modelnumber; i++)
            {
                if (remain_frame[i] > 1)                       //Cooldown 2nd part
                {
                    remain_frame[i]--;
                    continue;
                }
                if (remain_frame[i] < 0)                       //Cooldown 1st part
                {
                    if (++remain_frame[i] == 0)                //Blinking
                        remain_frame[i] = getrand % 3 == 0 ? 36 : 30;
                    continue;
                }
                if ((int)(getrand & 0x7F) == 0)                //Not Blinking
                    remain_frame[i] = -5;
            }
            if (raining && (phase = !phase))
                frameshift(2);
            return cnt;
        }

        private int[] Delay_M = new[] { 407, 412 };
        private int[] Delay_F = new[] { 402, 407 };
        private int fidget() // For more precise timeline
        {
            int delay1 = (int)(getrand % 90); // Random delay
            int delay2 = IsBoy ? Delay_M[getrand & 1] : Delay_F[getrand & 1]; // Decide movement type non-jump/jump
            return delay1 + delay2;
        }

        public void frameshift(int n)
        {
            for (int i = 0; i < n; i++)
                sfmt.Next();
            cnt += n;
        }

        public void CopyTo(ModelStatus st)
        {
            st.remain_frame = (int[])remain_frame.Clone();
            st.phase = phase;
        }

        public ModelStatus Clone()
        {
            ModelStatus st = new ModelStatus(Modelnumber, sfmt);
            CopyTo(st);
            st.raining = raining;
            st.fidget_cd = fidget_cd;
            st.IsBoy = IsBoy;
            return st;
        }
    }
}