using UnityEngine;

public class Tile : MonoBehaviour
{
    public int row;
    public int column;
    public bool isMoving = false;

    private Vector2 targetPosition;
    private TileGenerator tileGenerator;
    private Vector3 touchStartPosition;
    private bool isDragging = false;

    private const float swapSpeed = 10.0f;

    private void Start()
    {
        tileGenerator = FindObjectOfType<TileGenerator>();
        targetPosition = transform.position;
    }

    private void Update()
    {
        // If the tile is moving, we update its position
        if (isMoving)
        {
            transform.position = Vector2.Lerp(transform.position, targetPosition, swapSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                tileGenerator.TileMoved();
            }
        }
    }

    private void OnMouseDown()
    {

        touchStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    private void OnMouseUp()
    {
        if (!isDragging)
            return;

        // Get touch position when tile is released
        Vector3 touchEndPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float deltaX = touchEndPosition.x - touchStartPosition.x;
        float deltaY = touchEndPosition.y - touchStartPosition.y;

        // Check if the movement is mainly horizontal or vertical
        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))
        {
            if (Mathf.Abs(deltaX) >= 1.0f)
            {
                int targetColumn = deltaX > 0 ? column + 1 : column - 1;
                if (targetColumn >= 0 && targetColumn < tileGenerator.columns)
                {
                    tileGenerator.MoveTile(row, column, row, targetColumn);
                }
            }
        }
        else
        {
            if (Mathf.Abs(deltaY) >= 1.0f)
            {
                int targetRow = deltaY > 0 ? row + 1 : row - 1;
                if (targetRow >= 0 && targetRow < tileGenerator.rows)
                {
                    tileGenerator.MoveTile(row, column, targetRow, column);
                }
            }
        }

        isDragging = false;
    }

    public void MoveToNewPosition(Vector2 newPosition)
    {
        targetPosition = newPosition;
        isMoving = true;
    }

    public Color GetColor()
    {
        return this.GetComponent<SpriteRenderer>().color;
    }
}
