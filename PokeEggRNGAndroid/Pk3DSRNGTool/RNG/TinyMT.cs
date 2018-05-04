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

using System;

namespace Pk3DSRNGTool.RNG
{
    [Serializable()]
    public class TinyMT : IRNG, IRNGState
    {
        public uint[] status { get; set; }
        public const uint mat1 = 0x8f7011ee;
        public const uint mat2 = 0xfc78ff1f;
        public const uint tmat = 0x3793fdff;

        private const int MIN_LOOP = 8;
        private const int PRE_LOOP = 8;

        private const uint TINYMT32_MASK = 0x7FFFFFFF;
        private const int TINYMT32_SH0 = 1;
        private const int TINYMT32_SH1 = 10;
        private const int TINYMT32_SH8 = 8;

        public TinyMT(uint seed)
        {
            init(seed);
        }

        public TinyMT(uint[] st)
        {
            status = new uint[4];
            st.CopyTo(status, 0);
        }

        public void init(uint seed)
        {
            status = new uint[] { seed, mat1, mat2, tmat };

            for (int i = 1; i < MIN_LOOP; i++)
                status[i & 3] ^= (uint)i + 1812433253U * (status[(i - 1) & 3] ^ (status[(i - 1) & 3] >> 30));

            period_certification();

            for (int i = 0; i < PRE_LOOP; i++)
                nextState();
        }

        public void nextState()
        {
            uint y = status[3];
            uint x = (status[0] & TINYMT32_MASK) ^ status[1] ^ status[2];
            x ^= (x << TINYMT32_SH0);
            y ^= (y >> TINYMT32_SH0) ^ x;
            status[0] = status[1];
            status[1] = status[2];
            status[2] = x ^ (y << TINYMT32_SH1);
            status[3] = y;

            if ((y & 1) == 1)
            {
                status[1] ^= mat1;
                status[2] ^= mat2;
            }
        }

        public uint temper()
        {
            uint t0 = status[3];
            uint t1 = status[0] + (status[2] >> TINYMT32_SH8);

            t0 ^= t1;
            if ((t1 & 1) == 1)
            {
                t0 ^= tmat;
            }
            return t0;
        }

        #region IRNG Member
        public void Next()
        {
            nextState();
        }

        public uint Nextuint()
        {
            nextState();
            return temper();
        }

        public void Reseed(uint seed)
        {
            init(seed);
        }
        
        public PRNGState CurrentState() => new PRNGState(status);
        #endregion

        private void period_certification()
        {
            if ((status[0] & TINYMT32_MASK) == 0 && status[1] == 0 && status[2] == 0 && status[3] == 0)
                status = new uint[] { 'T', 'I', 'N', 'Y' };
        }
    }
}
