using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTrigger : ToggleableScript {
    override public bool IsActive {
        set {
            foreach (Transform child in transform) {
                if (value) {
                    child.gameObject.SetActive(true);
                    child.BroadcastMessage("NotifyActivate");
                } else {
                    child.BroadcastMessage("NotifyDeactivate");
                }
            }
        }
    }
}
