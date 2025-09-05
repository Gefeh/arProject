using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    private SequenceGame gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<SequenceGame>();
    }

    void Update()
    {
        // Works for both mouse (Editor) and touch (AR)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    if (gameManager != null)
                    {
                        gameManager.ObjectClicked(gameObject);
                    }
                }
            }
        }
    }
}
