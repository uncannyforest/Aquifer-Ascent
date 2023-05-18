using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Decor : MonoBehaviour {
    public GameObject deadTree;
    public float deadTreeRate = 6;
    public float positionOffsetRelHoriz = .5f;
    public float positionOffsetAbsY = -.25f;

    private int deadTreeCount = 0;

    public void UpdatePos(TriPos tri, bool[] data, GridPiece gridPiece) {
        List<int> validPositions = new List<int>();
        foreach (int i in new int[] {0, 1, 2})
            if (!data[i + 3] && data[i + 6])
                validPositions.Add(i);
        if (validPositions.Count > 0 && deadTreeCount++ % deadTreeRate == 0) {
            int chosenPosition = validPositions[Random.Range(0, validPositions.Count)];
            GameObject newDeadTree = GameObject.Instantiate(deadTree,
                tri.World + tri.CornersRelative[chosenPosition] * positionOffsetRelHoriz + Vector3.up * positionOffsetAbsY,
                Quaternion.identity, gridPiece.transform);
        }
    }
}
