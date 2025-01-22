using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> holds code governing interaction with other game objects </summary>
public class EnvironmentInteractor {
    private HoldObject script;
    private Transform playerHoldTransform;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();

    public HashSet<GameObject> NearObjects {
        get {
            nearObjects.RemoveWhere(go => go == null || go.tag != "CanPickUp");
            return nearObjects;
        }
    }

    public EnvironmentInteractor(HoldObject script, Transform playerHoldTransform) {
        this.script = script;
        this.playerHoldTransform = playerHoldTransform;
    }

    public void AddInteractableObject(GameObject trigger) {
        nearObjects.Add(trigger);
    }

    public void RemoveInteractableObject(GameObject trigger) {
        nearObjects.Remove(trigger);
    }

    private GameObject GetInteractableObject(GameObject trigger) {
        if (trigger.GetComponent<Holdable>() != null) {
            return trigger;
        } else if (trigger.transform.parent.GetComponent<Powerup>() != null || trigger.transform.parent.GetComponent<Holdable>() != null) {
            return trigger.transform.parent.gameObject;
        } else {
            Debug.LogError("Object tagged CanPickUp has no Holdable script on it or parent");
            return null;
        }
    }

    public void HoldClosestObject() {
        if (NearObjects.Count == 0) {
            return;
        }

        GameObject closestObject = NearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, script.transform.position)
            ).First();

        GameObject io = GetInteractableObject(closestObject);
        if (io.GetComponent<Holdable>() != null) io.GetComponent<Holdable>().Hold();
        else io.GetComponent<Powerup>().Add();
    }

    public void DropHeldObject() {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            childPickMeUp.Drop();
        }
    }

    public void NotifyHeldObjectReadyToDrop() {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            childPickMeUp.FinishDrop();
        }
    }

    public void UseHeldObject() {
        foreach (Transform child in playerHoldTransform) {
            child.SendMessage("Use");
        }
    }


}
