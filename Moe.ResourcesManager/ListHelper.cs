using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moe.ResourcesManager;

public static class ListHelper
{
    public static IEnumerable<T> FastReverse<T>(this IList<T> items)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            yield return items[i];
        }
    }

}
