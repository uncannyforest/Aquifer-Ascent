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
    public int maxAddOrbSteps = 18;
    public int orbChargeRampUp = 4;
    public int changeBiomeEvery = 18;
    public float biasToLeaveCenterOfGravity = 1;
    public float cheatSlowdown = 3;
    public float upwardRate = .5f;
    
    private Vector3 prevLoc = Vector3.zero;
    private Vector3 nextLoc = Vector3.zero;
    private float progress = 0;
    private bool cheat = false;
    private Image cheatButton = null;

    private InDarkness darkness;
    private Vector3 etherCurrent;
    private int orbChargeRampUpStep = 1;

    public void Awake() {
        darkness = GetComponentInChildren<InDarkness>();
        StartCoroutine(Runner());
        GameObject cheatButtonGo = GameObject.Find("Cheat");
        if (cheatButtonGo != null) cheatButton = cheatButtonGo.GetComponent<Image>();
    }

    public IEnumerator Runner() {
        int count = addOrbEvery;
        int absoluteCount = 0;
        
        foreach (RandomWalkAlgorithm.Output step in RandomWalkAlgorithm.EnumerateSteps(interiaOfEtherCurrent, changeBiomeEvery, biasToLeaveCenterOfGravity, upwardRate)) {
            for (int i = 0; i < step.newCave.Length; i++) {
                GridPos position = step.newCave[i];
                if (step.iOddsAreBridge) {
                    Debug.Log("Bridge at " + position + ", open: " + (i % 2 == 1));
                    if (i == 1) Debug.DrawLine(position.World, step.newCave[i - 1].World, Color.blue, 600);
                    if (i == 3) Debug.DrawLine(position.World, step.newCave[i - 1].World, Color.white, 600);
                }
                CaveGrid.I.SetPos(position, !step.iOddsAreBridge || (i % 2 == 0));
            }
            // if (step.newCave.Length > 0 && count++ % addOrbEvery == 0) {
            if (darkness.IsInDarkness) count++;
            else count = 0;
            absoluteCount++;
            if (count >= addOrbEvery || absoluteCount >= maxAddOrbSteps) {
                if (absoluteCount >= maxAddOrbSteps) Debug.Log("MAX ADD ORB STEPS TRIGGERED");
                StandardOrb orb = GameObject.Instantiate(orbPrefab, step.location, Quaternion.identity, orbParent);
                if (orbChargeRampUpStep < orbChargeRampUp) {
                    orb.chargeTime *= ((float)orbChargeRampUpStep / orbChargeRampUp);
                    orbChargeRampUpStep++;
                }
                GameObject.FindObjectOfType<RisingWater>().AddOrb(orb);
                if (cheat) orb.chargeTime *= cheatSlowdown;
                absoluteCount = 0;
            }
            foreach (GridPos interesting in step.interesting) {
                StandardOrb orb = GameObject.Instantiate(orbPrefab, interesting.World, Quaternion.identity, orbParent);
                if (cheat) {
                    orb.chargeTime *= cheatSlowdown;
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

    private static float CubicInterpolate(float x) {
        return 3 * Mathf.Pow(x, 2) - 2 * Mathf.Pow(x, 3);
    }
}
