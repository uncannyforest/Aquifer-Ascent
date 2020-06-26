using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class State : MonoBehaviour {

    public string createNewFromResource; // if null, modify object based on guid (must be present)
    public bool includePosition;
    public bool includeParent;

    private GameLoader manager;
    private GuidManager guidManager;
    private Guid guid; // may be null if createNewFromPrefab is not
    private List<Stateful> statefulComponents = new List<Stateful>();

    private bool IsUnique { get => (createNewFromResource == "" || createNewFromResource == null); }

    void Awake() {
        manager = GameObject.FindObjectOfType<GameLoader>();
        manager.Register(this); 
        guidManager = GameObject.FindObjectOfType<GuidManager>();
    }

    void Start() {
        guid = GetComponent<Guid>();
    }

    public void RegisterComponent(Stateful component) {
        statefulComponents.Add(component);
    }
    
    void OnDestroy() {
        manager.Unregister(this);
    }

    public Data SaveObject() {
        if (IsUnique) {
            return Data.CreateForUniqueObject(guid.id, GetState());
        } else {
            Debug.Log("Saving prefab " + createNewFromResource);
            return Data.CreateForPrefabInstance(createNewFromResource, GetState());
        }
    }

    public static IEnumerator<State> LoadObject(Data data, GuidManager guidManager) {
        GameObject loadedObject;
        if (data.IsUnique) {
            loadedObject = guidManager[data.Guid];
        } else {
            Debug.Log("Loading prefab " + data.PrefabPath);
            GameObject newObject = Resources.Load<GameObject>(data.PrefabPath);
            Debug.Log(newObject);
            loadedObject = GameObject.Instantiate(newObject);
        }
        State stateScript = loadedObject.GetComponent<State>();
        yield return stateScript;
        stateScript.SetState(data.State);
        yield return stateScript;
    }

    private Dictionary<Type, System.Object> GetState() {
        var dictionary = new Dictionary<Type, System.Object>();

        if (includeParent) {
            dictionary.Add(typeof(Transform), transform.parent.GetComponent<Guid>().id);
        }
        if (includePosition) {
            dictionary.Add(typeof(Vector3), transform.position);
        }

        foreach (Stateful component in statefulComponents) {
            dictionary.Add(component.GetType(), component.State);
        }
        return dictionary;
    }

    private void SetState(Dictionary<Type, System.Object> state) {
        if (includeParent) {
            transform.parent = guidManager[(string)(state[typeof(Transform)])].transform;
        }
        if (includePosition) {
            transform.position = (Vector3)(state[typeof(Vector3)]);
        }

        foreach (Stateful component in statefulComponents) {
            System.Object value = state[component.GetType()];
            component.State = value;
        }
    }

    public interface Stateful {
        System.Object State { get; set ;}
    }

    [Serializable]
    public class Data {
        [SerializeField] public string Guid { get; }
        [SerializeField] public string PrefabPath { get; }
        [SerializeField] public Dictionary<Type, System.Object> State;

        public bool IsUnique { get => (Guid != null); }

        private Data(string guid, string prefabPath, Dictionary<Type, System.Object> state) {
            this.Guid = guid;
            this.PrefabPath = prefabPath;
            this.State = state;
        }

        public static Data CreateForUniqueObject(string guid, Dictionary<Type, System.Object> state) {
            return new Data(guid, null, state);
        }

        public static Data CreateForPrefabInstance(string prefabPath, Dictionary<Type, System.Object> state) {
            return new Data(null, prefabPath, state);
        }
    }
}