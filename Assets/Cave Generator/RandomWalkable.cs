using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkable {
    public class Parameters {
        private static float[] LINK_MODE =
            new float[] {0, 2f, 0f, 0, .25f, 2};
        private static float[][] MODES = new float[][] {
            new float[] {0, 1.5f, -0.333f, 0, 1f, 0},
            new float[] {0, 8.5f, -0.333f, 0, 1f, 0},
            new float[] {0, 1.5f,  2.667f, 0, 1f, 1},
            new float[] {1, 5f, 1f, 1, .5f, 2},
            new float[] {1, 8.5f, 1f, 1, .5f, 2},
            new float[] {1, 5f, 2.667f, 1, .5f, 2},
            new float[] {1, 8.5f, 2.667f, 1, .5f, 2}
        };
        private static int PARAM_COUNT = 6;
        public static int MODE_COUNT = MODES.Length;

        private float[] parameters;
        private float[] interpolateDiff;
        public int prevMode;
        public int targetMode;
        public float lerp = 0;
        public float lerpStep = 0;

        public bool followWall { get => parameters[0] > 1/3f; }
        public int vScale { get => Mathf.RoundToInt(parameters[1]); } // in [1.5, 8.5]
        public float hScale { get => parameters[2]; } // in [-0.5, 3]
        public int vDelta { get => Mathf.FloorToInt(parameters[3] + 1/3f); } // in [0, 1]
        public float grade { get => parameters[4]; } // in [0, 1]
        public float forwardBias { get => parameters[5]; } // in [0, 2]

        public Parameters(int targetMode) {
            this.targetMode = targetMode;
            parameters = new float[PARAM_COUNT];
            Array.Copy(MODES[targetMode], parameters, PARAM_COUNT);
            ResetInterpolation();
        }

        public void ResetInterpolation() {
            prevMode = targetMode;
            interpolateDiff = new float[PARAM_COUNT];
        }

        public int StartLinkMode() {
            int linkModeLength = 3 + Mathf.CeilToInt(hScale);
            Array.Copy(LINK_MODE, parameters, PARAM_COUNT);
            ResetInterpolation();
            targetMode = RandomOtherMode(targetMode);
            linkModeLength += Mathf.CeilToInt(MODES[targetMode][2]);
            lerp = 0;
            lerpStep = 1f / linkModeLength;
            return linkModeLength;
        }

        public void JumpToNewMode() {
            int partialMode = RandomOtherMode(targetMode);
            for (int i = 0; i < PARAM_COUNT; i++) {
                parameters[i] = Mathf.Lerp(MODES[targetMode][i], MODES[partialMode][i], Maths.Bias0(Random.value) / 2);
            }
            ResetInterpolation();
        }

        public void StartNewInterpolation(float fraction) {
            prevMode = targetMode;
            targetMode = RandomOtherMode(targetMode);
            lerp = 0;
            lerpStep = fraction;
            for (int i = 0; i < PARAM_COUNT; i++) {
                interpolateDiff[i] = (MODES[targetMode][i] - parameters[i]) * PARAM_COUNT * fraction;
            }
        }

        // returns debug string
        public string Interpolate() {
            lerp = Mathf.Min(1, lerp + lerpStep);

            int p = Random.Range(0, PARAM_COUNT);
            parameters[p] += interpolateDiff[p];
            if (interpolateDiff[p] * (parameters[p] - MODES[targetMode][p]) > 0) { // same sign means we overshot
                parameters[p] = MODES[targetMode][p];
            }
            return "Approaching mode " + targetMode + " at " + parameters[0] + (followWall ? "FW, " : "Ins, ") + vScale + ", " + Mathf.Ceil(hScale) + ", " + parameters[3] + (vDelta > 0 ? "WW, " : "Flr, ") + grade + ", " + forwardBias;
        }

        public int SupplyBiome(int _) => (Random.value < Maths.CubicInterpolate(lerp) ? targetMode : prevMode) + 1;

        public static int RandomOtherMode(int mode) {
            int newMode = Random.Range(1, Parameters.MODE_COUNT);
            if (newMode == mode) newMode = 0;
            return newMode;
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
        GridPos largeMove = initDirection;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        
        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), Biomes.NoChange, 1/6f, smallPos, new GridPos[] {}, Vector3.zero);

        Parameters p = new Parameters(0);

        bool justFlipped = false;

        int modeSwitchCountdown = modeSwitchRate;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            string interpolateDebug = InterpolateMode(ref modeSwitchCountdown, modeSwitchRate, p);
            if (modeSwitchCountdown == 0) SetUpBiasForLinkMode(ref etherCurrent, ref smallMove, smallPos, largePos, inertiaOfEtherCurrent);

            UpdateEtherCurrent(ref etherCurrent, ref justFlipped, inertiaOfEtherCurrent, biasToLeaveStartLocation, p);
            Vector3 bias = GetBias(p.followWall ? largeMove : smallMove, etherCurrent, p);
            float elevChange = GetElevChangeRate(p);
            float upward = GetUpwardRate(upwardRate, p);

            int? neededWalkableAdjustment = null;
            bool largeWait = false;
            bool smallWait = false;
            if (p.followWall && p.hScale > 0)
                FollowWallThenMoveLarge(ref largePos, ref largeMove, ref largeWait, ref smallPos, ref smallMove, ref smallWait, ref neededWalkableAdjustment, path, bias, elevChange, upward, p);
            else MoveSmallAndLargeRelative(ref largePos, ref largeMove, ref smallPos, ref smallMove, ref neededWalkableAdjustment, path, bias, elevChange, upward, p);

            Debug.Log(interpolateDebug + ", step " + modeSwitchCountdown + ", ether current " + etherCurrent.HComponents.Max() / inertiaOfEtherCurrent + ", bias " + bias.Max() + ", rel v " + (smallPos - largePos).w);

            List<CaveGrid.Mod> newCave = largeWait ? new List<CaveGrid.Mod>() : LargePosMods(largePos, path, p);
            newCave.Add(CaveGrid.Mod.Cave(smallPos, p.hScale > 0 || neededWalkableAdjustment != null ? 1 : p.vScale - 1));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), p.SupplyBiome, 1, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
        }
    }

    private static string InterpolateMode(ref int modeSwitchCountdown, int modeSwitchRate, Parameters p) {
        modeSwitchCountdown++;
        if (modeSwitchCountdown == 0) {
            p.JumpToNewMode();
            p.StartNewInterpolation(1f / modeSwitchRate);
        } else if (modeSwitchCountdown >= modeSwitchRate) {
            if (Random.value > .5f) {
                modeSwitchCountdown = 0;
                p.StartNewInterpolation(1f / modeSwitchRate);
            } else {
                int linkModeLength = p.StartLinkMode();
                modeSwitchCountdown = -linkModeLength;
            }
        }
        return p.Interpolate();
    }

    private static void SetUpBiasForLinkMode(ref GridPos etherCurrent, ref GridPos smallMove, GridPos smallPos, GridPos largePos, int inertiaOfEtherCurrent) {
        if (largePos == smallPos) return;
        GridPos relativeLargePos = (largePos - smallPos);
        relativeLargePos.w = 0;
        Vector3 relativeLargePosWorld = relativeLargePos.World;
        smallMove.w = 0;
        Vector3 smallMoveWorld = smallMove.World;
        float angle = Vector3.SignedAngle(relativeLargePosWorld, smallMoveWorld, Vector3.up); // (to, from, up) in practice with CCW rots per ex in docs
        Debug.Log("smallMove " + smallMove + " world " + smallMoveWorld + " relLargePos " + relativeLargePos + " world " + relativeLargePosWorld + " angle " + angle);
        if (angle == 0 || angle == 180) return;
        GridPos targetDirection = angle < 0 ? smallMove.RotateLeft() : smallMove.RotateRight();
        smallMove = targetDirection;
        etherCurrent = targetDirection * (inertiaOfEtherCurrent / 2);
        Debug.Log("New smallMove " + smallMove + " etherCurrent " + etherCurrent);
    }

    private static void UpdateEtherCurrent(ref GridPos etherCurrent, ref bool justFlipped, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, Parameters p) {
        etherCurrent += GridPos.RandomHoriz(biasToLeaveStartLocation * GridPos.MODERATE_BIAS);
        if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
            etherCurrent /= -2;
            justFlipped = true;
        } else justFlipped = false;
    }

    private static Vector3 GetBias(GridPos move, GridPos etherCurrent, Parameters p) {
        return etherCurrent.HComponents.MaxNormalized() * (1 + p.forwardBias) * GridPos.MODERATE_BIAS + p.forwardBias * move.HComponents.MaxNormalized();
    }

    private static float GetElevChangeRate(Parameters p) {
        return p.grade * 2/3;
    }

    private static float GetUpwardRate(float upwardRate, Parameters p) {
        if (upwardRate == .5f || p.grade == 1) return upwardRate;
        if (p.grade < 1) return Mathf.Lerp(1, upwardRate, p.grade);
        else return Mathf.Lerp(upwardRate, 1, p.grade - 1);
    }

    private static void MoveSmallAndLargeRelative(ref GridPos largePos, ref GridPos largeMove,
            ref GridPos smallPos, ref GridPos smallMove, ref int? neededWalkableAdjustment,
            Grid<bool> path, Vector3 bias, float elevChange, float upwardRate, Parameters p) {
        smallMove = GridPos.Random(elevChange, bias, upwardRate);
        neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
        if (neededWalkableAdjustment == 2 || neededWalkableAdjustment == -2) { // roll the dice one more time
            smallMove = GridPos.Random(elevChange, bias, upwardRate);
            neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
        }

        int smallRelativeW = (smallPos.w - largePos.w) + GetSmallWRelativeToLargeDelta(largePos, smallPos, p);
        GridPos largeRelativeHoriz = GetLargeHorizRelativeToSmall(largePos, smallPos, p);
        smallPos += smallMove;
        GridPos oldLargePos = largePos;
        largePos = smallPos + largeRelativeHoriz - GridPos.up * smallRelativeW;
        largeMove = largePos - oldLargePos;
    }

    private static GridPos GetLargeHorizRelativeToSmall(GridPos largePos, GridPos smallPos, Parameters p) {
        if (p.hScale <= 0) return GridPos.zero;
        GridPos relative = largePos.Horizontal - smallPos.Horizontal;
        if (Random.value < 1/6f)
            relative += GridPos.RandomHoriz();
        if (relative.Magnitude >= p.hScale + 1)
            relative -= GridPos.RoundFromVector3(relative.HComponents.MaxNormalized());
        return relative;
    }

    private static void FollowWallThenMoveLarge(ref GridPos largePos, ref GridPos largeMove, ref bool largeWait,
            ref GridPos smallPos, ref GridPos smallMove, ref bool smallWait, ref int? neededWalkableAdjustment,
            Grid<bool> path, Vector3 bias, float elevChange, float upwardRate, Parameters p) {
        int hScale = Mathf.CeilToInt(p.hScale);

        GridPos catchUp = largePos - smallPos;
        int horizDistance = catchUp.Horizontal.Magnitude;
        largeWait = horizDistance > hScale * 3;
        smallWait = horizDistance <= hScale;

        int smallMoveW = GetSmallWRelativeToLargeDelta(largePos, smallPos, p);
        if (!smallWait) {
            // at this point, hScale + 1 <= horizDistance <= hScale * 3 (unless smallCatchUp)
            float smallTargetLargeFactor = Mathf.Min(1, (horizDistance - hScale - 1f) / (hScale * 2 - 1));
            Vector3 smallBias = Vector3.Lerp(smallMove.HComponents, catchUp.HComponents.MaxNormalized(), smallTargetLargeFactor);
            smallMove = GridPos.RoundFromVector3(smallBias.MaxNormalized());
            smallMove.w = smallMoveW;
            int wallAhead = CaveGrid.Grid[smallPos + smallMove] ? 0 : 1;
            int wallLeft = CaveGrid.Grid[smallPos + smallMove.RotateLeft()] ? 0 : 1;
            int wallRight = CaveGrid.Grid[smallPos + smallMove.RotateRight()] ? 0 : 1;
            int wallCode = wallLeft << 2 | wallAhead << 1 | wallRight;
            int turnCode = 0;
            if (wallCode == 0b011 || wallCode == 0b100) {
                if (Randoms.CoinFlip) turnCode = 1; // left
            } else if (wallCode == 0b110 || wallCode == 0b001) {
                if (Randoms.CoinFlip) turnCode = -1; // right
            } else {
                if (wallCode == 0b000 || wallCode == 0b111) {
                    int wallLeft120 = CaveGrid.Grid[smallPos + smallMove.RotateLeft().RotateLeft()] ? 0 : 1;
                    int wallRight120 = CaveGrid.Grid[smallPos + smallMove.RotateRight().RotateRight()] ? 0 : 1;
                    wallCode = wallLeft120 << 4 | wallCode << 1 | wallRight120;
                    if (wallCode == 0b01111 || wallCode == 0b10000) turnCode = 1; // left
                    else if (wallCode == 0b11110 || wallCode == 0b00001) turnCode = -1; // right
                    else if (wallCode == 0b10001 || wallCode == 0b01110) turnCode = Randoms.Sign;
                    else turnCode = Random.Range(-1, 2);
                } else turnCode = Random.Range(-1, 2);
            }
            // Debug.Log("Factor " + smallTargetLargeFactor + " smallMove " + smallMove + " wall code " + wallCode + (turnCode == 1 ? " turn left" : turnCode == -1 ? " turn right" : " no turn"));
            smallMove = smallMove.Rotate(turnCode * 60);
            neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
            smallPos += smallMove;
        }
        if (!largeWait) {
            largeMove = GridPos.Random(smallWait ? 0 : elevChange, bias, upwardRate);
            if (smallWait && Mathf.Abs(smallMoveW) > 1) largeMove.w = -smallMoveW;
            largePos += largeMove;
            catchUp = largePos - smallPos;
        }
    }
    private static int GetSmallWRelativeToLargeDelta(GridPos oldLargePos, GridPos oldSmallPos, Parameters p) {
        int oldW = oldSmallPos.w - oldLargePos.w;
        if (p.hScale <= 0 || p.vScale < 5 || p.vDelta == 0) return -oldW;
        else if (oldW < 1 || oldW > p.vScale) return Random.Range(3, p.vScale - 2) - oldW;
        else if (oldW < 3) return 1;
        else if (oldW > p.vScale - 2) return -1;
        else return Random.value < 1/3f ? Randoms.Sign : 0;
    }

    private static int? AdjustToBeWalkable(ref GridPos smallMove, GridPos smallPos, Grid<bool> path, Parameters p) {
        int? walkablePathCorrection = OverlapsPathAt(path, smallPos + smallMove.Horizontal, p);
        if (walkablePathCorrection is int correction) {
            if (Mathf.Abs(correction) <= 1) smallMove.w = correction;
            else if (Mathf.Abs(correction) == 3) smallMove.w = correction / -3;
        }
        return walkablePathCorrection;
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
