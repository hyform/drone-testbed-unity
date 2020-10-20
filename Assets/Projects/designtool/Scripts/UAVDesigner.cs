using System.Collections.Generic;
using UnityEngine;
using DesignerTools;
using DesignerObjects;
using DataObjects;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using static DataObjects.RestWebService;

namespace DesignerAssets
{
    /// <summary>
    /// 
    /// The main code for the designer interface. It includes all GUI controls and logic
    /// used to create vehicle configurations that adhere to a design grammar. The left side of
    /// the GUI displays controls to build a vehicle configuration. The right side shows a list
    /// of available team designs.
    /// 
    /// The below is a description for the uav string configuration and layout.
    /// 
    ///              J K L M N O P 
    ///                    z
    /// 
    ///                    |        forward     P
    ///                    |                    O
    ///                    |                    N
    ///              - - - - - - -         x    M
    ///                    |                    L
    ///                    |                    K
    ///                    |                    J
    ///                    
    /// 
    /// example string : *aMM0+++++*bNM2+++*cMN1+++*dLM2+++*eML1+++^ab^ac^ad^ae,5,3
    /// 
    /// component : *bNM2+++ : b:node id, N : x position, M : z position, 2 component type, +++ size
    ///             component types = 0 : structure, 1 Motor CW, 2 : Motor CCW, 3 : Foil, 4 : Empty  
    /// ^ab : edge : first char is the starting node id and the second char is the ending node id
    /// ,5,3 : capacity in pounds and the controller index, currently the controller index is not used
    /// 
    /// </summary>
    public class UAVDesigner : MonoBehaviour
    {

        /// <summary>
        /// maps a Unity game object of a joint to its vehicle assembly joint information
        /// </summary>
        private Dictionary<GameObject, JointInfo> jointGraph = new Dictionary<GameObject, JointInfo>();

        /// <summary>
        /// maps a Unity game object of a connection to its vehicle assembly connection information
        /// </summary>
        private Dictionary<GameObject, ConnectorInfo> connectionGraph = new Dictionary<GameObject, ConnectorInfo>();

        /// <summary>
        /// maps a Unity handle game object used to create a connection with the connection game object 
        /// </summary>
        private Dictionary<GameObject, GameObject> jointHandleToConnection = new Dictionary<GameObject, GameObject>();

        /// <summary>
        /// stores intersections to restrict overlapping geometries
        /// </summary>
        private List<Vector3> intersections = new List<Vector3>();

        /// <summary>
        /// connection length
        /// </summary>
        private float connectionSize = 10f;

        /// <summary>
        /// joint size
        /// </summary>
        private float jointSize = 2.0f;

        // variables to support analysis

        /// <summary>
        /// need to store capacity as a string representation to work for empty ""
        /// </summary>        
        private string capacityStr = "2";

        /// <summary>
        /// previous string store to identfiy user changes
        /// </summary>
        private string previousCapacityStr = "2";

        /// <summary>
        /// toggle when showing results after an evalution
        /// </summary>
        public static bool showingEvaluationMode = false;

        /// <summary>
        /// toggle when evaluating a design
        /// </summary>
        public static bool evaluating = false;

        /// <summary>
        /// stores if an evaluation is successful
        /// </summary>
        public static bool successfulRun = false;

        /// <summary>
        /// index in the trajectory path for the current evaluation vehicle in the animation
        /// </summary>
        private int evaluationTrajectoryIndex = 0;

        /// <summary>
        /// results of the last evaluation
        /// </summary>
        private RestWebService.EvaluationOutput lastOutput;

        /// <summary>
        /// sample interval for the trajectory path
        /// </summary>
        public float sampleInterval = 0.2f;

        /// <summary>
        /// simulation time of the evaluation
        /// </summary>
        private float simTime = 0;

        // flags and helpers to run the designer AI

        /// <summary>
        /// helper class to show AI generated vehicles
        /// </summary>
        private AIHelper aihelper = new AIHelper();

        /// <summary>
        /// flag to identify if the evaluation is for an AI run
        /// </summary>
        private bool aiRun = false;

        /// <summary>
        /// flag to identify if the view is currently in AI mode
        /// </summary>
        public static bool aiMode = false;

        /// <summary>
        /// saves the last result into a string 
        /// </summary>
        private string resultMessage = "";

        /// <summary>
        /// string to display in the bottom log display
        /// </summary>
        private string bottomLogString = "";

        /// <summary>
        /// stores the design tag entered by a user when submitting a vehicle
        /// </summary>
        private string designtag = "tag";

        // GUI flags

        /// <summary>
        /// toggles clicks and sounds
        /// </summary>
        private bool soundsOn = true;

        /// <summary>
        /// toggles background music
        /// </summary>
        private bool musicPaused = true;

        /// <summary>
        /// toggles the help information panel
        /// </summary>
        private bool showInfoPanel = false;

        /// <summary>
        /// toggle the motor rotation in the scene
        /// </summary>
        private bool rotateMotors = true;

        /// <summary>
        /// list of stored teams vehicle
        /// </summary>
        List<Vehicle> teamDesigns = new List<Vehicle>();

        /// <summary>
        /// list of previous interface states of the vehicle
        /// </summary>
        private List<string> history = new List<string>();

        /// <summary>
        /// current index in the history list
        /// </summary>
        private int historyIndex = 100000;

        /// <summary>
        /// string storing the base configuration
        ///
        /// since this is the base design, we should change it so there is no scaling on the components
        /// 
        /// for now, we will keep this as the base design though
        /// 
        /// </summary>
        public static string BASEVEHICLECONFIG = "*aMM0+++++*bNM2+++*cMN1+++*dLM2+++*eML1+++^ab^ac^ad^ae,5,3";

        /// <summary>
        /// variable to store right side team design scroll window position
        /// </summary>
        private Vector2 scrollPosition;

        /// <summary>
        /// store the local evaluation physics calculation class
        /// </summary>
        private UAVPhysics physics = null;

        // string constants
        public static string VEHICLECOMPONENT = "rb";
        public static string PROTOTYPESTRUCTURE = "protostructure";
        public static string PROTOTYPEWIDESTRUCTURE = "protowidestructure";
        public static string PROTOTYPENARROWSTRUCTURE = "protonarrowstructure";
        public static string PROTOTYPEMOTORCCW = "protomotorccw";
        public static string PROTOTYPEMOTORCW = "protomotorcw";
        public static string PROTOTYPECONNECTION = "protoconnection";
        public static string PROTOTYPEFOIL = "protofoil";
        public static string JOINT = "joint";
        public static string SIZELABEL = "sizelabel";
        public static string CONNECTION = "connection";
        public static string STRUCTURE = "structure";
        public static string WIDESTRUCTURE = "widestructure";
        public static string NARROWSTRUCTURE = "narrowstructure";
        public static string MOTORCCW = "motorccw";
        public static string MOTORCW = "motorcw";
        public static string FOIL = "foil";
        public static string SPINNERCW = "spinnercw";
        public static string SPINNERCCW = "spinnerccw";
        public static string POSITIVEX = "posx";
        public static string NEGATIVEX = "negx";
        public static string POSITIVEZ = "posz";
        public static string NEGATIVEZ = "negz";
        public static string POSITIVEY = "posy";
        public static string NEGATIVEY = "negy";
        public static string AI = "ai";
        public static string EVALUATIONDISPLAY = "evaluationdisplay";
        public static string NOEVENT = "NoEvent";

        // panel string constants
        public static string OPENDESIGNPOPUPCONFIRM = "popupOpenNewDesign";
        public static string OPENDESIGN = "OpenDesign";
        public static string RESETDESIGN = "ResetDesign";
        public static string SUBMITDESIGNCANVAS = "SubmitCanvas";
        public static string SUBMITINPUTTAG = "InputTag";
        public static string POPUPCONFIRMEVALUATION = "popupConfirmEvaluation";
        public static string DESIGNTOOLBOXPANEL = "toolboxPanel";
        public static string AICANVASPANEL = "AICanvas";
        public static string HELPPANEL = "helpPanel";
        public static string POPUPRESULTSPANEL = "popupResultsPanel";
        public static string POPUPRESULTSSUCCESSBUTTON = "popupsuccess";
        public static string POPUPRESULTSERRORBUTTON = "popuperror";
        public static string POPUPRESULTSERRORTEXT = "popuptext";
        public static string CLICKSOUND = "clicksound";

        // textures for toolbar icons
        public Texture2D undoimage;
        public Texture2D redoimage;
        public Texture2D resetdesignimage;
        public Texture2D resetviewimage;
        public Texture2D aiimage;
        public Texture2D dbloadimage;
        public Texture2D unselectedvehicle;
        public Texture2D selectedvehicle;
        public Texture2D infoimage;

        // tooltip variables
        private string tooltip = " ";
        private Rect tooltipRect = new Rect(0, 0, 65, 25);
        private Rect infoRect = new Rect(130, 10, 28, 28);
        private Rect resetViewRect = new Rect(100, 10, 28, 28);
        private Rect resetDesignRect = new Rect(70, 10, 28, 28);
        private Rect undoRect = new Rect(10, 10, 28, 28);
        private Rect redoRect = new Rect(40, 10, 28, 28);
        private Rect evalRect = new Rect(20, 100, 100, 25);
        private Rect submitRect = new Rect(340, 180, 100, 25);
        private Rect designModePopopRect = new Rect(224, 180, 100, 25);
        private Rect aiRect = new Rect(128, 100, 28, 28);
        //private Rect dronebotRect = new Rect(160, 100, 28, 28);
        private Rect dbRect;
        private Rect loadBoxRect = new Rect(0, 0, 1, 1);

        /// <summary>
        /// 
        /// Unity Start method
        /// 
        /// </summary>
        void Start()
        {

            Application.targetFrameRate = 30;

            // set log start time
            Capture.startLogTime();
            Capture.Log("StartSession", Capture.DESIGNER);

            // disable full screen
            Screen.fullScreen = false;

            // load toolbar icons
            LoadToolbarIcons();

            // get the current market (for shock cost)
            DataInterface.GetMarket();

            // reset the view position
            ResetView();

            // initialize the first joint and set the base design
            Initialize();
            fromstring(BASEVEHICLECONFIG);
            updateHistory(BASEVEHICLECONFIG);

            // load save vehicles and load bse gui vehicle
            DataInterface.GetVehicles();  

        }

        /// <summary>
        /// 
        /// runs when another team member uploads a new vehicle from another client
        /// 
        /// </summary>
        public void updateDesigns()
        {
            DataInterface.GetVehicles();
            Capture.Log("AutoLoadVehicles", Capture.DESIGNER);
            playClick();
        }

        /// <summary>
        /// 
        /// initializes the scene
        /// 
        /// </summary>
        void Initialize()
        {

            // sets the joint index counter to 0
            JointInfo.counter = 0;

            // reset and destroy all joint, components, and component labels
            foreach (GameObject joint in jointGraph.Keys)
            {
                if(jointGraph[joint] != null)
                    if (jointGraph[joint].gameObj != null)
                        Destroy(jointGraph[joint].gameObj);
                if (jointGraph[joint].textLabel != null)
                    Destroy(jointGraph[joint].textLabel);
                Destroy(joint);
            }
            foreach (GameObject connection in connectionGraph.Keys)
            {
                if(connection != null)
                    Destroy(connection);
            }

            // set the initial base joint
            addStartingJoint();

        }

