using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

public class StartupMono : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Startup.csrfToken = SessionLib.GetCSRFToken();
        Startup.userid = SessionLib.GetUserName();
        String isAiStr = SessionLib.GetIsAI();
        if(isAiStr.Equals("True"))
        {
            Startup.isAI = true;
        } else
        {
            Startup.isAI = false;
        }        

        string[] useridParts = Startup.userid.Split('_');
        if (useridParts.Length > 0)
        {
            Startup.teamname = useridParts[0];
        }

        Debug.Log("Username = " + Startup.userid + " , Teamname = " + Startup.teamname);
        AttachScripts();
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
        StartCoroutine(GetAuthToken());
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public static void AttachScripts()
    {

        //We have auth token if it's available, so attach key scripts to scenes so that
        //stuff can start loading
        GameObject designBase = GameObject.Find("GUI");
        if (designBase != null)
        {
            designBase.AddComponent<DesignerAssets.UAVDesigner>();
        }

        //GameObject planBase = GameObject.Find("groundcube");
        //if (planBase != null)
        //{
        //    planBase.AddComponent<PlanningTool>();
        //}

        GameObject planBase = GameObject.Find("groundcube");
        if (planBase != null)
        {
            if(Startup.isBusiness)
                planBase.AddComponent<BusinessPlanInterface>();
            else
                planBase.AddComponent<OpsPlanInterface>();
        }


    }

    private class TokenResults
    {
        public string token { get; set; }
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private static IEnumerator GetAuthToken()
    {



        WWWForm form = new WWWForm();
        form.AddField("username", Startup.editorUsername);
        form.AddField("password", Startup.editorPassword);

        Debug.Log(Startup.baseURL + "api-token-auth/");

        UnityWebRequest request = UnityWebRequest.Post(Startup.baseURL + "api-token-auth/", form);
        request.downloadHandler = new DownloadHandlerBuffer();

        


        yield return request.SendWebRequest();
        while (!request.downloadHandler.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        if (request.isNetworkError)
        {
            Debug.Log("StartupMono::GetAuthToken Error: " + request.error);
        }
        else
        {

            string resultText = request.downloadHandler.text;

            if (!string.IsNullOrEmpty(resultText))
            {
                TokenResults tokenData = JsonConvert.DeserializeObject<TokenResults>(resultText);
                if (tokenData != null)
                {
                    Startup.authToken = tokenData.token;
                    if (string.IsNullOrEmpty(Startup.authToken))
                    {
                        Debug.Log("Error: auth token request failed");
                    }
                    else
                    {
                        Debug.Log("auth token request set = " + Startup.authToken);
                    }
                } else
                {
                    Debug.Log("Error: bad auth token = " + resultText);
                }
            } else
            {
                Debug.Log("Error: auth token request is empty");
            }

            StartupMono.AttachScripts();
            //DesignerAssets.DataInterface.GetDesignTeam();
            //AttachScripts();
        }
    }
#endif    
}
