using System;
using System.Collections.Generic;
using DataObjects;

namespace PlanToolHelpers
{

    /// <summary>
    /// Calculates the range, capacity, and total time of a VehiclePath
    /// </summary>
    class PlanPathCalculation
    {

        /// <summary>
        /// vehicle delivery object with a list of customers 
        /// </summary>
        private VehicleDelivery path;

        /// <summary>
        /// variable to make sure food gets delivered in the food time windows
        /// </summary>
        private bool foodCheck = true;

        /// <summary>
        /// total range or distance of a delivery path
        /// </summary>
        private float totalRange = 0;

        /// <summary>
        /// total capacity delivered
        /// </summary>
        private int totalCapacity = 0;

        /// <summary>
        /// end time of the vehicle delivery path
        /// </summary>
        private float endTime = 0;

        /// <summary>
        /// total parcel weight delivered
        /// </summary>
        private int totalParcelDelivered = 0;

        /// <summary>
        /// total food weight delivered
        /// </summary>
        private int totalFoodDelivered = 0;

        /// <summary>
        /// main constructor
        /// </summary>
        /// <param name="path">VehicleDelivery object</param>
        public PlanPathCalculation(VehicleDelivery path)
        {
            this.path = path;
        }

        /// <summary>
        /// 
        /// distance calculation
        /// 
        /// </summary>
        /// <param name="x1">x location 1</param>
        /// <param name="x2">x location 2</param>
        /// <param name="z1">z location 1</param>
        /// <param name="z2">z location 2</param>
        /// <returns></returns>
        private float distance(float x1, float x2, float z1, float z2)
        {
            return (float) Math.Sqrt(System.Math.Pow(x1 - x2, 2) + Math.Pow(z1 - z2, 2));
        }

        /// <summary>
        /// calculates the metrics of a path
        /// </summary>
        public void calculate()
        {

            // get customer deliveries
            List<CustomerDelivery> pathPoints = path.customers;

            // if the path is not empty
            if (pathPoints.Count > 0)
            {

                // get the first segment metrics
                totalRange = distance(pathPoints[0].address.x,
                    path.warehouse.address.x,
                    pathPoints[0].address.z,
                    path.warehouse.address.z);
                endTime = (float) (totalRange / path.vehicle.velocity);
                totalCapacity = (int) pathPoints[0].weight;
                foodDeliveryCheck(pathPoints[0]);
                pathPoints[0].deliverytime = (float) endTime;


                // go through all middle segments of the path
                for (int i = 1; i < pathPoints.Count; i++)
                {
                    float dist = distance(pathPoints[i].address.x,
                        pathPoints[i - 1].address.x,
                        pathPoints[i].address.z,
                        pathPoints[i - 1].address.z);
                    totalRange += dist;
                    totalCapacity += (int) pathPoints[i].weight;
                    endTime += (float) (dist / path.vehicle.velocity);
                    foodDeliveryCheck(pathPoints[i]);
                    pathPoints[i].deliverytime = (float)endTime;
                }

                // add time and distance for return segment to the warehouse 
                float returndist = distance(pathPoints[pathPoints.Count - 1].address.x,
                    path.warehouse.address.x,
                    pathPoints[pathPoints.Count - 1].address.z,
                    path.warehouse.address.z);
                totalRange += returndist;
                endTime += (float) (returndist / path.vehicle.velocity);
                pathPoints[pathPoints.Count - 1].deliverytime = (float)endTime;

            }

        }

        /// <summary>
        /// adjusts delivery time and food window check
        /// </summary>
        /// <param name="customer"></param>
        public void foodDeliveryCheck(Customer customer) {
            if (customer.payload.Equals("food2"))
            {
                endTime = Math.Max(endTime, 4);
                if (endTime > 6)
                    foodCheck = false;
                totalFoodDelivered += (int) customer.weight;
            } else
            {
                totalParcelDelivered += (int) customer.weight;
            }
        }

        /// <summary>
        /// flag if food is delivered in time
        /// </summary>
        /// <returns></returns>
        public bool deliveredFoodinTime()
        {
            return foodCheck;
        }

        /// <summary>
        /// gets the total distance or range in miles
        /// </summary>
        /// <returns></returns>
        public float getTotalRange()
        {
            return totalRange;
        }

        /// <summary>
        /// gets the vehicle total range remaining in miles
        /// </summary>
        /// <returns></returns>
        public float getTotalRangeRemaining()
        {
            return (float) (path.vehicle.range - totalRange);
        }

        /// <summary>
        /// gets the total available capacity
        /// </summary>
        /// <returns></returns>
        public int getTotalCapacity()
        {
            return totalCapacity;
        }

        /// <summary>
        /// gets the vehicle capacity remaining in lb
        /// </summary>
        /// <returns></returns>
        public int getTotalCapacityRemaining()
        {
            return (int) (path.vehicle.payload - totalCapacity);
        }

        /// <summary>
        /// gets the total time 
        /// </summary>
        /// <returns></returns>
        public float getTotalTime()
        {
            return endTime;
        }

        /// <summary>
        /// gets the remaining time
        /// </summary>
        /// <returns></returns>
        public float getRemainingTime()
        {
            return 24f - endTime;
        }

        /// <summary>
        /// gets the total food weight delivered 
        /// </summary>
        /// <returns></returns>
        public int getTotalFoodDelivered()
        {
            return totalFoodDelivered;
        }

        /// <summary>
        /// gets the total parcel weight delivered 
        /// </summary>
        /// <returns></returns>
        public int getTotalParcelDelivered()
        {
            return totalParcelDelivered;
        }


        /// <summary>
        /// checks is the delivery path is valid with respect to total capacity,
        /// total range, and time constraints
        /// </summary>
        /// <returns></returns>
        public bool isValid()
        {
            return foodCheck &&
                (getTotalCapacityRemaining() >= 0) &&
                (getTotalRangeRemaining() >= 0) &&
                (getRemainingTime() >= 0);
        }


    }
}
