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
            SetInteractionMessage("beckon", null);
        } else {
            SetInteractionMessage(null, null);
        }
    }

    public void UpdateDisplayForHeldObject(GameObject heldObject) {
        SetInteractionMessage("release", null);
    }

    void SetInteractionMessage(string interact1, string interact2) {

#if (UNITY_IOS || UNITY_ANDROID)
        GameObject interactNotice = GameObject.Find("Mobile")
                .transform.Find("Interact/Notice").gameObject;
#else
        GameObject interactNotice = GameObject.Find("Nonmobile")
                .transform.Find("Interact Notice").gameObject;
#endif 

        if (interact1 != null) {
            interactNotice.SetActive(true);
            
#if (UNITY_IOS || UNITY_ANDROID)
            interactionMessage = char.ToUpper(interact1[0]) + interact1.Substring(1);
#else
            interact1 = "press <color=white>x</color> to <color=white>"
                + interact1 + "</color>";
#endif 
            interactNotice.transform.Find("Text").gameObject.GetComponent<Text>().text = interact1;
        } else {
            interactNotice.SetActive(false);
        }
    }
}
