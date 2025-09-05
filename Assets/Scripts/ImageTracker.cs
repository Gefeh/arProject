using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// ImageTracker - The core AR image tracking script for marker-based AR games
/// 
/// PURPOSE:
/// This script handles the detection and tracking of image markers in AR. When a marker is detected,
/// it spawns 3D objects on top of the marker and tracks their position. It also provides functionality
/// to "lock" objects in place so they don't disappear when the marker is lost.
/// 
/// HOW IT WORKS:
/// 1. ARTrackedImageManager detects image markers from your Reference Image Library
/// 2. When a marker is found, we instantiate a 3D object (prefab) at that location
/// 3. The object follows the marker as it moves in real-world space
/// 4. Users can tap objects to "lock" them, making them stay visible even when the marker is lost
/// 5. Locked objects become children of the camera and stay in screen space
/// 
/// SETUP FOR STUDENTS:
/// 1. Add this script to a GameObject in your AR scene
/// 2. Assign the ARTrackedImageManager (usually found on your AR Session Origin)
/// 3. Create a prefab for your 3D object and assign it to _basePrefab
/// 4. Assign your AR Camera to _arCamera
/// 5. Create a Reference Image Library with your marker images
/// 6. Put matching texture files in the Resources folder
/// 
/// CUSTOMIZATION IDEAS:
/// - Spawn different prefabs based on different markers
/// - Add animations when objects appear/disappear
/// - Create UI elements that appear when objects are locked
/// - Add sound effects for lock/unlock actions
/// - Implement object persistence between sessions
/// </summary>
public class ImageTracker : MonoBehaviour
{
    // ===== INSPECTOR FIELDS (Drag and drop these in the Unity Inspector) =====
    
    /// <summary>
    /// The AR Tracked Image Manager component that detects image markers
    /// STUDENT NOTE: Find this component on your AR Session Origin GameObject
    /// </summary>
    [SerializeField] private ARTrackedImageManager _trackedImageManager;
    
    /// <summary>
    /// The 3D object (prefab) that will be spawned when a marker is detected
    /// STUDENT NOTE: Create a prefab of your 3D model and drag it here
    /// Make sure your prefab has a Renderer component for texture application
    /// </summary>
    [SerializeField] private GameObject _basePrefab;
    
    /// <summary>
    /// The AR Camera that renders the AR view
    /// STUDENT NOTE: This is usually the Main Camera in your AR scene
    /// </summary>
    [SerializeField] private Camera _arCamera;

    // ===== INTERNAL DATA STRUCTURES (These manage the AR objects and their states) =====
    
    /// <summary>
    /// List of all 3D objects that have been spawned from detected markers
    /// STUDENT NOTE: This helps us keep track of all active AR objects in the scene
    /// </summary>
    private List<GameObject> spawnedARObjects = new List<GameObject>();
    
    /// <summary>
    /// Dictionary that stores loaded textures by name for quick lookup
    /// STUDENT NOTE: Textures are loaded from the Resources folder and matched to marker names
    /// </summary>
    private Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();
    
    /// <summary>
    /// Tracks which objects are currently "locked" (won't disappear when marker is lost)
    /// STUDENT NOTE: Locked objects stay visible and can be moved independently of markers
    /// </summary>
    private Dictionary<GameObject, bool> lockedObjects = new Dictionary<GameObject, bool>();
    
    /// <summary>
    /// References to lock icon GameObjects for each AR object
    /// STUDENT NOTE: Add a child GameObject with tag "LockIcon" to show lock status
    /// </summary>
    private Dictionary<GameObject, GameObject> lockIcons = new Dictionary<GameObject, GameObject>();
    
    // Store original parent and local transform for unlocking
    // STUDENT NOTE: These dictionaries remember where objects came from so we can restore them
    
    /// <summary>
    /// Stores the original parent Transform for each locked object
    /// This allows us to restore the object to its marker when unlocked
    /// </summary>
    private Dictionary<GameObject, Transform> originalParents = new Dictionary<GameObject, Transform>();
    
