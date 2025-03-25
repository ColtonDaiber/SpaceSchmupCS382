using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Keeps a GameObject on screen.
/// Note that this ONLY works for an orthographic Main Camera at [ 0, 0, 0 ].
/// </summary>
public class BoundsCheck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float radius = 1f;
    [Header("Set Dynamically")]
    public float camWidth;
    public float camHeight;
    void Awake()
    {
        camHeight = Camera.main.orthographicSize;
        camWidth = camHeight * Camera.main.aspect;
    }
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.x > camWidth - radius)
        {
            pos.x = camWidth - radius;
        }
        if (pos.x < -camWidth + radius)
        {
            pos.x = -camWidth + radius;
        }
        if (pos.y > camHeight - radius)
        {
            pos.y = camHeight - radius;
        }
        if (pos.y < -camHeight + radius)
        {
            pos.y = -camHeight + radius;
        }
        transform.position = pos;
    }
    // Draw the bounds in the Scene pane using OnDrawGizmos()
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Vector3 boundSize = new Vector3(camWidth * 2, camHeight * 2, 0.1f);
        Gizmos.DrawWireCube(Vector3.zero, boundSize);
    }
}