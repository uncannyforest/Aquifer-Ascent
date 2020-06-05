using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerTrigger : ToggleableScript {
    public ToggleableScript receivingScript;

    override public bool IsActive {
        set {
            if (receivingScript != null && receivingScript.transform.parent == transform) {
                receivingScript.IsActive = value;
            }
        }
    }
}