    /// <summary>
    /// Stores the original local position for each locked object
    /// This ensures objects return to the exact same spot on their marker
    /// </summary>
    private Dictionary<GameObject, Vector3> originalLocalPositions = new Dictionary<GameObject, Vector3>();
    
    /// <summary>
    /// Stores the original local rotation for each locked object
    /// This ensures objects maintain their correct orientation when restored
    /// </summary>
    private Dictionary<GameObject, Quaternion> originalLocalRotations = new Dictionary<GameObject, Quaternion>();

    /// <summary>
    /// Awake is called when the script instance is being loaded
    /// This is where we initialize our components and load resources
    /// STUDENT NOTE: Awake happens before Start, perfect for initialization
    /// </summary>
    void Awake()
    {
        // Auto-find the ARTrackedImageManager if not assigned in Inspector
        // STUDENT TIP: Always include fallbacks like this for easier setup!
        if (_trackedImageManager == null)
        {
            _trackedImageManager = GetComponent<ARTrackedImageManager>();
        }
        
        // Auto-find the main camera if not assigned
        if (_arCamera == null)
        {
            _arCamera = Camera.main;
        }
        
        // Load all textures from Resources folder at startup
        // This prevents lag during gameplay when textures are needed
        LoadTexturesFromResources();
    }
    
    /// <summary>
    /// Loads all texture files from the Resources folder into memory
    /// STUDENT NOTE: This method automatically finds textures that match your marker names
    /// 
    /// HOW TO USE:
    /// 1. Create a "Resources" folder in your Assets directory
    /// 2. Put texture files (.png, .jpg) with the same names as your Reference Images
    /// 3. This method will automatically load them and apply them to spawned objects
    /// 
    /// EXAMPLE: If your marker is named "MarkerA", put "MarkerA.png" in Resources folder
    /// </summary>
    private void LoadTexturesFromResources()
    {
        // Load all Texture2D files from the Resources folder
        // The empty string "" means load from the root of Resources folder
        Texture2D[] textures = Resources.LoadAll<Texture2D>("");
        
        // Store each texture in our dictionary with its filename as the key
        foreach (var texture in textures)
        {
            loadedTextures[texture.name] = texture;
            Debug.Log($"Loaded texture: {texture.name}");
            // STUDENT TIP: Check the Console to see which textures were loaded successfully!
        }
    }

