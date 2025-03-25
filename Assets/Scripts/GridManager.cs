using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;

    [SerializeField] private Tile tile;

    [SerializeField] private Transform cameraPosition;

    public void Start()
    {
        GenerateField();
    }

    public void GenerateField()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Instantiate(tile, new Vector4(i, 0, j), Quaternion.identity);
            }
        }

        float xRot = 90;

        cameraPosition.position = new Vector4((float)width / 2, 1, (float)height / 2);
        cameraPosition.rotation = Quaternion.Euler(xRot, 0, 0);
    }
}
