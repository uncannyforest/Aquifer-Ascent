using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StandardOrb : MonoBehaviour
{
    public float unchargeTime = 20f;
    public float chargeTime = 480f;
    public float maxHaloIntensity = 0.5f;
    public float maxLightIntensity = 0.5f;
    public float currentChargeLevel = 1.0f;

    private Light myLight;
    private Component halo;

    // Start is called before the first frame update
    void Start() {
        myLight = gameObject.transform.Find("Point Light").GetComponent<Light>();
        halo = gameObject.transform.Find("Sphere").GetComponent("Halo");
    }

    // Update is called once per frame
    void Update() {
        if(gameObject.GetComponent<PickMeUp>().PickedUp) {
            if (currentChargeLevel > 0f) {
                currentChargeLevel -= Time.deltaTime / unchargeTime;
                if (currentChargeLevel < 0f) {
                    currentChargeLevel = 0f;
                }
                setOrbColor(getColorFromCharge());
            }
        } else {
            if (currentChargeLevel < 1f) {
                currentChargeLevel += Time.deltaTime / chargeTime;
                if (currentChargeLevel > 1f) {
                    currentChargeLevel = 1f;
                }
                setOrbColor(getColorFromCharge());
            }
        }
    }

    void setOrbColor(Color color) {
        UnityEditor.SerializedObject serializedHalo = new UnityEditor.SerializedObject(halo);
        serializedHalo.FindProperty("m_Color").colorValue = color;
        serializedHalo.ApplyModifiedProperties();

        myLight.color = color;
    }

    Color getColorFromCharge() {
        float chargeSubLevel, r, g, b;
        if (currentChargeLevel <= 0.2) {
            chargeSubLevel = currentChargeLevel / 0.2f;
            r = 0.5f * chargeSubLevel;
            g = 0.125f * chargeSubLevel;
            b = 0.0f;
        } else if (currentChargeLevel < 0.6) {
            chargeSubLevel = (currentChargeLevel - 0.2f) / 0.4f;
            r = 0.5f + (1.0f - 0.5f) * chargeSubLevel;
            g = 0.125f + (0.75f - 0.125f) * chargeSubLevel;
            b = 0.0f;
        } else {
            chargeSubLevel = (currentChargeLevel - 0.6f) / 0.4f;
            r = 1.0f;
            g = 0.75f + 0.25f * chargeSubLevel;
            b = chargeSubLevel;
        }
        return new Color(r, g, b);
    }
}
