using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Takes IsActive as output.
public abstract class ToggleableScript : MonoBehaviour {
    public abstract bool IsActive { set; get; }
}