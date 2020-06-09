using UnityEngine;

namespace Assets
{

    /// <summary>
    /// Helper class for GUI images, color styles, and text rotation
    /// </summary>
    class GUIHelpers
    {

        public static GUIStyle whiteStyle;
        public static GUIStyle lightgrayStyle;
        public static GUIStyle grayStyle;
        public static GUIStyle darkgrayStyle;
        public static GUIStyle selectedStyle;
        public static GUIStyle unselectedStyle;
        public static GUIStyle greenStyle;
        public static GUIStyle redStyle;
        public static GUIStyle darkgreenStyle;
        public static GUIStyle darkredStyle;
        public static GUIStyle transparentBlack;
        public static GUIStyle grayRedStyle;

        public static Texture2D copyimage;
        public static Texture2D removeimage;
        public static Texture2D undoimage;
        public static Texture2D redoimage;
        public static Texture2D resetdesignimage;
        public static Texture2D toggleviewimage;
        public static Texture2D toggleperspectiveimage;
        public static Texture2D resetviewimage;
        public static Texture2D aiimage;
        public static Texture2D dbloadimage;
        public static Texture2D unselectedplan;
        public static Texture2D selectedplan;
        public static Texture2D losingplan;
        public static Texture2D dashboardimage;
        public static Texture2D infoimage;
        public static Texture2D tagimage;

        public static Color REDHOUSE = new Color(1.000f, 0.397f, 0.397f, 1.000f);
        public static Color YELLOWHOUSE = new Color(0.697f, 0.725f, 0.314f, 1.000f);

        /// <summary>
        /// loads toolbar icons
        /// </summary>
        public static void LoadImageIcons()
        {
            // load toolbar images
            copyimage = Resources.Load("copy") as Texture2D;
            removeimage = Resources.Load("trash") as Texture2D;
            undoimage = Resources.Load("undo") as Texture2D;
            redoimage = Resources.Load("redo") as Texture2D;
            resetdesignimage = Resources.Load("reset") as Texture2D;
            toggleviewimage = Resources.Load("orth") as Texture2D;
            resetviewimage = Resources.Load("view") as Texture2D;
            toggleperspectiveimage = Resources.Load("perspective") as Texture2D;
            aiimage = Resources.Load("agent") as Texture2D;
            dbloadimage = Resources.Load("import") as Texture2D;
            unselectedplan = Resources.Load("unselected") as Texture2D;
            selectedplan = Resources.Load("selected") as Texture2D;
            losingplan = Resources.Load("losing") as Texture2D;
            dashboardimage = Resources.Load("bill") as Texture2D;
            infoimage = Resources.Load("info") as Texture2D;
            tagimage = Resources.Load("tag") as Texture2D;
        }

