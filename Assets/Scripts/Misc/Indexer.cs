using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Indexer 
{
    public enum IndexType
    {
        After,
        Before,
        Across,
        AfterAcross,
        BeforeAcross
    }
    private delegate int CalculateIndex(int index, int count);


    private static Dictionary<IndexType, CalculateIndex> indexers;

    static Indexer()
    {
        indexers = new Dictionary<IndexType, CalculateIndex>();
        indexers.Add(IndexType.After, After);
        indexers.Add(IndexType.Before, Before);
        indexers.Add(IndexType.BeforeAcross, BeforeAcross);
        indexers.Add(IndexType.AfterAcross, AfterAcross);
    }


    public static int GetIndex(IndexType type, int index, int count)
    {
        return indexers[type](index, count);
    }

    private static int After(int index, int count)
    {
        return (index + 1) % count;
    }

    private static int Before(int index, int count)
    {
        return index == 0 ? count - 1 : index - 1;
    }

    private static int Across(int index, int count)
    {
        return (index + count / 2) % count;
    }

    private static int BeforeAcross(int index, int count)
    {
        int ind = (index + count / 2) % count;
        return ind == 0 ? count - 1 : ind - 1;
    }

    private static int AfterAcross(int index, int count)
    {
        return ((index + count / 2) + 1) % count;
    }
}
