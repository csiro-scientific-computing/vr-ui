using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElasticSurface
{
    /* ================= Surface Properties ================ */

    // -- Surface Elasticity
    public float Elasticity;
    // -- Surface Damping
    public float Damping;

    // -- Axis' that are influenced
    public Vector3 AxisInfluence = new Vector3(1, 1, 1);

    // -- Position of the surface apex
    Vector3 apexPosition;
    // -- Velocity of the surface apex
    Vector3 apexVelocity;


    /// <summary>
    /// Constructs surface with specified paramaters
    /// </summary>
    /// <param name="elasticity">Elasticity</param>
    /// <param name="damping">Damping</param>
    public ElasticSurface(float elasticity, float damping)
    {
        Elasticity = elasticity;
        Damping = damping;
    }

    /// <summary>
    /// Simulate surface
    /// </summary>
    /// <param name="deltaTime">Delta Time</param>
    public void Simulate(float deltaTime)
    {
        apexVelocity += (-apexPosition) * (0.1f * Damping);
        apexVelocity *= Mathf.Min(1.0f - (1.0f / (Elasticity)), 1.0f);

        if (apexVelocity.magnitude < 0.01f) apexVelocity = Vector3.zero;

        apexVelocity = Vector3.Scale(apexVelocity, AxisInfluence);

        apexPosition += apexVelocity;
        apexPosition *= 0.999f;

        if (apexPosition.magnitude < 0.01f) apexPosition = Vector3.zero;

        apexPosition.x = (float.IsNaN(apexPosition.x)) ? 0.0f : apexPosition.x;
        apexPosition.y = (float.IsNaN(apexPosition.y)) ? 0.0f : apexPosition.y;
        apexPosition.z = (float.IsNaN(apexPosition.z)) ? 0.0f : apexPosition.z;
    }

    /* ================ Getters and Setters ================ */

    /// <summary>
    /// Set the internal position of surface apex, overriding simulated position
    /// </summary>
    /// <param name="pos">New position</param>
    public void SetPosition(Vector3 pos)
    {
        apexPosition = pos;
    }

    /// <summary>
    /// Gets the internal position of the surface apex
    /// </summary>
    /// <returns>Internal position of the surface apex</returns>
    public Vector3 GetPosition()
    {
        return apexPosition;
    }
}