        /// <summary>
        /// initializes the styles, this method can only run in 
        /// a Unity onGUI method so this is why it is a separate method
        /// from above using null checks
        /// </summary>
        public static void InitStyles()
        {

            if (whiteStyle == null)
            {
                whiteStyle = new GUIStyle(GUI.skin.box);
                whiteStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.5f));
                whiteStyle.border.bottom = 0;
                whiteStyle.border.top = 0;
                whiteStyle.border.left = 0;
                whiteStyle.border.right = 0;
            }
            if (lightgrayStyle == null)
            {
                lightgrayStyle = new GUIStyle(GUI.skin.box);
                lightgrayStyle.normal.background = MakeTex(2, 2, new Color(0.8f, 0.8f, 0.8f, 0.5f));
                lightgrayStyle.border.bottom = 0;
                lightgrayStyle.border.top = 0;
                lightgrayStyle.border.left = 0;
                lightgrayStyle.border.right = 0;
            }
            if (grayStyle == null)
            {
                grayStyle = new GUIStyle(GUI.skin.box);
                grayStyle.normal.background = MakeTex(2, 2, new Color(0.6f, 0.6f, 0.6f, 0.5f));
                grayStyle.border.bottom = 0;
                grayStyle.border.top = 0;
                grayStyle.border.left = 0;
                grayStyle.border.right = 0;
            }
            if (darkgrayStyle == null)
            {
                darkgrayStyle = new GUIStyle(GUI.skin.box);
                darkgrayStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 0.5f));
                darkgrayStyle.border.bottom = 0;
                darkgrayStyle.border.top = 0;
                darkgrayStyle.border.left = 0;
                darkgrayStyle.border.right = 0;
            }
            if (selectedStyle == null)
            {
                selectedStyle = new GUIStyle(GUI.skin.box);
                selectedStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 1.0f, 0.0f, 0.5f));
            }
            if (unselectedStyle == null)
            {
                unselectedStyle = new GUIStyle(GUI.skin.box);
                unselectedStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 1.0f, 0.0f, 0.0f));
            }
            if (greenStyle == null)
            {
                greenStyle = new GUIStyle(GUI.skin.box);
                greenStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 1.0f, 0.0f, 1.0f));
                greenStyle.border.bottom = 0;
                greenStyle.border.top = 0;
                greenStyle.border.left = 0;
                greenStyle.border.right = 0;
            }
            if (darkgreenStyle == null)
            {
                darkgreenStyle = new GUIStyle(GUI.skin.box);
                darkgreenStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 0.5f, 0.0f, 1.0f));
                darkgreenStyle.border.bottom = 0;
                darkgreenStyle.border.top = 0;
                darkgreenStyle.border.left = 0;
                darkgreenStyle.border.right = 0;
            }
            if (darkredStyle == null)
            {
                darkredStyle = new GUIStyle(GUI.skin.box);
                darkredStyle.normal.background = MakeTex(2, 2, new Color(0.5f, 0.0f, 0.0f, 1.0f));
                darkredStyle.border.bottom = 0;
                darkredStyle.border.top = 0;
                darkredStyle.border.left = 0;
                darkredStyle.border.right = 0;
            }
            if (redStyle == null)
            {
                redStyle = new GUIStyle(GUI.skin.box);
                redStyle.normal.background = MakeTex(2, 2, new Color(1.0f, 0.0f, 0.0f, 1.0f));
            }
            if (transparentBlack == null)
            {
                transparentBlack = new GUIStyle(GUI.skin.box);
                transparentBlack.normal.textColor = Color.white;
                transparentBlack.normal.background = MakeTex(2, 2, new Color(0.0f, 0.0f, 0.0f, 0.5f));
            }
            if (grayRedStyle == null)
            {
                grayRedStyle = new GUIStyle(GUI.skin.box);
                grayRedStyle.normal.background = MakeTex(2, 2, new Color(0.7f, 0.6f, 0.6f, 0.5f));
                grayRedStyle.border.bottom = 0;
                grayRedStyle.border.top = 0;
                grayRedStyle.border.left = 0;
                grayRedStyle.border.right = 0;
            }
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// flips text in Unity scenes at 180 degree intervals to keep 
        /// it positioned towards the user
        /// </summary>
        /// <param name="textMesh">Unity textmesh object</param>
        public static void flipText(TextMesh textMesh)
        {
            Vector3 objectNormal = textMesh.transform.rotation * Vector3.forward;
            Vector3 cameraToText = textMesh.transform.position - Camera.main.transform.position;
            float f = Vector3.Dot(objectNormal, cameraToText);
            if (f < 0f)
            {
                textMesh.transform.Rotate(new Vector3(0, 1, 0), 180);
            }
        }

        /// <summary>
        /// aligns the text to point directly at the camera in the Unity scene.
        /// 
        /// use this for size and weight labels
        /// </summary>
        /// <param name="textMesh"></param>
        public static void alignText(TextMesh textMesh)
        {
            textMesh.transform.rotation = Quaternion.LookRotation(textMesh.transform.position - Camera.main.transform.position);
        }

    }

}
