using System;
using UnityEngine;

[Serializable]
public enum GridType
{
    Rectangle,
    Isometric,
    HexagonPointy,
    HexagonFlat
}

[CreateAssetMenu(fileName = "Grid Config", menuName = "Grid/Config")]
public class ScriptableGridConfig : ScriptableObject
{
    public GridType Type;
    [Space(10)]
    public GridLayout.CellLayout Layout;
    public GridTileBase GrassPrefab, ForestPrefab;
    public Vector3 CellSize;
    public GridLayout.CellSwizzle GridSwizzle;
}
