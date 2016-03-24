using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public static class HighScores
{
    private static string _filename = "scores.txt";
    private static List<KeyValuePair<string, int>> _scores = new List<KeyValuePair<string, int>>();
    static HighScores()
    {
        Load();
    }

    public static void Add(string name, int score)
    {
        _scores.Add(new KeyValuePair<string, int>(name, score));
        _scores.Sort(delegate(KeyValuePair<string, int> pair1,
                        KeyValuePair<string, int> pair2)
        {
            return pair2.Value.CompareTo(pair1.Value);
        });
    }

    public static int ScoreCount()
    {
        return _scores.Count;
    }

    public static void GetScore(int index, out string name, out int score)
    {
        name = _scores[index].Key;
        score = _scores[index].Value;
    }

    public static void Load()
    {
        if (System.IO.File.Exists(Application.dataPath + "/" + _filename))
        {
            _scores.Clear();
            using (StreamReader sr = new StreamReader(Application.dataPath + "/" + _filename))
            {
                while(!sr.EndOfStream)
                {
                    string[] values = sr.ReadLine().Split('|');
                    _scores.Add(new KeyValuePair<string, int>(values[0], int.Parse(values[1])));
                }
            }
        }
    }

    public static void Save()
    {
        using (StreamWriter sw = new StreamWriter(Application.dataPath + "/" + _filename, false))
        {
            foreach (var score in _scores)
            {
                sw.WriteLine(score.Key + "|" + score.Value);
            }
        }
    }
}
