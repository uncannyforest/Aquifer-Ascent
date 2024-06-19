using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridPiece : MonoBehaviour {
    public static int ALL_OPEN = 511; // 2 ^ 9 - 1

    public TriPos pos;
    public int data;

    public TriPos Pos {
        set {
            pos = value;
            gameObject.name = pos.ToString();
        }
    }

    // public void Refresh() {
    //     foreach (Transform child in transform) {
    //         GameObject.Destroy(child.gameObject);
    //     }
    //     allData.Clear();
    //     IEnumerable<int> levels =
    //         from hex in pos.Corners
    //         where CaveGrid.I.inside[hex]
    //         select CaveGrid.I.elevation[hex];
    //     if (levels.Count() == 0) return;
    //     int min = levels.Min();
    //     int max = levels.Max();

    //     IEnumerable<int> lakeHeights =
    //         from hex in pos.Corners
    //         where CaveGrid.I.stats.GetLakeHeight(hex) != null
    //         select (int)CaveGrid.I.stats.GetLakeHeight(hex);
    //     if (lakeHeights.Count() > 0) {
    //         int lakeMax = lakeHeights.Max();
    //         if (max < lakeMax) max = lakeMax;
    //     }

    //     for (int i = min; i <= max + 1; i++) {
    //         FillLevel(i);
    //     }
    // }

    public void Set() => Refresh();

    public void Refresh() {
        for (int i = transform.childCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
        // Sample current level, below, and above
        // 0 = all air, 3 = all ground, 1-2 = floor, 5-6 = ceiling
        int level = pos.w;
        Grid<bool> grid = CaveGrid.I.grid;
        GridPos[] hc = pos.HorizCorners;
        int[] data = new int[3];
        for (int i = 0; i < 3; i++) {
            if (grid[hc[i] + GridPos.up]) { // air at top
                data[i] = grid[hc[i] - GridPos.up] ? 0 // all air
                    : grid[hc[i]] ? 1 : 2;
            } else { // ground at top
                data[i] = !grid[hc[i] - GridPos.up] ? 3 // all ground
                    : !grid[hc[i]] ? 4 : 5;
            }
        }
        this.data = data[0] * 100 + data[1] * 10 + data[2];

        bool canYFlip = true;
        bool tryYFlip = false;
        for (int i = 0; i < 3; i++) {
            if (data[i] == 1 || data[i] == 2) canYFlip = false;
            if (data[i] == 4 || data[i] == 5) tryYFlip = true;
        }
        bool yFlipNow = canYFlip && tryYFlip; // Flip Rule
        if (yFlipNow) for (int i = 0; i < 3; i++) data[i] = (6 - data[i]) % 6;

        if (data[0] == 1 && data[1] == 1 && data[2] == 1) {
            Create(CaveGrid.I.floor, level, Random.Range(0, 3), Randoms.CoinFlip, yFlipNow, Three(0, 1, 2));
            return; // Floor Rule
        }
        if ((data[0] <= 1 || data[0] == 5) && (data[1] <= 1 || data[1] == 5) && (data[2] <= 1 || data[2] == 5)) {
            return; // Open Rule, nothing to render
        }
        if (data[0] >= 2 && data[1] >= 2 && data[2] >= 2 && data[0] <= 4 && data[1] <= 4 && data[2] <= 4) {
            return; // Ground Rule, nothing to render
        }

        int otherLoc = -1;
        int otherValue = -1;
        if (HasTwo(data, 0, ref otherLoc, ref otherValue)) {
            if (otherValue == 3) {
                Create(CaveGrid.I.revcorner, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 2));
            } else { // if (otherValue == 2)
                Create(CaveGrid.I.revcornerGutter, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 1, 2));
            } // 0, 1, 5 - Open Rule // 4 - Flip Rule
        } else if (HasTwo(data, 1, ref otherLoc, ref otherValue)) {
            if (otherValue == 3 || otherValue == 4) {
                Create(CaveGrid.I.revcornerBaseboard, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 2));
            } else { // if (otherValue == 2)
                Create(CaveGrid.I.lowerSlope, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 1, 2));
            } // 0, 1, 5 - Open Rule
        } else if (HasTwo(data, 5, ref otherLoc, ref otherValue)) {
            // if (otherValue == 2)
            Create(CaveGrid.I.revcornerBaseboard, level, otherLoc, Randoms.CoinFlip, true, Three(0, 2));
            // 0, 1, 5 - Open Rule // 0, 3, 4 - Flip Rule
        } else if (HasTwo(data, 2, ref otherLoc, ref otherValue)) {
            if (otherValue == 1) {
                Create(CaveGrid.I.upperSlope, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 1, 2));
            } else if (otherValue == 0) {
                Create(CaveGrid.I.cornerGutter, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(0, 1, 2));
            } else { // if (otherValue == 5)
                Create(CaveGrid.I.cornerBaseboard, level, otherLoc, Randoms.CoinFlip, true, Three(1));
            } // 2, 3, 4 - Ground Rule
        } else if (HasTwo(data, 3, ref otherLoc, ref otherValue)) {
            if (otherValue == 1) {
                Create(CaveGrid.I.cornerBaseboard, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(1));
            } else { // if (otherValue == 0)
                Create(CaveGrid.I.corner, level, otherLoc, Randoms.CoinFlip, yFlipNow, Three(1));
            }  // 2, 3, 4 - Ground Rule // 4, 5 - Flip Rule
        } else if (HasTwo(data, 4, ref otherLoc, ref otherValue)) {
            // if (otherValue == 1)
            Create(CaveGrid.I.cornerBaseboard, level, otherLoc, Randoms.CoinFlip, false, Three(1));
            // 2, 3, 4 - Ground Rule // 0, 3, 5 - Flip Rule
        } else if (HasTwo(data, 5, ref otherLoc, ref otherValue)) {
            // if (otherValue == 2)
            Create(CaveGrid.I.revcornerBaseboard, level, otherLoc, Randoms.CoinFlip, true, Three(0, 2));
            // 0, 1, 5 - Open Rule // 0, 3, 4 - Flip Rule
        } else { // data is 3 different numbers, out of 0, 1, 2, 3, 4, 5
            if (Mathf.Max(data[0], data[1], data[2]) <= 3) { // floorside: data is 3 different numbers, out of 0, 1, 2, 3
                int sum = data[0] + data[1] + data[2]; // sum is in [3, 6]
                int pivotValue = sum <= 4 ? 1 : 2; // where the triangle gets split down the middle: 1, 1, 2, 2
                int pivot = data[0] == pivotValue ? 0 : data[1] == pivotValue ? 1 : 2;
                bool xFlip = data[(pivot + 2) % 3] == (sum <= 4 ? 0 : 3); // the values at x = 1 in the model: 0, 0, 3, 3
                if (sum == 3) {
                    Create(CaveGrid.I.upperCurve, level, pivot, xFlip, yFlipNow, Three(0, 1, 2));
                } else if (sum == 4) {
                    Create(CaveGrid.I.endBaseboard, level, pivot, xFlip, yFlipNow, Three(1, 2));
                } else if (sum == 5) {
                    Create(CaveGrid.I.endGutter, level, pivot, xFlip, yFlipNow, Three(0, 1));
                } else {
                    Create(CaveGrid.I.lowerCurve, level, pivot, xFlip, yFlipNow, Three(0, 1));
                }
            } else if ((data[0] % 3) * (data[1] % 3) * (data[2] % 3) != 0){ // data is 3 different numbers, out of 1, 2, 4, 5 
                int sum = data[0] + data[1] + data[2]; // sum is in { 7, 8, 10, 11 }
                int pivotValue = sum == 7 ? 2 : sum == 8 ? 1 : sum == 10 ? 5 : 4; // where the triangle gets split down the middle: 2, 1, 5, 4
                int pivot = data[0] == pivotValue ? 0 : data[1] == pivotValue ? 1 : 2;
                bool xFlip = data[(pivot + 2) % 3] == (sum == 7 ? 4 : sum == 8 ? 5 : sum == 10 ? 1 : 2); // the values at x = 1 in the model: 4, 5, 1, 2
                if (sum == 7) {
                    Create(CaveGrid.I.lowerCurve, level, pivot, xFlip, false, Three(0, 1));
                } else if (sum == 8) {
                    Create(CaveGrid.I.tunnelStair, level, pivot, xFlip, false, Three(0, 1, 2));
                } else if (sum == 10) {
                    Create(CaveGrid.I.tunnelStair, level, pivot, xFlip, true, Three(0, 1, 2));
                } else {
                    Create(CaveGrid.I.lowerCurve, level, pivot, xFlip, true, Three(0, 1));
                }
            } else { // one { 1, 2 } and one { 4, 5 } and one { 0, 3 } - except 015 (Open Rule) or 234 (Ground Rule)
                int sum = data[0] + data[1] + data[2]; // sum is in { 5, 6, 7, 8, 9, 10 }: that is, [5, 10]
                int pivotValue = new int[] {0, 0, 0, 1, 3, 5} [sum - 5]; // where the triangle gets split down the middle
                int pivot = data[0] == pivotValue ? 0 : data[1] == pivotValue ? 1 : 2;
                bool xFlip;
                if (sum == 8 || sum == 10) xFlip = Randoms.CoinFlip;
                else xFlip = data[(pivot + 2) % 3] == (sum == 5 ? 1 : sum == 6 ? 4 : sum == 7 ? 5 : 1); // the values at x = 1 in the model: 1, 4, 5, _, 1, _
                if (sum == 5) {
                    Create(CaveGrid.I.tunnelThinLedge, level, pivot, xFlip, true, Three(0, 1, 2));
                } else if (sum == 6) {
                    Create(CaveGrid.I.tunnelBroadLedge, level, pivot, xFlip, false, Three(0, 1, 2));
                } else if (sum == 7) {
                    Create(CaveGrid.I.tunnelThinLedge, level, pivot, xFlip, false, Three(0, 1, 2));
                } else if (sum == 8) {
                    Create(CaveGrid.I.cornerBaseboard, level, pivot, xFlip, false, Three(1));
                } else if (sum == 9) {
                    Create(CaveGrid.I.tunnelBroadStair, level, pivot, xFlip, false, Three(0, 2));
                } else {
                    Create(CaveGrid.I.cornerBaseboard, level, pivot, xFlip, true, Three(1));
                }
            }
        }
    }

    public bool HasTwo(int[] data, int what, ref int otherLoc, ref int otherValue) {
        bool result = data.Where(d => d == what).Count() == 2;
        if (!result) return false;
        otherLoc = data[0] != what ? 0 : data[1] != what ? 1 : 2;
        otherValue = data[otherLoc];
        return true;
    }

    public Transform Create(GameObject prefab, int y, int pivot, bool xFlip, bool yFlip, int[] origSources) {
        int yRot = 30 - pivot * 120;
        int right = (pivot + (xFlip?1:2)) % 3;
        int left = (pivot + (xFlip?2:1)) % 3;
        int[] map = new int[] {-1, right, pivot, left};
        int[] sources = new int[] {map[origSources[0] + 1] + 1, map[origSources[1] + 1] + 1, map[origSources[2] + 1] + 1};
        Transform newPiece = GameObject.Instantiate(prefab, transform).transform;
        if (yFlip) newPiece.localPosition = new Vector3(0, CaveGrid.I.scale.y, 0);
        newPiece.localRotation = Quaternion.Euler(0, yRot, 0);
        newPiece.localScale = Vector3.Scale(new Vector3(xFlip ? -1 : 1, yFlip ? -1 : 1, 1), CaveGrid.I.scale);
        SetMaterial(newPiece, sources);
        return newPiece;
    }

    private int[] Three(params int[] values) {
        int[] result = new int[3];
        int i = 0;
        for (; i < values.Length; i++) result[i] = values[i];
        for (; i < 3; i++) result[i] = -1;
        return result;
    }

    private void SetMaterial(Transform newPiece, int[] src) {
        Color[] floors = CaveGrid.Biome.GetFloors(pos);
        Color[] walls = CaveGrid.Biome.GetWalls(pos);
        // Better not to make a new material — this is the current bottleneck.
        // 0.3 ms * 18 TriPoses * 2 Sets * 21 largest diamter = 200ms
        Material material = new Material(CaveGrid.I.defaultMaterial);
        material.SetColor("_Color1", floors[src[0]]);
        material.SetColor("_Color2", floors[src[1]]);
        material.SetColor("_Color3", floors[src[2]]);
        material.SetColor("_Walls1", walls[src[0]]);
        material.SetColor("_Walls2", walls[src[1]]);
        material.SetColor("_Walls3", walls[src[2]]);
        foreach (MeshRenderer renderer in newPiece.GetComponentsInChildren<MeshRenderer>()) renderer.material = material;
    }
}
