using System.Collections;
using UnityEngine;

public class RandomWalk : MonoBehaviour {
    public GameObject orbPrefab;
    public Transform orbParent;
    public float modRate = 1f;
    public float slowDown = 1/18f;
    public int addOrbEvery = 18;

    private Vector3 prevLoc = Vector3.zero;
    private Vector3 nextLoc = Vector3.zero;
    private float progress = 0;

    public void Awake() {
        StartCoroutine(Runner());
    }

    public IEnumerator Runner() {
        GridPos position = GridPos.zero;
        int count = 0;
        while (true) {
            if (!CaveGrid.I.grid[position]) {
                CaveGrid.I.SetPos(position, true);
                if (count % addOrbEvery == 0) GameObject.Instantiate(orbPrefab, position.World, Quaternion.identity, orbParent);
                count++;
            }
            prevLoc = nextLoc;
            nextLoc = position.World;
            progress = 0;
            yield return new WaitForSeconds(modRate);
            GridPos random = GridPos.Random;
            position += random;
            modRate += slowDown;
            // Debug.Log("Random step: " + random + "; new position: " + position + " at " + position.World);
        }
    }

    void Update() {
        progress += Time.deltaTime / modRate;
        transform.position = Vector3.Lerp(prevLoc, nextLoc, CubicInterpolate(progress));
    }

    private static float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }
}
