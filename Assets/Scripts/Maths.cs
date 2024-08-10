using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Maths {
    public static float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }

    // Takes a value from random uniform distribution [0, 1]
    // and returns a value where 0 is double probability and 1 is zero probability.
    // Does not transform x into 1-x because we are assuming the input doesn't matter, only its distribution.
    public static float Bias0(float x) {
        return 1 - Mathf.Sqrt(x);
    }
}
