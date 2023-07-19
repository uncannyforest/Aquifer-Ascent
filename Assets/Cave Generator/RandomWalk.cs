using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomWalk : MonoBehaviour {
    public StandardOrb orbPrefab;
    public Transform orbParent;
    public int interiaOfEtherCurrent = 6;
    public float modRate = 2/3f;
    public float slowDown = 1/18f;
    public int addOrbEvery = 18;
    public int changeBiomeEvery = 18;
    public float cheatSlowdown = 4;
    
    private Vector3 prevLoc = Vector3.zero;
    private Vector3 nextLoc = Vector3.zero;
    private float progress = 0;
    private bool cheat = false;
    private Image cheatButton = null;

    private InDarkness darkness;
    private Vector3 etherCurrent;

    public void Awake() {
        darkness = GetComponentInChildren<InDarkness>();
        StartCoroutine(Runner());
        GameObject cheatButtonGo = GameObject.Find("Cheat");
        if (cheatButtonGo != null) cheatButton = cheatButtonGo.GetComponent<Image>();
    }
    
    class ModInteger {
        public int value;
        public ModInteger(int value) {
            this.value = value;
        }
        public static implicit operator int(ModInteger mi) => mi.value;
    }

    public IEnumerator Runner() {
        int count = addOrbEvery;
        int biome = 1;
        ModInteger nextBiomeCount = new ModInteger(changeBiomeEvery);
        
        foreach (RandomWalkAlgorithm.Output step in RandomWalkAlgorithm.EnumerateSteps(interiaOfEtherCurrent)) {
            foreach (GridPos position in step.newCave) if (!CaveGrid.I.grid[position]) {
                biome = CaveGrid.Biome.Next(position, NextBiome(biome, nextBiomeCount));
                CaveGrid.I.SetPos(position, true);
            }
            // if (step.newCave.Length > 0 && count++ % addOrbEvery == 0) {
            if (darkness.IsInDarkness) count++;
            else count = 0;
            if (count >= addOrbEvery) {
                StandardOrb orb = GameObject.Instantiate(orbPrefab, step.location, Quaternion.identity, orbParent);
                if (cheat) orb.chargeTime *= cheatSlowdown;
                orb.pleaseNeverHoldMe = false;
                orb.IsHoldable = true;
            }
            foreach (GridPos interesting in step.interesting) {
                StandardOrb orb = GameObject.Instantiate(orbPrefab, interesting.World, Quaternion.identity, orbParent);
                if (cheat) {
                    orb.chargeTime *= cheatSlowdown;
                    orb.pleaseNeverHoldMe = false;
                    orb.IsHoldable = true;
                }
            }
            etherCurrent = step.etherCurrent;
            prevLoc = nextLoc;
            nextLoc = step.location;
            progress = 0;
            yield return new WaitForSeconds(modRate);
            modRate += slowDown;
        }
    }

    void Update() {
        progress += Time.deltaTime / modRate;
        transform.position = Vector3.Lerp(prevLoc, nextLoc, CubicInterpolate(progress));
        if (etherCurrent.y > .5f) {
            etherCurrent = new Vector3(etherCurrent.x, 0, etherCurrent.z);
            Debug.DrawLine(transform.position, transform.position + etherCurrent, Color.magenta, 600);
        } else
            Debug.DrawLine(transform.position, transform.position + etherCurrent, Color.magenta);

        if (SimpleInput.GetButtonDown("Cheat")) {
            cheat = !cheat;
            if (cheat) {
                modRate *= cheatSlowdown;
                if (cheatButton != null) cheatButton.color = Color.white;
            } else {
                modRate /= cheatSlowdown;
                if (cheatButton != null) cheatButton.color = Color.grey;
            }
        }
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
