using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class GuidManager : MonoBehaviour {
    private Dictionary<string, GameObject> guidToGameObject = new Dictionary<string, GameObject>();
    private Dictionary<string, int> guidToInstanceId = new Dictionary<string, int>();

    public GameObject this[string guid] {
        get => guidToGameObject[guid];
    }

    public bool Contains(string guid) {
        return guidToGameObject.ContainsKey(guid);
    }

    public bool IsRegisteredAlready(Guid guidScript) {
        bool foundKey = guidToInstanceId.TryGetValue(guidScript.id, out int id);
        if (foundKey) {
            Debug.Log(guidScript.gameObject.GetInstanceID() + " != " + id + "??");
        }
        return foundKey && guidScript.gameObject.GetInstanceID() != id;
    }

    public void Register(Guid guidScript) {
        guidToGameObject.Add(guidScript.id, guidScript.gameObject);
        guidToInstanceId.Add(guidScript.id, guidScript.gameObject.GetInstanceID());
    }

    public void Unregister(Guid guidScript) {
        guidToGameObject.Remove(guidScript.id);
        guidToInstanceId.Remove(guidScript.id);
    }
}
