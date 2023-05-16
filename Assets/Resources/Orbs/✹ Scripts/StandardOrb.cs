using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Holdable))]
public class StandardOrb : MonoBehaviour, State.Stateful {

    public System.Object State { get => state; set => state = (StateFields)value; }

    public StateFields state = new StateFields();
    [Serializable] public class StateFields {
        public bool isActive = true;
        public float currentChargeLevel = 1.0f;
        public bool isHoldable = true;
    }
    public bool mayNeedUpdating = true;
    public GameObject spawnLocation;
    public float unchargeTime = 30f; // set to 0 programmatically to lock color
    public float chargeTime = 450f;
    public float spawnTime = 1f;
    public float haloIntensity = 1.5f;
    public float spawnState = 0.0f;
    public bool pleaseNeverHoldMe = false;
    public float heldIntensity = 0.4f;
    public ColorTransition[] colorTransitions = {
        new ColorTransition(1.0f, new Color(1.0f, 1.0f, 1.0f)),
        new ColorTransition(0.6f, new Color(1.0f, 0.75f, 0.0f)),
        new ColorTransition(0.2f, new Color(0.5f, 0.125f, 0.0f))
    };
    public float explosionFactor = 10;

    private Light myLight;
    private Light halo;
    private FloatWanderAI wanderAI;
    private ParticleSystem childParticleSystem;
    
    private bool isDead = false;
    private bool isHoldable {
        get => state.isHoldable;
        set => state.isHoldable = value;
    }

    public bool IsActive {
        set {
            state.isActive = value;
            if (wanderAI != null) {
                wanderAI.CanMove = value;
            }
            if (!pleaseNeverHoldMe)
                IsHoldable = value; // assumes a previously inactive orb was not meant to stay unholdable
        }
        get => state.isActive;
    }

    public bool IsHoldable {
        set {
            if (isHoldable ^ value) {
                Debug.Log(gameObject.name + " was " + (isHoldable ? "" : "not ") + "holdable but now is" + (value ? "" : " not"));
                TriggerObjectDestroyer.Hide(gameObject); // must happen before untagging
                if (value) {
                    halo.tag = "CanPickUp";
                } else {
                    halo.tag = "Untagged";
                }
                TriggerObjectDestroyer.Show(gameObject); // reset so no effect when Halo tag does not matter
                isHoldable = value;
            }
        }
    }

    // Awake is called before any Start() calls in the game
    void Awake() {
        myLight = gameObject.transform.Find("Point Light").GetComponent<Light>();
        halo = gameObject.transform.Find("Halo").GetComponent<Light>();
        wanderAI = gameObject.GetComponent<FloatWanderAI>();
        UpdateOrbState();
        SetOrbColor(GetColorFromCharge());
        if (!state.isActive) {
            isHoldable = false;
            halo.tag = "Untagged";
        }
        Debug.Log(gameObject.name + " is " + (isHoldable ? "" : "not ") + "holdable");
        IsActive = state.isActive;
        childParticleSystem = gameObject.transform.GetComponentInChildren<ParticleSystem>();
        childParticleSystem.enableEmission = false;
    }

    // Update is called once per frame
    void Update() {
        UpdateOrbState();
    }

    // Responds to message sent by PickMeUp
    void UpdateHeldState(float heldState) {
        SetOrbIntensity(1 - (1 - heldIntensity) * heldState);
        if (heldState == 1) {
            childParticleSystem.enableEmission = true;
        } else if (childParticleSystem.isPlaying && childParticleSystem.enableEmission) {
            childParticleSystem.enableEmission = false;
        }
    }

    void NotifyActivate() {
        IsActive = true;
    }

    void NotifyDeactivate() {
        IsActive = false;
    }

    private void UpdateOrbState() {
        bool updateSpawnState = mayNeedUpdating ||
            (!state.isActive && spawnState > 0) || (state.isActive && spawnState < 1);
        bool updateCharge = unchargeTime != 0 && (mayNeedUpdating ||
            gameObject.GetComponent<Holdable>().IsHeld || state.currentChargeLevel < 1);
        mayNeedUpdating = updateSpawnState;

        if (updateSpawnState) {
            if (state.isActive) {
                spawnState += Time.deltaTime / spawnTime;
                if (spawnState >= 1) {
                    spawnState = 1;
                    mayNeedUpdating = false;
                }
                transform.localScale = Vector3.one * spawnState;
                SetOrbIntensity(spawnState);
            } else {
                spawnState -= Time.deltaTime / spawnTime;
                if (spawnState > 0) {
                    transform.localScale = Vector3.one * spawnState;
                    SetOrbIntensity(spawnState);
                } else if (isDead) {
                    if (spawnLocation == null) {
                        TriggerObjectDestroyer.Destroy(this.gameObject);
                    } else {
                        if(gameObject.GetComponent<Holdable>().IsHeld) {
                            gameObject.GetComponent<Holdable>().Drop();
                        }
                        transform.position = spawnLocation.transform.position;
                        state.currentChargeLevel = 1.0f;
                        SetOrbColor(GetColorFromCharge());
                        SetOrbIntensity(0);
                        childParticleSystem.emissionRate /= explosionFactor;
                        IsActive = true;
                    }
                } else {
                    spawnState = 0;
                    mayNeedUpdating = false;
                }
            }
        }
        if (updateCharge) {
            if (gameObject.GetComponent<Holdable>().IsHeld) {
                if (state.currentChargeLevel > 0f) {
                    state.currentChargeLevel -= Time.deltaTime / unchargeTime;
                }
            } else {
                if (state.currentChargeLevel < 1f || chargeTime < 0) {
                    state.currentChargeLevel += Time.deltaTime / chargeTime;
                    if (state.currentChargeLevel > 1f) {
                        state.currentChargeLevel = 1f;
                    }
                }
            }
            if (state.currentChargeLevel < 0f) {
                state.currentChargeLevel = 0f;
                Kill();
            }
            SetOrbColor(GetColorFromCharge());
        }
    }

    public void Kill() {
        state.isActive = false;
        isDead = true;
        childParticleSystem.emissionRate *= explosionFactor;
    }

    public void SetOrbIntensity(float intensity) {
        myLight.intensity = intensity;
        halo.intensity = haloIntensity * intensity;
    }

    public void MultiplyOrbIntensity(float intensity) {
        SetOrbIntensity(spawnState * intensity);
    }

    public void SetOrbColor(Color color) {
        halo.color = color;
        myLight.color = color;
    }

    public Color GetColorFromCharge() {
        if (state.currentChargeLevel >= colorTransitions[0].frame) {
            return colorTransitions[0].color;
        }

        float chargeSubLevel, r, g, b;

        for (int i = 1; i < colorTransitions.Length; i++) {
            if (state.currentChargeLevel >= colorTransitions[i].frame) {
                ColorTransition higher = colorTransitions[i - 1];
                ColorTransition lower = colorTransitions[i];
                chargeSubLevel = (state.currentChargeLevel - lower.frame)
                        / (higher.frame - lower.frame);
                r = lower.color.r + (higher.color.r - lower.color.r) * chargeSubLevel;
                g = lower.color.g + (higher.color.g - lower.color.g) * chargeSubLevel;
                b = lower.color.b + (higher.color.b - lower.color.b) * chargeSubLevel;

                return new Color(r, g, b);
            }
        }

        ColorTransition lowestToBlack = colorTransitions[colorTransitions.Length - 1];
        chargeSubLevel = state.currentChargeLevel / lowestToBlack.frame;
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
