using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LevelEditor : MonoBehaviour {
    public LevelEditorCavePiece[] cavePieces;
    public Transform panel;
    public Image panelSpritePrefab;
    public Vector3 junctionRotation = new Vector3(0, 180, 0);

    private Dictionary<GameObject, LevelEditorCavePiece> iconsToPrefabs = new Dictionary<GameObject, LevelEditorCavePiece>();

    private LevelEditorCavePiece prevPiece;
    private int prevExit;
    private LevelEditorCavePiece nextPiece;
    private int nextExit;

    void Start() {
        foreach (LevelEditorCavePiece piece in cavePieces) {
            Image sprite = GameObject.Instantiate(panelSpritePrefab, panel);
            sprite.sprite = piece.sprite;
            iconsToPrefabs[sprite.gameObject] = piece;
            sprite.GetComponent<Button>().onClick.AddListener(SelectPrefabAction(piece));
        }

        prevPiece = GameObject.Instantiate(cavePieces[0]);
        prevExit = 0;
    }

    UnityAction SelectPrefabAction(LevelEditorCavePiece prefab) {
        return () => SelectPrefab(prefab);
    }
    void SelectPrefab(LevelEditorCavePiece prefab) {
        if (nextPiece) GameObject.Destroy(nextPiece.gameObject);
        nextPiece = GameObject.Instantiate(prefab);
        nextExit = 0;
        Vector3 nextPositionRelativeToNextExit = Quaternion.Euler(junctionRotation) * nextPiece.exits[nextExit].transform.InverseTransformPoint(Vector3.zero);
        Quaternion nextRotationRelativeToNextExit = Quaternion.Euler(junctionRotation) * Quaternion.Inverse(nextPiece.exits[nextExit].rotation);
        nextPiece.transform.position = prevPiece.exits[prevExit].TransformPoint(nextPositionRelativeToNextExit);
        nextPiece.transform.rotation = prevPiece.exits[prevExit].rotation * nextRotationRelativeToNextExit;
    }
}
