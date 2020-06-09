using System.Collections.Generic;

/// <summary>
/// 
/// Includes all data storage objects of the interface, including vehicles,
/// scenarios, and operational plans. The data storage objects for a Vehicle
/// is a simple class with paremeters. Scenarios are stored as a nested class with customers 
/// and each customer has an address. Operational plans are stored as nested objects with
/// a scenario reference, and a list of paths. Each path has a reference to a vehicle
/// and set of customers.
/// 
/// This class also includes DataLog and Session objects.
/// 
/// </summary>

namespace DataObjects
{

    /// <summary>
    /// 
    /// stores data for a vehicle
    /// 
    /// </summary>
    public class Vehicle
    {

        private static int counter = 0;

        public Vehicle(){}

        /// <summary>
        /// database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// name or tag of the vehicle
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        /// the string representation of the vehicle
        /// </summary>
        public string config { get; set; }

        /// <summary>
        /// result of the vehicle (Success or Failure)
        /// </summary>
        public string result { get; set; }

        /// <summary>
        /// range in the miles
        /// </summary>
        public double range { get; set; }

        /// <summary>
        /// velocity in mph
        /// </summary>
        public double velocity { get; set; }

        /// <summary>
        /// fixed cost 
        /// </summary>
        public double cost { get; set; }

        /// <summary>
        /// capacity in lb
        /// </summary>
        public double payload { get; set; }

        /// <summary>
        /// unique hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// tostring just returns the string configuration
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return config;
        }

