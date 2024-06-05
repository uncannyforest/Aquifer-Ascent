using System;
using System.Collections;
using System.Collections.Generic;
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
    public int modeSwitchRate = 20;
    
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

    public IEnumerable<RandomWalkAlgorithm.Output> MasterEnumerateSteps() {
        Vector3 biasToFleeStartLocation = new Vector3(1, -.5f, -.5f) * biasToLeaveCenterOfGravity;

        int numModes = 3;
        int currentMode = 0;
        int stepsUntilNextMode = modeSwitchRate * 4;
        IEnumerator<RandomWalkAlgorithm.Output> enumerator = RandomWalkAlgorithm.EnumerateSteps(GridPos.zero, GridPos.E, modeSwitchRate, interiaOfEtherCurrent, changeBiomeEvery, biasToFleeStartLocation, upwardRate, true).GetEnumerator();
        while(enumerator.MoveNext()) {
            RandomWalkAlgorithm.Output output = enumerator.Current;
            yield return output;

            if (stepsUntilNextMode-- == 0) {
                int newMode = Random.Range(0, numModes - 1);
                if (newMode >= currentMode) newMode++; // don't allow current mode
                currentMode = newMode;
                stepsUntilNextMode = currentMode == 0 ? modeSwitchRate * 4 : modeSwitchRate;
                Debug.Log("Starting mode " + currentMode + " at position " + output.position);
                switch (currentMode) {
                    case 0:
                        enumerator = RandomWalkAlgorithm.EnumerateSteps(output.position, output.lastMove, modeSwitchRate, interiaOfEtherCurrent, changeBiomeEvery, biasToFleeStartLocation, upwardRate, false).GetEnumerator();
                        break;
                    case 1:
                        enumerator = RandomWalkAlgorithmStairs.MoveVertically(output.position, output.lastMove, upwardRate > .5f ? true : Randoms.CoinFlip).GetEnumerator();
                        break;
                    case 2:
                        enumerator = RandomWalkAlgorithmStairs.MoveHorizontally(output.position, output.lastMove, biasToFleeStartLocation, upwardRate).GetEnumerator();
                        break;
                }
            }
        }
    }

    public IEnumerator Runner() {
        int count = addOrbEvery;
        int absoluteCount = 0;
        
        CaveGrid.Biome.Next(GridPos.zero, (_) => 1, true);
        foreach (RandomWalkAlgorithm.Output step in MasterEnumerateSteps()) {
            for (int i = 0; i < step.newCave.Length; i++) {
                GridPos position = step.newCave[i];
                if (step.bridgeMode == RandomWalkAlgorithm.Output.BridgeMode.ODDS) {
                    Debug.Log("Bridge at " + position + ", open: " + (i % 2 == 1));
                    if (i == 1) Debug.DrawLine(position.World, step.newCave[i - 1].World, Color.blue, 600);
                    if (i == 3) Debug.DrawLine(position.World, step.newCave[i - 1].World, Color.white, 600);
                }
                bool clearSpace = step.bridgeMode == RandomWalkAlgorithm.Output.BridgeMode.NONE ||
                    (step.bridgeMode == RandomWalkAlgorithm.Output.BridgeMode.ODDS && (i % 2 == 0)) ||
                    (step.bridgeMode == RandomWalkAlgorithm.Output.BridgeMode.LAST && i < step.newCave.Length - 1);
                CaveGrid.I.SetPos(position, clearSpace);
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
                if (GameObject.FindObjectOfType<RisingWater>() != null) GameObject.FindObjectOfType<RisingWater>().AddOrb(orb);
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
