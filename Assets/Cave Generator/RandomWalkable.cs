using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkable {
    public class Parameters {
        private static float[] LINK_MODE =
            new float[] {0, 2f, 0f, 0, .25f, 2, 1};
        private static float[][] MODES = new float[][] {
            new float[] {0, 1.5f, -1f,     0, 1f,  0, .51f,  1}, // orig
            new float[] {0, 8.5f, -1f,     0, 1f,  0, .51f,  9}, // tall // or 8
            new float[] {1, 5f,    1f,     1, .5f, 2, .51f, 11}, // path small
            new float[] {1, 8.5f,  1f,     1, .5f, 2, .51f,  5}, // path tall // or 2
            new float[] {1, 1.5f,  0.333f, 1, 3f,  0, .51f,  3}, // stairwell small // or 10
            new float[] {0, 1.5f,  1f,     0, 3f,  0, .51f,  7}, // spiral // 9?
            new float[] {1, 8.5f,  1f,     0, 1f,  1, 8.5f,  4}, // jump rooms
            new float[] {1, 4f,    1f,     1, 2.5f, 0, 8.5f, 6}, // jump levels
        };
        private static int PARAM_COUNT = 7; // not counting biome, which is at index PARAM_COUNT
        public static int MODE_COUNT = MODES.Length;

        private float[] parameters;
        private float[] interpolateDiff;
        public int prevMode;
        public int targetMode;
        public float lerp = 0;
        public float lerpStep = 0;
        private float targetHScale = -1;

        public bool followWall { get => parameters[0] > 1/3f; }
        public int vScale { get => Mathf.RoundToInt(parameters[1]); } // in [1.5, 8.5]
        public float hScale { get => parameters[2]; } // in [-0.5, 3]
        public int vDelta { get => Mathf.FloorToInt(parameters[3] + 1/3f); } // in [0, 3], 2 means diagonal
        public float grade { get => parameters[4]; } // in [0, 1]
        public float forwardBias { get => parameters[5]; } // in [0, 2]
        public int stepSize { get => Mathf.RoundToInt(parameters[6]); } // [in 1, 8]

        public int getBiomeForMode(int mode) => Mathf.RoundToInt(MODES[mode][PARAM_COUNT]);

        public Parameters(int targetMode) {
            this.targetMode = targetMode;
            parameters = new float[PARAM_COUNT];
            Array.Copy(MODES[targetMode], parameters, PARAM_COUNT);
            ResetInterpolation();
        }

        public void Set(int p, float v) => parameters[p] = v;

        public void ResetInterpolation() {
            prevMode = targetMode;
            interpolateDiff = new float[PARAM_COUNT];
        }

        public int StartLinkMode() {
            int linkModeLength = 1 + 2 * Mathf.CeilToInt(hScale);
            Array.Copy(LINK_MODE, parameters, PARAM_COUNT);
            ResetInterpolation();
            targetMode = RandomOtherMode(targetMode);
            targetHScale = MODES[targetMode][2];
            RandomizeHScale(targetHScale);
            linkModeLength += 2 * Mathf.CeilToInt(targetHScale) - Mathf.RoundToInt(MODES[targetMode][6]);
            if (linkModeLength < 1) linkModeLength = 1;
            lerp = 0;
            lerpStep = 1f / linkModeLength;
            return linkModeLength;
        }

        public void JumpToNewMode() {
            int partialMode = RandomMode();
            for (int i = 0; i < PARAM_COUNT; i++) {
                if (ParamIsHScale(i)) {
                    parameters[i] = targetHScale;
                } else {
                    float modeMix =  Maths.Bias0(Random.value);
                    parameters[i] = Mathf.Lerp(MODES[targetMode][i], MODES[partialMode][i], modeMix);
                    if (modeMix > .5f) Debug.Log("Used more of mode " + partialMode + " in param " + i);
                }
            }
            ResetInterpolation();
        }

        public void StartNewInterpolation(float fraction) {
            prevMode = targetMode;
            targetMode = RandomMode();
            lerp = 0;
            lerpStep = fraction;
            for (int i = 0; i < PARAM_COUNT; i++) {
                float targetLevel = MODES[targetMode][i];
                if (ParamIsHScale(i)) targetLevel = RandomizeHScale(targetLevel);
                interpolateDiff[i] = (targetLevel - parameters[i]) * PARAM_COUNT * fraction;
            }
        }

        // returns debug string
        public string Interpolate() {
            lerp = Mathf.Min(1, lerp + lerpStep);

            int p = Random.Range(0, PARAM_COUNT);
            parameters[p] += interpolateDiff[p];
            bool overshot = interpolateDiff[p] * (parameters[p] - MODES[targetMode][p]) > 0; // check for same sign
            if (ParamIsHScale(p)) overshot = interpolateDiff[p] < 0 && parameters[p] < MODES[targetMode][p]; // only in neg direction
            if (overshot) parameters[p] = MODES[targetMode][p];
            return "Approaching mode " + targetMode + " and hScale " + targetHScale.ToString("F1") + " at "
                + parameters[0].ToString("F1") + (followWall ? "FW / " : "Ins / ")
                + vScale + ", " + hScale.ToString("F1") + " / "
                + parameters[3].ToString("F1") + (vDelta > 0 ? "WW / " : "Flr / ")
                + grade.ToString("F1") + " / " + forwardBias.ToString("F1") + " / "
                + stepSize;
        }

        private bool ParamIsHScale(int param) => param == 2;
        private float RandomizeHScale(float minimum) {
            targetHScale = minimum + Maths.SuperExpDecayDistribution(Random.value);
            Debug.Log("Approaching hScale " + targetHScale + " (minimum " + minimum + ")");
            return targetHScale;
        }

        public int SupplyBiome(int _) => Random.value < Maths.CubicInterpolate(lerp) ?
            getBiomeForMode(targetMode) : getBiomeForMode(prevMode);

        public static int RandomMode() => Random.Range(0, MODE_COUNT);
        public static int RandomOtherMode(int mode) {
            int newMode = Random.Range(1, Parameters.MODE_COUNT);
            if (newMode == mode) newMode = 0;
            return newMode;
        }

        public static int RandomOtherModeLegacy(int mode) {
            int currentModeLegacy = mode / 4;
            int newMode = Random.Range(1, Parameters.MODE_COUNT / 4);
            if (newMode == currentModeLegacy) newMode = 0;
            return newMode * 4 + Random.Range(0, 4);
        }

        public static int RandomOtherModeLegacyScale(int mode) {
            int currentScaleLegacy = mode % 4;
            int newMode = Random.Range(1, 4);
            if (newMode == currentScaleLegacy) newMode = 0;
            return newMode + (mode / 4) * 4;
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

    public static List<CaveGrid.Mod> RemoveShelfOverlaps(Grid<bool> path, GridPos nextPos, CaveGrid.Mod mod) {
        if (!mod.open) return new List<CaveGrid.Mod>() { mod };
        int maxBelow = mod.roof;
        int minAbove = 0;
        bool wasShelf = false;
        for (int i = -1; i <= mod.roof; i++) {
            GridPos pathLocToCheck = mod.pos + GridPos.up * (i + 2);
            if (path[pathLocToCheck] || nextPos == pathLocToCheck) {
                maxBelow = Mathf.Min(maxBelow, i - 1);
                minAbove = Mathf.Max(minAbove, i + 2);
                wasShelf = true;
            }
        }
        if (!wasShelf) return new List<CaveGrid.Mod>() { mod };

        List<CaveGrid.Mod> validMods = new List<CaveGrid.Mod>();
        if (maxBelow >= 1)
            validMods.Add(CaveGrid.Mod.Cave(mod.pos, maxBelow));
        if (minAbove <= mod.roof - 1)
            validMods.Add(CaveGrid.Mod.Cave(mod.pos + GridPos.up * minAbove, mod.roof - minAbove));
        return validMods;
    }

    public static IEnumerable<RandomWalk.Output> EnumerateSteps(GridPos initPos, GridPos initDirection, int modeSwitchRate, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, float upwardRate, Grid<bool> path) {
        GridPos smallPos = initPos;
        GridPos smallMove = initDirection;
        GridPos largePos = smallPos;
        GridPos largeMove = initDirection;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        
        LinkedList<GridPos> stepTimeQueue = SetUpStepTimeQueue(smallPos);

        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), Biomes.NoChange, 1/6f, smallPos, new GridPos[] {}, Vector3.zero);

        Parameters p = new Parameters(0);
        p.Set(4, 1.5f);

        bool justFlipped = false;

        int modeSwitchCountdown = modeSwitchRate;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            string interpolateDebug = InterpolateMode(ref modeSwitchCountdown, modeSwitchRate, p);
            if (modeSwitchCountdown == 0) SetUpBiasForLinkMode(ref etherCurrent, ref smallMove, smallPos, largePos, inertiaOfEtherCurrent);

            UpdateEtherCurrent(ref etherCurrent, ref justFlipped, inertiaOfEtherCurrent, biasToLeaveStartLocation, p);
            Vector3 bias = GetBias(p.followWall ? largeMove : smallMove, etherCurrent, p);
            float elevChange = GetElevChangeRate(p);
            float upward = GetUpwardRate(upwardRate, p.followWall ? largeMove : smallMove, p);

            int? neededWalkableAdjustment = null;
            bool largeWait = false;
            bool smallWait = false;
            if (p.followWall && p.hScale > 0)
                FollowWallThenMoveLarge(ref largePos, ref largeMove, ref largeWait, ref smallPos, ref smallMove, ref smallWait, ref neededWalkableAdjustment, path, bias, elevChange, upward, p);
            else MoveSmallAndLargeRelative(ref largePos, ref largeMove, ref smallPos, ref smallMove, ref neededWalkableAdjustment, path, bias, elevChange, upward, p);

            Debug.Log(interpolateDebug + ", step " + modeSwitchCountdown + ", ether current " + etherCurrent.HComponents.Max() / inertiaOfEtherCurrent + ", bias " + bias.Max() + ", rel v " + (smallPos - largePos).w);

            List<CaveGrid.Mod> newCave = largeWait ? new List<CaveGrid.Mod>() : LargePosMods(largePos, smallPos, path, p);
            newCave.Add(CaveGrid.Mod.Cave(smallPos, p.hScale > 0 || neededWalkableAdjustment != null ? 1 : p.vScale - 1));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            float stepTime = GetStepTime(smallPos, stepTimeQueue, etherCurrent.HComponents.Max() / inertiaOfEtherCurrent, p);
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), p.SupplyBiome, stepTime, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
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
        Vector3 etherCurrentBias = etherCurrent.HComponents.MaxNormalized() * (1 + p.forwardBias) * GridPos.MODERATE_BIAS;
        Vector3 forwardBias = p.forwardBias * move.HComponents.MaxNormalized();
        Vector3 bias = etherCurrentBias + forwardBias;
        Debug.Log("Bias from etherCurrent " + etherCurrentBias + " from forwardBias " + forwardBias
            + " (" + etherCurrentBias.Max().ToString("F1") + " + " + forwardBias.Max().ToString("F1") + " = " + bias.Max().ToString("F1")
            + ") grade factor " + Mathf.Min(1, 3 - p.grade));
        if (p.grade <= 2) return bias;
        else return Vector3.Lerp(bias, Vector3.zero, p.grade - 2);
    }

    private static float GetElevChangeRate(Parameters p) {
        if (p.grade < 1) return p.grade * 2/3;
        else if (p.grade < 2) return (p.grade + 1) / 3;
        else return 1;
    }

    private static float GetUpwardRate(float upwardRate, GridPos lastMove, Parameters p) {
        if (upwardRate == .5f) {
            if (lastMove.w == 0) return .5f;
            float w = (lastMove.w + 1) / 2;
            if (p.grade < 1) return Mathf.Lerp(w, .5f, p.grade);
            else return Mathf.Lerp(.5f, w, p.grade - 1);
        } else {
            if (p.grade < 1) return Mathf.Lerp(1, upwardRate, p.grade);
            else return Mathf.Lerp(upwardRate, 1, p.grade - 1);
        }
    }

    private static void MoveSmallAndLargeRelative(ref GridPos largePos, ref GridPos largeMove,
            ref GridPos smallPos, ref GridPos smallMove, ref int? neededWalkableAdjustment,
            Grid<bool> path, Vector3 bias, float elevChange, float upwardRate, Parameters p) {
        smallMove = GridPos.Random(elevChange, bias, upwardRate);
        neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
        if (WalkableAdjustmentIsDisfavored(neededWalkableAdjustment, upwardRate)) { // roll the dice one more time
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
        float catchUpThreshhold = (float)hScale * (p.grade < 2 ? 1f/p.stepSize : .5f + .5f/p.stepSize);
        int horizDistance = catchUp.Horizontal.Magnitude;
        int expectedW = Mathf.RoundToInt(upwardRate * 2 - 1); // when grade >= 2, this is only 0 if config UR = .5 && lastMove.w = 0
        int smallMoveW;

        if (p.grade < 2) {
            largeWait = horizDistance > catchUpThreshhold * 3;
            smallWait = horizDistance <= catchUpThreshhold;

            smallMoveW = GetSmallWRelativeToLargeDelta(largePos, smallPos, p);
        } else {
            int catchUpW = catchUp.w * expectedW; // so catchUp always >= 0 unless small got ahead
            int targetW = expectedW > 0 ? 1 : p.vScale; // when going down, target top of large
            largeWait = catchUpW > targetW;
            smallWait = catchUpW < targetW; // if expectedW == 0, always smallWait

            if (expectedW == 0) expectedW = Randoms.Sign;
            float upChance = (catchUpW - targetW) / (1 + p.hScale);
            smallMoveW = Random.value < upChance ? expectedW : 0;
        }

        if (!smallWait) {
            // at this point, catchUpThreshhold + 1 <= horizDistance <= catchUpThreshhold * 3 (unless smallCatchUp or grade >= 2)
            float smallTargetLargeFactor = catchUpThreshhold < .51f ? .99f
                : Mathf.Clamp01((horizDistance - catchUpThreshhold - 1f) / (catchUpThreshhold * 2 - 1));
            Debug.Log("catchUp " + catchUp + " catchUpThresshold " + catchUpThreshhold + " smallTargetLargeFactor " + smallTargetLargeFactor);
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
                if ((wallCode == 0b000 || wallCode == 0b111) && Random.value > smallTargetLargeFactor) {
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
            largeMove = Random.value < (16 - 5 * p.grade) / 6 // 3 -> 1/6, 2 or less -> 1
                ? GridPos.Random(smallWait && p.grade < 2 ? 0 : elevChange, bias, upwardRate)
                : GridPos.zero + GridPos.up * expectedW;
            JumpByStepSize(ref largeMove, p);
            if (smallWait && Mathf.Abs(smallMoveW) > 1) largeMove.w = -smallMoveW;
            largePos += largeMove;
            catchUp = largePos - smallPos;
        }
    }

    private static int GetSmallWRelativeToLargeDelta(GridPos oldLargePos, GridPos oldSmallPos, Parameters p) {
        int oldW = oldSmallPos.w - oldLargePos.w;
        if (p.hScale <= 0) return -oldW;
        else if (p.vScale < 5 || p.vDelta == 0) return Mathf.Clamp(-oldW, -1, 1);
        else if ((oldW < 1 || oldW > p.vScale) && p.vScale > 5 && p.grade < 1.5f) return Random.Range(3, p.vScale - 2) - oldW;
        else if (oldW < 3) return 1;
        else if (oldW > p.vScale - 2) return -1;
        else return Random.value < 1/3f ? Randoms.Sign : 0;
    }

    private static void JumpByStepSize(ref GridPos largeMove, Parameters p) {
        float horizFactor = Mathf.Lerp(p.stepSize, 1, p.grade - 2);
        float vertFactor = Mathf.Lerp(1, p.stepSize, p.grade / 2);
        int w = Mathf.RoundToInt(largeMove.w * vertFactor);
        largeMove *= Mathf.RoundToInt(horizFactor);
        largeMove.w = w;
        Debug.Log("jump horizFactor " + horizFactor + " vertFactor " + vertFactor + " largeMove " + largeMove);
    }

    private static int? AdjustToBeWalkable(ref GridPos smallMove, GridPos smallPos, Grid<bool> path, Parameters p) {
        int? walkablePathCorrection = OverlapsPathAt(path, smallPos + smallMove.Horizontal, p);
        if (walkablePathCorrection is int correction) {
            if (Mathf.Abs(correction) <= 1) smallMove.w = correction;
            else if (Mathf.Abs(correction) == 3) smallMove.w = correction / -3;
        }
        return walkablePathCorrection;
    }

    private static bool WalkableAdjustmentIsDisfavored(int? walkableAdjustment, float upwardRate) {
        if (walkableAdjustment == 2 || walkableAdjustment == -2) return true;
        else if (upwardRate == 1f && (walkableAdjustment == -1 || walkableAdjustment == 3)) return true;
        else if (upwardRate == 0f && (walkableAdjustment == 1 || walkableAdjustment == -3)) return true;
        else return false;
    }

    private static List<CaveGrid.Mod> LargePosMods(GridPos largePos, GridPos smallPos, Grid<bool> path, Parameters p) {
        List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
        if (p.hScale <= 0) return newCave;

        int vMidpoint = p.vScale / 2 - 1;
        int vMidpointExtraHeight = p.vScale % 2;

        bool canBumpAtMinus1 = p.vScale >= 4;
        int magnitude = 0;
        for ( ; magnitude < p.hScale - (canBumpAtMinus1 ? 2 : 1); magnitude++) foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
            CaveGrid.Mod mod = CaveGrid.Mod.Cave(largePos + unit, p.vScale - 1);
            newCave.AddRange(RemoveShelfOverlaps(path, smallPos, mod));
        }
        for ( ; magnitude < p.hScale - 1; magnitude++) foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
            int floorBump = Random.Range(0, 2);
            CaveGrid.Mod mod = CaveGrid.Mod.Cave(largePos + unit + floorBump * GridPos.up, p.vScale - (floorBump + Random.Range(1, 3)));
            newCave.AddRange(RemoveShelfOverlaps(path, smallPos, mod));
        }
        for ( ; magnitude < p.hScale; magnitude++) foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
            CaveGrid.Mod mod = CaveGrid.Mod.RandomVerticalExtension(largePos + unit + vMidpoint * GridPos.up, 0, vMidpoint, vMidpointExtraHeight, vMidpointExtraHeight + vMidpoint);
            newCave.AddRange(RemoveShelfOverlaps(path, smallPos, mod));
        }
        foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
            if (Random.value < p.hScale - magnitude + 1) {
                CaveGrid.Mod mod = CaveGrid.Mod.RandomVertical(largePos + unit, 0, p.vScale - 2);
                newCave.AddRange(RemoveShelfOverlaps(path, smallPos, mod));
            }
        }
        return newCave;
    }

    private static LinkedList<GridPos> SetUpStepTimeQueue(GridPos initialPos) {
        LinkedList<GridPos> queue = new LinkedList<GridPos>();
        for (int i = 0; i < 4; i++) queue.AddLast(initialPos);
        return queue;
    }
    private static float GetStepTime(GridPos smallPos, LinkedList<GridPos> recentPos, float etherCurrentMagnitude, Parameters p) {
        GridPos fourPosAgo = recentPos.First.Value;
        recentPos.RemoveFirst();
        recentPos.AddLast(smallPos);
        Vector3 scale = new Vector3(1, 2, 1);
        Vector3 scaledDisplacement = Vector3.Scale(scale, smallPos.World - fourPosAgo.World);
        // typically, (smallPos.World - onePosAgo.World).magnitude is 4. Finally, div by 4 because fourPosAgo
        float displacementFactor = (scaledDisplacement.sqrMagnitude / 16).ScaleTo(2/3f, 1) / 4;
        float forwardBiasFactor = p.forwardBias.ScaleTo(5/6f, 1f);
        Debug.Log("Step time displacement " + scaledDisplacement + " " + displacementFactor.ToString("F1")
            + " forwardBias " + forwardBiasFactor.ToString("F1"));
        return displacementFactor;
    }
}
