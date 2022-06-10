using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Scarf Materials List")]
public class ScarfMaterialsList : ScriptableObject {
    public Material[] materials;

    public int Count => materials.Length;
    public Material this[int i] => materials[i];
}