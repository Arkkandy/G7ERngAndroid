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

using System.Collections.Generic;

namespace Pk3DSRNGTool
{
    public static class Gen7EggPath
    {
        private static int[] Pre; // Previous Node
        private static int[] W; // Weight
        const int accept = 1;
        const int reject = 1;
        public static List<int> Calc(int[] FrameAdvList)
        {
            // Initialize
            int Maxdist = FrameAdvList.Length - 1;
            Pre = new int[Maxdist + 1];
            W = new int[Maxdist + 1];
            for (int i = 1; i <= Maxdist; i++)
                W[i] = int.MaxValue; // Max int32
            // Calc
            for (int i = 0; i <= Maxdist; i++)
            {
                // Reject path
                if (i != 0 && W[i] > W[i - 1] + reject)
                {
                    Pre[i] = i - 1;
                    W[i] = W[i - 1] + reject;
                }
                // Accept Path
                for (int j = i, k = i + FrameAdvList[i]; k <= Maxdist; j = k, k = j + FrameAdvList[j])
                {
                    if (W[k] > W[j] + accept)
                    {
                        Pre[k] = j;
                        W[k] = W[j] + accept;
                    }
                }
            }
            // Summary
            List<int> Results = new List<int>();
            for (int node = Maxdist; node != 0; node = Pre[node]) // Track back
                Results.Add(node);
            Results.Add(0);
            Results.Reverse();
            return Results;
        }

        public static int CountNodes(int[] FrameAdvList, int limit)
        {
            // Initialize
            int Maxdist = limit-1;
            Pre = new int[Maxdist + 1];
            W = new int[Maxdist + 1];
            for (int i = 1; i <= Maxdist; i++)
                W[i] = int.MaxValue; // Max int32
            // Calc
            for (int i = 0; i <= Maxdist; i++)
            {
                // Reject path
                if (i != 0 && W[i] > W[i - 1] + reject)
                {
                    Pre[i] = i - 1;
                    W[i] = W[i - 1] + reject;
                }
                // Accept Path
                for (int j = i, k = i + FrameAdvList[i]; k <= Maxdist; j = k, k = j + FrameAdvList[j])
                {
                    if (W[k] > W[j] + accept)
                    {
                        Pre[k] = j;
                        W[k] = W[j] + accept;
                    }
                }
            }
            // Summary
            int numNodes = 1;
            for (int node = Maxdist; node != 0; node = Pre[node]) // Track back
                numNodes++;
            return numNodes;
        }
    }
}
