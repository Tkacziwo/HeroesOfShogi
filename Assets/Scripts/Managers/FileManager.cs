using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Loads data from files to use withing game.
/// </summary>
public class FileManager : MonoBehaviour
{
    [SerializeField] private TextAsset File;

    Dictionary<string, string> MovesetDictionary;

    [SerializeField] private string positioning;

    public PositionCollection PiecesPositions;

    private readonly List<Tuple<string, string>> tutorialMessages = new();


    [Serializable]
    public class PieceInfo
    {
        public int posX, posY;
        public string piece;
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
        LoadTutorialMessagesFromFile();
    }

    /// <summary>
    /// Loads tutorial messages from file.
    /// </summary>
    private void LoadTutorialMessagesFromFile()
    {
        var file = Resources.Load<TextAsset>("Tutorial/Tutorial");
        if (file != null)
        {
            string fs = file.text;
            string[] fLines = Regex.Split(fs, "\n|\r|\r\n");
            for (int i = 0; i < fLines.Length; i++)
            {
                string[] values = Regex.Split(fLines[i], "#");
                if (values[0] != "")
                {
                    tutorialMessages.Add(new(values[0], values[1]));
                }
            }
        }
    }
    
    /// <summary>
    /// Loads moveset from text file.
    /// </summary>
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

    /// <summary>
    /// Returns moveset by providing piece name.
    /// </summary>
    /// <param name="name">piece name</param>
    /// <returns>int[]</returns>
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

    /// <summary>
    /// Loads pieces models from file.
    /// </summary>
    private void LoadPiecesFromFile()
    {
        var file = Resources.Load<TextAsset>(positioning);
        if (file != null)
        {
            var result = JsonUtility.FromJson<PositionCollection>(file.text);
            PiecesPositions = new PositionCollection(result);
        }
    }

    /// <summary>
    /// Gets tutorial messages from tutorialMessages field.
    /// </summary>
    /// <returns>List of tutorial messages</returns>
    public List<Tuple<string, string>> GetTutorialMessages()
        => tutorialMessages;
}
