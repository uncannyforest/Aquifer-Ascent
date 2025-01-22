using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalkAlgorithmStairs {

    public static void FollowWall(ref GridPos smallPos, ref GridPos smallMove, ref bool huggingLeft, bool? up) {
        bool wallAhead = !CaveGrid.Grid[smallPos + smallMove];
        GridPos besideMove = wallAhead ^ huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight();
        bool wallBeside = !CaveGrid.Grid[smallPos + besideMove];
        if (wallAhead && wallBeside) {
            GridPos besideMoveOtherWay = huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight();
            bool wallBesideOtherWay = !CaveGrid.Grid[smallPos + besideMoveOtherWay];
            if (wallBesideOtherWay) {
                Debug.Log("Return to cave to the " + (huggingLeft ? "left" : "right"));
                smallMove = besideMove; // return to cave
                if (up == true) smallMove.w = 1;
                else if (up == false) smallMove.w = -1;
            } else {
                Debug.Log("Found opening in surprise direction");
                smallMove = besideMoveOtherWay;
                huggingLeft = !huggingLeft;
            }
        } else if (wallAhead || wallBeside) {
            Debug.Log("Hugging wall to the " + (huggingLeft ? "left" : "right"));
            if (Random.value < 1/2f) smallMove = besideMove;
        } else { // open area
            GridPos besideMoveDouble = huggingLeft ? besideMove.RotateLeft() : besideMove.RotateRight();
            bool wallBesideDouble = !CaveGrid.Grid[smallPos + besideMoveDouble];
            if (wallBesideDouble) {
                Debug.Log("Hugging wall as it curves out");
                smallMove = besideMove;
            } else {
                Debug.Log("Open area");
                if (Randoms.CoinFlip) smallMove = besideMove;
                huggingLeft = Randoms.CoinFlip;
            }
        }
        smallPos += smallMove;
    }

    public static int GetVerticalScaleForBiome() => new int[] {0, 2, -1, 2, -1, 0, 1, 2, 1, 0, -1, 1}[CaveGrid.Biome.lastBiome - 1];

    public static IEnumerable<RandomWalk.Output> MoveVertically(GridPos initPos, GridPos initDirection, bool up) {
        int verticalDirection = (up ? 1 : -1);

        GridPos smallPos = initPos;
        GridPos smallMove = initDirection.Horizontal;

            // 3, 6, 12, 2,
            // 5, 1, 7, 4,
            // 11, 10, 9, 8
        int scale = GetVerticalScaleForBiome();
        Debug.Log("Stairwell scale " + scale);
        float units2Chance = scale == 0 ? 1/24f : scale == 1 ? 1/3f : 5/12f;
        float flatChance = scale == -1 ? 1/6f : scale == 0 ? 1/3f : scale == 1 ? 1/2f : 2/3f;

        GridPos initMove = smallMove * (scale >= 1 ? 2 : 1);
        GridPos largePos = smallPos + initMove + GridPos.up * (up ? 2 : -3);
        GridPos largeMove = GridPos.zero;

        bool huggingLeft = Randoms.CoinFlip;

        List<CaveGrid.Mod> initCave = new List<CaveGrid.Mod>();
        initCave.Add(CaveGrid.Mod.Cave(smallPos));
        initCave.Add(CaveGrid.Mod.Cave(smallPos + initMove));
        foreach (GridPos unit in GridPos.Units) initCave.Add(CaveGrid.Mod.Cave(smallPos + initMove + unit));

        initCave.Add(CaveGrid.Mod.Wall(smallPos - 2 * GridPos.up));
        yield return new RandomWalk.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), Biomes.NoChange, 1/6f, smallPos, null, Vector3.zero, RandomWalk.Output.BridgeMode.LAST);

        smallMove.w = verticalDirection;
        // smallMove = huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight();

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
            newCave.Add(CaveGrid.Mod.Cave(smallPos));
            if (smallMove.w != 0) { 
                newCave.Add(CaveGrid.Mod.Cave(largePos));
                foreach (GridPos unit in GridPos.Units) if (scale >= 0 || Random.value < 1/3f) newCave.Add(CaveGrid.Mod.Cave(largePos + unit));
                if (scale >= 0) foreach (GridPos unit in GridPos.ListAllWithMagnitude(2)) if (Random.value < units2Chance) newCave.Add(CaveGrid.Mod.Cave(largePos + unit));
            }
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));

            float speed = (11 - scale)/9f;
            yield return new RandomWalk.Output(smallPos.World, smallPos, huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight(), newCave.ToArray(), Biomes.NoChange, speed, smallPos, null, Vector3.zero, RandomWalk.Output.BridgeMode.LAST);
            
            smallMove.w = Random.value < flatChance ? 0 : verticalDirection;

            FollowWall(ref smallPos, ref smallMove, ref huggingLeft, up);

            if (smallMove.w != 0) {
                // Debug.Log("Moving up, adding layer");
                largeMove = Random.value < 1/6f ? GridPos.RandomHoriz() : GridPos.zero;
                largePos += largeMove + GridPos.up * verticalDirection;
            } else {
                // Debug.Log("Not moving up this time");
            }
        }
    }

    public static IEnumerable<RandomWalk.Output> MoveHorizontally(GridPos initPos, GridPos initDirection, int modeSwitchRate, Vector3 biasToFleeStartLocation, float upwardRate) {
        float smallUpwardRate = .5f;

        GridPos smallPos = initPos;
        GridPos smallMove = initDirection.Horizontal;

        GridPos largePos = smallPos + smallMove * 1;
        GridPos largeMove = initDirection;

        int biome = CaveGrid.Biome.lastBiome;
            // 3, 1, 6, 12, 2, 4,
            // 5, 11, 10, 8, 9, 7
        int hScale = new int[] {0, 1, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1}[biome - 1];
        int vScale = new int[] {0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1, 0}[biome - 1];
        Debug.Log("Walkway hScale " + hScale + " / vScale " + vScale);
        int nextScaleCount = modeSwitchRate * 2;

        bool huggingLeft = Randoms.CoinFlip;
        bool justCaughtUp = false;

        bool catchingUpHorizontally = false;
        // smallMove = huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight();
        bool waiting = false;

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
            newCave.Add(CaveGrid.Mod.Cave(smallPos));
            if (!catchingUpHorizontally) {//for (int i = -3 - vScale; i <= 1 + vScale; i += 1) {
                int maxExtraFloor = 2 + vScale;
                int maxExtraRoof = 2 + vScale;
                GridPos largeCenter = largePos - GridPos.up;
                newCave.Add(CaveGrid.Mod.Cave(largeCenter - maxExtraFloor * GridPos.up, maxExtraRoof + maxExtraFloor + 1));
                foreach (GridPos unit in GridPos.Units) {
                    if (hScale == 1) newCave.Add(CaveGrid.Mod.Cave(largeCenter + unit - maxExtraFloor * GridPos.up, maxExtraRoof + maxExtraFloor + 1));
                    else newCave.Add(CaveGrid.Mod.RandomVerticalExtension(largeCenter + unit, 0, maxExtraFloor, 0, maxExtraRoof));
                }
                if (hScale == 1) foreach (GridPos unit in GridPos.ListAllWithMagnitude(2)) newCave.Add(CaveGrid.Mod.RandomVerticalExtension(largeCenter + unit, 0, maxExtraFloor, 0, maxExtraRoof));
            }
            newCave.Add(CaveGrid.Mod.Wall(smallPos - GridPos.up * 2));

            float speed = waiting ? 1/6f : 1f;
            yield return new RandomWalk.Output(smallPos.World, smallPos, huggingLeft ? smallMove.RotateLeft() : smallMove.RotateRight(), newCave.ToArray(), Biomes.NoChange, speed, smallPos, null, Vector3.zero, RandomWalk.Output.BridgeMode.LAST);

            if (nextScaleCount-- == 0) {
                if (Randoms.CoinFlip) hScale = 1 - hScale;
                else vScale = 1 - vScale;
                Debug.Log("Walkway hScale " + hScale + " / vScale " + vScale);
                nextScaleCount = modeSwitchRate * 2;
                biome = new int[] {
                    3, 1, 6, 12, 2, 4,
                    5, 11, 10, 8, 9, 7
                }[vScale * 6 + hScale * 3 + Random.Range(0, 3)];
                CaveGrid.Biome.Next(smallPos, (_) => biome, true);
            }

            waiting = false;
            GridPos catchUp = largePos - smallPos;
            catchingUpHorizontally = catchUp.Horizontal.Magnitude > 3 + hScale * 2;

            if (catchUp.w > 0 + vScale) {
                Debug.Log("Moving up to catch up");
                smallMove.w = 1;
            } else if (catchUp.w < -1 - vScale) {
                Debug.Log("Moving down to catch up");
                smallMove.w = -1;
            } else {
                smallMove.w = Random.value > 1/3f ? 0 : ((Random.value < smallUpwardRate) ? 1 : -1);
            }


            if (!catchingUpHorizontally && !justCaughtUp) {
                if (Random.value < 1/6f) largeMove = largeMove.RandomHorizDeviation((biasToFleeStartLocation * 3 - catchUp.HComponents).MaxNormalized());
                largeMove.w = Random.value > 1/6f ? 0 : Random.value < upwardRate ? 1 : -1;
                largePos += largeMove;

                bool tooClose = (largePos - smallPos).Horizontal.Magnitude < 2 + hScale * 2;
                if (!tooClose) FollowWall(ref smallPos, ref smallMove, ref huggingLeft, null);
                else {
                    waiting = true;
                    Debug.Log("Waiting for large to move ahead");
                }
            } else {
                justCaughtUp = catchingUpHorizontally;
                Debug.Log("Not moving large this time, catching up " + catchUp.HComponents.MaxNormalized());
                int vert = smallMove.w;
                smallMove = GridPos.RoundFromVector3(catchUp.HComponents.MaxNormalized());
                smallMove.w = vert;
                smallPos += smallMove;
                huggingLeft = Randoms.CoinFlip;
            }

            // Debug.Log("Small move " + smallMove);
        }
    }
}
