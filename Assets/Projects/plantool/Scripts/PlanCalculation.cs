using DataObjects;
using UnityEngine;

namespace PlanToolHelpers
{

    /// <summary>
    /// Calculate the overall metrics of a plan by adding all delivery paths
    /// </summary>
    public class PlanCalculation
    {

        /// <summary>
        /// Plan object to calculate metrics against
        /// </summary>
        private Plan plan;

        /// <summary>
        /// scale of the scene
        /// </summary>
        private float scaleofScene;

        /// <summary>
        /// flag for the business view to modify the output string
        /// </summary>
        private bool business;

        /// <summary>
        /// total operating cost
        /// </summary>
        private float operatingCost;

        /// <summary>
        /// total profit
        /// </summary>
        private float profit;

        /// <summary>
        /// total startup cost
        /// </summary>
        private int startupCost;

        /// <summary>
        /// total number of customers
        /// </summary>
        private int customers = 0;

        /// <summary>
        /// total weight delivered
        /// </summary>
        private float totalWeightDelivered = 0;

        /// <summary>
        /// total vehicle travel time in hours
        /// </summary>
        private float totalVehicleRange = 0;

        /// <summary>
        /// total parcel delivered to all customers
        /// </summary>
        private int totalParcelDelivered = 0;

        /// <summary>
        /// total food delivered to all customers
        /// </summary>
        private int totalFoodDelivered = 0;

        /// <summary>
        /// information string displayed in the Unity interface
        /// </summary>
        private string infoString = "";

        /// <summary>
        /// log string displayed in the data logs
        /// </summary>
        private string logString = "";

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="plan">Plan object</param>
        /// <param name="scaleofScene">scale of the scene</param>
        /// <param name="business">flag to identify if in business view</param>
        public PlanCalculation(Plan plan,
            float scaleofScene,
            bool business)
        {
            this.plan = plan;
            this.scaleofScene = scaleofScene;
            this.business = business;
        }

        /// <summary>
        /// calculates the metrics of a Plan
        /// </summary>
        public void calculate()
        {

            // for all paths, the warehouse position is the same
            Vector3 p = GameObject.Find("base").transform.position;

            // set fixed cost
            startupCost = 0;

            // for all paths
            for (int i = 0; i < plan.paths.Count; i++)
            {

                // get the path calculation
                PlanPathCalculation pathCalculation = new PlanPathCalculation(plan.paths[i]);
                pathCalculation.calculate();
                totalVehicleRange += pathCalculation.getTotalRange();

                // get the total food and parcel delivered
                totalWeightDelivered += pathCalculation.getTotalCapacity();
                totalFoodDelivered += pathCalculation.getTotalFoodDelivered();
                totalParcelDelivered += pathCalculation.getTotalParcelDelivered();

                // update customer count and fixed cost 
                customers += plan.paths[i].customers.Count;
                startupCost += (int) (plan.paths[i].vehicle.cost);

            }
            
            // calculate operational cost, profit, and fixed cost
            // 100 is just a constant set to represent a cost per mile  
            operatingCost = (float)(System.Math.Round((double)totalVehicleRange * 100));

            // 200 and 100 are based on the problem statement
            profit = 200 * totalFoodDelivered + 100 * totalParcelDelivered;

            // set the plan name in the business view
            string planname = "";
            if (business)
                planname = "Name : " + plan.tag;

            // information string to show on the unity interface
            infoString = planname + "\nProfit ($) : " + profit
                + "\nOperating Cost ($) : " + operatingCost
                + "\nStartUp Cost ($) : " + (int) startupCost
                + "\n\nNumber of Deliveries : " + customers
                + "\n\nWeight Delivered (lb) : " + totalWeightDelivered
                + "\n - Parcel (lb) : " + (float)(System.Math.Round((double)totalParcelDelivered, 2))
                + "\n - Food   (lb) : " + (float)(System.Math.Round((double)totalFoodDelivered, 2));

            // log string for the data logs
            logString = "Profit," + profit
                + ",OperatingCost," + operatingCost
                + ",StartUpCost," + startupCost
                + ",Number of Deliveries," + customers
                + ",WeightDelivered," + totalWeightDelivered
                + ",Parcel," + (float)(System.Math.Round((double)totalParcelDelivered, 2))
                + ",Food," + (float)(System.Math.Round((double)totalFoodDelivered, 2));

        }

        /// <summary>
        /// gets the total number of delivery customers
        /// </summary>
        /// <returns>number of customers</returns>
        public int getCustomers()
        {
            return customers;
        }

        /// <summary>
        /// gets the total weight delivered in lb
        /// </summary>
        /// <returns></returns>
        public float GetTotalWeightDelivered()
        {
            return totalWeightDelivered;
        }

        /// <summary>
        /// gets the total parcel delivered in lb
        /// </summary>
        /// <returns></returns>
        public int getTotalParcelDelivered()
        {
            return totalParcelDelivered;
        }

        /// <summary>
        /// gets the total food delivered in lb
        /// </summary>
        /// <returns></returns>
        public int getTotalFoodDelivered()
        {
            return totalFoodDelivered;
        }

        /// <summary>
        /// gets the operating cost
        /// </summary>
        /// <returns></returns>
        public float getOperatingCost()
        {
            return operatingCost;
        }

        /// <summary>
        /// gets the profit of the plan
        /// </summary>
        /// <returns></returns>
        public float getProfit()
        {
            return profit;
        }

        /// <summary>
        /// gets the startup cost
        /// </summary>
        /// <returns></returns>
        public float getStartupCost()
        {
            return startupCost;
        }

        /// <summary>
        /// gets the information string to display in the interface
        /// </summary>
        /// <returns></returns>
        public string getInfoString()
        {
            return infoString;
        }

        /// <summary>
        /// gets the string to use in the data logs
        /// </summary>
        /// <returns></returns>
        public string getLogString()
        {
            return logString;
        }


    }
}