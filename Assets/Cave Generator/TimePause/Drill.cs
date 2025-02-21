using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class Drill : MonoBehaviour {
    public LineRenderer hint;

    private Transform guide;

    void Start() {
        guide = GameObject.FindObjectOfType<RandomWalk>().transform;
    }

    public void Activate() {
        foreach (GridPos pos in GridPos.FromWorld(guide.position).Line(GridPos.FromWorld(transform.position) + GridPos.up * 1))
            CaveGrid.I.SetPos(CaveGrid.Mod.Cave(pos));
    }

    void UpdateHeldState(float heldState) {
        hint.enabled = heldState != 0f;
    }

    void Use() {
        Activate();
        transform.parent.parent.parent.GetComponent<HoldObject>().OnDropObject(gameObject, false);
        Destroy(gameObject);
    }

    void Update() {
        if (hint.enabled) {
            hint.SetPositions(new Vector3[] {
                Vector3.zero,
                transform.InverseTransformPoint(guide.position)});
        }
    }
}
