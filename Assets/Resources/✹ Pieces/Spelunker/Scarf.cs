using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scarf : MonoBehaviour {
    public ScarfMaterialsList materials;
    public SkinnedMeshRenderer scarf;
    [SerializeField] private int scarfId;
    public bool[] scarvesFound;
    private int maxScarfFound;

    void Start() {
        if (scarvesFound == null || scarvesFound.Length == 0) {
            scarvesFound = new bool[materials.Count];
            scarvesFound[0] = true;
            maxScarfFound = 0;
        } else {
            maxScarfFound = scarvesFound.Length - 1;
        }
    }

    public int Id {
        get => scarfId;
        set {
            scarf.material = materials[value];
            scarfId = value;
        }
    }

    public int MaxScarfFound => maxScarfFound;

    public void AddToCollection(int id) {
        scarvesFound[id] = true;
        maxScarfFound = Mathf.Max(id, maxScarfFound);
    }

    public bool CollectionIncludes(int id) {
        return scarvesFound[id];
    }

    public void SwapScarf(bool increase, out int oldScarf, out int newScarf) {
        int delta = increase ? 1 : -1;
        newScarf = Id + delta;
        if (newScarf < 0) newScarf = materials.Count - 1;
        if (newScarf >= materials.Count) newScarf = 0;
        while (!CollectionIncludes(newScarf)) {
            newScarf += delta;
            if (newScarf < 0) newScarf = materials.Count - 1;
            if (newScarf >= materials.Count) newScarf = 0;
        }
        oldScarf = Id;
        Id = newScarf;
    }
}
