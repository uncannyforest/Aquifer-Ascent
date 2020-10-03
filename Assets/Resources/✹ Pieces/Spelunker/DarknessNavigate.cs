using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

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
	public Vector3 earlyFallback;

	private bool isInEffect = false;
	private float transition = 0;
	private LinkedList<GameObject> recentLights = new LinkedList<GameObject>();
	private LinkedList<string> recentSceneTransitions = new LinkedList<string>();
	private ThirdPersonCharacter characterScript;
	private GameObject colliderCheck;
	private List<GameObject> pathLights = new List<GameObject>();

	public bool IsInEffect {
		get => isInEffect;
	}

    void Start() {
		characterScript = GetComponent<ThirdPersonCharacter>();
		colliderCheck = transform.Find("ColliderCheck").gameObject;
		SceneManager.activeSceneChanged += OnActiveSceneChanged;
		SceneManager.sceneLoaded += OnSceneLoaded;
    }

	void Update() {
		if (isInEffect && transition < 1) {
			transition += Time.deltaTime / transitionTimeIn;
			SetLightEffect(transition);
		}

		else if (!isInEffect && transition > 0) {
			transition -= Time.deltaTime / transitionTimeOut;
			if (transition < 0) {
				transition = 0;
			}
			SetLightEffect(transition);
		}
	}

	void OnActiveSceneChanged(Scene current, Scene next) {
		NotifyActiveScene(next);
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
		if (IsInEffect) {
			Debug.Log("Continuing path");
			foreach (Transform child in parent.transform) {
				GameObject.Destroy(child.gameObject);
			}
			pathLights = new List<GameObject>();
			ProducePath();
			SetLightEffect(transition);
		}
	}

	public void NotifyActiveScene(Scene activeScene) {
		string name = activeScene.name;

		if (IsInEffect ||
				(recentSceneTransitions.Count > 0 && recentSceneTransitions.Last.Value == name)) {
			return;
		}

		recentSceneTransitions.AddLast(name);
		Debug.Log("Scene tracked for DarknessNavigate: " + name);
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

	private Vector3 FindRecentLight() {
		while (recentLights.Count > 0) {
			GameObject recentLight = recentLights.First.Value;
			recentLights.RemoveFirst();

			if (recentLight != null && recentLight.activeInHierarchy
					&& recentLight.GetComponent<StandardOrb>().IsActive) {
        		Physics.Raycast(recentLight.transform.position, Vector3.down, out RaycastHit ground);
				return ground.point;
			}
		}

		Debug.Log("No recent lights, checking scene portals . . .");
		return FindRecentSceneTransition();
	}

	private Vector3 FindRecentSceneTransition() {
		while (recentSceneTransitions.Count > 0) {
			string recentSceneTransition = recentSceneTransitions.First.Value;
			recentSceneTransitions.RemoveFirst();

			Debug.Log("Recent scene: " + recentSceneTransition);

			if (recentSceneTransition == SceneManager.GetActiveScene().name) {
				continue;
			}

        	ScenePortal[] portals = GameObject.FindObjectsOfType<ScenePortal>();
			foreach (ScenePortal portal in portals) {
				Debug.Log("Checking against scene portal: " + portal.otherScene);
				if (portal.open && portal.otherScene == recentSceneTransition) {
        			Physics.Raycast(portal.transform.position, Vector3.down, out RaycastHit ground);
					return ground.point;
				}
			}
		}

		Debug.LogWarning("No valid recent lights or scene");
		return earlyFallback;
	}

	private void ProducePath() {
		Vector3 endPosition = FindRecentLight();

		NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(this.transform.position, endPosition, NavMesh.AllAreas, path);
		Debug.Log("Path corners:" + path.corners.Length);
		if (path.corners.Length == 0) {
			Debug.LogError("No path!? Is navigation mesh baked?");
		}

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
					// Done! Stop here!
					return;
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

		foreach (GameObject light in pathLights) {
			light.GetComponent<Light>().intensity = transition * pathIntensity;
		}
	}

	private void CheckDarkness() {
		if (!characterScript.IsApproachingDarkness) {
			TearDown();
		}
	}

	private void TearDown() {
		isInEffect = false;
		CancelInvoke();
		NotifyActiveScene(SceneManager.GetActiveScene());

		colliderCheck.SetActive(false);

		foreach (Transform child in parent.transform) {
			GameObject.Destroy(child.gameObject);
		}
		pathLights = new List<GameObject>();
	}
}