    /// <summary>
    /// OnEnable is called when the GameObject becomes active
    /// This is where we subscribe to AR tracking events
    /// STUDENT NOTE: Always subscribe to events in OnEnable, not Start!
    /// </summary>
    void OnEnable()
    {
        // Subscribe to the trackablesChanged event
        // This event fires whenever AR markers are added, updated, or removed
        _trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    /// <summary>
    /// OnDisable is called when the GameObject becomes inactive
    /// This is where we unsubscribe from events to prevent memory leaks
    /// STUDENT NOTE: Always unsubscribe in OnDisable to match OnEnable!
    /// </summary>
    void OnDisable()
    {
        // Unsubscribe from the tracking event to prevent errors
        // This is crucial for proper memory management
        _trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    /// <summary>
    /// Public method to toggle the lock state of an AR object
    /// This is called by the TouchDetector when a user taps on an object
    /// 
    /// STUDENT NOTE: This is the main interface for the lock/unlock feature
    /// You can call this method from other scripts, UI buttons, or game events
    /// 
    /// PARAMETER: arObject - The AR object to lock or unlock
    /// </summary>
    public void ToggleLockState(GameObject arObject)
    {
        // Check if the object is currently locked
        // We use ContainsKey to avoid null reference exceptions
        bool isCurrentlyLocked = lockedObjects.ContainsKey(arObject) && lockedObjects[arObject];
        
        if (isCurrentlyLocked)
        {
            // Object is locked, so unlock it (return it to marker tracking)
            UnlockObject(arObject);
        }
        else
        {
            // Object is unlocked, so lock it (make it independent of marker)
            //LockObject(arObject);
        }
    }

    /// <summary>
    /// Locks an AR object, making it independent of marker tracking
    /// 
    /// HOW IT WORKS:
    /// 1. Saves the object's current position and parent (the marker)
    /// 2. Makes the object a child of the camera so it stays in screen space
    /// 3. Shows a lock icon to indicate the object is locked
    /// 
    /// STUDENT NOTE: Locked objects will stay visible even when the marker disappears!
    /// This is useful for creating persistent AR content that users can examine
    /// </summary>
    /*private void LockObject(GameObject arObject)
    {
        // Mark this object as locked in our tracking dictionary
        lockedObjects[arObject] = true;
        
        // Store the object's original transform data so we can restore it later
        // This is essential for the unlock functionality
        originalParents[arObject] = arObject.transform.parent;                    // Remember which marker it came from
        originalLocalPositions[arObject] = arObject.transform.localPosition;     // Remember its position on the marker
        originalLocalRotations[arObject] = arObject.transform.localRotation;     // Remember its rotation on the marker
        
        // Make the object a child of the camera for screen-space locking
        // The 'true' parameter preserves the world position during the parent change
        arObject.transform.SetParent(_arCamera.transform, true);

        // Look for a lock icon in the object's children and show it
        // STUDENT TIP: Add a child GameObject with tag "LockIcon" to show lock status visually
        foreach (Transform child in arObject.transform)
        {
            if (child.CompareTag("LockIcon"))
            {
                child.gameObject.SetActive(true);              // Make the lock icon visible
                lockIcons[arObject] = child.gameObject;        // Remember this icon for later
                break; // Stop looking once we find the first lock icon
            }
        }

        Debug.Log($"Locked object: {arObject.name} - now child of camera");
    }*/

    /// <summary>
    /// Unlocks an AR object, returning it to marker tracking
    /// 
    /// HOW IT WORKS:
    /// 1. Restores the object to its original parent (the marker)
    /// 2. Restores its original position and rotation on the marker
    /// 3. Hides the lock icon
    /// 4. Cleans up stored data
    /// 
    /// STUDENT NOTE: After unlocking, the object will follow its marker again
    /// and disappear when the marker is not visible
    /// </summary>
    private void UnlockObject(GameObject arObject)
    {
        // Mark the object as unlocked
        if (lockedObjects.ContainsKey(arObject))
        {
            lockedObjects[arObject] = false;
        }
        
        // Restore the object to its original parent (the marker)
        if (originalParents.ContainsKey(arObject))
        {
            // 'false' parameter means we DON'T preserve world position
            // This makes the object snap back to its position on the marker
            arObject.transform.SetParent(originalParents[arObject], false);
            
            // Restore the exact local position on the marker
            if (originalLocalPositions.ContainsKey(arObject))
            {
                arObject.transform.localPosition = originalLocalPositions[arObject];
                originalLocalPositions.Remove(arObject); // Clean up the stored data
            }
            
            // Restore the exact local rotation on the marker
            if (originalLocalRotations.ContainsKey(arObject))
            {
                arObject.transform.localRotation = originalLocalRotations[arObject];
                originalLocalRotations.Remove(arObject); // Clean up the stored data
            }
            
            // Clean up the parent reference
            originalParents.Remove(arObject);
        }
        
        // Hide the lock icon since the object is now unlocked
        if (lockIcons.ContainsKey(arObject) && lockIcons[arObject] != null)
        {
            lockIcons[arObject].SetActive(false);        // Hide the icon
            lockIcons.Remove(arObject);                   // Remove from our tracking
        }
        
        Debug.Log($"Unlocked object: {arObject.name} - restored to original parent");
    }

    /// <summary>
    /// This method is called whenever AR image tracking detects changes
    /// It handles adding new objects, updating existing ones, and removing lost objects
    /// 
    /// STUDENT NOTE: This is the heart of the AR system - it's automatically called by ARFoundation
    /// Understanding this method will help you customize your AR behavior
    /// 
    /// THE THREE PARTS:
    /// 1. eventArgs.added - New markers detected
    /// 2. eventArgs.updated - Existing markers moved or tracking state changed  
    /// 3. eventArgs.removed - Markers lost from tracking
    /// </summary>
    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // ===== PART 1: HANDLE NEWLY DETECTED MARKERS =====
        // This happens when the camera first sees a marker
        foreach (var image in eventArgs.added)
        {
            // Create a new 3D object at the marker's position
            // The 'image.transform' makes the object a child of the detected marker
            GameObject newPrefab = Instantiate(_basePrefab, image.transform);
            
            // Give the object the same name as the marker for easy identification
            newPrefab.name = image.referenceImage.name;
            
            // Try to find a matching texture for this marker
            if (loadedTextures.TryGetValue(image.referenceImage.name, out Texture2D matchingTexture))
            {
                Debug.Log($"Image added: {image.referenceImage.name}");
                
                // Apply the texture to the object's material
                Renderer renderer = newPrefab.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    // Create a copy of the material to avoid shared material issues
                    // This prevents changes from affecting other objects using the same material
                    Material materialCopy = new Material(renderer.material);
                    materialCopy.mainTexture = matchingTexture;
                    renderer.material = materialCopy;
                }
            }
            
            // Add the new object to our tracking lists
            spawnedARObjects.Add(newPrefab);                    // Track all spawned objects
            lockedObjects[newPrefab] = false;                   // Initialize as unlocked (follows marker)
        }

        // ===== PART 2: HANDLE MARKER UPDATES =====
        // This happens when existing markers move or change tracking state
        foreach (var image in eventArgs.updated)
        {
            // Find the spawned object that corresponds to this marker
            foreach (var spawned in spawnedARObjects)
            {
                // Skip objects that don't match this marker
                if (spawned.name != image.referenceImage.name)
                {
                    continue;
                }

                // Check if this object is currently locked
                bool isLocked = lockedObjects.ContainsKey(spawned) && lockedObjects[spawned];

                if (isLocked)
                {
                    // Locked objects are children of camera, always visible
                    // They don't depend on marker tracking anymore
                    spawned.SetActive(true);
                }
                else
                {
                    // Normal tracking behavior - object follows the marker
                    // Only show the object when the marker is actively being tracked
                    // TrackingState.Tracking means AR is confident about the marker's position
                    spawned.SetActive(image.trackingState == TrackingState.Tracking);
                    
                    // STUDENT TIP: You could add other tracking states here:
                    // - TrackingState.Limited: Tracking but with reduced accuracy
                    // - TrackingState.None: Not tracking at all
                }
            }
        }

        // ===== PART 3: HANDLE REMOVED MARKERS =====
        // This happens when markers are permanently lost from the AR session
        // Note: This is different from just losing tracking - this means the marker is removed from the session entirely
        foreach (var image in eventArgs.removed)
        {
            // Loop backwards through the list to safely remove items during iteration
            // STUDENT TIP: Always loop backwards when removing items from a list!
            for (int i = spawnedARObjects.Count - 1; i >= 0; i--)
            {
                // Skip objects that don't match this removed marker
                if (spawnedARObjects[i].name != image.Value.name)
                {
                    continue;
                }

                // Found the object to remove
                GameObject objectToRemove = spawnedARObjects[i];

                // Clean up the lock icon if it exists
                if (lockIcons.ContainsKey(objectToRemove) && lockIcons[objectToRemove] != null)
                {
                    lockIcons[objectToRemove].SetActive(false);    // Hide the icon
                    lockIcons.Remove(objectToRemove);              // Remove from tracking
                }

                // Clean up all tracking data for this object
                // This prevents memory leaks and null reference errors
                lockedObjects.Remove(objectToRemove);              // Remove lock state
                originalParents.Remove(objectToRemove);            // Remove parent reference
                originalLocalPositions.Remove(objectToRemove);     // Remove position data
                originalLocalRotations.Remove(objectToRemove);     // Remove rotation data

                // Destroy the GameObject and remove it from our list
                Destroy(objectToRemove);                           // Destroy the 3D object
                spawnedARObjects.RemoveAt(i);                      // Remove from our tracking list
                
                // STUDENT NOTE: This ensures complete cleanup when markers are removed
                // It's important to clean up properly to avoid memory leaks in your AR app!
            }
        }
    }
}
