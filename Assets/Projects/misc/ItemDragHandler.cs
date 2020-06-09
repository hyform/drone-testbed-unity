using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Used to support dragging of UAV components from the designer toolbar
/// to the vehicle joints
/// </summary>
public class ItemDragHandler : MonoBehaviour, IDragHandler, IEndDragHandler
{

    /// <summary>
    /// selected component type
    /// </summary>
    public int componentType = 0;

    /// <summary>
    /// initial drag position
    /// </summary>
    private Vector3 initialDragPosition = new Vector3(-1, -1, -1);

    /// <summary>
    /// event called mouse drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (initialDragPosition == new Vector3(-1, -1, -1))
            initialDragPosition = transform.position;
        transform.position = Input.mousePosition;
    }

    /// <summary>
    /// event called at end of drag
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = initialDragPosition;
        GameObject.Find("GUI").GetComponent<DesignerAssets.UAVDesigner>().dropComponent(Input.mousePosition, componentType);
        initialDragPosition = new Vector3(-1, -1, -1);
    }

    // Unity Start method
    void Start(){}

    // Unity OnGUI method
    void Update(){}

}
