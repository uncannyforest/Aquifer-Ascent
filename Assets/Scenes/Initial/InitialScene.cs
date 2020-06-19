using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialScene : ToggleableScript {

    public OrbRewardOnFinalFlower orbRewardOnFinalFlower;

    [Serializable]
    public class OrbRewardOnFinalFlower {
        public List<GameObject> flowers;
        public GameObject orbPrefab;

        private Dictionary<GameObject, bool> openFlowers;
        private int numOpenFlowers;
        private bool complete = false;

        public void Execute() {
            if (complete) {
                return;
            }
            int newNumOpenFlowers = 0;
            Dictionary<GameObject, bool> newOpenFlowers = new Dictionary<GameObject, bool>();
            foreach (GameObject flower in flowers) {
                bool isOpen = flower.GetComponent<Animator>().GetBool("IsOpen");
                newOpenFlowers.Add(flower, isOpen);
                Debug.Log(flower.name + " is " + (isOpen ? "" : "not ") + "open");
                if (isOpen) {
                    newNumOpenFlowers++;
                }
            }
            Debug.Log("Num open flowers: " + newNumOpenFlowers);

            if (newNumOpenFlowers > numOpenFlowers + 1) {
                Debug.LogError("numOpenFlowers increased from " + numOpenFlowers + " to " +
                        newNumOpenFlowers + ", this should not happen");
            }

            if (newNumOpenFlowers == 2) {
                openFlowers = newOpenFlowers; // otherwise don't bother
            } else if (newNumOpenFlowers == 3) {
                GameObject lastFlower = null;

                foreach (KeyValuePair<GameObject, bool> flower in openFlowers) {
                    if (!flower.Value) {
                        lastFlower = flower.Key;
                    }
                }

                ContainerTrigger container = lastFlower.transform.GetComponentInChildren<ContainerTrigger>();

                GameObject orb = GameObject.Instantiate(orbPrefab, container.transform);
                orb.transform.position = container.transform.position;

                container.receivingScript = orb.GetComponent<StandardOrb>();

                complete = true;
            }

            numOpenFlowers = newNumOpenFlowers;
        }
    }

    override public bool IsActive {
        set {
            if (true) { // only update if flower may be opening
                orbRewardOnFinalFlower.Execute();
            }
        }
        get => false;
    }

}
