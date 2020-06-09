using System.Collections.Generic;
using UnityEngine;
using Assets;
using PlannerAssets;
using DataObjects;
using System;
using TMPro;
using UnityEngine.UI;
using PlanToolHelpers;
using Newtonsoft.Json;

/// <summary>
/// 
/// This is the base file with common code for the Operational Planner and Business interfaces. 
/// 
/// Both the OpsPlanInterface and the BusinessPlanInterface extend this base class.
/// 
/// </summary>
public class BaseDeliveryInterface : MonoBehaviour
{

    /// <summary>
    /// boolean to track if this is the business interface (primarily for log data)
    /// </summary>
    protected bool business = Startup.isBusiness;

    /// <summary>
    /// scenario level 
    /// </summary>
    protected int scenarioLevel = Startup.scenarioLevel;

    /// <summary>
    /// stores the current plan in the view
    /// </summary>
    protected Plan plan = new Plan();

    /// <summary>
    /// stores the current scenario
    /// </summary>
    protected Scenario scenario = new Scenario();

    /// <summary>
    /// list stores all session team vehicles
    /// </summary>
    protected List<Vehicle> teamVehicles = new List<Vehicle>();

    /// <summary>
    /// GameObject used to draw paths
    /// </summary>
    protected GameObject cylinderPrefab;

    /// <summary>
    /// links Unity GameObjects of houses to a customer (for selection) 
    /// </summary>
    protected Dictionary<GameObject, Customer> customerIconsMap = new Dictionary<GameObject, Customer>();

    /// <summary>
    /// links Unity GameObjects of the connector to its two customers 
    /// </summary>
    protected Dictionary<GameObject, Customer[]> connectorMap = new Dictionary<GameObject, Customer[]>();

    /// <summary>
    /// links a customer to the above text label indicating its weight
    /// </summary>
    protected Dictionary<Customer, GameObject> weightLabelMap = new Dictionary<Customer, GameObject>();
    
    /// <summary>
    /// stores if the weight labels are displayed
    /// </summary>
    protected bool toggleWeightLabels = false;

    /// <summary>
    /// shows the overall plan metrics to the user in the bottom
    /// left corner of the interface
    /// </summary>
    protected string planMetricsStr = "";

    /// <summary>
    /// displays any debug or user actions at the bottom of the screen
    /// </summary>
    protected string debugStr = "";

    /// <summary>
    /// stores the results of the current plan calculation 
    /// </summary>
    protected PlanCalculation planCalculation;

    /// <summary>
    /// manually selected path, this is only set in the opsPlanInterface but
    /// kept it here for coding reasons
    /// </summary>
    protected int selectedPathIndex = -1;

    /// <summary>
    /// scale the positions of the customers, 1 is one to one on the unity scale and 
    /// higher values spread the customers further apart
    /// </summary>
    protected float scaleSceneFactor = 2.0f;

    /// <summary>
    /// stores all team plans, with integer plan ids as the key 
    /// </summary>
    protected Dictionary<int, StoredPlan> loadedPlans = new Dictionary<int, StoredPlan>();

    /// <summary>
    /// toggle orthogonal view
    /// </summary>
    public static bool orthogonalView = false;

    /// <summary>
    /// set the orthogonal zoom
    /// </summary>
    public static float orthoZoom = 16;

    /// <summary>
    /// store the scroll position of the right side operation plans list
    /// </summary>
    protected Vector2 scrollPositionOperationplans;

    /// <summary>
    /// toggle for click sounds
    /// </summary>
    protected bool soundsOn = true;

    /// <summary>
    /// toggle for music sounds
    /// </summary>
    protected bool musicPaused = true;

    /// <summary>
    /// shows the help panel
    /// </summary>
    protected bool showHelpInfoPanel = false;

    /// <summary>
    /// stores a user selected plan for popups
    /// </summary>
    protected Plan userSelectedPlan = null;

    /// <summary>
    /// stores the tooltip string to display
    /// </summary>
    protected string tooltip = " ";

    /// <summary>
    /// stores the tooltip position
    /// </summary>
    protected Rect tooltipRect = new Rect(0, 0, 65, 25);

    // tooltip locations
    protected Rect infoRect = new Rect(70, 10, 28, 28);

    /// <summary>
    /// rectangle for weight labels tooltip display
    /// </summary>
    protected Rect toggleWeightLabelsRect;

    /// <summary>
    /// rectangle for loading objects from the database tooltip display
    /// </summary>
    protected Rect databaseLoadRect;

    /// <summary>
    /// rectangle for perspective view tooltip display
    /// </summary>
    protected Rect perspectiveRect = new Rect(10, 10, 28, 28);

