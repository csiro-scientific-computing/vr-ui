using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUI_Menu : MonoBehaviour
{
    /* =================== Menu Variables ================== */

    [Header("Menu Behaviour")]
    public bool CanDrift = false;
    public bool SnapToControllerOrientation = false;

    [Header("Menu Bar")]
    public bool IsDraggable = true;
    public float ControlSize = 1.0f;

    [Header("Title Bar")]
    public bool ShowTitleBar = false;

    [Header("Meta")]
    public string Title;

    [Header("Tooltips")]
    public bool ShowToolTips;

    [Header("Border")]
    public bool ShowBorder = true;
    public float BorderMargin = 0.05f;
    GameObject border;

    // -- Decoration
    [Header("Decoration")]
    public float DecorationOffset = 0.5f;
    GameObject TitleBar;
    GameObject HandleBar;

    public GameObject[] ExtraObjects;

    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Setup menu on Unity Start event
    /// </summary>
    void Start()
    {
        Setup();
    }

    /// <summary>
    /// Used to primarily reflect changes in border affected by change in child positions
    /// </summary>
    private void OnValidate()
    {
        ResizeOutline();
    }

    private void Update()
    {
        ResizeOutline();
    }

    /* =================== Menu Functions ================== */

    /// <summary>
    /// Sets up menu components and preferences
    /// </summary>
    void Setup()
    {
        //Create border if specified
        if (ShowBorder)
        {
            if (border == null)
                border = Instantiate(Resources.Load("BorderedQuad"), transform, false) as GameObject;

            //Resize border to contain child elements
            ResizeOutline();
        }

        //Setup menu drifiting capabilities
        if (CanDrift)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.drag = 2.75f;
            rb.angularDrag = 3.0f;
        }

        //Show title bar
        if (ShowTitleBar)
        {
            TitleBar = new GameObject();
            TitleBar.name = "Title Bar";
            TitleBar.transform.SetParent(transform, false);
            TitleBar.transform.localPosition = border.transform.localPosition + (Vector3.back * (border.transform.localScale.z * 0.5f) + Vector3.back * DecorationOffset * 0.1f);

            GameObject titleText = Instantiate(Resources.Load("TextDisplay"), TitleBar.transform, false) as GameObject;
            titleText.GetComponent<TextDisplay>().DisplayText = Title;
            TextMesh tm = titleText.GetComponent<TextMesh>();
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.LowerCenter;
            tm.characterSize = 0.0025f;
            titleText.transform.Rotate(Vector3.right * 90);
        }

        //Add bar controls
        AddMenuBarControls();
    }

    /// <summary>
    /// Resizes the bounds of the border based on the location and scale of child elements
    /// </summary>
    void ResizeOutline()
    {
        VRUI_InteractionSurface[] interactables = GetComponentsInChildren<VRUI_InteractionSurface>();

        Bounds b = new Bounds(Vector3.zero, Vector3.zero);

        for (int i = 0; i < interactables.Length; ++i)
        {
            VRUI_InteractionSurface interactable = interactables[i];
            Vector3 ls = interactable.transform.localScale;
            Bounds bb = new Bounds(interactable.transform.localPosition, Vector3.Scale(interactable.GetComponent<BoxCollider>().size, new Vector3(ls.x, ls.y, ls.z)));
            if (i == 0)
                b = bb;
            else
                b.Encapsulate(bb);
        }

        if (ExtraObjects != null)
        {
            foreach (GameObject go in ExtraObjects)
            {
                Vector3 ls = go.transform.localScale;
                Bounds bb = new Bounds(go.transform.localPosition, Vector3.Scale(go.GetComponent<BoxCollider>().size, new Vector3(ls.x, ls.y, ls.z)));
                b.Encapsulate(bb);
            }
        }

        float ratio = b.size.x / b.size.z;
        //Debug.Log(gameObject.name + " : " + b.size);

        if (border)
        {
            border.transform.localPosition = b.center;
            Vector3 s = new Vector3(b.size.x, b.size.y, b.size.z);
            s.x *= (1.0f + BorderMargin);
            s.z *= (1.0f + BorderMargin);
            
            border.transform.localScale = s;// Vector3.Scale(s, transform.localScale);
            

            //Move menu bars
            if(TitleBar)
                TitleBar.transform.localPosition = border.transform.localPosition + (Vector3.back * (border.transform.localScale.z * 0.5f) + Vector3.back * DecorationOffset * 0.1f);
            if(HandleBar)
                HandleBar.transform.localPosition = border.transform.localPosition + (Vector3.forward * (border.transform.localScale.z * 0.5f) + Vector3.forward * DecorationOffset * 0.1f);
        }
    }

    /// <summary>
    /// Handles the spawning of enabled menu controls
    /// </summary>
    void AddMenuBarControls()
    {
        if (IsDraggable)
        {
            HandleBar = new GameObject();
            HandleBar.name = "Handle Bar";
            HandleBar.transform.SetParent(transform, false);
            HandleBar.transform.localPosition = border.transform.localPosition + (Vector3.forward * (border.transform.localScale.z * 0.5f) + Vector3.forward * DecorationOffset * 0.1f);

            GameObject Dragger = Instantiate(Resources.Load("Menu_Grabber"), HandleBar.transform, false) as GameObject;
        }
    }
}
