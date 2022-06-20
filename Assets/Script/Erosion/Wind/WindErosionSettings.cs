using System;
using UnityEngine;

[Serializable]
public class WindErosionSettings
{
    [Header("Wind settings")] //
    public Vector2 windDirection;

    [Header("Particle settings")] //
    public int maxParticleLifetime = 100000;
    public float gravity = 0.1f;
    public Vector3 initialSpeed = new Vector3(1, 0, 1);
    public float prevailingFactor = 0.1f;

    [Header("TODO")] //
    public float dt = 0.25f; // Delta-T
    public float suspension = 0.0001f; //Affects transport rate
    public float abrasion = 0.0001f;
    public float roughness = 0.005f;
    public float settling = 0.01f;
}
