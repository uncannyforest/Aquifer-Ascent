using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPosExtensions {
    public static float Max(this Vector3 v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    public static Vector3 ScaleDivide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
}

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
    public static GridPos[] Units => new GridPos[] {E, W, Q, A, S, D};

    public static GridPos operator -(GridPos a) => new GridPos(-a.w, -a.x, -a.y);
    public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.w + b.w, a.x + b.x, a.y + b.y);
    public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.w - b.w, a.x - b.x, a.y - b.y);
    public static GridPos operator *(GridPos a, int n) => new GridPos(a.w * n, a.x * n, a.y * n);
    public static GridPos operator *(int n, GridPos a) => new GridPos(a.w * n, a.x * n, a.y * n);
    public static GridPos operator /(GridPos a, int n) => new GridPos(a.w / n, a.x / n, a.y / n);

    public static bool operator ==(GridPos a, GridPos b) => a.w == b.w && a.x == b.x && a.y == b.y;
    public static bool operator !=(GridPos a, GridPos b) => a.w != b.w && a.x != b.x || a.y != b.y;
    public override bool Equals(object obj) => obj is GridPos a && this == a;
    public override int GetHashCode() => x.GetHashCode() + (y * SQRT3).GetHashCode() + (w * Mathf.Sqrt(2)).GetHashCode();
    public override string ToString() => "(" + w + " | " + x + ", " + y + ", " + z + ")";

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

    public Vector3 World { get => Vector3.Scale(CaveGrid.Scale, new Vector3(x * 3/2f, w * 2f, (y * 3 + x * 3/2f) / SQRT3)); }
    public static GridPos FromWorld(Vector3 worldCoord) {
        Vector3 coord = worldCoord.ScaleDivide(CaveGrid.Scale);
        float fX = coord.x / 1.5f;
        float fY = (Quaternion.Euler(0, 120, 0) * coord).x / 1.5f;
        float fZ = (Quaternion.Euler(0, -120, 0) * coord).x / 1.5f;
        int x = Mathf.RoundToInt(fX);
        int y = Mathf.RoundToInt(fY);
        int z = Mathf.RoundToInt(fZ);
        if (x + y + z != 0) {
            if (Mathf.Abs(fX - x) > Mathf.Abs(fY - y) && Mathf.Abs(fX - x) > Mathf.Abs(fZ - z))
                x = -y - z;
            else if (Mathf.Abs(fY - y) > Mathf.Abs(fZ - z))
                y = -x - z;
            // else set z = -x - y; but that will be automatic
        }
        return new GridPos(Mathf.RoundToInt(coord.y / 2f), x, y);
    }
    public Vector3 HComponents { get => new Vector3(x, y, z); }
    public Vector3 HScale(Vector3 vector) => Vector3.Scale(HComponents, vector);

    public TriPos[] Triangles { get => new TriPos[] {
        new TriPos(this + D, false),
        new TriPos(this, true),
        new TriPos(this, false),
        new TriPos(this + A, true),
        new TriPos(this + S, false),
        new TriPos(this + S, true),
    };}

    public static GridPos Random(float elevChangeRate, Vector3 bias) {
        GridPos horiz = RandomHoriz(bias);
        if (UnityEngine.Random.value < elevChangeRate) {
            int vert = UnityEngine.Random.value < .5f ? -1 : 1;
            return horiz + GridPos.up * vert;
        } else {
            return horiz;
        }
    }

    // bias has components [-1, 1] which sum to 0
    public static GridPos RandomHoriz(Vector3 bias) {
        GridPos tentative = RandomHoriz();
        int axis = UnityEngine.Random.Range(0, 3);
        if (axis == 0) {
            if (UnityEngine.Random.value < bias.x * -tentative.x) {
                // Debug.Log("Bias flipped random x");
                tentative = -tentative;
            }
        } else if (axis == 1) {
            if (UnityEngine.Random.value < bias.y * -tentative.y) {
                // Debug.Log("Bias flipped random y");
                tentative = -tentative;
            }
        } else {
            if (UnityEngine.Random.value < bias.z * -tentative.z) {
                // Debug.Log("Bias flipped random z");
                tentative = -tentative;
            }
        }
        return tentative;
    }

    public static GridPos RandomHoriz() => Units[UnityEngine.Random.Range(0, 6)];

    public GridPos RandomHorizDeviation(Vector3 bias) {
        // because this must be a unit GridPos,
        // componentwise(this * this) components are integer [0, 1], sum to 2, and one must be 0
        // possAxesNotToSwap components are float [0, 2], sum to a number [0, 4], and one must be 0
        // componentwise, possAxesNotToSwap is this * this + bias * this = this * (this + bias)
        if (bias == -HComponents) bias = Vector3.zero; // avoid denominator of 0
        Vector3 possAxesNotToSwap = HScale(HComponents + bias);
        float denominator = possAxesNotToSwap.x + possAxesNotToSwap.y + possAxesNotToSwap.z; // in (0, 4]
        float seed = UnityEngine.Random.value * denominator;
        // Debug.Log("Steering bias power of " + (denominator - 2));
        // To rotate 60 degrees, we simply swap two axes
        // but we needed seed and possAxesNotToSwap to determine which two to swap
        if (seed < possAxesNotToSwap.x)
            return new GridPos(w, x, z);
        else if (seed < possAxesNotToSwap.x + possAxesNotToSwap.y)
            return new GridPos(w, z, y);
        else
            return new GridPos(w, y, x);
    }

    public GridPos RandomVertDeviation(float elevChangeRate) {
        if (this.w == 0) {
            if (UnityEngine.Random.value < elevChangeRate) {
                return this + up * (UnityEngine.Random.Range(0, 2) * 2 - 1);
            } else {
                return this;
            }
        } else {
            if (UnityEngine.Random.value < elevChangeRate) {
                return this;
            } else {
                return this.Horizontal;
            }
        }
    }
}
