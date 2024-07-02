using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkable {
    public struct Parameters {
        private static float[][] MODES = new float[][] {
            new float[] {1.5f, 1.5f},
            new float[] {1.5f, 8.5f}
        };
        private static int PARAM_COUNT = 2;
        public static int MODE_COUNT = MODES.Length;

        private float[] parameters;
        private float[] interpolateDiff;
        public int targetMode;

        public float current { get => parameters[0]; } // in [0, 1]
        public int vScale { get => Mathf.RoundToInt(parameters[1]); } // in [2, 8]

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

        public void Interpolate() {
            int p = Random.Range(0, PARAM_COUNT);
            parameters[p] += interpolateDiff[p];
            if (interpolateDiff[p] * (parameters[p] - MODES[targetMode][p]) > 0) { // same sign means we overshot
                parameters[p] = MODES[targetMode][p];
            }
            Debug.Log("Approaching mode " + targetMode + " at " + String.Join(", ", parameters));
        }
    }

    // returns int in [-3, 3]: 1 if path is one above, -1 if path is 1 below
    // 0 if no path overlap or we hit the path exactly
    public static int? overlapsPathAt(Grid<bool> path, GridPos pos, Parameters p) {
        if (path[pos]) return 0;
        for (int i = 1; i <= 3; i++) {
            if (path[pos + GridPos.up * i]) return i;
            if (path[pos - GridPos.up * i]) return -i;
        }
        for (int i = 4; i <= p.vScale + 1; i++)
            if (path[pos + GridPos.up * i]) return i;

        return null;
    }

    public static IEnumerable<RandomWalk.Output> EnumerateSteps(GridPos initPos, GridPos initDirection, int modeSwitchRate, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, float upwardRate, Grid<bool> path) {
        GridPos smallPos = initPos;
        GridPos smallMove = initDirection;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        
        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), 1/6f, smallPos, new GridPos[] {}, Vector3.zero);

        Parameters p = new Parameters(0);

        bool justFlipped = false;

        int modeSwitchCountdown = 1;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            if (--modeSwitchCountdown <= 0) {
                modeSwitchCountdown = modeSwitchRate;
                p.StartInterpolation(1 - p.targetMode, 1f / modeSwitchRate);
            }
            p.Interpolate();

            etherCurrent += GridPos.RandomHoriz(biasToLeaveStartLocation);
            float currentFactor = p.current <= 1 ? p.current / inertiaOfEtherCurrent
                : 1 / ((2 - p.current) * (inertiaOfEtherCurrent - 1) + 1);
            // Debug.Log("Ether current factor " + etherCurrent.HComponents * currentFactor + " at " + smallPos);
            if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
                Debug.Log("Ether current " + etherCurrent + " at " + smallPos + " FLIPPED: TAKE NOTE");
                etherCurrent /= -2;
                justFlipped = true;
            } else justFlipped = false;
            
            smallMove = GridPos.Random(2/3f, etherCurrent.HComponents * currentFactor, upwardRate);
            int? walkablePathCorrection = overlapsPathAt(path, smallPos + smallMove.Horizontal, p);
            if (walkablePathCorrection is int correction) {
                if (Mathf.Abs(correction) <= 1) smallMove.w = correction;
                else if (Mathf.Abs(correction) == 3) smallMove.w = correction / -3;
            }
            smallPos += smallMove;
            if (walkablePathCorrection == 2 || walkablePathCorrection == -2) Debug.Log("walkablePathCorrection " + walkablePathCorrection + " at " + smallPos);

            List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
            newCave.Add(CaveGrid.Mod.Cave(smallPos, walkablePathCorrection != null ? 1 : p.vScale - 1));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            foreach (CaveGrid.Mod mod in newCave) CaveGrid.Biome.Next(mod.pos);
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), 1, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
        }
    }
}
