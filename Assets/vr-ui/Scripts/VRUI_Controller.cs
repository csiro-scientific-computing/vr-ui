using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUI_Controller_Buttons { }

public class VRUI_Controller : MonoBehaviour
{
    /* ================ Controller Variables =============== */

    //Should spawn interaction cursor
    [Header("Options")]
    public bool SpawnInteractionCursor = true;

    //Menus
    [Header("Menus")]
    public GameObject AttachedMenuLeft;
    public GameObject AttachedMenuRight;
    public float MenuScale = 1.0f;
    public float MenuOffset = 0;

    [HideInInspector]
    //Reference to gameobject acting as cursor
    public GameObject Cursor;

    [HideInInspector]
    //Current velocity of the controller
    public Vector3 ControllerVelocity;

    [HideInInspector]
    //Current angular velocity of the controller
    public Vector3 ControllerAngularVelocity;

    [HideInInspector]
    //Internal index of the controller, specified by the vr sdk
    public int ControllerIndex;

    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Abstract virtual awake function. Use to link components
    /// </summary>
    protected virtual void Awake() { }

    /// <summary>
    /// Virtual Start function of VRUI_Controller
    /// </summary>
    protected virtual void Start()
    {
        //If should spawn cursor, spawn it
        if (SpawnInteractionCursor)
            SpawnDefaultCursor();

        SpawnAttachedMenus();
    }

    /// <summary>
    /// Abstract virtual update function.
    /// </summary>
    protected virtual void Update() { }

    /// <summary>
    /// Abstract virtual fixed update function.
    /// </summary>
    protected virtual void FixedUpdate() { }

    /* ================== Spawning Methods ================= */

    /// <summary>
    /// Spawns attached menus at cursor position
    /// </summary>
    void SpawnAttachedMenus()
    {
        if (AttachedMenuLeft)
        {
            GameObject leftMenu = Instantiate(AttachedMenuLeft, Cursor.transform, false);
            leftMenu.transform.localScale *= 25 * MenuScale;
            leftMenu.transform.localPosition += Vector3.right * (MenuOffset + 25 / 2);
        }

        if (AttachedMenuRight)
        {
            GameObject rightMenu = Instantiate(AttachedMenuRight, Cursor.transform, false);
            rightMenu.transform.localScale *= 25 * MenuScale;
            rightMenu.transform.localPosition += Vector3.left * (MenuOffset + 25 / 2);
        }
    }

    /// <summary>
    /// Spawns a default sphere at the default cursor position. Creates a RigidBody component and sets it to kinematic.
    /// </summary>
    void SpawnDefaultCursor()
    {
        //Create sphere primitive
        Cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //Set default gameobject properties
        Cursor.name = "Interaction Cursor";
        Cursor.tag = "VRUI-Cursor";
        Cursor.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        //Set default transforms
        Cursor.transform.SetParent(transform, false);
        Cursor.transform.rotation = Quaternion.Euler(32.0f, 180.0f, 0.0f);
        Cursor.transform.localPosition = new Vector3(0, -0.0135f, 0.05f);
        Cursor.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);

        //Add rigidbody component
        Rigidbody cursorRB = Cursor.AddComponent<Rigidbody>();

        //Set default rigidbody properties
        cursorRB.useGravity = false;
        cursorRB.isKinematic = true;
        cursorRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    /* =============== Controller Interfaces =============== */

    /// <summary>
    /// Abstract function to trigger haptic feedback over a length of time.
    /// </summary>
    /// <param name="strength">Value between 0 and 1</param>
    /// <param name="length">Length in seconds</param>
    public virtual void TriggerHapticFeedback(float strength, float length) { }

    /// <summary>
    /// Abstract function which triggers device specific haptic pulse.
    /// </summary>
    /// <param name="strength">Value between 0 and 1</param>
    public virtual void TriggerHapticPulse(float strength) { }

    /// <summary>
    /// Abstract function allowing for a common utility function to get the state of a controller button.
    /// </summary>
    /// <param name="controllerIndex">Device specific index of controller</param>
    /// <param name="buttonIndex">Device specific index of button</param>
    /// <returns></returns>
    public virtual bool GetButtonState(uint controllerIndex, int buttonIndex)
    {
        return false;
    }
}
