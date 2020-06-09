using System.Collections.Generic;
using UnityEngine;
using Assets;
using PlannerAssets;
using DataObjects;
using System;
using TMPro;
using UnityEngine.UI;
using PlanToolHelpers;

/// <summary>
/// 
/// This is the main file for the business planner code. 
/// It extends the BaseDeliveryInterface class.
/// 
/// </summary>
public class BusinessPlanInterface : BaseDeliveryInterface
{

    /// <summary>
    /// toggle the billboard view with selected plan metrics
    /// </summary>
    protected bool dashboardView = true;

    /// <summary>
    /// toggle the Play button if two plans are selected
    /// </summary>
    protected bool playEnabled = false;

    /// <summary>
    /// stores the winning plan
    /// </summary>
    protected Plan winningPlayPlan = null;

    /// <summary>
    /// stores the losing plan
    /// </summary>
    protected Plan losingPlayPlan = null;

    /// <summary>
    /// stores the business load current market rectangular box for tooltip display 
    /// </summary>
    protected Rect businessLoadCurrentMarketRect = new Rect(120, 10, 28, 28);

    /// <summary>
    /// stores the Play button rectangular box for tooltip display 
    /// </summary>
    protected Rect playRect;

    /// <summary>
    /// stores the Select Plan rectangular box for tooltip display 
    /// </summary>
    protected Rect selectPlanRect;

    /// <summary>
    /// stores the Submit scenario rectangular box for tooltip display 
    /// </summary>
    protected Rect submitScenarioRect = new Rect(10, 52, 100, 25);

    /// <summary>
    /// stores the Dashboard View rectangular box for tooltip display 
    /// </summary>
    protected Rect dashboardRect;

    /// <summary>
    /// stores the plan list of the right panel rectangular box for tooltip display 
    /// </summary>
    protected Rect businessPlansRect;

    /// <summary>
    /// toggles if a final plan has been submitted in the session
    /// </summary>
    protected bool businessFinalPlanSelected = false;

