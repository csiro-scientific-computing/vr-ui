using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TextDisplay : MonoBehaviour
{

    public VRUI_InteractionSurface connection;

    TextMesh tm;

    [Tooltip("-1 Won't round")]
    public int DecimalPlaces = 3;

    private string _displayText = "";
    public string DisplayText
    {
        get
        {
            return _displayText;
        }
        set
        {
            _displayText = value;
            tm.text = _displayText;
        }
    }

    private void Awake()
    {
        tm = GetComponent<TextMesh>();
        if (connection)
            connection.ValueChangedEvent += Connection_ValueChanged;
    }

    private void Connection_ValueChanged(object sender, float value)
    {
        SetDisplayText(value);
    }

    public void SetDisplayText(string text) { DisplayText = text; }
    public void SetDisplayText(int text) { DisplayText = text.ToString(); }
    public void SetDisplayText(float text) { DisplayText = (decimal.Round((decimal)text, DecimalPlaces, System.MidpointRounding.AwayFromZero)).ToString(); }
    public void SetDisplayText(double text) { DisplayText = (text).ToString(); }
    public void SetDisplayText(Vector2 text) { DisplayText = (text).ToString(); }
    public void SetDisplayText(Vector3 text) { DisplayText = (text).ToString(); }
    public void SetDisplayText(Vector4 text) { DisplayText = (text).ToString(); }
}
