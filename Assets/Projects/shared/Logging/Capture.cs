using UnityEngine;

/// <summary>
/// Used to send log statement to the local files and server
/// </summary>
public class Capture {

    private static System.DateTime start = System.DateTime.Now;

    public static string BUSINESS = "business";
    public static string PLANNER = "planner";
    public static string DESIGNER = "designer";

    /// <summary>
    /// start the log time
    /// </summary>
    public static void startLogTime()
    {
        start = System.DateTime.Now;
    }

    /// <summary>
    /// logs an actions
    /// </summary>
    /// <param name="str">string to be logged</param>
    /// <param name="type">message type</param>
    public static void Log(string str, string type)
    {
        
        if (Startup.logToServer)
        {
            GameObject.Find("restapi").GetComponent<DataObjects.RestWebService>().PostLog(str, type);            
        }
        else
        {
            System.IO.File.AppendAllText("./" + type + ".log", (System.DateTime.Now).ToString() + ";" + str + "\r\n");
        }
    }

}