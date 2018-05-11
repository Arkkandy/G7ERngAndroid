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

namespace Pk3DSRNGTool
{
    public enum GameVersion
    {
        // Not actually stored values, but assigned as properties.
        Any = -1,

        // Version IDs, also stored in PKM structure
        /*Gen6*/
        X = 24, Y = 25, AS = 26, OR = 27,
        /*Gen7*/
        SN = 30, MN = 31, US = 32, UM = 33,

        // Game Groupings (SaveFile type)
        XY = 106,
        ORAS = 108,
        SM = 109,
        USUM = 110,

        Gen6,
        Gen7,
    }

    public static class Extension
    {
        public static bool Contains(this GameVersion g1, GameVersion g2)
        {
            if (g1 == g2 || g1 == GameVersion.Any)
                return true;

            switch (g1)
            {
                case GameVersion.XY: return g2 == GameVersion.X || g2 == GameVersion.Y;
                case GameVersion.ORAS: return g2 == GameVersion.OR || g2 == GameVersion.AS;
                case GameVersion.Gen6:
                    return GameVersion.XY.Contains(g2) || GameVersion.ORAS.Contains(g2);

                case GameVersion.SM: return g2 == GameVersion.SN || g2 == GameVersion.MN;
                case GameVersion.USUM: return g2 == GameVersion.US || g2 == GameVersion.UM;
                case GameVersion.Gen7:
                    return GameVersion.SM.Contains(g2) || GameVersion.USUM.Contains(g2);

                default: return false;
            }
        }
    }
}
