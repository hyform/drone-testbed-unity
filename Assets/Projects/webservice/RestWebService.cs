using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

namespace DataObjects
{

    /// <summary>
    /// Used to send and receive data to the central web framework
    /// </summary>
    class RestWebService : MonoBehaviour
    {
 
        /// <summary>
        /// queue for scenarios
        /// </summary>
        public static List<Scenario> scenarioqueue = new List<Scenario>();

        /// <summary>
        /// queue for plans
        /// </summary>
        public static List<Plan> planqueue = new List<Plan>();

        /// <summary>
        /// queue for short plans
        /// </summary>
        public static List<PlanShort> planshortqueue = new List<PlanShort>();

        /// <summary>
        /// queue for vehicles
        /// </summary>
        public static List<Vehicle> vehiclequeue = new List<Vehicle>();

        /// <summary>
        /// queue for server results
        /// </summary>
        public static string resultstr = null;

        /// <summary>
        /// queue for AI Designer results
        /// </summary>
        public static Dictionary<string, double[]> aiDesignerQueue = new Dictionary<string, double[]>();

        /// <summary>
        /// queue for AI Planner results
        /// </summary>
        public static string aiPlannerResultQueue = null;
        public static Plan aiPlannerResultQueuePlan = null;

        /// <summary>
        /// queue for AI Evaluation results
        /// </summary>
        public static EvaluationOutput uavEvaluation = null;

        /// <summary>
        /// stores the market id
        /// </summary>
        public static int market = 0;

        /// <summary>
        /// store results from the dronebot
        /// </summary>
        public static string dronebotqueue = null;

        void Start(){}
        
        void Update(){}

        /// <summary>
        /// gets the base url for the central web framework data storage endpoints
        /// </summary>
        /// <returns>url</returns>
        private string GetCentralDatastoreEndpoint()
        {
            return Startup.location;
        }

        /// <summary>
        /// gets the base url for the central web framework service endpoints
        /// </summary>
        /// <returns>url</returns>
        private string GetCentralServiceEndpoint()
        {
            return Startup.baseURL;
        }

        /// <summary>
        /// get the user name
        /// </summary>
        /// <returns></returns>
        private string GetUserName()
        {
            return Startup.userid;
        }

        /// <summary>
        /// gets the team name
        /// </summary>
        /// <returns></returns>
        private string GetTeamName()
        {
            return Startup.teamname;
        }

