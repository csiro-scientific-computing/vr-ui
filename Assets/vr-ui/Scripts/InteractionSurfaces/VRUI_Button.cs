using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class VRUI_Button : VRUI_InteractionSurface
{

    // -- Button Types
    public enum VRUI_Button_Type
    {
        Trigger,
        Toggle
    }

    /* ================== Button Variables ================= */

    [Header("Button Events")]
    public UnityEvent BTN_State_Pressed;
    public UnityEvent BTN_State_Released;

    // -- Button Variables
    [Header("Button Properties")]
    public VRUI_Button_Type ButtonType;

    // -- Toggle state of button
    protected bool ToggleState;

    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Override of Interactable Start function
    /// </summary>
    protected override void Start()
    {
        //Call base Start function
        base.Start();

        //Sends shader value based on button type
        switch (ButtonType)
        {
            case VRUI_Button_Type.Trigger:
                surfaceRenderer.material.SetInt("_ButtonType", 0);
                break;
            case VRUI_Button_Type.Toggle:
                surfaceRenderer.material.SetInt("_ButtonType", 1);
                break;
        }
    }

    /* ================= Overriden Functions =============== */

    /// <summary>
    /// Called on beginning activated
    /// </summary>
    protected override void BeginActivate()
    {
        base.BeginActivate();
        //Set interactable state value to 1, it's active
        Value = 1.0f;

        if (ButtonType == VRUI_Button_Type.Toggle)
        {
            if(!ToggleState)
                BTN_State_Pressed.Invoke();
            else
                BTN_State_Released.Invoke();
        } else
        {
            BTN_State_Pressed.Invoke();
        }
        
    }

    /// <summary>
    /// Called on end activation
    /// </summary>
    protected override void EndActivate()
    {
        base.EndActivate();

        //If toggle button, then keep activated until next toggle
        if (ButtonType == VRUI_Button_Type.Toggle)
        {
            ToggleState = !ToggleState;
            surfaceRenderer.material.SetInt("_ToggleState", (ToggleState) ? 1 : 0);
            Value = (ToggleState) ? 1 : 0;
        }
        else
        {
            Value = 0.0f;
            BTN_State_Released.Invoke();
        }
    }

}
