using System;
using System.Collections.Generic;
using DesignerAssets;
using UnityEngine;

namespace DesignerTools
{

    /// <summary>
    /// 
    /// Helper class to show and assist in displaying AI generated designs
    /// 
    /// </summary>
    class AIHelper
    {

        /// <summary>
        /// stores the AI designer results from the server, with the configuration 
        /// as the key and the performance metrics as the values
        /// </summary>
        public Dictionary<string, double[]> aiUavs { get; set; }

        /// <summary>
        /// quick reference to the keys in the above dictionary to interate through
        /// </summary>
        public string[] keys { get; set; }

        /// <summary>
        /// used to determine the number of frame updates that oocur after creating
        /// graphical configurations (Unity appears to take a frame or two to clean
        /// up destroyed objects)
        /// </summary>
        public int aiStatus { get; set; }

        /// <summary>
        /// the current index in the key array when making graphical representations
        /// </summary>
        public int aiPrototypeIndex { get; set; }

        /// <summary>
        /// stores the last vehicle in the designer before changing to AI view, used 
        /// to restore to the original vehicle if the user does not select one of
        /// the AI generated designs
        /// </summary>
        public string baseVehicle { get; set; }

        /// <summary>
        /// stores a Unity game object with vehicle configurations for mouse selection events
        /// </summary>
        private Dictionary<GameObject, string> aiMap = new Dictionary<GameObject, string>();

        /// <summary>
        /// list of labels in the AI view, used by the designer code to rotate labels properly 
        /// as the user rotates the view
        /// </summary>
        public List<TextMesh> labels = new List<TextMesh>();

        /// <summary>
        /// maximum range of the AI generated designs, used for scaling the metric bars
        /// </summary>
        public static float maxAIRange;

        /// <summary>
        /// maximum cost of the AI generated designs, used for scaling the metric bars
        /// </summary>
        public static float maxAICost;

        /// <summary>
        /// maximum capacity of the AI generated designs, uses for scaling the metric bars
        /// </summary>
        public static float maxAICapacity;

        /// <summary>
        /// sets the width of the metric bars
        /// </summary>
        public float barWidthScale = 5;

        /// <summary>
        /// main constructor
        /// </summary>
        public AIHelper()
        {
            Reset();
        }

        /// <summary>
        /// 
        /// resets the AIHelper values to the default
        /// 
        /// </summary>
        private void Reset()
        {
            aiPrototypeIndex = 0;
            aiStatus = 0;
            aiMap.Clear();
            labels.Clear();
        }

        /// <summary>
        /// 
        /// calls the central server to return AI designer vehicles based on the performance 
        /// metrics
        /// 
        /// </summary>
        /// <param name="range">range of the base vehicle in miles</param>
        /// <param name="cost">cost of the base vehicle in $</param>
        /// <param name="capacity">capacity of the base vehicle in lb</param>
        /// <param name="config">configuration of the base vehicle</param>
        public void callWebServiceForAIDesigns(float range, float cost, float capacity, string config)
        {
            baseVehicle = config;
            DataInterface.GetAIVehicles(range, cost, capacity);
        }

        /// <summary>
        /// 
        /// generates graphical vehicle representations for each AI generated design
        /// 
        /// </summary>
        /// <param name="aiResults">designer AI vehicles with the configuration as the key and 
        /// performance metrics as the values</param>
        public void startDesignerAIDisplayGeneration(Dictionary<string, double[]> aiResults)
        {

            Reset();

            // copy the cached queue
            aiUavs = new Dictionary<string, double[]>();
            foreach (string config in aiResults.Keys)
                aiUavs[config] = aiResults[config];

            // create the keys array for iteration
            keys = new string[aiUavs.Keys.Count];
            aiUavs.Keys.CopyTo(keys, 0);

            maxAIRange = 0;
            maxAICost = 0;
            maxAICapacity = 0;

            // get the maximum bounds for each metric
            foreach (string config in aiUavs.Keys)
            {
                maxAIRange = (float) Math.Max(maxAIRange, aiUavs[config][0]);
                maxAICost = (float) Math.Max(maxAICost, aiUavs[config][1]);
                maxAICapacity = (float) Math.Max(maxAICapacity, aiUavs[config][2]);
            }

        }

