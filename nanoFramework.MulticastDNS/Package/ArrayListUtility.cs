using System;
using System.Collections;

namespace nanoFramework.MulticastDNS.Package
{
    internal static class ArrayListUtility
    {
        public static void AddRange(this ArrayList arrayList, Array bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                arrayList.Add(bytes.GetValue(i));
        }

        public static void AddRange(this ArrayList target, ArrayList source)
        {
            for (int i = 0; i < source.Count; i++)
                target.Add(source[i]);
        }
    }
}
