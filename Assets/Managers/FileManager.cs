using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

public class FileManager : MonoBehaviour
{
    [SerializeField] private TextAsset File;

    Dictionary<string, string> MovesetDictionary;

    [SerializeField] private string positioning;

    public PositionCollection PiecesPositions;


    [Serializable]
    public class PieceInfo
    {
        public int posX, posY;
        public string  piece;
    }

    [Serializable]
    public class PositionCollection
    {
        public PositionCollection(PositionCollection other)
        {
            this.boardPositions = other.boardPositions;
        }

        public PieceInfo[] boardPositions;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadMovesetsFromFile();
        LoadPiecesFromFile();
    }

    public void LoadMovesetsFromFile()
    {
        MovesetDictionary = new();
        string fileContents = File.text;
        string pattern = @"([A-Za-z]+)\s([0-3]*)(\n|\r|\r\n)+";
        var matches = Regex.Matches(fileContents, pattern);
        foreach (Match m in matches)
        {
            GroupCollection groups = m.Groups;
            MovesetDictionary.Add(groups[1].Value, groups[2].Value);
        }
        Debug.Log(File.text);
    }

    public int[] GetMovesetByPieceName(string name)
    {
        string val = MovesetDictionary.GetValueOrDefault(name);
        int[] arr = new int[9];
        for (int i = 0; i < 9; i++)
        {
            arr[i] = (int)char.GetNumericValue(val[i]);
        }

        return arr;
    }
    
    private void LoadPiecesFromFile()
    {
        var file = Resources.Load<TextAsset>(positioning);
        if (file != null)
        {
            var result = JsonUtility.FromJson<PositionCollection>(file.text);
            PiecesPositions = new PositionCollection(result);
        }
    }
}
