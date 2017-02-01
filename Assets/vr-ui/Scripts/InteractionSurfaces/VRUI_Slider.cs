using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUI_Slider : VRUI_InteractionSurface
{

    //Slider Data Structures
    [System.Serializable]
    public class SliderRange { public float Minimum = 0.0f; public float Maximum = 1.0f; public SliderRange(float min, float max) { Minimum = min; Maximum = max; } }

    /* ================== Slider Variables ================= */

    [Header("Slider Meta")]
    // -- Range of the slider
    public SliderRange Range = new SliderRange(0, 1);

    // -- Range of slider between 0 and 1
    [HideInInspector]
    public float SliderValueNormalized = 0.5f;
    
    /* =================== Unity Pipeline ================== */

    /// <summary>
    /// Override of Update function
    /// </summary>
    protected override void Update()
    {
        base.Update();

        //Sends slider amount to shader
        surfaceRenderer.sharedMaterial.SetFloat("_SliderAmount", SliderValueNormalized);
    }

    /// <summary>
    /// Reflects realtime changes of slider value
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        Renderer r = GetComponentInChildren<Renderer>();
        if (r && r.sharedMaterial)
        {
            SliderValueNormalized = Mathf.InverseLerp(Range.Minimum, Range.Maximum, Value);
            r.sharedMaterial.SetFloat("_SliderAmount", SliderValueNormalized);
        }
    }

    /* ================ Overriden Functions ================ */

    /// <summary>
    /// Overide of activating function, called while activated
    /// </summary>
    protected override void Activating()
    {
        base.Activating();

        //Calculate slider value
        SliderValueNormalized = 0.5f - CursorLocalTrigerPosition.x;
        SliderValueNormalized = Mathf.Clamp(SliderValueNormalized, 0.0f, 1.0f);

        //Set the interactable state value
        Value = Mathf.SmoothStep(Range.Minimum, Range.Maximum, SliderValueNormalized);
    }

}
