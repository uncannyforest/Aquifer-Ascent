using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T> {
    private List<List<List<T>>>[] quads = new List<List<List<T>>>[] {
        new List<List<List<T>>>(), // x y z >= 0
        new List<List<List<T>>>(), // y z
        new List<List<List<T>>>(), // z
        new List<List<List<T>>>(), // x z
        new List<List<List<T>>>(), // x
        new List<List<List<T>>>(), //
        new List<List<List<T>>>(), // y
        new List<List<List<T>>>()}; // x y

    private T get3D(List<List<List<T>>> list3d, int x, int y, int z) {
        if (list3d.Count <= z) return default(T);
        List<List<T>> list2d = list3d[z];
        if (list2d.Count <= y) return default(T);
        List<T> list = list2d[y];
        if (list.Count <= x) return default(T);
        return list[x];
    }

    private void set3D(List<List<List<T>>> list3d, int x, int y, int z, T value) {
        while (list3d.Count <= z) list3d.Add(new List<List<T>>());
        List<List<T>> list2d = list3d[z];
        while (list2d.Count <= y) list2d.Add(new List<T>());
        List<T> list = list2d[y];
        while (list.Count <= x) list.Add(default(T));
        list[x] = value;
    }

    private void ThrowIfLarge(GridPos pos) {
        if (1000_000 <= pos.x || 1000_000 <= pos.y || 1000_000 <= pos.w
                || 1000_000 <= -pos.x || 1000_000 <= -pos.y || 1000_000 <= -pos.w)
            throw new ArgumentOutOfRangeException("Huge GridPos " + pos);
    }

    public T this[GridPos pos] {
        get {
            ThrowIfLarge(pos);
            int xIndex = pos.x >= 0 ? pos.x : -1 - pos.x;
            int yIndex = pos.y >= 0 ? pos.y : -1 - pos.y;
            int zIndex = pos.w >= 0 ? pos.w : -1 - pos.w;
            int quadIndex = pos.x >= 0 ? 1 : 0;
            if (pos.y >= 0) quadIndex = 3 - quadIndex;
            if (pos.w >= 0) quadIndex = 7 - quadIndex;
            return get3D(quads[quadIndex], xIndex, yIndex, zIndex);
        }
        set {
            ThrowIfLarge(pos);
            int xIndex = pos.x >= 0 ? pos.x : -1 - pos.x;
            int yIndex = pos.y >= 0 ? pos.y : -1 - pos.y;
            int zIndex = pos.w >= 0 ? pos.w : -1 - pos.w;
            if (xIndex > xMax) xMax = xIndex;
            if (yIndex > yMax) yMax = yIndex;
            if (zIndex > zMax) zMax = zIndex;
            if (xIndex < xMin) xMin = xIndex;
            if (yIndex < yMin) yMin = yIndex;
            if (zIndex < zMin) zMin = zIndex;
            int quadIndex = pos.x >= 0 ? 1 : 0;
            if (pos.y >= 0) quadIndex = 3 - quadIndex;
            if (pos.w >= 0) quadIndex = 7 - quadIndex;
            set3D(quads[quadIndex], xIndex, yIndex, zIndex, value);
        }
    }

    private int xMax = -1;
    private int yMax = -1;
    private int zMax = -1;
    private int xMin = 0;
    private int yMin = 0;
    private int zMin = 0;

    public BoundsInt Bounds { get => new BoundsInt(xMin, yMin, zMin, xMax, yMax, zMax); }
}
