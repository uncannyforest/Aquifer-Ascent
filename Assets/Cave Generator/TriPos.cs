using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TriPos {
    public static float SQRT3 = Mathf.Sqrt(3);

    public GridPos hexPos;
    public bool right;

    public int w { get => hexPos.w; }

    public TriPos(GridPos hexPos, bool right) {
        this.hexPos = hexPos;
        this.right = right;
    }

    public static TriPos operator +(TriPos a, GridPos b) => new TriPos(a.hexPos + b, a.right);
    public static TriPos operator -(TriPos a, GridPos b) => new TriPos(a.hexPos - b, a.right);
    override public string ToString() => "(" + hexPos.w + ", " + hexPos.x + ", " + hexPos.y + ")." + (right ? "r" : "l");

    public GridPos[] HorizCorners {
        get => right ? new GridPos[] {
            hexPos,
            hexPos + GridPos.E,
            hexPos + GridPos.W
        } : new GridPos[] {
            hexPos + GridPos.W,
            hexPos + GridPos.Q,
            hexPos
        };
    }

    public static Vector3[][] AllCornersRelative = new Vector3[][] {
        new Vector3[] {
            Vector3.Scale(CaveGrid.Scale, new Vector3(-.5f, 0, -SQRT3/2)),
            Vector3.Scale(CaveGrid.Scale, new Vector3(1f, 0, 0)),
            Vector3.Scale(CaveGrid.Scale, new Vector3(-.5f, 0, SQRT3/2)),
        },
        new Vector3[] {
            Vector3.Scale(CaveGrid.Scale, new Vector3(.5f, 0, SQRT3/2)),
            Vector3.Scale(CaveGrid.Scale, new Vector3(-1f, 0, 0)),
            Vector3.Scale(CaveGrid.Scale, new Vector3(.5f, 0, -SQRT3/2)),
        },
    };

    public Vector3[] CornersRelative { get => AllCornersRelative[right ? 0 : 1]; }

    public Vector3 World {
        get => hexPos.World - CornersRelative[right ? 0 : 2];
    }

    public TriPos GetAdjacent(GridPos direction) {
        GridPos h = direction.Horizontal;
        int v = direction.w;
        if (this.right) {
            if (h == GridPos.S || h == GridPos.D)
                return new TriPos(hexPos + GridPos.D + GridPos.up * v, false);
            if (h == GridPos.E || h == GridPos.W)
                return new TriPos(hexPos + GridPos.E + GridPos.up * v, false);
            return new TriPos(hexPos + GridPos.up * v, false);
        } else {
            if (h == GridPos.W || h == GridPos.Q)
                return new TriPos(hexPos + GridPos.Q + GridPos.up * v, true);
            if (h == GridPos.A || h == GridPos.S)
                return new TriPos(hexPos + GridPos.A + GridPos.up * v, true);
            return new TriPos(hexPos + GridPos.up * v, true);
        }
    }
}