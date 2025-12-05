using System.Collections.Generic;
using UnityEngine;

public class PathingResult : MonoBehaviour
{
    private List<TileInfo> Path { get; set; }

    public static PathingResult Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SetPath(List<TileInfo> path)
        => this.Path = path;

    public List<TileInfo> GetPath()
        => this.Path;

    public void ClearPath()
        => this.Path.Clear();
}