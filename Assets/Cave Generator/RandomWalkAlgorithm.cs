using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static IEnumerable<Output> EnumerateSteps(int inertiaOfEtherCurrent) {
        GridPos position = GridPos.zero;
        TriPos triPosition = new TriPos(GridPos.zero, false);
        int hScale = 0;
        int vScale = 0;
        GridPos lastMove = GridPos.zero;
        GridPos etherCurrent = new GridPos(0, inertiaOfEtherCurrent / 2, 0);
        bool debugJustFlipped = false;

        GridPos? revisit = null;
        yield return new Output(position.World, new GridPos[] {position}, new GridPos[] {}, Vector3.zero);
        while (true) {
            debugJustFlipped = false;
            etherCurrent += GridPos.RandomHoriz();
            if (Mathf.Abs(etherCurrent.x) > inertiaOfEtherCurrent
                    || Mathf.Abs(etherCurrent.y) > inertiaOfEtherCurrent
                    || Mathf.Abs(etherCurrent.z) > inertiaOfEtherCurrent) {
                etherCurrent /= -2;
                Debug.Log("Ether current " + etherCurrent + " + FLIPPED");
                debugJustFlipped = true;
            }

            List<GridPos> interesting = new List<GridPos>();
            int changeAmount = lastMove == GridPos.zero ? 2
                : hScale == 0 ? 2
                : hScale + vScale == 1 ? Random.Range(0, 3)
                : hScale == vScale ? Random.Range(0, 2)
                : 2 - Mathf.FloorToInt(Mathf.Sqrt(Random.Range(0, 9)));
            GridPos random = changeAmount == 0 ? lastMove.RandomVertDeviation(hScale == 1 && vScale == 1 ? 1/27f : 1/9f)
                : changeAmount == 1 ? lastMove.RandomHorizDeviation(etherCurrent.HComponents / etherCurrent.HComponents.Max())
                : GridPos.Random(hScale == 0 ? 1/3f : 1/9f, etherCurrent.HComponents / etherCurrent.HComponents.Max());
            bool hScaleChange = false;
            if (hScale == 1) position = triPosition.HorizCorners[Random.Range(0, 3)];
            if (false && random == -lastMove) {
                interesting.Add(position);
                if (revisit is GridPos doRevisit) {
                    position = doRevisit;
                    revisit = null;
                } else {
                    revisit = position;
                }
            } else if (random == lastMove && Random.value < 1/3f) {
                if (hScale == 0) {
                    if (Random.Range(0, 2) == 0) {
                        hScale = 2;
                        vScale = 2;
                        Debug.Log("JUMPED BIG!");
                    } else {
                        hScale = 1;
                        triPosition = ToHScale1(position, random);
                        hScaleChange = true;
                    }
                } else if (vScale == 2) {
                    if (Random.Range(0, 2) == 0) {
                        hScale = 0;
                        vScale = 0;
                        Debug.Log("JUMPED SMALL!");
                    } else {
                        vScale = 1;
                        ToVScale1(ref position);
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
                            position = FromHScale1(triPosition, random);
                        }
                    } else {
                        if (Random.Range(0, 2) == 0) vScale = 0;
                        else hScale = 2;
                        if (vScale == 0) FromVScale1(ref random);
                        else {
                            hScaleChange = true;
                            position = FromHScale1(triPosition, random);
                        }
                    }
                } else {
                    if (vScale == 0) {
                        if (Random.Range(0, 2) == 0) vScale = 1;
                        else hScale = 1;
                        if (vScale == 1) ToVScale1(ref random);
                        else {
                            hScaleChange = true;
                            triPosition = ToHScale1(position, random);
                        }
                    } else {
                        int seed = Random.Range(0, 3);
                        vScale = seed;
                        hScale = 2 - seed % 2;
                        if (hScale == 2) FromVScale1(ref random);
                        else {
                            hScaleChange = true;
                            triPosition = ToHScale1(position, random);
                        }
                    }
                }
                Debug.Log("New hScale " + hScale + " / new vScale" + vScale);
            }
            if (!hScaleChange) {
                if (hScale == 1) {
                    triPosition = triPosition.GetAdjacent(random);
                } else {
                    position += random;
                }
            }
            Vector3 nextLoc;
            if (hScale == 1) nextLoc = triPosition.World;
            else nextLoc = position.World;
            if (vScale == 1) nextLoc += GridPos.up.World * .5f;
            List<GridPos> newCave = new List<GridPos>();
            for (int i = vScale == 2 ? -1 : 0; i <= Mathf.Min(vScale, 1); i++) {
                if (hScale == 0) newCave.Add(position);
                else if (hScale == 2) {
                    newCave.Add(position + GridPos.up * i);
                    foreach (GridPos unit in GridPos.Units) {
                        if (vScale == 2) {
                            if (i == 0 || Random.value < 1/3f)
                                newCave.Add(position + unit + GridPos.up * i);
                        } else if (Random.value < 1/2f)
                            newCave.Add(position + unit + GridPos.up * i);
                    }
                } else {
                    foreach (GridPos corner in triPosition.HorizCorners)
                        if (vScale == 0 || Random.value < .5f)
                            newCave.Add(corner + GridPos.up * i);
                }
            }
            yield return new Output(nextLoc, newCave.ToArray(), interesting.ToArray(), etherCurrent.World / inertiaOfEtherCurrent + (debugJustFlipped ? Vector3.up : Vector3.zero));
            lastMove = random;
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
}
