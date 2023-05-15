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
            int openCorner = data[3] ? 0 : data[4] ? 1 : 2;
            int yRot = 30 - 120 * openCorner;
            int openCase = 0;
            if (data[(openCorner + 1) % 3]) openCase += 1; // below right
            if (data[(openCorner + 2) % 3]) openCase += 2; // below left
            if (data[(openCorner + 1) % 3 + 6]) openCase += 4; // above right
            if (data[(openCorner + 2) % 3 + 6]) openCase += 8; // above left
            switch (openCase) {
                case 15: Create(CaveGrid.I.cornerShelf, yRot, false, false); break;
                case 14: Create(CaveGrid.I.cornerTightstair, yRot, false, true); break;
                case 13: Create(CaveGrid.I.cornerTightstair, yRot, true, true); break;
                case 12: Create(CaveGrid.I.cornerGutter, yRot, false, false); break;
                case 11: Create(CaveGrid.I.cornerTightstair, yRot, false, false); break;
                case 10: Create(CaveGrid.I.cornerShelfwall, yRot, false, false); break;
                case 9: Create(CaveGrid.I.cornerStair, yRot, false, false); break;
                case 8: Create(CaveGrid.I.cornerBroadstair, yRot, false, false); break;
                case 7: Create(CaveGrid.I.cornerTightstair, yRot, true, false); break;
                case 6: Create(CaveGrid.I.cornerStair, yRot, false, true); break;
                case 5: Create(CaveGrid.I.cornerShelfwall, yRot, true, false); break;
                case 4: Create(CaveGrid.I.cornerBroadstair, yRot, true, false); break;
                case 3: Create(CaveGrid.I.cornerGutter, yRot, false, true); break;
                case 2: Create(CaveGrid.I.cornerBroadstair, yRot, false, true); break;
                case 1: Create(CaveGrid.I.cornerBroadstair, yRot, true, true); break;
                case 0: Create(CaveGrid.I.corner, yRot, false, false); break;
            }
        } else {
            int wallCorner = !data[3] ? 0 : !data[4] ? 1 : 2;
            int yRot = 30 - 120 * wallCorner;
            bool openBelow = data[wallCorner];
            bool openAbove = data[wallCorner + 6];
            if (openAbove) {
                if (openBelow) Create(CaveGrid.I.revcornerShelf, yRot, false, false);
                else Create(CaveGrid.I.revcornerGutter, yRot, false, false);
            } else {
                if (openBelow) Create(CaveGrid.I.revcornerGutter, yRot, false, true);
                else Create(CaveGrid.I.revcorner, yRot, false, false);
            }
        }

    }

    public void MaybeCreateFloor(bool a, bool b, bool c, bool flipped) {
        if (!a && !b && !c) return;
        if (a && b && c) {
            Create(CaveGrid.I.floor, -90, false, flipped);
        } else if (a ^ b ^ c) {
            int yRot = a ? 30 : b ? -90 : -210;
            Create(CaveGrid.I.cornerFloor, yRot, false, flipped);
        } else {
            int yRot = !a ? 30 : !b ? -90 : -210;
            Create(CaveGrid.I.revcornerFloor, yRot, false, flipped);
        }
    }

    public Transform Create(GameObject prefab, int yRot, bool xFlip, bool yFlip) {
        Transform newPiece = GameObject.Instantiate(prefab, transform).transform;
        newPiece.localRotation = Quaternion.Euler(0, yRot, 0);
        newPiece.localScale = new Vector3(xFlip ? -1 : 1, yFlip ? -1 : 1, 1);
        newPiece.GetComponent<MeshRenderer>().material = CaveGrid.I.defaultMaterial;
        return newPiece;
    }

    public static int BoolArrayToInt(bool[] arr) {
        int result = 0;
        for (int i = 0; i < arr.Length; i++) {
            if (arr[i]) result |= 1 << i;
        }
        return result;
    }
}
