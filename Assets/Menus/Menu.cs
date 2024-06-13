using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class Menu : MonoBehaviour {
    public Button buttonPrefab;
    abstract public string[] Buttons { get; }

    void Start() {
        foreach (Transform child in transform) Destroy(child.gameObject);
        for (int i = 0; i < Buttons.Length; i++) {
            Button button = GameObject.Instantiate(buttonPrefab, transform);
            button.GetComponentInChildren<Text>().text = Buttons[i];
            int num = i; // C# requires this for parameters to anonymous functions created in a loop
            button.onClick.AddListener(() => PressAction(num));
        }
    }

    private void PressAction(int num) {
        OnPressed(num);
        gameObject.SetActive(false);
    }

    abstract public void OnPressed(int num);
}
