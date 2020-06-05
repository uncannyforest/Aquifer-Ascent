using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Offers IsActive as input.
public abstract class BooleanScript : MonoBehaviour {
    public abstract bool IsActive { get; }
}