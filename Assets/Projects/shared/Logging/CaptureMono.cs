using UnityEngine;

//A regular class cannot create a Coroutine, so this is here
//just as an instance of MonoBehavior to use for creating
//a Coroutine, which is needed to reliably send logs back to the web server
public class CaptureMono : MonoBehaviour {
    public static CaptureMono instance;

    void Start () {
#if UNITY_WEBGL && !UNITY_EDITOR       
        //This is used for rest requests to web server, so
        //only enable in the web version
        //Need to check for !UNITY_EDITOR because if in editor and
        //the active build is set to WebGL, the UNITY_WEBGL will be true
        CaptureMono.instance = this;
        Debug.Log("Unity WebGL");
#else
        CaptureMono.instance = null;
        Debug.Log("Not Unity WebGL");
#endif
    }
}
