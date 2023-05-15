using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TriPos {
    public static float SQRT3 = Mathf.Sqrt(3);

    public GridPos hexPos;
    public bool right;

    public TriPos(GridPos hexPos, bool right) {
        this.hexPos = hexPos;
        this.right = right;
    }

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

    public Vector3 World {
        get => hexPos.World + new Vector3(right ? 1f : -1f, 0, SQRT3/2);
    }
}