        /// <summary>
        /// gets the current market id
        /// </summary>
        public void GetMarket()
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralServiceEndpoint() + "exper/session/", "GET",
            request =>
            {
                request.redirectLimit = 0;
                request.SetRequestHeader("Content-Type", "application/json");
            },
            request =>
            {
                Debug.Log(request.downloadHandler.text);
                Session postData = JsonConvert.DeserializeObject<Session>(request.downloadHandler.text);
                market = postData.market;
            }));
        }

        /// <summary>
        /// gets the vehicles of the team and session
        /// </summary>
        public void GetVehicles()
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "vehicle/", "GET",
                request =>
                {
                    request.redirectLimit = 0;
                    request.SetRequestHeader("Content-Type", "application/json");
                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    VehiclesResultMessage postData = JsonConvert.DeserializeObject<VehiclesResultMessage>(request.downloadHandler.text);
                    foreach (Vehicle result in postData.results)
                        vehiclequeue.Add(result);                  
                }));
        }

        /// <summary>
        /// gets the current scenario of the team and session
        /// </summary>
        public void GetScenario()
        {

            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "scenario/", "GET",
                request =>
                {
                    request.redirectLimit = 0;
                    request.SetRequestHeader("Content-Type", "application/json");
                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    Scenario scenarioData = JsonConvert.DeserializeObject<Scenario>(request.downloadHandler.text);
                    scenarioqueue.Add(scenarioData);
                }));
        }

        /// <summary>
        /// gets short plan representations of information by name and id
        /// </summary>
        public void GetShortPlans()
        {

            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "planshort/", "GET",
                request =>
                {
                    request.redirectLimit = 0;
                    request.SetRequestHeader("Content-Type", "application/json");
                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    PlanShortResultMessage results = JsonConvert.DeserializeObject<PlanShortResultMessage>(request.downloadHandler.text);
                    foreach (PlanShort result in results.results)
                        planshortqueue.Add(result);                   
                }));

        }

        /// <summary>
        /// gets the full plan representation by id
        /// </summary>
        /// <param name="id">plan id</param>
        public void GetPlan(int id)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "plan/" + id + "/", "GET",
                request =>
                {
                    request.redirectLimit = 0;
                    request.SetRequestHeader("Content-Type", "application/json");
                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    Plan result = JsonConvert.DeserializeObject<Plan>(request.downloadHandler.text);
                    planqueue.Add(result);
                }));
        }

        /// <summary>
        /// gets the designer AI vehicles based on current evaluation metrics 
        /// </summary>
        /// <param name="range">range in miles</param>
        /// <param name="cost">cost</param>
        /// <param name="capacity">capacity in lb</param>
        public void GetAIVehicles(float range, float cost, float capacity)
        {

            StartCoroutine(SessionLib.FullWebRequest(GetCentralServiceEndpoint() + "ai/designer1/?range=" 
                + range + "&cost=" + cost + "&payload=" + capacity, "GET",
                request =>
                {
                    request.redirectLimit = 0;
                    request.SetRequestHeader("Content-Type", "application/json");
                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    VehiclesResultMessage postData = JsonConvert.DeserializeObject<VehiclesResultMessage>(request.downloadHandler.text);
                    aiDesignerQueue.Clear();
                    foreach (Vehicle result in postData.results)
                       aiDesignerQueue.Add(result.config, new double[] { result.range, result.cost, result.payload });                   
                }));

        }

        /// <summary>
        /// Get Dronebot results
        /// </summary>
        /// <param name="question">question to ask dronebot</param>
        public void GetDronebotVehicles(string question)
        {

            StartCoroutine(SessionLib.FullWebRequest(GetCentralServiceEndpoint() + "ai/dronebot/", "POST",
                request =>
                {

                    // create the log statement
                    DroneBotObject obj = new DroneBotObject();
                    obj.input = question;
                    obj.output = "";

                    string postData = JsonConvert.SerializeObject(obj);
                    Debug.Log(postData);
                    request.redirectLimit = 0;
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                    request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");

                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    DroneBotObject postData = JsonConvert.DeserializeObject<DroneBotObject>(request.downloadHandler.text);
                    dronebotqueue = postData.output;

                }));

        }


        /// <summary>
        /// posts a log message to the server
        /// </summary>
        /// <param name="logstr">log message to write to the central data logs</param>
        /// <param name="type">the type of log message</param>
        public void PostLog(string logstr, string type)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "datalog/", "POST",
                request =>
                {
                    // create the log statement
                    DataLog log = new DataLog();
                    log.team = GetTeamName();
                    log.user = GetUserName();
                    log.action = type + ";" + logstr;

                    string postData = JsonConvert.SerializeObject(log);
                    Debug.Log(postData);
                    request.redirectLimit = 0;
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");

                },
                request =>
                {
                    Debug.Log(request.downloadHandler.text);
                    Debug.Log("Status Code: " + request.responseCode + " " + " " + request.downloadHandler.text);
                }));

        }        

        /// <summary>
        /// posts a plan to the central server
        /// </summary>
        /// <param name="plan"></param>
        public void PostPlan(Plan plan)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "plan/", "Post",
               request =>
               {

                   // create a post plan object
                   PlanPost planPost = new PlanPost();
                   planPost.tag = plan.tag;
                   planPost.scenario = plan.scenario.id;
                   planPost.paths = new List<PlanVehicleDeliveryPost>();
                   foreach(VehicleDelivery p in plan.paths)
                   {

                       if (p.customers.Count > 0)
                       {
                           PlanVehicleDeliveryPost planpath = new PlanVehicleDeliveryPost();
                           planpath.vehicle = p.vehicle.id;
                           planpath.warehouse = p.warehouse.id;
                           planpath.customers = new List<int>();
                           planpath.leavetime = 0;
                           planpath.returntime = p.customers[p.customers.Count - 1].deliverytime;
                           foreach (CustomerDelivery customerpath in p.customers)                          
                               planpath.customers.Add(customerpath.id);
                           planPost.paths.Add(planpath);
                       }

                   }

                   string postData = JsonConvert.SerializeObject(planPost);
                   Debug.Log(postData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");
               },
               request =>
               {
                   Debug.Log(request.downloadHandler.text);
                   resultstr = " Submit Plan : " + getServerResponse(request.responseCode);
               }));
        }

        /// <summary>
        /// post scenario to the central server
        /// </summary>
        /// <param name="scenario">Scenario object</param>
        public void PostScenario(Scenario scenario)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "scenario/", "PUT",
               request =>
               {

                   ScenarioPost scenarioPost = new ScenarioPost();
                   scenarioPost.customers = new List<int>();
                   scenarioPost.tag = "updated";
                   if(scenario.tag != null)
                       scenarioPost.tag = scenario.tag + "_" + (scenario.version + 1);
                   foreach (Customer customer in scenario.customers)
                       if (customer.selected)
                           scenarioPost.customers.Add(customer.id);
                   
                   // post a scenario object
                   string postData = JsonConvert.SerializeObject(scenarioPost);
                   Debug.Log(postData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");

               },
               request =>
               {
                   resultstr = " Submit Scenario : " + getServerResponse(request.responseCode);
                   Debug.Log("Status Code: " + request.responseCode + " " + " " + request.downloadHandler.text);
               }));
        }


        /// <summary>
        /// post scenario to the central server
        /// </summary>
        /// <param name="scenario">Scenario object</param>
        public void PutVehicle(int vehicle_id)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "vehicle/" + vehicle_id + "/", "PUT",
               request =>
               {

                   Vehicle v = new Vehicle();
                   v.id = vehicle_id;


                   // post a scenario object
                   string vehiclePutData = JsonConvert.SerializeObject(v);
                   Debug.Log("put data " + vehiclePutData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(vehiclePutData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");

               },
               request =>
               {
                   resultstr = " Put Vehicle : " + getServerResponse(request.responseCode);
                   Debug.Log("Status Code: " + request.responseCode + " " + " " + request.downloadHandler.text);
               }));
        }


        /// <summary>
        /// post scenario to the central server
        /// </summary>
        /// <param name="scenario">Scenario object</param>
        public void PutPlan(int plan_id)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "plan/" + plan_id + "/", "PUT",
               request =>
               {

                   Debug.Log("put");
                   Plan p = new Plan();
                   p.id = plan_id;


                   // post a scenario object
                   string planPutData = JsonConvert.SerializeObject(p);
                   Debug.Log("planPutData " + planPutData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(planPutData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");

               },
               request =>
               {
                   try
                   {
                       Debug.Log(request.downloadHandler.text);
                       Debug.Log("Status Code: " + request.responseCode + " " + " " + request.downloadHandler.text);
                   } catch (Exception e)
                   {
                       Debug.Log(e);
                   }
               }));
        }

        /// <summary>
        /// posts a set of vehicles and customers the central service and it returns 
        /// an AI plan result
        /// </summary>
        /// <param name="plan">current PLan</param>
        public void PostPlanToAI(Plan plan)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralServiceEndpoint() + "ai/opsplan/", "POST",
                request =>
                {

                    // setup planner AI object
                    OpsPlanAIRun test = new OpsPlanAIRun();
                    test.input = JsonConvert.SerializeObject(plan);
                    test.output = "";

                    string postData = JsonConvert.SerializeObject(test);
                    Debug.Log("planner AI send data : " + postData);
                    request.redirectLimit = 0;
                    byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                    request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");

                },
                request =>
                {

                    resultstr = " Return AI Plan : " + getServerResponse(request.responseCode);
                    string convertResults = request.downloadHandler.text.Replace("True", "true");
                    convertResults = convertResults.Replace("False", "false");
                    OpsPlanAIRun aiResults = JsonConvert.DeserializeObject<OpsPlanAIRun>(convertResults);

                    // convert python True to true
                    string formatted = aiResults.output.Replace("True", "true");
                    aiPlannerResultQueuePlan = JsonConvert.DeserializeObject<Plan>(formatted);

                }));
        }

        /// <summary>
        /// posts a vehicle to the evaluation service
        /// </summary>
        /// <param name="vehicle"></param>
        public void PostEvaluation(Vehicle vehicle)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralServiceEndpoint() + "ai/uavdesign2traj/", "POST",
               request =>
               {

                   EvaluationInput input = new EvaluationInput();
                   input.config = vehicle.config;

                   string postData = JsonConvert.SerializeObject(input);
                   Debug.Log(postData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");

               },
               request =>
               {

                   Debug.Log(request.downloadHandler.text);
                   uavEvaluation = JsonConvert.DeserializeObject<EvaluationOutput>(request.downloadHandler.text);
                   resultstr = " Evaluation : " + getServerResponse(request.responseCode);

                   // currently the trajectory is offset by the below x and z values
                   float yoffset = 1035.0f;
                   float zoffset = 2000.0f;
                   Vector3 lastPosition = new Vector3(0, 0, 0);

                   foreach (Trajectory trajectory in uavEvaluation.trajectory)
                   {
                       trajectory.position[1] = trajectory.position[1] - yoffset;
                       trajectory.position[2] = trajectory.position[2] - zoffset;
                       Vector3 nextPosition = new Vector3(
                           (float)trajectory.position[0],
                           (float)trajectory.position[1],
                           (float)trajectory.position[2]);
                   }

               }));
        }

        /// <summary>
        /// posts a vehicle to store on the central data server
        /// </summary>
        /// <param name="vehicle"></param>
        public void PostVehicle(Vehicle vehicle)
        {
            StartCoroutine(SessionLib.FullWebRequest(GetCentralDatastoreEndpoint() + "vehicle/", "POST",
               request =>
               {
                   string postData = JsonConvert.SerializeObject(vehicle);
                   Debug.Log(postData);
                   request.redirectLimit = 0;
                   byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                   request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
                   request.SetRequestHeader("Content-Type", "application/json");
               },
               request =>
               {
                   resultstr = " Submit Vehicle : " + getServerResponse(request.responseCode);
                   Debug.Log("Status Code: " + request.responseCode + " " + " " + request.downloadHandler.text);
               }));
        }

        /// <summary>
        /// gets the server response
        /// </summary>
        /// <param name="responseCode">response code from the server</param>
        /// <returns>string representation of the response</returns>
        private static string getServerResponse(long responseCode)
        {
            if (responseCode == 200 || responseCode == 201 || responseCode == 202)
                return "Success";
            else
                return "Failure";
        }

        /// <summary>
        /// message with the vehicle query returned
        /// </summary>
        private class VehiclesResultMessage
        {
            public int count { get; set; }
            public object next { get; set; }
            public object previous { get; set; }
            public List<Vehicle> results { get; set; }

        }

        /// <summary>
        /// gets the resulting plan query with short results
        /// </summary>
        private class PlanShortResultMessage
        {
            public int count { get; set; }
            public object next { get; set; }
            public object previous { get; set; }
            public List<PlanShort> results { get; set; }

        }

        /// <summary>
        /// evaluation input object for the designer view
        /// </summary>
        private class EvaluationInput
        {
            /// <summary>
            /// configuration of the vehicle
            /// </summary>
            public string config { get; set; }

        }

        /// <summary>
        /// results from the evaluation output
        /// </summary>
        public class EvaluationOutput
        {

            /// <summary>
            /// string configuration of the vehicle
            /// </summary>
            public string config { get; set; }

            /// <summary>
            /// result of analysis
            /// </summary>
            public string result { get; set; }

            /// <summary>
            /// range in miles
            /// </summary>
            public double range { get; set; }

            /// <summary>
            /// velocity in mph
            /// </summary>
            public double velocity { get; set; }

            /// <summary>
            /// cost
            /// </summary>
            public double cost { get; set; }

            /// <summary>
            /// trajectory results of the vehicle
            /// </summary>
            public List<Trajectory> trajectory { get; set; }

        }

        /// <summary>
        /// trajectory object returned in the evaluation output result object
        /// </summary>
        public class Trajectory
        {
            /// <summary>
            /// time of the trajectory segment
            /// </summary>
            public double time { get; set; }

            /// <summary>
            /// position of the trajectory segment
            /// </summary>
            public List<double> position { get; set; }

            /// <summary>
            /// quaternion of the trajectory segment
            /// </summary>
            public List<double> orientation { get; set; }

        }

        /// <summary>
        /// operations plans AI input object
        /// </summary>
        private class DroneBotObject
        {
            /// <summary>
            /// xml string of the planner AI input with vehicles and customers
            /// </summary>
            public string input { get; set; }

            public string output { get; set; }

            public bool success { get; set; }

        }

        /// <summary>
        /// operations plans AI input object
        /// </summary>
        private class OpsPlanAIRun
        {
            /// <summary>
            /// xml string of the planner AI input with vehicles and customers
            /// </summary>
            public string input { get; set; }

            public string output { get; set; }

        }

        /// <summary>
        /// operations plan AI output object
        /// </summary>
        private class OpsPlanAIXMLOutput
        {
            public string input { get; set; }

            /// <summary>
            /// xml string of the planner AI output with vehicle paths
            /// </summary>
            public string output { get; set; }

        }

        /// <summary>
        /// 
        /// ScenarioPost is used to submit a scenario to the central server
        /// 
        /// </summary>
        private class ScenarioPost
        {

            public string tag { get; set; }

            /// <summary>
            /// customers included in the scenario by id
            /// </summary>
            public List<int> customers { get; set; }

        }

        /// <summary>
        /// 
        /// posts a plan to the central server
        /// 
        /// </summary>
        private class PlanPost
        {

            /// <summary>
            /// name of the plan
            /// </summary>
            public string tag { get; set; }

            /// <summary>
            /// scenario id for the plan
            /// </summary>
            public int scenario { get; set; }

            /// <summary>
            /// paths of the submitted plan
            /// </summary>
            public List<PlanVehicleDeliveryPost> paths { get; set; }

        }


        /// <summary>
        /// 
        /// used to save a vehicle path to the central server
        /// 
        /// </summary>
        private class PlanVehicleDeliveryPost
        {

            /// <summary>
            /// id of the vehicle
            /// </summary>
            public int vehicle { get; set; }

            /// <summary>
            /// warehouse id of the path
            /// </summary>
            public int warehouse { get; set; }

            /// <summary>
            /// leave time in hours
            /// </summary>
            public float leavetime { get; set; }

            /// <summary>
            /// return time in hours
            /// </summary>
            public float returntime { get; set; }

            /// <summary>
            /// customers included in the path by id
            /// </summary>
            public List<int> customers { get; set; }

        }

    }

    /// <summary>
    /// 
    /// short plan representations with id and tag
    /// 
    /// </summary>
    public class PlanShort
    {

        /// <summary>
        /// plan integer id
        /// </summary>
        public int id;

        /// <summary>
        /// plan tag 
        /// </summary>
        public string tag;


    }


    public class VehiclePut
    {

        /// <summary>
        /// plan integer id
        /// </summary>
        public int id;

    }

}
