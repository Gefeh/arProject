using UnityEngine;
using UnityEngine.InputSystem;
public class SwipeDetection : MonoBehaviour 
{
    public static SwipeDetection instance;
    //public delegate void Swipe(Vector2 direction);
    //public event Swipe swipePerformed;
    [SerializeField] private InputAction position, press;
    [SerializeField] private Camera _arCamera;
    [SerializeField] private float swipeResistance = 10;
    public GameObject trailObject;
    private TrailRenderer trailRenderer;
    private ParticleSystem particles;
    
    
    private Vector2 initialPos;
    private Vector2 currentPos => position.ReadValue<Vector2>();
    private Vector2 lastPos;
    private void Awake () 
    {
        trailRenderer = trailObject.GetComponent<TrailRenderer>();
        particles = trailObject.GetComponent<ParticleSystem>();
        trailRenderer.emitting = false;
        position.Enable();
        press.Enable();
        press.canceled += _ => trailRenderer.emitting = false;
        instance = this;
    }

    private void Update()
    {
        Ray ray = _arCamera.ScreenPointToRay(currentPos);
        trailObject.transform.position = ray.GetPoint(10);
        if (press.IsPressed())
        {
            if (press.WasPressedThisFrame())
            {
                initialPos = currentPos;
                trailRenderer.emitting = true;
            }
            else
            {
                Vector2 delta = currentPos - initialPos;
                if (currentPos != lastPos && delta.magnitude > swipeResistance)
                {
                    lastPos = currentPos;
                    
                    

                    // Perform raycasting to see if the ray hits any colliders
                    // Physics.Raycast returns true if something was hit, and fills the 'hit' variable with details
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        // We hit something! Get the GameObject that was hit and toggle its lock state
                        // hit.collider.gameObject gives us the GameObject that owns the collider we hit
                        ChainController chainController = hit.transform.gameObject.GetComponentInParent<ChainController>();
                        if (chainController == null) return;
                        if (chainController.IsBroken()) return;
                        hit.transform.gameObject.SetActive(false);
                        chainController.Break();
                        //particles.Emit(1);
                
                        // STUDENT TIP: You could add more logic here:
                        // - Check if the hit object is actually an AR object
                        // - Add visual feedback (highlight, particles, etc.)
                        // - Play sound effects
                        // - Handle different types of objects differently
                
                        // DEBUGGING TIP: Uncomment the line below to see what you're hitting in the Console
                        Debug.Log($"Hit object: {hit.collider.gameObject.name}");
                    }
                }
            }
        }
    }

    /*private void DetectSwipe () 
    {
        Vector2 delta = currentPos - initialPos;
        Vector2 direction = Vector2.zero;

        if(Mathf.Abs(delta.x) > swipeResistance)
        {
            direction.x = Mathf.Clamp(delta.x, -1, 1);
        }
        if(Mathf.Abs(delta.y) > swipeResistance)
        {
            direction.y = Mathf.Clamp(delta.y, -1, 1);
        }
        Debug.Log("SwipePerformed: " + direction);
    }*/
}