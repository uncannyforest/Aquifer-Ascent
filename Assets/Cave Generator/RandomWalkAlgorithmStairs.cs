using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomWalkAlgorithmStairs {

    public static void FollowWall(ref GridPos smallPos, ref GridPos smallMove, ref bool huggingRight) {
        bool wallAhead = !CaveGrid.Grid[smallPos + smallMove];
        GridPos besideMove = wallAhead ^ huggingRight ? smallMove.RotateRight() : smallMove.RotateLeft();
        bool wallBeside = !CaveGrid.Grid[smallPos + besideMove];
        if (wallAhead && wallBeside) {
            // Debug.Log("Return to cave to the " + (huggingRight ? "right" : "left"));
            smallMove = besideMove; // return to cave
        } else if (wallAhead || wallBeside) {
            // Debug.Log("Hugging wall to the " + (huggingRight ? "right" : "left"));
            if (Random.value < (wallAhead ? 2/3f : 1/3f)) smallMove = besideMove;
        } else { // open area
            // Debug.Log("Open area");
            if (Randoms.CoinFlip) smallMove = besideMove;
            huggingRight = Randoms.CoinFlip;
        }
        smallPos += smallMove;
    }

    public static IEnumerable<RandomWalkAlgorithm.Output> MoveVertically(GridPos initPos, GridPos initDirection, bool up) {
        int verticalDirection = (up ? 1 : -1);

        GridPos smallPos = initPos;
        GridPos smallMove = initDirection.Horizontal;

        GridPos largePos = smallPos + smallMove + GridPos.up * (up ? 2 : -3);
        GridPos largeMove = GridPos.zero;

        bool huggingRight = Randoms.CoinFlip;

        List<GridPos> initCave = new List<GridPos>();
        initCave.Add(smallPos);
        initCave.Add(smallPos + smallMove);
        foreach (GridPos unit in GridPos.Units) {
            initCave.Add(smallPos + smallMove + unit);
        }
        initCave.Add(smallPos - 2 * GridPos.up);
        yield return new RandomWalkAlgorithm.Output(smallPos.World, smallPos, smallMove, initCave.ToArray(), new GridPos[] {}, Vector3.zero, RandomWalkAlgorithm.Output.BridgeMode.LAST);

        smallMove.w = verticalDirection;
        smallMove = huggingRight ? smallMove.RotateRight() : smallMove.RotateLeft();

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            List<GridPos> newCave = new List<GridPos>();
            newCave.Add(smallPos);
            if (smallMove.w != 0) { 
                newCave.Add(largePos);
                foreach (GridPos unit in GridPos.Units) newCave.Add(largePos + unit);
            }
            newCave.Add(smallPos - GridPos.up * 2);

            foreach (GridPos pos in newCave) CaveGrid.Biome.Next(pos);
            yield return new RandomWalkAlgorithm.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), new GridPos[] {}, Vector3.zero, RandomWalkAlgorithm.Output.BridgeMode.LAST);
            
            smallMove.w = Random.value < 1/6f ? 0 : verticalDirection;
            if (smallMove.w != 0) {
                // Debug.Log("Moving up, adding layer");
                largeMove = Random.value < 1/6f ? GridPos.RandomHoriz() : GridPos.zero;
                largePos += largeMove + GridPos.up * verticalDirection;
            } else {
                // Debug.Log("Not moving up this time");
            }

            FollowWall(ref smallPos, ref smallMove, ref huggingRight);
        }
    }

    public static IEnumerable<RandomWalkAlgorithm.Output> MoveHorizontally(GridPos initPos, GridPos initDirection, Vector3 biasToFleeStartLocation, float upwardRate) {
        float smallUpwardRate = .5f;

        GridPos smallPos = initPos;
        GridPos smallMove = initDirection.Horizontal;

        GridPos largePos = smallPos + smallMove * 1;
        GridPos largeMove = initDirection;

        bool huggingRight = Randoms.CoinFlip;

        bool catchingUpHorizontally = false;
        smallMove = huggingRight ? smallMove.RotateRight() : smallMove.RotateLeft();

        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            List<GridPos> newCave = new List<GridPos>();
            newCave.Add(smallPos);
            if (!catchingUpHorizontally) for (int i = -3; i <= 1; i += 1) {
                newCave.Add(largePos + GridPos.up * i);
                foreach (GridPos unit in GridPos.Units) if (i == -1 || Random.value < 1/3f) newCave.Add(largePos + unit + GridPos.up * i);
            }
            newCave.Add(smallPos - GridPos.up * 2);

            foreach (GridPos pos in newCave) CaveGrid.Biome.Next(pos);
            yield return new RandomWalkAlgorithm.Output(smallPos.World, smallPos, smallMove, newCave.ToArray(), new GridPos[] {}, Vector3.zero, RandomWalkAlgorithm.Output.BridgeMode.LAST);
            
            GridPos catchUp = largePos - smallPos;
            catchingUpHorizontally = catchUp.Horizontal.Magnitude > 3;

            if (catchUp.w > 0) {
                // Debug.Log("Moving up to catch up");
                smallMove.w = 1;
            } else if (catchUp.w < -1) {
                // Debug.Log("Moving down to catch up");
                smallMove.w = -1;
            } else {
                smallMove.w = Random.value > 1/3f ? 0 : ((Random.value < smallUpwardRate) ? 1 : -1);
            }


            if (!catchingUpHorizontally) {
                if (Random.value < 1/6f) largeMove = largeMove.RandomHorizDeviation((biasToFleeStartLocation - catchUp.HComponents).MaxNormalized());
                largeMove.w = Random.value > 1/6f ? 0 : Random.value < upwardRate ? 1 : -1;
                largePos += largeMove;

                bool tooClose = (largePos - smallPos).Horizontal.Magnitude < 2;
                if (!tooClose) FollowWall(ref smallPos, ref smallMove, ref huggingRight);
            } else {
                // Debug.Log("Not moving large this time, catching up " + catchUp.HComponents.MaxNormalized());
                int vert = smallMove.w;
                smallMove = GridPos.RoundFromVector3(catchUp.HComponents.MaxNormalized());
                smallMove.w = vert;
                smallPos += smallMove;
                huggingRight = Randoms.CoinFlip;
            }

            // Debug.Log("Small move " + smallMove);
        }
    }
}