    /// <summary>
    /// rectangle for orthogonal view tooltip display
    /// </summary>
    protected Rect orthRect = new Rect(40, 10, 28, 28);

    /// <summary>
    /// integer for the current budget of the market
    /// </summary>
    protected int budget = 15000;

    /// <summary>
    /// bool if the current selected plan is overbudget
    /// </summary>
    protected bool overbudget = false;

    /// <summary>
    /// maximum vehicle cost stored in the team session
    /// </summary>
    protected double maxVehicleCost = 0;

    /// <summary>
    /// maximum vehicle range stored in the team session
    /// </summary>
    protected double maxVehicleRange = 0;

    /// <summary>
    /// maximum vehicle velocity stored in the team session
    /// </summary>
    protected double maxVehicleVelocity = 0;

    /// <summary>
    /// maximum vehicle capacity stored in the team session
    /// </summary>
    protected double maxVehicleCapacity = 0;

    /// <summary>
    /// maximum delivery weight in the market
    /// </summary>
    protected double maxDeliveryWeight = 0;

    /// <summary>
    /// Unity start method and initializes the scene
    /// </summary>
    void Start()
    {

        Capture.Log("StartSession", business ? Capture.BUSINESS : Capture.PLANNER);

        // load image icons
        GUIHelpers.LoadImageIcons();

        // get the market ID
        DataInterface.GetMarket();

        // set the base plate and grid
        SetupBasePlateAndScale();
        // change view
        ResetView();
        // load all database objects
        DatabaseLoadAllObjects();

    }


    /// <summary>
    /// 
    /// load all vehicles, scenarios, and short plans
    /// 
    /// </summary>
    public void DatabaseLoadAllObjects()
    {
        DataInterface.GetVehicles();
        DataInterface.GetScenario();
        DataInterface.GetPlanIds();
    }

    /// <summary>
    /// 
    /// adds the ground, grid, and houses to the scene. 
    /// 
    /// </summary>
    protected void SetupBasePlateAndScale()
    {

        // adds the ground 
        Vector3 scale = GameObject.Find("groundcube").transform.localScale;
        GameObject.Find("groundcube").transform.localScale = new Vector3((float)(scale.x * (scaleSceneFactor / 2.0)), scale.y, (float)(scale.z * (scaleSceneFactor / 2.0)));

        // adds the grid
        Vector3 basegridposition = GameObject.Find("gridbase").transform.position;
        Vector3 basegridscale = GameObject.Find("gridbase").transform.localScale;
        GameObject.Find("gridbase").transform.localScale = new Vector3(basegridscale.x, basegridscale.y, basegridscale.z * scaleSceneFactor / 2.0f);
        for (int i = -10; i <= 10; i++)
        {
            GameObject.Instantiate(GameObject.Find("gridbase"), new Vector3(basegridposition.x + i * scaleSceneFactor, basegridposition.y, basegridposition.z), Quaternion.identity);
        }
        for (int i = -10; i <= 10; i++)
        {
            GameObject basez = GameObject.Instantiate(GameObject.Find("gridbase"));
            basez.transform.Rotate(new Vector3(0, 1, 0), 90);
            basez.transform.position = new Vector3(basegridposition.x, basegridposition.y, basegridposition.z + i * scaleSceneFactor);
        }
        Vector3 baseposition = GameObject.Find("base").transform.position;
        GameObject.Find("base").transform.position = new Vector3(baseposition.x * scaleSceneFactor, baseposition.y, baseposition.z * scaleSceneFactor);

        // add the mile icon
        Vector3 milePosition = GameObject.Find("mile").transform.position;
        GameObject.Find("mile").transform.position = new Vector3(milePosition.x * scaleSceneFactor / 2f, milePosition.y, milePosition.z * scaleSceneFactor / 2f);
        GameObject.Find("mile").transform.localScale = new Vector3(scaleSceneFactor, 0.1f, 0.1f);

        // add the labels for the direction
        Vector3 northPosition = GameObject.Find("North").transform.position;
        GameObject.Find("North").transform.position = new Vector3(northPosition.x, northPosition.y, northPosition.z * scaleSceneFactor / 2f);
        Vector3 southPosition = GameObject.Find("South").transform.position;
        GameObject.Find("South").transform.position = new Vector3(southPosition.x, southPosition.y, southPosition.z * scaleSceneFactor / 2f);
        Vector3 eastPosition = GameObject.Find("East").transform.position;
        GameObject.Find("East").transform.position = new Vector3(eastPosition.x * scaleSceneFactor / 2f, eastPosition.y, eastPosition.z);
        Vector3 westPosition = GameObject.Find("West").transform.position;
        GameObject.Find("West").transform.position = new Vector3(westPosition.x * scaleSceneFactor / 2f, westPosition.y, westPosition.z);

        cylinderPrefab = GameObject.Find("Cylinder");

    }


