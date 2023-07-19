using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Solvent : MonoBehaviour {
    public Transform modLocation;

    void Update() {
        GridPos checkPosition = GridPos.FromWorld(transform.position);
        GridPos modPosition = GridPos.FromWorld(modLocation.position);
        if (!CaveGrid.I.grid[checkPosition] && !CaveGrid.I.grid[modPosition]) CaveGrid.I.SetPos(modPosition, true);
    }
}
