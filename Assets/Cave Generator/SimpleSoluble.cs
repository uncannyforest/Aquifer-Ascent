using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSoluble : MonoBehaviour {
    public GridPos pos;

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "Player") {
            // CaveGrid.I.SetPos(CaveGrid.Mod.Cave(GridPos.FromWorld(transform.position)));
            // GameObject.Destroy(gameObject);

            CaveGrid.I.soft[pos] = false;
            CaveGrid.I.SetPos(CaveGrid.Mod.Cave(pos));
        }
    }
}