        /// <summary>
        /// 
        /// resets the component and connection dictionaries and adds a base joint
        /// 
        /// </summary>
        private void addStartingJoint()
        {
            // reset all dictionaries
            jointGraph = new Dictionary<GameObject, JointInfo>();
            connectionGraph = new Dictionary<GameObject, ConnectorInfo>();
            jointHandleToConnection = new Dictionary<GameObject, GameObject>();
            intersections = new List<Vector3>();

            // add initital joint
            Vector3 pos = new Vector3(0f, 0f, 0f);
            intersections.Add(pos);
            GameObject basejoint = Instantiate(GameObject.Find(JOINT), pos, Quaternion.identity) as GameObject;
            GameObject labelText = GameObject.Find(SIZELABEL);
            Vector3 aboveJoint = new Vector3(pos.x, pos.y + 4, pos.z);
            GameObject baseTextObject = Instantiate(labelText, aboveJoint, Quaternion.identity) as GameObject;
            baseTextObject.transform.Rotate(new Vector3(0, 1, 0), 225f);
            jointGraph.Add(basejoint, new JointInfo(JointInfo.UAVComponentType.None, 0, 0, 0, null, baseTextObject));
            jointGraph[basejoint].locked = true;

        }

        /// <summary>
        /// 
        /// loads toolbar icons for the view
        /// 
        /// </summary>
        void LoadToolbarIcons()
        {
            undoimage = Resources.Load("undo") as Texture2D;
            redoimage = Resources.Load("redo") as Texture2D;
            resetdesignimage = Resources.Load("reset") as Texture2D;
            resetviewimage = Resources.Load("view") as Texture2D;
            aiimage = Resources.Load("agent") as Texture2D;
            dbloadimage = Resources.Load("import") as Texture2D;
            unselectedvehicle = Resources.Load("unselected") as Texture2D;
            selectedvehicle = Resources.Load("selected") as Texture2D;
            infoimage = Resources.Load("info") as Texture2D;
        }
        
        /// <summary>
        /// 
        /// graphical user interface Unity method
        /// 
        /// </summary>
        void OnGUI()
        {

            // add design mode tools if the not in the below modes
            // evaluating a design
            // showing the evaluation of a design
            // if AI designs are shown and in aiMode
            // if a popup is opened in the view
            bool designMode = !evaluating && 
                !showingEvaluationMode && 
                !aiMode && 
                !GUIAssets.PopupButton.showing && !GameObject.Find(OPENDESIGNPOPUPCONFIRM).GetComponent<Canvas>().enabled;

            if (designMode)
                onGUIDesignMode();
            else if (showingEvaluationMode)
                onGUIShowEvalutionMode();
            else if (aiMode)
                onGUIAIMode();
            onGUIAllModes();

        }

