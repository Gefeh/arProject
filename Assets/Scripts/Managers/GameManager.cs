using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]

    [SerializeField] private UIDocument _gameUI;

    [Header("GPS")]

    [SerializeField] private GPSController _gpsController;

    public UIDocument GameUI { get { return _gameUI; } private set { _gameUI = value; } }
    public GPSController GPSController { get { return _gpsController; } private set { _gpsController = value; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }


}
