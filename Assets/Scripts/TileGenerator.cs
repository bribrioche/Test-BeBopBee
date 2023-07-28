using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileGenerator : MonoBehaviour
{
    public int rows = 5;
    public int columns = 5;
    public int totalColors = 4;
    public float tileSize = 1.0f;
    public GameObject tilePrefab;
    private GameObject[,] tileGrid;
    public float startX = 0;
    public float startY = 0;

    public List<GameObject> tilePool = new List<GameObject>();
    private List<Color> colors = new List<Color>();

    private Tile selectedTile;


    private bool isSwapping = false;
    private List<Tile> tilesToSwap = new List<Tile>();
    private bool gameStart = false;

    public int swapCount;
    public float swapDelay = 0.5f;

    public InputField inputFieldSwaps;



    void Start()
    {
        tileGrid = new GameObject[rows, columns];
        GenerateColors();
        GenerateTiles();

    }


    private void Update()
    {
        if (!isSwapping & gameStart)
        {
            CheckMatchesAndCollapse();
        }
    }

    private void GenerateTiles()
    {
        startX = -(columns * tileSize) / 2 + tileSize / 2;
        startY = -(rows * tileSize) / 2 + tileSize / 2;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject newTile = GetRandomTile();
                newTile.transform.position = new Vector2(startX + col * tileSize, startY + row * tileSize);
                newTile.GetComponent<Tile>().row = row;
                newTile.GetComponent<Tile>().column = col;
                newTile.SetActive(true);

                // Check the sequence of three tiles of the same color in horizontal
                while (col >= 2 && newTile.GetComponent<SpriteRenderer>().color ==
                       tileGrid[row, col - 1].GetComponent<SpriteRenderer>().color &&
                       newTile.GetComponent<SpriteRenderer>().color ==
                       tileGrid[row, col - 2].GetComponent<SpriteRenderer>().color)
                {
                    // Change the color of the tile until it is different from the previous two
                    SpriteRenderer spriteRenderer = newTile.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = GetColor();
                }

                // Check the sequence of three tiles of the same color in vertical
                while (row >= 2 && newTile.GetComponent<SpriteRenderer>().color ==
                       tileGrid[row - 1, col].GetComponent<SpriteRenderer>().color &&
                       newTile.GetComponent<SpriteRenderer>().color ==
                       tileGrid[row - 2, col].GetComponent<SpriteRenderer>().color)
                {
                    // Change the color of the tile until it is different from the previous two
                    SpriteRenderer spriteRenderer = newTile.GetComponent<SpriteRenderer>();
                    spriteRenderer.color = GetColor();
                }

                tileGrid[row, col] = newTile;
            }
        }
        gameStart = true;
    }

    private GameObject GetRandomTile()
    {
        if (tilePool.Count == 0)
            FillTilePool();

        int randomIndex = Random.Range(0, tilePool.Count);
        GameObject randomTile = tilePool[randomIndex];
        tilePool.RemoveAt(randomIndex);

        SpriteRenderer spriteRenderer = randomTile.GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetColor();

        return randomTile;
    }

    private void FillTilePool()
    {
        for (int i = 0; i < totalColors; i++)
        {
            for (int j = 0; j < rows * columns / totalColors; j++)
            {
                GameObject newTile = Instantiate(tilePrefab, transform);
                newTile.SetActive(false);
                tilePool.Add(newTile);
            }
        }
    }

    private void GenerateColors()
    {
        for (int i = 1; i <= totalColors; i++)
        {
            Color newColor = new Color(Random.value, Random.value, Random.value);
            colors.Add(newColor);
        }
    }

    private Color GetColor()
    {
        return colors[Random.Range(0, totalColors - 1)];
    }

    public void MoveTile(int currentRow, int currentColumn, int newRow, int newColumn)
    {
        if (isSwapping)
            return;

        Tile currentTile = tileGrid[currentRow, currentColumn].GetComponent<Tile>();
        Tile newTile = tileGrid[newRow, newColumn].GetComponent<Tile>();

        if (currentTile == null || newTile == null)
            return;

        // Swap coordinates of tiles in the grid
        tileGrid[currentRow, currentColumn] = newTile.gameObject;
        tileGrid[newRow, newColumn] = currentTile.gameObject;

        currentTile.row = newRow;
        currentTile.column = newColumn;
        newTile.row = currentRow;
        newTile.column = currentColumn;

        // Move tiles
        currentTile.MoveToNewPosition(GetTilePosition(newRow, newColumn));
        newTile.MoveToNewPosition(GetTilePosition(currentRow, currentColumn));

        isSwapping = true;
    }

    private Vector3 GetTilePosition(int row, int col)
    {
        float x = startX + col * tileSize;
        float y = startY + row * tileSize;
        return new Vector3(x, y, 0f);
    }

    public void TileMoved()
    {
        foreach (Tile tile in tilesToSwap)
        {
            if (tile.isMoving)
                return;
        }

        // Swap tiles in the grid
        if (tilesToSwap.Count == 2)
        {
            Tile tile1 = tilesToSwap[0];
            Tile tile2 = tilesToSwap[1];

            int row1 = tile1.row;
            int col1 = tile1.column;
            int row2 = tile2.row;
            int col2 = tile2.column;

            tileGrid[row1, col1] = tile2.gameObject;
            tileGrid[row2, col2] = tile1.gameObject;

            tile1.row = row2;
            tile1.column = col2;
            tile2.row = row1;
            tile2.column = col1;

            tile1.isMoving = false;
            tile2.isMoving = false;
        }

        tilesToSwap.Clear();
        isSwapping = false;
    }

    private void CheckMatchesAndCollapse()
    {
        StartCoroutine(CheckMatchesAndCollapseWithDelay());
    }

    private IEnumerator CheckMatchesAndCollapseWithDelay()
    {
        List<Tile> matchedTiles = new List<Tile>();

        // Check horizontal matches
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++)
            {
                Tile currentTile = tileGrid[row, col]?.GetComponent<Tile>();
                Tile tile1 = tileGrid[row, col + 1]?.GetComponent<Tile>();
                Tile tile2 = tileGrid[row, col + 2]?.GetComponent<Tile>();

                if (currentTile != null && tile1 != null && tile2 != null && currentTile.GetColor() == tile1.GetColor() && currentTile.GetColor() == tile2.GetColor())
                {
                    if (!matchedTiles.Contains(currentTile))
                        matchedTiles.Add(currentTile);
                    if (!matchedTiles.Contains(tile1))
                        matchedTiles.Add(tile1);
                    if (!matchedTiles.Contains(tile2))
                        matchedTiles.Add(tile2);
                }
            }
        }

        // Check vertical matches
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++)
            {
                Tile currentTile = tileGrid[row, col]?.GetComponent<Tile>();
                Tile tile1 = tileGrid[row + 1, col]?.GetComponent<Tile>();
                Tile tile2 = tileGrid[row + 2, col]?.GetComponent<Tile>();

                if (currentTile != null && tile1 != null && tile2 != null && currentTile.GetColor() == tile1.GetColor() && currentTile.GetColor() == tile2.GetColor())
                {
                    if (!matchedTiles.Contains(currentTile))
                        matchedTiles.Add(currentTile);
                    if (!matchedTiles.Contains(tile1))
                        matchedTiles.Add(tile1);
                    if (!matchedTiles.Contains(tile2))
                        matchedTiles.Add(tile2);
                }
            }
        }

        // Remove the matching tiles and knock down the top ones
        if (matchedTiles.Count >= 3)
        {
            foreach (Tile tile in matchedTiles)
            {
                tilePool.Add(tile.gameObject);
                tileGrid[tile.row, tile.column] = null;
                tile.gameObject.SetActive(false);

                for (int row = tile.row + 1; row < rows; row++)
                {
                    Tile tileAbove = tileGrid[row, tile.column]?.GetComponent<Tile>();
                    if (tileAbove != null)
                    {
                        tileAbove.row--;
                        tileAbove.MoveToNewPosition(GetTilePosition(tileAbove.row, tileAbove.column));
                        tileGrid[row - 1, tile.column] = tileAbove.gameObject;
                        tileGrid[row, tile.column] = null;
                    }
                }
            }

            // Delay to be sure the tiles have fallen
            yield return new WaitForSeconds(0.2f);

            FillEmptySpaces();
        }
    }

    private void FillEmptySpaces()
    {
        for (int col = 0; col < columns; col++)
        {
            int emptySpaces = 0;
            for (int row = 0; row < rows; row++)
            {
                if (tileGrid[row, col] == null)
                {
                    emptySpaces++;
                }
                else if (emptySpaces > 0)
                {
                    Tile tile = tileGrid[row, col].GetComponent<Tile>();
                    tile.row -= emptySpaces;
                    tile.MoveToNewPosition(GetTilePosition(tile.row, tile.column));
                    tileGrid[row - emptySpaces, col] = tile.gameObject;
                    tileGrid[row, col] = null;
                }
            }

            // Generate new tiles to fill the empty squares in the column
            for (int i = 0; i < emptySpaces; i++)
            {
                int newRow = rows - i - 1;
                int newCol = col;

                GameObject newTile = Instantiate(tilePrefab, transform);
                newTile.name = "NewTile";
                newTile.transform.position = GetTilePosition(newRow, newCol);
                newTile.GetComponent<Tile>().row = newRow;
                newTile.GetComponent<Tile>().column = newCol;

                SpriteRenderer spriteRenderer = newTile.GetComponent<SpriteRenderer>();
                spriteRenderer.color = GetColor();

                newTile.SetActive(true);

                tileGrid[newRow, newCol] = newTile;
            }
        }
    }
    public void LaunchSimulation()
    {
        swapCount = int.Parse(inputFieldSwaps.text);
        StartCoroutine(SimulateSwaps());
    }


    private IEnumerator SimulateSwaps()
    {
        isSwapping = true;

        for (int i = 0; i < swapCount; i++)
        {
            // Random choice of a position in the grid
            int row = Random.Range(0, rows);
            int col = Random.Range(0, columns);

            // Check if the position is valid and the tile exists
            if (IsValidPosition(row, col) && tileGrid[row, col] != null)
            {
                // Check valid neighbor positions
                List<Vector2Int> validNeighborPositions = new List<Vector2Int>();

                if (IsValidPosition(row - 1, col)) // Top
                    validNeighborPositions.Add(new Vector2Int(row - 1, col));

                if (IsValidPosition(row + 1, col)) // Bottom
                    validNeighborPositions.Add(new Vector2Int(row + 1, col));

                if (IsValidPosition(row, col - 1)) // Left
                    validNeighborPositions.Add(new Vector2Int(row, col - 1));

                if (IsValidPosition(row, col + 1)) // Right
                    validNeighborPositions.Add(new Vector2Int(row, col + 1));

                if (validNeighborPositions.Count > 0)
                {
                    // Choose a neighbor
                    int randomIndex = Random.Range(0, validNeighborPositions.Count);
                    Vector2Int neighborPos = validNeighborPositions[randomIndex];

                    // Swap
                    SwapTiles(row, col, neighborPos.x, neighborPos.y);

                    // Delay befor next swap
                    yield return new WaitForSeconds(swapDelay);
                }
            }
        }

        isSwapping = false;
    }

    private void SwapTiles(int row1, int col1, int row2, int col2)
    {
        // Check that the positions are valid and that the tiles exist
        if (IsValidPosition(row1, col1) && IsValidPosition(row2, col2) && tileGrid[row1, col1] != null && tileGrid[row2, col2] != null)
        {
            // Swap tiles
            Tile tile1 = tileGrid[row1, col1].GetComponent<Tile>();
            Tile tile2 = tileGrid[row2, col2].GetComponent<Tile>();

            int tempRow = tile1.row;
            int tempCol = tile1.column;
            tile1.row = tile2.row;
            tile1.column = tile2.column;
            tile2.row = tempRow;
            tile2.column = tempCol;

            tileGrid[row1, col1] = tile2.gameObject;
            tileGrid[row2, col2] = tile1.gameObject;

            tile1.MoveToNewPosition(GetTilePosition(tile1.row, tile1.column));
            tile2.MoveToNewPosition(GetTilePosition(tile2.row, tile2.column));
        }
    }

    // Check if a position is valid in the grid
    private bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < rows && col >= 0 && col < columns;
    }
}


