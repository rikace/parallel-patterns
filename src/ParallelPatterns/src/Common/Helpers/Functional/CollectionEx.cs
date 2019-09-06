using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{    public static class CollectionEx
    {
        public static IEnumerable<string> EnumLines(this StringReader reader)
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (null == line) yield break;

                yield return line;
            }
        }

        public static T[] ForAll<T>(this T[] array, Action<T> action)
        {
            foreach (var item in array)
                action(item);
            return array;
        }

    }
}
