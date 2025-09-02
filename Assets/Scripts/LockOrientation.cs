using UnityEngine;

/// <summary>
/// LockOrientation - A simple utility script for AR applications
/// 
/// PURPOSE:
/// This script forces the device to stay in landscape orientation, which is often
/// preferred for AR experiences as it provides a wider field of view and better
/// stability for tracking markers.
/// 
/// USAGE FOR STUDENTS:
/// 1. Attach this script to any GameObject in your scene (commonly the Main Camera or an empty GameObject)
/// 2. The orientation will be locked automatically when the scene starts
/// 3. You can modify the orientation settings below to suit your AR game's needs
/// 
/// CUSTOMIZATION IDEAS:
/// - Change to ScreenOrientation.Portrait for portrait-mode AR games
/// - Allow multiple orientations by setting autorotate flags to true
/// - Add UI buttons to let users toggle orientation during gameplay
/// </summary>
public class LockOrientation : MonoBehaviour
{
    /// <summary>
    /// Start is called once when the GameObject is first created
    /// This is where we set up the screen orientation for our AR game
    /// </summary>
    void Start()
    {
        // Force the screen to landscape left orientation
        // This is ideal for AR marker tracking as it provides a wide horizontal view
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // Disable automatic rotation to portrait modes
        // Portrait modes can make AR tracking less stable and reduce the visible area
        Screen.autorotateToPortrait = false;                    // Blocks normal portrait
        Screen.autorotateToPortraitUpsideDown = false;          // Blocks upside-down portrait

        // Enable landscape orientations
        Screen.autorotateToLandscapeLeft = true;                // Allows landscape left (our main orientation)
        Screen.autorotateToLandscapeRight = false;              // Blocks landscape right (you can enable this if desired)
        
        // STUDENT TIP: Try changing these settings to see how they affect your AR experience!
        // For example, set autorotateToLandscapeRight = true to allow both landscape orientations
    }
}