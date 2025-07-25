using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RandomWalk : MonoBehaviour {
    private static RandomWalk instance;
    RandomWalk(): base() {
        instance = this;
    }

    public StandardOrb orbPrefab;
    public Transform orbParent;
    public GameObject rubblePrefab;
    public int interiaOfEtherCurrent = 6;
    public float modRate = 2/3f;
    public float modRateYFactor = 4f;
    public int addOrbEvery = 18;
    public int maxAddOrbSteps = 18;
    public int orbChargeRampUp = 4;
    public int changeBiomeEvery = 18;
    public float rubbleRate = 1/18f;
    public float biasToLeaveCenterOfGravity = 1;
    public float cheatSlowdown = 3;
    public float upwardRate = .5f;
    public int modeSwitchRate = 20;
    public int interestingMinRate = 6;
    public GameObject interestingPrefab;
    public LineRenderer interestingHint;
    public GameObject creaturePrefab;
    
    private Vector3 prevLoc = Vector3.zero;
    private Vector3 nextLoc = Vector3.zero;
    private float progress = 0;
    private float relSpeed = 1;
    private bool cheat = false;
    private Image cheatButton = null;

    private InDarkness darkness;
    private Vector3 etherCurrent;
    private GridPos exitDirection;
    private int orbChargeRampUpStep = 1;

    public Grid<bool> path = new Grid<bool>();

    public void Awake() {
        darkness = GetComponentInChildren<InDarkness>();
        GameObject cheatButtonGo = GameObject.Find("Cheat");
        if (cheatButtonGo != null) cheatButton = cheatButtonGo.GetComponent<Image>();
    }

    public void Start() {
        StartCoroutine(Runner());
    }

    public IEnumerable<Output> MasterEnumerateSteps() {
        Random.State seed = CaveGrid.I.seed;
        Debug.Log("Loaded seed into RW");

        int numModes = 3;
        int currentMode = 3;//Random.Range(0, 2);
        int stepsUntilNextMode = 0;
        IEnumerator<Output> enumerator = MuxEnumerator(currentMode, GridPos.zero, GridPos.E, ref stepsUntilNextMode);
        while(enumerator.MoveNext()) {
            Output output = enumerator.Current;
            seed = Random.state;
            yield return output;
            Random.state = seed;
            
            if (false){//stepsUntilNextMode-- == 0) {
                int newMode = Random.Range(0, numModes - 1);
                if (newMode >= currentMode) newMode++; // don't allow current mode
                currentMode = newMode;
                Debug.Log("Starting mode " + currentMode + " at position " + output.position);
                enumerator = MuxEnumerator(currentMode, output.position, output.exitDirection, ref stepsUntilNextMode);
            };
        }
    }

    public IEnumerator<Output> MuxEnumerator(int currentMode, GridPos position, GridPos exitDirection, ref int stepsUntilNextMode) {
        Vector3 biasToFleeStartLocation = new Vector3(1, -.5f, -.5f) * biasToLeaveCenterOfGravity;

        stepsUntilNextMode = currentMode == 0 ? Random.Range(2, modeSwitchRate * 12)
            : currentMode == 1 ? Random.Range(2, modeSwitchRate * (4 + RandomWalkAlgorithmStairs.GetVerticalScaleForBiome()) / 3)
            : currentMode == 2 ? Random.Range(2, modeSwitchRate * 4)
            : modeSwitchRate;

        switch (currentMode) {
            case 0:
                return RandomWalkAlgorithm.EnumerateSteps(position, exitDirection, modeSwitchRate, interiaOfEtherCurrent, biasToFleeStartLocation, upwardRate).GetEnumerator();
            case 1:
                return RandomWalkAlgorithmStairs.MoveVertically(position, exitDirection, upwardRate > .5f ? true : Randoms.CoinFlip).GetEnumerator();
            case 2:
                return RandomWalkAlgorithmStairs.MoveHorizontally(position, exitDirection, modeSwitchRate, biasToFleeStartLocation, upwardRate).GetEnumerator();
            case 3:
                return RandomWalkable.EnumerateSteps(position, exitDirection, modeSwitchRate, interiaOfEtherCurrent, biasToFleeStartLocation, upwardRate, modRateYFactor, interestingMinRate, path).GetEnumerator();
            default: throw new IndexOutOfRangeException();
        }
    }

    public IEnumerator Runner() {
        int count = addOrbEvery;
        int absoluteCountDown = maxAddOrbSteps * orbChargeRampUpStep / orbChargeRampUp;
        Vector3 lastPositionForMoreOrbs = Vector3.zero;
        
        GridPos? lastPath = null;
        bool canRubble = false;

        CaveGrid.Biome.Next(GridPos.zero, (_) => 1, true);
        foreach (Output step in MasterEnumerateSteps()) {
            relSpeed = step.speed;
            for (int i = 0; i < step.newCave.Length; i++) {
                CaveGrid.Mod mod = step.newCave[i];
                if (mod.IsUnnecessary) {
                    if (mod.open) canRubble = false;
                    continue;
                }
                CaveGrid.Biome.Next(mod.pos, step.biome, true);
                
                if (mod.open) {
                    // removed criterion step.etherCurrent.ScaleDivide(CaveGrid.Scale).magnitude > 1f
                    // because RandomWalkable's trajectory is much more complicated nowadays
                    if (step.newCave.Length <= 2 && !mod.Overlaps) {
                        if (canRubble && Random.value < rubbleRate) {
                            CaveGrid.I.soft[mod.pos] = true;
                            CaveGrid.I.UpdatePos(mod.pos, 0, 1);
                            Debug.Log("Adding rubble for funsies :)");
                            Debug.DrawLine(mod.pos.World - Vector3.up * .5f, mod.pos.World + Vector3.up * .5f, Color.red, 600);
                            continue;
                        }
                        // Debug.Log(canRubble ? "Can rubble, still" : "Can rubble, now");
                        canRubble = true;
                    } else {
                        canRubble = false;
                        // if (step.newCave.Length <= 2) Debug.Log("Can NOT rubble! " + step.etherCurrent.ScaleDivide(CaveGrid.Scale).magnitude + " " + mod.Overlaps);
                    }
                }
                if (step.bridgeMode == Output.BridgeMode.ODDS) {
                    Debug.Log("Bridge at " + mod.pos + ", open: " + (i % 2 == 1));
                    if (i == 1) Debug.DrawLine(mod.pos.World, step.newCave[i - 1].pos.World, Color.blue, 600);
                    if (i == 3) Debug.DrawLine(mod.pos.World, step.newCave[i - 1].pos.World, Color.white, 600);
                }
                GridPos? blocksPath = null;
                if (!mod.open) for (int j = -1; j <= mod.roof; j++) {
                    if (path[mod.pos + GridPos.up * j]) {
                        blocksPath = mod.pos + GridPos.up * j;
                        break;
                    }
                }
                if (blocksPath is GridPos soft) {
                    Debug.Log("BLOCKS PATH! ADDING RUBBLE");
                    CaveGrid.I.soft[soft] = true;
                    Debug.DrawLine(soft.World - Vector3.up * .5f, soft.World + Vector3.up * .5f, Color.red, 600);
                }
                CaveGrid.I.SetPos(mod);
            }
            // Debug.Log("Ether current magnitude:" + step.etherCurrent.ScaleDivide(CaveGrid.Scale).magnitude);
            if (step.onPath is GridPos onPath) {
                path[onPath] = true;
                if (lastPath is GridPos lastPathPos) {
                    Debug.DrawLine(onPath.World, lastPathPos.World, Color.white, 60);
                    if (lastPathPos.w - onPath.w > 1) { // drop
                        CaveGrid.I.SetPos(CaveGrid.Mod.Cave(onPath, lastPathPos.w - onPath.w));
                        for (int i = 1; i < lastPathPos.w - onPath.w; i++) path[onPath + GridPos.up * i] = true;
                    } else if (onPath.w - lastPathPos.w > 1) { // jump
                        CaveGrid.I.SetPos(CaveGrid.Mod.Cave(lastPathPos, onPath.w - lastPathPos.w));
                        for (int i = 1; i < onPath.w - lastPathPos.w; i++) path[lastPathPos + GridPos.up * i] = true;
                    }
                }
            }
            lastPath = step.onPath;
            // if (step.newCave.Length > 0 && count++ % addOrbEvery == 0) {
            if (darkness.IsInDarkness) count++;
            else count = 0;
            absoluteCountDown--;
            if (count >= addOrbEvery || absoluteCountDown <= 0) {
                if (absoluteCountDown >= maxAddOrbSteps) Debug.Log("MAX ADD ORB STEPS TRIGGERED");
                StandardOrb orb = GameObject.Instantiate(orbPrefab, lastPositionForMoreOrbs, Quaternion.identity, orbParent);
                if (orbChargeRampUpStep < orbChargeRampUp) {
                    orb.chargeTime *= ((float)orbChargeRampUpStep / orbChargeRampUp);
                    orbChargeRampUpStep++;
                }
                if (GameObject.FindObjectOfType<RisingWater>() != null) GameObject.FindObjectOfType<RisingWater>().AddOrb(orb);
                if (cheat) orb.chargeTime *= cheatSlowdown;
                absoluteCountDown = maxAddOrbSteps * orbChargeRampUpStep / orbChargeRampUp;
            }
            lastPositionForMoreOrbs = transform.position;
            if (step.item is Item item) {
                GridPos interesting = item.pos;
                Transform parent = CaveGrid.I.GetPosParent(interesting - GridPos.up);
                if (parent != null) {
                    if (item.type == Item.Type.STOP_TIME)
                        GameObject.Instantiate(interestingPrefab,
                            interesting.World + CaveGrid.Scale.y * Vector3.down,
                            Quaternion.identity, parent);
                    else
                        GameObject.Instantiate(creaturePrefab,
                            interesting.World,
                            Quaternion.identity);
                    LineRenderer hint = GameObject.Instantiate(interestingHint);
                    hint.SetPositions(new Vector3[] {
                        step.location,
                        interesting.World + CaveGrid.Scale.y * Vector3.down
                    });
                }
            }
            if (step.etherCurrent.y > .5f) {
                Debug.DrawLine(transform.position, transform.position + etherCurrent, Color.magenta, 600);
                etherCurrent = new Vector3(step.etherCurrent.x, 0, step.etherCurrent.z);
            } else etherCurrent = step.etherCurrent;
            exitDirection = step.exitDirection;
            prevLoc = nextLoc;
            nextLoc = step.location;
            progress = 0;
            if (TimeTravel.I.timePaused) yield return new WaitForSeconds(TimeTravel.I.timePausedFor);
            else yield return new WaitForSeconds(modRate * relSpeed);
        }
    }

    public static void DebugDrawLine(GridPos from, GridPos to, Color color, float duration = 600) => instance.DrawLine(from, to, color, duration);
    private void DrawLine(GridPos from, GridPos to, Color color, float duration) {
        Debug.DrawLine(from.World, to.World, color, duration);
    }

    void Update() {
        if (TimeTravel.I.timePaused) return;
        progress += Time.deltaTime / (modRate * relSpeed);
        transform.position = Vector3.Lerp(prevLoc, nextLoc, Maths.CubicInterpolate(Mathf.Clamp01(progress)));
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

    public struct Output {
        public Vector3 location;
        public GridPos position;
        public GridPos exitDirection;
        public CaveGrid.Mod[] newCave;
        public Func<int, int> biome;
        public float speed;
        public GridPos? onPath;
        public Item? item;
        public Vector3 etherCurrent;
        public BridgeMode bridgeMode; // for debug lines only

        public enum BridgeMode {
            NONE,
            ODDS,
            LAST
        }

        public Output(Vector3 location, GridPos position, GridPos exitDirection, CaveGrid.Mod[] newCave, Func<int, int> biome, float speed, GridPos? onPath, Item? item, Vector3 etherCurrent, BridgeMode bridgeMode = BridgeMode.NONE) {
            this.location = location;
            this.position = position;
            this.exitDirection = exitDirection;
            this.newCave = newCave;
            this.biome = biome;
            this.speed = speed;
            this.onPath = onPath;
            this.item = item;
            this.etherCurrent = etherCurrent;
            this.bridgeMode = bridgeMode;
        }
    }

    public struct Item {
        public GridPos pos;
        public Type type;
        public enum Type {
            STOP_TIME,
            CREATURE,
        }

        public Item(GridPos position, Type type) {
            this.pos = position;
            this.type = type;
        }

        public static Item StopTime(GridPos position) => new Item(position, Type.STOP_TIME);
        public static Item Creature(GridPos position) => new Item(position, Type.CREATURE);
    }
}
