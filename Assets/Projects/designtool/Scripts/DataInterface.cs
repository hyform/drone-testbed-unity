using UnityEngine;
using DataObjects;

namespace DesignerAssets
{
    /// <summary>
    /// a bridge to the datastore and evaluation services 
    /// </summary>
    class DataInterface
    {

        /// <summary>
        /// 
        /// gets the current market 
        /// 
        /// </summary>
        public static void GetMarket()
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetMarket();
        }

        /// <summary>
        /// 
        /// gets all team vehicles
        /// 
        /// </summary>
        public static void GetVehicles()
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetVehicles();
        }

        /// <summary>
        /// 
        /// evaluates a vehicle
        /// 
        /// </summary>
        /// <param name="vehicle">vehicle to evaluate</param>
        public static void EvaluateVehicle(Vehicle vehicle)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().PostEvaluation(vehicle);
        }

        /// <summary>
        /// 
        /// submits a vehicle
        /// 
        /// </summary>
        /// <param name="vehicle">vehicle to submit</param>
        public static void SubmitVehicle(Vehicle vehicle)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().PostVehicle(vehicle);
        }

        /// <summary>
        /// 
        /// gets suggested AI designer vehicles
        /// 
        /// </summary>
        /// <param name="range">range of the current vehicle in miles</param>
        /// <param name="cost">cost of the current vehicle in $</param>
        /// <param name="capacity">capacity of the current vehicle in lb</param>
        public static void GetAIVehicles(float range, float cost, float capacity)
        {
            GameObject.Find("restapi").GetComponent<RestWebService>().GetAIVehicles(range, cost, capacity);
        }


    }

}
