using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decor : MonoBehaviour {
    public GameObject deadTree;
    public float deadTreeRate = 6;

    private int deadTreeCount = 0;

    public void UpdatePos(TriPos tri, bool[] data, GridPiece gridPiece) {
        if (!data[3] && !data[4] && !data[5] && data[6] && data[7] && data[8] && deadTreeCount++ % deadTreeRate == 0) {
            GameObject newDeadTree = GameObject.Instantiate(deadTree, tri.World, Quaternion.identity, gridPiece.transform);
        }
    }
}
