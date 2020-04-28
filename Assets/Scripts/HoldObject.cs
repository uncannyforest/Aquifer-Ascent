using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HoldObject : MonoBehaviour
{
    private Transform playerHoldTransform;
    private HashSet<GameObject> nearObjects = new HashSet<GameObject>();
    Animator m_Animator;
    private float heldObjectWidth;
    public float handWeightCurrent = 0;
    public float transitionTime = .5f;

    // Start is called before the first frame update
    void Awake()
    {
        playerHoldTransform = gameObject.transform.Find("HoldLocation");
        m_Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SimpleInput.GetButtonDown("Interact1")) {
            Interact();
        }
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<Holdable>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<Holdable>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
            nearObjects.Add(objectToPickUp);
            UpdateInteractionMessages();
        }
    }

	void OnTriggerExit(Collider other) {
        if(other.tag == "CanPickUp") {
            GameObject objectToPickUp;
            if(other.gameObject.GetComponent<Holdable>() != null) {
                objectToPickUp = other.gameObject;
            } else if (other.transform.parent.GetComponent<Holdable>() != null) {
                objectToPickUp = other.transform.parent.gameObject;
            } else {
                Debug.LogError("Object tagged CanPickUp has no PickMeUp script on it or parent");
                return;
            }
			bool foundObject = nearObjects.Remove(objectToPickUp);
            if (!foundObject) {
                Debug.LogWarning("Tried to remove object from nearObjects that was not there");
            }
            UpdateInteractionMessages();
		}
    }
    
    void Interact() {
        if (playerHoldTransform.childCount > 0) {
            DropAnyPickedUpObjects();
            return;
        }

        if (nearObjects.Count == 0) {
            return;
        }

        GameObject closestObject = nearObjects.OrderBy(
                o => Vector3.Distance(o.transform.position, gameObject.transform.position)
            ).First();

        heldObjectWidth = closestObject.GetComponent<Holdable>().GetColliderWidth();

        closestObject.GetComponent<Holdable>().PickUp();
        UpdateInteractionMessages();
    }

    void OnAnimatorIK()
    {
        if(playerHoldTransform.childCount < 1 && handWeightCurrent == 0){
            return;
        }

        Vector3 rightHandlePosition = playerHoldTransform.position + (.5f * heldObjectWidth * this.transform.right);
        Vector3 leftHandlePosition = playerHoldTransform.position - (.5f * heldObjectWidth * this.transform.right);

        if(playerHoldTransform.childCount > 0) { // if you're holding something 
            if( handWeightCurrent < 1f ){
                handWeightCurrent += Time.deltaTime / transitionTime;
            }else{
                handWeightCurrent = 1f;
            }
        }else{
            // Let the hands relax :)

            if( handWeightCurrent > 0f ){
                handWeightCurrent -= Time.deltaTime / transitionTime;
            }else{
                handWeightCurrent = 0f;
            }
        }

        m_Animator.SetIKPositionWeight(AvatarIKGoal.RightHand,handWeightCurrent);
        m_Animator.SetIKRotationWeight(AvatarIKGoal.RightHand,handWeightCurrent); 	
        m_Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand,handWeightCurrent);
        m_Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand,handWeightCurrent); 

        m_Animator.SetIKPosition(AvatarIKGoal.RightHand,rightHandlePosition);
        m_Animator.SetIKRotation(AvatarIKGoal.RightHand,playerHoldTransform.rotation);
        m_Animator.SetIKPosition(AvatarIKGoal.LeftHand,leftHandlePosition);
        m_Animator.SetIKRotation(AvatarIKGoal.LeftHand,playerHoldTransform.rotation);  

    }
    
    void DropAnyPickedUpObjects() {
        foreach (Transform child in playerHoldTransform) {
            Holdable childPickMeUp = child.GetComponent<Holdable>();
            if (childPickMeUp == null) {
                Debug.LogWarning("Child of playerHold had no PickMeUp script!");
            } else {
                childPickMeUp.SetDown();
            }
            UpdateInteractionMessages();
        }
    }

    void UpdateInteractionMessages() {
        List<string> interactionMessages = new List<string>();
        if (playerHoldTransform.childCount > 0) {
            interactionMessages.Add("release");
        } else if (nearObjects.Count > 0) {
            interactionMessages.Add("beckon");
        }


#if (UNITY_IOS || UNITY_ANDROID)
        GameObject interactNotice = GameObject.Find("Mobile")
                .transform.Find("Interact/Notice").gameObject;
#else
        GameObject interactNotice = GameObject.Find("Nonmobile")
                .transform.Find("Interact Notice").gameObject;
#endif 

        if (interactionMessages.Count > 0) {
            interactNotice.SetActive(true);

            // TODO: handle multiple nearby objects
            string interactionMessage = interactionMessages[0];
            
#if (UNITY_IOS || UNITY_ANDROID)
            interactionMessage = char.ToUpper(interactionMessage[0]) + interactionMessage.Substring(1);
#else
            interactionMessage = "press <color=white>x</color> to <color=white>"
                + interactionMessage + "</color>";
#endif 
            interactNotice.transform.Find("Text").gameObject.GetComponent<Text>().text = interactionMessage;
        } else {
            interactNotice.SetActive(false);
        }
    }

}
