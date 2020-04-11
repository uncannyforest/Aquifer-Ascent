using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyShowOnMobile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if !(UNITY_IOS || UNITY_ANDROID)
        gameObject.SetActive(false);
#endif
    }
}
