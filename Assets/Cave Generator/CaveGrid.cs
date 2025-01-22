using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public Material softMaterial;
    public ParticleSystem dustPrefab;
    public ParticleSystem undustPrefab;
    public Random.State seed;

    public Grid<bool> grid = new Grid<bool>();
    public static Grid<bool> Grid { get => instance.grid; }
    public Grid<bool> soft = new Grid<bool>();

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

    void Awake() {
        seed = Random.state;
        Debug.Log("Set seed from Random");
    }

    public static bool CanOpen(GridPos pos) {
        return I.grid[pos] || I.soft[pos] || I.soft[pos - GridPos.up];
    }

    public void UpdatePos(GridPos pos, int relMinNeedsUpdate, int relMaxNeedsUpdate) {
        for (int i = relMaxNeedsUpdate; i >= relMinNeedsUpdate; i--) {
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

    [Obsolete("Deprecated, use CaveMod parameter")]
    public void SetPos(GridPos pos, bool value) => SetPos(new Mod(pos, 1, value));

    public Transform GetPosParent(GridPos pos) {
        bool childExists = renderGrid.TryGetValue(pos.Triangles[0], out GridPiece parent);
        return childExists ? parent.transform : null;
    }

    public void SetPos(Mod mod) {
        GridPos pos = mod.pos;
        int roof = mod.roof;
        bool value = mod.open;

        int relMinUpdated = 0;
        int relMaxUpdated = roof;

        if (grid[pos + (roof + 1) * GridPos.up] != value && grid[pos + (roof + 2) * GridPos.up] == value)
            relMaxUpdated++;
        if (grid[pos - GridPos.up] != value && grid[pos - 2 * GridPos.up] == value)
            relMinUpdated--;

        if (value) {
            ForceSetPos(pos, relMinUpdated, relMaxUpdated, true);
            MakeDust(pos, relMinUpdated, relMaxUpdated, true);
        } else {
            MakeDust(pos, relMinUpdated, relMaxUpdated, false);
            this.Invoke(() => ForceSetPos(pos, relMinUpdated, relMaxUpdated, false), undustPrefab.main.duration);
        }
    }

    private void ForceSetPos(GridPos pos, int relMinUpdated, int relMaxUpdated, bool value) {
        CaveGrid.Biome.Next(pos);
        for (int i = relMinUpdated; i <= relMaxUpdated; i++)
            grid[pos + GridPos.up * i] = value;
        if (value)
            for (int i = relMinUpdated; i <= relMaxUpdated - 1; i++)
                if (soft[pos + i * GridPos.up]) {
            Debug.Log("Unneeded soft at " + (pos + i * GridPos.up));
            
            soft[pos + i * GridPos.up] = false;
        }
        UpdatePos(pos, relMinUpdated - 1, relMaxUpdated + 1);
    }

    private void MakeDust(GridPos pos, int relMinUpdated, int relMaxUpdated, bool open) {
        Transform dust;
        if (open) dust = GameObject.Instantiate(dustPrefab).transform;
        else dust = GameObject.Instantiate(undustPrefab).transform;
        dust.position = pos.World + Vector3.up * CaveGrid.Scale.y * relMinUpdated;
        dust.localScale = Vector3.Scale(CaveGrid.Scale, new Vector3(1, relMaxUpdated - relMinUpdated, 1));
    }

    public struct Mod {
        public GridPos pos;
        public int roof;
        public bool open;

        public Mod(GridPos pos, int roof, bool open) {
            this.pos = pos;
            this.roof = roof;
            this.open = open;
        }

        public static Mod Cave(GridPos pos, int roof = 1) => new Mod(pos, roof, true);
        public static Mod Wall(GridPos pos, int roof = 1) => new Mod(pos, roof, false);

        public Mod Inverted { get => new Mod(pos, roof, !open); }
        public bool IsUnnecessary {
            get {
                bool result = true;
                for (int i = 0; i <= roof; i++)
                    if (CaveGrid.I.grid[pos + i * GridPos.up] != open)
                        result = false;
                return result;
            }
        }

        public bool Overlaps {
            get {
                bool result = false;
                for (int i = 0; i <= roof; i++)
                    if (CaveGrid.I.grid[pos + i * GridPos.up] == open)
                        result = true;
                return result;
            }
        }

        public Mod RandomBump() {
            if (roof <= 2) return this;
            int floorBump = Random.Range(0, 2);
            return CaveGrid.Mod.Cave(pos + floorBump * GridPos.up, roof - (floorBump + Random.Range(0, 2)));
        }
        public Mod RandomFromMidpoint(bool maximizeFloor = false, bool maximizeCeil = false) {
            int vMidpoint = (roof - 1) / 2;
            int vMidpointExtraHeight = (roof - 1) % 2;
            return RandomVerticalExtension(pos + vMidpoint * GridPos.up,
                maximizeFloor ? vMidpoint : 0, vMidpoint,
                vMidpointExtraHeight + (maximizeCeil ? vMidpoint : 0), vMidpointExtraHeight + vMidpoint,
                open);
        }
        public Mod RandomSubset() => RandomVertical(pos, 0, roof - 1, open);
        public Mod RandomEnd(bool ceil) {
            int height = Randoms.ExpDecay(1, roof);
            return new Mod(ceil ? pos + GridPos.up * (roof - height) : pos, height, open);
        }

        public static Mod RandomVerticalExtension(GridPos pos, int minExtraFloor, int maxExtraFloor, int minExtraRoof, int maxExtraRoof, bool open = true) {
            int floor = Random.Range(-maxExtraFloor, -minExtraFloor + 1);
            int roof = Random.Range(minExtraRoof, maxExtraRoof + 1) + 1 - floor;
            return new Mod(pos + GridPos.up * floor, roof, true);
        }
        // Picks random floor and roof
        // if equal, returns null
        // otherwise returns column [floor, roof]
        public static Mod? RandomVerticalMaybe(GridPos pos, int maxExtraFloor, int maxExtraRoof) {
            int r1 = Random.Range(-maxExtraFloor, maxExtraRoof + 2);
            int r2 = Random.Range(-maxExtraFloor, maxExtraRoof + 2);
            if (r1 == r2) return null;
            int floor = Mathf.Min(r1, r2);
            int roof = Mathf.Max(r1, r2) - floor;
            return new Mod(pos + GridPos.up * floor, roof, true);
        }

        // Picks random floor and roof not equal
        // returns column [floor, roof]
        public static Mod RandomVertical(GridPos pos, int maxExtraFloor, int maxExtraRoof, bool open = true) {
            int r1 = Random.Range(-maxExtraFloor, maxExtraRoof + 1);
            int r2 = Random.Range(-maxExtraFloor, maxExtraRoof + 1);
            int floor = Mathf.Min(r1, r2);
            int roof = Mathf.Max(r1, r2) - floor + 1;
            return new Mod(pos + GridPos.up * floor, roof, open);
        }

        public override string ToString() {
            return pos + " | " + roof + (open ? " air" : " wall");
        }
    }
}
