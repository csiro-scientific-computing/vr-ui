using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUI_Button_Demo : VRUI_Button
{
    public TextDisplay output;

    public string ButtonText = "1";

    protected override void BeginActivate()
    {
        base.BeginActivate();
        output.DisplayText = ButtonText;
    }
}
