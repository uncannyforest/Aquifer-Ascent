using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Biome : MonoBehaviour {
    public Color[] floors; // 0 is default: ignore
    public Color[] walls; // 0 is default: ignore

    public Grid<int> grid = new Grid<int>();

    private int lastBiome = 0;

    public int Next(GridPos pos, Func<int> biomeSupplier) {
        int value = grid[pos.Horizontal];
        if (value == 0) {
            value = biomeSupplier();
            grid[pos.Horizontal] = value;
        }
        lastBiome = value;
        return value;
    }

    public int Next(GridPos pos) {
        return Next(pos, () => lastBiome);
    }

    public int[] Get(TriPos tri) {
        return (int[])
            from corner in tri.HorizCorners
            select grid[corner.Horizontal];
    }

    public Color[] GetFloors(TriPos tri) {
        return new Color[] {
            floors[0],
            floors[grid[tri.HorizCorners[0].Horizontal]],
            floors[grid[tri.HorizCorners[1].Horizontal]],
            floors[grid[tri.HorizCorners[2].Horizontal]]};
    }
    public Color[] GetWalls(TriPos tri) {
        return new Color[] {
            walls[0],
            walls[grid[tri.HorizCorners[0].Horizontal]],
            walls[grid[tri.HorizCorners[1].Horizontal]],
            walls[grid[tri.HorizCorners[2].Horizontal]]};
    }
}
