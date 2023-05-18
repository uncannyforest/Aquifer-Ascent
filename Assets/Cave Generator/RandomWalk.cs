using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalk : MonoBehaviour {
    public GameObject orbPrefab;
    public Transform orbParent;
    public float modRate = 1f;
    public float slowDown = 1/18f;
    public int addOrbEvery = 18;
    public int changeBiomeEvery = 18;

    private Vector3 prevLoc = Vector3.zero;
    private Vector3 nextLoc = Vector3.zero;
    private float progress = 0;

    public void Awake() {
        StartCoroutine(Runner());
    }

    class ModInteger {
        public int value;
        public ModInteger(int value) {
            this.value = value;
        }
        public static implicit operator int(ModInteger mi) => mi.value;
    }

    public IEnumerator Runner() {
        GridPos position = GridPos.zero;
        int count = 0;
        int biome = 1;
        ModInteger nextBiomeCount = new ModInteger(changeBiomeEvery);
        
        while (true) {
            if (!CaveGrid.I.grid[position]) {
                biome = CaveGrid.Biome.Next(position, NextBiome(biome, nextBiomeCount));
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

    private Func<int> NextBiome(int prevBiome, ModInteger nextBiomeCount) {
        return () => {
            nextBiomeCount.value--;
            if (nextBiomeCount.value == 0) {
                nextBiomeCount.value = changeBiomeEvery;
                return Random.Range(1, CaveGrid.Biome.floors.Length);
            } else return prevBiome;
        };
    }

    private static float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }
}
