using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TouchHandler : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler {

    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {

    }

    private Vector2 startPanPos = Vector2.zero;

    static GameObject endpoint = null;

    public void OnPointerClick(PointerEventData data)
    {
        if( !data.dragging )
        {

        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        simpleGymCam.rotating = true;
        simpleGymCam.deltaX = 0;
        simpleGymCam.deltaY = 0;

        startPanPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 panOffset = eventData.position - this.startPanPos;
        startPanPos = eventData.position;

        // Camera Rotating
        simpleGymCam.deltaX = panOffset.x;
        simpleGymCam.deltaY = panOffset.y;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        simpleGymCam.rotating = false;
    }


}