    /// <summary>
    /// 
    /// removes all old houses and adds new ones
    /// 
    /// </summary>
    protected void AddHouses()
    {

        // check for a valid scenario
        if (scenario == null)
            return;
        if (scenario.customers == null)
            return;
        if (scenario.customers.Count == 0)
            return;

        // clear dictionaries
        customerIconsMap.Clear();
        weightLabelMap.Clear();

        // remove all old graphics objects of houses
        GameObject[] objs = GameObject.FindGameObjectsWithTag("buildings");
        foreach (GameObject obj in objs)
            Destroy(obj);

        // get Unity graphics objects
        GameObject house = GameObject.Find("house");
        GameObject housefood = GameObject.Find("housefood");
        GameObject payloadObject = GameObject.Find("payloadlabel");

        // for each customer
        foreach (Customer customer in scenario.customers)
        {

            // create a house object and assign to a customer
            GameObject cube = Instantiate(customer.payload.Contains("food") ? housefood : house, ScaleInScene(new Vector3(customer.address.x, 0.0f, customer.address.z)), Quaternion.identity);
            cube.tag = "buildings";
            customerIconsMap[cube] = customer;
            float sc = 0.5f + customer.weight / 5f;
            Vector3 houseScale = cube.transform.localScale;
            cube.transform.localScale = new Vector3(houseScale.x * sc, houseScale.y * sc, houseScale.z * sc);
            for (int j = 0; j < cube.transform.childCount; j++)
            {
                customerIconsMap[cube.transform.GetChild(j).gameObject] = customer;
                cube.transform.GetChild(j).gameObject.tag = "buildings";
            }
            // rotation of houses is handled in SetHouseAndLabelDisplay()

            // add payload text icon
            Vector3 position = ScaleInScene(new Vector3(customer.address.x, 2.0f, customer.address.z));
            GameObject weightTextIcon = Instantiate(payloadObject, position, Quaternion.identity) as GameObject;
            weightTextIcon.GetComponent<TextMesh>().text = customer.weight + "";
            weightTextIcon.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
            weightTextIcon.GetComponent<TextMesh>().alignment = TextAlignment.Center;
            weightTextIcon.tag = "buildings";
            weightTextIcon.GetComponent<MeshRenderer>().enabled = false;

            // add both to the maps
            weightLabelMap[customer] = weightTextIcon;

            // select or deselect 
            ToggleHouseSelection(cube, customer);

        }

    }

    /// <summary>
    /// 
    /// add delivery paths for each selected vehicle to the scene
    /// 
    /// </summary>
    protected void RefreshPaths()
    {

        connectorMap.Clear();

        // remove all paths
        GameObject[] objs = GameObject.FindGameObjectsWithTag("link");
        foreach (GameObject go in objs)     
            Destroy(go);
        
        // remove play icons for the business view (probably should move this to the
        // business interface code)
        objs = GameObject.FindGameObjectsWithTag("playicons");
        foreach (GameObject go in objs)      
            Destroy(go);     

        // for all paths
        for (int i = 0; i < plan.paths.Count; i++)
        {

            // thicken selected path
            float scalePath = 0.12f;
            if (selectedPathIndex == i)          
                scalePath = 0.40f;
            

            // for each path segment not starting or ending at the warehouse
            for (int j = 1; j < plan.paths[i].customers.Count; j++)
            {
                Address addressA = plan.paths[i].customers[j - 1].address;
                Address addressB = plan.paths[i].customers[j].address;
                GameObject obj = CreateCylinderBetweenPoints(ScaleInScene(new Vector3(addressA.x, 1, addressA.z)), ScaleInScene(new Vector3(addressB.x, 1, addressB.z)), scalePath, i == selectedPathIndex ? Color.cyan : Color.white, plan.paths[i].customers[j].deliverytime, "link");
                connectorMap[obj] = new Customer[] { plan.paths[i].customers[j - 1], plan.paths[i].customers[j] };
            }

            // get paths to and from warehouse
            if (plan.paths[i].customers.Count > 0)
            {
                Address addressB = plan.paths[i].customers[0].address;
                GameObject obj = CreateCylinderBetweenPoints(ScaleInScene(new Vector3(plan.paths[i].warehouse.address.x, 1, plan.paths[i].warehouse.address.z)), ScaleInScene(new Vector3(addressB.x, 1, addressB.z)), scalePath, i == selectedPathIndex ? Color.cyan : Color.white, plan.paths[i].customers[0].deliverytime, "link");
                connectorMap[obj] = new Customer[] { null, plan.paths[i].customers[0] };
                Address addressA = plan.paths[i].customers[plan.paths[i].customers.Count - 1].address;
                obj = CreateCylinderBetweenPoints(ScaleInScene(new Vector3(addressA.x, 1, addressA.z)), ScaleInScene(new Vector3(plan.paths[i].warehouse.address.x, 1, plan.paths[i].warehouse.address.z)), scalePath, i == selectedPathIndex ? Color.cyan : Color.white, 0.0f, "link");
                connectorMap[obj] = new Customer[] { plan.paths[i].customers[plan.paths[i].customers.Count - 1], null };
            }
        }

        // Calculates the resulting manual or AI based solutions. The ortools returns results 
        // where if there is a time window, it says the the travel time is from 0 to 4, instead of 3.5 to 
        // 4, which results in incorrect calculations, so just use the below to get the correct metrics
        planCalculation = new PlanCalculation(plan, scaleSceneFactor, business);
        planCalculation.calculate();
        planMetricsStr = planCalculation.getInfoString();
        overbudget = planCalculation.getStartupCost() > budget;
        Capture.Log("PathMetrics;" + planCalculation.getLogString(), business ? Capture.BUSINESS : Capture.PLANNER);

    }

