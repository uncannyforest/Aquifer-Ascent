using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrbTree : MonoBehaviour
{
    public float growthTime = 343;
    public float growthProgress = 0;
    public int fractalLevel = 6;
    public int numBranches = 3;
    public Vector3 branchPosition = new Vector3(1, 0, 0);
    public Vector3 branchRotation = new Vector3(0, 0, 30);
    public Vector3 branchScale = new Vector3(.7f, .55f, .55f);
    public int trunkBonusBranches = 0;
    public float targetActiveBuds = 2.4f;
    public float numBranchesCorrectionRate = .1f; // chance that # is tweaked toward targetActiveBuds
    public float branchMissRate = .02f;
    public float budDormancyRate = .2f;
    public float heightJitter = .1f;
    public float rotationJitter = 7;
    public float scaleJitter = .02f;

    GameObject prototype;
    List<GameObject> activeBuds = new List<GameObject>();
    List<GameObject> endBranches = new List<GameObject>(); // each *might* grow an active bud next round

    int currentFractalLevel = 0;
    float fractalLevelFactor;

    // Start is called before the first frame update
    void Start()
    {
        prototype = gameObject.transform.Find("Prototype").gameObject;
        GameObject trunkBud = new GameObject("Plumule");
        trunkBud.transform.SetParent(this.transform, false);
        trunkBud.transform.localScale = Vector3.zero;
        GameObject trunk = GameObject.Instantiate(prototype, trunkBud.transform, false);
        trunk.SetActive(true);
        trunk.name = "Trunk";
        activeBuds.Add(trunkBud);
        endBranches.Add(trunk);
        fractalLevelFactor = ((fractalLevel + 1f) * (fractalLevel + 1f)); // don't calc this every time
    }

    void Update() {
        if (growthProgress < 1) {
            growthProgress += (Time.deltaTime / growthTime);
            growthProgress = Mathf.Min(1, growthProgress);
            this.transform.localScale = Vector3.one * Mathf.Pow(growthProgress, 1/3f);

            while(growthProgress * fractalLevelFactor >
                (currentFractalLevel + 1) * (currentFractalLevel + 1)) {
                currentFractalLevel++;
                foreach (GameObject bud in activeBuds) {
                    bud.transform.localScale = Vector3.one;
                }
                AddBranches(currentFractalLevel);
            }

            float currentFractalLevelFractionalPart = 
                (growthProgress * fractalLevelFactor - currentFractalLevel * currentFractalLevel)
                / (currentFractalLevel * 2 + 1); // this line is (cfl+1)^2 - cfl^2

            foreach (GameObject bud in activeBuds) {
                bud.transform.localScale = Vector3.one * currentFractalLevelFractionalPart;
            }
        }
    }

    void AddBranches(int currentFractalLevel) {
        List<GameObject> oldEndBranches = endBranches;
        activeBuds = new List<GameObject>();
        endBranches = new List<GameObject>();
        foreach (GameObject branch in oldEndBranches) {
            if (Random.value > budDormancyRate || oldEndBranches.Count == 1) {
                GameObject bud = new GameObject("Bud");
                bud.transform.SetParent(branch.transform, false);
                bud.transform.localPosition = branchPosition;
                bud.transform.localScale = Vector3.zero;

                activeBuds.Add(bud);
            } else {
                endBranches.Add(branch);
            }
        }
        
        foreach (GameObject bud in activeBuds) {
            int currentNumBranches = numBranches;
            if (activeBuds.Count == 1) {
                currentNumBranches += trunkBonusBranches;
            }

            // I'm really proud of this block.  It auto-corrects for extremes in total branch numbers.
            // In other words, unusually low numbers of branches towards the base will result in higher
            // numbers later, and vice versa.
            while (Random.value < numBranchesCorrectionRate) {
                if (Random.value < .5f * activeBuds.Count / Mathf.Pow(targetActiveBuds, currentFractalLevel)) {
                    currentNumBranches--;
                } else {
                    currentNumBranches++;
                }
            }

            for (int j = 0; j < currentNumBranches; j++) {
                if (Random.value > branchMissRate) {
                    GameObject newBranch = GameObject.Instantiate(prototype, bud.transform, false);
                    newBranch.SetActive(true);
                    newBranch.name = "Branch";
                    newBranch.transform.localPosition = Random.value * heightJitter * Vector3.left;
                    newBranch.transform.localScale = branchScale + new Vector3(
                            (Random.value * 2 - 1) * scaleJitter,
                            (Random.value * 2 - 1) * scaleJitter,
                            (Random.value * 2 - 1) * scaleJitter);
                    Vector3 rotationEuler = branchRotation
                        + (j * 360f / currentNumBranches + (Random.value * 2 - 1) * rotationJitter) * Vector3.right
                        + (Random.value * 2 - 1) * rotationJitter * Vector3.forward;
                    newBranch.transform.localRotation = Quaternion.Euler(rotationEuler);
                    endBranches.Add(newBranch);
                }
            }
        }
    }
}
