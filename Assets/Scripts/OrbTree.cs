using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrbTree : MonoBehaviour
{
    public int fractalLevel = 6;
    public int numBranches = 4;
    public Vector3 branchPosition = new Vector3(1, 0, 0);
    public Vector3 branchRotation = new Vector3(0, 0, 30);
    public Vector3 branchScale = new Vector3(.7f, .55f, .55f);
    public int trunkBonusBranches = 1;
    public float targetActiveBuds = 2.5f;
    public float numBranchesCorrectionRate = .6f; // chance that # is tweaked toward targetActiveBuds
    public float branchMissRate = .1f;
    public float budDormancyRate = 1/3f;
    public float heightJitter = .15f;
    public float rotationJitter = 10;
    public float scaleJitter = .05f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject prototype = gameObject.transform.Find("Prototype").gameObject;
        GameObject trunk = GameObject.Instantiate(prototype, this.transform, false);
        trunk.SetActive(true);
        trunk.name = "Trunk";
        List<GameObject> activeBuds = new List<GameObject>();
        List<GameObject> dormantBuds = new List<GameObject>();
        activeBuds.Add(trunk);
        for (int i = 0; i < fractalLevel; i++) {
            List<GameObject> newBuds = dormantBuds;

            foreach (GameObject branch in activeBuds) {
                int currentNumBranches = numBranches;
                if (activeBuds.Count == 1) {
                    currentNumBranches += trunkBonusBranches;
                }

                // I'm really proud of this block.  It auto-corrects for extremes in total branch numbers.
                // In other words, unusually low numbers of branches towards the base will result in higher
                // numbers later, and vice versa.
                while (Random.value < numBranchesCorrectionRate) {
                    if (Random.value < .5f * activeBuds.Count / Mathf.Pow(targetActiveBuds, i)) {
                        currentNumBranches--;
                    } else {
                        currentNumBranches++;
                    }
                }

                for (int j = 0; j < currentNumBranches; j++) {
                    if (Random.value > branchMissRate) {
                        GameObject newBranch = GameObject.Instantiate(prototype, branch.transform, false);
                        newBranch.SetActive(true);
                        newBranch.name = "Branch";
                        newBranch.transform.localPosition = branchPosition - Random.value * heightJitter * Vector3.right;
                        newBranch.transform.localScale = branchScale + new Vector3(
                                (Random.value * 2 - 1) * scaleJitter,
                                (Random.value * 2 - 1) * scaleJitter,
                                (Random.value * 2 - 1) * scaleJitter);
                        Vector3 rotationEuler = branchRotation
                            + (j * 360f / currentNumBranches + (Random.value * 2 - 1) * rotationJitter) * Vector3.right
                            + (Random.value * 2 - 1) * rotationJitter * Vector3.forward;
                        newBranch.transform.localRotation = Quaternion.Euler(rotationEuler);
                        newBuds.Add(newBranch);
                    }
                }
            }
            
            activeBuds = new List<GameObject>();
            dormantBuds = new List<GameObject>();
            foreach (GameObject bud in newBuds) {
                if (Random.value > budDormancyRate) {
                    activeBuds.Add(bud);
                } else {
                    dormantBuds.Add(bud);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
