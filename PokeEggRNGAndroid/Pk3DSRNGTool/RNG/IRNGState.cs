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

namespace Pk3DSRNGTool
{
    public class PRNGState
    {
        private uint? state1;
        private uint[] state2;

        public PRNGState(uint s)
        {
            state1 = s;
        }

        public PRNGState(uint[] s)
        {
            state2 = (uint[])s.Clone();
        }

        public override string ToString() => state1?.ToString("X8") ?? string.Join(",", state2.Select(v => v.ToString("X8")).Reverse());
    }

    internal interface IRNGState
    {
        PRNGState CurrentState();
    }
}
