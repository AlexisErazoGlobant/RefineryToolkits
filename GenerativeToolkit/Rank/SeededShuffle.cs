﻿#region namespaces
using Autodesk.DesignScript.Runtime;
using System;
using System.Collections;
using System.Linq;
#endregion

namespace Autodesk.GenerativeToolkit.Rank
{

    public static class ListShuffle
    {
        /// <summary>
        /// Shuffles a list, randomizing the order of its items based on the seed.
        /// </summary>
        /// <param name="list">List to shuffle</param>
        /// <param name="seed">Seed</param>
        /// <returns>randomized list</returns>
        public static IList SeededShuffle(IList list, int seed)
        {
            var rng = new Random(seed);
            return list.Cast<object>().OrderBy(_ => rng.Next()).ToList();
        }
    }
}
