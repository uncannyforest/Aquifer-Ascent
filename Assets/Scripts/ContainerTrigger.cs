using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTrigger : ToggleableScript {
    private bool isActive;

    override public bool IsActive {
        set {
            isActive = value;
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

    public void Refresh() {
        IsActive = isActive;
    }
}
