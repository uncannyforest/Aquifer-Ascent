using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandardOrb : MonoBehaviour
{
    public GameObject spawnLocation;
    public float unchargeTime = 30f;
    public float chargeTime = 450f;
    public float spawnTime = 1f;
    public float haloIntensity;
    public float currentChargeLevel = 1.0f;
    public float spawnState = 1.0f;

    private Light myLight;
    private Light halo;

    // Start is called before the first frame update
    void Start() {
        myLight = gameObject.transform.Find("Point Light").GetComponent<Light>();
        halo = gameObject.transform.Find("Halo").GetComponent<Light>();
        updateOrbState();
    }

    // Update is called once per frame
    void Update() {
        updateOrbState();
    }

    void updateOrbState() {
        if (spawnState < 1) {
            if (spawnState >= 0) {
                spawnState += Time.deltaTime / spawnTime;
                spawnState = Mathf.Min(1, spawnState);
                transform.localScale = Vector3.one * spawnState;
                myLight.intensity = spawnState;
                halo.intensity = spawnState * haloIntensity;
            } else {
                spawnState += Time.deltaTime / spawnTime;
                if (spawnState < 0) {
                    transform.localScale = Vector3.one * -spawnState;
                } else {
                    if (spawnLocation == null) {
                        GameObject.Destroy(gameObject);
                    } else {
                        if(gameObject.GetComponent<PickMeUp>().PickedUp) {
                            gameObject.GetComponent<PickMeUp>().SetDown();
                        }
                        transform.position = spawnLocation.transform.position;
                        currentChargeLevel = 1.0f;
                        setOrbColor(getColorFromCharge());
                        myLight.intensity = 0;
                        halo.intensity = 0;
                    }
                }
            }
        } else {
            if(gameObject.GetComponent<PickMeUp>().PickedUp) {
                if (currentChargeLevel > 0f) {
                    currentChargeLevel -= Time.deltaTime / unchargeTime;
                    if (currentChargeLevel < 0f) {
                        currentChargeLevel = 0f;
                        spawnState = -1;
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
    }

    void setOrbColor(Color color) {
        halo.color = color;
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
