using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CinematicManager : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject mainCanvas; // I-drag dito ang 'Controll' or 'Canvas' object

    [Header("Flight Path")]
    public List<Vector3> waypoints = new List<Vector3>(); // Ang huling point nito ang magiging default cam position
    
    [Header("Cinematic Settings")]
    public float flySpeed = 5.0f;
    public float rotationSpeed = 2.0f;

    void Start()
    {
        // Magsisimula agad pagkaload ng scene
        if (waypoints.Count > 0)
        {
            StartCoroutine(BeginSceneIntro());
        }
    }

    IEnumerator BeginSceneIntro()
    {
        // 1. Itago ang lahat ng UI sa Canvas
        if (mainCanvas != null) mainCanvas.SetActive(false);

        // 2. Flythrough sa bawat waypoint
        for (int i = 0; i < waypoints.Count; i++)
        {
            Vector3 targetPoint = waypoints[i];
            
            // Habang malayo pa sa current point, move lang
            while (Vector3.Distance(transform.position, targetPoint) > 0.1f)
            {
                // Smooth position transition
                transform.position = Vector3.Lerp(transform.position, targetPoint, Time.deltaTime * flySpeed);
                
                // Rotation logic: Tumingin sa direksyon ng nililipadan
                if (transform.position != targetPoint)
                {
                    Quaternion targetRot = Quaternion.LookRotation(targetPoint - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
                }
                
                yield return null;
            }
        }

        // 3. Cinematic Done: Ibalik ang UI sa default camera view
        if (mainCanvas != null) mainCanvas.SetActive(true);
        
        Debug.Log("Cinematic Finished at the final waypoint.");
    }
}