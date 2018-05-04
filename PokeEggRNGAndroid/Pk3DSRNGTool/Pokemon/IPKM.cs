﻿/*MIT License

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
    public abstract class IPKM
    {
        //public virtual GameVersion Version { get; protected set; } = GameVersion.Any;
        public short Species;
        public byte Forme;

        public virtual byte Ability { get; set; }
        public virtual byte Nature { get; set; }
        public virtual byte Gender { get; set; }

        public int[] IVs;
        public byte Level;

        //public PKHeX.Core.PersonalInfo info => (GameVersion.Gen6.Contains(Version) ? PersonalTable.ORAS : PersonalTable.USUM).getFormeEntry(Species, Forme);
    }
}