using System;
using UnityEngine;

public class LevelEditorCavePiece : MonoBehaviour {
    public Sprite sprite;

    [NonSerialized] public LevelEditorCavePiece[] attachedPieces;
    [NonSerialized] public Transform[] exits;

    void Awake() {
        Transform exitsParent = transform.Find("Exits");
        attachedPieces = new LevelEditorCavePiece[exitsParent.childCount];
        exits = new Transform[exitsParent.childCount];
        for (int i = 0; i < exitsParent.childCount; i++) {
            exits[i] = exitsParent.GetChild(i);
        }
    }
}
