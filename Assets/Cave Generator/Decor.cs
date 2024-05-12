using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Decor : MonoBehaviour {
    public GameObject deadTree;
    public float deadTreeRate = 6;
    public float positionOffsetRelHoriz = .5f;
    public float positionOffsetAbsY = -.25f;

    private int deadTreeCount = 0;

    public void UpdatePos(TriPos tri, GridPiece gridPiece) {
        List<int> validPositions = new List<int>();
        GridPos[] corners = tri.HorizCorners;
        for (int i = 0; i < 3; i++)
            if (!CaveGrid.I.grid[corners[i]] && CaveGrid.I.grid[corners[i] + GridPos.up])
                validPositions.Add(i);
        if (validPositions.Count > 0 && deadTreeCount++ % deadTreeRate == 0) {
            int chosenPosition = Randoms.InList(validPositions);
            GameObject newDeadTree = GameObject.Instantiate(deadTree,
                tri.World + tri.CornersRelative[chosenPosition] * positionOffsetRelHoriz + Vector3.up * positionOffsetAbsY,
                Quaternion.identity, gridPiece.transform);
            if (CaveGrid.I.grid[corners[chosenPosition] + GridPos.up]
                    && CaveGrid.I.grid[corners[chosenPosition] + 2 * GridPos.up]
                    && CaveGrid.I.grid[corners[chosenPosition] + 3 * GridPos.up]
                    && CaveGrid.I.grid[corners[chosenPosition] + 4 * GridPos.up]) {
                Vector3 scale = newDeadTree.transform.localScale;
                scale.y *= 2;
                scale.x *= 1.5f;
                scale.z *= 1.5f;
                newDeadTree.transform.localScale = scale;
                Debug.Log("First??");
            }
        }
    }
}
