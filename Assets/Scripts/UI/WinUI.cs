using UnityEngine;
using UnityEngine.UIElements;

public class WinUI : MonoBehaviour
{
    private Label _bigText;
    private Label _smallText;

    public VisualElement ui;
    private UIDocument _uiDocument;
    private VisualTreeAsset _visualTreeAsset;
    public Label BigText { get { return _bigText; } private set { _bigText = value; } }
    public Label SmallText { get { return _smallText; } private set { _smallText = value; } }

    private void OnEnable()
    {
        _uiDocument = GetComponent<UIDocument>();
        _visualTreeAsset = _uiDocument.visualTreeAsset;
        ui = _uiDocument.rootVisualElement;

        _bigText = ui.Q<Label>("BigText");
        _smallText = ui.Q<Label>("SmallText");
        ui.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {

    }

    public void UpdateText(Label text, string message)
    {
        if (text != null)
        {
            text.text = message;
        }
    }

    public void AppendText(Label text, string message)
    {
        if (text != null)
        {
            text.text += "\n" + message;
        }
    }

    public void EnableUI()
    {
        ui.style.display = DisplayStyle.Flex;
    }
}
