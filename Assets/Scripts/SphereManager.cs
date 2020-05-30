﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnSpheres))]
public class SphereManager : MonoBehaviour
{
    public int maxSpheres = 40;
    public float sphereChangeSpeed = 10.0f;
    public float gravityAmount = -0.1f;
    public float minTimeUntilChangeActive = 3f;
    public float maxTimeUntilChangeActive = 10f;
    private List<SphereController> spheres = new List<SphereController>();
    private SpawnSpheres spawner;

    void Awake()
    {
        Physics.gravity = new Vector3(0, gravityAmount, 0);
        spawner = GetComponent<SpawnSpheres>();
    }

    public void AddSphere(GameObject newSphere)
    {
        var s = newSphere.GetComponent<SphereController>();
        spheres.Add(s);

        if (spheres.Count == 1)
        {
            ActivateSphere(s);
        }
        float timeUntilChangeActive = Random.Range(minTimeUntilChangeActive, maxTimeUntilChangeActive);
        Invoke("ChangeActiveSphere", timeUntilChangeActive);
    }

    public void KillSphere(SphereController sp)
    {
        spheres.Remove(sp);
        sp.gameObject.SetActive(false);
    }

    public GameObject GetParentSphere()
    {
        return spawner.Parent.gameObject;
    }


    public bool CanSpawn()
    {
        return spheres.Count < maxSpheres;
    }

    public (int count, int max) GetSphereCountRange()
    {
        return (spheres.Count, maxSpheres);
    }

    public (float min, float max) GetSphereRotationSpeedRange()
    {
        return (spawner.MinRotationSpeed, spawner.MaxRotationSpeed);
    }

    public (float min, float max) GetSphereSizeRange()
    {
        return (spawner.MinSphereSize, spawner.MaxSphereSize);
    }

    public (float min, float max) GetSphereRotateAroundSpeedRange()
    {
        return (spawner.MinRotateAroundSpeed, spawner.MaxRotateAroundSpeed);
    }

    private SphereController GetActiveSphere()
    {
        return spheres.Find(s => s.IsActive());
    }

    private void ActivateSphere(SphereController s)
    {
        if (s)
        {
            s.Activate();
        }
    }

    private void DeactivateActiveSphere()
    {
        var s = GetActiveSphere();
        if (s)
        {
            s.Deactivate();
        }

    }

    private void ChangeActiveSphere()
    {
        if (spheres.Count > 1)
        {
            var previousSphere = GetActiveSphere();
            var activeSphere = spheres[Random.Range(0, spheres.Count)];
            if (previousSphere)
            {
                if (activeSphere != previousSphere && !activeSphere.HasTouchedFloor)
                {
                    DeactivateActiveSphere();
                    ActivateSphere(activeSphere);
                }
            }
            else
            {
                if (!activeSphere.HasTouchedFloor)
                {
                    ActivateSphere(activeSphere);
                }

            }
        }
    }

    public void ChangeActiveSphereTo(SphereController newActiveSphere)
    {
        DeactivateActiveSphere();
        ActivateSphere(newActiveSphere);
        CancelInvoke("ChangeActiveSphere");
        float timeUntilChangeActive = Random.Range(minTimeUntilChangeActive, maxTimeUntilChangeActive);
        Invoke("ChangeActiveSphere", timeUntilChangeActive);
    }
}
