using System.Collections;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;

/// <summary>
/// Handles web request calls to the central web framework
/// </summary>
public class SessionLib : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern string GetCSRFToken();

    [DllImport("__Internal")]
    public static extern string GetUserName();

    [DllImport("__Internal")]
    public static extern string GetIsAI();

    /// <summary>
    /// Gets a Unity web request
    /// </summary>
    /// <param name="url">the url for the service endpoint</param>
    /// <param name="method">GET, PUT, or POST</param>
    /// <returns></returns>
    public static UnityWebRequest GetWebRequest(string url, string method)
    {
        var request = new UnityWebRequest(url, method);
#if UNITY_WEBGL && !UNITY_EDITOR
        if (method != "GET")
        {
            request.SetRequestHeader("X-CSRFToken", Startup.csrfToken);
        }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
        if(string.IsNullOrEmpty(Startup.authToken))
        {
            Debug.Log("Error: auth token not set");
        }
        request.SetRequestHeader("Authorization", "Token " + Startup.authToken);
#endif
        return request;
    }

    public delegate void Del(UnityWebRequest request);

    /// <summary>
    /// 
    /// Full web request
    /// 
    /// </summary>
    /// <param name="url">the url for the service endpoint</param>
    /// <param name="method">GET, PUT, or POST</param>
    /// <param name="setup">setup method</param>
    /// <param name="onSuccess">success method</param>
    /// <returns></returns>
    public static IEnumerator FullWebRequest(string url, string method, Del setup, Del onSuccess)
    {
        var request = SessionLib.GetWebRequest(url, method);
        request.downloadHandler = new DownloadHandlerBuffer();

        setup(request);

        yield return request.SendWebRequest();

        if (request.isHttpError) {
            Debug.Log("HttpError: " + request.error);
        }
        else if (request.isNetworkError)
        {
            Debug.Log("NetworkError: " + request.error);
        }
        else if(request.responseCode < 400 || request.responseCode >= 100)
        {
            onSuccess(request);
        }
        else
        {
            Debug.Log("Error: response code = " + request.responseCode);
        }
    }

    // Start is called before the first frame update
    void Start(){}

    // Update is called once per frame
    void Update(){}

}
