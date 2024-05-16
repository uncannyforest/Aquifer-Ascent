using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Biomes))]
[RequireComponent(typeof(Decor))]
public class CaveGrid : MonoBehaviour {
    private static CaveGrid instance;
    public static CaveGrid I { get => instance; }
    CaveGrid(): base() {
        instance = this;
    }
    public static Vector3 Scale { get => I.scale; }

    public GridPiece prefab;
    public Vector3 scale = new Vector3(4/3f, 2/3f, 4/3f);
    public GameObject floor;
    public GameObject revcorner;
    public GameObject corner;
    public GameObject revcornerBaseboard;
    public GameObject revcornerGutter;
    public GameObject cornerBaseboard;
    public GameObject cornerGutter;
    public GameObject lowerSlope;
    public GameObject upperSlope;
    public GameObject lowerCurve;
    public GameObject upperCurve;
    public GameObject endBaseboard;
    public GameObject endGutter;
    public GameObject tunnelStair;
    public GameObject tunnelBroadStair;
    public GameObject tunnelThinLedge;
    public GameObject tunnelBroadLedge;
    public Material defaultMaterial;

    public Grid<bool> grid = new Grid<bool>();
    public static Grid<bool> Grid { get => instance.grid; }

    private Dictionary<TriPos, GridPiece> renderGrid = new Dictionary<TriPos, GridPiece>();

    private Decor decor;
    public static Decor Decor {
        get {
            if (I.decor == null) I.decor = I.GetComponent<Decor>();
            return I.decor;
        }
    }
    private Biomes biome;
    public static Biomes Biome {
        get {
            if (I.biome == null) I.biome = I.GetComponent<Biomes>();
            return I.biome;
        }
    }

    private void UpdatePos(GridPos pos) {
        for (int i = 1; i >= -1; i--) {
            GridPos posToCheck = pos + GridPos.up * i;
            foreach (TriPos tri in posToCheck.Triangles) {
                GridPos[] horizCorners = tri.HorizCorners;
                bool childExists = renderGrid.TryGetValue(tri, out GridPiece child);
                if (!childExists) {
                    child = GameObject.Instantiate(prefab, tri.World + Vector3.down * scale.y,
                        tri.right ? Quaternion.identity : Quaternion.Euler(0, 180, 0), transform);
                    child.Pos = tri;
                    renderGrid[tri] = child;
                }
                child.Refresh();
                Decor.UpdatePos(tri, child);
            }
        }
    }

    public void SetPos(GridPos pos, bool value) {
        CaveGrid.Biome.Next(pos);
        grid[pos] = value;
        grid[pos + GridPos.up] = value;
        if (grid[pos + 2 * GridPos.up] != value && grid[pos + 3 * GridPos.up] == value) {
            grid[pos + 2 * GridPos.up] = value;
            UpdatePos(pos + 2 * GridPos.up);
        }
        if (grid[pos - GridPos.up] != value && grid[pos - 2 * GridPos.up] == value) {
            grid[pos - GridPos.up] = value;
            UpdatePos(pos - GridPos.up);
        }
        UpdatePos(pos);
        UpdatePos(pos + GridPos.up);
    }

}