    /// <summary>
    /// 
    /// creates a connection between two locations
    /// 
    /// </summary>
    /// <param name="start">starting position of the connection</param>
    /// <param name="end">ending position of the connection</param>
    /// <param name="width">width of the connection</param>
    /// <param name="color">color of the connection</param>
    /// <param name="time_delivery">time of the delivery</param>
    protected GameObject CreateCylinderBetweenPoints(Vector3 start, Vector3 end, 
        float width, Color color, float time_delivery, string tag)
    {

        var offset = end - start;
        var scale = new Vector3(width, offset.magnitude / 2.0f, width);
        var position = start + (offset / 2.0f);

        // create the connection game object
        GameObject cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
        cylinder.transform.up = offset;
        cylinder.transform.localScale = scale;
        cylinder.tag = tag;

        cylinder.GetComponent<MeshRenderer>().material.color = color;
        cylinder.GetComponent<CapsuleCollider>().enabled = tag.Equals("link");

        // add a small sphere above each customer
        float sphereScale = 0.24f + 0.2f * time_delivery / 24f;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = end;
        sphere.transform.localScale = new Vector3(sphereScale, sphereScale, sphereScale);
        sphere.tag = tag;
        sphere.GetComponent<SphereCollider>().enabled = false;

        sphere.GetComponent<MeshRenderer>().material.color = color;

        return cylinder;

    }

    /// <summary>
    /// 
    /// scale objects in the scene only in the x and z dimension
    /// 
    /// </summary>
    /// <param name="pos">original position</param>
    /// <returns>scaled position in the scene</returns>
    protected Vector3 ScaleInScene(Vector3 pos)
    {
        return new Vector3(pos.x * scaleSceneFactor, pos.y, pos.z * scaleSceneFactor);
    }
    
    /// <summary>
    /// method used by the client interface to autoupdate team objects
    /// </summary>
    public void updatePlansAndDesigns()
    {
        DatabaseLoadAllObjects();
        playClick();
        Capture.Log("Auto LoadDatabase", business ? Capture.BUSINESS : Capture.PLANNER);
    }

