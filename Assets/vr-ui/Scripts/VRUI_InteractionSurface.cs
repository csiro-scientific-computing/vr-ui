using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interactable Unity Event which takes in a float which will be the value of 0 to 1 of the interaction state
/// </summary>
[System.Serializable]
public class InteractionSurfaceEvent : UnityEvent<float> { }

public delegate void ValueChangedHandler(object sender, float value);

[Serializable]
[RequireComponent(typeof(BoxCollider))]
public class VRUI_InteractionSurface : MonoBehaviour
{

    /// <summary>
    /// Enum of every state an interactable can be
    /// </summary>
    public enum VRUI_Interactable_State
    {
        Idle,
        Hovering,
        Activated,
        Released
    }

    /// <summary>
    /// Struct describing the meta data of a state. Each VRUI_Interactable_State should have a corresponding VRUI_State_Data available
    /// </summary>
    [System.Serializable]
    public struct VRUI_State_Data
    {
        //Visual Feedback
        public Color StateColor;                        //Color to apply visually while state is active

        public float HapticPulse;                       //Haptic to play on change to state
        public float HapticAmount;                      //Haptic amount to play while state is active

        //Events
        public InteractionSurfaceEvent BeginStateEvent;       //Trigered on start of state
        public InteractionSurfaceEvent ContinuousStateEvent;  //Triggered on holding of state
        public InteractionSurfaceEvent EndStateEvent;         //Triggerd at end of state
    }

    /* =============== Interactable Variables ============== */

    // -- Interactable Components
    public BoxCollider interactionTrigger;
    public Renderer surfaceRenderer;

    // -- Interactable State Data
    [Header("States")]
    protected VRUI_Interactable_State currentState;

    public VRUI_State_Data IdleState = new VRUI_State_Data();
    public VRUI_State_Data HoveringState = new VRUI_State_Data();
    public VRUI_State_Data ActivatingState = new VRUI_State_Data();
    public VRUI_State_Data ReleasedState = new VRUI_State_Data();

    // -- Events
    public event ValueChangedHandler ValueChangedEvent;

    [Header("Surface Icon")]
    public Texture InteractableIcon;
    [Range(0.0f, 1.0f)]
    public float IconScale = 0.5f;

    [Header("Metadata")]

    // -- Tooltip
    public string Tooltip;

    // -- Interaction Variables
    protected GameObject Cursor;                        //Object that is interacting with interactable
    protected VRUI_Controller CursorController;         //Reference to controller component of current cursor
    protected Vector3 CursorLocalTrigerPosition;        //Cursor's position in local space of the interactable trigger
    protected Vector3 CursorLocalRendererPosition;      //Cursor's position in local space of the mesh of the interactable

    protected Vector3 CursorSurfaceVelocity;            //Velocity of interactable nib
    protected Vector3 CursorSurfacePosition;            //Position of interactable nib

    // -- Surface Properties
    [Header("Interaction")]

    // -- Surface Physics
    public ElasticSurface SurfacePhysics;
    public float SurfaceFalloff = 0.9f;                     //Falloff of the cursors interaction with the surface

    // -- Interaction Settings
    float currentInteractionValue = 0.0f;               //Cursors amount of interaction with the surface (0-1)
    public float InteractionThreshold = 0.5f;           //Threshold of interaction before triggering the activated state
    public float TriggerMargin = 0.2f;

    [Header("Visual Addons")]
    public bool ShowThreshold = true;                   //Should show visual representation of threshold amount
    public GameObject ThresholdVis;                     //Optional gameobject to show at threshold location. If null, will create default

    // -- Axis locking
    [Tooltip("Locks certain axis from moving")]
    public Vector3 CursorAxisInfluence = new Vector3(1.0f, 1.0f, 1.0f); //Axis influence value the cursor

    [Header("Events")]
    public InteractionSurfaceEvent ValueChanged;

    [Header("Data")]
    public float Value = 0.0f;           //Common value of interactable state (Button will be 0 off, 1 on. Slider will be 0-1 range of slider)

    /* ================== Unity Pipeline =================== */

