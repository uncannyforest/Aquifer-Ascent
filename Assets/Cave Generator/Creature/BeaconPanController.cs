using System.Collections.Generic;
using UnityEngine;

public class BeaconPanController : MonoBehaviour {
    public float speed = 2f;
    public Transform cameraTransform;
    public Transform root;

    private Camera camConfig;
    private List<Transform> beacons = new List<Transform>();
    private Transform last;
    private int position = 0;

    void Start() {
        camConfig = GetComponentInChildren<Camera>();
    }

    public void Add(Transform beacon) {
        beacons.Add(beacon);
    }

    public void ResetWithLast(Transform last) {
        beacons.Clear();
        beacons.Add(root);
        this.last = last;
    }

    void Update() {
        float h = SimpleInput.GetAxisRaw("Horizontal");
        float v = SimpleInput.GetAxisRaw("Vertical");

        if (h == 0 && v == 0) return;
        int approachIndex = h > 0 ? position + 1 : position;
        Transform approach = approachIndex == beacons.Count ? last : beacons[approachIndex];

        float rate = speed * Time.unscaledDeltaTime * camConfig.orthographicSize;

        if (h != 0) transform.position = Vector3.MoveTowards(transform.position, approach.position, rate);
        transform.position += v * Vector3.Scale(cameraTransform.forward + cameraTransform.up, new Vector3(1, 0, 1)).normalized * rate;

        if (transform.position == approach.position && approachIndex != 0 && approachIndex != beacons.Count)
            position += h > 0 ? 1 : -1;
    }
}
