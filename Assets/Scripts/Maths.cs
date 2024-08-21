using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class MathExtensions {
    public static float ScaleTo(this float value, float from0, float from1) {
        return (from1 - from0) * value + from0;
    }
}

public class Maths {
    public static float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }

    // Given a value with random uniform distribution [0, 1],
    // returns a new value [0, 1) where 0 is double probability and 1 is zero probability.
    // Output function is decreasing (transforms input 1 into 0 and 0 into 1):
    // use of this function manually pass (1 - x) if input
    // carries extra meaning or input distribution is not uniform.
    public static float Bias0(float x) {
        return 1 - Mathf.Sqrt(x);
    }

    // Given a value with random uniform distribution [0, 1],
    // returns a value y in [0, infinity) where
    // - there is 1/2 chance y > 1; if so,
    // - there is a 1/4 chance y > 2 (1/8 total); if so,
    // - there is a 1/16 chance y > 3 (1/128 total); if so,
    // - there is a 1/256 chance y > 4 (1/32768 total), etc.
    // Output function  is decreasing (transforms input 1 into 0 and 0 into infinity).
    public static float SuperExpDecayDistribution(float x) {
        return Mathf.Log(1 - Mathf.Log(x, 2), 2);
    }
}
