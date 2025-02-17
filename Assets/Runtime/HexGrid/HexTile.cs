using UnityEngine;

public class HexTile : GridTileBase
{
    public override void Init(Vector3Int coordinate)
    {
        name = $"Hex {coordinate.ToString()}";
    }
}
