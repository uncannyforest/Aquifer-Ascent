using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileToggle : MonoBehaviour
{
    public bool showIffMobile;
    // Start is called before the first frame update
    void Start()
    {
#if (UNITY_IOS || UNITY_ANDROID)
        if (!showIffMobile) {
            gameObject.SetActive(false);
        }
#else
        if (showIffMobile) {
            gameObject.SetActive(false);
        }
#endif
    }
}