        /// <summary>
        /// 
        /// GUI controls for all modes
        /// 
        /// includes the bottom log information string, rotating text to the main camera,
        /// and tooltips
        /// 
        /// </summary>
        void onGUIAllModes()
        {

            Rect rect = Camera.main.pixelRect;
            GUI.contentColor = Color.white;

            // bottom log string
            GUI.Label(new Rect(10, Screen.height - 36, Screen.width - 200, 36), "" + bottomLogString);

            // flip text labels if needed
            foreach (GameObject obj in jointGraph.Keys)
            {
                Assets.GUIHelpers.alignText(jointGraph[obj].textLabel.GetComponent<TextMesh>());
            }
            foreach (TextMesh obj in aihelper.labels)
            {
                Assets.GUIHelpers.alignText(obj);
            }
            Assets.GUIHelpers.flipText(GameObject.Find("fronttext").GetComponent<TextMesh>());

            // tooltips for each button control
            if (Event.current.type == EventType.Repaint && GUI.tooltip != tooltip)
            {
                tooltip = GUI.tooltip;
                if (infoRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(infoRect.xMax - 10, infoRect.yMax + 2, 400, infoRect.height);
                if (resetViewRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(resetViewRect.xMax - 10, resetViewRect.yMax + 2, 400, resetViewRect.height);
                if (resetDesignRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(resetDesignRect.xMax - 10, resetDesignRect.yMax + 2, 400, resetDesignRect.height);
                if (undoRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(undoRect.xMax - 10, undoRect.yMax + 2, 400, undoRect.height);
                if (redoRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(redoRect.xMax - 10, redoRect.yMax + 2, 400, redoRect.height);
                if (evalRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(evalRect.xMax - 10, evalRect.yMax + 2, 400, evalRect.height);
                if (submitRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(submitRect.xMin + 10, submitRect.yMax + 2, 400, submitRect.height);
                if (designModePopopRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(designModePopopRect.xMin + 10, designModePopopRect.yMax + 2, 400, designModePopopRect.height);
                if (aiRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(aiRect.xMax - 10, aiRect.yMax + 2, 400, aiRect.height);
                //if (dronebotRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(dronebotRect.xMax - 10, dronebotRect.yMax + 2, 400, dronebotRect.height);
                if (loadBoxRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(loadBoxRect.xMin, loadBoxRect.yMax, loadBoxRect.width, loadBoxRect.height);
                if (dbRect.Contains(Event.current.mousePosition)) tooltipRect = new Rect(dbRect.xMin - 168, dbRect.yMax + 2, 400, dbRect.height);
            }
            GUI.Label(tooltipRect, tooltip);

        }

        /// <summary>
        /// 
        /// GUI controls to include in the design mode
        /// 
        /// </summary>
        void onGUIDesignMode()
        {

            // set successful run
            successfulRun = false;

            // get sizing variables
            Rect rect = Camera.main.pixelRect;
            int xbuffer = 5;
            int ybuffer = 5;

            // hide AI panels
            GameObject.Find(DESIGNTOOLBOXPANEL).GetComponent<Canvas>().enabled = true;
            GameObject.Find(AICANVASPANEL).GetComponent<Canvas>().enabled = false;

            // show a box around the capacity and evaluate controls
            GUI.Box(new Rect(10, 50, 154, 92), "");

            // capacity label and input
            GUI.color = Color.white;
            GUI.Label(new Rect(20, 60, 120, 25), "Capacity (lb)");
            capacityStr = GUI.TextField(new Rect(110, 62, 28, 25), capacityStr + "", 2);
            if (!capacityStr.Equals(previousCapacityStr))
            {
                Capture.Log("CapacityInput;" + capacityStr, Capture.DESIGNER);
                previousCapacityStr = capacityStr;
            }

            // bound the capacity
            int i = 0;
            bool validCapacity = int.TryParse(capacityStr, out i);
            if (validCapacity)
            {
                int capacity = i;

                // bound the payload
                capacity = Math.Max(1, capacity);
                capacityStr = "" + capacity;
                previousCapacityStr = capacityStr;
            } 

            // create right side team designs list
            GUI.Box(loadBoxRect, new GUIContent("Team Designs", ""));
            int heightScroll = (int) Math.Max(rect.height - 120, 200);
            loadBoxRect = new Rect(Screen.width - 184 - xbuffer, ybuffer, 184, heightScroll);
            GUI.skin.box.fontSize = 18;

            // add a button to manually auto refresh the team design database (that will now also 
            // get auto refreshed, so we may be able to just remove this button)
            dbRect = new Rect(Screen.width - 188, 4, 24, 24);
            if (GUI.Button(dbRect, new GUIContent(dbloadimage, "Load Designs from Your Team")))
            {
                DataInterface.GetVehicles();
                Capture.Log("LoadVehicles", Capture.DESIGNER);
                playClick();
            }

            // add scroll window with the right panel
            int counter = 0;
            scrollPosition = GUI.BeginScrollView(new Rect(rect.width - 184, 40, 184, heightScroll - 40), scrollPosition, new Rect(0, 0, 184, teamDesigns.Count * 24 + 100));
            foreach (Vehicle s in teamDesigns)
            {
                // add button to open a design
                if (GUI.Button(new Rect(0, 0 + counter * 24, 164, 20), new GUIContent(s.tag, "Select to Open : " + s.tag + "\n" + s.range.ToString("0.0") + " mi\n" + s.payload.ToString("0") + " lb\n$" + UAVDesigner.getShockCost(s.cost).ToString("0") + "\n" + s.velocity.ToString("0.0") + " mph")))
                {
                    GUIAssets.PopupButton.popupPanelID = OPENDESIGNPOPUPCONFIRM;
                    GUIAssets.PopupButton.storedData = OPENDESIGN + ";" + s.tag + ";" + s.config + ";" + s.range.ToString("0.00") + ";" + s.payload.ToString("0") + ";" + UAVDesigner.getShockCost(s.cost).ToString("0") + ";" + s.velocity.ToString("0.00");
                    GameObject.Find(OPENDESIGNPOPUPCONFIRM).GetComponent<Canvas>().enabled = true;
                }
                counter += 1;
            }
            // End the scroll view that we began above
            GUI.EndScrollView();

            // evaluate button for a valid design
            if (validCapacity)
            {
                if (GUI.Button(evalRect, new GUIContent("Evaluate", "Evaluate the Design Performance in a Test Environment")))
                {
                    aiRun = false;
                    // runServerEvaluation();
                    runLocalEvaluation();
                    Capture.Log("Evaluate;" + generatestring(), Capture.DESIGNER);
                    ShowMsg("Evaluating ...", false);
                    playClick();

                }
            }

            // add an undo button to revert to previous history
            if (GUI.Button(undoRect, new GUIContent(undoimage, "Undo")))
            {
                if (historyIndex > 0)
                {                   
                    // revert back one in the history list
                    historyIndex -= 1;
                    fromstring(history[historyIndex]);
                    Capture.Log("Undo:" + history[historyIndex], Capture.DESIGNER);
                    playClick();
                }
            }

            // add a redo button to forward to recent stored histories
            if (GUI.Button(redoRect, new GUIContent(redoimage, "Redo")))
            {
                try
                {
                    if (historyIndex < history.Count - 1)
                    {
                        // forward to a save configuration
                        historyIndex += 1;
                        fromstring(history[historyIndex]);
                        Capture.Log("Redo:" + history[historyIndex], Capture.DESIGNER);
                        playClick();
                    }
                }
                catch (Exception e)
                {
                    bottomLogString = e.ToString();
                    Debug.Log(e);
                }
            }

            // this button opens a popup to reset to the base design
            if (GUI.Button(resetDesignRect, new GUIContent(resetdesignimage, "Reset Design")))
            {
                GUIAssets.PopupButton.popupPanelID = OPENDESIGNPOPUPCONFIRM;
                GUIAssets.PopupButton.storedData = RESETDESIGN;
                GameObject.Find(OPENDESIGNPOPUPCONFIRM).GetComponent<Canvas>().enabled = true;
            }

            // this button resets the view orientation
            if (GUI.Button(resetViewRect, new GUIContent(resetviewimage, "Reset View")))
            {
                ResetView();
                Capture.Log("ResetView;" + generatestring(), Capture.DESIGNER);
                playClick();
            }

            // this button toggles the information panel
            if (GUI.Button(infoRect, new GUIContent(infoimage, "Toggle Information Panel")))
            {
                showInfoPanel = !showInfoPanel;
                GameObject.Find(HELPPANEL).GetComponent<Canvas>().enabled = showInfoPanel;
                Capture.Log("ToggleInfoPanel:" + showInfoPanel, Capture.DESIGNER);
                playClick();
            }

            // if the session allows for AI and there is a valid capacity value entered,
            // show the AI button
            if (Startup.isAI && validCapacity)
            {
                if (GUI.Button(aiRect, new GUIContent(aiimage, "AI Agent to Generate Design Alternatives")))
                {

                    // close any opened popups
                    GameObject.Find(OPENDESIGNPOPUPCONFIRM).GetComponent<Canvas>().enabled = false;

                    // commented out server evaluation
                    //Vehicle vehicle = new Vehicle();
                    //vehicle.config = generatestring();

                    //// tag that this is an ai run, or that ai design should be shown based on the evaluation
                    //aiRun = true;
                    //DataInterface.EvaluateVehicle(vehicle);

                    aiRun = true;
                    runLocalEvaluation();

                    // display log messages
                    bottomLogString = "Evaluating for AI agent ... ";
                    ShowMsg("Evaluating for AI agent ...", false);
                    Capture.Log("RunDesignAgent;" + generatestring(), Capture.DESIGNER);

                    playClick();

                }
            }

            // if the session allows for AI and there is a valid capacity value entered,
            // show the AI button
            //if (Startup.droneBot)
            //{
            //    if (GUI.Button(dronebotRect, new GUIContent(aiimage, "Dronebot")))
            //    {

            //        DataInterface.GetDronebotResponse("What vehicles have more range than 20");
            //        playClick();

            //    }
            //}
        }

        /// <summary>
        /// 
        /// GUI controls when showing the evaluation of a resulting analysis
        /// 
        /// </summary>
        void onGUIShowEvalutionMode()
        {

            Rect rect = Camera.main.pixelRect;

            // hide toolbox panel and AI controls
            GameObject.Find(DESIGNTOOLBOXPANEL).GetComponent<Canvas>().enabled = false;
            GameObject.Find(AICANVASPANEL).GetComponent<Canvas>().enabled = false;

            // currently not submitting a design
            if (!GameObject.Find(SUBMITDESIGNCANVAS).GetComponent<Canvas>().enabled)
            {

                // show return to design mode and submit buttons

                // button to toggle back to design mode
                if (GUI.Button(designModePopopRect, new GUIContent("OK", "Return to Design Mode")))
                {
                    GameObject.Find(POPUPCONFIRMEVALUATION).GetComponent<Canvas>().enabled = false;
                    ResetDesignModeView();
                    Capture.Log("DesignMode", Capture.DESIGNER);
                }

                // if successful evaluation, show the Submit button
                if (successfulRun)
                    if (GUI.Button(submitRect, new GUIContent("Submit", "Submit the Design to Your Team")))
                    {
                        // show the popup controls to submit a design with a tag
                        designtag = "";
                        GameObject.Find(SUBMITINPUTTAG).GetComponent<TMP_InputField>().text = "";
                        GUIAssets.PopupButton.popupPanelID = SUBMITDESIGNCANVAS;
                        GUIAssets.PopupButton.showing = true;
                        GameObject.Find(SUBMITDESIGNCANVAS).GetComponent<Canvas>().enabled = true;
                    }

            }

        }

        /// <summary>
        /// 
        /// GUI controls for AI mode
        /// 
        /// </summary>
        void onGUIAIMode()
        {

            // remove design toolbox and show AI canvas
            GameObject.Find(DESIGNTOOLBOXPANEL).GetComponent<Canvas>().enabled = false;
            GameObject.Find(AICANVASPANEL).GetComponent<Canvas>().enabled = true;

            try
            {

                // if the index of the current AI prototype is less than the number of 
                // designer AI vehicles to generate
                if (aihelper.aiPrototypeIndex < aihelper.aiUavs.Count)
                {
                    // ready to make a new prototype
                    if (aihelper.aiStatus == 0)
                    {
                        // create the model representation using a string
                        fromstring(aihelper.keys[aihelper.aiPrototypeIndex]);
                        aihelper.aiStatus = 1;
                    }

                    // Unity and destroying of objects has shown to take more than 
                    // just one update to clean everything up, so we added a buffer 
                    // update rate of 10 
                    int updateBuffer = 10;
                    if (aihelper.aiStatus > updateBuffer + 1)
                    {
                        aihelper.copyToCreateVehicleRepresentation();
                        aihelper.aiPrototypeIndex += 1;
                        aihelper.aiStatus = 0;
                    }

                    // increment the aistatus variable to keep track of how many
                    // update calls have occurred since the fromstring call
                    if (aihelper.aiStatus >= 1)
                        aihelper.aiStatus += 1;

                } 

            }
            catch (System.Exception e)
            {

                Debug.Log(e);

                // if there is an error of any kind, 
                // just return to design mode with the current base design

                // clear the AI queue
                RestWebService.aiDesignerQueue.Clear();
                aihelper.removeAIGeneratedUAVDisplays();

                ResetView();
                fromstring(aihelper.baseVehicle);
                ShowMsg("AI agent error : try again", true);

                bottomLogString = "";

            }

            Rect rect = Camera.main.pixelRect;
            // button to toggle back to design mode
            if (GUI.Button(new Rect(20, 20, 100, 25), "Design Mode"))
            {
                GameObject.Find(POPUPCONFIRMEVALUATION).GetComponent<Canvas>().enabled = false;
                aiMode = false;
                aihelper.removeAIGeneratedUAVDisplays();
                ResetView();
                fromstring(aihelper.baseVehicle);
                Capture.Log("DesignMode", Capture.DESIGNER);
            }

        }

        /// <summary>
        /// 
        /// Unity FixedUpdate method
        /// 
        /// </summary>
        void FixedUpdate(){
            // check to run physics analysis for local evaluation
            localEvaluationPhysics();
        }

        /// <summary>
        /// 
        /// Check for server reponses or GUI popup changes.
        /// 
        /// Also, set all key and mouse listeners.
        /// 
        /// Updates evaluation display and rotate motors.
        /// 
        /// </summary>
        void Update()
        {

            // check for any server objects
            checkServerCache();

            // check for popup actions
            checkForActionsFromOpenedPopups();

            // add key and mouse listeners

            // add controls to toggle sounds
            if (Input.GetKeyDown("s") && !GUIAssets.PopupButton.showing)
            {
                soundsOn = !soundsOn;
                if (soundsOn)
                    playClick();
            }

            // add controls to stop blade rotation
            if (Input.GetKeyDown("r") && !GUIAssets.PopupButton.showing)
                rotateMotors = !rotateMotors;

            // add controls to start and stop music
            if (Input.GetKeyDown("m") && !GUIAssets.PopupButton.showing)
            { 
                if (musicPaused)
                    GameObject.Find("backgroundmusic").GetComponent<AudioSource>().Play();
                else
                    GameObject.Find("backgroundmusic").GetComponent<AudioSource>().Pause();
                musicPaused = !musicPaused;
            }

            // left mouse click in design mode, leftshift check is to check for mousepad zoom
            // and evaluating is to check for evaluation run
            if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftShift) && !evaluating)
            {

                // hide popup results unless showing evaluation mode
                if (!showingEvaluationMode)
                    GameObject.Find(POPUPRESULTSPANEL).GetComponent<Canvas>().enabled = false;
                
                // check for a mouse selection that intesects a game object
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000.0f))
                {
 
                    GameObject selected = hitInfo.transform.gameObject;
                    string[] result = leftClickSelected(selected);

                    // if change in assembly
                    if (!result.Equals(NOEVENT))
                    {
                        string s = generatestring();
                        updateHistory(s);
                        playClick();
                        Capture.Log("MouseClick;" + result[0] + ";" + s + ";" + result[1], Capture.DESIGNER);
                    }

                }
            }

            // if AI mode, check if an AI component is selected, mouse up seems to do
            // better at avoiding any undesired selections
            if (aiMode)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    selectAIComponent();
                }
            }

            // scale up component, either right mouse click or up arrow key
            if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f))
                {
                    GameObject selected = hitInfo.transform.gameObject;
                    scaleUpComponentAtJoint(selected);

                    string s = generatestring();
                    updateHistory(s);
                    playClick();
                    Capture.Log((Input.GetMouseButtonDown(1) ? "ScaleUp;" : "HotKeyScaleUp;") + s + ";" + getJointPositionStr(selected), Capture.DESIGNER);
                }
            }

            // scale down component, middle mouse click, down arrow key, or shift left click for mouse pad
            if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.DownArrow) 
                || (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift)))
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f))
                {
                    GameObject selected = hitInfo.transform.gameObject;
                    scaleDownComponentAtJoint(selected);

                    string s = generatestring();
                    updateHistory(s);
                    playClick();
                    Capture.Log((Input.GetKeyDown(KeyCode.DownArrow) ? "HotKeyScaleDown;" : "ScaleDown;") + s + ";" + getJointPositionStr(selected), Capture.DESIGNER);
                    
                }
            }

            // hot keys for component selection over a joint
            bool key1Down = Input.GetKeyDown("1");
            bool key2Down = Input.GetKeyDown("2");
            bool key3Down = Input.GetKeyDown("3");
            bool key4Down = Input.GetKeyDown("4");
            bool key5Down = Input.GetKeyDown("5");
            if (key1Down || key2Down || key3Down || key4Down || key5Down)
            {
                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f))
                {
                    GameObject selected = hitInfo.transform.gameObject;

                    // check if over a joint
                    if (selected.name.StartsWith(JOINT))
                    {
                        JointInfo.UAVComponentType comptype = JointInfo.UAVComponentType.Structure;
                        string logInfo = "HotKeyComponentToStructure";

                        if (key1Down)
                        {
                            comptype = JointInfo.UAVComponentType.Structure;
                            logInfo = "HotKeyComponentToStructure";
                        }
                        else if (key2Down)
                        {
                            comptype = JointInfo.UAVComponentType.MotorCW;
                            logInfo = "HotKeyComponentToMotorCW";
                        }
                        else if (key3Down)
                        {
                            comptype = JointInfo.UAVComponentType.MotorCCW;
                            logInfo = "HotKeyComponentToMotorCCW";
                        }
                        else if (key4Down)
                        {
                            comptype = JointInfo.UAVComponentType.Foil;
                            logInfo = "HotKeyComponentToFoil";
                        }
                        else if (key5Down)
                        {
                            comptype = JointInfo.UAVComponentType.None;
                            logInfo = "HotKeyComponentToEmpty";
                        }

                        changeJointToComponent(selected, comptype);

                        playClick();

                        string s = generatestring();
                        Capture.Log(logInfo + ";" + comptype.ToString() + ";" + s + ";" + getJointPositionStr(selected), Capture.DESIGNER);
                        updateHistory(s);

                    }
                }
            }

            // delete operation
            if (Input.GetKeyDown("d"))
            {

                RaycastHit hitInfo = new RaycastHit();
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f))
                {

                    GameObject selected = hitInfo.transform.gameObject;
                    if (selected) { 

                        // if mouse is over a joint
                        if (selected.name.StartsWith(JOINT))
                        {
                            // hide the results panel
                            GameObject.Find(POPUPRESULTSPANEL).GetComponent<Canvas>().enabled = false;

                            // change joint component to empty
                            changeJointToComponent(selected, JointInfo.UAVComponentType.None);
                            playClick();
                            Capture.Log("RemovedComponent;" + generatestring() + ";" + getJointPositionStr(selected), Capture.DESIGNER);

                        }

                        // delete connector 
                        if (selected.name.StartsWith(CONNECTION))
                        {
                            // hide the results panel
                            GameObject.Find(POPUPRESULTSPANEL).GetComponent<Canvas>().enabled = false;

                            ConnectorInfo connectorInfo = connectionGraph[selected];
                            bool endConnection = true;
                            foreach (GameObject key in connectionGraph.Keys)
                                if (connectionGraph[key].x1 == connectorInfo.x2)                             
                                    endConnection = false;

                            // identify joint component to delete
                            GameObject connectedjoint = null;
                            foreach (GameObject joint in jointGraph.Keys)
                            {

                                // checks to see if the connection
                                // has an ending component (for a cycle in the assembly, joints
                                // are not always added at the end since there is one there already
                                if (connectionGraph[selected].x2 == jointGraph[joint].index
                                    && connectionGraph[selected].addedComponent)
                                    connectedjoint = joint;
                            }

                            // if there is not ending component (a cycle in the assembly)
                            // or if it is an ending connection
                            if (connectedjoint == null || endConnection)
                            {

                                // if there is a component at the end of the connection,
                                // remove the component
                                if (connectedjoint != null)
                                {
                                    intersections.Remove(connectedjoint.transform.position);
                                    if (jointGraph[connectedjoint].gameObj != null)
                                        Destroy(jointGraph[connectedjoint].gameObj);
                                    if (jointGraph[connectedjoint].textLabel != null)
                                        Destroy(jointGraph[connectedjoint].textLabel);
                                    jointGraph.Remove(connectedjoint);
                                    Destroy(connectedjoint);
                                    bottomLogString = "Removed component";
                                }

                                string position = "";
                                // reactivate handle associated with this connection and make it visible
                                GameObject reactivateHandle = null;
                                foreach (GameObject key in jointHandleToConnection.Keys)
                                {
                                    if (selected.Equals(jointHandleToConnection[key]))
                                    {
                                        key.SetActive(true);
                                        reactivateHandle = key;
                                        try
                                        {
                                            position = getJointPositionStr(key.transform.parent.gameObject) + "," + key.name;
                                        } catch (Exception e)
                                        {
                                            Debug.Log(e);
                                        }
                                    }
                                }

                                // remove joint and cleanup dictionaries
                                Destroy(selected);
                                connectionGraph.Remove(selected);
                                intersections.Remove(selected.transform.position);
                                if (reactivateHandle != null)
                                    jointHandleToConnection.Remove(reactivateHandle);

                                // when removing joints and connection, reorder components
                                reorderJoints();

                                string s = generatestring();
                                Capture.Log("RemovedConnector;" + s + ";" + position, Capture.DESIGNER);
                                bottomLogString = "Removed connector";
                                updateHistory(s);

                                playClick();

                            }
                            else
                            {
                                ShowMsg("Not an ending connection", true);
                            }

                        }

                    }

                }
            }

            // show evaluation animation
            if (showingEvaluationMode)
                moveEvalautionVehicle();

            // rotate motor blades
            if (rotateMotors || evaluating)
            {
                // graphically spin all rotors
                GameObject[] objscw = GameObject.FindGameObjectsWithTag(SPINNERCW);
                GameObject[] objsccw = GameObject.FindGameObjectsWithTag(SPINNERCCW);
                foreach (GameObject obj in objscw)
                {
                    obj.transform.Rotate(new Vector3(0, 1, 0), 6);
                }
                foreach (GameObject obj in objsccw)
                {
                    obj.transform.Rotate(new Vector3(0, 1, 0), -6);
                }
            }

        }

        /// <summary>
        /// 
        /// when removing connections, clean up indices of joints and connections 
        /// 
        /// </summary>
        private void reorderJoints()
        {

            // add changes of joint indices to a dictionary
            Dictionary<int, int> changes = new Dictionary<int, int>();
            int index = 0;
            // order all keys by incrememtning all of the joints
            foreach (GameObject obj in jointGraph.Keys)
            {
                if (jointGraph[obj].index != index)
                {
                    changes[jointGraph[obj].index] = index;
                    jointGraph[obj].index = index;
                }
                index += 1;
            }

            // reassign indicies of connectors
            JointInfo.counter = index;
            foreach (GameObject obj in connectionGraph.Keys)
            {
                if (changes.ContainsKey(connectionGraph[obj].x1))
                    connectionGraph[obj].x1 = changes[connectionGraph[obj].x1];              
                if (changes.ContainsKey(connectionGraph[obj].x2))
                    connectionGraph[obj].x2 = changes[connectionGraph[obj].x2];
            }


        }

        /// <summary>
        /// 
        /// Click sound for user selection
        /// 
        /// </summary>
        public void playClick()
        {
            if (soundsOn)
            {
                AudioSource audioData = GameObject.Find(CLICKSOUND).GetComponent<AudioSource>();
                audioData.Play(0);
            }
        }

        /// <summary>
        /// 
        /// Changes the component at a joint to a specific type
        /// 
        /// </summary>
        /// <param name="joint">Unity game object of a joint</param>
        /// <param name="compType">component type</param>
        private string[] changeJointToComponent(GameObject joint, JointInfo.UAVComponentType compType)
        {

            string typeAction = "";
            string position = getJointPositionStr(joint);

            // get previous size
            int previousSize = jointGraph[joint].sizedata;
            jointGraph[joint].sizedata = 0;

            // remove component at the joint
            if (jointGraph[joint].gameObj != null)
                Destroy(jointGraph[joint].gameObj);

            // get new position
            Vector3 pos = joint.transform.position;
            Vector3 newPos = new Vector3(pos.x, pos.y, pos.z);


            // add component at the joint position
            // steps : clone the gameobject in unity, rotate if needed, and update the jointgraph object
            if (compType.Equals(JointInfo.UAVComponentType.Structure))
            {

                GameObject pickedObject = GameObject.Find(STRUCTURE);
                GameObject childObject = Instantiate(pickedObject.gameObject, newPos, Quaternion.identity) as GameObject;
                childObject.transform.Rotate(new Vector3(0, 1, 0), 45f);
                childObject.tag = VEHICLECOMPONENT;
                jointGraph[joint].gameObj = childObject;
                jointGraph[joint].componentType = JointInfo.UAVComponentType.Structure;
                sizeComponent(joint, previousSize);
                jointGraph[joint].setTextLabel();

                bottomLogString = "Selected structure";
                typeAction = "ToggleStructure";

            }
            else if (compType.Equals(JointInfo.UAVComponentType.MotorCCW))
            {

                GameObject pickedObject = GameObject.Find(MOTORCCW);
                GameObject childObject = Instantiate(pickedObject.gameObject, newPos, Quaternion.identity) as GameObject;
                childObject.tag = VEHICLECOMPONENT;
                jointGraph[joint].gameObj = childObject;
                jointGraph[joint].componentType = JointInfo.UAVComponentType.MotorCCW;
                sizeComponent(joint, previousSize);
                jointGraph[joint].setTextLabel();

                bottomLogString = "Selected ccw motor";
                typeAction = "ToggleCCWMotor";

            }
            else if (compType.Equals(JointInfo.UAVComponentType.MotorCW))
            {

                GameObject pickedObject = GameObject.Find(MOTORCW);
                GameObject childObject = Instantiate(pickedObject.gameObject, newPos, Quaternion.identity) as GameObject;
                childObject.tag = VEHICLECOMPONENT;
                jointGraph[joint].gameObj = childObject;
                jointGraph[joint].componentType = JointInfo.UAVComponentType.MotorCW;
                sizeComponent(joint, previousSize);
                jointGraph[joint].setTextLabel();

                bottomLogString = "Selected cw motor";
                typeAction = "ToggleCWMotor";

            }
            else if (compType.Equals(JointInfo.UAVComponentType.Foil))
            {

                GameObject pickedObject = GameObject.Find(FOIL);
                GameObject childObject = Instantiate(pickedObject.gameObject, newPos, Quaternion.identity) as GameObject;
                childObject.transform.Rotate(new Vector3(0, 1, 0), 45);
                childObject.transform.Rotate(new Vector3(1, 0, 0), -10);
                childObject.tag = VEHICLECOMPONENT;
                jointGraph[joint].gameObj = childObject;
                jointGraph[joint].componentType = JointInfo.UAVComponentType.Foil;
                sizeComponent(joint, previousSize);
                jointGraph[joint].setTextLabel();

                bottomLogString = "Selected foil";
                typeAction = "ToggleFoil";

            }
            else if (compType.Equals(JointInfo.UAVComponentType.None))
            {

                jointGraph[joint].gameObj = null;
                jointGraph[joint].componentType = JointInfo.UAVComponentType.None;
                jointGraph[joint].sizedata = previousSize;
                jointGraph[joint].setTextLabel();

                bottomLogString = "No component";
                typeAction = "ToggleEmpty";

            }

            return new string[] { typeAction, position };

        }


        private string getJointPositionStr(GameObject joint)
        {
            string position = "";
            try
            {
                float x = jointGraph[joint].x;
                float z = jointGraph[joint].z;
                position = JointInfo.getPositionChar((int)x) + "" + JointInfo.getPositionChar((int)z);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
            return position;
        }

        /// <summary>
        /// 
        /// adds a connector at the selected handle
        /// 
        /// </summary>
        /// <param name="selected">Unity game object of the selected handle</param>
        /// <returns></returns>
        private string[] addConnectorAtHandle(GameObject selected)
        {
            Vector3 pos = selected.transform.position;
            string handleInfo = getJointPositionStr(selected.transform.parent.gameObject);

            if (selected.name.Equals(POSITIVEZ))
            {
                Vector3 endPoint = new Vector3(pos.x, pos.y, -jointSize / 2.0f + pos.z + connectionSize);
                Vector3 startPoint = new Vector3(pos.x, pos.y, -jointSize / 2.0f + pos.z);
                addConnector(selected, endPoint, startPoint, 0f, 0f, 1f);
                handleInfo += ",posz";
            }

            if (selected.name.Equals(POSITIVEX))
            {
                Vector3 endPoint = new Vector3(-jointSize / 2.0f + pos.x + connectionSize, pos.y, pos.z);
                Vector3 startPoint = new Vector3(-jointSize / 2.0f + pos.x, pos.y, pos.z);
                addConnector(selected, endPoint, startPoint, 90f, 0f, 1f);
                handleInfo += ",posx";
            }

            if (selected.name.Equals(NEGATIVEZ))
            {
                Vector3 endPoint = new Vector3(pos.x, pos.y, jointSize / 2.0f + pos.z - connectionSize);
                Vector3 startPoint = new Vector3(pos.x, pos.y, jointSize / 2.0f + pos.z);
                addConnector(selected, endPoint, startPoint, 180f, 0f, 1f);
                handleInfo += ",negz";
            }

            if (selected.name.Equals(NEGATIVEX))
            {
                Vector3 endPoint = new Vector3(jointSize / 2.0f + pos.x - connectionSize, pos.y, pos.z);
                Vector3 startPoint = new Vector3(jointSize / 2.0f + pos.x, pos.y, pos.z);
                addConnector(selected, endPoint, startPoint, -90f, 0f, 1f);
                handleInfo += ",negx";
            }

            bottomLogString = "Assembly change";
            return new string[] { "AssemblyChange", handleInfo };

        }

        /// <summary>
        /// 
        /// check for a left click selection on joints (toggle structure, foil, motors at joints) 
        /// and joint handles for connections (assembly process)
        /// 
        /// </summary>
        /// <param name="selected">selected Unity game object</param>
        private string[] leftClickSelected(GameObject selected)
        {

            string[] typeAction = new string[] { NOEVENT, "" };

            // check for left click on joint handle
            if (selected.name.StartsWith(JOINT))
            {
                JointInfo.UAVComponentType componentType = JointInfo.getNextComponentType(jointGraph[selected].componentType);
                typeAction[0] = "ToggleComponent";
                typeAction = changeJointToComponent(selected, componentType);
            }
            // check for left click on assembly handle
            else if (selected.name.Equals(POSITIVEZ) ||
                selected.name.Equals(NEGATIVEZ) ||
                selected.name.Equals(POSITIVEX) ||
                selected.name.Equals(NEGATIVEX))
            {
                typeAction = addConnectorAtHandle(selected);
            }

            return typeAction;


        }

        /// <summary>
        /// 
        /// scale up the component at the selected joint
        /// 
        /// </summary>
        /// <param name="selected">Unity game object at the selected mouse location</param>
        private void scaleUpComponentAtJoint(GameObject selected)
        {

            if (selected.name.StartsWith(JOINT) && jointGraph[selected].gameObj != null)
            {
                Vector3 scale = jointGraph[selected].gameObj.transform.gameObject.transform.localScale;
                float increment = (scale.x + 0.25f) / scale.x;
                jointGraph[selected].gameObj.transform.gameObject.transform.localScale = new Vector3(increment * scale.x, increment * scale.y, increment * scale.z);
                jointGraph[selected].sizedata = jointGraph[selected].sizedata + 1;
                jointGraph[selected].setTextLabel();

                bottomLogString = "Increase size";
            }

        }

        /// <summary>
        /// 
        /// scale down the component at the selected joint
        /// 
        /// </summary>
        /// <param name="selected">Unity game object at the selected mouse location</param>
        private void scaleDownComponentAtJoint(GameObject selected)
        {
            if (selected.name.StartsWith(JOINT) && jointGraph[selected].gameObj != null)
            {

                Vector3 scale = jointGraph[selected].gameObj.transform.gameObject.transform.localScale;
                if (scale.x > 0.8)
                {
                    float increment = (scale.x - 0.25f) / scale.x;
                    jointGraph[selected].gameObj.transform.gameObject.transform.localScale = new Vector3(increment * scale.x, increment * scale.y, increment * scale.z);
                    jointGraph[selected].sizedata = jointGraph[selected].sizedata - 1;
                    jointGraph[selected].setTextLabel();

                    bottomLogString = "Decrease size";
                }
            }

        }

        /// <summary>
        /// 
        /// adds a connector at the handle connection point
        /// 
        /// </summary>
        /// <param name="selected">selected Unity object handle</param>
        /// <param name="endPoint">connector endpoint</param>
        /// <param name="startPoint">connector startpoint</param>
        /// <param name="rotateAngle">angle to rotate the connection</param>
        /// <param name="connectorScale">scale of the connector</param>
        private void addConnector(GameObject selected, Vector3 endPoint, 
            Vector3 startPoint, float rotateAngleY, float rotateAngleX, 
            float connectorScale)
        {


            Vector3 pos = selected.transform.position;
            Vector3 center = new Vector3((startPoint.x + endPoint.x) / 2.0f, (startPoint.y + endPoint.y) / 2.0f, (startPoint.z + endPoint.z) / 2.0f);

            // check for existing joint at the endpoint for cyclic assembly
            bool jointAtEndingLocation = false;
            foreach (GameObject joint in jointGraph.Keys)
            {
                // tolerance for Unity rounding errors
                jointAtEndingLocation = jointAtEndingLocation 
                    || (System.Math.Abs(connectionSize*jointGraph[joint].x - endPoint.x) < 0.01 
                    && System.Math.Abs(connectionSize * jointGraph[joint].z - endPoint.z) < 0.01);
            }

            // add joint if no existing joint is at the end point
            // this is where joints are added to the assembly
            if (!jointAtEndingLocation)
            {

                GameObject joint = GameObject.Find(JOINT);
                GameObject component = Instantiate(joint, endPoint, Quaternion.identity) as GameObject;

                GameObject sizeLabel = GameObject.Find(SIZELABEL);
                Vector3 aboveJoint = new Vector3(endPoint.x, endPoint.y + 4, endPoint.z);
                GameObject componentTextLabel = Instantiate(sizeLabel, aboveJoint, Quaternion.identity) as GameObject;
                componentTextLabel.transform.Rotate(new Vector3(0, 1, 0), 225f);

                JointInfo jointInfo = new JointInfo(JointInfo.UAVComponentType.None, 0, endPoint.x / 10.0f, endPoint.z / 10.0f, null, componentTextLabel);
                jointGraph.Add(component, jointInfo);
                jointInfo.sizedata = 0;

            }

            // check for existing connections at the same location
            bool connectionIntersection = false;
            foreach (Vector3 v in intersections)
                connectionIntersection = connectionIntersection 
                    || (System.Math.Abs(v.x - center.x) < 0.01 
                    && System.Math.Abs(v.y - center.y) < 0.01 
                    && System.Math.Abs(v.z - center.z) < 0.01);


            // if no existing connections at this location (this should always be true) 
            // but just used as a double check
            if (!connectionIntersection)
            {

                // add new connector and rotate connector
                GameObject connection = GameObject.Find(CONNECTION);
                GameObject connectionObject = Instantiate(connection, center, Quaternion.identity) as GameObject;
                connectionObject.tag = VEHICLECOMPONENT;
                connectionObject.transform.Rotate(new Vector3(0, 1, 0), rotateAngleY);
                connectionObject.transform.Rotate(new Vector3(1, 0, 0), rotateAngleX);

                // scale the connector
                Vector3 lScale = connectionObject.transform.localScale;
                lScale.z = connectorScale * lScale.z;
                connectionObject.transform.localScale = lScale;

                // hide GUI handle and attach it to the connection object in the dictionary
                jointHandleToConnection[selected] = connectionObject;
                selected.SetActive(false);
                intersections.Add(center);

                int indexa = -1;
                int indexb = -1;

                // find starting and ending joint indices and create a connection
                foreach (GameObject key in jointGraph.Keys)
                {
                    if (System.Math.Abs(jointGraph[key].x - (startPoint.x / 10f)) < 0.01 
                        && System.Math.Abs(jointGraph[key].z - (startPoint.z / 10f)) < 0.01)
                    {
                        indexa = jointGraph[key].index;
                    }
                    if (System.Math.Abs(jointGraph[key].x - (endPoint.x / 10f)) < 0.01 
                        && System.Math.Abs(jointGraph[key].z - (endPoint.z / 10f)) < 0.01)
                    {
                        indexb = jointGraph[key].index;
                    }
                }
                connectionGraph.Add(connectionObject, new ConnectorInfo(indexa, indexb, !jointAtEndingLocation));

            }

        }


        /// <summary>
        /// 
        /// drops a component type if the mouse is released above a joint
        /// when a user is dragging a component from the toolbox
        /// 
        /// </summary>
        /// <param name="mouseposition">mouse position</param>
        /// <param name="type">type of component</param>
        public void dropComponent(Vector3 mouseposition, int type)
        {

            // do a raycast at the end mouse position
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(mouseposition), out hitInfo, 1000.0f))
            {

                // test for a joint intersection
                GameObject selected = hitInfo.transform.gameObject;
                if (selected.name.StartsWith(JOINT))
                {

                    // get type of component and assign at the joint
                    JointInfo.UAVComponentType compType = (JointInfo.UAVComponentType)type;
                    changeJointToComponent(selected, compType);

                    playClick();

                    // add to log
                    string s = generatestring();
                    Capture.Log("DragComponent;" + compType.ToString() + ";" + s + ";" + getJointPositionStr(selected), Capture.DESIGNER);
                    updateHistory(s);


                }
            }
        }

        /// <summary>
        /// 
        /// sizes the component at a joint
        /// 
        /// </summary>
        /// <param name="selected">the selected Unity game object</param>
        /// <param name="size">integer value for the size of the component</param>
        private void sizeComponent(GameObject selected, int size)
        {

            // this should never happen, but a check
            if (jointGraph[selected].gameObj == null)
                return;

            // add a counter to prevent an infinite look if there is any issue
            int counter = 0;
            while ((int)jointGraph[selected].sizedata < size  
                && counter < 100)
            {
                scaleUpComponentAtJoint(selected);
                counter++;
            }
            while ((int)jointGraph[selected].sizedata > size 
                && counter < 100)
            {
                scaleDownComponentAtJoint(selected);
                counter++;
            }
        }

        /// <summary>
        /// 
        /// resets the view to the default view in design mode
        /// and removes evaluation objects
        /// 
        /// </summary>
        void ResetDesignModeView()
        {

            // set to design mode
            showingEvaluationMode = false;
            evaluating = false;

            // remove any evalaution objects
            RemoveEvaluationObjects();

            // resets the view
            ResetView();

            // remove the teststand
            ShowTestStand(false);

        }

        /// <summary>
        /// 
        /// resets the view to the default design mode
        /// 
        /// </summary>
        void ResetView()
        {
            float scaleView = 2.0f;
            Camera.main.transform.position = new Vector3(scaleView * 16.9f, scaleView * 13.60f, scaleView * 16.9f);
            Camera.main.transform.rotation = new Quaternion(0.0864f, -.8952f, 0.197f, 0.389f);
        }

        /// <summary>
        /// 
        /// change the view of the camera to evaluation view
        /// 
        /// </summary>
        void ResetViewToEvaluationView()
        {
            float scaleView = 2.0f;
            Camera.main.transform.position = new Vector3(1.8f*scaleView * 0f, scaleView * 27.20f, 1.8f*scaleView * -28.8f);
            Camera.main.transform.rotation = new Quaternion(-0.198f, -0.395f, 0.088f, -0.892f);
        }

        /// <summary>
        /// 
        /// removes all evaluation display objects (evaluation vehicle and path of the design)
        /// 
        /// </summary>
        private void RemoveEvaluationObjects()
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(EVALUATIONDISPLAY);
            foreach (GameObject obj in objects)
                Destroy(obj);
        }

        /// <summary>
        /// 
        /// creates a cylinder between two points (used to show the path trajectory)
        /// 
        /// </summary>
        /// <param name="start">start of the path segment</param>
        /// <param name="end">end of the path segment</param>
        /// <param name="width">width of the path segment</param>
        /// <param name="color">color of the path segment</param>
        private void CreateCylinderBetweenPoints(Vector3 start, Vector3 end, 
            float width, Color color)
        {

            var offset = end - start;
            var scale = new Vector3(width, offset.magnitude / 2.0f, width);
            var position = start + (offset / 2.0f);

            var cylinder = Instantiate(GameObject.Find("trajectorypath"), position, Quaternion.identity);
            cylinder.transform.up = offset;
            cylinder.transform.localScale = scale;
            cylinder.tag = EVALUATIONDISPLAY;

            cylinder.GetComponent<MeshRenderer>().material.color = color;

        }

        /// <summary>
        /// 
        /// the RestWebService holds the queues for vehicles as a list, server response
        /// as a string, and AI designs as dictionary
        /// 
        /// </summary>
        void checkServerCache()
        {

            // check for vehicles in the server queue
            List<Vehicle> vehicleJSONs = RestWebService.vehiclequeue;
            if (vehicleJSONs.Count > 0)
            {
                // clear the local team designs
                teamDesigns.Clear();
                foreach (Vehicle vehicle in vehicleJSONs)
                    teamDesigns.Add(vehicle);
                RestWebService.vehiclequeue.Clear();
            }

            // check for result string from the server
            string serverResultsStr = RestWebService.resultstr;
            if (serverResultsStr != null)
            {
                if (serverResultsStr.Contains("Success"))
                    ShowMsg(serverResultsStr, false);
                else
                    ShowMsg(serverResultsStr, true);
                RestWebService.resultstr = null;
                Capture.Log("ServerMessage;" + serverResultsStr, Capture.DESIGNER);
            }

            // still some issues we need to resolve before switching to a server base evaluation
            //// return evaluation result from the server
            //if(RestWebService.uavEvaluation != null)
            //{
            //    try
            //    {
            //        // if not an ai run (or a regular evaluation, show the resulting resuls and trajectory
            //        if (!aiRun)
            //        {

            //            showingEvaluationMode = true;

            //            // create a clone to the vehicle to display vehicle path
            //            createEvaluationVehicle();

            //            // store the evaluation as the last output
            //            lastOutput = RestWebService.uavEvaluation;

            //            // store last position of trajectory to draw segment
            //            Vector3 lastPosition = new Vector3(0, 0, 0);
            //            foreach (RestWebService.Trajectory trajactory in RestWebService.uavEvaluation.trajectory)
            //            {
            //                Vector3 nextPosition = new Vector3((float)trajactory.position[0],
            //                    (float)trajactory.position[1],
            //                    (float)trajactory.position[2]);
            //                CreateCylinderBetweenPoints(lastPosition, nextPosition, 0.8f, Color.white);
            //                lastPosition = nextPosition;
            //            }

            //            // show teststand and set view to show trajectory
            //            ShowTestStand(true);
            //            ResetViewToEvaluationView();

            //            // show result message 
            //            String resultMessage = lastOutput.result;
            //            if (resultMessage.Contains("Success"))
            //            {
            //                successfulRun = true;
            //                int capacity = getCapacity(lastOutput.config);
            //                resultMessage = GetResults(lastOutput.result, "\n", lastOutput.range, capacity, 
            //                    UAVDesigner.getShockCost(lastOutput.cost), lastOutput.velocity, null);
            //                bottomLogString = GetResults("Last Run", " : ", lastOutput.range, capacity, UAVDesigner.getShockCost(lastOutput.cost), lastOutput.velocity, null); ;
            //                ShowSuccessMsg(resultMessage);
            //            } else
            //            {
            //                ShowErrorMsg(resultMessage);
            //            }

            //        } else
            //        {
            //            // get the resulting evaluation
            //            RestWebService.EvaluationOutput output = RestWebService.uavEvaluation;
            //            string s = generatestring();
            //            float capacity = float.Parse(s.Split(',')[1]);

            //            // call the designer AI to find designs based on the current evaluation metrics
            //            aihelper.callWebServiceForAIDesigns((float)output.range, (float)output.cost, capacity, s);
            //        }

            //    }
            //    catch (Exception e)
            //    {
            //        Debug.Log(e);
            //    }
            //    finally
            //    {
            //        RestWebService.uavEvaluation = null;
            //    }
            //}

            // check for AI queue results, only check when in design mode
            if(!evaluating && !aiMode && !showingEvaluationMode)
                if (RestWebService.aiDesignerQueue.Count > 0)
                {

                    // set to AI mode and start creating design representations 
                    aiMode = true;

                    // send returned AI designs from server to the AIHelper
                    aihelper.startDesignerAIDisplayGeneration(RestWebService.aiDesignerQueue);

                    // clear the queue
                    RestWebService.aiDesignerQueue.Clear();

                    // change to AI mode view location
                    float scaleView = 14.0f;
                    Camera.main.transform.position = new Vector3(-2000 + scaleView * 16.9f, scaleView * 11.08f, scaleView * 16.9f);
                    Camera.main.transform.rotation = new Quaternion(0.085f, -.892f, 0.189f, 0.40f);

                }

        }

        /// <summary>
        /// 
        /// checks for opened popup panels or windows
        /// 
        /// </summary>
        void checkForActionsFromOpenedPopups()
        {

            // submit design to the central server was selected 
            if (GUIAssets.PopupButton.submit)
            {

                // deselect submit boolean and get the entered text
                GUIAssets.PopupButton.submit = false;
                designtag = GameObject.Find("InputTag").GetComponent<TMP_InputField>().text;

                // make sure they entered a value
                if (designtag != null)
                {
                    // first remove semicolons, because they are used to deliminate fields
                    designtag = designtag.Replace(";", "");

                    // test design tag for a valid input
                    bool emptytag = designtag.Equals("");
                    bool existingName = false;
                    bool tooLong = designtag.Length > 20;
                    foreach (Vehicle v in teamDesigns)
                    {
                        if (designtag.Equals(v.tag))
                            existingName = true;
                    }

                    // reset input tag to empty and debug info
                    GameObject.Find("InputTag").GetComponent<TMP_InputField>().text = "";
                    bottomLogString = "";

                    // if the design tag is valid 
                    if (!emptytag && !existingName && !tooLong)
                    {

                        try
                        {
                            
                            Vehicle vehicle = new Vehicle();
                            vehicle.tag = designtag;
                            vehicle.config = lastOutput.config;
                            vehicle.range = lastOutput.range;
                            vehicle.velocity = lastOutput.velocity;
                            vehicle.cost = lastOutput.cost;
                            vehicle.result = lastOutput.result;

                            int capacity = (int) double.Parse(vehicle.config.Split(',')[1]);
                            vehicle.payload = capacity;

                            // set the GUI capacity strings
                            capacityStr = capacity + "";
                            previousCapacityStr = capacityStr;

                            // submit the vehicle
                            DataInterface.SubmitVehicle(vehicle);

                            // log the submit action
                            Capture.Log("SubmitToDB;" + designtag + ";" + generatestring(), Capture.DESIGNER);

                            // reset to design mode view
                            ResetDesignModeView();

                        } catch (Exception e)
                        {
                            Debug.Log(e);
                        }

                    }
                    else if (existingName)
                    {
                        ShowMsg("Existing vehicle with the same name", true);
                        ResetDesignModeView();
                    }
                    else if (tooLong)
                    {
                        ShowMsg("Name is too long", true);
                        ResetDesignModeView();
                    }
                    else
                    {
                        ShowMsg("Error entering a tag name", true);
                        ResetDesignModeView();
                    } 
                }
            }

            // ok selected to reset to base design
            if (GUIAssets.PopupButton.ok && GUIAssets.PopupButton.storedData.Equals(RESETDESIGN))
            {
                // deselect the ok button
                GUIAssets.PopupButton.ok = false;
                GUIAssets.PopupButton.storedData = "";

                // reset the base design
                fromstring(BASEVEHICLECONFIG);
                Capture.Log("ResetDesign", Capture.DESIGNER);

                updateHistory(BASEVEHICLECONFIG);

                playClick();

            }

            // ok selected to open a new design
            if (GUIAssets.PopupButton.ok && GUIAssets.PopupButton.storedData.StartsWith(OPENDESIGN))
            {

                // vehicle information is stored using string with ; deliminator
                // should switch to storing this information as a vehicle and not a string
                string designtag = GUIAssets.PopupButton.storedData.Split(';')[1];
                string config = GUIAssets.PopupButton.storedData.Split(';')[2];
                string range = GUIAssets.PopupButton.storedData.Split(';')[3];
                string capacity = GUIAssets.PopupButton.storedData.Split(';')[4];
                string cost = GUIAssets.PopupButton.storedData.Split(';')[5];
                string velocity = GUIAssets.PopupButton.storedData.Split(';')[6];

                // deselect the ok button
                GUIAssets.PopupButton.ok = false;
                GUIAssets.PopupButton.storedData = "";

                // set vehicle configuration
                fromstring(config);

                Capture.Log("Opened;" + designtag + ";" + config, Capture.DESIGNER);
                ShowMsg("Opened : " + designtag + "\nRange = " + range + " mi\nCapacity = " + capacity + " lb\nCost = $" + cost + "\nVelocity = " + velocity + " mph", false);

                updateHistory(config);
                playClick();

            }

            // ok is selected for the user to return to the design mode after an evaluation
            if (GUIAssets.PopupButton.ok && GUIAssets.PopupButton.popupPanelID.Equals(POPUPCONFIRMEVALUATION))
            {

                showingEvaluationMode = false;
                evaluating = false;

                // deselect ok
                GUIAssets.PopupButton.ok = false;
                GUIAssets.PopupButton.storedData = "";

                ResetView();

            }

        }

        /// <summary>
        /// 
        /// parses the vehicle configuration to get the capacity value
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private int getCapacity(string config) 
        {
            return (int)double.Parse(config.Split(',')[1]);
        }

        /// <summary>
        /// 
        /// adds the configuration to the history list 
        /// 
        /// </summary>
        /// <param name="config"></param>
        private void updateHistory(string config)
        {
            for (int i = history.Count - 1; i > historyIndex; i--)
                history.RemoveAt(i);
            history.Add(config);
            historyIndex = history.Count - 1;
        }


        /// <summary>
        /// 
        /// generates a string representation of the current design
        /// 
        /// </summary>
        /// <returns></returns>
        private string generatestring()
        {

            try
            {

                string str = "";

                // components
                foreach (GameObject handle in jointGraph.Keys)
                {
                    JointInfo jointinfo = jointGraph[handle];
                    str += jointinfo.grammar();
                }

                // connections
                foreach (GameObject handle in connectionGraph.Keys)
                {
                    ConnectorInfo connectioninfo = connectionGraph[handle];
                    str += connectioninfo.grammar();
                }

                // original grammar allowed for different controllers
                // currently setting to custom or fixed 3
                return str + "," + capacityStr + ",3";

            }
            catch (Exception e)
            {
                ShowMsg(e.Message, true);
                fromstring(history[historyIndex]);
                Capture.Log(e.Message, Capture.DESIGNER);
                return history[historyIndex];
            }

        }

        /// <summary>
        /// 
        /// Generates a UAV design from a string. The entire vehicle is basically rotated 45 degrees,
        /// since existing drones have 45 degree connections. This way increments of 1 in the
        /// x and z connection correspond to a rotated frame of reference by 45 degress, where a vector
        /// of x,z = 1,1 represents the forward direction. This allowed for shorter string representations
        /// for the x and z positions for components.
        /// 
        ///              J K L M N O P 
        ///                    z
        /// 
        ///                    |        forward     P
        ///                    |                    O
        ///                    |                    N
        ///              - - - - - - -         x    M
        ///                    |                    L
        ///                    |                    K
        ///                    |                    J
        ///                    
        /// 
        /// example string : *aMM0+++++*bNM2+++*cMN1+++*dLM2+++*eML1+++^ab^ac^ad^ae,5,3
        /// 
        /// component : *bNM2+++ : b:node id, N : x position, M : z position, 2 component type, +++ size
        ///             component types = 0 : structure, 1 Motor CW, 2 : Motor CCW, 3 : Foil, 4 : empty  
        /// ^ab : edge : first character is the starting node id and the second character is the ending node id
        /// ,5,3 : capacity in pounds and the controller index
        /// 
        /// 
        /// </summary>
        /// <param name="str">string configuration of the vehicle</param>
        public void fromstring(string str)
        {

            try
            {

                // get node and edge tokens
                // ex. *aMM0+++++*bNM2+++*cMN1+++*dLM2+++*eML1+++^ab^ac^ad^ae,5,3
                // node tokens : split string by ^, get first token, then split by *
                // edge tokens : split string by , and get first token, then split by ^ and remove first token 
                string[] nodetokens = str.Split('^')[0].Split(new char[] {'*'}).Skip(1).ToArray();
                string[] edgetokens = str.Split(',')[0].Split('^').Skip(1).ToArray();

                // get capacity
                int capacity = getCapacity(str);
                capacityStr = capacity + "";
                previousCapacityStr = capacityStr;

                // store information about nodes in a dictionary
                Dictionary<string, JointInfo> nodes = new Dictionary<string, JointInfo>();
                foreach (string t in nodetokens)
                {
                    JointInfo jointInfo = new JointInfo(t);
                    nodes.Add(jointInfo.getNodeIDAsString(), new JointInfo(t));
                }

                // create the assembly sequence
                
                // maximum assembly connection index
                int maxConnectionStepIndex = 0;

                // dictionary that stores the ending node index of an edge with the string repesentation
                // of the edge, want to assemble the vehicle in edge ascending order
                Dictionary<int, string> sortedEdgeConnectionSteps = new Dictionary<int, string>();

                // since the assembly can be cyclic, these connections will be added to a separate list
                // and appended
                List<string> edgesEndAtExistingComponent = new List<string>();

                // for each edge
                foreach (string edgetoken in edgetokens)
                {

                    // get the index of the nodes at each end
                    //
                    // example ^ac : firstIndex = 0; secondIndex = 2
                    //
                    int secondIndex = JointInfo.getNodeIndexByChar(edgetoken[1]);
                    int firstIndex = JointInfo.getNodeIndexByChar(edgetoken[0]);

                    // store edge by second index
                    if (!sortedEdgeConnectionSteps.ContainsKey(secondIndex))
                        sortedEdgeConnectionSteps[secondIndex] = edgetoken;
                    else // ending index already exists, so must be a cycle
                        edgesEndAtExistingComponent.Add(edgetoken);

                    // set the maximum second index
                    maxConnectionStepIndex = System.Math.Max(maxConnectionStepIndex, secondIndex);

                }

                // if there are cycles, add to the end of the dictionary
                foreach (string edgeAtCylce in edgesEndAtExistingComponent)
                {
                    maxConnectionStepIndex += 1;
                    sortedEdgeConnectionSteps[maxConnectionStepIndex] = edgeAtCylce;
                }

                // reset to the base joint
                Initialize();

                // add all connections
                for (int i = 0; i < maxConnectionStepIndex + 1; i++)
                {
                    if (sortedEdgeConnectionSteps.ContainsKey(i))
                    {

                        // get the edge token, ex "ad"
                        string t = sortedEdgeConnectionSteps[i];

                        // the first character is the starting joint
                        // the second character is the ending joint
                        string start = "" + t[0];
                        string end = "" + t[1];

                        // get the joint information based on the input configuration string
                        JointInfo startnode = nodes[start];
                        JointInfo endnode = nodes[end];

                        // get the x and z positions for the starting and ending node
                        int xstart = (int) startnode.x;
                        int zstart = (int) startnode.z;
                        int xend = (int) endnode.x;
                        int zend = (int) endnode.z;

                        // get the starting node 
                        char startChar = start[0];

                        // if the starting node has a lower x and the same z , then we have a positive X
                        // connection, the remaining follows this same logic
                        if ((xstart - xend) == -1 && (zstart - zend) == 0)
                            assemble(startChar, POSITIVEX);
                        else if ((xstart - xend) == 1 && (zstart - zend) == 0)                 
                            assemble(startChar, NEGATIVEX);                     
                        else if ((xstart - xend) == 0 && (zstart - zend) == -1)
                            assemble(startChar, POSITIVEZ);
                        else if ((xstart - xend) == 0 && (zstart - zend) == 1)
                            assemble(startChar, NEGATIVEZ);
                        else
                            Debug.Log("Error in file " + t);

                        // check that the new joint index matches the configuration file, 
                        // deleting and adding of nodes changes the default ordering
                        // might be able to remove this
                        foreach (GameObject joint in jointGraph.Keys)
                        {

                            // if the joint is at the new connection ending point and it is not locked
                            if ((int)jointGraph[joint].x == xend 
                                && (int)jointGraph[joint].z == zend 
                                && !jointGraph[joint].locked) 
                            {
                                // check that the string representing the index in the configuration is the same joint index
                                if(!end.Equals("" + JointInfo.nodeIdChars[jointGraph[joint].index]))
                                {
                                    int index  = JointInfo.getNodeIndexByChar(end[0]);
                                    jointGraph[joint].index = index;
                                    JointInfo.counter = System.Math.Max(index + 1, JointInfo.counter);
                                }
                                jointGraph[joint].locked = true;
                            }

                        }

                    }

                }

                // add components at each joint
                foreach (string obj in nodes.Keys)
                {
                    JointInfo value = nodes[obj];
                    changeJointToComponent(getJointByChar(obj[0]), value.componentType);
                }

                // size components at each joint
                foreach (string obj in nodes.Keys)
                {
                    JointInfo value = nodes[obj];
                    sizeComponent(getJointByChar(obj[0]), value.sizedata);                   
                }

            }
            catch (Exception e)
            {
                Debug.Log(e);

                // try and reset to the base configuration if error
                if(!str.Equals(BASEVEHICLECONFIG))
                    fromstring(BASEVEHICLECONFIG);
            }

        }

        /// <summary>
        /// 
        /// gets the Unity game object joint by character , to support fromstring where 
        /// edges are identified by starting character and ending character
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private GameObject getJointByChar(char c)
        {
            GameObject selected = null;
            int index = JointInfo.getNodeIndexByChar(c);
            foreach (GameObject key in jointGraph.Keys)
            {
                if (jointGraph[key].index == index)
                    selected = key;
            }
            return selected;
        }

        /// <summary>
        /// 
        /// for a node, a connection is added to that node based on a positive 
        /// or negative x or z dir flag
        /// 
        /// </summary>
        /// <param name="jointid">the character of the joint</param>
        /// <param name="type">direction of the connection (x, X, z, Z)</param>
        private void assemble(char jointid, string type)
        {

            GameObject selected = getJointByChar(jointid);
            if (selected != null)
                for (int j = 0; j < selected.transform.childCount; j++)
                    // handle names are posx, negx, posz, negz
                    if (selected.transform.GetChild(j).gameObject.name.Equals(type))
                        if (selected.transform.GetChild(j).gameObject.activeSelf)
                        {
                            leftClickSelected(selected.transform.GetChild(j).gameObject);
                            return;
                        }
                    
            // assembly operation failed
            Debug.Log("Assembly operation failed for " + jointid + " " + type);

        }

        /// <summary>
        /// 
        /// shows a success message to the user
        /// 
        /// </summary>
        /// <param name="str">string message</param>
        public void ShowMsg(string str, bool error)
        {
            GameObject.Find(POPUPRESULTSPANEL).GetComponent<Canvas>().enabled = true;
            GameObject.Find(POPUPRESULTSSUCCESSBUTTON).GetComponent<Image>().enabled = true;
            GameObject.Find(POPUPRESULTSERRORBUTTON).GetComponent<Image>().enabled = error;
            GameObject.Find(POPUPRESULTSERRORTEXT).GetComponent<TextMeshProUGUI>().text = str;
        }

        /// <summary>
        /// 
        /// applies a shock cost based on the current market id
        /// 
        /// </summary>
        /// <param name="cost">vehicle cost in $</param>
        /// <returns></returns>
        public static double getShockCost(double cost)
        {
            return getShockCost(cost, RestWebService.market);
        }

        /// <summary>
        /// 
        /// applies a shock cost based on the session market id
        /// 
        /// </summary>
        /// <param name="cost">vehicle cost in $</param>
        /// <param name="market">market id</param>
        /// <returns></returns>
        public static double getShockCost(double cost, int market)
        {
            if (market == 1)
                return 1.0 * cost;
            else if (market == 2)
                return 0.7 * cost;
            return cost;
        }

        /// <summary>
        /// 
        /// gets a string representation of the vehicle evaluation results
        /// 
        /// </summary>
        /// <param name="pretext">initial text in the results string</param>
        /// <param name="delimiter">delimiter</param>
        /// <param name="range">range of vehicle in miles</param>
        /// <param name="capacity">capacity of vehicle in lb</param>
        /// <param name="cost">cost of vehicle in $</param>
        /// <param name="velocity">velocity of vehicle in mph</param>
        /// <param name="config">vehicle configuration string</param>
        /// <returns></returns>
        private string GetResults(string pretext, string delimiter, 
            double range, int capacity, double cost, double velocity, string config)
        {
            string result = pretext 
                + delimiter + "range(mi) = " + range.ToString("0.00") 
                + delimiter + "capacity(lb) = " + capacity 
                + delimiter + "cost($) = " + cost.ToString("0") 
                + delimiter + "velocity(mph) = " + velocity.ToString("0.00");
            if (config != null)
                result += delimiter + config;
            return result;
        }

        /// <summary>
        /// 
        /// selected AI configuration by a mouse click on the metric bars above
        /// each AI generated design
        /// 
        /// </summary>
        private void selectAIComponent()
        {

            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000.0f))
            {
                GameObject selected = hitInfo.transform.gameObject;

                // check in the Gameobject has an AI tag, which means that it is one of the metric bars
                // above the AI design
                if (selected.tag.Equals(AI))
                {

                    // the dictionary in the AI Helper will return the string configuration
                    // for the selected design
                    string config = aihelper.getSelected(selected);
                    if (config != null)
                    {

                        // get metrics
                        double[] metrics = aihelper.getMetrics(config);

                        // deactivate AI mode and remove all AI generated designs
                        aiMode = false;
                        aihelper.removeAIGeneratedUAVDisplays();

                        // resets the view and sets the vehicle configuration
                        ResetView();
                        fromstring(config);

                        // updates the history
                        updateHistory(config);

                        Capture.Log("SelectedAIDesign;" + generatestring() + 
                            ";range=" + metrics[0].ToString("0.00") +
                            ";cost=" + metrics[1].ToString("0") +
                            ";capacity=" + metrics[2].ToString("0.00"), Capture.DESIGNER);

                    }
                }
            }
        }

        /// <summary>
        /// runs the evaluation using the central server, still working on some issues 
        /// with this approach, specifically currently working on bugs with orientation 
        /// of vehicles in the trajectory data and allowing for multiple runs simulaneously
        /// </summary>
        //private void runServerEvaluation()
        //{
        //    Vehicle vehicle = new Vehicle();
        //    vehicle.config = generatestring();

        //    // set information strings
        //    ShowSuccessMsg("Evaluating ...");

        //    // send evaluation to server and log
        //    evaluating = true;
        //    DataInterface.EvaluateVehicle(vehicle);

        //}

        /// <summary>
        /// runs the vehicle evaluation within the scene
        /// </summary>
        private void runLocalEvaluation()
        {

            // reset variables for the analysis 
            lastOutput = new RestWebService.EvaluationOutput();
            lastOutput.trajectory = new List<Trajectory>();
            UavCollision.hit = false;

            // create the vehicle prototype using the current representation
            QuadrantMotorVehicleLayout prototype = new QuadrantMotorVehicleLayout();
            prototype.makePrototype(capacityStr);

            // make sure the prototype has a structure with a battery
            if(prototype.mainPrototypeStructure == null){
                ShowMsg("Invalid design", true);
                removePrototype();
                return;
            }

            // begin the simulation
            evaluating = true;

            // sets the time scale for a faster analysis
            Time.timeScale = 20.0f;
            Time.fixedDeltaTime = 0.02f;

            // creates the physics analysis code
            physics = new UAVPhysics(prototype);

            // resets the simulation time
            simTime = 0;

            // calculates the cost 
            lastOutput.cost = prototype.getCost();


        }

        /// <summary>
        /// physics for the local evaluation of vehicles
        /// </summary>
        private void localEvaluationPhysics()
        {

            // if there is a current evaluation
            if (physics != null)
            {

                // increment time and apply forces
                simTime += Time.fixedDeltaTime;
                physics.AddAutoPilot();
                physics.AddMotorAndFoilForce();

                // store trajectory and position data at specific intervals
                if ((simTime % sampleInterval) <= Time.fixedDeltaTime)
                {
                    Vector3 pos = physics.getPosition();
                    Quaternion ori = physics.getOrientation();
                    Trajectory trajectory = new Trajectory();
                    trajectory.time = simTime;
                    trajectory.position = new List<double>(new double[] { pos.x, pos.y - 1000, pos.z });
                    trajectory.orientation = new List<double>(new double[] { ori.x, ori.y, ori.z, ori.w });
                    lastOutput.trajectory.Add(trajectory);
                }
                // if an ending condition has occurred
                if (physics.analysisEnded)
                {

                    // remove the physical prototype
                    removePrototype();

                    // record the metrics
                    lastOutput.config = generatestring();
                    lastOutput.range = physics.range;
                    lastOutput.velocity = physics.velocity;
                    lastOutput.result = physics.resultMsg;

                    // done evaluating
                    evaluating = false;

                    // log data
                    string config = generatestring();
                    Capture.Log("Evaluated;" + config + ";range=" + lastOutput.range.ToString("0.00") + ";capacity=" 
                        + getCapacity(config) + ";cost=" + lastOutput.cost.ToString("0") + ";velocity=" 
                        + lastOutput.velocity.ToString("0.00"), Capture.DESIGNER);

                    // if a regular evaluation run
                    if (!aiRun)
                    {

                        // toggle the show evaluation mode
                        showingEvaluationMode = true;
                        
                        // draw the trajectory
                        Vector3 lastPosition = new Vector3(0, 0, 0);
                        foreach (RestWebService.Trajectory trajactory in lastOutput.trajectory)
                        {
                            Vector3 nextPosition = new Vector3((float)trajactory.position[0],
                                (float)trajactory.position[1],
                                (float)trajactory.position[2]);
                            CreateCylinderBetweenPoints(lastPosition, nextPosition, 0.8f, Color.white);
                            lastPosition = nextPosition;
                        }

                        // show the evaluation teststand and create a clone for the evaluation vehicle display 
                        ShowTestStand(true);
                        createEvaluationVehicle();

                        // set the view to behind the evaluation vehicle
                        ResetViewToEvaluationView();

                        

                        // show the results as a popup panel
                        if (physics.resultMsg.Contains("Success"))
                        {
                            successfulRun = true;
                            int capacity = getCapacity(lastOutput.config);
                            resultMessage = GetResults(lastOutput.result, "\n", lastOutput.range, capacity,
                                UAVDesigner.getShockCost(lastOutput.cost), lastOutput.velocity, null);
                            bottomLogString = GetResults("Last Run", " : ", lastOutput.range, capacity, UAVDesigner.getShockCost(lastOutput.cost), lastOutput.velocity, null); ;
                            ShowMsg("Showing Trajectory : " + resultMessage, false);
                        }
                        else
                        {
                            resultMessage = physics.resultMsg;
                            ShowMsg("Showing Trajectory ...", true);
                        }
                    }
                    else   // AI run
                    {

                        // use the metrics from the analysis run to a query close proximity designs
                        string s = generatestring();
                        float capacity = float.Parse(s.Split(',')[1]);

                        // call the designer AI to find designs based on the current evaluation metrics
                        aihelper.callWebServiceForAIDesigns((float)lastOutput.range, (float)lastOutput.cost, capacity, s);

                    }

                    // toggle the physics analysis off
                    physics = null;

                }
            }
        }

        /// <summary>
        /// 
        /// shows the test stand with a bottom plate and side walls for evaluation results
        /// 
        /// </summary>
        /// <param name="visible"></param>
        public void ShowTestStand(bool visible)
        {
            GameObject.Find("groundplate").GetComponent<MeshRenderer>().enabled = visible;
            GameObject.Find("leftside").GetComponent<MeshRenderer>().enabled = visible;
            GameObject.Find("rightside").GetComponent<MeshRenderer>().enabled = visible;
        }
        
        /// <summary>
        /// 
        /// removes the physical vehicle objects by removing all prototype 
        /// objects from the Unity scene
        /// 
        /// </summary>
        private void removePrototype()
        {

            Time.timeScale = 1.0f;

            GameObject[] objects = GameObject.FindGameObjectsWithTag(PROTOTYPESTRUCTURE);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPEWIDESTRUCTURE);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPENARROWSTRUCTURE);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPEMOTORCCW);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPEMOTORCW);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPECONNECTION);
            foreach (GameObject obj in objects)
                Destroy(obj);

            objects = GameObject.FindGameObjectsWithTag(PROTOTYPEFOIL);
            foreach (GameObject obj in objects)
                Destroy(obj);

        }

        /// <summary>
        /// creates a clone of the vehicle without mass properties 
        /// to display evalaution results
        /// </summary>
        private void createEvaluationVehicle()
        {

            // set the trajectory index to 0 for animation purposes
            evaluationTrajectoryIndex = 0;

            // create an empty object as the parent to assign the trajectory position 
            // and orientation
            GameObject parentEvaluationObject = new GameObject("EvaluationVehicle");
            parentEvaluationObject.tag = EVALUATIONDISPLAY;
            parentEvaluationObject.transform.rotation = Quaternion.Euler(0, 45, 0);

            // clone all vehicle components and set as children to the parent empty object
            GameObject[] vehicleObjs = GameObject.FindGameObjectsWithTag(VEHICLECOMPONENT);
            foreach (GameObject obj in vehicleObjs)
            {
                GameObject o = GameObject.Instantiate(obj);
                o.tag = EVALUATIONDISPLAY;
                o.transform.parent = parentEvaluationObject.transform;
            }
        }

        /// <summary>
        /// move the clone of a vehicle based on the evaluation results
        /// </summary>
        private void moveEvalautionVehicle()
        {

            // increment index
            evaluationTrajectoryIndex += 1;

            // remove evaluation vehicle after it reaches the end of the trajectory
            if (evaluationTrajectoryIndex >= lastOutput.trajectory.Count)
            {
                GameObject evaluationVehicle = GameObject.Find("EvaluationVehicle");
                if (evaluationVehicle != null)
                {
                    Destroy(GameObject.Find("EvaluationVehicle"));
                    ShowMsg(resultMessage, !resultMessage.Contains("Success"));
                }
                return;
            }
                
            // if reached the end, show the last position
            evaluationTrajectoryIndex = Math.Min(evaluationTrajectoryIndex, lastOutput.trajectory.Count - 1);

            // get and set position and orientation
            List<double> position = lastOutput.trajectory[evaluationTrajectoryIndex].position;
            List<double> orientation = lastOutput.trajectory[evaluationTrajectoryIndex].orientation;
            Vector3 nextPosition = new Vector3((float)position[0], (float)position[1], (float)position[2]);
            GameObject.Find("EvaluationVehicle").transform.position = nextPosition;
            Quaternion quarternion = new Quaternion((float)orientation[0], (float)orientation[1], (float)orientation[2], (float)orientation[3]);
            GameObject.Find("EvaluationVehicle").transform.rotation = quarternion;

            // follow with camera only for success, error can cause quick camera movement
            if (lastOutput.result.Contains("Success") || evaluationTrajectoryIndex < 2)
                Camera.main.transform.LookAt(nextPosition);


        }

        private string getMetaComponentData(GameObject obj)
        {
            string metaData = "";
            try
            {
                //float x = jointGraph[obj].x;
                //float z = jointGraph[obj].z;
                //metaData += JointInfo.getPositionChar((int)x);
                //metaData += JointInfo.getPositionChar((int)z);
            } catch (Exception e)
            {
                Debug.Log(e);
            }
            return metaData;
        }


    }


}