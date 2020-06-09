using System;
using System.Collections.Generic;
using DataObjects;

/// <summary>
/// Helper class that stores plans, metrics, and bounds 
/// </summary>
public class DashboardData
{

    /// <summary>
    /// stores plan with its metrics
    /// </summary>
    public static Dictionary<Plan, PlanData> dashboardData = new Dictionary<Plan, PlanData>();

    /// <summary>
    /// minimum plan profit in the billboard view 
    /// </summary>
    public static float minProfit = 0;

    /// <summary>
    /// maximum plan profit in the billboard view 
    /// </summary>
    public static float maxProfit = -10000000;

    /// <summary>
    /// minimum operating cost in the billboard view 
    /// </summary>
    public static float minOperatingCost = 0;

    /// <summary>
    /// maximum operating cost in the billboard view 
    /// </summary>
    public static float maxOperatingCost = -10000000;

    /// <summary>
    /// minimum startup cost in the billboard view 
    /// </summary>
    public static float minstartupCost = 0;

    /// <summary>
    /// maximum startup cost in the billboard view 
    /// </summary>
    public static float maxstartupCost = -10000000;

    /// <summary>
    /// minimum number of deliveries in the billboard view 
    /// </summary>
    public static float minDeliveries = 0;

    /// <summary>
    /// maximum number of deliveries in the billboard view 
    /// </summary>
    public static float maxDeliveries = -10000000;

    /// <summary>
    /// minimum mass delivered in the billboard view 
    /// </summary>
    public static float minMassDelivered = 0;

    /// <summary>
    /// maximum mass delivered in the billboard view 
    /// </summary>
    public static float maxMassDelivered = -10000000;

    /// <summary>
    /// minimum parcel mass delivered in the billboard view 
    /// </summary>
    public static float minParcelMass = 0;

    /// <summary>
    /// maximum parcel mass delivered in the billboard view 
    /// </summary>
    public static float maxParcelMass = -10000000;

    /// <summary>
    /// minimum food mass delivered in the billboard view 
    /// </summary>
    public static float minFoodMass = 0;

    /// <summary>
    /// maximum parcel mass delivered in the billboard view 
    /// </summary>
    public static float maxFoodMass = -10000000;

    /// <summary>
    /// 
    /// add a plan to the billboard view
    /// 
    /// </summary>
    /// <param name="plan">Plan object</param>
    /// <param name="data">PlanData metrics</param>
    public static void addPlanData(DataObjects.Plan plan, PlanData data)
    {

        // add the plan to a dictionary
        dashboardData[plan] = data;

        // sets the maximum bounds for each variable
        maxProfit = Math.Max(maxProfit, data.profit);
        maxOperatingCost = Math.Max(maxOperatingCost, data.operatingCost);
        maxstartupCost = Math.Max(maxstartupCost, data.startupCost);
        maxDeliveries = Math.Max(maxDeliveries, data.deliveries);
        maxMassDelivered = Math.Max(maxMassDelivered, data.massDelivered);
        maxParcelMass = Math.Max(maxParcelMass, data.parcelMass);
        maxFoodMass = Math.Max(maxFoodMass, data.foodMass);

    } 

    /// <summary>
    /// Stores plan metric data
    /// </summary>
    public class PlanData
    {
        /// <summary>
        /// plan tag
        /// </summary>
        public string tag = "";

        /// <summary>
        /// plan profit 
        /// </summary>
        public float profit = 0;

        /// <summary>
        /// plan operating cost
        /// </summary>
        public float operatingCost = 0;

        /// <summary>
        /// startup cost
        /// </summary>
        public float startupCost = 0;

        /// <summary>
        /// number of deliveries of the plan
        /// </summary>
        public float deliveries = 0;

        /// <summary>
        /// total mass delivered to the customers
        /// </summary>
        public float massDelivered = 0;

        /// <summary>
        /// total parcel mass delivered to the customers
        /// </summary>
        public float parcelMass = 0;

        /// <summary>
        /// total food mass delivered to the customers
        /// </summary>
        public float foodMass = 0;

    }

}