    /// <summary>
    /// 
    /// GUI controls for both the planner and business view
    /// 
    /// </summary>
    protected void OnGUIAll()
    {

        Assets.GUIHelpers.InitStyles();

        // right panel plan load list button, this is not needed since auto update is used now
        // but we will keep it in
        Rect rect = Camera.main.pixelRect;
        databaseLoadRect = new Rect(rect.width - 180, 10, 24, 24);
        if (GUI.Button(databaseLoadRect, new GUIContent(GUIHelpers.dbloadimage, "Load Plans from Your Team : Load time dependent on number of plans")))
        {
            DatabaseLoadAllObjects();
            playClick();
            Capture.Log("LoadDatabase", business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // right panel plan load list
        GUI.Label(new Rect(10, rect.height - 20, 300, 20), debugStr);
        GUI.color = Color.white;
        GUI.Box(new Rect(10, rect.height - 210, 272, 188), "Plan Metrics");
        GUI.Label(new Rect(20, rect.height - 190, 232, 160), "" + planMetricsStr);

        // if over budget, show a warning label
        if (overbudget)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(168, rect.height - 154, 200, 40), "Over Budget");
            GUI.color = Color.white;
        }

        // toolbar items
        // button to toggle perspective view
        if (GUI.Button(perspectiveRect, new GUIContent(GUIHelpers.toggleperspectiveimage, "3D View")))
        {
            orthogonalView = false;
            SetHouseAndLabelDisplay();
            ResetView();
            playClick();
            Capture.Log("PerspectiveCamera", business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // button to toggle orthogonal view
        if (GUI.Button(orthRect, new GUIContent(GUIHelpers.toggleviewimage, "2D Top Down View")))
        {
            orthoZoom = 16;
            orthogonalView = true;
            SetHouseAndLabelDisplay();
            ResetView();
            playClick();
            Capture.Log("OrthogonalCamera", business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // toggle weight labels button on the lower right area of the screen
        toggleWeightLabelsRect = new Rect(rect.width - 184, rect.height - 146, 28, 28);
        if (GUI.Button(toggleWeightLabelsRect, new GUIContent(GUIHelpers.tagimage, "Toggle Weight Labels")))
        {
            toggleWeightLabels = !toggleWeightLabels;
            SetHouseAndLabelDisplay();
            playClick();
            Capture.Log("ToggleWeightIcons:" + toggleWeightLabels, business ? Capture.BUSINESS : Capture.PLANNER);
        }

        // rotate weight labels above houses in the scene
        AlignAllText();

    }

    /// <summary>
    /// 
    /// rotates text to face the camera, alignText continually rotates to camera
    /// and flips text at a 90 degree angle
    /// 
    /// </summary>
    protected void AlignAllText()
    {
        if (!orthogonalView)
            foreach (Customer obj in weightLabelMap.Keys)
               Assets.GUIHelpers.alignText(weightLabelMap[obj].GetComponent<TextMesh>());                
        Assets.GUIHelpers.flipText(GameObject.Find("milelabel").GetComponent<TextMesh>());
    }

    /// <summary>
    /// 
    /// shows success message overlay
    /// 
    /// </summary>
    /// <param name="str">string to display</param>
    /// <param name="error">the message is an error</param>
    protected void ShowMsg(string str, bool error)
    {
        GameObject.Find("popup").GetComponent<Canvas>().enabled = true;
        GameObject.Find("popupsuccess").GetComponent<Image>().enabled = true;
        GameObject.Find("popuperror").GetComponent<Image>().enabled = error;
        GameObject.Find("popuptext").GetComponent<TextMeshProUGUI>().text = str;
    }

    /// <summary>
    /// 
    /// user selects to open a plan.
    /// 
    /// in business view, open the plan since the users do not edit plans.
    /// 
    /// in planner view, open a prompt to confirm that changes will be lost (based on MITRE feedback).
    /// 
    /// </summary>
    /// <param name="planopen">plan to open</param>
    protected virtual void OpenPlan(Plan planopen) { }

    /// <summary>
    /// 
    /// make sure houses and weight labels are rotated correctly for 3D and orthogonal view
    /// 
    /// </summary>
    protected void SetHouseAndLabelDisplay()
    {

        Camera.main.orthographic = orthogonalView ? true : false;
        Camera.main.orthographicSize = orthoZoom;
        GameObject.Find("milelabelorth").gameObject.GetComponent<MeshRenderer>().enabled = orthogonalView;
        GameObject.Find("groundcube").gameObject.GetComponent<MeshRenderer>().enabled = !orthogonalView;

        // house icons
        foreach (GameObject obj in customerIconsMap.Keys)
            obj.transform.rotation = orthogonalView ? new Quaternion(0.707f, 0.0f, 0.0f, 0.707f) : Quaternion.identity;

        // weight labels
        foreach (Customer customer in weightLabelMap.Keys)
        {
            GameObject gameObj = weightLabelMap[customer];
            gameObj.transform.rotation = orthogonalView ? new Quaternion(0.707f, 0.0f, 0.0f, 0.707f) : Quaternion.identity;
            Vector3 pos = ScaleInScene(new Vector3(customer.address.x, 0f, customer.address.z));
            gameObj.transform.position = orthogonalView ? new Vector3(pos.x + 0.38f, 1.0f, pos.z - 0.1f) : new Vector3(pos.x, 2.0f, pos.z);
            gameObj.GetComponent<MeshRenderer>().enabled = customer.selected && toggleWeightLabels;
        }

    }

    /// <summary>
    /// hide the house weight labels 
    /// </summary>
    protected void HideHouseLabels()
    {
        if(!toggleWeightLabels)
            foreach (Customer customer in weightLabelMap.Keys)
            {
                GameObject gameObj = weightLabelMap[customer];
                gameObj.GetComponent<MeshRenderer>().enabled = false;
            }
    }

    /// <summary>
    /// shows the house weight labels 
    /// </summary>
    protected void ShowHouseLabel(Customer customer)
    {
        GameObject gameObj = weightLabelMap[customer];
        gameObj.transform.rotation = orthogonalView ? new Quaternion(0.707f, 0.0f, 0.0f, 0.707f) : Quaternion.identity;
        Vector3 pos = ScaleInScene(new Vector3(customer.address.x, 0f, customer.address.z));
        gameObj.transform.position = orthogonalView ? new Vector3(pos.x + 0.38f, 1.0f, pos.z - 0.1f) : new Vector3(pos.x, 2.0f, pos.z);
        gameObj.GetComponent<MeshRenderer>().enabled = customer.selected && toggleWeightLabels;
        if(!orthogonalView)
            Assets.GUIHelpers.alignText(gameObj.GetComponent<TextMesh>());
    }

    /// <summary>
    /// Unity OnGUI call
    /// </summary>
    void OnGUI()
    {

        if (!GUIAssets.PopupButton.showing)
        {
            OnGUIAll();
            OnGUICustom();
        }

        // add popup tooltips
        if (Event.current.type == EventType.Repaint && GUI.tooltip != tooltip)
        {
            tooltip = GUI.tooltip;
            if (infoRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(infoRect.xMax - 10, infoRect.yMax - 2, 400, infoRect.height);
            if (toggleWeightLabelsRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(toggleWeightLabelsRect.xMax - 60, toggleWeightLabelsRect.yMax + 2, 400, toggleWeightLabelsRect.height);
            if (databaseLoadRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(databaseLoadRect.xMin - 160, databaseLoadRect.yMax, 200, 100);
            if (perspectiveRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(perspectiveRect.xMax - 10, perspectiveRect.yMax - 2, 400, perspectiveRect.height);
            if (orthRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(orthRect.xMax - 10, orthRect.yMax - 2, 400, orthRect.height);
            tooltipDisplay();
        }

        GUI.Label(tooltipRect, tooltip);

    }

    /// <summary>
    /// custom GUI controls for the overriding class
    /// </summary>
    protected virtual void OnGUICustom() { }

    /// <summary>
    /// tooltip display for the overriding class
    /// </summary>
    protected virtual void tooltipDisplay() { }


    /// <summary>
    /// 
    /// resets the camera to the default view
    /// 
    /// </summary>
    protected void ResetView()
    {

        if (!orthogonalView)
        {
            Camera.main.transform.position = new Vector3(0, 12f, -30f);
            Camera.main.transform.rotation = new Quaternion(0.216f, 0.0f, 0.0f, 0.975f);
        }
        else
        {
            orthoZoom = 16;
            Camera.main.transform.position = new Vector3(0, 100f, 0f);
            Camera.main.transform.rotation = new Quaternion(0.707107f, 0.0f, 0.0f, 0.707107f);
            Camera.main.orthographicSize = orthoZoom;
        }

    }

    /// <summary>
    /// 
    /// creates an empty path
    /// 
    /// </summary>
    /// <returns>VehicleDelivery with an empty customer list</returns>
    protected VehicleDelivery simplePath()
    {
        Vector3 pos = GameObject.Find("base").transform.position;
        DataObjects.VehicleDelivery p = new DataObjects.VehicleDelivery();
        p.warehouse = new Warehouse();
        try
        {
            p.warehouse = scenario.warehouse;
        } catch (Exception e)
        {
            Debug.Log(e);
        }
        return p;
    }

    /// <summary>
    /// 
    /// remove and reset all paths in a plan
    /// 
    /// </summary>
    protected void RemovePaths()
    {
        selectedPathIndex = -1;
        plan.paths.Clear();
        RefreshPaths();
        planMetricsStr = "";
        overbudget = false;
    }

    /// <summary>
    /// 
    /// Unity update call
    /// 
    /// </summary>
    void Update()
    {

        // close popup view if a user mouse clicks
        if (Input.GetMouseButtonDown(0))
            GameObject.Find("popup").GetComponent<Canvas>().enabled = false;

        // check server cache and popup cache
        checkServerCache();
        checkServerCacheCustom();
        checkPopupCache();
        customUpdate();

        // toggle sounds
        if (Input.GetKeyDown("s") && !GUIAssets.PopupButton.showing)
        {
            soundsOn = !soundsOn;
            if (soundsOn)
                playClick();
        }

        // toggle music
        if (Input.GetKeyDown("m") && !GUIAssets.PopupButton.showing)
        {
            if (musicPaused)
                GameObject.Find("backgroundmusic").GetComponent<AudioSource>().Play();
            else
                GameObject.Find("backgroundmusic").GetComponent<AudioSource>().Pause();
            musicPaused = !musicPaused;
        }

        // refresh paths and remove selected path
        if (Input.GetKeyDown("r") && !GUIAssets.PopupButton.showing)
        {
            selectedPathIndex = -1;
            RefreshPaths();
        }

    }

    /// <summary>
    /// custom Update code to run in the overriding class
    /// </summary>
    protected virtual void customUpdate() { }

    /// <summary>
    /// gets the customer at the mouse location
    /// </summary>
    /// <returns>Customer object</returns>
    protected Customer customerAtMouse()
    {
        object[] retObejcts = customerAtMousePosition();
        if (retObejcts == null)
            return null;
        else
            return (Customer)retObejcts[1];
    }


    /// <summary>
    /// ray cast for a house or customer at the mouse location
    /// </summary>
    /// <returns>an array [GameObject of the house, Customer object] </returns>
    protected object[] customerAtMousePosition()
    {
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000.0f))
        {
            GameObject selectedObj = hitInfo.transform.gameObject;
            if (selectedObj != null)
            {
                if (customerIconsMap.ContainsKey(selectedObj))
                {
                    return new object[] { selectedObj, customerIconsMap[selectedObj] };
                }
            }
        }
        return null;
    }

    /// <summary>
    /// ray cast for a connection at the mouse location
    /// </summary>
    /// <returns>GameObject representing the connection, null if nothing is found</returns>
    protected GameObject connectionAtMouse()
    {
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000.0f))
        {
            GameObject selectedObj = hitInfo.transform.gameObject;
            if (selectedObj != null)
            {
                if (connectorMap.ContainsKey(selectedObj))
                {
                    if(selectedObj.GetComponent<MeshRenderer>().material.color.Equals(Color.cyan)) // only use select path
                        return selectedObj;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// 
    /// toggles the selected house location
    /// 
    /// </summary>
    /// <param name="selectedObj">the Unity game object selected</param>
    /// <param name="target">the Customer object associated with the location</param>
    protected void ToggleHouseSelection(GameObject selectedObj, Customer target)
    {

        if (!target.selected) 
            selectedObj.GetComponent<MeshRenderer>().material.color = Color.black;
        else if(target.selected && target.payload.StartsWith("food"))
            selectedObj.GetComponent<MeshRenderer>().material.color = GUIHelpers.REDHOUSE;
        else
            selectedObj.GetComponent<MeshRenderer>().material.color = GUIHelpers.YELLOWHOUSE;

        if (weightLabelMap.ContainsKey(target))
            weightLabelMap[target].GetComponent<MeshRenderer>().enabled = target.selected && toggleWeightLabels;
    }


    /// <summary>
    /// 
    /// sets the plan in the interface
    /// 
    /// </summary>
    /// <param name="openPlan">Plan to load</param>
    public void fromPlan(Plan openPlan)
    {

        // remove in memory references with serialize and deserialize
        string planData = JsonConvert.SerializeObject(openPlan);
        Plan sentPlan = JsonConvert.DeserializeObject<Plan>(planData);

        // reset selected path index
        selectedPathIndex = -1;

        // reset and load scenario
        // planner will see plan against the submitted scenario
        // business will see plans against the associate sceanrio   
        setPlanFromPlan(sentPlan);
        AddHouses();

        plan.tag = sentPlan.tag;
        plan.scenario = scenario;
        plan.paths.Clear();

        foreach(VehicleDelivery delivery in sentPlan.paths)
        {
            delivery.warehouse = scenario.warehouse;
            plan.paths.Add(delivery);
        }

        RefreshPaths();

        // update calculations
        SetHouseAndLabelDisplay();

    }

    /// <summary>
    /// 
    /// custom fromPlan method for the operational planner view and business view.
    /// 
    /// the operational planner view keeps the scenario fixed and the business planner
    /// updates the scenario based on the plan scenario.
    /// 
    /// </summary>
    /// <param name="sentPlan">Plan object</param>
    protected virtual void setPlanFromPlan(Plan sentPlan) { }

    /// <summary>
    /// 
    /// play the click sounds when user presses a button
    /// 
    /// </summary>
    protected void playClick()
    {
        if (soundsOn)
        {
            AudioSource audioData = GameObject.Find("clicksound").GetComponent<AudioSource>();
            audioData.Play(0);
        }
    }

    /// <summary>
    /// 
    /// checks the server queue in the RestWebService
    /// 
    /// </summary>
    private void checkServerCache()
    {

        // check for full plans to load
        Plan[] plans = RestWebService.planqueue.ToArray();
        if (plans.Length > 0)
        {
            foreach (Plan plan in plans)
            {
                // set all paths to visible
                foreach (VehicleDelivery delivery in plan.paths)
                {
                    // cost shock applied to vehicles
                    delivery.vehicle.cost = DesignerAssets.UAVDesigner.getShockCost(delivery.vehicle.cost, RestWebService.market);
                }
                loadedPlans[plan.id].plan = plan;
                Debug.Log("save " + plan.id);
            }
            RestWebService.planqueue.Clear();
        }

        // check for short plan id and tags to load
        PlanShort[] shortplans = RestWebService.planshortqueue.ToArray();
        if (shortplans.Length > 0)
        {
            foreach (PlanShort plan in shortplans)
            {
                if (!loadedPlans.ContainsKey(plan.id))
                {
                    
                    // create a stored plan and add a tag and id
                    StoredPlan storedplan = new StoredPlan();
                    storedplan.selected = false;
                    storedplan.tag = plan.tag;
                    loadedPlans.Add(plan.id, storedplan);

                    // load the full plan
                    DataInterface.GetPlan(plan.id);

                }

            }
            RestWebService.planshortqueue.Clear();

        }

        // check for scenario
        List<Scenario> scenarioqueue = RestWebService.scenarioqueue;
        if (scenarioqueue.Count > 0)
        {

            // set the scenario
            scenario = scenarioqueue[scenarioqueue.Count - 1];
            plan.scenario = scenario;

            // gets the maximum delivery weight
            maxDeliveryWeight = 0;
            for (int i = 0; i < scenario.customers.Count; i++)           
                maxDeliveryWeight = Math.Max(maxDeliveryWeight, scenario.customers[i].weight);
            
            // add houses to the scene
            AddHouses();

            RestWebService.scenarioqueue.Clear();

            // add warehouses to each path
            foreach(DataObjects.VehicleDelivery p in plan.paths)
                p.warehouse = scenario.warehouse;

            // update house display
            SetHouseAndLabelDisplay();

            Capture.Log("ScenarioLoaded:" + scenario.id, business ? Capture.BUSINESS : Capture.PLANNER);

        }

        // add vehicles from the queue, removes the paths and restarts

        // need to wait until the current market is loaded
        Vehicle[] vehiclequeue = RestWebService.vehiclequeue.ToArray();
        if (vehiclequeue.Length > 0 && RestWebService.market != 0)
        {

            // clear all vehicles
            teamVehicles.Clear();

            // for each vehicle in the queue
            foreach (Vehicle vehicle in vehiclequeue)
            {
                // cost shock for vehicles
                vehicle.cost = DesignerAssets.UAVDesigner.getShockCost(vehicle.cost, RestWebService.market);
                teamVehicles.Add(vehicle);

                maxVehicleCost = System.Math.Max(maxVehicleCost, vehicle.cost);
                maxVehicleRange = System.Math.Max(maxVehicleRange, vehicle.range);
                maxVehicleVelocity = System.Math.Max(maxVehicleVelocity, vehicle.velocity);
                maxVehicleCapacity = System.Math.Max(maxVehicleCapacity, vehicle.payload);
            }
            RestWebService.vehiclequeue.Clear();

        }

        // checks for a result message from the server
        if (RestWebService.resultstr != null)
        {
            string msg = RestWebService.resultstr;
            if (msg.Contains("Success"))
                ShowMsg(msg, false);
            else
                ShowMsg(msg, true);
            Capture.Log("ServerMessage;" + RestWebService.resultstr, business ? Capture.BUSINESS : Capture.PLANNER);
            RestWebService.resultstr = null;
        }

    }

    /// <summary>
    /// overriding class server cache update code 
    /// </summary>
    protected virtual void checkServerCacheCustom() { }

    /// <summary>
    /// 
    /// checks if a popup action occurs
    /// 
    /// </summary>
    void checkPopupCache()
    {

        // planner : open a plan popup
        if (GUIAssets.PopupButton.ok && GUIAssets.PopupButton.storedData.StartsWith("OpenPlan"))
        {

            // reset ok variable
            GUIAssets.PopupButton.ok = false;
            GUIAssets.PopupButton.storedData = "";

            fromPlan(userSelectedPlan);

            try
            {
                Capture.Log("Opened;" + userSelectedPlan.tag, business ? Capture.BUSINESS : Capture.PLANNER);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            userSelectedPlan = null;
            ShowMsg("Plan opened : " + plan.tag, false);

        }

        checkPopupCacheCustom();

    }

    /// <summary>
    /// custom popup cache code for the overriding class
    /// </summary>
    protected virtual void checkPopupCacheCustom() { }

    /// <summary>
    /// gets the string representation of a plan for data logs
    /// </summary>
    /// <returns></returns>
    protected string toString()
    {
        return JsonConvert.SerializeObject(plan);
    }

    /// <summary>
    /// 
    /// stores plan information
    /// 
    /// </summary>
    protected class StoredPlan
    {
        /// <summary>
        /// toggles plan selection 
        /// </summary>
        public bool selected = false;

        /// <summary>
        /// tag or name of the plan
        /// </summary>
        public string tag = "";

        /// <summary>
        /// the full plan object
        /// </summary>
        public Plan plan = null;

    }


}


