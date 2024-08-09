using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Biome {
    public GameObject[] decorFloor;
    public GameObject[] decorTallFloor;
    public GameObject[] decorAnywhere;
}

public class Biomes : MonoBehaviour {
    public Color[] floors; // 0 is default: ignore
    public Color[] walls; // 0 is default: ignore
    public Biome[] decor;

    private Grid<int> grid = new Grid<int>();

    public int lastBiome = 0;

    public int Next(GridPos pos, Func<int, int> biomeSupplier, bool allowOldBiomes) {
        int value = grid[pos.Horizontal];
        lastBiome = biomeSupplier(lastBiome);
        if (!allowOldBiomes && value != 0 && value != lastBiome) return 0;
        if (value == 0) {
            value = lastBiome;
            grid[pos.Horizontal] = value;
        }
        return value;
    }

    public static int NoChange(int lastBiome) => lastBiome;

    public int Next(GridPos pos) => Next(pos, (lastBiome) => lastBiome, true);

    public int this[GridPos pos] => grid[pos.Horizontal];

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
