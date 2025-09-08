using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] Transform _area;
    [SerializeField] int width, height;
    [SerializeField] float cellSize;
    [SerializeField] Tile _tilePrefab;

    public Dictionary<Vector2Int, Tile> _tiles;
    public Dictionary<Vector2Int, bool[]> visits;

    int xOrigin, zOrigin;

  
    public void GenerateGrid()
    {
        visits = new Dictionary<Vector2Int, bool[]>();
        _tiles = new Dictionary<Vector2Int, Tile>();
        
        xOrigin = ((int)_area.localPosition.x);
        zOrigin = ((int)_area.localPosition.z);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                visits.Add(new Vector2Int(x, z), new bool[4]);
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(xOrigin - x, 0, zOrigin - z) * cellSize, Quaternion.identity);
                //spawnedTile.name = $"Tile {x} {z}";
                spawnedTile.name = "Tile";

                var isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                spawnedTile.Init(isOffset);

                _tiles.Add(new Vector2Int(x, z), spawnedTile);
 
              //  _tiles[new Vector3(x, 0, z)] = spawnedTile;
            }
        }
    }

    public void Visit(Vector3 agentPosition, int index)
    {
        var xPos = Mathf.FloorToInt(xOrigin - agentPosition.x / cellSize);
        var zPos = Mathf.FloorToInt(zOrigin - agentPosition.z / cellSize);
        var myPos = new Vector2Int(xPos, zPos);
        visits[myPos][index] = true;
        //_tiles[myPos].ActivateColor(index);
    
    }

    public bool HasVisited(Vector3 agentPosition, int index)
    {
        var xPos = Mathf.FloorToInt(xOrigin - agentPosition.x / cellSize);
        var zPos = Mathf.FloorToInt(zOrigin - agentPosition.z / cellSize);
        visits.TryGetValue(new Vector2Int(xPos, zPos), out bool[] _visits);
        if (_visits != null && _visits.Length > 0)
        {
            return _visits[index];

        }
        else
        {
            return false;
        }
    }
}
