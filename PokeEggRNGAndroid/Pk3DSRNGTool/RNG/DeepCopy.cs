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

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Pk3DSRNGTool.RNG
{
    static class DeepCopyUtils
    {
        public static object DeepCopy(this object target)
        {

            object result;
            BinaryFormatter b = new BinaryFormatter();

            MemoryStream mem = new MemoryStream();

            try
            {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = b.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;
        }
    }

    public static class DeepCopyHelper
    {
        public static T DeepCopy<T>(T target)
        {

            T result;
            BinaryFormatter b = new BinaryFormatter();

            MemoryStream mem = new MemoryStream();

            try
            {
                b.Serialize(mem, target);
                mem.Position = 0;
                result = (T)b.Deserialize(mem);
            }
            finally
            {
                mem.Close();
            }

            return result;
        }
    }
}
