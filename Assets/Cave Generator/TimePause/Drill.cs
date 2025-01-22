using UnityEngine;

public class Drill {
    public static void Activate() {
        Vector3 player = GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector3 guide = GameObject.FindObjectOfType<RandomWalk>().transform.position;
        foreach (GridPos pos in GridPos.FromWorld(guide).Line(GridPos.FromWorld(player) + GridPos.up * 2))
            CaveGrid.I.SetPos(CaveGrid.Mod.Cave(pos));
    }
}
