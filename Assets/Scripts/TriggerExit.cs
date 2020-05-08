using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerExit : MonoBehaviour
{
    public void SendTriggerExit(Collider other) {
        Debug.Log("HIIIII" + other);
        SendMessage("OnTriggerExit", other);
    }
}
