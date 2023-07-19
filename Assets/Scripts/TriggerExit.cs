using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerExit : MonoBehaviour {
    public void SendTriggerExit(Collider other) {
        SendMessage("OnTriggerExit", other);
    }

    public void SendTriggerEnter(Collider other) {
        SendMessage("OnTriggerEnter", other);
    }
}
