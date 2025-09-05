using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]

    [SerializeField] private GameUI _gameUI;
    [SerializeField] private MMUI _mmui;

    [Header("GPS")]

    [SerializeField] private GPSController _gpsController;

    public GameUI GameUI { get { return _gameUI; } private set { _gameUI = value; } }
    public GPSController GPSController { get { return _gpsController; } private set { _gpsController = value; } }
    public MMUI MMUI { get { return _mmui; } private set { _mmui = value; } }

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
