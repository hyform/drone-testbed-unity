using System.Collections.Generic;
using UnityEngine;
using Assets;
using PlannerAssets;
using DataObjects;
using System;
using TMPro;
using PlanToolHelpers;
using Newtonsoft.Json;

/// <summary>
/// 
/// This is the main file for the operational planner code. It extends the BaseDeliveryInterface class.
/// 
/// </summary>
public class OpsPlanInterface : BaseDeliveryInterface
{

    /// <summary>
    /// 
    /// Unity GameObject storing the selected connector for delivery path 
    /// dragging operations 
    /// 
    /// </summary>
    protected GameObject selectedConnector = null;

    /// <summary>
    /// 
    /// store the amount of capacity remaining in the current selected delivery path
    /// 
    /// </summary>
    protected double manualPathCapacityRemaining = 0;

    /// <summary>
    /// 
    /// store the amount of range remaining in the current selected delievery path
    /// 
    /// </summary>
    protected double manualPathRangeRemaining = 0;

    /// <summary>
    /// 
    /// stores the scroll position of the selected vehicle scroll window on the left side
    /// 
    /// </summary>
    protected Vector2 scrollPositionVehicles;

    /// <summary>
    /// 
    /// stores the scroll position of the dashboard list of team vehicles
    /// 
    /// </summary>
    protected Vector2 scrollPositionVehicleSelection;

    /// <summary>
    /// 
    /// toggle that stores the visibility of the dashboard list of team vehicles
    /// 
    /// </summary>
    protected bool vehicleDashboardView = true;

    /// <summary>
    /// stores the Remove All Paths rectangular box for tooltip display 
    /// </summary>
    protected Rect removeAllPathsRect = new Rect(120, 10, 28, 28);

    /// <summary>
    /// stores the team plans rectangular box for tooltip display 
    /// </summary>
    protected Rect teamPlansRect;

    /// <summary>
    /// stores the AI button rectangular box for tooltip display 
    /// </summary>
    protected Rect aiButtonRect;

    /// <summary>
    /// stores the Submit button rectangular box for tooltip display 
    /// </summary>
    protected Rect submitRect;

    /// <summary>
    /// stores the refresh database load button rectangular box for tooltip display 
    /// (might be able to remove this since objects are auto refreshed)
    /// </summary>
    protected Rect vehicleLoadRect = new Rect(20, 50, 28, 28);

    /// <summary>
    /// stores the selected vehicle list rectangular box for tooltip display 
    /// </summary>
    protected Rect vehicleRect;

    /// <summary>
    /// stores the dashboard vehicle list rectangular box for tooltip display 
    /// </summary>
    protected Rect vehicleSelectionRect;

