using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSoluble : MonoBehaviour {
    public List<GridPos> positions;

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player") {
            // CaveGrid.I.SetPos(CaveGrid.Mod.Cave(GridPos.FromWorld(transform.position)));
            // GameObject.Destroy(gameObject);

            foreach (GridPos pos in positions) {
                CaveGrid.I.soft[pos] = false;
                CaveGrid.I.SetPos(CaveGrid.Mod.Cave(pos));
            }
        }
    }
}
