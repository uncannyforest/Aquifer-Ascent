using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomWalk : MonoBehaviour {
    public StandardOrb orbPrefab;
    public Transform orbParent;
    public GameObject rubblePrefab;
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
    private GridPos exitDirection;
    private bool justChangedMode = true;
    private int orbChargeRampUpStep = 1;

    public Grid<bool> path = new Grid<bool>();

    public void Awake() {
        darkness = GetComponentInChildren<InDarkness>();
        StartCoroutine(Runner());
        GameObject cheatButtonGo = GameObject.Find("Cheat");
        if (cheatButtonGo != null) cheatButton = cheatButtonGo.GetComponent<Image>();
    }

    public IEnumerable<Output> MasterEnumerateSteps() {

        int numModes = 3;
        int currentMode = Random.Range(0, 2);
        int stepsUntilNextMode = 0;
        IEnumerator<Output> enumerator = MuxEnumerator(currentMode, GridPos.zero, GridPos.E, ref stepsUntilNextMode);
        while(enumerator.MoveNext()) {
            Output output = enumerator.Current;
            yield return output;

            if (stepsUntilNextMode-- == 0) {
                int newMode = Random.Range(0, numModes - 1);
                if (newMode >= currentMode) newMode++; // don't allow current mode
                currentMode = newMode;
                Debug.Log("Starting mode " + currentMode + " at position " + output.position);
                enumerator = MuxEnumerator(currentMode, output.position, output.exitDirection, ref stepsUntilNextMode);
                justChangedMode = true;
            } else justChangedMode = false;
        }
    }

    public IEnumerator<Output> MuxEnumerator(int currentMode, GridPos position, GridPos exitDirection, ref int stepsUntilNextMode) {
        Vector3 biasToFleeStartLocation = new Vector3(1, -.5f, -.5f) * biasToLeaveCenterOfGravity;

        stepsUntilNextMode = currentMode == 0 ? Random.Range(modeSwitchRate / 2, modeSwitchRate * 6) : Random.Range(2, modeSwitchRate * 2);

        switch (currentMode) {
            case 0:
                return RandomWalkAlgorithm.EnumerateSteps(position, exitDirection, modeSwitchRate, interiaOfEtherCurrent, biasToFleeStartLocation, upwardRate).GetEnumerator();
            case 1:
                return RandomWalkAlgorithmStairs.MoveVertically(position, exitDirection, upwardRate > .5f ? true : Randoms.CoinFlip).GetEnumerator();
            case 2:
                return RandomWalkAlgorithmStairs.MoveHorizontally(position, exitDirection, modeSwitchRate, biasToFleeStartLocation, upwardRate).GetEnumerator();
            default: throw new IndexOutOfRangeException();
        }
    }

    public IEnumerator Runner() {
        int count = addOrbEvery;
        int absoluteCountDown = maxAddOrbSteps * orbChargeRampUpStep / orbChargeRampUp;
        
        GridPos? lastPathForDebug = null;

        CaveGrid.Biome.Next(GridPos.zero, (_) => 1, true);
        foreach (Output step in MasterEnumerateSteps()) {
            for (int i = 0; i < step.newCave.Length; i++) {
                CaveGrid.Mod mod = step.newCave[i];
                if (step.bridgeMode == Output.BridgeMode.ODDS) {
                    Debug.Log("Bridge at " + mod.pos + ", open: " + (i % 2 == 1));
                    if (i == 1) Debug.DrawLine(mod.pos.World, step.newCave[i - 1].pos.World, Color.blue, 600);
                    if (i == 3) Debug.DrawLine(mod.pos.World, step.newCave[i - 1].pos.World, Color.white, 600);
                }
                bool blocksPath = false;
                if (!mod.open) for (int j = -1; j <= mod.roof; j++) {
                    if (path[mod.pos + GridPos.up * j]) blocksPath = true;
                }
                CaveGrid.I.SetPos(mod);
                if (blocksPath) {
                    Debug.Log("BLOCKS PATH! ADDING RUBBLE");
                    GameObject.Instantiate(rubblePrefab, mod.pos.World, Quaternion.identity);
                }
            }
            if (step.onPath is GridPos onPath) {
                path[onPath] = true;
                if (lastPathForDebug is GridPos actualLastPath) Debug.DrawLine(onPath.World, actualLastPath.World, Color.white, 30);
            }
            lastPathForDebug = step.onPath;
            // if (step.newCave.Length > 0 && count++ % addOrbEvery == 0) {
            if (darkness.IsInDarkness) count++;
            else count = 0;
            absoluteCountDown--;
            if (count >= addOrbEvery || absoluteCountDown <= 0) {
                if (absoluteCountDown >= maxAddOrbSteps) Debug.Log("MAX ADD ORB STEPS TRIGGERED");
                StandardOrb orb = GameObject.Instantiate(orbPrefab, step.location, Quaternion.identity, orbParent);
                if (orbChargeRampUpStep < orbChargeRampUp) {
                    orb.chargeTime *= ((float)orbChargeRampUpStep / orbChargeRampUp);
                    orbChargeRampUpStep++;
                }
                if (GameObject.FindObjectOfType<RisingWater>() != null) GameObject.FindObjectOfType<RisingWater>().AddOrb(orb);
                if (cheat) orb.chargeTime *= cheatSlowdown;
                absoluteCountDown = maxAddOrbSteps * orbChargeRampUpStep / orbChargeRampUp;
            }
            foreach (GridPos interesting in step.interesting) {
                StandardOrb orb = GameObject.Instantiate(orbPrefab, interesting.World, Quaternion.identity, orbParent);
                if (cheat) {
                    orb.chargeTime *= cheatSlowdown;
                }
            }
            etherCurrent = step.etherCurrent;
            exitDirection = step.exitDirection;
            prevLoc = nextLoc;
            nextLoc = step.location;
            progress = 0;
            yield return new WaitForSeconds(modRate);
            modRate += slowDown;
        }
    }

    void Update() {
        progress += Time.deltaTime / modRate;
        transform.position = Vector3.Lerp(prevLoc, nextLoc, CubicInterpolate(Mathf.Clamp01(progress)));
        if (etherCurrent.y > .5f) {
            etherCurrent = new Vector3(etherCurrent.x, 0, etherCurrent.z);
            Debug.DrawLine(transform.position, transform.position + etherCurrent, Color.magenta, 600);
        } else
            Debug.DrawLine(transform.position, transform.position + etherCurrent, Color.magenta);

        Debug.DrawLine(transform.position, transform.position + exitDirection.World, Color.red);

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

    public struct Output {
        public Vector3 location;
        public GridPos position;
        public GridPos exitDirection;
        public CaveGrid.Mod[] newCave;
        public GridPos? onPath;
        public GridPos[] interesting;
        public Vector3 etherCurrent;
        public BridgeMode bridgeMode; // for debug lines only

        public enum BridgeMode {
            NONE,
            ODDS,
            LAST
        }

        public Output(Vector3 location, GridPos position, GridPos exitDirection, CaveGrid.Mod[] newCave, GridPos? onPath, GridPos[] interesting, Vector3 etherCurrent, BridgeMode bridgeMode = BridgeMode.NONE) {
            this.location = location;
            this.position = position;
            this.exitDirection = exitDirection;
            this.newCave = newCave;
            this.onPath = onPath;
            this.interesting = interesting;
            this.etherCurrent = etherCurrent;
            this.bridgeMode = bridgeMode;
        }
    }
}
