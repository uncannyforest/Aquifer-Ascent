using UnityEngine;

[RequireComponent(typeof(Scarf))]
public class ScarfKey : MonoBehaviour {
    private Scarf scarf;
    void Start() {
        scarf = GetComponent<Scarf>();
    }

    void Update() {
        if (Input.GetKeyDown(",")) {
            scarf.SwapScarf(false, out int _, out int __);
        } else if (Input.GetKeyDown(".")) {
            scarf.SwapScarf(true, out int _, out int __);
        }
    }
}
