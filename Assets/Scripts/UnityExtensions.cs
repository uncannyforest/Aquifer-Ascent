using System;
using System.Collections;
using UnityEngine;

public static class GameObjectExtensions {
    public static T GetComponentStrict<T>(this Transform t) {
        return t.GetComponent<T>() ?? throw new ArgumentException
            ("Transform " + t + " in layer " + t.gameObject.layer + " has no " + typeof(T));
    }

    public static T GetComponentStrict<T>(this Collider2D c) {
        return c.GetComponent<T>() ?? throw new ArgumentException
            ("Collider " + c + " in layer " + c.gameObject.layer + " has no " + typeof(T));
    }

    public static T GetComponentStrict<T>(this MonoBehaviour mb) {
        return mb.GetComponent<T>() ?? throw new ArgumentException
            ("MonoBehaviour " + mb + " in layer " + mb.gameObject.layer + " has no " + typeof(T));
    }

    public static T GetComponentStrict<T>(this GameObject go) {
        return go.GetComponent<T>() ?? throw new ArgumentException
            ("GameObject " + go + " in layer " + go.layer + " has no " + typeof(T));
    }

    public static bool Contains(this LayerMask mask, int layer) {
        return mask == (mask | (1 << layer));
    }

    public static bool LayerIsIn(this GameObject go, params string[] layerNames) {
        return ((LayerMask)LayerMask.GetMask(layerNames)).Contains(go.layer);
    }

    // copied from https://discussions.unity.com/t/tip-invoke-any-function-with-delay-also-with-parameters/810392
    public static void Invoke(this MonoBehaviour mb, Action f, float delay) {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }
    private static IEnumerator InvokeRoutine(System.Action f, float delay) {
        yield return new WaitForSeconds(delay);
        f();
    }
}

public static class TransformExtensions {
    // copied from http://answers.unity.com/answers/509669/view.html
    public static void SetLayer(this Transform transform, int layer)  {
        transform.gameObject.layer = layer;
        foreach(Transform child in transform)
                child.SetLayer(layer);
    }

    public static Bounds GetLocalBounds(this Transform transform) {
        return new Bounds(transform.localPosition, transform.localScale);
    }
}