    /// <summary>
    /// Link this component to other components needed
    /// </summary>
    protected virtual void Awake()
    {
        LinkComponents();

        AdjustTriggerBounds();
    }

    /// <summary>
    /// Virtual Unity Start function, set up shader properties, and display visualizers
    /// </summary>
    protected virtual void Start()
    {
        SetupShader(surfaceRenderer);
        SetupVisualizers();
        if (ValueChangedEvent != null) ValueChangedEvent(this, Value);
        if (ValueChanged != null) ValueChanged.Invoke(Value);
    }

    /// <summary>
    /// Virtual Unity Update function.
    /// </summary>
    protected virtual void Update()
    {

        HandleStateChange();

        //If there is a cursor
        if (Cursor)
        {
            //Cursor position in mesh space
            CursorLocalRendererPosition = CalcLocalCursorPosition(surfaceRenderer.transform);
            //Cursor position in trigger space
            CursorLocalTrigerPosition = CalcLocalCursorPosition(transform);

            //Calculate interaction value
            currentInteractionValue = CalcInteractionValue();

            CallContinuousEvents();

        }

        CalcSurfacePhysics();
    }

    protected virtual void FixedUpdate()
    {
        if (surfaceRenderer)
        {
            surfaceRenderer.material.SetVector("_CursorPos", SurfacePhysics.GetPosition());
            surfaceRenderer.material.SetFloat("_ButtonFalloff", SurfaceFalloff * ((currentState == VRUI_Interactable_State.Activated) ? 0.5f : 1.0f));
        }
    }

    /* ======================= Setup ======================= */

    /// <summary>
    /// Show interactable visualizers, primarily the threshold limit
    /// </summary>
    private void SetupVisualizers()
    {
        if (ShowThreshold)
        {
            if (ThresholdVis == null)
            {
                ThresholdVis = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Material mat = new Material(Shader.Find("VRUI/Visualizers"));
                ThresholdVis.GetComponent<Renderer>().material = mat;
                mat.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
                ThresholdVis.transform.SetParent(transform, false);
            }
            else
            {
                ThresholdVis = Instantiate(ThresholdVis, transform, false);
            }

            //ThresholdVis.transform.position = Vector3.zero;
            //ThresholdVis.transform.Rotate(Vector3.right * 90.0f);
            ThresholdVis.transform.localPosition = new Vector3(0, -InteractionThreshold, 0);
        }
    }

    /// <summary>
    /// Gets components for necessary links
    /// </summary>
    void LinkComponents()
    {
        interactionTrigger = GetComponent<BoxCollider>();
        surfaceRenderer = GetComponentInChildren<Renderer>();
    }

    /// <summary>
    /// Adjusts trigger bounds to add margin of correct ratio
    /// </summary>
    private void AdjustTriggerBounds()
    {
        Vector3 oldSize = interactionTrigger.size;

        float ratio = transform.localScale.x / transform.localScale.z;

        oldSize.x = 1.0f + TriggerMargin * (1.0f / ratio);
        oldSize.y = 1.0f + TriggerMargin;
        oldSize.z = 1.0f + TriggerMargin;

        interactionTrigger.size = oldSize;
    }