        /// <summary>
        /// 
        /// copies the current vehicle to create a graphical representation of the
        /// AI design
        /// 
        /// </summary>
        public void copyToCreateVehicleRepresentation()
        {

            // position the representation within a circle selection
            float angle = (360f / aiUavs.Count) * (Mathf.PI / 180);
            float radius = 140;
            

            // offset the ai selectionby -2000 in the x dir in the Unity scene
            // could have made a new scene but initally kept in the current scene
            // for ease on implementation in the web-based interface
            int xpos = (int)(Mathf.Sin(aiPrototypeIndex * angle) * radius) + -2000;
            int zpos = (int)(Mathf.Cos(aiPrototypeIndex * angle) * radius);

            // if prototype index is valid
            if (aiPrototypeIndex <= keys.Length - 1 && aiPrototypeIndex >= 0)
            {

                // get metric values
                double[] resultValues = aiUavs[keys[aiPrototypeIndex]];
                if (resultValues != null)
                {
                    float minsize = 1.0f;

                    float range = (float) resultValues[0];
                    float cost = (float)resultValues[1];
                    float capacity = (float)resultValues[2];

                    // get all components of the uav design
                    GameObject[] objects = GameObject.FindGameObjectsWithTag(DesignerAssets.UAVDesigner.VEHICLECOMPONENT);

                    // get the bounds
                    Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
                    foreach (GameObject obj in objects) 
                        bounds.Encapsulate(obj.GetComponent<MeshRenderer>().bounds);
                    Vector3 center = bounds.center;

                    // set the scale of the AI design representation by getting the minimum bounds
                    // and then dividing this minumum value
                    minsize = System.Math.Min(minsize, bounds.size.x);
                    minsize = System.Math.Min(minsize, bounds.size.y);
                    minsize = System.Math.Min(minsize, bounds.size.z);
                    float scale = 2 / minsize;

                    // set the position of the ai design representation
                    float clonex = xpos;
                    float cloney = 0;
                    float clonez = zpos;

                    // copy all vehicle components
                    foreach (GameObject obj in objects)
                    {

                        // clone the component to the new AI design position
                        Vector3 basePosition = obj.transform.position;
                        Vector3 clonePosition = new Vector3(
                            scale * basePosition.x + clonex, 
                            scale * basePosition.y + cloney, 
                            scale * basePosition.z + clonez);
                        GameObject childObject = GameObject.Instantiate(obj, clonePosition, obj.transform.rotation) as GameObject;

                        // apply the scale 
                        Vector3 localScale = childObject.transform.localScale;
                        childObject.transform.localScale = new Vector3(
                            scale * localScale.x, 
                            scale * localScale.y, 
                            scale * localScale.z);
                        childObject.tag = DesignerAssets.UAVDesigner.AI;

                        // add the game object to a dictionary for mouse selections
                        aiMap[childObject] = keys[aiPrototypeIndex];

                    }

                    // calculate the height of the metric bars above each design
                    float rangeHeight = (1 - (maxAIRange - range) / maxAIRange);
                    float costHeight = (1 - (maxAICost - cost) / maxAICost);
                    float capacityHeight = (1 - 1f * (maxAICapacity - capacity) / maxAICapacity);

                    // add the metric bars
                    addMetricBar("capacitylabel", "Capacity(lb)=", capacity, clonex, clonez, capacityHeight, 14, clonex + 2 * barWidthScale, Color.blue);
                    addMetricBar("costlabel", "Cost($)=", cost, clonex + barWidthScale, clonez, costHeight, 22, clonex + 2 * barWidthScale, Color.yellow);
                    addMetricBar("rangelabel", "Range(mi)=", range, clonex + 2 * barWidthScale, clonez, rangeHeight, 30, clonex + 2*barWidthScale, Color.green);

                }

            }

            // hide popup 
            GameObject.Find(UAVDesigner.POPUPRESULTSPANEL).GetComponent<Canvas>().enabled = false;

        }

        /// <summary>
        /// 
        /// Adds a metric bar above the AI design representation
        /// 
        /// </summary>
        /// <param name="id">Unity game engine object id</param>
        /// <param name="title">label for the metric bar</param>
        /// <param name="value">value of the metric</param>
        /// <param name="x">x position of the metric bar</param>
        /// <param name="z">z position of the metric bar</param>
        /// <param name="propotionHeight">proportional height of the bar</param>
        /// <param name="textoffest">vertical offset of text label in the y direction</param>
        /// <param name="xTextPosition">x position of the text label</param>
        /// <param name="color">color of the metric bar</param>
        private void addMetricBar(string id, string title, float value, 
            float x, float z, float propotionHeight, int textoffest, 
            float xTextPosition, Color color)
        {

            // assign position and height properties for the metric bars above the 
            // AI design representation
            float barBottomYPos = 40;
            float barHeight = 40;

            // add bar and label
            GameObject metricBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            metricBar.transform.position = new Vector3(x + barWidthScale + 1, barBottomYPos + propotionHeight * barHeight / 2.0f, z);
            metricBar.transform.localScale = new Vector3(barWidthScale, propotionHeight * barHeight, barWidthScale);
            metricBar.GetComponent<MeshRenderer>().material.color = color;
            metricBar.tag = DesignerAssets.UAVDesigner.AI;

            // add the gameobject to the dictionary to links mouse event to the design configuration
            aiMap[metricBar] = keys[aiPrototypeIndex];

            // add the text label 
            GameObject metricLabel = new GameObject(id);
            metricLabel.AddComponent<TextMesh>();
            metricLabel.GetComponent<TextMesh>().text = title + (float)(System.Math.Round((double)value, 2));
            metricLabel.GetComponent<TextMesh>().fontSize = 60;
            metricLabel.tag = DesignerAssets.UAVDesigner.AI;
            metricLabel.transform.position = new Vector3(xTextPosition, barBottomYPos + barHeight + textoffest, z);
            metricLabel.transform.Rotate(new Vector3(0, 1, 0), 180);
            labels.Add(metricLabel.GetComponent<TextMesh>());

        }

        /// <summary>
        /// 
        /// gets a vehicle configuration for a selected AI game object. Returns null if the
        /// game object is not in the dictionary.
        /// 
        /// </summary>
        /// <param name="obj">selected GameObject in Unity</param>
        /// <returns>vehicle configuration</returns>
        public string getSelected(GameObject obj)
        {
            return aiMap[obj];
        }

        /// <summary>
        /// gets the metrics of an AI design
        /// </summary>
        /// <param name="config"></param>
        /// <returns>metrics (range, cost, capacity)</returns>
        public double[] getMetrics(string config)
        {
            return aiUavs[config];
        }

        /// <summary>
        /// 
        /// remove graphical objects of AI generated designs
        /// 
        /// </summary>
        public void removeAIGeneratedUAVDisplays()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(DesignerAssets.UAVDesigner.AI);
            foreach (GameObject obj in objects)
                GameObject.Destroy(obj);
            labels.Clear();
            Reset();
        }

    }
}
