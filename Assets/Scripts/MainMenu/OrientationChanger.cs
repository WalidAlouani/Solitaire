using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationChanger : MonoBehaviour
{
    [SerializeField] private GameObject portraitLayout;
    [SerializeField] private GameObject landscapeLayout;

    // A private variable to store the orientation from the previous frame
    private ScreenOrientation lastOrientation;

    void Start()
    {
        // Initialize the last orientation on startup
        lastOrientation = Screen.orientation;

        // Log the initial orientation for debugging
        Debug.Log("Initial Orientation: " + lastOrientation);

        // Optionally call a method to set up the initial state
        HandleOrientationChange(lastOrientation);
    }

    void Update()
    {
        // Check if the current orientation is different from the last frame's
        if (Screen.orientation != lastOrientation)
        {
            // The orientation has changed!
            Debug.Log("Orientation Changed from " + lastOrientation + " to " + Screen.orientation);

            // Call the method to handle the change
            HandleOrientationChange(Screen.orientation);

            // Update the stored orientation for the next frame's check
            lastOrientation = Screen.orientation;
        }
    }

    // Method to execute specific logic based on the new orientation
    private void HandleOrientationChange(ScreenOrientation newOrientation)
    {
        switch (newOrientation)
        {
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                portraitLayout.SetActive(true);
                landscapeLayout.SetActive(false);
                break;

            case ScreenOrientation.LandscapeLeft:
            case ScreenOrientation.LandscapeRight:
                portraitLayout.SetActive(false);
                landscapeLayout.SetActive(true);
                break;

            default:
                // Handle AutoRotation, Unknown, or FaceUp/FaceDown (if using Input.deviceOrientation)
                Debug.Log("Orientation is either Auto, Unknown, or a flat position.");
                break;
        }
    }
}
