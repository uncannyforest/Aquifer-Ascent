using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour {
    public float delay = 2; // should exceed undust duration
    public Collectible prefab;
    public Color finalColor = Color.green;

    private Collectible collectible;

    void Start() {
        FindObjectOfType<CollectibleHint>().Add(gameObject);
        FindObjectOfType<BeaconPanController>(true).Add(transform);
        this.Invoke(PlaceCollectible, delay);
    }

    public void Collect() {
        StandardOrb orb = transform.GetComponentInChildren<StandardOrb>();
        orb.colorTransitions[0].color = finalColor;
        orb.UpdateColor();
    }

    private void PlaceCollectible() {
        collectible = Instantiate(prefab, transform.position + CaveGrid.Scale.y * Vector3.down, Quaternion.identity);
        collectible.beacon = this;
    }
}
