using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 
/// Startup class for the project.
/// 
/// if webgl load the correct scene.
/// 
/// if standalone or development mode, load settings from the auth.txt file.
/// 
/// </summary>
class Startup
{

    /// <summary>
    /// level of the scenario, we might be able to remove this
    /// </summary>
    public static int scenarioLevel = 1;
#if UNITY_WEBGL && !UNITY_EDITOR
    public static string baseURL = ParseBaseURL(Application.absoluteURL);
    public static string location = baseURL + "repo/";
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
    public static string baseURL = "";
    public static string location = baseURL + "repo/";
    public static string editorUsername = "";
    public static string editorPassword = "";
    public static string authToken = "";
#endif

    /// <summary>
    /// used for non get requests
    /// </summary>
    public static string csrfToken = "";

    /// <summary>
    /// user name
    /// </summary>
    public static string userid = "";

    /// <summary>
    /// team name
    /// </summary>
    public static string teamname = "";

    /// <summary>
    /// flag is the planning tool is in business view
    /// </summary>
    public static bool isBusiness = false;

    /// <summary>
    /// log to server or log to local files
    /// </summary>
    public static bool logToServer = true;

    /// <summary>
    /// flag to include AI
    /// </summary>
    public static bool isAI = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {

#if UNITY_WEBGL && !UNITY_EDITOR
        Application.runInBackground = true;

        if(Application.absoluteURL.Contains("scene=design") || Application.absoluteURL.Contains("design"))
        {
            SceneManager.LoadScene("drone_designer");
        }
        else if (Application.absoluteURL.EndsWith("scene=ops") || Application.absoluteURL.Contains("ops"))
        {
            SceneManager.LoadScene("plan_tool");
        }
        else if (Application.absoluteURL.EndsWith("scene=business") || Application.absoluteURL.Contains("business"))
        {
            isBusiness = true;
            SceneManager.LoadScene("plan_tool");
        }
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
        string[] authInfo = System.IO.File.ReadAllLines(@"auth.txt");
        if(authInfo.Length >= 6)
        {

            Startup.baseURL = ParseBaseURL(authInfo[0]);
            Startup.location = baseURL + "repo/";

            Startup.editorUsername = authInfo[1];
            Startup.userid = authInfo[1];

            Startup.editorPassword = authInfo[2];

            Startup.teamname = authInfo[3];

            var checkbusiness = authInfo[4];
            if (checkbusiness == "true")
            {
                Startup.isBusiness = true;
            }
            else
            {
                Startup.isBusiness = false;
            }

            var checklog = authInfo[5];
            if (checklog == "true")
            {
                Startup.logToServer = true;
            }
            else
            {
                Startup.logToServer = false;
            }

            var checkAI = authInfo[6];
            if (checkAI == "true")
            {
                Startup.isAI = true;
            }
            else
            {
                Startup.isAI = false;
            }

        } else
        {
            Debug.Log("Unexpected authInfo from auth.txt");
        }
#endif
    }

    /// <summary>
    /// gets the base url from the provided url string
    /// </summary>
    /// <param name="URL"></param>
    /// <returns></returns>
    private static string ParseBaseURL(string URL)
    {
        string protocol = "";
        if(URL.StartsWith("http://"))
        {
            protocol = "http://";
        } else if(URL.StartsWith("https://"))
        {
            protocol = "https://";
        }
        string rest = URL.Substring(protocol.Length);

        string baseAddress = "";
        string[] parts = rest.Split('/');
        if(parts.Length > 0)
        {
            baseAddress = parts[0];
        }
        Debug.Log("Parsed URL = " + protocol + baseAddress + "/");
        return protocol + baseAddress + "/";
    }
}
