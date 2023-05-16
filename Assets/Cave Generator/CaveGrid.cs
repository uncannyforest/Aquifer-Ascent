using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Decor))]
public class CaveGrid : MonoBehaviour {
    private static CaveGrid instance;
    public static CaveGrid I { get => instance; }
    CaveGrid(): base() {
        instance = this;
    }

    public GridPiece prefab;
    public Vector3 scale = new Vector3(2f, 2/3f, 2f);
    public GameObject floor;
    public GameObject revcorner;
    public GameObject corner;
    public GameObject revcornerFloor;
    public GameObject revcornerGutter;
    public GameObject revcornerShelf;
    public GameObject cornerFloor;
    public GameObject cornerGutter;
    public GameObject cornerShelf;
    public GameObject cornerShelfwall;
    public GameObject cornerBroadstair;
    public GameObject cornerStair;
    public GameObject cornerTightstair;
    public Material defaultMaterial;

    public Grid<bool> grid = new Grid<bool>();
    public static Grid<bool> Grid { get => instance.grid; }

    private Decor decor;
    public static Decor Decor {
        get {
            if (I.decor == null) I.decor = I.GetComponent<Decor>();
            return I.decor;
        }
    }

    private void UpdatePos(GridPos pos) {
        for (int i = -1; i <= 1; i++) {
            GridPos posToCheck = pos + GridPos.up * i;
            foreach (TriPos tri in posToCheck.Triangles) {
                GridPos[] horizCorners = tri.HorizCorners;
                bool[] data = new bool[] {
                    grid[horizCorners[0] - GridPos.up],
                    grid[horizCorners[1] - GridPos.up],
                    grid[horizCorners[2] - GridPos.up],
                    grid[horizCorners[0]],
                    grid[horizCorners[1]],
                    grid[horizCorners[2]],
                    grid[horizCorners[0] + GridPos.up],
                    grid[horizCorners[1] + GridPos.up],
                    grid[horizCorners[2] + GridPos.up],
                };
                Transform childTransform = transform.Find(tri.ToString());
                GridPiece child;
                if (childTransform == null) {
                    child = GameObject.Instantiate(prefab, tri.World,
                        tri.right ? Quaternion.identity : Quaternion.Euler(0, 180, 0), transform);
                    child.pos = tri;
                } else {
                    child = childTransform.GetComponent<GridPiece>();
                }
                child.Set(data);
                Decor.UpdatePos(tri, data, child);
            }
        }
    }

    public void SetPos(GridPos pos, bool value) {
        grid[pos] = value;
        UpdatePos(pos);
    }

}
