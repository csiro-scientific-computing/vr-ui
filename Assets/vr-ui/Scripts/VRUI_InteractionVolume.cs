using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct InteractionInfo
{
    public GameObject Instigator;
}

[Serializable]
public class InteractionEvent : UnityEvent<GameObject> { }

[RequireComponent(typeof(Collider))]
public class VRUI_InteractionVolume : MonoBehaviour
{
    /* =============== Interactable Variables ============== */

    // -- Activation
    public VRUI_ViveController.VRUI_ViveController_Buttons ActivationButton;    //This needs to be abstracted somehow...

    // -- Volume Physics
    public ElasticSurface VolumePhysics;

    // -- Components
    protected Collider volumeCollider;
    protected Renderer volumeRenderer;

    // -- Interacting
    protected GameObject InteractingGO;
    protected VRUI_Controller InteractingController;
    public bool IsTriggering = false;
    public bool WasActivating = false;

    // -- Events
    public InteractionEvent VolumeHoverBegin = new InteractionEvent();
    public InteractionEvent VolumeHovering = new InteractionEvent();
    public InteractionEvent VolumeHoverEnd = new InteractionEvent();

    public InteractionEvent VolumeActivateBegin = new InteractionEvent();
    public InteractionEvent VolumeActivating = new InteractionEvent();
    public InteractionEvent VolumeActivateEnd = new InteractionEvent();

    /* ============== Virtual Event Functions ============== */

    protected virtual void HoverBegin() { if (VolumeHoverBegin != null) VolumeHoverBegin.Invoke(InteractingGO); }
    protected virtual void Hovering() { if (VolumeHovering != null) VolumeHovering.Invoke(InteractingGO); }
    protected virtual void HoverEnd() { if (VolumeHoverEnd != null) VolumeHoverEnd.Invoke(InteractingGO); }

    protected virtual void ActivateBegin() { if (VolumeActivateBegin != null) VolumeActivateBegin.Invoke(InteractingGO); }
    protected virtual void Activating() { if (VolumeActivating != null) VolumeActivating.Invoke(InteractingGO); }
    protected virtual void ActivateEnd() { if (VolumeActivateEnd != null) VolumeActivateEnd.Invoke(InteractingGO); }

    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Links components on GameObject Awake
    /// </summary>
    protected virtual void Awake()
    {
        LinkComponents();
    }

    /// <summary>
    /// Virtual Start Function
    /// </summary>
    protected virtual void Start() { }

    /// <summary>
    /// Provides renderer with constant position of interacting cursor
    /// </summary>
    protected virtual void FixedUpdate()
    {
        volumeRenderer.material.SetVector("_CursorPosition", VolumePhysics.GetPosition());
    }

    /// <summary>
    /// Handles calling of interaction events and simulating surface
    /// </summary>
    protected virtual void Update()
    {
        if (IsTriggering)
        {
            if (IsActivating())
            {
                if (!WasActivating)
                    ActivateBegin();
                else
                    Activating();
                WasActivating = true;
            }
            else
            {
                if (WasActivating)
                {
                    ActivateEnd();
                }
                else
                    Hovering();
                WasActivating = false;
            }
        }
        else
        {
            if (WasActivating)
            {
                ActivateEnd();

                InteractingController = null;
                InteractingGO = null;
            }
            VolumePhysics.Simulate(Time.deltaTime);
        }
    }

    /* ================== Helper Functions ================= */

    /// <summary>
    /// Helper function which returns true if interacting controller is pressing activation button
    /// </summary>
    /// <returns>If controller is activating the volume</returns>
    bool IsActivating()
    {
        if (InteractingController)
            return InteractingController.GetButtonState(0, (int)ActivationButton);
        else
            return false;
    }

    /// <summary>
    /// Link all necessary internal components 
    /// </summary>
    void LinkComponents()
    {
        volumeCollider = GetComponent<Collider>();
        volumeRenderer = GetComponent<Renderer>();
    }

    /* ===================== Unity Events ================== */

    protected virtual void OnTriggerEnter(Collider other)
    {
        //Check if collider was the cursor
        if (!other.gameObject.CompareTag("VRUI-Cursor"))
            return;

        IsTriggering = true;

        InteractingGO = other.gameObject;

        if (!InteractingGO)
            return;

        InteractingController = InteractingGO.GetComponentInParent<VRUI_Controller>();
        if (InteractingController == null)
            InteractingController = InteractingGO.GetComponent<VRUI_Controller>();


        if (IsActivating())
            ActivateBegin();
        else
            HoverBegin();
    }

    protected virtual void OnTriggerStay(Collider other)
    {

    }

    protected virtual void OnTriggerExit(Collider other)
    {
        //Check if collider was the cursor
        if (!other.gameObject.CompareTag("VRUI-Cursor"))
            return;

        if (IsActivating())
            return;

        HoverEnd();

        InteractingController = null;

        InteractingGO = null;

        IsTriggering = false;
    }
}
