using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Score : MonoBehaviour {
    private Transform player;
    private Text display;
    private float maxHorizDistance = 0;
    private float maxY = 0;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        display = GetComponent<Text>();
    }

    void Update() {
        if (Time.timeSinceLevelLoad < .5f) return;
        maxY = Mathf.Max(maxY, player.position.y);
        Vector3 playerHoriz = player.position;
        playerHoriz.y = 0;
        maxHorizDistance = Mathf.Max(maxHorizDistance, playerHoriz.magnitude);
        float maxDistance = maxHorizDistance + maxY;
        display.text = maxDistance.ToString("F1");
    }
}
