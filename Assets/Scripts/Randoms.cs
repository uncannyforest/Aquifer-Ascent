using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Randoms {
    public static bool CoinFlip { get => Random.Range(0, 2) == 0; }
    public static int Sign { get => Random.Range(0, 2) * 2 - 1; }

    public static T InArray<T>(T[] array) => array[Random.Range(0, array.Length)];

    public static Vector2Int Vector2Int(Vector2Int v0, Vector2Int v1) {
        return new Vector2Int(Random.Range(v0.x, v1.x), Random.Range(v0.y, v1.y));
    }
    public static Vector2Int Vector2Int(int x0, int y0, int x1, int y1) {
        return new Vector2Int(Random.Range(x0, x1), Random.Range(y0, y1));
    }

    public static Vector2Int Midpoint(Vector2Int v0, Vector2Int v1) {
        int x = v0.x + v1.x;
        int y = v0.y + v1.y;
        int dx = (x % 2 == 0) ? 0 : Random.Range(0, 2);
        int dy = (y % 2 == 0) ? 0 : Random.Range(0, 2);
        return new Vector2Int(x / 2 + dx, y / 2 + dy);
    }

    public static Vector2 ChebyshevUnit() {
        float mean = Random.Range(-1f, 1f);
        float extreme = Sign;
        if (CoinFlip) {
            return new Vector2(mean, extreme);
        } else {
            return new Vector2(extreme, mean);
        }
    }

    public static IEnumerable<T> Order<T>(T first, T second) {
        if (CoinFlip) {
            yield return first;
            yield return second;
        } else {
            yield return second;
            yield return first;
        }
    }
}
