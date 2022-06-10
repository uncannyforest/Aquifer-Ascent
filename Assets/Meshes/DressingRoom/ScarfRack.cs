using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScarfRack : MonoBehaviour {
    public int index;
    public ScarfMaterialsList materials;
    public GameObject scarfHangerPrefab;
    public MeshRenderer scarfPrefab;
    public float width = 0.4f;
    public float repeatTime = 0.5f;

    private Scarf scarf;

    private GameObject[] rackScarves;

    public bool ReadInput = false;
    public float keyDown = 0;

    // Start is called before the first frame update
    void Start() {
        scarf = GameObject.FindObjectOfType<Scarf>();
        int maxScarf = Mathf.Max(scarf.MaxScarfFound, index);
        rackScarves = new GameObject[maxScarf + 1];
        float offset = maxScarf / 2f * width;
        for (int i = 0; i <= maxScarf; i++) {
            GameObject scarfHanger = GameObject.Instantiate(scarfHangerPrefab, transform);
            scarfHanger.transform.localPosition = new Vector3(i * width - offset, 0, .001f * (i % 2));
            MeshRenderer scarf = GameObject.Instantiate(scarfPrefab, transform);
            scarf.transform.localPosition = new Vector3(i * width - offset, 0, 0);
            scarf.material = materials[i];
            rackScarves[i] = scarf.gameObject;
            scarf.gameObject.SetActive(ShowScarf(i));
        }
    }

    private bool ShowScarf(int id) {
        if (scarf.Id == id) return false;
        if (scarf.CollectionIncludes(id) || id == index) return true;
        return false;
    }

    public void AddScarf() {
        scarf.AddToCollection(index);
    }

    void Update() {
        if (ReadInput) {
            float x = SimpleInput.GetAxis("Mouse X");
            if (x  > .5 || x < -.5) {
                if (keyDown < Time.time) {
                    scarf.SwapScarf(x > 0, out int oldScarf, out int newScarf);
                    rackScarves[oldScarf].SetActive(true);
                    rackScarves[newScarf].SetActive(false);
                    keyDown = Time.time + repeatTime;
                }
            } else if (keyDown > 0) {
                keyDown = 0;
            }
        }
    }
}
