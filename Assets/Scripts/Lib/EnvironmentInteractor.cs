using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> holds code governing interaction with other game objects </summary>
public class EnvironmentInteractor {
    private HoldObject script;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();

    public HashSet<GameObject> NearObjects {
        get => nearObjects;
    }

    public EnvironmentInteractor(HoldObject script) {
        this.script = script;
    }

    public void AddInteractableObject(GameObject trigger) {
        nearObjects.Add(GetInteractableObject(trigger));
    }

    public void RemoveInteractableObject(GameObject trigger) {
        nearObjects.Remove(GetInteractableObject(trigger));
    }

    private GameObject GetInteractableObject(GameObject trigger) {
        if(trigger.GetComponent<Holdable>() != null) {
            return trigger;
        } else if (trigger.transform.parent.GetComponent<Holdable>() != null) {
            return trigger.transform.parent.gameObject;
        } else {
            Debug.LogError("Object tagged CanPickUp has no Holdable script on it or parent");
            return null;
        }
    }

    public void HoldClosestObject() {
        if (nearObjects.Count == 0) {
            return;
        }

        GameObject closestObject = nearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, script.transform.position)
            ).First();

        closestObject.GetComponent<Holdable>().Hold();
    }

    public void DropHeldObject(Transform playerHoldTransform) {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            childPickMeUp.Drop();
        }
    }

    public void NotifyHeldObjectReadyToDrop(Transform playerHoldTransform) {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            childPickMeUp.FinishDrop();
        }
    }

    public void UseHeldObject(Transform playerHoldTransform) {
        foreach (Transform child in playerHoldTransform) {
            child.SendMessage("Use");
        }
    }

}
