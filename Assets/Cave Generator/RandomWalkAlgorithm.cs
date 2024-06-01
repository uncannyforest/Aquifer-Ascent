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
        public bool iOddsAreBridge;

        public Output(Vector3 location, GridPos[] newCave, GridPos[] interesting, Vector3 etherCurrent, bool iZeroIsBridge) {
            this.location = location;
            this.newCave = newCave;
            this.interesting = interesting;
            this.etherCurrent = etherCurrent;
            this.iOddsAreBridge = iZeroIsBridge;
        }
    }

    public static IEnumerable<Output> EnumerateSteps(int inertiaOfEtherCurrent, int changeBiomeEvery, float biasToLeaveCenterOfGravity, float upwardRate) {
        GridPos position = GridPos.zero;
        TriPos triPosition = new TriPos(GridPos.zero, false);
        int currentHScale = 0;
        int currentVScale = 0;
        GridPos lastMove = GridPos.E;
        GridPos etherCurrent = new GridPos(0, inertiaOfEtherCurrent / 2, 0);
        int levelOut = 0;
        bool lastMoveBridge = false;
        bool justFlipped = false;

        GridPos lastEndPos = GridPos.zero;
        GridPos lastEndEther = new GridPos(0, -inertiaOfEtherCurrent - 1, 0);

        int nextBiomeCount = 10; // changeBiomeEvery;
        int biome = 1;//Random.Range(1, CaveGrid.Biome.floors.Length);

        CaveGrid.Biome.Next(position, (_) => biome, true);
        yield return new Output(position.World, new GridPos[] {position}, new GridPos[] {}, Vector3.zero, false);
        for (int infiniteLoopCatch = 0; infiniteLoopCatch < 100000; infiniteLoopCatch++) {
            // if (biomeTries == 108) {
            //     justFlipped = false;
            //     position = lastEndPos;
            //     etherCurrent = lastEndEther;
            //     biome = CaveGrid.Biome[position];
            //     Debug.Log("Position is now " + position + " biome is now " + biome + ": NOTE TAKEN");
            //     currentVScale = 2;
            //     currentHScale = 2;
            //     biomeTries = 1;
            //     nextBiomeCount = 6; // change it quickly
            // }
            GridPos newPosition = position;
            TriPos newTriPosition = triPosition;
            int vScale = currentVScale;
            int hScale = currentHScale;
            for (int i = 0; i <= hScale + vScale; i++) etherCurrent += GridPos.RandomHoriz(new Vector3(1, -.5f, -.5f) * biasToLeaveCenterOfGravity);
            if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
                etherCurrent /= -3;
                justFlipped = true;
            }

            List<GridPos> interesting = new List<GridPos>();
            int changeAmount = hScale == 2 || lastMoveBridge ? Random.Range(0, 2)
                : hScale == 1 ? Random.Range(0, 3)
                : 2;
            GridPos random = changeAmount == 0 ? lastMove.RandomVertDeviation(2/3f, 2/3f, upwardRate)
                : changeAmount == 1 ? lastMove.RandomHorizDeviation(etherCurrent.HComponents.MaxNormalized())
                : GridPos.Random(2/3f, etherCurrent.HComponents, upwardRate);
            if (levelOut != 0) {
                random.w = levelOut;
                Debug.Log("LEVELING OUT! " + levelOut);
            } else {
                // Debug.Log("Was " + position.w + " / Up? " + random.w);
            }
            bool hScaleChange = false;
            if (nextBiomeCount == 0) {
            // if (Random.value < .1f) {//random == lastMove && Random.value < .5f/(1 + hScale + vScale)) {
                if (hScale == 0 && vScale == 0) {
                    if (Randoms.CoinFlip) { // JUMP BIG
                        hScale = 2;
                        vScale = 2;
                    } else if (Randoms.CoinFlip) {
                        vScale = 1;
                        ToVScale1(ref newPosition);
                    } else {
                        hScale = 1;
                        newTriPosition = ToHScale1(newPosition, random);
                        hScaleChange = true;
                    }
                } else if (vScale == 2 && hScale == 2) {
                    if (Randoms.CoinFlip) { // JUMP SMALL
                        hScale = 0;
                        vScale = 0;
                    } else if (Randoms.CoinFlip) {
                        hScale = 1;
                        newTriPosition = ToHScale1(newPosition, random);
                        hScaleChange = true;
                    } else {
                        vScale = 1;
                        ToVScale1(ref newPosition);
                    }
                } else if (hScale == 1) {
                    if (vScale == 0 || vScale == 2) {
                        int seed = Random.Range(0, 4);
                        if (seed != 3) { // otherwise DON'T change!
                            hScale = seed;
                            vScale = Mathf.Abs(vScale - seed % 2);
                            if (vScale == 1) ToVScale1(ref random);
                            else {
                                hScaleChange = true;
                                newPosition = FromHScale1(newTriPosition, random);
                            }
                        }
                    } else {
                        if (Randoms.CoinFlip) vScale = Random.Range(0, 2) * 2;
                        else hScale = Random.Range(0, 2) * 2;
                        if (hScale == 1) FromVScale1(ref random);
                        else {
                            hScaleChange = true;
                            newPosition = FromHScale1(newTriPosition, random);
                        }
                    }
                } else { // hScale == 0 or 2 but not same as vScale
                    if (vScale == 0 || vScale == 2) {
                        if (Randoms.CoinFlip) { // JUMP
                            vScale = 2 - vScale;
                            hScale = 2 - hScale;
                        } else {
                            if (Randoms.CoinFlip) vScale = 1;
                            else hScale = 1;
                            if (vScale == 1) ToVScale1(ref random);
                            else {
                                hScaleChange = true;
                                newTriPosition = ToHScale1(newPosition, random);
                            }
                        }
                    } else {
                        int seed = Random.Range(0, 4);
                        if (seed != 3) { // otherwise DON'T change!
                            vScale = seed;
                            hScale = Mathf.Abs(hScale - seed % 2);
                            if (vScale != 1) FromVScale1(ref random);
                            else {
                                hScaleChange = true;
                                newTriPosition = ToHScale1(newPosition, random);
                            }
                        }
                    }
                }
                Debug.Log("New hScale " + hScale + " / new vScale" + vScale);
                biome = 1 + vScale + hScale * 3;
                nextBiomeCount = hScale == 2 ? 10 : 20;
            }
            if (!hScaleChange) {
                if (hScale == 1) {
                    newTriPosition = newTriPosition.GetAdjacent(random);
                } else {
                    newPosition += random;
                }
            }

            GridPos levelOutPos = hScale == 1 ? Randoms.InArray(newTriPosition.HorizCorners) : newPosition;
            if (vScale == 2) levelOutPos -= GridPos.up;
            levelOut = LevelOut(levelOutPos, vScale, hScale != 1 && lastMove.Horizontal != -random.Horizontal, out int? bridgeInstead);
            bool doBridge = false;
            if (bridgeInstead is int bridge) {
                doBridge = true;
                Debug.Log("BRIDGE! " + bridge);
            }

            Vector3 nextLoc;
            if (hScale == 1) nextLoc = newTriPosition.World;
            else nextLoc = newPosition.World;
            if (vScale == 1) nextLoc += GridPos.up.World * .5f;
            List<GridPos> newCave = new List<GridPos>();
            if (doBridge) {
                GridPos bridgePos = newPosition + GridPos.up * (int)bridgeInstead;
                newCave.Add(bridgePos);
                newCave.Add(bridgePos - GridPos.up * 2);
                if (!lastMoveBridge) {
                    Debug.Log("Last pos before bridge: " + position);
                    GridPos also = position;
                    also.w = Mathf.Max(position.w, bridgePos.w - 1); // not needed if bridge must == 0
                    newCave.Add(also);
                    newCave.Add(also - GridPos.up * 2);
                    if (bridgePos.w > also.w)
                        newCave.Add(also + GridPos.up);
                } else if (bridgePos.w > position.w + 1) {
                    newCave.Add(position + GridPos.up);
                }
            } else for (int i = vScale == 2 ? -1 : 0; i <= Mathf.Min(vScale, 1); i++) {
                if (hScale == 0 || (hScale == 2 && lastMoveBridge)) newCave.Add(newPosition + GridPos.up * i);
                else if (hScale == 2) {
                    newCave.Add(newPosition + GridPos.up * i);
                    foreach (GridPos unit in GridPos.Units) {
                        if (vScale == 2) {
                            if (Random.value < 1/3f) // i == 0 || 
                                newCave.Add(newPosition + unit + GridPos.up * i);
                        } else if (Random.value < 1/2f)
                            newCave.Add(newPosition + unit + GridPos.up * i);
                    }
                } else if (hScale == 4) { // not used currently
                    newCave.Add(newPosition + GridPos.up * i);
                    foreach (GridPos unit in GridPos.Units) {
                        newCave.Add(newPosition + unit + GridPos.up * i);
                    }
                    foreach (GridPos unit in GridPos.Units2) {
                        if (Random.value < 1/3f)
                            newCave.Add(newPosition + unit + GridPos.up * i);
                    }
                } else {
                    foreach (GridPos corner in newTriPosition.HorizCorners) {
                        if (vScale < 2 || i == 0 || Random.value < 1/3f) newCave.Add(corner + GridPos.up * i);
                    }
                }
            }

            if (hScale == 1) newPosition = newTriPosition.HorizCorners[Random.Range(0, 3)];

            // if (nextBiomeCount == 0) {
            //     biome = Random.Range(1, CaveGrid.Biome.floors.Length);
            //     nextBiomeCount = changeBiomeEvery;
            //     // GridPos move = GridPos.RandomHoriz(etherCurrent.HComponents / etherCurrent.HComponents.Max());
            //     // while (CaveGrid.Grid[newPosition]) {
            //     //     newPosition += move;
            //     //     newTriPosition += move;
            //     // }
            // }
            CaveGrid.Biome.Next(newPosition, (_) => biome, true);
            // if (maybeBiome == 0) {
            //     Debug.Log("Position " + newPosition + " failed (ethercurrent " + etherCurrent + "), trying again for same biome (" + biome + "), try " + (biomeTries + 1));
            //     biomeTries++;
            //     continue;
            // }
            foreach (GridPos pos in newCave) CaveGrid.Biome.Next(pos);

            yield return new Output(nextLoc, newCave.ToArray(), interesting.ToArray(), etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero), doBridge);
            lastMove = random;
            lastMoveBridge = doBridge;
            position = newPosition;
            triPosition = newTriPosition;
            currentHScale = hScale;
            currentVScale = vScale;
            nextBiomeCount--;
            if (justFlipped) {
                Debug.Log("Ether current " + etherCurrent + " at " + position + " FLIPPED: TAKE NOTE");
                lastEndPos = position;
                lastEndEther = etherCurrent * -1;
            }
            justFlipped = false;
            // biomeTries = 0;
            // Debug.Log("Moved " + lastMove + " to " + position);
        }
    }

    private static int LevelOut(GridPos pos, int vScale, bool considerBridge, out int? bridgeInstead) {
        int spaceBelow = 1; // skip poss ledge to erase unless beyond it is also ground
        int spaceAbove = 1;
        GridPos checkPos = pos - GridPos.up * 2;
        // Debug.Log("Not checking height " + (pos.w) + " through " + (pos.w + 1 + vScale));
        while (CaveGrid.I.grid[checkPos]) {
            spaceBelow++;
            checkPos -= GridPos.up;
        }
        if (spaceBelow == 1 && !CaveGrid.I.grid[pos - GridPos.up * 1]) spaceBelow = 0; // beyond poss ledge is ground
        checkPos = pos + GridPos.up * (vScale + 3);
        while (CaveGrid.I.grid[checkPos]) {
            spaceAbove++;
            checkPos += GridPos.up;
        }
        if (spaceAbove == 1 && !CaveGrid.I.grid[pos + GridPos.up * (vScale + 2)]) spaceAbove = 0;
        // if (spaceAbove + spaceBelow > 0) Debug.Log("Space above: " + spaceAbove + " space below: " + spaceBelow + " total: " + (spaceAbove + spaceBelow + vScale + 2));
        if (considerBridge && spaceBelow + spaceAbove + vScale >= 4) {
            bridgeInstead = Mathf.Max(4 - spaceBelow + (vScale == 2 ? -1 : 0), 0);
            if (bridgeInstead <= 1) {
                return 0;
            } else {
                Debug.Log("Would bridge if we started higher, factor " + bridgeInstead);
                bridgeInstead = null;
            }
            int placeboBridge = Mathf.Max(4 - spaceAbove + (vScale >= 1 ? -1 : 0), 0); // to balance up/down of random walk
            if (placeboBridge <= 1) {
                Debug.Log("Not leveling out because all the space above makes an imaginary upside-down bridge");
                return 0;
            }
        } else {
            bridgeInstead = null;
        }
        // Debug.Log("Current pos " + pos + " NCH " + pos.w + " through " + (pos.w + 1 + vScale) +
        //     " and space B/A " + spaceBelow + "/" + spaceAbove);
        if (spaceBelow > 0 && spaceAbove > 0) {
            Debug.Log("Entering open area");
            return 0;
        } else {
            return spaceBelow > 0 ? -1 : spaceAbove > 0 ? 1 : 0;
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
