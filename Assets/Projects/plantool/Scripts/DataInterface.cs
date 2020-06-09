using UnityEngine;
using DataObjects;

namespace PlannerAssets
{

    /// <summary>
    /// A bridge to the central evaluation services. 
    /// </summary>
    class DataInterface : MonoBehaviour
    {

        /// <summary>
        /// 
        /// calls the web service to get the current market 
        /// 
        /// </summary>
        public static void GetMarket()
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetMarket();
        }

        /// <summary>
        /// 
        /// calls the web service to get all team session vehicles 
        /// 
        /// </summary>
        public static void GetVehicles()
        {
             GameObject.Find("restapi").GetComponent<RestWebService>().GetVehicles();
        }

        /// <summary>
        /// 
        /// post a scenario to the team session
        /// 
        /// </summary>
        /// <param name="scenario">scenario object</param>
        public static void PostScenario(Scenario scenario)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().PostScenario(scenario);
        }

        /// <summary>
        /// 
        /// get the current team scenario of the session
        /// 
        /// </summary>
        public static void GetScenario()
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetScenario();
        }

        /// <summary>
        /// 
        /// posts a plan to the team session
        /// 
        /// </summary>
        /// <param name="plan">Plan object</param>
        public static void PostPlan(Plan plan)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().PostPlan(plan);
        }

        /// <summary>
        /// 
        /// gets all plan ids and names of the team session
        /// 
        /// </summary>
        public static void GetPlanIds()
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetShortPlans();
        }

        /// <summary>
        /// 
        /// gets the full plan object
        /// 
        /// </summary>
        /// <param name="id">id of the plan object</param>
        public static void GetPlan(int id)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetPlan(id);
        }

        /// <summary>
        /// 
        /// run the plan AI analysis 
        /// 
        /// </summary>
        /// <param name="input">input string that includes customers and available vehicles</param>
        public static void runPlanAIAnalysis(string input)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().PostPlanToAIPlanner(input);
        }

    }

}
