using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObjectDestroyer {
    public static void Destroy(GameObject gameObject) {
        TriggerExit[] allGameTriggers = Object.FindObjectsOfType<TriggerExit>();
        Collider[] gameObjectColliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (TriggerExit trigger in allGameTriggers) {
            foreach (Collider collider in gameObjectColliders) {
                trigger.SendTriggerExit(collider);
            }
        }

        GameObject.Destroy(gameObject);
    }
}
