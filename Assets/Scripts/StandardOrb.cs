using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class StandardOrb : MonoBehaviour
{
    public GameObject spawnLocation;
    public float unchargeTime = 30f;
    public float chargeTime = 450f;
    public float spawnTime = 1f;
    public float haloIntensity = 1.5f;
    public float currentChargeLevel = 1.0f;
    public float spawnState = 1.0f;
    public float heldIntensity = 0.4f;
    public ColorTransition[] colorTransitions = {
        new ColorTransition(1.0f, new Color(1.0f, 1.0f, 1.0f)),
        new ColorTransition(0.6f, new Color(1.0f, 0.75f, 0.0f)),
        new ColorTransition(0.2f, new Color(0.5f, 0.125f, 0.0f))
    };

    private Light myLight;
    private Light halo;

    // Start is called before the first frame update
    void Start() {
        myLight = gameObject.transform.Find("Point Light").GetComponent<Light>();
        halo = gameObject.transform.Find("Halo").GetComponent<Light>();
        UpdateOrbState();
    }

    // Update is called once per frame
    void Update() {
        UpdateOrbState();
    }

    // Responds to message sent by PickMeUp
    void UpdateHeldState(float heldState) {
        SetOrbIntensity(1 - (1 - heldIntensity) * heldState);
    }

    private void UpdateOrbState() {
        if (spawnState < 1) {
            if (spawnState >= 0) {
                spawnState += Time.deltaTime / spawnTime;
                spawnState = Mathf.Min(1, spawnState);
                transform.localScale = Vector3.one * spawnState;
                SetOrbIntensity(spawnState);
            } else {
                spawnState += Time.deltaTime / spawnTime;
                if (spawnState < 0) {
                    transform.localScale = Vector3.one * -spawnState;
                } else {
                    if (spawnLocation == null) {
                        GameObject.Destroy(gameObject);
                    } else {
                        if(gameObject.GetComponent<Holdable>().IsHeld) {
                            gameObject.GetComponent<Holdable>().Drop();
                        }
                        transform.position = spawnLocation.transform.position;
                        currentChargeLevel = 1.0f;
                        SetOrbColor(GetColorFromCharge());
                        SetOrbIntensity(0);
                    }
                }
            }
        } else {
            if(gameObject.GetComponent<Holdable>().IsHeld) {
                if (currentChargeLevel > 0f) {
                    currentChargeLevel -= Time.deltaTime / unchargeTime;
                    if (currentChargeLevel < 0f) {
                        currentChargeLevel = 0f;
                        spawnState = -1;
                    }
                    SetOrbColor(GetColorFromCharge());
                }
            } else {
                if (currentChargeLevel < 1f) {
                    currentChargeLevel += Time.deltaTime / chargeTime;
                    if (currentChargeLevel > 1f) {
                        currentChargeLevel = 1f;
                    }
                    SetOrbColor(GetColorFromCharge());
                }
            }
        }
    }

    public void SetOrbIntensity(float intensity) {
        myLight.intensity = intensity;
        halo.intensity = haloIntensity * intensity;
    }

    private void SetOrbColor(Color color) {
        halo.color = color;
        myLight.color = color;
    }

    private Color GetColorFromCharge() {
        if (currentChargeLevel >= colorTransitions[0].frame) {
            return colorTransitions[0].color;
        }

        float chargeSubLevel, r, g, b;

        for (int i = 1; i < colorTransitions.Length; i++) {
            if (currentChargeLevel >= colorTransitions[i].frame) {
                ColorTransition higher = colorTransitions[i - 1];
                ColorTransition lower = colorTransitions[i];
                chargeSubLevel = (currentChargeLevel - lower.frame)
                        / (higher.frame - lower.frame);
                r = lower.color.r + (higher.color.r - lower.color.r) * chargeSubLevel;
                g = lower.color.g + (higher.color.g - lower.color.g) * chargeSubLevel;
                b = lower.color.b + (higher.color.b - lower.color.b) * chargeSubLevel;

                return new Color(r, g, b);
            }
        }

        ColorTransition lowestToBlack = colorTransitions[colorTransitions.Length - 1];
        chargeSubLevel = currentChargeLevel / lowestToBlack.frame;
        r = lowestToBlack.color.r * chargeSubLevel;
        g = lowestToBlack.color.g * chargeSubLevel;
        b = lowestToBlack.color.b * chargeSubLevel;

        return new Color(r, g, b);
    }

    [Serializable]
    public struct ColorTransition {
        public float frame;
        public Color color;

        public ColorTransition (float frame, Color color) {
            this.frame = frame;
            this.color = color;
        }
    }
}
