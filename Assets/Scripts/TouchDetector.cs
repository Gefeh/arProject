using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// TouchColliderDetector - Handles touch input for interacting with AR objects
/// 
/// PURPOSE:
/// This script detects when users tap on AR objects and triggers actions (like locking/unlocking).
/// It uses Unity's New Input System and raycasting to detect which object was touched.
/// 
/// HOW IT WORKS:
/// 1. Listens for tap/touch input using the Input System
/// 2. Converts touch position to a 3D ray from the camera
/// 3. Uses Physics.Raycast to check if the ray hits any colliders
/// 4. If an AR object is hit, calls the ImageTracker to toggle its lock state
/// 
/// SETUP FOR STUDENTS:
/// 1. Add this script to a GameObject in your AR scene
/// 2. Assign your AR Camera to _arCamera
/// 3. Create and assign an Input Action for tap detection
/// 4. Assign your ImageTracker script to _imageTracker
/// 5. Make sure your AR objects have Collider components!
/// 
/// CUSTOMIZATION IDEAS:
/// - Add different actions for long press vs short tap
/// - Implement multi-touch gestures (pinch, rotate)
/// - Add visual feedback when objects are touched
/// - Support different interaction modes (select, move, delete)
/// - Add sound effects for touch interactions
/// </summary>
public class TouchColliderDetector : MonoBehaviour
{
    // ===== INSPECTOR FIELDS (Drag and drop these in the Unity Inspector) =====

    /// <summary>
    /// The AR Camera used to convert screen touches to 3D rays
    /// STUDENT NOTE: This should be the same camera used for AR rendering
    /// </summary>
    [SerializeField] private Camera _arCamera;

    /// <summary>
    /// Input Action that defines what input triggers the tap detection
    /// STUDENT NOTE: Create this in the Input Action Asset (usually mapped to touch or mouse click)
    /// Set it to "Button" type with "Press" interaction for best results
    /// </summary>
    [SerializeField] private InputAction _tapAction;

    /// <summary>
    /// Reference to the ImageTracker script that handles lock/unlock functionality
    /// STUDENT NOTE: This should be the same ImageTracker that manages your AR objects
    /// </summary>
    [SerializeField] private ImageTracker _imageTracker;

    /// <summary>
    /// OnEnable is called when this GameObject becomes active
    /// This is where we activate the input action to start listening for touches
    /// STUDENT NOTE: Always enable Input Actions in OnEnable, not Start!
    /// </summary>
    /// 
    [SerializeField] private AudioSource audioSource;
    
    private void OnEnable()
    {
        // Enable the tap action so it starts detecting input
        // Without this, the Input Action won't respond to touches/clicks
        _tapAction.Enable();
    }

    /// <summary>
    /// OnDisable is called when this GameObject becomes inactive
    /// This is where we deactivate the input action to stop listening for touches
    /// STUDENT NOTE: Always disable Input Actions in OnDisable to prevent errors!
    /// </summary>
    private void OnDisable()
    {
        // Disable the tap action to stop detecting input
        // This prevents memory leaks and errors when the GameObject is destroyed
        _tapAction.Disable();
    }

    /// <summary>
    /// Update is called once per frame
    /// This is where we check for touch input and handle raycasting
    /// STUDENT NOTE: This is the main interaction detection loop
    /// </summary>
    void Update()
    {
        // Check if the tap action was performed this frame
        // WasPerformedThisFrame() returns true only on the frame when input occurs
        if (_tapAction.WasPerformedThisFrame())
        {
            // Get the screen position of the touch/click
            // This works for both mouse (Pointer) and touch (Touchscreen) input
            Vector2 screenPos = Pointer.current != null
                                ? Pointer.current.position.ReadValue()      // Mouse position
                                : Touchscreen.current.primaryTouch.position.ReadValue(); // Touch position

            // Convert the 2D screen position to a 3D ray
            // This ray starts at the camera and goes through the touched point into 3D space
            Ray ray = _arCamera.ScreenPointToRay(screenPos);

            // Perform raycasting to see if the ray hits any colliders
            // Physics.Raycast returns true if something was hit, and fills the 'hit' variable with details
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                if (hit.collider.CompareTag("Dog"))
                {
                    Bark();
                }
                // We hit something! Get the GameObject that was hit and toggle its lock state
                // hit.collider.gameObject gives us the GameObject that owns the collider we hit
                //_imageTracker.ToggleLockState(hit.collider.gameObject);

                // STUDENT TIP: You could add more logic here:
                // - Check if the hit object is actually an AR object
                // - Add visual feedback (highlight, particles, etc.)
                // - Play sound effects
                // - Handle different types of objects differently

                // DEBUGGING TIP: Uncomment the line below to see what you're hitting in the Console
                // Debug.Log($"Hit object: {hit.collider.gameObject.name}");
            }
        }
    }
    
    void Bark()
    {
        if (audioSource != null)
        {
            audioSource.Play();
            Debug.Log("Albert barked!");
        }
    }
}