    /// <summary>
    /// 
    /// GUI controls for the business view
    /// 
    /// </summary>
    protected override void OnGUICustom()
    {

        Rect rect = Camera.main.pixelRect;

        // add toolbar button to load the current market setup
        if (GUI.Button(businessLoadCurrentMarketRect, new GUIContent(GUIHelpers.resetdesignimage, "Load Current Market")))
        {
            RemovePaths();
            DataInterface.GetScenario();

            Capture.Log("LoadCurrentScenario", Capture.BUSINESS);
            playClick();
        }

        // submit market button
        if (GUI.Button(submitScenarioRect, new GUIContent("Submit", "Submit the Selected Customers to Your Planners")))
        {
            GUIAssets.PopupButton.popupPanelID = "SubmitBusiness";
            GUIAssets.PopupButton.showing = true;
            GameObject.Find("SubmitBusiness").GetComponent<Canvas>().enabled = true;
        }

        // button for the information panel
        if (GUI.Button(infoRect, new GUIContent(GUIHelpers.infoimage, "Show Information Panel")))
        {
            showHelpInfoPanel = !showHelpInfoPanel;
            playClick();
            GameObject.Find("helpcanvasbusiness").GetComponent<Canvas>().enabled = showHelpInfoPanel;
            Capture.Log("ToggleInfoPanel:" + showHelpInfoPanel, business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // add the plan list on the right side panel
        int counter = 0;
        int scrollheight = (int)Math.Max(rect.height - 250, 200);
        businessPlansRect = new Rect(rect.width - 180, 10, 170, scrollheight);
        GUI.Box(businessPlansRect, new GUIContent("    Team Plans", "Select a Plan To Open"));
        scrollPositionOperationplans = GUI.BeginScrollView(new Rect(rect.width - 174, 40, 164, scrollheight), scrollPositionOperationplans, new Rect(0, 0, 140, 20 + loadedPlans.Count * 21));

        // for each plan add a toggle button and a button to load
        foreach (int id in loadedPlans.Keys)
        {

            // adds a toggle button if the plan is fully loaded
            bool loaded = loadedPlans[id].plan != null;
            string tag = (!loaded ? "* " : "") + loadedPlans[id].tag;
            bool val = false;
            if (loaded)
                val = GUI.Toggle(new Rect(0, 20 + 20 * counter, 24, 20), loadedPlans[id].selected, new GUIContent("", "Toggle in Dashboard"));

            // if there is a change in plan selection
            if ((val && !loadedPlans[id].selected) || (!val && loadedPlans[id].selected)) {

                RemovePlayIcons();

                loadedPlans[id].selected = val;
                if (!loadedPlans[id].selected)
                    ShowMsg("Plan unselected : " + tag, false);

                // code for a selected plan, probably do not need the loaded check, but kept it in
                if (loadedPlans[id].selected && loaded)
                {

                    // calculate the plan metrics
                    PlanCalculation calculation = new PlanCalculation(loadedPlans[id].plan, scaleSceneFactor, business);
                    calculation.calculate();

                    // add plan data to dashboard
                    DashboardData.PlanData planData = new DashboardData.PlanData();
                    planData.tag = tag;
                    planData.profit = calculation.getProfit();
                    planData.operatingCost = calculation.getOperatingCost();
                    planData.startupCost = calculation.getStartupCost();
                    planData.deliveries = calculation.getCustomers(); ;
                    planData.massDelivered = calculation.GetTotalWeightDelivered();
                    planData.parcelMass = calculation.getTotalParcelDelivered();
                    planData.foodMass = calculation.getTotalFoodDelivered();
                    DashboardData.addPlanData(loadedPlans[id].plan, planData);

                    ShowMsg("Plan selected : " + tag, false);

                }

                // hide or show the Play button if two plans are selected
                int selectedPlans = 0;
                foreach (int idkey in loadedPlans.Keys)
                    if (loadedPlans[idkey].selected)
                        selectedPlans += 1;
                playEnabled = (selectedPlans == 2);

                Capture.Log("SelectedPlan;" + tag + ";" + loadedPlans[id].selected, Capture.BUSINESS);
                playClick();

            }
            GUI.color = Color.white;


            // add GUI button to open a plan
            GUI.color = loaded ? Color.white : Color.gray;
            if (loaded)
            {
                // check for play color results
                if (loadedPlans[id].plan != null)
                {
                    if (loadedPlans[id].plan.Equals(winningPlayPlan))
                        GUI.color = Color.green;
                    else if (loadedPlans[id].plan.Equals(losingPlayPlan))
                        GUI.color = Color.red;
                }
            }

            // button to open the plan
            if (GUI.Button(new Rect(20, 20 + 20 * counter, 120, 20), tag))
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

        // add Play button
        if (playEnabled)
        {
            playRect = new Rect(rect.width - 120, scrollheight + 40, 80, 25);
            if (GUI.Button(playRect, new GUIContent("Play", "Play Two Selected Plans Against Each Other")))
            {
                Capture.Log("Play", business ? Capture.BUSINESS : Capture.PLANNER);
                play();
                playEnabled = false;
            }
        }

        // select the final plan button at the button left corner of the display if not already selected
        selectPlanRect = new Rect(160, rect.height - 50, 84, 24);
        if (!businessFinalPlanSelected)
            if (GUI.Button(selectPlanRect, new GUIContent("Select Plan", "Select Final Plan for Your Team")))
            {

                // make sure that there are customers in a delivery plan, this should alway 
                // have more than 0 customers
                int totalCustomers = 0;
                foreach (DataObjects.VehicleDelivery p in plan.paths)
                    totalCustomers += p.customers.Count;

                // make sure a plan with the correct market is selected
                bool correctMarket = false;
                try
                {
                    correctMarket = (RestWebService.market == plan.scenario.customers[0].market);
                } catch (Exception e)
                {
                    Debug.Log(e);
                }

                // a valid plan
                if (totalCustomers > 0 && correctMarket)
                {
                    GUIAssets.PopupButton.showing = true;
                    GUIAssets.PopupButton.popupPanelID = "SubmitPlan";
                    GameObject.Find("SubmitPlan").GetComponent<Canvas>().enabled = true;
                }
                else
                {
                    GameObject.Find("popup").GetComponent<Canvas>().enabled = true;
                    GameObject.Find("popupsuccess").GetComponent<Image>().enabled = true;
                    GameObject.Find("popuperror").GetComponent<Image>().enabled = true;
                    GameObject.Find("popuptext").GetComponent<TextMeshProUGUI>().text = !correctMarket ? "Plan Does Not Use the Current Market" : "Empty Plan";
                }

            }


        // toggle button for the dashboard
        dashboardRect = new Rect(rect.width - 200, rect.height - 38, 28, 28);
        if (GUI.Button(dashboardRect, new GUIContent(GUIHelpers.dashboardimage, "Toggle The Dashboard View")))
        {
            dashboardView = !dashboardView;
            playClick();
            Capture.Log("DashboardView;" + dashboardView, business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // add dashboard
        if (dashboardView)
        {

            // get selected plans
            List<Plan> selectedPlans = new List<Plan>();
            foreach (int id in loadedPlans.Keys)
                if (loadedPlans[id].selected)
                    selectedPlans.Add(loadedPlans[id].plan);

            // set billboard sizing variables
            float dashboardheight = Math.Min(100 + 20 * selectedPlans.Count, 200);
            float height = Math.Min((dashboardheight - 100) / selectedPlans.Count, 20);
            float dashboardwidth = Math.Max(3.0f * (rect.width - 454f) / 4.0f, 600);
            float width = Math.Max((dashboardwidth - 60) / 8, 10);
            float dashboardx = rect.width - 200 - dashboardwidth;
            float labelx = dashboardx + 100;

            // add the outer box
            GUI.Box(new Rect(dashboardx, rect.height - dashboardheight - 10, dashboardwidth, dashboardheight), "");
            if (selectedPlans.Count > 0)
            {

                // add labels for each metric
                GUI.Label(new Rect(labelx, rect.height - dashboardheight + 20, width, 20), "Profit($)");
                GUI.Label(new Rect(labelx + 1 * (width + 4), rect.height - dashboardheight + 6, width, 40), "Oper\nCost($)");
                GUI.Label(new Rect(labelx + 2 * (width + 4), rect.height - dashboardheight + 6, width, 40), "StartUp\nCost($)");
                GUI.Label(new Rect(labelx + 3 * (width + 4), rect.height - dashboardheight + 20, width, 20), "Deliveries");
                GUI.Label(new Rect(labelx + 4 * (width + 4), rect.height - dashboardheight + 20, width, 20), "Mass(lb)");
                GUI.Label(new Rect(labelx + 5 * (width + 4), rect.height - dashboardheight + 20, width, 20), "Parcel(lb)");
                GUI.Label(new Rect(labelx + 6 * (width + 4), rect.height - dashboardheight + 20, width, 20), "Food(lb)");

                GUI.Label(new Rect(labelx, rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxProfit);
                GUI.Label(new Rect(labelx + 1 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxOperatingCost);
                GUI.Label(new Rect(labelx + 2 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxstartupCost);
                GUI.Label(new Rect(labelx + 3 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxDeliveries);
                GUI.Label(new Rect(labelx + 4 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxMassDelivered);
                GUI.Label(new Rect(labelx + 5 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxParcelMass);
                GUI.Label(new Rect(labelx + 6 * (width + 4), rect.height - dashboardheight + 40, width, 20), "Max:" + DashboardData.maxFoodMass);

                // add plan data
                for (int i = 0; i < selectedPlans.Count; i++)
                {
                    // get plan metric data
                    DashboardData.PlanData p = DashboardData.dashboardData[selectedPlans[i]];

                    // get y offset
                    float yOffset = 0;
                    if (height < 20)
                        yOffset = (20 - height) / 2;

                    // add the plan label, shorten the name if long
                    GUI.skin.label.alignment = TextAnchor.MiddleRight;
                    string planLabel = p.tag;
                    if (p.tag.Length > 14)
                        planLabel = p.tag.Substring(0, 14);
                    GUI.Label(new Rect(dashboardx, rect.height - dashboardheight + 60 + i * (height + 2) - yOffset, 90, 20), planLabel);
                    GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                    // if there are play results, then shade based on winning and losing colors
                    GUIStyle boxStyle = Assets.GUIHelpers.lightgrayStyle;
                    if (selectedPlans[i].Equals(winningPlayPlan))
                        boxStyle = Assets.GUIHelpers.darkgreenStyle;
                    else if (selectedPlans[i].Equals(losingPlayPlan))
                        boxStyle = Assets.GUIHelpers.darkredStyle;

                    // add metric bars to display plan performance
                    GUI.Box(new Rect(labelx, rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.profit - DashboardData.minProfit) / (DashboardData.maxProfit - DashboardData.minProfit), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 1 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.operatingCost - DashboardData.minOperatingCost) / (DashboardData.maxOperatingCost - DashboardData.minOperatingCost), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 2 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.startupCost - DashboardData.minstartupCost) / (DashboardData.maxstartupCost - DashboardData.minstartupCost), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 3 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.deliveries - DashboardData.minDeliveries) / (DashboardData.maxDeliveries - DashboardData.minDeliveries), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 4 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.massDelivered - DashboardData.minMassDelivered) / (DashboardData.maxMassDelivered - DashboardData.minMassDelivered), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 5 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.parcelMass - DashboardData.minParcelMass) / (DashboardData.maxParcelMass - DashboardData.minParcelMass), height), "", boxStyle);
                    GUI.Box(new Rect(labelx + 6 * (width + 4), rect.height - dashboardheight + 60 + i * (height + 2), 2 + (width - 2) * (p.foodMass - DashboardData.minFoodMass) / (DashboardData.maxFoodMass - DashboardData.minFoodMass), height), "", boxStyle);

                }
            }
        }

    }

    protected override void tooltipDisplay()
    {
        if (businessLoadCurrentMarketRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(businessLoadCurrentMarketRect.xMax - 10, businessLoadCurrentMarketRect.yMax + 2, 400, businessLoadCurrentMarketRect.height);
        if (playRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(playRect.xMin - 200, playRect.yMax + 2, 400, playRect.height);
        if (selectPlanRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(selectPlanRect.xMax, selectPlanRect.yMax, 400, selectPlanRect.height);
        if (submitScenarioRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(submitScenarioRect.xMax, submitScenarioRect.yMax, 400, submitScenarioRect.height);
        if (dashboardRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(dashboardRect.xMin, dashboardRect.yMin - dashboardRect.height, 400, dashboardRect.height);
        if (businessPlansRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(businessPlansRect.xMin - 140, 100, 400, 40);
    }

    /// <summary>
    /// 
    /// open a plan without adding a popup since the users do not edit plans
    /// 
    /// </summary>
    /// <param name="planopen">Plan object to open</param>
    protected override void OpenPlan(Plan planopen)
    {
        fromPlan(planopen);
        ShowMsg("Plan opened: " + plan.tag, false);
        playClick();
        Capture.Log("Opened;" + planopen.tag, business ? Capture.BUSINESS : Capture.PLANNER);
    }

    /// <summary>
    /// 
    /// remove all play result icons
    /// 
    /// </summary>
    private void RemovePlayIcons()
    {
        winningPlayPlan = null;
        losingPlayPlan = null;

        GameObject[] objs = GameObject.FindGameObjectsWithTag("playicons");
        foreach (GameObject obj in objs)
            Destroy(obj);

    }

    /// <summary>
    /// 
    /// Update method for the business view
    /// 
    /// </summary>
    protected override void customUpdate()
    {

        // mouse listener for including or excluding locations in the business plan
        if (Input.GetMouseButtonDown(0))
        {

            object[] selectedCustomer = customerAtMousePosition();
            if (selectedCustomer != null) {

                GameObject selectedObj = (GameObject)selectedCustomer[0];
                Customer customer = (Customer)selectedCustomer[1];

                customer.selected = !customer.selected;
                if (selectedObj.transform.parent != null)
                    selectedObj = selectedObj.transform.parent.gameObject;

                ToggleHouseSelection(selectedObj, customer);

                playClick();

                Capture.Log("SelectNode;" + customer.id + ";" + customer.selected, business ? Capture.BUSINESS : Capture.PLANNER);

            }

        }

        // mouse over for customers in the business view
        if ((!Input.GetMouseButton(0) && !Input.GetMouseButton(1)))
        {
            object[] highlighted = customerAtMousePosition();
            HideHouseLabels();

            // no house is highlighted
            if (highlighted == null)
            {
                GameObject.Find("customerhighlight").transform.position = new Vector3(1000, 1000, 1000);
            }
            else
            {
                GameObject obj = (GameObject)highlighted[0];
                Customer customer = (Customer)highlighted[1];
                Vector3 pos = obj.transform.position;
                Vector3 scale = obj.transform.localScale;
                GameObject.Find("customerhighlight").transform.position = new Vector3(pos.x, pos.y + 0.0f, !orthogonalView ? pos.z : pos.z + 0.2f);
                GameObject.Find("customerhighlight").transform.localScale = new Vector3(1 + customer.weight / 4f, scale.y, 1 + customer.weight / 4f);
                ShowHouseLabel(customer);
            }

        }

    }

    /// <summary>
    /// 
    /// play two selected plans against each other
    /// 
    /// </summary>
    public void play()
    {

        // get the two selected plans 
        List<Plan> selectedplans = new List<Plan>();
        foreach (int id in loadedPlans.Keys)
        {
            if (loadedPlans[id].selected)
            {
                selectedplans.Add(loadedPlans[id].plan);
            }
        }

        // just a double check but this should be true
        if (selectedplans.Count == 2)
        {
            play(selectedplans[0], selectedplans[1]);
            playClick();
        }

    }


    /// <summary>
    /// 
    /// plays two plans against each other , if a plan path reaches a location first,
    /// that plan gets the profit for that customer
    /// 
    /// </summary>
    /// <param name="firstPlan"></param>
    /// <param name="secondPlan"></param>
    /// <returns></returns>
    private float play(Plan firstPlan, Plan secondPlan)
    {

        Dictionary<string, PlayResults> playResults = new Dictionary<string, PlayResults>();
        int dup = 0;
        int n = 0;

        // get all delivery times for the first plan by location
        foreach (DataObjects.VehicleDelivery path in firstPlan.paths)
            foreach (CustomerDelivery wayPoint in path.customers)
                if (wayPoint.deliverytime > 0)
                {

                    // using locations and not ids, since maybe in the future we want to compare 
                    // plans with different scenarios
                    string id = wayPoint.address.x.ToString("0.0") + ":" + wayPoint.address.z.ToString("0.0");
                    if (!playResults.ContainsKey(id))
                    {
                        playResults[id] = new PlayResults();
                        playResults[id].deliveryType = wayPoint.payload;
                        playResults[id].weight = wayPoint.weight;
                        playResults[id].x = wayPoint.address.x;
                        playResults[id].z = wayPoint.address.z;
                        n += 1;
                    }
                    else
                    {
                        dup += 1;
                    }

                    // set the delivery time for the first plan
                    playResults[id].time = wayPoint.deliverytime;

                }

        // get all delivery times for the second plan by location
        foreach (DataObjects.VehicleDelivery path in secondPlan.paths)
            foreach (CustomerDelivery wayPoint in path.customers)
                if (wayPoint.deliverytime > 0)
                {

                    string id = wayPoint.address.x.ToString("0.0") + ":" + wayPoint.address.z.ToString("0.0");
                    if (!playResults.ContainsKey(id))
                    {

                        playResults[id] = new PlayResults();
                        playResults[id].deliveryType = wayPoint.payload;
                        playResults[id].weight = wayPoint.weight;
                        playResults[id].x = wayPoint.address.x;
                        playResults[id].z = wayPoint.address.z;
                        n += 1;
                    }
                    else
                    {
                        dup += 1;
                    }
                    playResults[id].timeAdversary = wayPoint.deliverytime;
                }

        // calculate the score
        float score = 0;
        List<string> winningNodes = new List<string>();
        List<string> losingNodes = new List<string>();
        List<string> tieNodes = new List<string>();

        // add winning and losing node ids
        // score is incremented with 200*foodweight or 100*parcelweight
        foreach (string key in playResults.Keys)
        {

            if (playResults[key].time > 0 && playResults[key].timeAdversary == -1)
            {
                score += playResults[key].weight * (playResults[key].deliveryType.StartsWith("food") ? 200 : 100);
                winningNodes.Add(key);
            }
            else if (playResults[key].time == -1 && playResults[key].timeAdversary > 0)
            {
                score -= playResults[key].weight * (playResults[key].deliveryType.StartsWith("food") ? 200 : 100);
                losingNodes.Add(key);
            }
            else if (System.Math.Abs(playResults[key].timeAdversary - playResults[key].time) < 0.001f)
            {
                tieNodes.Add(key);
            }
            else if (playResults[key].timeAdversary == -1f && playResults[key].time == -1f)
            {
                tieNodes.Add(key);
            }
            else if (playResults[key].time < playResults[key].timeAdversary)
            {
                score += playResults[key].weight * (playResults[key].deliveryType.StartsWith("food") ? 200 : 100);
                winningNodes.Add(key);
            }
            else if (playResults[key].timeAdversary < playResults[key].time)
            {
                score -= playResults[key].weight * (playResults[key].deliveryType.StartsWith("food") ? 200 : 100);
                losingNodes.Add(key);
            }
        }

        if (score > 0)
        {
            fromPlan(firstPlan);
            SetHouseAndLabelDisplay();
            winningPlayPlan = firstPlan;
            losingPlayPlan = secondPlan;
            Capture.Log("PlayResult;Win=" + firstPlan.tag + ";Lost=" + secondPlan.tag, business ? Capture.BUSINESS : Capture.PLANNER);
            ShowMsg(firstPlan.tag + " wins based on profit", false);
        }
        else if (score < 0)
        {
            fromPlan(secondPlan);
            SetHouseAndLabelDisplay();
            winningPlayPlan = secondPlan;
            losingPlayPlan = firstPlan;
            Capture.Log("PlayResult;Win=" + secondPlan.tag + ";Lost=" + firstPlan.tag, business ? Capture.BUSINESS : Capture.PLANNER);

            List<string> temp = new List<string>(winningNodes);
            winningNodes = losingNodes;
            losingNodes = temp;
            ShowMsg(secondPlan.tag + " wins based on profit", false);

        } else
        {
            ShowMsg("Plans tied in score", false);
            Capture.Log("PlayResult;Tie=" + firstPlan.tag + ";Tie=" + secondPlan.tag, business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // remove all play icons
        GameObject[] objs = GameObject.FindGameObjectsWithTag("playicons");
        foreach (GameObject obj in objs)
            Destroy(obj);

        // show winning nodes
        GameObject winningnode = GameObject.Find("winningnode");
        foreach (string node in winningNodes)
        {
            string[] tokens = node.Split(':');
            GameObject cube = Instantiate(winningnode, ScaleInScene(new Vector3(float.Parse(tokens[0]),
                4.0f, float.Parse(tokens[1]))), Quaternion.identity);
            cube.tag = "playicons";
        }

        // show losing nodes
        GameObject losingnode = GameObject.Find("losingnode");
        foreach (string node in losingNodes)
        {
            string[] tokens = node.Split(':');
            GameObject cube = Instantiate(losingnode, ScaleInScene(new Vector3(float.Parse(tokens[0]),
                4.0f, float.Parse(tokens[1]))), Quaternion.identity);
            cube.tag = "playicons";
        }

        return score;

    }

    /// <summary>
    /// 
    /// Play results
    /// 
    /// </summary>
    private class PlayResults
    {

        public PlayResults() { }

        /// <summary>
        /// time delivery of the first plan
        /// </summary>
        public float time = -1f;

        /// <summary>
        /// time delivery of the second plan
        /// </summary>
        public float timeAdversary = -1f;

        /// <summary>
        /// delivery type
        /// </summary>
        public string deliveryType = "";

        /// <summary>
        /// total weight for the delivery
        /// </summary>
        public float weight = -1f;

        /// <summary>
        /// x position
        /// </summary>
        public float x = -1f;

        /// <summary>
        /// z position
        /// </summary>
        public float z = -1f;

    }


    protected override void checkPopupCacheCustom()
    {

        // Business : submit a selected market 
        if (GUIAssets.PopupButton.submitScenario)
        {
            GUIAssets.PopupButton.submitScenario = false;
            scenario.tag = scenarioLevel + "";
            DataInterface.PostScenario(scenario);
            Capture.Log("SubmitScenario", business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // Business : submit a final plan
        if (GUIAssets.PopupButton.ok && GUIAssets.PopupButton.popupPanelID.StartsWith("SubmitPlan"))
        {
            GUIAssets.PopupButton.ok = false;
            GUIAssets.PopupButton.storedData = "";

            businessFinalPlanSelected = true;

            ShowMsg("Selected Final Plan for Your Team", false);
            Capture.Log("BusinessPlanSelected:" + plan.tag + ":" + plan.id, business ? Capture.BUSINESS : Capture.PLANNER);

        }

    }

    protected override void setPlanFromPlan(Plan sentPlan){
        scenario = sentPlan.scenario; 
    }

}


