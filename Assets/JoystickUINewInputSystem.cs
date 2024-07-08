using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class JoystickUINewInputSystem : MonoBehaviour
{
    public InputActionAsset inputActionAsset;
    [FormerlySerializedAs("activationActionName")] public string touchActionName;
    [FormerlySerializedAs("touchPositionAction")] public string touchPositionActionName;
    [FormerlySerializedAs("moveActionName")] public string touchMoveActionName;

    [SerializeField][Tooltip("Maximum range for the joystick knob.")]private float maximumRange = 1f;
    [FormerlySerializedAs("frane")] [FormerlySerializedAs("center")] [SerializeField][Tooltip("Center of the joystick. This will not move.")]private RectTransform frame;
    [SerializeField][Tooltip("Knob of the joystick. This transform moves with the finger in the screen.")] private RectTransform knob;

    private InputAction moveAction;
    private InputAction positionAction;
    
    // Start is called before the first frame update
    void Start()
    {
        moveAction = inputActionAsset.FindAction(touchMoveActionName);
        moveAction.Enable();
        
        var activationAction = inputActionAsset.FindAction(touchActionName);
        activationAction.canceled += OnMovementEnd;
        activationAction.started += OnMovementStart;
        activationAction.Enable();
        
        positionAction = inputActionAsset.FindAction(touchPositionActionName);
        positionAction.Enable();
        
        ShowJoystick(false);
    }

    private void OnMovementStart(InputAction.CallbackContext obj)
    {
        ShowJoystick(true);
        moveAction.performed += OnMovement;
        SetFramePosition(positionAction.ReadValue<Vector2>());
    }
    
    private void OnMovement(InputAction.CallbackContext obj)
    {
        var delta= obj.ReadValue<Vector2>();
        SetKnobPosition(delta);

    }

    private void OnMovementEnd(InputAction.CallbackContext obj)
    {
        SetKnobPosition(Vector2.zero);
        ShowJoystick(false);
        moveAction.performed -= OnMovement;
    }
    
    private void SetKnobPosition(Vector2 delta)
    {
        var knobPosition = knob.anchoredPosition;
        knobPosition += delta;
        knobPosition = Vector2.ClampMagnitude(knobPosition, maximumRange);
        knob.anchoredPosition = knobPosition;
    }

    
    /// <summary>
    /// Allows to activate or deactivate the joystick.
    /// </summary>
    /// <param name="activate"></param>
    private void ShowJoystick(bool activate)
    {
        knob.gameObject.SetActive(activate);
        frame.gameObject.SetActive(activate);
    }
    
    private void SetFramePosition(Vector2 position)
    {
        frame.anchoredPosition = position;
        // Debug.Log($"Set frame position to {position}");
    }


  
    
}
