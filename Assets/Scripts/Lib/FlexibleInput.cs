using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleInput {
    private HoldObject script;

    public FlexibleInput(HoldObject script) {
        this.script = script;
    }

    public void UpdateDisplayForNearbyObjects(HashSet<GameObject> nearObjects) {
        if (script.IsHolding) {
            return; // no state change of IsHolding, nothing to do
        }

        if (nearObjects.Count > 0) {
            SetInteractionMessages("beckon", null);
        } else {
            SetInteractionMessages(null, null);
        }
    }

    public void UpdateDisplayForHeldObject(GameObject heldObject) {
        string interact2 = heldObject.GetComponent<Holdable>().optionalAction;

        SetInteractionMessages("release", interact2);
    }

    void SetInteractionMessages(string interact1, string interact2) {

#if (UNITY_IOS || UNITY_ANDROID)
        GameObject interactNotice1 = GameObject.Find("Mobile")
                .transform.Find("Interact 1/Notice").gameObject;
        GameObject interactNotice2 = GameObject.Find("Mobile")
                .transform.Find("Interact 2/Notice").gameObject;
#else
        GameObject interactNotice1 = GameObject.Find("Nonmobile")
                .transform.Find("Interact Notice 1").gameObject;
        GameObject interactNotice2 = GameObject.Find("Nonmobile")
                .transform.Find("Interact Notice 2").gameObject;
#endif 

        SetInteractionMessage(interactNotice1, interact1, "x");
        SetInteractionMessage(interactNotice2, interact2, "z");
    }

    void SetInteractionMessage(GameObject interactNotice, string message, string desktopKey) {
        if (!string.IsNullOrEmpty(message)) {
            interactNotice.SetActive(true);
            
#if (UNITY_IOS || UNITY_ANDROID)
            message = char.ToUpper(message[0]) + message.Substring(1);
#else
            message = "press <color=white>" + desktopKey + "</color> to <color=white>"
                + message + "</color>";
#endif 

            interactNotice.transform.Find("Text").gameObject.GetComponent<Text>().text = message;
        } else {
            interactNotice.SetActive(false);
        }

    }
}