    /// <summary>
    /// Pass all necessary values to interactable shader.
    /// </summary>
    void SetupShader(Renderer rend)
    {
        if (Application.isPlaying)
        {
            if (rend != null && rend.material != null)
            {
                rend.material.SetColor("_IdlingColor", IdleState.StateColor);
                rend.material.SetColor("_HoveringColor", HoveringState.StateColor);
                rend.material.SetColor("_ActivatingColor", ActivatingState.StateColor);

                rend.material.SetFloat("_InteractionThreshold", InteractionThreshold);

                float ratio = Mathf.Min(transform.localScale.x, transform.localScale.z);
                float x = transform.localScale.x / ratio;
                float y = transform.localScale.z / ratio;

                rend.material.SetVector("_SurfaceScale", new Vector4(x, y, 0.0f, 0.0f));
                //Debug.Log(x + " : " + y);

                rend.material.SetTexture("_IconTex", InteractableIcon);

                rend.material.SetFloat("_IconScale", IconScale);
            }
        }
        else
        {
            if (rend != null && rend.sharedMaterial != null)
            {
                rend.sharedMaterial.SetColor("_IdlingColor", IdleState.StateColor);
                rend.sharedMaterial.SetColor("_HoveringColor", HoveringState.StateColor);
                rend.sharedMaterial.SetColor("_ActivatingColor", ActivatingState.StateColor);

                rend.sharedMaterial.SetFloat("_InteractionThreshold", InteractionThreshold);

                float ratio = Mathf.Min(transform.localScale.x, transform.localScale.z);
                float x = transform.localScale.x / ratio;
                float y = transform.localScale.z / ratio;

                rend.sharedMaterial.SetVector("_SurfaceScale", new Vector4(x, y, 0.0f, 0.0f));
                //Debug.Log(x + " : " + y);

                rend.sharedMaterial.SetTexture("_IconTex", InteractableIcon);

                rend.sharedMaterial.SetFloat("_IconScale", IconScale);
            }
        }
    }

    /* ====================== Helpers ====================== */

    /// <summary>
    /// Helper function to check if being intereacted with
    /// </summary>
    /// <returns>Returns true if interactable is currently being interacted with</returns>
    public bool IsBeingInteracted()
    {
        return !(currentState == VRUI_Interactable_State.Idle);
    }

    //Change state and play correct state change events
    private void ChangeState(VRUI_Interactable_State a_newState)
    {
        //State hasn't actually changed...
        if (currentState == a_newState)
            return;

        switch (currentState)
        {
            case VRUI_Interactable_State.Idle:
                EndIdle();
                IdleState.EndStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Hovering:
                EndHover();
                HoveringState.EndStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Activated:
                EndActivate();
                ActivatingState.EndStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Released:
                ReleasedState.EndStateEvent.Invoke(Value);
                break;
        }

        currentState = a_newState;

        switch (currentState)
        {
            case VRUI_Interactable_State.Idle:
                if (surfaceRenderer) surfaceRenderer.material.SetInt("_InteractionState", 0);
                if (CursorController) CursorController.TriggerHapticPulse(IdleState.HapticPulse);
                BeginIdle();
                IdleState.BeginStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Hovering:
                if (surfaceRenderer) surfaceRenderer.material.SetInt("_InteractionState", 1);
                if (CursorController) CursorController.TriggerHapticPulse(HoveringState.HapticPulse);
                BeginHover();
                HoveringState.BeginStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Activated:
                if (surfaceRenderer) surfaceRenderer.material.SetInt("_InteractionState", 2);
                if (CursorController) CursorController.TriggerHapticPulse(ActivatingState.HapticPulse);
                BeginActivate();
                ActivatingState.BeginStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Released:
                if (CursorController) CursorController.TriggerHapticPulse(ReleasedState.HapticPulse);
                ReleasedState.BeginStateEvent.Invoke(Value);
                break;
        }
    }

    /* ======================= Logic ======================= */

    /// <summary>
    /// Transform cursor position into given transform local space
    /// </summary>
    /// <param name="transformSpace">Transform to use to find the inverse transform</param>
    /// <returns>Cursor position in the local space of transformSpace</returns>
    Vector3 CalcLocalCursorPosition(Transform transformSpace)
    {
        Vector3 cp = transformSpace.InverseTransformPoint(Cursor.transform.position);
        cp.x *= CursorAxisInfluence.x;
        cp.y *= CursorAxisInfluence.y;
        cp.z *= CursorAxisInfluence.z;
        return cp;
    }

    /// <summary>
    /// Calculates current interaction value based on cursor and it's local position
    /// </summary>
    /// <returns>Clamped value from 0 - 1</returns>
    protected float CalcInteractionValue()
    {
        float maxDepth = (interactionTrigger.size.y / 2.0f) - interactionTrigger.center.y;

        return 1.0f - Mathf.Clamp((CursorLocalTrigerPosition.y + maxDepth) / (interactionTrigger.size.y), 0.0f, 1.0f);
    }

