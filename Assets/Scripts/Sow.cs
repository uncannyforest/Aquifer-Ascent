using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sow : MonoBehaviour
{
    void Use()
    {
        transform.parent.parent.GetComponent<HoldObject>().OnDropObject(gameObject, true);
    }
}
