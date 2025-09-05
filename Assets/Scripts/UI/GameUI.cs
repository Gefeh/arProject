using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    private Label _bottomText;

    public VisualElement ui;
    private UIDocument _uiDocument;
    private VisualTreeAsset _visualTreeAsset;
    public Label BottomText { get { return _bottomText; } private set { _bottomText = value; } }

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _visualTreeAsset = _uiDocument.visualTreeAsset;
        ui = _uiDocument.rootVisualElement;

        _bottomText = ui.Q<Label>("QuestLog");
        ui.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        
    }

    public void UpdateText(string message)
    {
        if (_bottomText != null)
        {
            _bottomText.text = message;
        }
    }

    public void AppendText(string message)
    {
        if (_bottomText != null)
        {
            _bottomText.text += "\n" + message;
        }
    }

    public void EnableUI()
    {
        ui.style.display = DisplayStyle.Flex;
    }

}
