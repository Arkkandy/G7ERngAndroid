using System;
using System.Collections.Generic;
using System.IO;
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
    public class FileHelper
    {
        public static byte[] ReadFileBytes(Android.Content.Res.AssetManager assets, string path)
        {
            var sr = new StreamReader(assets.Open(path));
            var memStream = new MemoryStream();
            sr.BaseStream.CopyTo(memStream);
            return memStream.ToArray();
        }
    }
}