    /// <summary>
    /// 
    /// GUI controls specific to the planner
    /// 
    /// </summary>
    protected override void OnGUICustom() {

        // get screen size and set scroll window height
        Rect rect = Camera.main.pixelRect;
        int scrollheight = (int)Math.Max(rect.height - 250, 200);

        // right panel where users can select plans to open, add a button to open a plan
        int counter = 0;
        teamPlansRect = new Rect(rect.width - 180, 10, 170, scrollheight + 20);
        GUI.Box(teamPlansRect, new GUIContent("Team Plans", "Select a Plan To Open"));
        scrollPositionOperationplans = GUI.BeginScrollView(new Rect(rect.width - 174, 40, 164, scrollheight), 
            scrollPositionOperationplans, new Rect(0, 0, 140, 20 + loadedPlans.Count * 21));

        // for each plan id loaded, add a button to open the plan, if not loaded fully, display it
        // but unhighlight it and remove the event on the button
        foreach (int id in loadedPlans.Keys)
        {

            // check if fully loaded
            bool loaded = (loadedPlans[id].plan != null);
            string tag = (!loaded ? " *" : "") + loadedPlans[id].tag;
            GUI.color = loaded ? Color.white : Color.gray;
            if (GUI.Button(new Rect(0, 10 + 20 * counter, 164, 20), tag))
            {
                if (loaded)
                {
                    userSelectedPlan = loadedPlans[id].plan;
                    OpenPlan(loadedPlans[id].plan);
                }
            }
            GUI.color = Color.white;
            counter += 1;
        }
        GUI.EndScrollView();

        // add a toolbar button to remove all plan paths
        if (GUI.Button(removeAllPathsRect, new GUIContent(GUIHelpers.removeimage, "Remove All Paths")))
        {
            playClick();
            RemovePaths();
            Capture.Log("RemoveAllPaths;" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);
        }

        // submit plan button to your team, it will open a popup for confirmation
        submitRect = new Rect(192, rect.height - 56, 80, 24);
        if (GUI.Button(submitRect, new GUIContent("Submit", "Submit the Plan to Your Team")))
        {

            // check that the plan has delivery customers
            int totalCustomers = 0;
            foreach(DataObjects.VehicleDelivery p in plan.paths)
                totalCustomers += p.customers.Count;
            
            // open popup
            if (totalCustomers > 0)
            {
                GUIAssets.PopupButton.showing = true;
                GUIAssets.PopupButton.popupPanelID = "SubmitCanvas";
                GameObject.Find("SubmitCanvas").GetComponent<Canvas>().enabled = true;
            }
            else
            {
                ShowMsg("Empty Plan", true);
            }

        }

        // add AI button if the session includes it
        if (Startup.isAI)
        {
            aiButtonRect = new Rect(10, rect.height - 210, 32, 32);
            if (GUI.Button(aiButtonRect, new GUIContent(GUIHelpers.aiimage, "AI Agent to Generate Plan Alternatives")))
            {
                vehicleDashboardView = false;
                runAIAgent();
                Capture.Log("RunPathAgent", Capture.PLANNER);
            }
        }

        // add controls for the purchased vehicle list on the left
        int heightVehicleScroll = (int)rect.height - 270;

        // labels for database vehicles
        GUI.Box(new Rect(10, 50, 272, heightVehicleScroll), "Purchased Designs");
        GUI.color = Color.white;
        int yInc = 28;

        // scroll window for all plan vehicles
        vehicleRect = new Rect(10, 80, 272, heightVehicleScroll - 48);
        scrollPositionVehicles = GUI.BeginScrollView(vehicleRect, scrollPositionVehicles, new Rect(0, 0, 272, 20 + plan.paths.Count * yInc));

        // add GUI controls for vehicle paths
        for (int i = 0; i < plan.paths.Count; i++)
        {

            string vehicletag = plan.paths[i].vehicle.tag;
            double cost = plan.paths[i].vehicle.cost;
            double range = plan.paths[i].vehicle.range;
            double velocity = plan.paths[i].vehicle.velocity;
            double payload = plan.paths[i].vehicle.payload;

            string tooltipStr = "Selects " + vehicletag + " , range = " + range.ToString("0.0") + " mi , capacity = " + payload.ToString("0") + " lb , $" + cost.ToString("0");

            GUI.color = Color.white;
            if (selectedPathIndex == i)
                GUI.color = Color.cyan;

            // select a vehicle to highlight and edit it
            if (GUI.Button(new Rect(26, 10 + i * yInc, 164, 24), new GUIContent(vehicletag, tooltipStr)))
            {

                vehicleDashboardView = false;
                if(selectedPathIndex == i)
                    selectedPathIndex = -1;
                else
                {
                    selectedPathIndex = i;
                    PlanPathCalculation planPathCalculation = new PlanPathCalculation(plan.paths[i]);
                    planPathCalculation.calculate();
                    manualPathRangeRemaining = planPathCalculation.getTotalRangeRemaining();
                    manualPathCapacityRemaining = planPathCalculation.getTotalCapacityRemaining();
                }

                RefreshPaths();
                playClick();
                Capture.Log("SelectPath;" + i + ";" + vehicletag, Capture.PLANNER);

            }

            // draw vehicle metrics as bar charts
            GUI.color = Color.white;
            GUI.Box(new Rect(192, 14 + 16 + i * yInc - (float)(16 * range / maxVehicleRange), 12, 16 * (float)(range / maxVehicleRange)), "", Assets.GUIHelpers.whiteStyle);
            GUI.Box(new Rect(206, 14 + 16 + i * yInc - (float)(16 * payload / maxVehicleCapacity), 12, 16 * (float)(payload / maxVehicleCapacity)), "", Assets.GUIHelpers.lightgrayStyle);
            GUI.Box(new Rect(220, 14 + 16 + i * yInc - (float)(16 * cost / maxVehicleCost), 12, 16 * (float)(cost / maxVehicleCost)), "", Assets.GUIHelpers.grayRedStyle);

            
            GUI.color = Color.white;
            // timing issue in Unity when deleting vehicles, I think this is fixed
            // since the remove button in below this in the code now
            try
            {
                if (plan.paths[i].customers.Count == 0)
                    GUI.color = new Color(0.2f, 0.2f, 0.2f);
            } catch (Exception e)
            {
                Debug.Log(e);
            }

            // add the remove path button
            // only show if the dashboard view is not shown, since mouse actions go through 
            if (!vehicleDashboardView)
            {

                // adds the remove button to the view
                if (GUI.Button(new Rect(234, 10 + i * yInc, 24, 24), new GUIContent(GUIHelpers.removeimage, "Remove the Path")))
                {

                    // set the remaining variable to the full values
                    manualPathRangeRemaining = plan.paths[i].vehicle.range;
                    manualPathCapacityRemaining = plan.paths[i].vehicle.payload;
                    plan.paths[i].customers.Clear();

                    selectedPathIndex = i;

                    // remove and add path to show highlighted path
                    RefreshPaths();

                    playClick();

                    Capture.Log("VehiclePathRemoved;" + i + ";" + plan.paths[i].vehicle.tag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);

                }
            }

            GUI.color = Color.white;
            if (GUI.Button(new Rect(0, 10 + i * yInc, 24, 24), new GUIContent("-", "Remove Design from Purchased List")))
            {

                selectedPathIndex = -1;
                plan.paths.RemoveAt(i);

                // refresh the path lines
                RefreshPaths();

                playClick();
                Capture.Log("VehicleRemoved;" + i + ";" + vehicletag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);
                ShowMsg(vehicletag + " Removed", false);

                if (plan.paths.Count == 0)
                    vehicleDashboardView = true;

            }

        }
        GUI.EndScrollView();

        // show the dashboard view in the upper center of the window with the range and capacity
        // remaining for the display
        if (selectedPathIndex != -1)
        {
            GUI.Box(new Rect(290, 10, 360, 62), "");
            GUI.Label(new Rect(384, 20, 10, 52), "0");

            float maxRange = (float) plan.paths[selectedPathIndex].vehicle.range;
            float maxCapacity = (float)plan.paths[selectedPathIndex].vehicle.payload;

            GUI.Label(new Rect(505, 10, 28, 56), "\n" + maxRange.ToString("0.0") + "\n" + maxCapacity.ToString("0.0"));

            GUI.Box(new Rect(399, 30, 1, 36), "", Assets.GUIHelpers.whiteStyle);
            GUI.Box(new Rect(501, 30, 1, 36), "", Assets.GUIHelpers.whiteStyle);
            GUI.Label(new Rect(294, 10, 390, 56), "\n  Range (mi)\n  Weight (lb)");
            GUI.Label(new Rect(542, 10, 390, 56), "\n  Remaining=" + manualPathRangeRemaining.ToString("0.0") + "\n  Remaining=" + manualPathCapacityRemaining.ToString("0"));

            GUI.color = Color.cyan;

            GUI.Label(new Rect(294, 10, 390, 56), plan.paths[selectedPathIndex].vehicle.tag + "\n\n");

            GUI.Box(new Rect(400, 36, (float) (100f * (1 - manualPathRangeRemaining / maxRange)), 10), "", Assets.GUIHelpers.whiteStyle);
            GUI.Box(new Rect(400, 50, (float) (100f * (1 - manualPathCapacityRemaining / maxCapacity)), 10), "", Assets.GUIHelpers.whiteStyle);

            GUI.color = Color.white;

        }

        // add a button to toggle the information help panel
        GUI.color = Color.white;
        if (GUI.Button(infoRect, new GUIContent(GUIHelpers.infoimage, "Toggle Information Panel")))
        {
            showHelpInfoPanel = !showHelpInfoPanel;
            playClick();
            GameObject.Find("helpcanvasplanner").GetComponent<Canvas>().enabled = showHelpInfoPanel;
            Capture.Log("ToggleInfoPanel:" + showHelpInfoPanel, Capture.PLANNER);
        }

        // add a button to toggle the information help panel
        vehicleSelectionRect = new Rect(244, 52, 28, 28);
        if (GUI.Button(vehicleSelectionRect, new GUIContent(GUIHelpers.dashboardimage, "Add a Design From the Design Team")))
        {
            vehicleDashboardView = !vehicleDashboardView;
            if (vehicleDashboardView)
                selectedPathIndex = -1;
            playClick();
            Capture.Log("ShowVehicleList;" + vehicleDashboardView, Capture.PLANNER);
        }

        // add the team design selection list
        if (vehicleDashboardView)
        {

            // multiple boxes adds opacity
            for(int i = 0; i < 4; i++)
                GUI.Box(new Rect(200, 94, 260, 400), "Team Designs");

            scrollPositionVehicleSelection = GUI.BeginScrollView(new Rect(210, 130, 260, 300), 
                scrollPositionVehicleSelection, new Rect(0, 0, 240, 30 * teamVehicles.Count));

            // add all team vehicles to the list
            int counterdash = 0;
            foreach (Vehicle vehicle in teamVehicles)
            {

                string tooltipStr = "Add design " + vehicle.tag + " , range = " + vehicle.range.ToString("0.0") + " mi , capacity = " 
                    + vehicle.payload.ToString("0") + " lb , $" + vehicle.cost.ToString("0");

                // add the plus button to add a vehicle to the purchased list
                if (GUI.Button(new Rect(0, counterdash * 30, 28, 28), new GUIContent("+", tooltipStr)))
                {
                    DataObjects.VehicleDelivery p = simplePath();
                    p.vehicle = vehicle;

                    try
                    {
                        p.warehouse = scenario.warehouse;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e);
                    }

                    plan.paths.Add(p);
                    RefreshPaths();
                    playClick();
                    Capture.Log("VehicleAdd;" + vehicle.tag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);
                    ShowMsg(vehicle.tag + " Added", false);

                }

                // add the vehicle tag
                GUI.Label(new Rect(30, counterdash * 30, 164, 28), vehicle.tag);

                // add the metric bars
                GUI.color = Color.white;
                GUI.Box(new Rect(186, (counterdash + 1) * 30 - 8 - 16 * (float)(vehicle.range / maxVehicleRange), 12, 16 * (float)(vehicle.range / maxVehicleRange)), "", Assets.GUIHelpers.whiteStyle);
                GUI.Box(new Rect(202, (counterdash + 1) * 30 - 8 - 16 * (float)(vehicle.payload / maxVehicleCapacity), 12, 16 * (float)(vehicle.payload / maxVehicleCapacity)), "", Assets.GUIHelpers.lightgrayStyle);
                GUI.Box(new Rect(218, (counterdash + 1) * 30 - 8 - 16 * (float)(vehicle.cost / maxVehicleCost), 12, 16 * (float)(vehicle.cost / maxVehicleCost)), "", Assets.GUIHelpers.grayRedStyle);

                GUI.color = Color.white;
                counterdash += 1;

            }
            GUI.EndScrollView();

            // add the hide button at the bottom of the view
            if (GUI.Button(new Rect(250, 456, 140, 28), "Hide"))           
                vehicleDashboardView = false;
            
        }

