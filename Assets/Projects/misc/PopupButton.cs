using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

namespace GUIAssets
{
    /// <summary>
    /// Script attached to buttons in popup panels
    /// </summary>
    public class PopupButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        /// <summary>
        /// the Unity panel button
        /// </summary>
        private Button myButton;

        /// <summary>
        /// sets the id of the popup panel
        /// </summary>
        public static string popupPanelID = "";

        /// <summary>
        /// stores meta data for the popup
        /// </summary>
        public static string storedData = "";

        /// <summary>
        /// flag that identifies if it is showing
        /// </summary>
        public static bool showing = false;

        /// <summary>
        /// ok is selected
        /// </summary>
        public static bool ok = false;

        /// <summary>
        /// submit button is selected
        /// </summary>
        public static bool submit = false;

        /// <summary>
        /// submit scenario button is selected
        /// </summary>
        public static bool submitScenario = false;

        /// <summary>
        /// flag to identify in the designer AI view if camera should be rotated left or right
        /// </summary>
        private int designerAIRotate = 0;

        /// <summary>
        /// Unity Start method that initializes components
        /// </summary>
        void Start()
        {
            myButton = GetComponent<Button>();
            myButton.onClick.AddListener(TaskOnClick);
        }

        /// <summary>
        /// Unity update method that initializes components
        /// </summary>
        void Update()
        {
            if(designerAIRotate == 1)
                Camera.main.transform.RotateAround(new Vector3(-2000, 0, 0), new Vector3(0, 1, 0), -1);
            else if (designerAIRotate == 2)
                Camera.main.transform.RotateAround(new Vector3(-2000, 0, 0), new Vector3(0, 1, 0), 1);
        }

        /// <summary>
        /// event fired when users clicks a button
        /// </summary>
        void TaskOnClick()
        {

            // label names are entered in the Unity GUI
            if (myButton.name.StartsWith("Submit"))
            {
                Debug.Log("Submit");
                GameObject.Find(popupPanelID).GetComponent<Canvas>().enabled = false;
                submit = true;
                showing = false;
            }
            else if (myButton.name.StartsWith("Yes"))
            {
                Debug.Log("Yes");
                GameObject.Find(popupPanelID).GetComponent<Canvas>().enabled = false;
                submitScenario = true;
                showing = false;
            }
            else if (myButton.name.StartsWith("OK"))
            {
                Debug.Log("OK");
                GameObject.Find(popupPanelID).GetComponent<Canvas>().enabled = false;
                ok = true;
                showing = false;
            }
            else if (myButton.name.StartsWith("No"))
            {
                Debug.Log("No");
                GameObject.Find(popupPanelID).GetComponent<Canvas>().enabled = false;
                showing = false;
            }
            else if (myButton.name.StartsWith("Cancel"))
            {
                Debug.Log("Cancel");
                GameObject.Find(popupPanelID).GetComponent<Canvas>().enabled = false;
                showing = false;
                storedData = "";
                popupPanelID = "";
            }

        }

        /// <summary>
        /// event fired when a user holds down the button
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (myButton.name.StartsWith("LeftAI"))
            {
                designerAIRotate = 1;
            }
            else if (myButton.name.StartsWith("RightAI"))
            {
                designerAIRotate = 2;
            }
        }

        /// <summary>
        /// event fired when a user releases down the button
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerUp(PointerEventData eventData)
        {
            designerAIRotate = 0;
        }


    }
}