        /// <summary>
        /// 
        /// clones the vehicle
        /// 
        /// </summary>
        /// <returns></returns>
        public Vehicle clone()
        {
            Vehicle vehicle = new Vehicle();
            vehicle.id = id;
            vehicle.tag = tag;
            vehicle.config = config;
            vehicle.range = range;
            vehicle.velocity = velocity;
            vehicle.cost = cost;
            vehicle.payload = payload;
            return vehicle;
        }
        
    }

    /// <summary>
    /// 
    /// a customer includes a location, market, delivery type, and weight
    /// 
    /// </summary>
    public class Customer
    {
        public Customer() { }

        /// <summary>
        /// database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// address of the customer
        /// </summary>
        public Address address { get; set; }

        /// <summary>
        /// market id of the the customer
        /// </summary>
        public int market { get; set; }

        /// <summary>
        /// type of delivery (food or parcel)
        /// </summary>
        public string payload { get; set; }

        /// <summary>
        /// numerical weight of the delivery 
        /// </summary>
        public float weight { get; set; }

        /// <summary>
        /// if the customer is selected
        /// </summary>
        public bool selected { get; set; }

        /// <summary>
        /// clones the customer to a CustomerDelivery object
        /// </summary>
        /// <returns></returns>
        public CustomerDelivery clone()
        {
            CustomerDelivery customerPath = new CustomerDelivery();
            customerPath.id = id;
            customerPath.address = address;
            customerPath.market = market;
            customerPath.payload = payload;
            customerPath.selected = selected;
            customerPath.weight = weight;
            return customerPath;
        }

        /// <summary>
        /// unique id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// simple tostring implementation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Customer:" + id;
        }

    }

    /// <summary>
    /// 
    /// Extends Customer to include a delivery time
    /// 
    /// </summary>
    public class CustomerDelivery : Customer
    {
        public CustomerDelivery() { }
 
        /// <summary>
        /// delivery time in hours
        /// </summary>
        public float deliverytime { get; set; }

    }


    /// <summary>
    /// 
    /// Address includes and x and z location in Unity
    /// 
    /// </summary>
    public class Address
    {

        public Address() { }

        /// <summary>
        /// unique database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// x position of the address
        /// </summary>
        public float x { get; set; }

        /// <summary>
        /// z position of the address
        /// </summary>
        public float z { get; set; }

        /// <summary>
        /// region of the address (currently unused)
        /// </summary>
        public string region { get; set; }

        /// <summary>
        /// unique id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// simple ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Address:" + id;
        }

    }

    /// <summary>
    /// 
    /// scenario (or market) includes selected customers and a warehouse
    /// 
    /// </summary>
    public class Scenario
    {

        /// <summary>
        /// database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// tag or name of the scenario
        /// </summary>
        public string tag { get; set; }

        /// <summary>
        /// version of the scenario
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// warehouse of the scenario
        /// </summary>
        public Warehouse warehouse { get; set; }

        /// <summary>
        /// list of customers of the scenario
        /// </summary>
        public List<Customer> customers { get; set; }

        /// <summary>
        /// gets the customer by an id (currently a linear search)
        /// </summary>
        /// <param name="i">id of the customer</param>
        /// <returns>the customer with the id</returns>
        public Customer getCustomer(int i)
        {
            foreach (Customer customer in customers)
                if (customer.id == i)
                    return customer;
            return null;
        }

        /// <summary>
        /// simple ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Scenario:" + id;
        }

    }

    /// <summary>
    /// 
    /// warehouse includes an address 
    /// 
    /// </summary>
    public class Warehouse
    {
        public Warehouse(){}

        /// <summary>
        /// databases id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// address of the warehouse
        /// </summary>
        public Address address { get; set; }

        /// <summary>
        /// unique id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// simple ToString method 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Warehouse:" + id;
        }

    }

    /// <summary>
    /// 
    /// vehicle delivery include a list of customer deliveries, 
    /// warehouse, and a vehicle
    /// 
    /// </summary>
    public class VehicleDelivery
    {

        private static int counter = 0;

        public VehicleDelivery()
        {
            this.id = counter;
            counter += 1;
            customers = new List<CustomerDelivery>();
        }

        /// <summary>
        /// database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// vehicle used for the delivery
        /// </summary>
        public Vehicle vehicle { get; set; }

        /// <summary>
        /// warehouse for the vehicle path
        /// </summary>
        public Warehouse warehouse { get; set; }

        /// <summary>
        /// list of customers delivered to
        /// </summary>
        public List<CustomerDelivery> customers { get; set; }

        /// <summary>
        /// starting leave time in hours
        /// </summary>
        public float leavetime { get; set; }

        /// <summary>
        /// return time in hours
        /// </summary>
        public float returntime { get; set; }

        /// <summary>
        /// unique id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// simple ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Path:" + id;
        }

    }

    /// <summary>
    /// 
    /// plan includes a scenario and list of vehicle deliveries
    /// 
    /// </summary>
    public class Plan
    {

        public Plan()
        {
            paths = new List<VehicleDelivery>();
        }

        /// <summary>
        /// database id for the plan
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// name or tag of the plan
        /// </summary>
        public string tag { set; get; }

        /// <summary>
        /// scenario associated with the plan
        /// </summary>
        public Scenario scenario { set; get; }

        /// <summary>
        /// list of delivery paths of the plan
        /// </summary>
        public List<VehicleDelivery> paths { set; get; }

        /// <summary>
        /// unique id
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return id;
        }

        /// <summary>
        /// simple ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Plan:" + id;
        }

    }

    /// <summary>
    /// 
    /// DataLog object saves log statements to a central server
    /// 
    /// </summary>
    public class DataLog
    {
        public DataLog(){}

        /// <summary>
        /// database id
        /// </summary>
        public int id { set; get; }

        /// <summary>
        /// user of the log statement 
        /// </summary>
        public string user { set; get; }

        /// <summary>
        /// team of the user
        /// </summary>
        public string team { set; get; }

        /// <summary>
        /// the log or action statement
        /// </summary>
        public string action { set; get; }   

    }

    /// <summary>
    /// stores the session information
    /// </summary>
    public class Session
    {

        /// <summary>
        /// database id
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// name of the session
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// does this session include AI tools
        /// </summary>
        public bool use_ai { get; set; }

        /// <summary>
        /// status of the session
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// experiment of the session
        /// </summary>
        public int experiment { get; set; }

        /// <summary>
        /// session variable
        /// </summary>
        public int session { get; set; }

        /// <summary>
        /// prior experiment variables
        /// </summary>
        public int prior_experiment { get; set; }

        /// <summary>
        /// market of the experiment
        /// </summary>
        public int market { get; set; }


    }
    
}
