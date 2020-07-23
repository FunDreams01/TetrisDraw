using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceConversionUtility
{
    public static Vector3 BottomLeftSpawnPos, LeftDir,UpDir, CanvasScale;
    public static Rect TetrisScreenBounds;
    public static int ScreenWidthInBlocks, ScreenHeightInBlocks;
    public static float BlockWidth;

    public static Rect RectTransformToScreenSpace(RectTransform rtt)
    {
        Vector2 size = Vector2.Scale(rtt.rect.size, rtt.lossyScale);
        return new Rect((Vector2)rtt.position - (size * 0.5f), size);
    }

    public static Vector3 GridSpaceToWorldSpace(Vector2 Coords)
    {
        Vector3 pos = BottomLeftSpawnPos;
        pos += LeftDir * Coords.x;
        pos += UpDir * Coords.y;
        return pos;
    }

    public static bool ScreenSpaceToRectSpace(Vector2 pos, out Vector2 localpos)
    {
        localpos = new Vector2(pos.x - TetrisScreenBounds.xMin, -(pos.y - TetrisScreenBounds.yMax));
        localpos /= CanvasScale;
        return TetrisScreenBounds.Contains(pos);
    }

    public static Vector3 SnapRectPosToGridRectPos(Vector3 pos, out int Coordx, int CoordxMin = int.MinValue, int CoordxMax = int.MaxValue)
    {
        float stepsize = (TetrisScreenBounds.width / CanvasScale.x) / ScreenWidthInBlocks;
        Coordx = Mathf.RoundToInt(pos.x / stepsize);
        Coordx = Mathf.Clamp(Coordx,CoordxMin,CoordxMax);
        pos.x = stepsize * Coordx;
        return pos;
    }

}
