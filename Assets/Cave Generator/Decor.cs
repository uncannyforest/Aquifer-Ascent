using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Decor : MonoBehaviour {
    public GameObject deadTree;
    public float deadTreeRate = 6;
    public int nothingRate = 3;
    public float sameDecorRate = 2/3f;
    public float positionOffsetRelHoriz = .5f;
    public float positionOffsetAbsY = -.25f;

    private int deadTreeCount = 0;
    private int lastBiome = 0;
    private int lastDecor = -1;

    public void UpdatePos(TriPos tri, GridPiece gridPiece) {
        List<int> validPositions = new List<int>();
        GridPos[] corners = tri.HorizCorners;
        for (int i = 0; i < 3; i++)
            if (!CaveGrid.I.grid[corners[i]] && CaveGrid.I.grid[corners[i] + GridPos.up])
                validPositions.Add(i);
        if (validPositions.Count > 0) {
            if (deadTreeCount++ % deadTreeRate == 0) {
                int chosenPosition = Randoms.InList(validPositions);
                GameObject newDeadTree = GameObject.Instantiate(deadTree,
                    tri.World + tri.CornersRelative[chosenPosition] * positionOffsetRelHoriz + Vector3.up * positionOffsetAbsY,
                    Quaternion.identity, gridPiece.transform);
                if (CaveGrid.I.grid[corners[chosenPosition] + 2 * GridPos.up]
                        && CaveGrid.I.grid[corners[chosenPosition] + 3 * GridPos.up]
                        && CaveGrid.I.grid[corners[chosenPosition] + 4 * GridPos.up]) {
                    Vector3 scale = newDeadTree.transform.localScale;
                    scale.y *= 2.5f;
                    scale.x *= 1.5f;
                    scale.z *= 1.5f;
                    newDeadTree.transform.localScale = scale;
                    Debug.Log("First??");
                }
            } else {
                int chosenPosition = Randoms.InList(validPositions);
                int newBiome = CaveGrid.Biome[corners[chosenPosition]];
                if (newBiome != lastBiome) {
                    lastBiome = newBiome;
                    lastDecor = -1;
                }
                Biome biome = CaveGrid.Biome.decor[newBiome];
                int shortFactor = biome.decorFloor.Length;
                int tallFactor = CaveGrid.I.grid[corners[chosenPosition] + 2 * GridPos.up]
                    && CaveGrid.I.grid[corners[chosenPosition] + 3 * GridPos.up]
                    && CaveGrid.I.grid[corners[chosenPosition] + 4 * GridPos.up]
                    ? biome.decorTallFloor.Length : 0;
                int seed = Random.Range(0, shortFactor + tallFactor + nothingRate);
                if (seed < shortFactor) {
                    if (lastDecor >= 0 && lastDecor < shortFactor && Random.value < sameDecorRate)
                        seed = lastDecor;
                    GameObject newThing = GameObject.Instantiate(biome.decorFloor[seed],
                        tri.World + tri.CornersRelative[chosenPosition] * positionOffsetRelHoriz + Vector3.up * positionOffsetAbsY,
                        Quaternion.identity, gridPiece.transform);
                } else if (seed < shortFactor + tallFactor) {
                    if (lastDecor >= 0 && lastDecor < shortFactor + tallFactor && Random.value < sameDecorRate)
                        seed = lastDecor;
                    GameObject newThing = GameObject.Instantiate(biome.decorTallFloor[seed - shortFactor],
                        tri.World + tri.CornersRelative[chosenPosition] * positionOffsetRelHoriz + Vector3.up * positionOffsetAbsY,
                        Quaternion.identity, gridPiece.transform);
                }
            }
        }
    }
}
