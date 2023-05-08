using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Transform _Camera;
    [SerializeField] private Vector2 _MinMaxXRotation = new Vector2(-90f, 90f);
    [SerializeField] private Vector2 _MinMaxZPosition = new Vector2(-300f, -70f);

    private float _TouchDistance; 
        
    // Update is called once per frame
    void Update()
    {
        //If left mouse click
        if (Input.GetMouseButton(0))
        {
            //Rotate camera around origin based on mouse movement
            var currentRotation = transform.rotation.eulerAngles;
            var currentXRotation = currentRotation.x;
            if (currentXRotation > _MinMaxXRotation.y)
            {
                currentXRotation -= 360f;
            }
            
            var newXRotation = Mathf.Clamp(currentXRotation - Input.GetAxis("Mouse Y"), _MinMaxXRotation.x, _MinMaxXRotation.y);
            var newYRotation = currentRotation.y + Input.GetAxis("Mouse X");
            transform.rotation = Quaternion.Euler(newXRotation, newYRotation, currentRotation.z);
        }
        
        if (Input.touchCount >= 2)
        {
            Vector2 touch0, touch1;
            touch0 = Input.GetTouch(0).position;
            touch1 = Input.GetTouch(1).position;
            if (_TouchDistance == 0)
            {
                _TouchDistance = Vector2.Distance(touch0, touch1);
            }
            else
            {
                var currentZPosition = _Camera.localPosition.z;
                var newZPosition = Mathf.Clamp(currentZPosition + Vector2.Distance(touch0, touch1) - _TouchDistance,
                    _MinMaxZPosition.x, _MinMaxZPosition.y);
                _TouchDistance = Vector2.Distance(touch0, touch1);
                _Camera.localPosition = new Vector3(0, 0, newZPosition);
            }
        }
        else
        {
            _TouchDistance = 0f;
        }

        if (Input.mouseScrollDelta.magnitude > 0)
        {
            var currentZPosition = _Camera.localPosition.z;
            var newZPosition = Mathf.Clamp(currentZPosition + Input.mouseScrollDelta.y *10f, _MinMaxZPosition.x, _MinMaxZPosition.y);
            _Camera.localPosition = new Vector3(0, 0, newZPosition);
        }
    }
}