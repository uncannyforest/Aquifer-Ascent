using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialScene : ToggleableScript, State.Stateful {

    public System.Object State { get => state; set => state = (StateFields)value; }

    public StateFields state = new StateFields();
    [Serializable] public class StateFields {
        public bool orbRewardOnFinalFlowerIsComplete = false;
    }

    public OrbRewardOnFinalFlower orbRewardOnFinalFlower;

    [Serializable]
    public class OrbRewardOnFinalFlower {
        public List<GameObject> flowers;
        public GameObject orbToMove;

        private Dictionary<GameObject, bool> openFlowers;
        private int numOpenFlowers;

        public void Execute(StateFields state) {
            if (state.orbRewardOnFinalFlowerIsComplete) {
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

                orbToMove.transform.position = container.transform.position;
                orbToMove.GetComponent<StandardOrb>().state.isActive = true;

                state.orbRewardOnFinalFlowerIsComplete = true;
            }

            numOpenFlowers = newNumOpenFlowers;
        }
    }

    override public bool IsActive {
        set {
            if (true) { // only update if flower may be opening
                orbRewardOnFinalFlower.Execute(state);
            }
        }
    }

}
