using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridPosExtensions {
    public static float Max(this Vector3 v) => Mathf.Max(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    public static Vector3 SetNaNTo(this Vector3 v, float def) {
        float x = float.IsNaN(v.x) ? def : v.x;
        float y = float.IsNaN(v.y) ? def : v.y;
        float z = float.IsNaN(v.z) ? def : v.z;
        return new Vector3(x, y, z);
    }
    public static Vector3 MaxNormalized(this Vector3 v) {
        if (v.Max() == float.PositiveInfinity) {
            float x = v.x == float.PositiveInfinity ? 1 : v.x == float.NegativeInfinity ? -1 : 0;
            float y = v.y == float.PositiveInfinity ? 1 : v.y == float.NegativeInfinity ? -1 : 0;
            float z = v.z == float.PositiveInfinity ? 1 : v.z == float.NegativeInfinity ? -1 : 0;
            float correction = (x + y + z) / -3; // make sum to zero in this case
            v = new Vector3(x, y, z) + correction * Vector3.one;
        }
        if (v == Vector3.zero) return Vector3.zero;
        else return v / v.Max();
    }
    public static Vector3 ScaleDivide(this Vector3 a, Vector3 b) => new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
    public static GridPos[] Line(this GridPos from, GridPos to) {
        List<GridPos> result = new List<GridPos>() { from };
        GridPos displacement = to - from;
        int numSteps = displacement.Magnitude;
        Vector3 step = displacement.World / numSteps;
        Vector3 world = from.World;
        for (int i = 0; i < numSteps; i++) {
            world += step;
            result.Add(GridPos.FromWorld(world));
        }
        return result.ToArray();
    }

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
    public int Magnitude { get => Mathf.Abs(w) + Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z)); }
    // square of the Euclidian magnitude. A hex grid has an additional x*y term
    public int SqrEuclMagnitude { get => w * w + x * x + x * y + y * y; }
    // Square of the Euclidian magnitude of the hex pos represented. A hex grid has an additional x*y term
    public static float SqrHexEuclMag(Vector3 v) => v.x * v.x + v.x * v.y + v.y * v.y;

    public GridPos RotateLeft() => Rotate(60);
    public GridPos RotateRight() => Rotate(-60);
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

    // Not used, but sanity check that Angle() was coded correctly
    public static int UnitsAngle(GridPos from, GridPos to) {
        int angle = to.ToUnitRotation() - from.ToUnitRotation();
        if (angle < 0) angle += 360;
        return angle;
    }

    public static float Angle(GridPos from, GridPos to) => Vector3.SignedAngle(from.World, to.World, Vector3.down);

    public int ToUnitRotation() {
        if (this == E) return 0;
        else if (this == W) return 60;
        else if (this == Q) return 120;
        else if (this == A) return 180;
        else if (this == S) return 240;
        else if (this == D) return 300;
        else throw new InvalidOperationException("Not unit hex: " + ToString());
    }

    public static float WorldHorizScale = CaveGrid.Scale.z * 2;
    public Vector3 World { get => Vector3.Scale(CaveGrid.Scale, new Vector3(x * SQRT3, w, y * 2 + x)); }
    public static GridPos FromWorld(Vector3 worldCoord) {
        Vector3 coord = worldCoord.ScaleDivide(CaveGrid.Scale);
        int w = Mathf.RoundToInt(coord.y);
        float fX = coord.x / SQRT3;
        float fY = (Quaternion.Euler(0, 120, 0) * coord).x / SQRT3;
        float fZ = (Quaternion.Euler(0, -120, 0) * coord).x / SQRT3;
        return GridPos.RoundFromVector3(new Vector3(fX, fY, fZ)) + up * w;
    }
    public static GridPos RoundFromVector3(Vector3 hComponents) {
        int x = Mathf.RoundToInt(hComponents.x);
        int y = Mathf.RoundToInt(hComponents.y);
        int z = Mathf.RoundToInt(hComponents.z);
        if (x + y + z != 0) {
            if (Mathf.Abs(hComponents.x - x) > Mathf.Abs(hComponents.y - y) && Mathf.Abs(hComponents.x - x) > Mathf.Abs(hComponents.z - z))
                x = -y - z;
            else if (Mathf.Abs(hComponents.y - y) > Mathf.Abs(hComponents.z - z))
                y = -x - z;
            // else set z = -x - y; but that will be automatic
        }
        return new GridPos(0, x, y);
    }
    public Vector3 HComponents { get => new Vector3(x, y, z); }
    public Vector3 HScale(Vector3 vector) => Vector3.Scale(HComponents, vector);
    public GridPos HNormalized { get => RoundFromVector3(HComponents.MaxNormalized()); }

    public TriPos[] Triangles { get => new TriPos[] {
        new TriPos(this + D, false),
        new TriPos(this, true),
        new TriPos(this, false),
        new TriPos(this + A, true),
        new TriPos(this + S, false),
        new TriPos(this + S, true),
    };}

    public static GridPos[] ListAllWithMagnitude(int mag) {
        if (mag == 0) return new GridPos[] { GridPos.zero };
        List<GridPos> result = new List<GridPos>();
        for (int i = 0; i < 6; i++) {
            GridPos corner = Units[i] * mag;
            GridPos increment = Units[(i + 2) % 6];
            result.Add(corner);
            for (int j = 1; j < mag; j++) {
                corner += increment;
                result.Add(corner);
            }
        }
        return result.ToArray();
    }

    public static GridPos Random(float elevChangeRate, Vector3 bias, float upwardRate = .5f) {
        GridPos horiz = RandomHoriz(bias);
        if (UnityEngine.Random.value < elevChangeRate) {
            int vert = UnityEngine.Random.value < upwardRate ? 1 : -1;
            return horiz + GridPos.up * vert;
        } else {
            return horiz;
        }
    }

    // multiply RandomHoriz parameter by this factor to get the outcome of the old algorithm prior to refactor
    public static float MODERATE_BIAS = 1/3f;

    // bias has components which sum to 0
    // bias magnitude (.Max()) impact on probability of angle from bias:
    // mag | 180deg 120deg 60deg 0deg
    //  0  |   1/6   1/6   1/6   1/6
    // 1/3 |   1/18  2/18  4/18  5/18  <- multiply MaxNormalized Vector3 by MODERATE_BIAS
    // 1/2 |    0    1/12  3/12  4/12
    //  1  |    0     0    1/3   1/3
    //  2  |    0     0    1/6   2/3
    // inf |    0     0     0     1
    public static GridPos RandomHoriz(Vector3 bias) {
        if (bias.Max() > 1 && UnityEngine.Random.value > 1 / bias.Max())
            return GridPos.RoundFromVector3(bias.MaxNormalized());

        GridPos tentative = RandomHoriz();
        float chanceOfFlip = bias.x * -tentative.x + bias.y * -tentative.y + bias.z * -tentative.z;
        // short-circuit: don't bother running Random.value if chanceOfFlip outside (0, 1)
        if (chanceOfFlip >= 1 || chanceOfFlip > 0 && UnityEngine.Random.value < chanceOfFlip)
            return -tentative;
        else return tentative;
    }

    public static GridPos RandomHoriz() => Units[UnityEngine.Random.Range(0, 6)];

    public GridPos RandomDeviation(Vector3 bias) {
        GridPos tentative = RandomHoriz(bias);
        if (tentative == this.Horizontal) {
            tentative = this.RandomVertDeviation(1, 0);
        }
        return tentative;
    }

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

    public GridPos RandomVertDeviation(float elevChangeRate, float flattenBackRate, float upwardRate = .5f) {
        if (this.w == 0) {
            if (UnityEngine.Random.value < elevChangeRate) {
                return this + up * (UnityEngine.Random.value < upwardRate ? 1 : -1);
            } else {
                return this;
            }
        } else {
            if (UnityEngine.Random.value < flattenBackRate) {
                return this.Horizontal;
            } else {
                return this;
            }
        }
    }
}
