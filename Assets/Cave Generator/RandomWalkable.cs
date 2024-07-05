using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkable {
    public class Parameters {
        private static float[][] MODES = new float[][] {
            new float[] {1.5f, -0.5f, 0, 0},
            new float[] {8.5f, -0.5f, 0, 0},
            new float[] {1.5f, 2.75f, 0, 1},
            new float[] {8.5f, 2.75f, 1, 1}
        };
        private static int PARAM_COUNT = 4;
        public static int MODE_COUNT = MODES.Length;

        private float[] parameters;
        private float[] interpolateDiff;
        public int targetMode;

        public int vScale { get => Mathf.RoundToInt(parameters[0]); } // in [1.5, 8.5]
        public float hScale { get => parameters[1]; } // in [-0.5, 3]
        public int vDelta { get => Mathf.RoundToInt(parameters[2]); } // in [0, 1]
        public float forwardBias { get => parameters[3]; } // in [0, 1]

        public Parameters(int targetMode) {
            this.targetMode = targetMode;
            interpolateDiff = new float[PARAM_COUNT];
            parameters = new float[PARAM_COUNT];
            Array.Copy(MODES[targetMode], parameters, PARAM_COUNT);
        }

        public void StartInterpolation(int endMode, float fraction) {
            targetMode = endMode;
            for (int i = 0; i < PARAM_COUNT; i++) {
                interpolateDiff[i] = (MODES[endMode][i] - parameters[i]) * PARAM_COUNT * fraction;
            }
        }

        // returns debug string
        public string Interpolate() {
            int p = Random.Range(0, PARAM_COUNT);
            parameters[p] += interpolateDiff[p];
            if (interpolateDiff[p] * (parameters[p] - MODES[targetMode][p]) > 0) { // same sign means we overshot
                parameters[p] = MODES[targetMode][p];
            }
            return "Approaching mode " + targetMode + " at " + vScale + ", " + Mathf.Ceil(hScale) + ", " + (vDelta > 0 ? "WW, " : "Flr, ") + forwardBias;
        }
    }

    // returns int in [-3, 3]: 1 if path is one above, -1 if path is 1 below
    // 0 if no path overlap or we hit the path exactly
    public static int? OverlapsPathAt(Grid<bool> path, GridPos pos, Parameters p) {
        if (path[pos]) return 0;
        for (int i = 1; i <= 3; i++) {
            if (path[pos + GridPos.up * i]) return i;
            if (path[pos - GridPos.up * i]) return -i;
        }
        for (int i = 4; i <= p.vScale + 1; i++)
            if (path[pos + GridPos.up * i]) return i;

        return null;
    }

    public static List<GridPos> OverlapsShelfAt(Grid<bool> path, CaveGrid.Mod mod) {
        List<GridPos> overlaps = new List<GridPos>();
        for (int i = -1; i <= mod.roof; i++) {
            if (path[mod.pos + GridPos.up * (i + 2)]) overlaps.Add(mod.pos + GridPos.up * i);
        }
        return overlaps;
    }

    public static IEnumerable<RandomWalk.Output> EnumerateSteps(GridPos initPos, GridPos initDirection, int modeSwitchRate, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, float upwardRate, Grid<bool> path) {
        GridPos smallPos = initPos;
        GridPos smallMove = initDirection;
        GridPos largePos = smallPos;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        
        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), 1/6f, smallPos, new GridPos[] {}, Vector3.zero);

        Parameters p = new Parameters(3);

        bool justFlipped = false;

        int modeSwitchCountdown = 1;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            string interpolateDebug = InterpolateMode(ref modeSwitchCountdown, modeSwitchRate, p);

            UpdateEtherCurrent(ref etherCurrent, ref justFlipped, inertiaOfEtherCurrent, biasToLeaveStartLocation, p);
            Vector3 bias = GetBias(smallMove, etherCurrent, p);

            Debug.Log(interpolateDebug + ", ether current " + etherCurrent.HComponents.Max() / inertiaOfEtherCurrent + ", bias " + bias.Max());

            smallMove = GridPos.Random(2/3f, bias, upwardRate);
            int? neededAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
            if (neededAdjustment == 2 || neededAdjustment == -2) { // roll the dice one more time
                smallMove = GridPos.Random(2/3f, bias, upwardRate);
                neededAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
            }

            int smallRelativeW = GetSmallWRelativeToLarge(largePos, smallPos, p);
            GridPos largeRelativeHoriz = GetLargeHorizRelativeToSmall(largePos, smallPos, p);
            smallPos += smallMove;
            largePos = smallPos + largeRelativeHoriz - GridPos.up * smallRelativeW;

            List<CaveGrid.Mod> newCave = LargePosMods(largePos, path, p);
            newCave.Add(CaveGrid.Mod.Cave(smallPos, p.hScale > 0 || neededAdjustment != null ? 1 : p.vScale - 1));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            foreach (CaveGrid.Mod mod in newCave) CaveGrid.Biome.Next(mod.pos);
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), 1, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
        }
    }

    private static string InterpolateMode(ref int modeSwitchCountdown, int modeSwitchRate, Parameters p) {
        if (--modeSwitchCountdown <= 0) {
            modeSwitchCountdown = modeSwitchRate;
            int newMode = Random.Range(1, Parameters.MODE_COUNT);
            if (newMode == p.targetMode) newMode = 0;
            p.StartInterpolation(newMode, Random.Range(.5f, 1f) / modeSwitchRate);
        }
        return p.Interpolate();
    }

    private static void UpdateEtherCurrent(ref GridPos etherCurrent, ref bool justFlipped, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, Parameters p) {
        etherCurrent += GridPos.RandomHoriz(biasToLeaveStartLocation * GridPos.MODERATE_BIAS);
        if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
            etherCurrent /= -2;
            justFlipped = true;
        } else justFlipped = false;
    }

    private static Vector3 GetBias(GridPos move, GridPos etherCurrent, Parameters p) {
        return etherCurrent.HComponents.MaxNormalized() * (1 + p.forwardBias) * GridPos.MODERATE_BIAS + p.forwardBias * move.HComponents;
    }

    private static int? AdjustToBeWalkable(ref GridPos smallMove, GridPos smallPos, Grid<bool> path, Parameters p) {
        int? walkablePathCorrection = OverlapsPathAt(path, smallPos + smallMove.Horizontal, p);
        if (walkablePathCorrection is int correction) {
            if (Mathf.Abs(correction) <= 1) smallMove.w = correction;
            else if (Mathf.Abs(correction) == 3) smallMove.w = correction / -3;
        }
        return walkablePathCorrection;
    }

    private static int GetSmallWRelativeToLarge(GridPos oldLargePos, GridPos oldSmallPos, Parameters p) {
        int oldW = oldSmallPos.w - oldLargePos.w;
        if (p.hScale <= 0 || p.vScale < 5 || p.vDelta == 0) return Mathf.Max(0, oldW - 1);
        if (oldW < 3) return oldW + 1;
        else if (oldW > p.vScale - 2) return oldW - 1;
        else return oldW + (Random.value < 1/3f ? Randoms.Sign : 0);
    }
    private static GridPos GetLargeHorizRelativeToSmall(GridPos largePos, GridPos smallPos, Parameters p) {
        if (p.hScale <= 0) return GridPos.zero;

        GridPos relative = largePos.Horizontal - smallPos.Horizontal;
        if (Random.value < 1/6f) {
            relative += GridPos.RandomHoriz();
        }
        while (relative.Magnitude >= p.hScale + 1) {
            relative -= GridPos.RoundFromVector3(relative.HComponents.MaxNormalized());
        }
        return relative;
    }

    private static List<CaveGrid.Mod> LargePosMods(GridPos largePos, Grid<bool> path, Parameters p) {
        List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
        if (p.hScale <= 0) return newCave;

        int vMidpoint = p.vScale / 2 - 1;
        int vMidpointExtraHeight = p.vScale % 2;

        CaveGrid.Mod mod = CaveGrid.Mod.RandomVerticalExtension(largePos + vMidpoint * GridPos.up, 0, vMidpoint, vMidpointExtraHeight, vMidpointExtraHeight + vMidpoint);
        newCave.Add(mod);
        foreach (GridPos overlap in OverlapsShelfAt(path, mod)) newCave.Add(CaveGrid.Mod.Wall(overlap));

        int magnitude = 1;
        for ( ; magnitude < p.hScale; magnitude++) {
            foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
                mod = CaveGrid.Mod.RandomVerticalExtension(largePos + unit + vMidpoint * GridPos.up, 0, vMidpoint, vMidpointExtraHeight, vMidpointExtraHeight + vMidpoint);
                newCave.Add(mod);
                foreach (GridPos overlap in OverlapsShelfAt(path, mod)) newCave.Add(CaveGrid.Mod.Wall(overlap));
            }
        }
        foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
            if (Random.value < p.hScale - magnitude + 1) {
                mod = CaveGrid.Mod.RandomVertical(largePos + unit, 0, p.vScale - 2);
                newCave.Add(mod);
                foreach (GridPos overlap in OverlapsShelfAt(path, mod)) newCave.Add(CaveGrid.Mod.Wall(overlap));
            }
        }
        return newCave;
    }

}
