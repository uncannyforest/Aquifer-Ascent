using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkAlgorithm {

    public struct Output {
        public Vector3 location;
        public GridPos[] newCave;
        public GridPos[] interesting;
        public Vector3 etherCurrent;

        public Output(Vector3 location, GridPos[] newCave, GridPos[] interesting, Vector3 etherCurrent) {
            this.location = location;
            this.newCave = newCave;
            this.interesting = interesting;
            this.etherCurrent = etherCurrent;
        }
    }

    public static IEnumerable<Output> EnumerateSteps(int inertiaOfEtherCurrent, int changeBiomeEvery, float biasToLeaveCenterOfGravity) {
        GridPos position = GridPos.zero;
        TriPos triPosition = new TriPos(GridPos.zero, false);
        int currentHScale = 0;
        int currentVScale = 0;
        GridPos lastMove = GridPos.zero;
        GridPos etherCurrent = new GridPos(0, inertiaOfEtherCurrent / 2, 0);
        bool justFlipped = false;

        GridPos lastEndPos = GridPos.zero;
        GridPos lastEndEther = new GridPos(0, -inertiaOfEtherCurrent - 1, 0);

        int nextBiomeCount = changeBiomeEvery;
        int biome = 1;
        int biomeTries = 0;

        Vector3 centerOfGravitySum = Vector3.zero;
        int centerOfGravityDenominator = 1;

        CaveGrid.Biome.Next(position, (_) => biome, true);
        yield return new Output(position.World, new GridPos[] {position}, new GridPos[] {}, Vector3.zero);
        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            if (biomeTries == 108) {
                justFlipped = false;
                position = lastEndPos;
                etherCurrent = lastEndEther;
                biome = CaveGrid.Biome[position];
                Debug.Log("Position is now " + position + " biome is now " + biome + ": NOTE TAKEN");
                currentVScale = 2;
                currentHScale = 2;
                biomeTries = 1;
                nextBiomeCount = 6; // change it quickly
            }
            GridPos newPosition = position;
            TriPos newTriPosition = triPosition;
            int vScale = currentVScale;
            int hScale = currentHScale;
            Vector3 relativeToCenterOfGravity = position.HComponents - centerOfGravitySum / centerOfGravityDenominator;
            for (int i = 0; i <= hScale + vScale; i++) etherCurrent += GridPos.RandomHoriz(relativeToCenterOfGravity.MaxNormalized() * biasToLeaveCenterOfGravity);
            if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
                etherCurrent /= -3;
                justFlipped = true;
            }
            Debug.Log("Center of gravity: " + (centerOfGravitySum / centerOfGravityDenominator));

            List<GridPos> interesting = new List<GridPos>();
            int changeAmount = lastMove == GridPos.zero ? 2
                : hScale == 0 ? 2
                : hScale + vScale == 1 ? Random.Range(0, 3)
                : hScale == vScale ? Random.Range(0, 2)
                : 2 - Mathf.FloorToInt(Mathf.Sqrt(Random.Range(0, 9)));
            GridPos random = changeAmount == 0 ? lastMove.RandomVertDeviation(hScale == 1 && vScale == 1 ? 1/27f : 1/9f)
                : changeAmount == 1 ? lastMove.RandomHorizDeviation(etherCurrent.HComponents.MaxNormalized())
                : GridPos.Random(hScale == 0 ? 1/3f : 1/9f, etherCurrent.HComponents.MaxNormalized());
            bool hScaleChange = false;
            if (random == lastMove && Random.value < .5f/(1 + hScale + vScale)) {
                if (hScale == 0) {
                    if (Random.Range(0, 2) == 0) {
                        hScale = 2;
                        vScale = 2;
                        Debug.Log("JUMPED BIG!");
                    } else {
                        hScale = 1;
                        newTriPosition = ToHScale1(newPosition, random);
                        hScaleChange = true;
                    }
                } else if (vScale == 2) {
                    if (Random.Range(0, 2) == 0) {
                        hScale = 0;
                        vScale = 0;
                        Debug.Log("JUMPED SMALL!");
                    } else {
                        vScale = 1;
                        ToVScale1(ref newPosition);
                    }
                } else if (hScale == 1) {
                //     hScale = Random.Range(0, 2) * 2;
                //     position = FromHScale1(triPosition, random);
                // } else if (vScale == 0) {
                //     vScale = Random.Range(0, 2);
                //     hScale = 1 + vScale;
                //     if (hScale == 1) triPosition = ToHScale1(position, random);
                //     else {
                //         hScaleChange = false;
                //         ToVScale1(ref random);
                //     }
                // } else {
                //     vScale = 0;
                //     hScaleChange = false;
                //     FromVScale1(ref random);
                    if (vScale == 0) {
                        int seed = Random.Range(0, 3);
                        hScale = seed;
                        vScale = seed % 2;
                        if (vScale == 1) ToVScale1(ref random);
                        else {
                            hScaleChange = true;
                            newPosition = FromHScale1(newTriPosition, random);
                        }
                    } else {
                        if (Random.Range(0, 2) == 0) vScale = 0;
                        else hScale = 2;
                        if (vScale == 0) FromVScale1(ref random);
                        else {
                            hScaleChange = true;
                            newPosition = FromHScale1(newTriPosition, random);
                        }
                    }
                } else {
                    if (vScale == 0) {
                        if (Random.Range(0, 2) == 0) vScale = 1;
                        else hScale = 1;
                        if (vScale == 1) ToVScale1(ref random);
                        else {
                            hScaleChange = true;
                            newTriPosition = ToHScale1(newPosition, random);
                        }
                    } else {
                        int seed = Random.Range(0, 3);
                        vScale = seed;
                        hScale = 2 - seed % 2;
                        if (hScale == 2) FromVScale1(ref random);
                        else {
                            hScaleChange = true;
                            newTriPosition = ToHScale1(newPosition, random);
                        }
                    }
                }
                Debug.Log("New hScale " + hScale + " / new vScale" + vScale);
            }
            if (!hScaleChange) {
                if (hScale == 1) {
                    newTriPosition = newTriPosition.GetAdjacent(random);
                } else {
                    newPosition += random;
                }
            }
            Vector3 nextLoc;
            if (hScale == 1) nextLoc = newTriPosition.World;
            else nextLoc = newPosition.World;
            if (vScale == 1) nextLoc += GridPos.up.World * .5f;
            List<GridPos> newCave = new List<GridPos>();
            for (int i = vScale == 2 ? -1 : 0; i <= Mathf.Min(vScale, 1); i++) {
                if (hScale == 0) newCave.Add(newPosition);
                else if (hScale == 2) {
                    newCave.Add(newPosition + GridPos.up * i);
                    foreach (GridPos unit in GridPos.Units) {
                        if (vScale == 2) {
                            if (i == 0 || Random.value < 1/3f)
                                newCave.Add(newPosition + unit + GridPos.up * i);
                        } else if (Random.value < 1/2f)
                            newCave.Add(newPosition + unit + GridPos.up * i);
                    }
                } else {
                    foreach (GridPos corner in newTriPosition.HorizCorners)
                        if (vScale == 0 || Random.value < .5f)
                            newCave.Add(corner + GridPos.up * i);
                }
            }

            if (hScale == 1) newPosition = newTriPosition.HorizCorners[Random.Range(0, 3)];

            if (nextBiomeCount == 0) {
                biome = Random.Range(1, CaveGrid.Biome.floors.Length);
                nextBiomeCount = changeBiomeEvery;
                GridPos move = GridPos.RandomHoriz(etherCurrent.HComponents / etherCurrent.HComponents.Max());
                while (CaveGrid.Grid[newPosition]) {
                    newPosition += move;
                    newTriPosition += move;
                }
            }
            int maybeBiome = CaveGrid.Biome.Next(newPosition, (_) => biome, false);
            if (maybeBiome == 0) {
                Debug.Log("Position " + newPosition + " failed (ethercurrent " + etherCurrent + "), trying again for same biome (" + biome + "), try " + (biomeTries + 1));
                biomeTries++;
                continue;
            }
            foreach (GridPos pos in newCave) CaveGrid.Biome.Next(pos);

            yield return new Output(nextLoc, newCave.ToArray(), interesting.ToArray(), etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero));
            lastMove = random;
            position = newPosition;
            triPosition = newTriPosition;
            currentHScale = hScale;
            currentVScale = vScale;
            if (biomeTries == 0) nextBiomeCount--;
            if (justFlipped) {
                Debug.Log("Ether current " + etherCurrent + " at " + position + " FLIPPED: TAKE NOTE");
                lastEndPos = position;
                lastEndEther = etherCurrent * -1;
            }
            justFlipped = false;
            biomeTries = 0;
            centerOfGravitySum += position.HComponents;
            centerOfGravityDenominator++;
            Debug.Log("Moved " + lastMove + " to " + position);
        }
    }

    private static void ToVScale1(ref GridPos random) {
        if (random.w == 0) random.w = Random.Range(-1, 1);
        else if (random.w == 1) random.w = 0;
    }

    private static void FromVScale1(ref GridPos random) {
        if (random.w == 0) random.w = Random.Range(0, 2);
        else if (random.w == -1) random.w = 0;
    }

    private static TriPos ToHScale1(GridPos position, GridPos random) {
        GridPos h = random.Horizontal;
        int v = random.w;
        int unit = h.ToUnitRotation();
        unit = (unit + Random.Range(0, 2)) % 6;
        return (position + GridPos.up * v).Triangles[unit];
    }

    private static GridPos FromHScale1(TriPos triPosition, GridPos random) {
        GridPos h = random.Horizontal;
        int v = random.w;
        if (triPosition.right) {
            if (h == GridPos.D || h == GridPos.E)
                return triPosition.hexPos + GridPos.E + GridPos.up * v;
            if (h == GridPos.W || h == GridPos.Q)
                return triPosition.hexPos + GridPos.W + GridPos.up * v;
            return triPosition.hexPos + GridPos.up * v;
        } else {
            if (h == GridPos.E || h == GridPos.W)
                return triPosition.hexPos + GridPos.W + GridPos.up * v;
            if (h == GridPos.Q || h == GridPos.A)
                return triPosition.hexPos + GridPos.Q + GridPos.up * v;
            return triPosition.hexPos + GridPos.up * v;
        }
    }

    
    class ModInteger {
        public int value;
        public ModInteger(int value) {
            this.value = value;
        }
        public static implicit operator int(ModInteger mi) => mi.value;
    }

    private static Func<int, int> NextBiome(bool change) {
        return (prevBiome) => {
            if (change) return Random.Range(1, CaveGrid.Biome.floors.Length);
            else return prevBiome;
        };
    }
}
