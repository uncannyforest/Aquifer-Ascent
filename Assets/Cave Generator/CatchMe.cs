using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatchMe : MonoBehaviour {
    public float burstSize = 12;

    private RandomWalk rw;
    private float rwStepTime;
    private bool inBurst = true; // start with fake burst . . .

    void Start() {
        rw = transform.GetComponentStrict<RandomWalk>();
        rwStepTime = rw.modRate;
        Invoke("EndBurst", rwStepTime * 3); // . . . to give time to get away
    }

    void OnTriggerEnter(Collider collider) {
        if (!inBurst && collider.gameObject.tag == "Player") {
            Scarf scarf = collider.gameObject.GetComponentStrict<Scarf>();
            scarf.AddToCollection(scarf.MaxScarfFound + 1);
            scarf.SwapScarf(true, out int oldScarf, out int newScarf);
            rw.modRate = rwStepTime / burstSize;
            inBurst = true;
            Invoke("EndBurst", rwStepTime * 2);
        }
    }

    private void EndBurst() {
        rw.modRate = rwStepTime;
        inBurst = false;
    }
}
