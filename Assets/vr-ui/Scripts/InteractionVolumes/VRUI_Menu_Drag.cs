using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Ideas:
 *  Can dock menus together when attach two of these spheres together
 *  Have dock ports on controllers
 *      Make dock ports on controller show on button press
*/

public class VRUI_Menu_Drag : VRUI_InteractionVolume
{
    // -- Linked menu
    VRUI_Menu menu;

    /* ================ Overriden Functions ================ */

    protected override void Awake()
    {
        base.Awake();
        menu = GetComponentInParent<VRUI_Menu>();
    }

    protected override void Activating()
    {
        base.Activating();



        if (InteractingController)
            InteractingController.TriggerHapticPulse(0.45f);

        if (InteractingGO)
            VolumePhysics.SetPosition(volumeCollider.transform.InverseTransformPoint(InteractingGO.transform.position));
    }

    protected override void Hovering()
    {
        base.Hovering();


        if (InteractingController)
            InteractingController.TriggerHapticPulse(0.25f);

        if (InteractingGO)
            VolumePhysics.SetPosition(volumeCollider.transform.InverseTransformPoint(InteractingGO.transform.position));
    }

    protected override void HoverBegin()
    {
        base.HoverBegin();
        StopAllCoroutines();
        //StartCoroutine(ScaleTo(0.1f, 0.5f));
    }

    protected override void HoverEnd()
    {
        base.HoverEnd();
        StopAllCoroutines();
        //StartCoroutine(ScaleTo(0.05f, 0.5f));
    }

    protected override void ActivateBegin()
    {
        base.ActivateBegin();

        if (menu.SnapToControllerOrientation)
        {
            //Set rotation to controllers
            menu.transform.rotation = InteractingGO.transform.rotation;// * Quaternion.Euler(90.0f, 0, 0);

        }

        //Move back by offset
        menu.transform.position = InteractingGO.transform.position + (menu.transform.position - transform.position);
        //StartCoroutine(MoveTo(menu.gameObject, InteractingGO.transform.position + (menu.transform.position - transform.position), 0.5f));

        menu.transform.SetParent(InteractingController.Cursor.transform, true);
        if (menu.GetComponent<Rigidbody>())
        {
            menu.GetComponent<Rigidbody>().velocity = new Vector3();
            menu.GetComponent<Rigidbody>().isKinematic = true;
        }


        volumeRenderer.material.SetInt("_CursorState", 1);
    }

    protected override void ActivateEnd()
    {
        base.ActivateEnd();
        if (menu.GetComponent<Rigidbody>())
        {
            menu.GetComponent<Rigidbody>().isKinematic = false;

            if (InteractingController.ControllerVelocity.magnitude > 1.0f)
                menu.GetComponent<Rigidbody>().velocity = InteractingController.ControllerVelocity;
            if (InteractingController.ControllerAngularVelocity.magnitude > 1.0f)
                menu.GetComponent<Rigidbody>().angularVelocity = InteractingController.ControllerAngularVelocity;
        }

        volumeRenderer.material.SetInt("_CursorState", 0);

        menu.transform.SetParent(null, true);
    }

    /* =============== Translating Corutines =============== */

    IEnumerator ScaleTo(float scaleAmt, float timeLength)
    {
        for (float i = 0; i < timeLength; i += Time.deltaTime)
        {
            float ls = transform.localScale.x;

            float ns = Mathf.Lerp(ls, scaleAmt, i / timeLength);

            transform.localScale = new Vector3(ns, ns, ns);
            yield return null;
        }
    }

    IEnumerator MoveTo(GameObject obj, Vector3 newPos, float timeLength)
    {
        Vector3 oldPos = transform.position;
        for (float i = 0; i < timeLength; i += Time.deltaTime)
        {
            obj.transform.position = Vector3.Lerp(oldPos, newPos, i / timeLength);
            yield return null;
        }
    }

    IEnumerator RotateTo(GameObject obj, Quaternion newRot, float timeLength)
    {
        Quaternion oldRot = transform.rotation;
        for (float i = 0; i < timeLength; i += Time.deltaTime)
        {
            obj.transform.rotation = Quaternion.Slerp(oldRot, newRot, i / timeLength);
            yield return null;
        }
    }
}
