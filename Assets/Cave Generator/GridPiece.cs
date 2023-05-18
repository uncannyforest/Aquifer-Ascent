using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPiece : MonoBehaviour {
    public TriPos pos;
    public int data;
    
    void Start() {
        gameObject.name = pos.ToString();
    }

    public void Set(bool[] data) {
        int newData = BoolArrayToInt(data);
        if (this.data == newData) return;
        this.data = newData;
        for (int i = transform.childCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);

        if (data[3] && data[4] && data[5]) return;

        if (!data[3] && !data[4] && !data[5]) {
            MaybeCreateFloor(data[6], data[7], data[8], false);
            MaybeCreateFloor(data[0], data[1], data[2], true);
        } else if (data[3] ^ data[4] ^ data[5]) {
            int open = data[3] ? 0 : data[4] ? 1 : 2;
            int yRot = 30 - 120 * open;
            int openCase = 0;
            if (data[(open + 1) % 3]) openCase += 1; // below right
            if (data[(open + 2) % 3]) openCase += 2; // below left
            if (data[(open + 1) % 3 + 6]) openCase += 4; // above right
            if (data[(open + 2) % 3 + 6]) openCase += 8; // above left
            switch (openCase) {
                case 15: Create(CaveGrid.I.cornerShelf, yRot, false, false, Four((open+2)%3+1, (open+1)%3+1, (open+2)%3+1, (open+1)%3+1)); break;
                case 14: Create(CaveGrid.I.cornerTightstair, yRot, false, true, Four((open+2)%3+1, (open+1)%3+1, (open+2)%3+1, (open)%3+1)); break;
                case 13: Create(CaveGrid.I.cornerTightstair, yRot, true, true, Four((open+1)%3+1, (open+2)%3+1, (open+1)%3+1, (open)%3+1)); break;
                case 12: Create(CaveGrid.I.cornerGutter, yRot, false, false, Four((open+2)%3+1, (open)%3+1, (open+1)%3+1)); break;
                case 11: Create(CaveGrid.I.cornerTightstair, yRot, false, false, Four((open+2)%3+1, (open+1)%3+1, (open+2)%3+1, (open)%3+1)); break;
                case 10: Create(CaveGrid.I.cornerShelfwall, yRot, false, false, Four((open+2)%3+1, (open)%3+1, (open+2)%3+1)); break;
                case 9: Create(CaveGrid.I.cornerStair, yRot, false, false, Four((open)%3+1, (open+1)%3+1, (open+2)%3+1, (open)%3+1)); break;
                case 8: Create(CaveGrid.I.cornerBroadstair, yRot, false, false, Four((open+2)%3+1, 0,(open)%3+1)); break;
                case 7: Create(CaveGrid.I.cornerTightstair, yRot, true, false, Four((open+1)%3+1, (open+2)%3+1, (open+1)%3+1, (open)%3+1)); break;
                case 6: Create(CaveGrid.I.cornerStair, yRot, false, true, Four((open)%3+1, (open+1)%3+1, (open+2)%3+1, (open)%3+1)); break;
                case 5: Create(CaveGrid.I.cornerShelfwall, yRot, true, false, Four((open+1)%3+1, (open)%3+1, (open+1)%3+1)); break;
                case 4: Create(CaveGrid.I.cornerBroadstair, yRot, true, false, Four((open+1)%3+1, 0,(open)%3+1)); break;
                case 3: Create(CaveGrid.I.cornerGutter, yRot, false, true, Four((open+2)%3+1, (open)%3+1, (open+1)%3+1)); break;
                case 2: Create(CaveGrid.I.cornerBroadstair, yRot, false, true, Four((open+2)%3+1, 0,(open)%3+1)); break;
                case 1: Create(CaveGrid.I.cornerBroadstair, yRot, true, true, Four((open+1)%3+1, 0,(open)%3+1)); break;
                case 0: Create(CaveGrid.I.corner, yRot, false, false, Four(open+1)); break;
            }
        } else {
            int wall = !data[3] ? 0 : !data[4] ? 1 : 2;
            int yRot = 30 - 120 * wall;
            bool openBelow = data[wall];
            bool openAbove = data[wall + 6];
            if (openAbove) {
                if (openBelow) Create(CaveGrid.I.revcornerShelf, yRot, false, false, Four((wall)%3+1, (wall)%3+1));
                else Create(CaveGrid.I.revcornerGutter, yRot, false, false, Four((wall+2)%3+1, (wall)%3+1, (wall+1)%3+1));
            } else {
                if (openBelow) Create(CaveGrid.I.revcornerGutter, yRot, false, true, Four((wall+2)%3+1, (wall)%3+1, (wall+1)%3+1));
                else Create(CaveGrid.I.revcorner, yRot, false, false, Four((wall+2)%3+1, (wall+1)%3+1));
            }
        }

    }

    public void MaybeCreateFloor(bool a, bool b, bool c, bool flipped) {
        if (!a && !b && !c) return;
        if (a && b && c) {
            Create(CaveGrid.I.floor, -90, false, flipped, Four(1, 2, 3));
        } else if (a ^ b ^ c) {
            int yRot = a ? 30 : b ? -90 : -210;
            Create(CaveGrid.I.cornerFloor, yRot, false, flipped, Four(0, 0, 0, a ? 1 : b ? 2 : 3));
        } else {
            int yRot = !a ? 30 : !b ? -90 : -210;
            Create(CaveGrid.I.revcornerFloor, yRot, false, flipped, !a ? Four(3, 2) : !b ? Four(1, 3) : Four(2, 1));
        }
    }

    public Transform Create(GameObject prefab, int yRot, bool xFlip, bool yFlip, int[] sources) {
        Transform newPiece = GameObject.Instantiate(prefab, transform).transform;
        newPiece.localRotation = Quaternion.Euler(0, yRot, 0);
        newPiece.localScale = Vector3.Scale(new Vector3(xFlip ? -1 : 1, yFlip ? -1 : 1, 1), CaveGrid.I.scale);
        SetMaterial(newPiece, sources);
        return newPiece;
    }

    private int[] Four(params int[] values) {
        int[] result = new int[4];
        int i = 0;
        for (; i < values.Length; i++) result[i] = values[i];
        for (; i < 4; i++) result[i] = 0;
        return result;
    }

    private void SetMaterial(Transform newPiece, int[] src) {
        Color[] floors = CaveGrid.Biome.GetFloors(pos);
        Color[] walls = CaveGrid.Biome.GetWalls(pos);
        Material material = new Material(CaveGrid.I.defaultMaterial);
        material.SetColor("_Color1", floors[src[0]]);
        material.SetColor("_Color2", floors[src[1]]);
        material.SetColor("_Color3", floors[src[2]]);
        material.SetColor("_Color4", floors[src[3]]);
        material.SetColor("_Walls1", walls[src[0]]);
        material.SetColor("_Walls2", walls[src[1]]);
        material.SetColor("_Walls3", walls[src[2]]);
        material.SetColor("_Walls4", walls[src[3]]);
        newPiece.GetComponent<MeshRenderer>().material = material;
    }

    public static int BoolArrayToInt(bool[] arr) {
        int result = 0;
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i]) result |= 1 << i;
        }
        return result;
    }
}
