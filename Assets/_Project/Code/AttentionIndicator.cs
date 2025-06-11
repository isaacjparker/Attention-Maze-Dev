using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttentionIndicator : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleAttentionIndicator();
        }
    }

    private void HandleAttentionIndicator()
    {
        Debug.Log("Attention Indicated");
    }
}
