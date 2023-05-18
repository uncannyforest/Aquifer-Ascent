using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GridPos  {
    public static float SQRT3 = Mathf.Sqrt(3);

    public GridPos(int w, int x, int y) {
        this.w = w;
        this.x = x;
        this.y = y;
    }

    public int w; // elevation
    public int x;
    public int y;
    public int z { get => -x - y; }

    public static GridPos zero => new GridPos(0, 0, 0);
    public static GridPos up => new GridPos(1, 0, 0);
    public static GridPos E => new GridPos(0, 1, 0);
    public static GridPos W => new GridPos(0, 0, 1);
    public static GridPos Q => new GridPos(0, -1, 1);
    public static GridPos A => new GridPos(0, -1, 0);
    public static GridPos S => new GridPos(0, 0, -1);
    public static GridPos D => new GridPos(0, 1, -1);

    public static GridPos operator -(GridPos a) => new GridPos(-a.w, -a.x, -a.y);
    public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.w + b.w, a.x + b.x, a.y + b.y);
    public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.w - b.w, a.x - b.x, a.y - b.y);
    public static GridPos operator *(GridPos a, int n) => new GridPos(a.w * n, a.x * n, a.y * n);
    public static GridPos operator *(int n, GridPos a) => new GridPos(a.w * n, a.x * n, a.y * n);

    public static bool operator ==(GridPos a, GridPos b) => a.w == b.w && a.x == b.x && a.y == b.y;
    public static bool operator !=(GridPos a, GridPos b) => a.w != b.w && a.x != b.x || a.y != b.y;
    public override bool Equals(object obj) => obj is GridPos a && this == a;
    public override int GetHashCode() => x.GetHashCode() + (y * SQRT3).GetHashCode() + (w * Mathf.Sqrt(2)).GetHashCode();
    public override string ToString() => "(" + w + ", " + x + ", " + y + ")";

    public GridPos Horizontal { get => new GridPos(0, x, y); }

    public GridPos Rotate(float angle) {
        int rotations = Mathf.RoundToInt(angle / 60);
        while (rotations < 0) rotations += 600;
        switch (rotations % 6) {
            case 1: return new GridPos(w, -y, -z);
            case 2: return new GridPos(w, z, x);
            case 3: return new GridPos(w, -x, -y);
            case 4: return new GridPos(w, y, z);
            case 5: return new GridPos(w, -z, -x);
            default: return this;
        }
    }

    public int ToUnitRotation() {
        if (this == E) return 0;
        else if (this == W) return 60;
        else if (this == Q) return 120;
        else if (this == A) return 180;
        else if (this == S) return 240;
        else if (this == D) return 300;
        else throw new InvalidOperationException("Not unit hex: " + ToString());
    }

    public Vector3 World { get => new Vector3(x * 3, w * 4/3f, (y * 6 + x * 3) / SQRT3); }

    public TriPos[] Triangles { get => new TriPos[] {
        new TriPos(this + D, false),
        new TriPos(this, true),
        new TriPos(this, false),
        new TriPos(this + A, true),
        new TriPos(this + S, false),
        new TriPos(this + S, true),
    };}

    public static GridPos Random {
        get {
            int horiz = UnityEngine.Random.Range(0, 6);
            if (UnityEngine.Random.value < 2/3f) {
                return new GridPos[] {E, W, Q, A, S, D}[horiz];
            } else {
                int vert = UnityEngine.Random.value < .5f ? -1 : 1;
                return new GridPos[] {E, W, Q, A, S, D}[horiz] + GridPos.up * vert;
            }
        }
    }
}
