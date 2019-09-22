using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public enum TileDirection
{
    Up,
    Down,
    Left,
    Right
}

public struct TilePosition
{
    public TilePiece currentPiece;
    public Vector2Int coordinates;
    public Vector3 position;
}

public class TileBoard : MonoBehaviour
{
    public int gridWidth = 16;
    public int gridHeight = 12;
    public int numberOfUniquePieces;
    public int tilesOnBoard;
    public TilePiece newPiecePrefab;
    public TilePiece[] piecePrefabs;
    public TilePosition[,] tileGrid;
    
    public void GenerateGrid()
    {
        // This section of the code creates new dummy prefabs if they aren't already set up in the editor
        if (numberOfUniquePieces != piecePrefabs.Length)
        {
            TilePiece[] newPiecePrefabs = new TilePiece[numberOfUniquePieces];
            for (int a = 0; a < newPiecePrefabs.Length; a++)
            {
                if (a < piecePrefabs.Length && piecePrefabs[a] != null)
                {
                    newPiecePrefabs[a] = piecePrefabs[a];
                }
                else
                {
                    TilePiece newPiece = GameObject.Instantiate(newPiecePrefab);
                    newPiece.GetComponentInChildren<TextMeshPro>().text = "" + a;
                    newPiece.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0,1.0f), Random.Range(0,1.0f), Random.Range(0,1.0f));
                    newPiece.transform.SetParent(this.transform);
                    newPiece.gameObject.name = "Generated Prefab " + a;
                    newPiece.gameObject.SetActive(false);
                    newPiecePrefabs[a] = newPiece;
                }
            }
            piecePrefabs = newPiecePrefabs;
        }

        tileGrid = new TilePosition[gridWidth, gridHeight];
        Vector2 gridOffset = new Vector2((float)-gridWidth/2.0f + 0.5f,(float)-gridHeight/2.0f + 0.5f);
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                tileGrid[x,y].coordinates = new Vector2Int(x,y);
                tileGrid[x,y].position = new Vector3(x + gridOffset.x,0,y + gridOffset.y);
                tileGrid[x,y].currentPiece = null;
            }
        }
        // I'm not filling the whole grid.  I'm leaving the edge free and using it for connection calculations
        int totalTilesToAdd = (gridWidth - 2) * (gridHeight - 2);
        tilesOnBoard = totalTilesToAdd;
        // This section selects a number of random tiles.  They need to be added in pairs so the puzzle is solvable 

        // I want to add some random pairs to the grid so that it's more solvable
        int pairsToAdd = 20;
        for (int a = 0; a < pairsToAdd; a=a+2)
        {
            int addThisPiece = Random.Range(0,numberOfUniquePieces);
            int x = Random.Range(1,gridWidth - 3);
            int y = Random.Range(1,gridHeight - 3);
            while (tileGrid[x,y].currentPiece != null || 
                tileGrid[x,y+1].currentPiece != null || 
                tileGrid[x+1,y].currentPiece != null)
            {
                x = Random.Range(1,gridWidth - 3);
                y = Random.Range(1,gridHeight - 3);
            }
            PlacePieceOnGrid(x,y,addThisPiece);
            if (Random.Range(0,1) == 0)
                x = x + 1;
            else
                y = y + 1;
            PlacePieceOnGrid(x,y,addThisPiece);
        }
        totalTilesToAdd -= pairsToAdd;


        // Here I create a list of at least one pair of every tile type and the rest is random pairs
        List<int> piecesToAdd = new List<int>();
        int minimumNumberOfPiece = 2;
        for (int a = 0; a < numberOfUniquePieces; a++)
        {
            piecesToAdd.Add(a);
            piecesToAdd.Add(a);
        }
        for (int a = minimumNumberOfPiece * numberOfUniquePieces; a < totalTilesToAdd; a = a + 2)
        {
            int addThisPiece = Random.Range(0,numberOfUniquePieces);
            piecesToAdd.Add(addThisPiece);
            piecesToAdd.Add(addThisPiece);
        }

        // This section fills the rest of the grid randomly from the above
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (x != 0 && y != 0 && x != gridWidth - 1 && y != gridHeight - 1 && tileGrid[x, y].currentPiece == null)
                {
                    // selects a random piece from the dictionary and adds it to the grid
                    tileGrid[x, y].coordinates = new Vector2Int(x, y);
                    tileGrid[x, y].position = new Vector3(x + gridOffset.x, 0, y + gridOffset.y);
                    int randomIndex = Random.Range(0, piecesToAdd.Count);
                    int randomPiece = piecesToAdd[randomIndex];
                    piecesToAdd.RemoveAt(randomIndex);
                    PlacePieceOnGrid(x, y, randomPiece);
                }
            }
        }
    }

    private void PlacePieceOnGrid(int x, int y, int pieceIndex)
    {
        tileGrid[x,y].currentPiece = GameObject.Instantiate(piecePrefabs[pieceIndex]);
        tileGrid[x,y].currentPiece.transform.position = tileGrid[x,y].position;
        tileGrid[x,y].currentPiece.tilePosition = tileGrid[x,y];
        tileGrid[x,y].currentPiece.pieceValue = pieceIndex;
        tileGrid[x,y].currentPiece.transform.SetParent(this.transform);
        tileGrid[x,y].currentPiece.gameObject.SetActive(true);
    }

    public void ClearGrid()
    {
        if (tileGrid == null)
            return;
        for (int x = 0; x < tileGrid.GetLength(0); x++)
        {
            for (int y = 0; y < tileGrid.GetLength(1); y++)
            {
                if (tileGrid[x,y].currentPiece != null)
                    Destroy(tileGrid[x,y].currentPiece.gameObject);
            }
        }
    }

    public void RemoveTilePiece(TilePiece toRemove)
    {
        TilePosition impactedPosition = toRemove.tilePosition;
        impactedPosition.currentPiece = null;
        Destroy(toRemove.gameObject);
        tilesOnBoard--;
    }
    
    public bool CanPiecesConnect(TilePiece firstTile, TilePiece secondTile, out Vector3 [] pathway)
    {
        // The rules of the game are that the pieces need to be have a clear path between them containing 3 line segments or fewer.
        // That is what this function checks for.  It also needs to provide the pathway for the game to display properly.
        pathway = new Vector3[0];

        // First get some of the easier checks out of the way.
        if (firstTile == secondTile)
            return false;
        if (firstTile.pieceValue != secondTile.pieceValue)
            return false;

        Vector2Int startPosition = firstTile.tilePosition.coordinates;
        Vector2Int endPosition = secondTile.tilePosition.coordinates;
        Vector2Int[] pathwayCoords = new Vector2Int[0];
        bool foundAPath = false;

        // I'm trying to do the checks in the order from simplest to most involved.  
        // Using fewer line segments should be more efficient.
        if (Can1SegmentConnect(startPosition, endPosition, out pathwayCoords))
        {
            foundAPath = true;
        }
        if (!foundAPath && Can2SegmentsConnect(startPosition, endPosition, out pathwayCoords))
        {
            foundAPath = true;
        }
        if (!foundAPath && Can3SegmentsConnect(startPosition, endPosition, out pathwayCoords))
        {
            foundAPath = true;
        }
        if (foundAPath)
        {
            pathway = CoordinatesToVectors(pathwayCoords);
            return true;
        }
        return false;
    }
    
    private int GetClearanceFromPosition(Vector2Int startPosition, TileDirection direction, int maxClearance = 100)
    {
        // This function takes a startPosition and direction then returns how many empty tiles are in the direction from the start position.
        // It does not check the startPosition itself or ignore any specific tiles.
        // It will never return a value that would be outside of the array.
        Vector2Int modVector = Vector2Int.zero;
        switch (direction)
        {
            case TileDirection.Up:
                modVector = Vector2Int.up;
                break;
            case TileDirection.Down:
                modVector = Vector2Int.down;
                break;
            case TileDirection.Left:
                modVector = Vector2Int.left;
                break;
            case TileDirection.Right:
                modVector = Vector2Int.right;
                break;
        }
        int currentClearance = 0;
        for (int a = 1; a <= maxClearance; a++)
        {
            Vector2Int checkVector = modVector * a + startPosition;
            if (checkVector.x < 0 || checkVector.y < 0 || checkVector.x >= gridWidth || checkVector.y >= gridHeight)
                break;
            else if (tileGrid[checkVector.x,checkVector.y].currentPiece == null)
            {
                currentClearance = a;
            }
            else
                break;
        }
        return currentClearance;
    }

    private bool Can1SegmentConnect(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        // This function just checks if there's a free straight line between them.
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        bool foundAPath = false;
        if (xDiff == 0)
        {
            if (yDiff > 0 && GetClearanceFromPosition(startPosition, TileDirection.Up, yDist - 1) == yDist - 1)
                foundAPath = true;
            else if (yDiff < 0 && GetClearanceFromPosition(startPosition, TileDirection.Down, yDist - 1) == yDist - 1)
                foundAPath = true;
        }
        else if (yDiff == 0)
        {
            if (xDiff > 0 && GetClearanceFromPosition(startPosition, TileDirection.Right, xDist - 1) == xDist - 1)
                foundAPath = true;
            else if (xDiff < 0 && GetClearanceFromPosition(startPosition, TileDirection.Left, xDist - 1) == xDist - 1)
                foundAPath = true;
        }
        if (foundAPath)
        {
            pathway = new Vector2Int[2];
            pathway[0] = startPosition;
            pathway[1] = endPosition;
            return true;
        }
        return false;
    }

    private bool Can2SegmentsConnect(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        // This function checks for cases where there are 2 line segments connecting the 2 tiles.
        // There are only 2 options for possible pivots.  Either the pivot point shares an x coordinate with the first tile
        // and a y coordinate with the second or the pivot point shares a y coordinate with the first tile and an x coordinate with the second.
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        bool firstVertical = false;
        bool firstHorizontal = false;
        if (xDiff > 0 && yDiff > 0)// top right
        {
            if (GetClearanceFromPosition(startPosition, TileDirection.Right, xDist) == xDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Down, yDist) == yDist)
                firstHorizontal = true;
            else if (GetClearanceFromPosition(startPosition, TileDirection.Up, yDist) == yDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Left, xDist) == xDist)
                firstVertical = true;
        }
        else if (xDiff > 0 && yDiff < 0)// bottom right
        {
            if (GetClearanceFromPosition(startPosition, TileDirection.Right, xDist) == xDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Up, yDist) == yDist)
                firstHorizontal = true;
            else if (GetClearanceFromPosition(startPosition, TileDirection.Down, yDist) == yDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Left, xDist) == xDist)
                firstVertical = true;
        }
        else if (xDiff < 0 && yDiff > 0)// top left
        {
            if (GetClearanceFromPosition(startPosition, TileDirection.Left, xDist) == xDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Down, yDist) == yDist)
                firstHorizontal = true;
            else if (GetClearanceFromPosition(startPosition, TileDirection.Up, yDist) == yDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Right, xDist) == xDist)
                firstVertical = true;
        }
        else if (xDiff < 0 && yDiff < 0)// bottom left
        {
            if (GetClearanceFromPosition(startPosition, TileDirection.Left, xDist) == xDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Up, yDist) == yDist)
                firstHorizontal = true;
            else if (GetClearanceFromPosition(startPosition, TileDirection.Down, yDist) == yDist &&
                GetClearanceFromPosition(endPosition, TileDirection.Right, xDist) == xDist)
                firstVertical = true;
        }
        if (firstVertical)
        {
            pathway = new Vector2Int[3];
            pathway[0] = startPosition;
            pathway[1] = new Vector2Int(startPosition.x, endPosition.y);
            pathway[2] = endPosition;
            return true;
        }
        else if (firstHorizontal)
        {
            pathway = new Vector2Int[3];
            pathway[0] = startPosition;
            pathway[1] = new Vector2Int(endPosition.x, startPosition.y);
            pathway[2] = endPosition;
            return true;
        }
        return false;
    }

    private bool Can3SegmentsConnect(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        // Because the two tiles are connected by at most 3 line segments this means that something is always true about the path that connects them;
        // either there's a vertical straight line segment with each end sharing an x-coordinate with a different one of the tiles
        // or there's a horizontal straight line segment with each end sharing a y-coordinate with one of the tiles.
        // I decided to break up the different ways of checking for 3 segments into most likely to least likely to solve the problem
        if (CheckInner3Segments(startPosition, endPosition, out pathway))
        {
            return true;
        }
        TileDirection[] outerOrder = GetOuter3SegmentSearchOrder(startPosition, endPosition);
        for (int a = 0; a < outerOrder.Length; a++)
        {
            switch (outerOrder[a])
            {
                case TileDirection.Up:
                    if (CheckUpward3Segments(startPosition, endPosition, out pathway))
                    {
                        return true;
                    }
                    break;
                case TileDirection.Down:
                    if (CheckDownward3Segments(startPosition, endPosition, out pathway))
                    {
                        return true;
                    }
                    break;
                case TileDirection.Left:
                    if (CheckLeft3Segments(startPosition, endPosition, out pathway))
                    {
                        return true;
                    }
                    break;
                case TileDirection.Right:
                    if (CheckRight3Segments(startPosition, endPosition, out pathway))
                    {
                        return true;
                    }
                    break;
            }
        }
        return false;
    }

    private bool CheckInner3Segments(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        bool foundAPath = false;

        // First I will check the horizontal line segments that are between two pieces
        int lowY;
        int highY;
        if (yDiff > 0)
        {
            lowY = endPosition.y - GetClearanceFromPosition(endPosition, TileDirection.Down, yDist - 1);
            highY = startPosition.y + GetClearanceFromPosition(startPosition, TileDirection.Up, yDist - 1);
        }
        else
        {
            lowY = startPosition.y - GetClearanceFromPosition(startPosition, TileDirection.Down, yDist - 1);
            highY = endPosition.y + GetClearanceFromPosition(endPosition, TileDirection.Up, yDist - 1);
        }
        if (lowY <= highY)
        {
            for (int y = lowY; y <= highY; y++)
            {
                Vector2Int firstPivot = new Vector2Int(startPosition.x, y);
                if (xDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Right, xDist - 1) == xDist - 1)
                    foundAPath = true;
                else if (xDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Left, xDist - 1) == xDist - 1)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(endPosition.x, firstPivot.y);
                    pathway[3] = endPosition;
                    return true;
                }
            }
        }

        // Next I will check the vertical line segments that are between the two pieces
        int leftX;
        int rightX;
        if (xDiff > 0)
        {
            rightX = startPosition.x + GetClearanceFromPosition(startPosition, TileDirection.Right, xDist-1);
            leftX = endPosition.x - GetClearanceFromPosition(endPosition, TileDirection.Left, xDist-1);
        }
        else
        {
            rightX = endPosition.x + GetClearanceFromPosition(endPosition, TileDirection.Right, xDist-1);
            leftX = startPosition.x - GetClearanceFromPosition(startPosition, TileDirection.Left, xDist-1);
        }
        if (leftX <= rightX)
        {
            for (int x = leftX; x <= rightX; x++)
            {
                Vector2Int firstPivot = new Vector2Int(x, startPosition.y);
                if (yDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Up, yDist - 1) == yDist - 1)
                    foundAPath = true;
                else if (yDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Down, yDist - 1) == yDist - 1)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(firstPivot.x, endPosition.y);
                    pathway[3] = endPosition;
                    return true;
                }
            }
        }
        return false;
    }
  
    private bool CheckUpward3Segments(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        int highY;
        if (yDiff > 0)
            highY = endPosition.y;
        else
            highY = startPosition.y;
        int startClearance = GetClearanceFromPosition(startPosition, TileDirection.Up);
        int endClearance = GetClearanceFromPosition(endPosition, TileDirection.Up);
        int maxClearance = Mathf.Min(startClearance, endClearance);
        if (startPosition.y + startClearance > highY && endPosition.y + endClearance > highY)
        {
            for (int y = highY + 1; y <= highY + maxClearance; y++)
            {
                Vector2Int firstPivot = new Vector2Int(startPosition.x, y);
                bool foundAPath = false;
                if (xDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Right, xDist) == xDist)
                    foundAPath = true;
                else if (xDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Left, xDist) == xDist)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(endPosition.x, firstPivot.y);
                    pathway[3] = endPosition;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckDownward3Segments(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        int lowY;
        if (yDiff > 0)
            lowY = startPosition.y;
        else
            lowY = endPosition.y;
        int startClearance = GetClearanceFromPosition(startPosition, TileDirection.Down);
        int endClearance = GetClearanceFromPosition(endPosition, TileDirection.Down);
        int maxClearance = Mathf.Min(startClearance, endClearance);
        if (startPosition.y - startClearance < lowY && endPosition.y - endClearance < lowY)
        {
            for (int y = lowY - 1; y >= lowY-maxClearance; y--)
            {
                Vector2Int firstPivot = new Vector2Int(startPosition.x, y);
                bool foundAPath = false;
                if (xDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Right, xDist) == xDist)
                    foundAPath = true;
                else if (xDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Left, xDist) == xDist)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(endPosition.x, firstPivot.y);
                    pathway[3] = endPosition;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckLeft3Segments(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        int leftX;
        if (xDiff > 0)
            leftX = startPosition.x;
        else
            leftX = endPosition.x;
        int endClearance = GetClearanceFromPosition(endPosition, TileDirection.Left);
        int startClearance = GetClearanceFromPosition(startPosition, TileDirection.Left);
        int maxClearance = Mathf.Min(startClearance, endClearance);
        if (startPosition.x - startClearance < leftX && endPosition.x - endClearance < leftX)
        {
            for (int x = leftX - 1; x >= leftX-maxClearance; x--)
            {
                Vector2Int firstPivot = new Vector2Int(x, startPosition.y);
                bool foundAPath = false;
                if (yDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Up, yDist) == yDist)
                    foundAPath = true;
                else if (yDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Down, yDist) == yDist)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(firstPivot.x, endPosition.y);
                    pathway[3] = endPosition;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckRight3Segments(Vector2Int startPosition, Vector2Int endPosition, out Vector2Int[] pathway)
    {
        pathway = new Vector2Int[0];
        int xDiff = endPosition.x - startPosition.x;
        int yDiff = endPosition.y - startPosition.y;
        int xDist = Mathf.Abs(xDiff);
        int yDist = Mathf.Abs(yDiff);
        int rightX;
        if (xDiff > 0)
            rightX = endPosition.x;
        else
            rightX = startPosition.x;
        int endClearance = GetClearanceFromPosition(endPosition, TileDirection.Right);
        int startClearance = GetClearanceFromPosition(startPosition, TileDirection.Right);
        int maxClearance = Mathf.Min(startClearance, endClearance);
        if (startPosition.x + startClearance > rightX && endPosition.x + endClearance > rightX)
        {
            for (int x = rightX + 1; x <= rightX + maxClearance; x++)
            {
                Vector2Int firstPivot = new Vector2Int(x, startPosition.y);
                bool foundAPath = false;
                if (yDiff > 0 && GetClearanceFromPosition(firstPivot, TileDirection.Up, yDist) == yDist)
                    foundAPath = true;
                else if (yDiff < 0 && GetClearanceFromPosition(firstPivot, TileDirection.Down, yDist) == yDist)
                    foundAPath = true;
                if (foundAPath)
                {
                    pathway = new Vector2Int[4];
                    pathway[0] = startPosition;
                    pathway[1] = firstPivot;
                    pathway[2] = new Vector2Int(firstPivot.x, endPosition.y);
                    pathway[3] = endPosition;
                    return true;
                }
                continue;
            }
        }
        return false;
    }
    
    private TileDirection[] GetOuter3SegmentSearchOrder(Vector2Int startPosition, Vector2Int endPosition)
    {
        // This function takes the average position between the two tiles returns a search order based on which edge it's closest too
        TileDirection[] outerOrder = new TileDirection[4];
        float averageX = (startPosition.x + endPosition.x)/2.0f;
        float middleX = gridWidth/2.0f - 0.5f;
        float averageY = (startPosition.y + endPosition.y)/2.0f;
        float middleY = gridHeight/2.0f - 0.5f;
        if (averageX > middleX)//right
        {
            if (averageY > middleY)//top
            {
                if (gridHeight - 1 - averageY < gridWidth - 1 - averageX)//closer to top
                {
                    outerOrder[0] = TileDirection.Up;
                    outerOrder[1] = TileDirection.Right;
                    outerOrder[2] = TileDirection.Left;
                    outerOrder[3] = TileDirection.Down;
                }
                else//closer to right
                {
                    outerOrder[0] = TileDirection.Right;
                    outerOrder[1] = TileDirection.Up;
                    outerOrder[2] = TileDirection.Down;
                    outerOrder[3] = TileDirection.Left;
                }
            }
            else//bottom
            {
                if (averageY < gridWidth - 1 - averageX)//closer to bottom
                {
                    outerOrder[0] = TileDirection.Down;
                    outerOrder[1] = TileDirection.Right;
                    outerOrder[2] = TileDirection.Left;
                    outerOrder[3] = TileDirection.Up;
                }
                else//closer to right
                {
                    outerOrder[0] = TileDirection.Right;
                    outerOrder[1] = TileDirection.Up;
                    outerOrder[2] = TileDirection.Down;
                    outerOrder[3] = TileDirection.Left;
                }
            }
        }
        else//left
        {
            if (averageY > middleY)//top
            {
                if (gridHeight - 1 - averageY < averageX)//closer to top
                {
                    outerOrder[0] = TileDirection.Up;
                    outerOrder[1] = TileDirection.Left;
                    outerOrder[2] = TileDirection.Right;
                    outerOrder[3] = TileDirection.Down;
                }
                else//closer to left
                {
                    outerOrder[0] = TileDirection.Left;
                    outerOrder[1] = TileDirection.Up;
                    outerOrder[2] = TileDirection.Down;
                    outerOrder[3] = TileDirection.Right;
                }
            }
            else//bottom
            {
                if (averageY < averageX)//closer to bottom
                {
                    outerOrder[0] = TileDirection.Down;
                    outerOrder[1] = TileDirection.Left;
                    outerOrder[2] = TileDirection.Right;
                    outerOrder[3] = TileDirection.Up;
                }
                else// closer to left
                {
                    outerOrder[0] = TileDirection.Left;
                    outerOrder[1] = TileDirection.Up;
                    outerOrder[2] = TileDirection.Down;
                    outerOrder[3] = TileDirection.Right;
                }
            }
        }
        return outerOrder;
    }

    private Vector3 [] CoordinatesToVectors(Vector2Int[] inputPath)
    {
        Vector3[] returnVect = new Vector3[inputPath.Length];
        for (int a = 0; a < inputPath.Length; a++)
        {
            returnVect[a] = tileGrid[inputPath[a].x, inputPath[a].y].position;
        }
        return returnVect;
    }
}
