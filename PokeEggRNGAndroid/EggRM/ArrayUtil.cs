using System;
using System.Collections.Generic;
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
    public static class ArrayUtil
    {
        public static int[] SortArrayIndex( string[] arr) {
            int[] indices = Enumerable.Range(0, arr.Length).ToArray();

            Array.Sort(arr, indices);

            int[] indicesSorted = new int[arr.Length];
            for (int i = 0; i < indices.Length; ++i) {
                indicesSorted[indices[i]] = i;
            }

            return indices;
        }

        /*public static List<int> SortArrayIndex<T>(T[] arr, IComparer<T> cp) {
            List<int> idc = new List<int>();
            for (int i= 0; i )
                return idc;
        }*/
    }
}