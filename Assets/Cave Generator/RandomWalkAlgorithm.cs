using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkAlgorithm {

    public static CaveGrid.Mod[] SimpleHole(GridPos pos) => new CaveGrid.Mod[] {CaveGrid.Mod.Cave(pos)};

    public static IEnumerable<RandomWalk.Output> EnumerateSteps(GridPos initPos, GridPos initDirection, int modeSwitchRate, int inertiaOfEtherCurrent, Vector3 biasToLeaveStartLocation, float upwardRate) {
        GridPos position = initPos;
        TriPos triPosition = new TriPos(initPos, false);
        GridPos lastMove = initDirection;
        GridPos etherCurrent = initDirection * (inertiaOfEtherCurrent / 2);
        int levelOut = 0;
        int levelOutGapAllowed = 0;
        int lastMoveBridge = 0;
        bool justFlipped = false;

        GridPos lastEndPos = GridPos.zero;
        GridPos lastEndEther = new GridPos(0, -inertiaOfEtherCurrent - 1, 0);

        int biome = CaveGrid.Biome.lastBiome;
            // 1, 6, 12, 2,
            // 5, 3, 7, 4,
            // 11, 10, 8, 9
        int currentHScale = new int[] {0, 3, 1, 3, 0, 1, 2, 2, 3, 1, 0, 2}[biome - 1];
        int currentVScale = new int[] {0, 0, 1, 1, 1, 0, 1, 2, 2, 2, 2, 0}[biome - 1];
        Debug.Log("RW hScale " + currentHScale + " / vScale" + currentVScale);
        int nextBiomeCount = currentHScale == 1 ? modeSwitchRate * 6 : modeSwitchRate; // formerly changeBiomeEvery;

        CaveGrid.Biome.Next(position, (_) => biome, true);
        yield return new RandomWalk.Output(position.World, position, lastMove, SimpleHole(position), 1/6f, position, new GridPos[] {}, Vector3.zero);
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
            for (int i = 0; i <= hScale + vScale; i++) etherCurrent += GridPos.RandomHoriz(biasToLeaveStartLocation);
            if (etherCurrent.HComponents.Max() > inertiaOfEtherCurrent) {
                etherCurrent /= -3;
                justFlipped = true;
            }

            List<GridPos> interesting = new List<GridPos>();
            int changeAmount = hScale >= 2 || lastMoveBridge > 0 ? Random.Range(0, 2)
                : hScale == 1 ? Random.Range(0, 3)
                : 2;
            GridPos random = changeAmount == 0 ? lastMove.RandomVertDeviation(2/3f, 2/3f, upwardRate > .5f ? 1 : upwardRate)
                : changeAmount == 1 ? lastMove.RandomHorizDeviation(etherCurrent.HComponents.MaxNormalized())
                : GridPos.Random(2/3f, etherCurrent.HComponents, hScale == 1 && upwardRate > .5f ? 1 : upwardRate);
            if (levelOut != 0) {
                random.w = levelOut;
                Debug.Log("LEVELING OUT! " + levelOut);
            } else {
                // Debug.Log("Was " + position.w + " / Up? " + random.w);
            }
            bool hScaleChange = false;
            if (nextBiomeCount == 0) {
            // if (Random.value < .1f) {//random == lastMove && Random.value < .5f/(1 + hScale + vScale)) {
                int maxHScale = 3;
                int realMaxVScale = 3;
                int fakeMaxVScale = 2;
                int oldHScale = hScale;
                int oldVScale = vScale;
                int deltaHScale = 0;
                int deltaVScale = 0;
                if (Random.value < 1/3f) deltaHScale = Randoms.Sign;
                else deltaVScale = Randoms.Sign;
                hScale += deltaHScale;
                vScale += deltaVScale;
                if (vScale > fakeMaxVScale) {
                    if (deltaVScale == 1) {
                        if (vScale > realMaxVScale) { // just exceeded real max
                            vScale = fakeMaxVScale;
                        } else { // just exceeded fake max
                            if (Randoms.CoinFlip) {
                                hScale = maxHScale - hScale;
                                vScale = 0;
                            }
                        }
                    } else {
                        hScale = Mathf.Clamp(hScale, 0, maxHScale);
                    }
                } else if (hScale < 0 || vScale < 0 || hScale > maxHScale) {
                    hScale = Mathf.Clamp(hScale, 0, maxHScale);
                    vScale = Mathf.Clamp(vScale, 0, fakeMaxVScale);
                    if ((hScale == 0 || hScale == maxHScale) && (vScale == 0 || vScale == fakeMaxVScale)) {
                        // jump scale
                        hScale = maxHScale - hScale;
                        vScale = fakeMaxVScale - vScale; 
                    }
                }

                if (oldVScale != 1 && vScale == 1) ToVScale1(ref random);
                if (oldVScale == 1 && vScale != 1) FromVScale1(ref random);
                if (oldHScale != 1 && hScale == 1) {
                    hScaleChange = true;
                    newTriPosition = ToHScale1(newPosition, random);
                } else if (oldHScale == 1 && hScale != 1) {
                    hScaleChange = true;
                    newPosition = FromHScale1(newTriPosition, random);
                }
                Debug.Log("RW hScale " + hScale + " / vScale" + vScale);
                biome = new int[] {
                    1, 6, 12, 2,
                    5, 3, 7, 4,
                    11, 10, 8, 9
                }[(vScale % 3) * 4 + hScale];
                nextBiomeCount = hScale == 1 ? modeSwitchRate * 6 : modeSwitchRate;
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
            if (vScale == 3) levelOutPos -= 2 * GridPos.up;
            levelOutGapAllowed = hScale == 0 ? 0 : (levelOutGapAllowed + 1) % 3; // to balance going wrong direction vs jumps too big
            levelOut = LevelOut(levelOutPos, vScale == 3 ? 4 : vScale, levelOutGapAllowed % 2, hScale != 1 && lastMove.Horizontal != -random.Horizontal, out int? bridgeInstead);
            bool doBridge = false;
            if (bridgeInstead is int bridge) {
                doBridge = true;
                Debug.Log("BRIDGE! " + bridge);
            }

            Vector3 nextLoc;
            if (hScale == 1) nextLoc = newTriPosition.World;
            else nextLoc = newPosition.World;
            if (vScale == 1) nextLoc += GridPos.up.World * .5f;
            List<CaveGrid.Mod> newCave = new List<CaveGrid.Mod>();
            float speed = hScale == 0 ? 2/3f + vScale * vScale / 27f : new float[] {1/6f, .75f, 2/3f}[hScale - 1];
            GridPos? onPath = null;
            if (doBridge) {
                GridPos bridgePos = newPosition + GridPos.up * (int)bridgeInstead;
                newCave.Add(CaveGrid.Mod.Cave(bridgePos));
                newCave.Add(CaveGrid.Mod.Wall(bridgePos - GridPos.up * 2));
                onPath = bridgePos;
                if (lastMoveBridge == 0) {
                    Debug.Log("Last pos before bridge: " + position);
                    GridPos also = position;
                    int stepSize = hScale > 2 ? 2 : 1;
                    also.w = Mathf.Max(position.w, bridgePos.w - stepSize); // not needed if bridge must == 0
                    newCave.Add(CaveGrid.Mod.Cave(also, bridgePos.w > also.w ? 2 : 1));
                    newCave.Add(CaveGrid.Mod.Wall(also - GridPos.up * 2));
                } else if (bridgePos.w > position.w + 1) {
                    newCave.Add(CaveGrid.Mod.Cave(position + GridPos.up));
                }
            } else if (hScale >= 2 && lastMoveBridge > 0) {
                newCave.Add(CaveGrid.Mod.Cave(newPosition));
                if (lastMoveBridge > 1) {
                    newCave.Add(CaveGrid.Mod.Wall(newPosition - GridPos.up * 2));
                    onPath = newPosition;
                }
                lastMoveBridge--;
            } else { // for (int i = vScale == 3 ? -2 : vScale == 2 ? -1 : 0; i <= (vScale >= 2 ? vScale - 1 : vScale); i++) {
                int lowestFloor = vScale == 3 ? -2 : vScale == 2 ? -1 : 0;
                int roof = vScale == 3 ? 5 : vScale + 1;
                int maxExtraFloor = -lowestFloor;
                int maxExtraRoof = roof - maxExtraFloor - 1;
                if (hScale == 0) {
                    newCave.Add(CaveGrid.Mod.Cave(newPosition + GridPos.up * lowestFloor, roof));
                    onPath = newPosition + GridPos.up * lowestFloor;
                } else if (hScale == 2) {
                    newCave.Add(CaveGrid.Mod.Cave(newPosition + GridPos.up * lowestFloor, roof));
                    foreach (GridPos unit in GridPos.Units)
                        if (CaveGrid.Mod.RandomVerticalMaybe(newPosition + unit, maxExtraFloor, maxExtraRoof)
                            is CaveGrid.Mod mod) newCave.Add(mod);
                } else if (hScale == 3) {
                    newCave.Add(CaveGrid.Mod.Cave(newPosition + GridPos.up * lowestFloor, roof));
                    foreach (GridPos unit in GridPos.Units) {
                        newCave.Add(CaveGrid.Mod.Cave(newPosition + unit + GridPos.up * lowestFloor, roof));
                    }
                    foreach (GridPos unit in GridPos.Units2) {
                        if (CaveGrid.Mod.RandomVerticalMaybe(newPosition + unit, maxExtraFloor, maxExtraRoof)
                            is CaveGrid.Mod mod) newCave.Add(mod);
                    }
                } else {
                    foreach (GridPos corner in newTriPosition.HorizCorners) {
                        if (vScale < 2) newCave.Add(CaveGrid.Mod.Cave(corner, roof));
                        else newCave.Add(CaveGrid.Mod.RandomVerticalExtension(corner, 0, maxExtraFloor, 0, maxExtraRoof));
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
            foreach (CaveGrid.Mod mod in newCave) CaveGrid.Biome.Next(mod.pos);

            yield return new RandomWalk.Output(nextLoc, newPosition, random, newCave.ToArray(), speed, onPath, interesting.ToArray(), etherCurrent.World / inertiaOfEtherCurrent + (justFlipped ? Vector3.up : Vector3.zero), doBridge || lastMoveBridge >= 2 ? RandomWalk.Output.BridgeMode.ODDS : RandomWalk.Output.BridgeMode.NONE);
            lastMove = random;
            lastMoveBridge = doBridge && hScale >= 2 ? hScale - 1 : 0;
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

    private static int LevelOut(GridPos pos, int height, int gapAllowed, bool considerBridge, out int? bridgeInstead) {
        int heightDip = height / 2;
        int spaceBelow = 1; // skip poss ledge to erase unless beyond it is also ground
        int spaceAbove = 1;
        GridPos checkPos = pos - GridPos.up * 2;
        // Debug.Log("Not checking height " + (pos.w) + " through " + (pos.w + 1 + vScale));
        while (CaveGrid.CanOpen(checkPos)) {
            spaceBelow++;
            checkPos -= GridPos.up;
        }
        if (spaceBelow == 1 && !CaveGrid.CanOpen(pos - GridPos.up * 1)) spaceBelow = 0; // beyond poss ledge is ground
        checkPos = pos + GridPos.up * (height + 3);
        while (CaveGrid.CanOpen(checkPos)) {
            spaceAbove++;
            checkPos += GridPos.up;
        }
        if (spaceAbove == 1 && !CaveGrid.CanOpen(pos + GridPos.up * (height + 2))) spaceAbove = 0;
        // if (spaceAbove + spaceBelow > 0) Debug.Log("Space above: " + spaceAbove + " space below: " + spaceBelow + " total: " + (spaceAbove + spaceBelow + vScale + 2));
        if (considerBridge && spaceBelow + spaceAbove + height >= 4) {
            bridgeInstead = Mathf.Max(4 - spaceBelow - heightDip, 0);
            if (bridgeInstead <= 1) {
                return 0;
            } else {
                Debug.Log("Would bridge if we started higher, factor " + bridgeInstead);
                bridgeInstead = null;
            }
            int placeboBridge = Mathf.Max(4 - spaceAbove - heightDip, 0); // to balance up/down of random walk
            if (placeboBridge <= 1) {
                Debug.Log("Not leveling out because all the space above makes an imaginary upside-down bridge");
                return 0;
            }
        } else {
            bridgeInstead = null;
        }
        // Debug.Log("Current pos " + pos + " NCH " + pos.w + " through " + (pos.w + 1 + vScale) +
        //     " and space B/A " + spaceBelow + "/" + spaceAbove);
        if (spaceBelow > gapAllowed && spaceAbove > gapAllowed) {
            Debug.Log("Entering open area");
            return 0;
        } else {
            return spaceBelow > gapAllowed ? -1 : spaceAbove > gapAllowed ? 1 : 0;
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
