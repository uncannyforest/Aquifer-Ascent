using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;

public class DarknessNavigate : MonoBehaviour {
	public GameObject exitPathStep;
	public GameObject parent;
	public float pathSpacing = 2f;
	public int numRecentLightsTracked = 3;
	public float successUpdateFrequency = .2f;
	public float lightFactor = 10f;
	public float pathIntensity = .75f;
	public float transitionTimeIn = 1f;
	public float transitionTimeOut = .25f;

	private bool isInEffect = false;
	private float transition = 0;
	private LinkedList<GameObject> recentLights = new LinkedList<GameObject>();
	private ThirdPersonCharacter characterScript;
	private GameObject colliderCheck;
	private List<GameObject> pathLights = new List<GameObject>();

	public bool IsInEffect {
		get => isInEffect;
	}

    void Start() {
		characterScript = GetComponent<ThirdPersonCharacter>();
		colliderCheck = transform.Find("ColliderCheck").gameObject;
    }

	void Update() {
		if (isInEffect && transition < 1) {
			transition += Time.deltaTime / transitionTimeIn;
			SetLightEffect(transition);
			foreach (GameObject light in pathLights) {
				light.GetComponent<Light>().intensity = transition * pathIntensity;
			}
		}

		else if (!isInEffect && transition > 0) {
			transition -= Time.deltaTime / transitionTimeOut;
			if (transition < 0) {
				transition = 0;
			}
			SetLightEffect(transition);
		}
	}

	public void NotifyRecentLight(GameObject light) {
		if (recentLights.Count > 0 && light == recentLights.Last.Value) {
			return;
		}

		recentLights.AddLast(light);
		while (recentLights.Count > numRecentLightsTracked) {
			recentLights.RemoveFirst();
		}
	}

	public void SetUp() {
		isInEffect = true;
        InvokeRepeating("CheckDarkness", successUpdateFrequency, successUpdateFrequency);

		colliderCheck.SetActive(true);
		colliderCheck.GetComponent<StayWithinCollider>().stayWithin = parent;

		ProducePath();
	}

	private void ProducePath() {
		Vector3 endPosition = Vector3.zero;
		while (recentLights.Count > 0 && endPosition == Vector3.zero) {
			GameObject recentLight = recentLights.First.Value;
			recentLights.RemoveFirst();

			if (recentLight != null && recentLight.activeInHierarchy
					&& recentLight.GetComponent<StandardOrb>().IsActive) {
        		Physics.Raycast(recentLight.transform.position, Vector3.down, out RaycastHit ground);
				endPosition = ground.point;
			}
		}
		if (endPosition == Vector3.zero) {
			Debug.LogError("No valid recent lights");
		}

		NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(this.transform.position, endPosition, NavMesh.AllAreas, path);
		Debug.Log("Path corners:" + path.corners.Length);

		Vector3 currentPosition = this.transform.position;
		int nextCorner = 1;
		int safeCrash = 100;
		while(safeCrash > 0) {
            GameObject pathStep = GameObject.Instantiate(exitPathStep, parent.transform);
			pathStep.transform.position = currentPosition;

			pathLights.Add(pathStep.transform.GetChild(0).gameObject);

			float moveDistanceLeft = pathSpacing;
			Vector3 newPosition = Vector3.MoveTowards(currentPosition, path.corners[nextCorner], moveDistanceLeft);

			while (newPosition == path.corners[nextCorner] && safeCrash > 0) {
				nextCorner++;
				if (nextCorner == path.corners.Length) {
					return; // Done! Stop here!
				}

				moveDistanceLeft -= Vector3.Distance(currentPosition, newPosition);
				currentPosition = newPosition;
				newPosition = Vector3.MoveTowards(currentPosition, path.corners[nextCorner], moveDistanceLeft);
				safeCrash--;
			}

			currentPosition = newPosition;
			safeCrash--;
		}

		Debug.LogError("Unable to exit loop");
	}

	private void SetLightEffect(float on) {
		float intensity = Mathf.Lerp(1, lightFactor, on);
			
		StandardOrb[] orbs = FindObjectsOfType<StandardOrb>();
		foreach (StandardOrb orb in orbs) {
			orb.MultiplyOrbIntensity(intensity);
		};
	}

	private void CheckDarkness() {
		if (!characterScript.IsApproachingDarkness) {
			TearDown();
		}
	}

	private void TearDown() {
		isInEffect = false;
		CancelInvoke();

		colliderCheck.SetActive(false);

		foreach (Transform child in parent.transform) {
			GameObject.Destroy(child.gameObject);
		}
	}
}
