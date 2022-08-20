﻿namespace TomsToolbox.Essentials;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Some enumerators for various scenarios.
/// </summary>
public static class Enumerate
{
    /// <summary>
    /// Enumerates the elements of two enumerations as tuples.
    /// </summary>
    /// <typeparam name="T1">The type of the first collection.</typeparam>
    /// <typeparam name="T2">The type of the second collection.</typeparam>
    /// <param name="first">The first collection.</param>
    /// <param name="second">The second collection.</param>
    /// <returns>Tuples of the elements.</returns>
    /// <remarks>If the number of elements in each collection is different, the smaller collection determines the number of enumerated items.</remarks>
    public static IEnumerable<Tuple<T1, T2>> AsTuples<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second)
    {
        using var e1 = first.GetEnumerator();
        using var e2 = second.GetEnumerator();
        while (e1.MoveNext() && e2.MoveNext())
        {
            yield return new Tuple<T1, T2>(e1.Current, e2.Current);
        }
    }

    /// <summary>
    /// Enumerates the elements of two enumerations as tuples.
    /// </summary>
    /// <param name="first">The first collection.</param>
    /// <param name="second">The second collection.</param>
    /// <returns>Tuples of the elements.</returns>
    /// <remarks>If the number of elements in each collection is different, the smaller collection determines the number of enumerated items.</remarks>
    public static IEnumerable<Tuple<object, object>> AsTuples(IEnumerable first, IEnumerable second)
    {
        var e1 = first.GetEnumerator();
        var e2 = second.GetEnumerator();

        try
        {
            while (e1.MoveNext() && e2.MoveNext())
            {
                yield return new Tuple<object, object>(e1.Current, e2.Current);
            }
        }
        finally
        {
            Disposable.Dispose(e1);
            Disposable.Dispose(e2);
        }
    }
}