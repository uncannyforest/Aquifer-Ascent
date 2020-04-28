using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> holds code governing interaction with other game objects </summary>
public class EnvironmentInteractor {
    private HoldObject script;

    public EnvironmentInteractor(HoldObject script) {
        this.script = script;
    }

    public GameObject GetInteractableObject(GameObject trigger) {
        if(trigger.GetComponent<Holdable>() != null) {
            return trigger;
        } else if (trigger.transform.parent.GetComponent<Holdable>() != null) {
            return trigger.transform.parent.gameObject;
        } else {
            Debug.LogError("Object tagged CanPickUp has no Holdable script on it or parent");
            return null;
        }
    }

    public GameObject HoldClosestObject(IEnumerable<GameObject> nearObjects) {
        GameObject closestObject = nearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, script.transform.position)
            ).First();

        closestObject.GetComponent<Holdable>().PickUp();

        return closestObject;
    }

    public void DropHeldObject(Transform playerHoldTransform) {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            if (childPickMeUp == null) {
                Debug.LogWarning("Child of playerHold had no PickMeUp script!");
            } else {
                childPickMeUp.SetDown();
            }
        }
    }

}
