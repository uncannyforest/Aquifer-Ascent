﻿using System;
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
    }
    public GameObject spawnLocation;
    public float unchargeTime = 30f; // set to 0 programmatically to lock color
    public float chargeTime = 450f;
    public float spawnTime = 1f;
    public float haloIntensity = 1.5f;
    public float spawnState = 0.0f;
    public float defaultIntensity = 1f;
    public float heldIntensity = 0.4f;
    public ColorTransition[] colorTransitions = {
        new ColorTransition(1.0f, new Color(1.0f, 1.0f, 1.0f)),
        new ColorTransition(0.6f, new Color(1.0f, 0.75f, 0.0f)),
        new ColorTransition(0.2f, new Color(0.5f, 0.125f, 0.0f))
    };
    public float explosionFactor = 10;
    public AudioClip explosionSound;

    private Light myLight;
    private Light halo;
    private ParticleSystem childParticleSystem;
    private Holdable holdable;
    private AudioSource objectAudio; 
    
    private bool isDead = false;

    public Action died;

    // not active if inside a container
    public bool IsActive {
        set {
            state.isActive = value;
            holdable.CanHold("active-orb", value);
        }
        get => state.isActive;
    }
    public bool IsHeld {
        get => holdable.IsHeld;
    }

    void Awake() {
        myLight = gameObject.transform.Find("Point Light").GetComponent<Light>();
        halo = gameObject.transform.Find("Halo").GetComponent<Light>();
        holdable = gameObject.GetComponent<Holdable>();
        UpdateOrbState();
        SetOrbColor(GetColorFromCharge());
        if (!state.isActive) holdable.CanHold("active-orb", false);
        IsActive = state.isActive;
        childParticleSystem = gameObject.transform.GetComponentInChildren<ParticleSystem>();
        childParticleSystem.enableEmission = false;
        objectAudio = GetComponent<AudioSource>();
    }

    void Update() {
        if (!TimeTravel.I.timePaused) UpdateOrbState();
    }

    //message sent by Holdable
    void UpdateHeldState(float heldState) {
        SetOrbIntensity(defaultIntensity - (defaultIntensity - heldIntensity) * heldState);
        if (heldState == 1) {
            childParticleSystem.enableEmission = true;
        } else if (childParticleSystem.isPlaying && childParticleSystem.enableEmission) {
            childParticleSystem.enableEmission = false;
        }
    }

    // messages sent by ContainerTrigger
    void NotifyActivate() => IsActive = true;
    void NotifyDeactivate() => IsActive = false;

    private void UpdateOrbState() {
        bool updateSpawnState = (!state.isActive && spawnState > 0) || (state.isActive && spawnState < 1);
        bool updateCharge = unchargeTime != 0 && (IsHeld || state.currentChargeLevel < 1 || chargeTime < 0) && !isDead;

        if (updateSpawnState) {
            if (state.isActive) {
                spawnState += Time.deltaTime / spawnTime;
                if (spawnState >= 1) spawnState = 1;
            } else {
                spawnState -= Time.deltaTime / spawnTime;
                if (spawnState <= 0) {
                    spawnState = 0;
                    if (isDead) {
                        Die();
                        return;
                    }
                }
            }
            transform.localScale = Vector3.one * spawnState;
            SetOrbIntensity(spawnState * defaultIntensity);
        }
        if (updateCharge) {
            if (IsHeld) {
                if (state.currentChargeLevel > 0f) {
                    state.currentChargeLevel -= Time.deltaTime / unchargeTime;
                }
            } else {
                if (state.currentChargeLevel < 1f || chargeTime < 0) {
                    state.currentChargeLevel += Time.deltaTime / chargeTime;
                    if (state.currentChargeLevel > 1f) state.currentChargeLevel = 1f;
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
        childParticleSystem.enableEmission = true;
        childParticleSystem.emissionRate *= explosionFactor;
        if (explosionSound != null) objectAudio.PlayOneShot(explosionSound);
        if (died != null) died();
    }

    private void Die() {
        if (spawnLocation == null) {
            GameObject.Destroy(gameObject);
        } else {
            if (IsHeld) holdable.Drop();
            transform.position = spawnLocation.transform.position;
            state.currentChargeLevel = 1.0f;
            SetOrbColor(GetColorFromCharge());
            SetOrbIntensity(0);
            childParticleSystem.emissionRate /= explosionFactor;
            IsActive = true;
        }
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
