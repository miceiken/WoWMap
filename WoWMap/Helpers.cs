using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMap
{
    public class Helpers
    {
        public static IEnumerable<string> SplitStrings(byte[] data)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == '\0') // Terminate string
                {
                    if (sb.Length > 1)
                        yield return sb.ToString();
                    sb = new StringBuilder();

                    continue;
                }

                sb.Append((char)data[i]);
            }
        }
    }
}
