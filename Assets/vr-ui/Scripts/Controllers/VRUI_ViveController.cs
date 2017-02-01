using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve;
using Valve.VR;

[RequireComponent(typeof(SteamVR_TrackedObject))]
public class VRUI_ViveController : VRUI_Controller
{
    //Attached steam vr controller component
    SteamVR_TrackedObject controller;

    [System.Serializable]
    public enum VRUI_ViveController_Buttons
    {
        TriggerClick = EVRButtonId.k_EButton_SteamVR_Trigger,
        TriggerAnalog = EVRButtonId.k_EButton_Max - 1,
        Grip = EVRButtonId.k_EButton_Grip,
        TouchpadClick = EVRButtonId.k_EButton_SteamVR_Touchpad,
        TouchpadTouch = EVRButtonId.k_EButton_Max - 2,
        Menu = EVRButtonId.k_EButton_ApplicationMenu,
        Steam = EVRButtonId.k_EButton_System
    }

    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Links internal components
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<SteamVR_TrackedObject>();
    }

    /// <summary>
    /// Overide of base Start function, gets attached Steam Controller component
    /// </summary>
    protected override void Start()
    {
        base.Start();
        ControllerIndex = (int)controller.index;

    }

    /// <summary>
    /// Used to retrieve the controllers current angular and physical velocity
    /// </summary>
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        ControllerVelocity = SteamVR_Controller.Input((int)controller.index).velocity;
        ControllerAngularVelocity = SteamVR_Controller.Input((int)controller.index).angularVelocity;
    }

    /* =========== Controller Specific Integration ========= */

    /// <summary>
    /// Vive specific implementation of TriggerHapticFeedback
    /// </summary>
    /// <param name="strength">Value between 0 and 1</param>
    /// <param name="length">Length in seconds</param>
    public override void TriggerHapticFeedback(float strength, float length)
    {
        base.TriggerHapticFeedback(strength, length);
    }

    /// <summary>
    /// Vive specific implementation of TriggerHapticPulse
    /// </summary>
    /// <param name="strength">Value between 0 and 1</param>
    public override void TriggerHapticPulse(float strength)
    {
        base.TriggerHapticPulse(strength);
        SteamVR_Controller.Input((int)controller.index).TriggerHapticPulse((ushort)Mathf.Lerp(0.0f, 3999.0f, strength));
    }

    /// <summary>
    /// Gets button state for Vive Controller
    /// </summary>
    /// <param name="controllerIndex">HTC Vive Controller Index</param>
    /// <param name="buttonIndex">HTC Vive Button index</param>
    /// <returns>State of specified button</returns>
    public override bool GetButtonState(uint controllerIndex, int buttonIndex)
    {
        var system = OpenVR.System;
        if (system == null)
            return false;

        VRControllerState_t controllerState = new VRControllerState_t();

        bool state = false;

        if (system.GetControllerState((uint)controller.index, ref controllerState, (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VRControllerState_t))))
        {
            ulong btn = controllerState.ulButtonPressed & (1UL << (buttonIndex));

            if (btn > 0L)
                state = true;
            else if (btn == 0L)
                state = false;
        }

        return state;
    }

}
