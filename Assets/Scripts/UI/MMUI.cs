using UnityEngine;
using UnityEngine.UIElements;

public class MMUI : MonoBehaviour
{
    private Label _markText;
    private Button _startButton;

    public VisualElement ui;
    private UIDocument _uiDocument;
    private VisualTreeAsset _visualTreeAsset;
    public Label MarkText { get { return _markText; } private set { _markText = value; } }
    public Button StartButton { get { return _startButton; } private set { _startButton = value; } }

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _visualTreeAsset = _uiDocument.visualTreeAsset;
        ui = _uiDocument.rootVisualElement;

        _markText = ui.Q<Label>("MarkText");
        _startButton = ui.Q<Button>("StartButton");

        if (_startButton != null)
        {
            _startButton.SetEnabled(false);
            _startButton.clicked += OnStartButtonPressed;
        }
    }

    private void OnDisable()
    {
        if (_startButton != null)
        {
            _startButton.clicked -= OnStartButtonPressed;
        }
    }

    void OnStartButtonPressed()
    {
        Debug.Log("Start Button Pressed!");
        GPSController.instance.StartLandmarkHunt();
        ui.style.display = DisplayStyle.None;
    }

    public void SetStartButtonEnabled(bool isEnabled)
    {
        if (_startButton != null)
        {
            _startButton.SetEnabled(isEnabled);
        }
    }

    public void UpdateText(string message)
    {
        if (_markText != null)
        {
            _markText.text = message;
        }
    }

    public void AppendText(string message)
    {
        if (_markText != null)
        {
            _markText.text += "\n" + message;
        }
    } 
}
