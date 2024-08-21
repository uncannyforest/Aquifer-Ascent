using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchMe : MonoBehaviour {
    private RandomWalk rw;
    private bool inBurst = true; // start with fake burst . . .

    void Start() {
        rw = transform.GetComponentStrict<RandomWalk>();
        Invoke("EndBurst", rw.modRate * 3); // . . . to give time to get away
    }

    void OnTriggerEnter(Collider collider) {
        if (!inBurst && collider.gameObject.tag == "Player") {
            Scarf scarf = collider.gameObject.GetComponentStrict<Scarf>();
            scarf.AddToCollection(scarf.MaxScarfFound + 1);
            scarf.SwapScarf(true, out int oldScarf, out int newScarf);
            inBurst = true;
            Invoke("EndBurst", rw.modRate * 2);
        }
    }

    private void EndBurst() {
        inBurst = false;
    }
}