        // lock the camera view if the dashboard view is opened or the user is 
        // dragging a selected component
        MoveCamera.lockView = vehicleDashboardView || (selectedConnector != null);

    }

    protected override void tooltipDisplay() {
        if (teamPlansRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(teamPlansRect.xMin - 140, 100, 400, 40);
        if (aiButtonRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(aiButtonRect.xMax, aiButtonRect.yMin - aiButtonRect.height + 2, 400, aiButtonRect.height);
        if (submitRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(submitRect.xMax, submitRect.yMin - submitRect.height + 2, 400, submitRect.height);
        if (vehicleLoadRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(vehicleLoadRect.xMax, vehicleLoadRect.yMin - vehicleLoadRect.height + 2, 400, vehicleLoadRect.height);
        if (vehicleSelectionRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(vehicleSelectionRect.xMax, vehicleSelectionRect.yMin + 2, 400, 40);
        if (vehicleRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(vehicleRect.xMax, vehicleRect.yMin - 20, 800, 40);
    }

    /// <summary>
    /// 
    /// opens a prompt to confirm that changes will be lost ( based on MITRE feedback)
    /// 
    /// </summary>
    /// <param name="planopen">Plan to open</param>
    protected override void OpenPlan(Plan planopen)
    {
        GUIAssets.PopupButton.popupPanelID = "popupconfirm";
        GUIAssets.PopupButton.storedData = "OpenPlan";
        GameObject.Find("popupconfirm").GetComponent<Canvas>().enabled = true;
        playClick();
    }

    /// <summary>
    /// 
    /// update method for the planner view
    /// 
    /// </summary>
    protected override void customUpdate()
    {

        // check for a path dragging operation
        if (selectedConnector != null)
        {

            GameObject[] objs = GameObject.FindGameObjectsWithTag("connectiondrag");
            foreach (GameObject go in objs)          
                Destroy(go);
            
            Customer[] customers = connectorMap[selectedConnector];

            Vector3 positionA = (customers[0] == null) ?
                ScaleInScene(new Vector3(plan.paths[selectedPathIndex].warehouse.address.x, 1, plan.paths[selectedPathIndex].warehouse.address.z)) :
                ScaleInScene(new Vector3(customers[0].address.x, 1, customers[0].address.z)) ;

            Vector3 positionB = (customers[1] == null) ?
                ScaleInScene(new Vector3(plan.paths[selectedPathIndex].warehouse.address.x, 1, plan.paths[selectedPathIndex].warehouse.address.z)) :
                ScaleInScene(new Vector3(customers[1].address.x, 1, customers[1].address.z));

            // get mouse position
            float[] getlocation = mouseRayPoint();

            // draw cylinders
            CreateCylinderBetweenPoints(positionA, new Vector3(getlocation[0], 1, getlocation[2]), 0.16f, Color.gray, 0, "connectiondrag");
            CreateCylinderBetweenPoints(positionB, new Vector3(getlocation[0], 1, getlocation[2]), 0.16f, Color.gray, 0, "connectiondrag");

        }

        // up listener for dragging (end of the drag)
        if (Input.GetMouseButtonUp(0) && selectedConnector != null)
        {

            // check if there is a selected customer at the end of dragging
            Customer customer = customerAtMouse();
            if (customer != null)   
                if (customer.selected)
                {

                    // get the id of the customer
                    int customerId = customer.id;

                    // get the position to insert into the VehiclePath
                    Customer connectorStartingCustomer = connectorMap[selectedConnector][0];
                    int position = 0;

                    // not the starting segment from the warehouse
                    if (connectorStartingCustomer != null) 
                    {
                        List<CustomerDelivery> customers = plan.paths[selectedPathIndex].customers;
                        for (int i = 0; i < customers.Count; i++)
                            if (connectorStartingCustomer.id == customers[i].id)                            
                                position = i + 1;
                    }

                    // insert the customer
                    Capture.Log("ManualPathAddedDrag;AfterCustomerIndex=" + position, Capture.PLANNER);
                    insertCustomer(customerId, position);

                }
            
            // cleans up connection drags objects
            GameObject[] objs = GameObject.FindGameObjectsWithTag("connectiondrag");
            foreach (GameObject go in objs)
                Destroy(go);

            // set the selected connector 
            selectedConnector = null;

        }


        // right click to remove a customer in a path
        if (Input.GetMouseButtonDown(1) && selectedPathIndex != -1)
        {

            // check for a customer
            Customer customer = customerAtMouse();
            if (customer != null)
            {
                int customerID = customer.id;
                int pathindex = -1;

                // get its position in the delivery path
                List<CustomerDelivery> currentPath = plan.paths[selectedPathIndex].customers;
                for (int i = 0; i < currentPath.Count; i++)              
                    if (currentPath[i].id == customerID)                   
                        pathindex = i;
                
                // should have found the index
                if (pathindex != -1)
                {

                    string position = "";
                    try
                    {
                        Address address = plan.paths[selectedPathIndex].customers[pathindex].address;
                        position += address.x + "," + address.z;
                    } catch (Exception e)
                    {
                        Debug.Log(e);
                    }

                    // remove the customer
                    plan.paths[selectedPathIndex].customers.RemoveAt(pathindex);
                    RefreshPaths();

                    // recalculate the path
                    PlanPathCalculation pathPathCalculation = new PlanPathCalculation(plan.paths[selectedPathIndex]);
                    pathPathCalculation.calculate();
                    manualPathCapacityRemaining = pathPathCalculation.getTotalCapacityRemaining();
                    manualPathRangeRemaining = pathPathCalculation.getTotalRangeRemaining();

                    Capture.Log("ManualPathRemove;VehicleIndex=" + selectedPathIndex + ";RemovedIndex=" + customerID + ";RemovedPosition=" + position + ";VehicleName=" + plan.paths[selectedPathIndex].vehicle.tag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);

                    playClick();

                }

            }

        }

        // left click to select a customer or connection
        if (Input.GetMouseButtonDown(0) && selectedPathIndex != -1)
        {

            Customer customer = customerAtMouse();

            // check if the customer is selected
            if (customer != null)
            {
                if (customer.selected)
                {
                    int index = customer.id;
                    insertCustomer(index, -1);
                }
            } else
            {
                // check for a selected connector, it will return null if no connector
                // is selected
                selectedConnector = connectionAtMouse();
            }

        }

        // mouse over for customers in the planner view
        if ((!Input.GetMouseButton(0) && 
            !Input.GetMouseButton(1)) || 
            selectedConnector != null)
        {
            // get highlighted customer
            object[] highlighted = customerAtMousePosition();

            // remove house labels
            HideHouseLabels();
            if (highlighted == null)
                GameObject.Find("customerhighlight").transform.position = new Vector3(1000, 1000, 1000);
            else
            {

                // highlight customer
                GameObject obj = (GameObject)highlighted[0];
                Customer customer = (Customer)highlighted[1];

                // if customer is selected (only difference from the business view implementation)
                if (customer.selected)
                {
                    Vector3 pos = obj.transform.position;
                    Vector3 scale = obj.transform.localScale;
                    GameObject.Find("customerhighlight").transform.position = new Vector3(pos.x, 0.02f, pos.z);
                    GameObject.Find("customerhighlight").transform.localScale = new Vector3(1 + customer.weight / 4f, 0.01f*scale.y, 1 + customer.weight / 4f);
                    ShowHouseLabel(customer);
                }

            }
        }

        // remove all connection highlight objects
        GameObject[] highlightobjs = GameObject.FindGameObjectsWithTag("connectionhighlight");
        foreach (GameObject obj in highlightobjs)
            Destroy(obj);
      
        // if a path is selected and there is not a selected connector for dragging
        if (selectedPathIndex != -1 && selectedConnector == null)
        {

            // get the connection at the mouse location
            GameObject connector = connectionAtMouse();
            if (connector != null)
            {

                Customer[] customers = connectorMap[connector];

                Vector3 positionA = (customers[0] == null) ?
                    ScaleInScene(new Vector3(plan.paths[selectedPathIndex].warehouse.address.x, 1, plan.paths[selectedPathIndex].warehouse.address.z)) :
                    ScaleInScene(new Vector3(customers[0].address.x, 1, customers[0].address.z));

                Vector3 positionB = (customers[1] == null) ?
                    ScaleInScene(new Vector3(plan.paths[selectedPathIndex].warehouse.address.x, 1, plan.paths[selectedPathIndex].warehouse.address.z)) :
                    ScaleInScene(new Vector3(customers[1].address.x, 1, customers[1].address.z));

                CreateCylinderBetweenPoints(positionA, positionB, 0.48f, new Color(1.0f, 0.5f, 0.3f), 0, "connectionhighlight");

            }

        }

    }

    /// <summary>
    /// gets the Unity scene position at the mouse location
    /// </summary>
    /// <returns></returns>
    protected float[] mouseRayPoint()
    {
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000.0f))
        {
            return new float[] { hitInfo.point.x, hitInfo.point.y, hitInfo.point.z };
        }
        return null;
    }

    /// <summary>
    /// inserts a Customer into the selected VehiclePath
    /// </summary>
    /// <param name="customerId">customer id</param>
    /// <param name="position">position to insert in the current VehiclePath</param>
    private void insertCustomer(int customerId, int position)
    {
        // gets the current delivery path 
        List<CustomerDelivery> currentPath = plan.paths[selectedPathIndex].customers;
        if (currentPath.Count == 0)
            currentPath = new List<CustomerDelivery>();
        
        // if -1, add to the end of the path
        if (position == -1)
            position = currentPath.Count;

        // check all paths to check if a Customer is selected by another path
        bool alreadySelected = false;
        for (int j = 0; j < plan.paths.Count; j++)
            for (int i = 0; i < plan.paths[j].customers.Count; i++)
                if (plan.paths[j].customers[i].id == customerId)
                    alreadySelected = true;

        // if not selected
        if (!alreadySelected)
        {

            // add costomer from the scenario
            CustomerDelivery customerPath = scenario.getCustomer(customerId).clone();
            plan.paths[selectedPathIndex].customers.Insert(position, customerPath);

            // calculate the path metrics with new inserted node
            PlanPathCalculation planPathCalculation = new PlanPathCalculation(plan.paths[selectedPathIndex]);
            planPathCalculation.calculate();

            // if valid
            if (planPathCalculation.isValid())
            {

                string addedposition = "";
                try
                {
                    Address addedAddress = customerPath.address;
                    addedposition += addedAddress.x + "," + addedAddress.z;
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                RefreshPaths();

                manualPathCapacityRemaining = planPathCalculation.getTotalCapacityRemaining();
                manualPathRangeRemaining = planPathCalculation.getTotalRangeRemaining();
                debugStr = "Path updated";
                Capture.Log("ManualPathAdded;VehicleIndex=" + selectedPathIndex + ";AddedIndex=" + customerId + ";AddedPosition=" + addedposition + ";VehicleTag=" + plan.paths[selectedPathIndex].vehicle.tag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);
                playClick();

            }
            else if (planPathCalculation.getTotalRangeRemaining() < 0) // too long
            {
                plan.paths[selectedPathIndex].customers.RemoveAt(position);
                debugStr = "Path : range to long";
                Capture.Log("ManualPathAdded;RangeTooLong", Capture.PLANNER);
                ShowMsg("Path too long", true);
            }
            else if (planPathCalculation.getTotalCapacityRemaining() < 0) // too high of capacity
            {
                plan.paths[selectedPathIndex].customers.RemoveAt(position);
                debugStr = "Path : weight constraint reached";
                Capture.Log("ManualPathAdded;PayloadConstraint", Capture.PLANNER);
                ShowMsg("Path weight too high", true);
            }
            else if (planPathCalculation.getTotalTime() > 24.0) // too long in duration
            {
                plan.paths[selectedPathIndex].customers.RemoveAt(position);
                debugStr = "Path : duration to long";
                Capture.Log("ManualPathAdded;DurationTooLong", Capture.PLANNER);
                ShowMsg("Path is longer than 24 h", true);
            }
            else if (!planPathCalculation.deliveredFoodinTime()) // food not delivered in time
            {
                plan.paths[selectedPathIndex].customers.RemoveAt(position);
                debugStr = "Path : food not delivered in time";
                Capture.Log("ManualPathAdded;FoodTimeConstraint", Capture.PLANNER);
                ShowMsg("Food is not in time", true);
            }

        }
        else
        {
            debugStr = "Customer has delivery";
            ShowMsg("Customer has delivery", true);
        }
    }

    /// <summary>
    /// 
    /// run the AI planning agent
    /// 
    /// </summary>
    private void runAIAgent()
    {

        // remove memory references
        string planStr = JsonConvert.SerializeObject(plan);
        Plan planCopy = JsonConvert.DeserializeObject<Plan>(planStr);
        // romove all customers
        foreach (VehicleDelivery delivery in planCopy.paths)
            delivery.customers.Clear();

        bool includedVehicles = planCopy.paths.Count > 0;
        if (!includedVehicles)
        {
            ShowMsg("No vehicles selected", true);
            return;
        }

        // remove selected path
        selectedPathIndex = -1;

        // send to server
        GameObject.Find("restapi").GetComponent<RestWebService>().PostPlanToAI(planCopy);

    }

    /// <summary>
    /// 
    /// checks if a popup action occurs
    /// 
    /// </summary>
    protected override void checkPopupCacheCustom()
    {

        // submit a plan
        if (GUIAssets.PopupButton.submit)
        {

            // reset the submit variable
            GUIAssets.PopupButton.submit = false;
            string planTag = GameObject.Find("InputTag").GetComponent<TMP_InputField>().text;

            // make sure the planTag has a value to avoid errors
            if (planTag == null)
                planTag = "";
            
            // check for a value
            if (!planTag.Equals(""))
            {

                // check for the same name
                bool existingName = false;
                foreach (int id in loadedPlans.Keys)
                    if (planTag.Equals(loadedPlans[id].tag))                 
                        existingName = true;

                // keep the plan names short
                bool tooLong = false;
                if (planTag.Length > 20)              
                    tooLong = true;
                

                // submit plan if the name is valid
                if (!existingName && !tooLong)
                {
                    plan.tag = planTag;
                    DataInterface.PostPlan(plan);
                    Capture.Log("SubmitPlanToDB;" + planTag + ";" + JsonConvert.SerializeObject(plan) + ";" + planCalculation.getLogString(), Capture.PLANNER);
                } else if (tooLong)
                {
                    ShowMsg("Plan name is too long", true);
                } else
                {
                    ShowMsg("Existing plan with the same name", true);
                }

            }
            else
            {
                ShowMsg("Error entering a tag name", true);
            }
            
        }


    }

    protected override void checkServerCacheCustom() {

        // checks for AI Plan results
        if (RestWebService.aiPlannerResultQueuePlan != null)
        {
            string planData = JsonConvert.SerializeObject(RestWebService.aiPlannerResultQueuePlan);
            Plan sentPlan = JsonConvert.DeserializeObject<Plan>(planData);
            fromPlan(sentPlan);
            RestWebService.aiPlannerResultQueuePlan = null;
            Capture.Log("PathAgentResult;" + JsonConvert.SerializeObject(plan), Capture.PLANNER);
        }


    }

    protected override void setPlanFromPlan(Plan sentPlan){}

}