    /// <summary>
    /// Calculates state change based on the current cursor interaction value
    /// </summary>
    protected void HandleStateChange()
    {
        //No cursor linked so not being interacted with
        if (Cursor == null)
        {
            if (currentState != VRUI_Interactable_State.Idle)
            {
                if (currentState != VRUI_Interactable_State.Released)
                    ChangeState(VRUI_Interactable_State.Released);
                else
                    ChangeState(VRUI_Interactable_State.Idle);
            }
            return;
        }

        //If slightly pressed but not past the threshold
        if (currentInteractionValue < InteractionThreshold)
            ChangeState(VRUI_Interactable_State.Hovering);

        //Check if hovering and now past the threshold
        if (currentState == VRUI_Interactable_State.Hovering && currentInteractionValue > InteractionThreshold)
        {
            //Interactable is now activated
            ChangeState(VRUI_Interactable_State.Activated);
        }
    }

    /// <summary>
    /// Call current interaction states continuous event
    /// </summary>
    protected void CallContinuousEvents()
    {
        switch (currentState)
        {
            case VRUI_Interactable_State.Idle:
                if (CursorController) CursorController.TriggerHapticPulse(IdleState.HapticAmount);
                Idling();
                IdleState.ContinuousStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Hovering:
                if (CursorController) CursorController.TriggerHapticPulse(HoveringState.HapticAmount);
                Hovering();
                HoveringState.ContinuousStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Activated:
                if (CursorController) CursorController.TriggerHapticPulse(ActivatingState.HapticAmount);
                Activating();
                ActivatingState.ContinuousStateEvent.Invoke(Value);
                break;
            case VRUI_Interactable_State.Released:
                if (CursorController) CursorController.TriggerHapticPulse(ReleasedState.HapticAmount);
                ReleasedState.ContinuousStateEvent.Invoke(Value);
                break;
        }
    }

    /// <summary>
    /// Calculate surface physics based on cursor position and cursor velocity
    /// </summary>
    protected void CalcSurfacePhysics()
    {
        if (currentState == VRUI_Interactable_State.Idle)
            SurfacePhysics.Simulate(Time.deltaTime);
        else
            SurfacePhysics.SetPosition(CursorLocalRendererPosition);
    }

    /* ====================== Events ======================= */

    protected virtual void BeginIdle() { }
    protected virtual void Idling() { }
    protected virtual void EndIdle() { }

    protected virtual void BeginHover() { }
    protected virtual void Hovering() { if (ValueChangedEvent != null) ValueChangedEvent(this, Value); }
    protected virtual void EndHover() { }

    protected virtual void BeginActivate() { }
    protected virtual void Activating() { if (ValueChangedEvent != null) ValueChangedEvent(this, Value); if (ValueChanged != null) ValueChanged.Invoke(Value); }
    protected virtual void EndActivate() { }

    /* ================ Collision Detection ================ */

    /// <summary>
    /// Called when triggered by collider. If other has controller component, also store reference to that
    /// </summary>
    /// <param name="other">Collider that triggered</param>
    private void OnTriggerEnter(Collider other)
    {
        //Check if collider was the cursor
        if (!other.gameObject.CompareTag("VRUI-Cursor"))
            return;

        Cursor = other.gameObject;
        if (Cursor)
            CursorController = Cursor.GetComponentInParent<VRUI_Controller>();
    }

    /// <summary>
    /// Called when trigger exited by collider. Sets all references to cursor to null
    /// </summary>
    /// <param name="other">Collider that exited trigger</param>
    private void OnTriggerExit(Collider other)
    {
        //Check if collider was the cursor
        if (!other.gameObject.CompareTag("VRUI-Cursor"))
            return;

        CursorController = null;
        Cursor = null;
    }

    /* ====================== Editor ======================= */

    protected virtual void OnValidate()
    {
        //Shader
        Renderer r = GetComponentInChildren<Renderer>();
        SetupShader(r);
    }
}
