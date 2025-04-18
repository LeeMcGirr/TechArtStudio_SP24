using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXTrailController : MonoBehaviour
{
    public VisualEffect vfxGraph;  // Assign your VFX component here
    public CharacterController characterController; // Reference to the character's movement component

    void Update()
    {
        // Check if the character is moving
        bool isMoving = characterController.velocity.magnitude > 0.1f;

        // Update the VFX Graph parameter
        vfxGraph.SetBool("IsMoving", isMoving);
    }
}

