using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalkable {
    public struct Parameters {
        public float current { get; private set; } // in [0, 1]

        public Parameters(float current) {
            this.current = current;
        }
    }

    // returns int in [-3, 3]: 1 if path is one above, -1 if path is 1 below
    // 0 if no path overlap or we hit the path exactly
    public static int? overlapsPathAt(Grid<bool> path, GridPos pos) {
        if (path[pos]) return 0;
        for (int i = 1; i <= 3; i++) {
            if (path[pos + GridPos.up * i]) return i;
            if (path[pos - GridPos.up * i]) return -i;
        }
        return null;
    }

    public static IEnumerable<RandomWalk.Output> EnumerateSteps(GridPos initPos, GridPos initDirection, int modeSwitchRate, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, float upwardRate, Grid<bool> path) {
        GridPos smallPos = initPos;
        GridPos smallMove = initDirection;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        
        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), 1/6f, smallPos, new GridPos[] {}, Vector3.zero);

        Parameters p = new Parameters(1.5f);

        bool justFlipped = false;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            etherCurrent += GridPos.RandomHoriz(biasToLeaveStartLocation);
            float currentFactor = p.current <= 1 ? p.current / inertiaOfEtherCurrent
                : 1 / ((2 - p.current) * (inertiaOfEtherCurrent - 1) + 1);
            Debug.Log("Ether current factor " + etherCurrent.HComponents * currentFactor + " at " + smallPos);
            if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
                Debug.Log("Ether current " + etherCurrent + " at " + smallPos + " FLIPPED: TAKE NOTE");
                etherCurrent /= -2;
                justFlipped = true;
            } else justFlipped = false;
            
            smallMove = GridPos.Random(2/3f, etherCurrent.HComponents * currentFactor, upwardRate);
            int? walkablePathCorrection = overlapsPathAt(path, smallPos + smallMove.Horizontal);
            if (walkablePathCorrection is int correction) {
                if (Mathf.Abs(correction) <= 1) smallMove.w = correction;
                else if (Mathf.Abs(correction) == 3) smallMove.w = correction / -3;
            }
            smallPos += smallMove;
            if (walkablePathCorrection == 2 || walkablePathCorrection == -2) Debug.Log("walkablePathCorrection " + walkablePathCorrection + " at " + smallPos);

            List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
            newCave.Add(CaveGrid.Mod.Cave(smallPos));
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));
            foreach (CaveGrid.Mod mod in newCave) CaveGrid.Biome.Next(mod.pos);
            yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), 1, smallPos, new GridPos[] {}, etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
        }
    }
}
