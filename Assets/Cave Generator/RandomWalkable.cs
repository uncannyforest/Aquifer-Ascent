using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkable {
    public class Parameters {
        private static float[] LINK_MODE =
            new float[] {0, 2f,    0f,     0, .25f, 2, 0,     1,    1};
        private static float[] PARAMS_MIN =
            new float[] {0, 1.5f, -1f,    -1,  0,  0,  0,  .51f,    0, 8};
        private static float[] PARAMS_MAX =
            new float[] {1,10.5f,  3f,     1, 3f,  4,  1, 12.5f,    1, 8};
        private static float[][] MODES = new float[][] {
            new float[] {0, 1.5f, -1f,     0,  1f,   0,  0,  .51f,  .5f, 1}, // orig
            new float[] {0, 8.5f, -1f,     0,  1f,   0,  0,  .51f,  .5f, 10}, // tall
            new float[] {1, 5f,    1f,     1,  .5f,  2,  0,  .51f,  .5f, 7}, // path small
            new float[] {1, 8.5f,  1f,     1,  .5f,  2,  0,  .51f,  .5f, 2}, // path tall
            new float[] {1, 1.5f,  0.333f, 1,  3f,   2,  0,  .51f,  .5f, 3}, // stairwell small
            // new float[] {0, 1.5f,  1f,     0, 3f,  0,  0,  .51f,    1, 11}, // spiral
            new float[] {1, 8.5f,  2f,    -1,  1f,   1,  0,  8.5f,  .5f, 4}, // jump rooms
            // new float[] {1, 4f,    1f,     1, 2.5f, 2,  0,  8.5f, .5f, 6}, // jump levels
            // new float[] {1, 8.5f,  1f,     0,  1f, 1.5f, 0,   3f,    1,  5}, // pillars
            // new float[] {1, 4f,    2f,     0,  1.5f,  0, .5f,  5f, .5f,  5}, // maze
            new float[] {1, 5f,    1f,     0,  1.5f,  1, .5f,  4f, .5f,  5}, // maze of mediocrity
            new float[] {0, 1.5f,  1f,     0,  2f,   2,  1,  .51f, .25f, 9}, // turn
            new float[] {1, 3f,  4f,       1,  3f,   0,  0,   3f,    1, 12}, // tower
            new float[] {1, 8.5f, 2f,      1, 2.25f, 1,  0,   6f,  .25f, 6}, // cliff
            new float[] {0, 8.5f, -1f,     0, 1.5f, 1f, .8f, .51f, .5f, 11}, // tall turn
            new float[PARAM_COUNT + 1] // random
        };
        private const int PARAM_COUNT = 9; // not counting biome, which is at index PARAM_COUNT
        public static int MODE_COUNT = MODES.Length;
        public static int RANDOM_MODE = MODE_COUNT - 1;

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
        public float vDelta { get => parameters[3]; } // in [-1, 1]
        public int vDeltaMode { get => parameters[3] < 0 ? -1 : parameters[3] < 2/3f ? 0 : 1; } // in [-1, 1]
        public float grade { get => parameters[4]; } // in [0, 3], 2 means diagonal
        public float inertia { get => parameters[5]; } // in [0, 2]
        public float torque { get=> parameters[6]; } // in [0, 2]
        public int stepSize { get => Mathf.RoundToInt(parameters[7]); } // [in 1, 8]
        public float stalactites { get => parameters[8]; } // [in 0, 1]

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
            RandomizeHScale(MODES[targetMode][2]);
            linkModeLength += 2 * Mathf.CeilToInt(targetHScale) - Mathf.RoundToInt(MODES[targetMode][6]);
            if (linkModeLength < 1) linkModeLength = 1;
            lerp = 0;
            lerpStep = 1f / linkModeLength;
            return linkModeLength;
        }

        public void JumpToNewMode() {
            if (targetMode == RANDOM_MODE) {
                parameters = MODES[targetMode];
                return;
            }
            int partialMode = RandomMode(5);
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

        public bool StartNewInterpolation(float fraction) {
            prevMode = targetMode;
            targetMode = RandomMode(2);
            lerp = 0;
            lerpStep = fraction;
            for (int i = 0; i < PARAM_COUNT; i++) {
                float targetLevel = MODES[targetMode][i];
                if (ParamIsHScale(i) && targetMode != RANDOM_MODE) targetLevel = RandomizeHScale(targetLevel);
                interpolateDiff[i] = (targetLevel - parameters[i]) * PARAM_COUNT * fraction;
            }
            return targetMode != prevMode;
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
                + vScale + " x " + hScale.ToString("F1") + " / "
                + parameters[3].ToString("F1") + (vDeltaMode == 1 ? "WW / " : vDeltaMode == 0 ? "Flr / " : "Low / ")
                + grade.ToString("F1") + " x " + inertia.ToString("F1") + " x " + torque.ToString("F1") + " / "
                + stepSize + " / " + stalactites.ToString("F1");
        }

        private bool ParamIsHScale(int param) => param == 2;
        public float RandomizeHScale(float minimum) {
            targetHScale = minimum + Maths.SuperExpDecayDistribution(Random.value);
            Debug.Log("Approaching hScale " + targetHScale + " (minimum " + minimum + ")");
            return targetHScale;
        }

        public int SupplyBiome(int _) => Random.value < Maths.CubicInterpolate(lerp) ?
            getBiomeForMode(targetMode) : getBiomeForMode(prevMode);

        public int RandomMode(int additionalNoChangeFactor = 0) {
            int mode = Random.Range(-additionalNoChangeFactor, MODE_COUNT);
            if (mode < 0) mode = targetMode;
            if (mode == RANDOM_MODE) SetRandomParams();
            return mode;
        }
        public int RandomOtherMode(int mode) {
            int newMode = Random.Range(1, MODE_COUNT);
            if (newMode == mode) newMode = 0;
            if (newMode == RANDOM_MODE) SetRandomParams();
            return newMode;
        }

        public void SetRandomParams() {
            for (int i = 0; i < PARAM_COUNT + 1; i++) {
                if (ParamIsHScale(i)) {
                    targetHScale = PARAMS_MIN[i] + RandomizeHScale(0) * PARAMS_MAX[i];
                    MODES[RANDOM_MODE][i] = targetHScale;
                }
                else MODES[RANDOM_MODE][i] = Random.Range(PARAMS_MIN[i], PARAMS_MAX[i]);
            }
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
        bool? canJump = false; // used by GetSmallWRelativeToLargeDelta(), null means walkway activated (vDelta 1)
        float turnTime = 0;
        int skipLargeStep = 0;
        float lastGrade = 0;

        int modeSwitchCountdown = modeSwitchRate;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            string interpolateDebug = InterpolateMode(ref modeSwitchCountdown, modeSwitchRate, p);
            if (modeSwitchCountdown == 0) SetUpBiasForLinkMode(ref etherCurrent, ref smallMove, smallPos, largePos, inertiaOfEtherCurrent);

            UpdateEtherCurrent(ref etherCurrent, ref justFlipped, inertiaOfEtherCurrent, biasToLeaveStartLocation, p);
            GridPos inertialMove = Turn(p.followWall ? largeMove : smallMove, etherCurrent, inertiaOfEtherCurrent, ref turnTime, p);
            Vector3 bias = GetBias(inertialMove, etherCurrent, p);
            float elevChange = GetElevChangeRate(p);
            float upward = GetUpwardRate(upwardRate, p.followWall ? largeMove : smallMove, p);

            int? neededWalkableAdjustment = null;
            bool largeWait = false;
            bool smallWait = false;
            if (p.followWall && p.hScale > 0)
                FollowWallThenMoveLarge(ref largePos, ref largeMove, ref largeWait, ref smallPos, ref smallMove, ref smallWait, ref neededWalkableAdjustment, ref canJump, ref lastGrade, path, bias, elevChange, upward, p);
            else MoveSmallAndLargeRelative(ref largePos, ref largeMove, ref largeWait, ref smallPos, ref smallMove, ref neededWalkableAdjustment, ref canJump, ref skipLargeStep, path, bias, elevChange, upward, p);

            Debug.Log(interpolateDebug + ", step " + modeSwitchCountdown + ", ether current " + etherCurrent.HComponents.Max() / inertiaOfEtherCurrent + ", bias " + bias.Max() + ", rel v " + (smallPos - largePos).w + " jump " + canJump);

            List<CaveGrid.Mod> newCave = largeWait ? new List<CaveGrid.Mod>() : LargePosMods(largePos, smallPos, path, p);
            newCave.Add(CaveGrid.Mod.Cave(smallPos, p.hScale > 0 || neededWalkableAdjustment != null ? 1 : p.vScale - 1));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            float stepTime = GetStepTime(smallPos, stepTimeQueue, etherCurrent.HComponents.Max() / inertiaOfEtherCurrent, p);
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), p.SupplyBiome, stepTime, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
        }
    }

    private static string InterpolateMode(ref int modeSwitchCountup, int modeSwitchRate, Parameters p) {
        modeSwitchCountup++;
        if (modeSwitchCountup == 0) {
            p.JumpToNewMode();
            bool modeChanged = p.StartNewInterpolation(1f / modeSwitchRate);
            modeSwitchCountup = modeChanged ? 0 : Mathf.FloorToInt(Maths.Bias1(Random.value) * modeSwitchRate);
        } else if (modeSwitchCountup >= modeSwitchRate) {
            if (Random.value > .5f) {
                bool modeChanged = p.StartNewInterpolation(1f / modeSwitchRate);
                modeSwitchCountup = modeChanged ? 0 : Mathf.FloorToInt(Maths.Bias1(Random.value) * modeSwitchRate);
            } else {
                int linkModeLength = p.StartLinkMode();
                modeSwitchCountup = -linkModeLength;
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
            Debug.Log("Ether current flipped, was " + etherCurrent);
            etherCurrent /= 2;
            etherCurrent = etherCurrent.Rotate(Randoms.Sign * 120);
            justFlipped = true;
            Debug.Log("Now " + etherCurrent);
        } else justFlipped = false;
    }

    private static GridPos Turn(GridPos move, GridPos etherCurrent, int inertiaOfEtherCurrent, ref float turnTime, Parameters p) {
        if (p.torque == 0) {
            turnTime = 0;
            return move;
        }

                             int direction;          float absTime;
        if (turnTime > 0)      { direction = 1;            absTime = turnTime;  }
        else if (turnTime < 0) { direction = -1;           absTime = -turnTime; }
        else                   { direction = Randoms.Sign; absTime = 1;         }

        absTime -= p.torque;

        if (absTime <= 0) {
            float etherAngle = GridPos.Angle(move, etherCurrent) * direction; // positive if turning towards etherCurrent
            float etherStrength = etherCurrent.Magnitude / (float)inertiaOfEtherCurrent;
            if (etherAngle >= -120 && etherAngle < -60 && Random.value < etherStrength) {
                direction *= -1;
                Debug.Log("Swap direction");
            }
            absTime += 1;
            move = move.Rotate(60 * direction);
        }

        turnTime = absTime * direction;
        return move;
    }

    private static Vector3 GetBias(GridPos move, GridPos etherCurrent, Parameters p) {
        Vector3 etherCurrentBias = etherCurrent.HComponents.MaxNormalized() * (1 + p.inertia) * GridPos.MODERATE_BIAS * (1 - p.torque);
        Vector3 forwardBias = p.inertia / ((1 - p.torque) * (1 - p.torque)) * move.HComponents.MaxNormalized(); // can be infinite
        forwardBias = forwardBias.SetNaNTo(0);
        Vector3 bias = etherCurrentBias + forwardBias;
        Debug.Log("Bias from etherCurrent " + etherCurrentBias + " from forwardBias " + forwardBias
            + " (" + etherCurrentBias.Max().ToString("F1") + " + " + forwardBias.Max().ToString("F1") + " = " + bias.Max().ToString("F1")
            + ") grade factor " + Mathf.Min(1, 3 - p.grade));
        if (p.grade <= 2) return bias;
        else return Vector3.Lerp(bias, Vector3.zero, p.grade - 2);
    }

    private static float GetElevChangeRate(Parameters p) {
        if (p.grade < 1) return p.grade.ScaleTo(0, 2/3f);
        else if (p.grade < 2) return (p.grade - 1).ScaleTo(2/3f, 1);
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

    private static void MoveSmallAndLargeRelative(ref GridPos largePos, ref GridPos largeMove, ref bool largeWait,
            ref GridPos smallPos, ref GridPos smallMove, ref int? neededWalkableAdjustment, ref bool? canJump, ref int skipLargeStep,
            Grid<bool> path, Vector3 bias, float elevChange, float upwardRate, Parameters p) {
        GridPos oldSmallMove = smallMove;
        smallMove = GridPos.Random(elevChange, bias, upwardRate);
        if (p.grade < 2 && p.grade / 2 < Random.value) AdjustToFollowGround(ref smallMove, smallPos, path, upwardRate);
        neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
        if (WalkableAdjustmentIsDisfavored(neededWalkableAdjustment, upwardRate)) { // roll the dice one more time
            smallMove = GridPos.Random(elevChange, bias, upwardRate);
            neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
        }

        largeWait = (++skipLargeStep < p.stepSize);
        if (largeWait) {
            smallPos += smallMove;
            largePos += smallMove;
        } else {
            skipLargeStep = 0;
            int smallRelativeW = (smallPos.w - largePos.w) + GetSmallWRelativeToLargeDelta(largePos, smallPos, ref canJump, p);
            GridPos largeRelativeHoriz = GetLargeHorizRelativeToSmall(largePos, smallPos, p);
            MaybeRotateLargeHorizRelativeToSmall(ref largeRelativeHoriz, oldSmallMove, smallMove, p);
            smallPos += smallMove;
            GridPos oldLargePos = largePos;
            largePos = smallPos + largeRelativeHoriz - GridPos.up * smallRelativeW;
            largeMove = largePos - oldLargePos;
        }
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

    private static void MaybeRotateLargeHorizRelativeToSmall(ref GridPos largeRelativeHoriz,
            GridPos oldSmallMove, GridPos smallMove, Parameters p) {
        if (p.torque == 0 || oldSmallMove.Horizontal == smallMove.Horizontal) return;
        int rotation = GridPos.UnitsAngle(oldSmallMove.Horizontal, smallMove.Horizontal);
        largeRelativeHoriz = largeRelativeHoriz.Rotate(rotation);
    }

    private static void FollowWallThenMoveLarge(ref GridPos largePos, ref GridPos largeMove, ref bool largeWait,
            ref GridPos smallPos, ref GridPos smallMove, ref bool smallWait, ref int? neededWalkableAdjustment, ref bool? canJump, ref float lastGrade,
            Grid<bool> path, Vector3 bias, float elevChange, float upwardRate, Parameters p) {
        int hScale = Mathf.CeilToInt(p.hScale);

        GridPos catchUp = largePos - smallPos;
        //float stepSizeFactor = p.grade < 2 ? .75f : .5f;

        // smallTargetLargeFactor will interpolate between 0 @ catchUpMin---beyond which smallWait = true if p.grade < 2
        // ---and 1 @ catchUpMax---beyond which largeWait = true if p.grade < 2.
        // Normally, we want catchUpThreshhold to be hScale.
        // Usually, p.inertia is 2. But when p.inertia is 0, we want [catchUpMin, catchUpMax] to be [0, 3].
        // When jumping levels, we have large stepSize, and we want it to be around .5 to linger inside the edge.
        // When jumping rooms, large stepSize and p.inertia is 1, so catchUpMin and catchUpMax are about half jumping levels.
        float catchUpThreshhold = (float)hScale * Mathf.Lerp(1, 1/p.stepSize, .5f);// stepSizeFactor);
        float catchUpMin = p.inertia.ScaleFrom(0, 2).ScaleTo(0, catchUpThreshhold + 1);
        float catchUpMax = p.inertia.ScaleFrom(0, 2).ScaleTo(3, catchUpThreshhold * 3);
        int horizDistance = catchUp.Horizontal.Magnitude;
        int expectedW = Mathf.RoundToInt(upwardRate * 2 - 1); // when grade >= 2, this is only 0 if config UR = .5 && lastMove.w = 0
        int smallMoveW;

        int catchUpW = catchUp.w * expectedW; // so catchUp always >= 0 unless small got ahead
        int targetW = expectedW > 0 ? 1 : p.vScale; // when going down, target top of large
        largeWait = horizDistance > catchUpMax || catchUpW > targetW;
        smallWait = horizDistance < catchUpMin && catchUpW < targetW; // if expectedW == 0, always smallWait

        if (p.grade < 2) {
            smallMoveW = GetSmallWRelativeToLargeDelta(largePos, smallPos, ref canJump, p);
        } else {            
            if (expectedW == 0) expectedW = Randoms.Sign;
            // Usually, (catchupW - targetW) == 1 and largeWait == true.
            // When p.hScale == .333f, upChance is 3/4
            // When p.hScale == 2, upChance is 1/3
            // But when p.vDelta is near 1, upChance is 1.
            float lerp = Mathf.InverseLerp(0, 1, p.vDelta);
            float upChance = Mathf.Lerp((catchUpW - targetW) / (1 + p.hScale), 1, Maths.EaseOut(lerp));
            smallMoveW = Random.value < upChance ? expectedW : 0;
            canJump = false;
        }

        if (!smallWait) {
            float smallTargetLargeFactor = Mathf.InverseLerp(catchUpMin, catchUpMax, horizDistance);
            Vector3 smallBias = Vector3.Lerp(smallMove.HComponents, catchUp.HComponents.MaxNormalized(), smallTargetLargeFactor);
            Debug.Log("catchUp " + catchUp + " catchUpThresshold " + catchUpThreshhold + " catchUpMin " + catchUpMin
                + " catchUpMax " + catchUpMax + " smallTargetLargeFactor " + smallTargetLargeFactor
                + " smallMove " + smallMove.HComponents + " catchUp normalized " + catchUp.HComponents.MaxNormalized()
                + " result " + GridPos.RoundFromVector3(smallBias.MaxNormalized()));
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
                    else turnCode = RandomTurnCode();
                } else turnCode = RandomTurnCode();
            }
            // Debug.Log("Factor " + smallTargetLargeFactor + " smallMove " + smallMove + " wall code " + wallCode + (turnCode == 1 ? " turn left" : turnCode == -1 ? " turn right" : " no turn"));
            smallMove = smallMove.Rotate(turnCode * 60);
            if (p.vDelta >= -.333f && (p.grade <= 2 || p.grade - 2 < Random.value))
                AdjustToFollowGround(ref smallMove, smallPos, path, upwardRate);
            neededWalkableAdjustment = AdjustToBeWalkable(ref smallMove, smallPos, path, p);
            if (WalkableAdjustmentIsDisfavored(neededWalkableAdjustment, 0)) {
                int newTurnCode = Randoms.Sign;
                GridPos newSmallMove = smallMove.Rotate(newTurnCode * 60);
                int? newNeededWalkableAdjustment = AdjustToBeWalkable(ref newSmallMove, smallPos, path, p);
                Debug.Log("Retry Walkable in FollowWallAndThenMoveLarge.  Old turnCode " + turnCode + " smallMove " + smallMove
                    + " new turnCode " + newTurnCode + " smallMove " + newSmallMove + " neededWalkableAdjustment " + newNeededWalkableAdjustment);
                if (!WalkableAdjustmentIsDisfavored(newNeededWalkableAdjustment, 0)) {
                    smallMove = newSmallMove;
                    neededWalkableAdjustment = newNeededWalkableAdjustment;
                }
            }
            smallPos += smallMove;
        }
        if (!largeWait) {
            largeMove = Random.value < (16 - 5 * lastGrade) / 6 // 3 -> 1/6, 2 or less -> 1
                ? GridPos.Random(smallWait && lastGrade < 2 ? 0 : elevChange, bias, upwardRate)
                : GridPos.zero + GridPos.up * expectedW;
            JumpByStepSize(ref largeMove, ref lastGrade, p);
            if (smallWait)  { // just entered mode
                if (p.grade < 2) largeMove.w = -smallMoveW; // keep small level, large below
                else if (lastGrade < 2) largeMove.w = Mathf.Clamp(largeMove.w, 2 - p.vScale, 0); // bring large level w small
            }
            RandomWalk.DebugDrawLine(largePos, largePos + largeMove, new Color(.5f, .5f, .5f, .125f), 60);
            largePos += largeMove;
            catchUp = largePos - smallPos;
        }
    }

    private static int RandomTurnCode() {
        int turnCode = Random.Range(-1, 2); // Random.Range(3, 9) / 4 - 1;
        if (turnCode != 0) Debug.Log("RANDOMLY TURNED! " + turnCode);
        return turnCode;
    }

    private static int GetSmallWRelativeToLargeDelta(GridPos oldLargePos, GridPos oldSmallPos, ref bool? nextCanJump, Parameters p) {
        bool? canJump = nextCanJump;
        int oldW = oldSmallPos.w - oldLargePos.w;

        // comments indicate what state we are in if we've reached this line of code
        nextCanJump = false;
        if (p.hScale <= 0) return -oldW;
        // hScale is valid for considering vDelta -1 and 1
        if (p.vDeltaMode == -1) return Random.value > Mathf.Abs(p.vScale / 4 - oldW) * 2f / p.vScale // + .25f
            ? 0 : Mathf.Clamp(p.vScale / 4 - oldW, -1, 1); // Mathf.Clamp(Mathf.Clamp(oldW, 0, (p.vScale - 1) / 2) - oldW, -1, 1);
        if (p.vScale < 5) return Mathf.Clamp(-oldW, -1, 1);

        // if anything, we only need vDelta to increment to start the walkway
        nextCanJump = p.grade < 2 && p.stepSize == 1;
        if (p.vDeltaMode == 0) return canJump == null ? -oldW : Mathf.Clamp(-oldW, -1, 1); // null means can jump down

        // do walkway because all are valid: hScale, vScale, vDelta
        nextCanJump = null;
        if (canJump == true && oldW < 2) return Random.Range(3, p.vScale - 2) - oldW;
        // but don't jump
        if (oldW < 3) return 1;
        if (oldW > p.vScale - 2) return -1;
        // walkway is already within walkway range
        return Random.value < 1/3f ? Randoms.Sign : 0;
    }

    private static void JumpByStepSize(ref GridPos largeMove, ref float lastGrade, Parameters p) {
        float horizFactor = Mathf.Lerp(p.stepSize, 1, lastGrade - 2);
        float vertFactor = Mathf.Lerp(1, p.stepSize, lastGrade / 2);
        int w = Mathf.RoundToInt(largeMove.w * vertFactor);
        largeMove *= Mathf.RoundToInt(horizFactor);
        largeMove.w = w;
        Debug.Log("jump horizFactor " + horizFactor + " vertFactor " + vertFactor + " largeMove " + largeMove);
        lastGrade = p.grade;
    }

    private static void AdjustToFollowGround(ref GridPos smallMove, GridPos oldSmallPos, Grid<bool> path, float upwardRate) {
        if (Mathf.Abs(smallMove.w) > 1) return;
        int oldW = oldSmallPos.w;
        bool[] possPath = new bool[4];
        for (int i = -2; i <= 1; i++) {
            GridPos posToCheck = oldSmallPos + smallMove;
            posToCheck.w = oldW + i;
            possPath[i + 2] = CaveGrid.Grid[posToCheck];
        }
        int? maybeOverride = null;
        if (!possPath[0] && possPath[1]) maybeOverride = -1;
        else if (!possPath[1] && possPath[2]) maybeOverride = 0;
        else if (!possPath[2] && possPath[3]) maybeOverride = 1;
        if (maybeOverride is int doOverride && doOverride != smallMove.w
                && !WalkableAdjustmentIsDisfavored(doOverride, upwardRate)) {
            GridPos oldSmallMove = smallMove;
            smallMove.w = doOverride;
            if(!path[oldSmallPos + smallMove]) RandomWalk.DebugDrawLine(oldSmallPos + oldSmallMove, oldSmallPos + smallMove, Color.cyan);
        }
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

        bool stalactite = Random.value < p.stalactites;
        bool stalagmite = p.hScale <= 2 ? stalactite : Random.value < p.stalactites;
        for (int magnitude = 0; magnitude < p.hScale + 1; magnitude++)
            foreach (GridPos unit in GridPos.ListAllWithMagnitude(magnitude)) {
                CaveGrid.Mod mod = CaveGrid.Mod.Cave(largePos + unit, p.vScale - 1);

                int stalactiteBounds = Mathf.Max(0, Mathf.CeilToInt(p.hScale) / 2 - 1);
                if (magnitude <= stalactiteBounds) {
                    if (stalagmite & stalactite) continue;
                    else if (stalactite) mod = mod.RandomEnd(false);
                    else if (stalagmite) mod = mod.RandomEnd(true);
                } else if (p.hScale > 2 && magnitude <= stalactiteBounds + 1) {
                    if (stalagmite & stalactite) mod = mod.RandomSubset();
                    else if (stalactite) mod = mod.RandomFromMidpoint(true, false);
                    else if (stalagmite) mod = mod.RandomFromMidpoint(false, true);
                } else if (magnitude >= p.hScale) {
                    if (Random.value > p.hScale - magnitude + 1) continue;
                    mod = mod.RandomSubset();
                }
                else if (magnitude >= p.hScale - 1 && !stalactite && !stalagmite) mod = mod.RandomFromMidpoint();
                else if (magnitude >= p.hScale - 2) mod = mod.RandomBump();

                newCave.AddRange(RemoveShelfOverlaps(path, smallPos, mod));
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
        float scaleY = 4;
        Vector3 displacement = smallPos.World - fourPosAgo.World;
        Vector3 displacementHoriz = Vector3.Scale(displacement, new Vector3(1, 0, 1));
        float semiChebyshevSquared = Mathf.Max(displacement.y * displacement.y * scaleY * scaleY, displacementHoriz.sqrMagnitude);
        // typically, (smallPos.World - onePosAgo.World).magnitude is 4. Finally, div by 4 because fourPosAgo
        float displacementFactor = (semiChebyshevSquared / 16).ScaleTo(2/3f, 1) / 4;
        float forwardBiasFactor = p.inertia.ScaleTo(5/6f, 1f);
        // Debug.Log("Step time displacement " + semiChebyshevSquared + " " + displacementFactor.ToString("F1")
        //     + " forwardBias " + forwardBiasFactor.ToString("F1"));
        return displacementFactor;
    }
}
