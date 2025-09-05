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

    [Header("Game")]

    [SerializeField] private int _currentChallengeCount;
    [SerializeField] private int _maxChallengeCount = 3;
    [SerializeField] private GameObject _challengeOrigin;

    public GameUI GameUI { get { return _gameUI; } private set { _gameUI = value; } }
    public GPSController GPSController { get { return _gpsController; } private set { _gpsController = value; } }
    public MMUI MMUI { get { return _mmui; } private set { _mmui = value; } }
    public GameObject ChallengeOrigin { get { return _challengeOrigin; } private set { _challengeOrigin = value; } }
    public int CurrentChallengeCount { get { return _currentChallengeCount; } set { _currentChallengeCount = value; } }
    public int MaxChallengeCount { get { return _maxChallengeCount; } private set { _maxChallengeCount = value; } }

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
