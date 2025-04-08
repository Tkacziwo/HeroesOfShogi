using UnityEngine;
using UnityEngine.UIElements;

public class Piece : MonoBehaviour
{
    private string PieceName;

    private int[] Moveset;

    private int posX;

    private int posY;

    private GameObject emptyGameObject;

    [SerializeField] private float speed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        emptyGameObject = new GameObject();
        emptyGameObject.transform.position = transform.position;
    }

    public void InitializePiece(string name, int[] moveset, int x, int y)
    {
        PieceName = name;
        Moveset = moveset;
        posX = x;
        posY = y;
    }

    public void MovePiece(int x, int y)
    {
        posX = x;
        posY = y;
    }

    public void MovePiece(Vector2Int vec)
    {
        posX = vec.x;
        posY = vec.y;
    }

    public int[] GetMoveset()
    {
        return Moveset;
    }

    public Vector2Int GetPosition()
    {
        return new Vector2Int(posX, posY);
    }

    public void SetTargetPosition(Vector3 target)
    {
        emptyGameObject.transform.position = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, emptyGameObject.transform.position) > 0.001f)
        {
            var step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, emptyGameObject.transform.position, step);
        }
    }

}
