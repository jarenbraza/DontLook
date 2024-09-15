using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MenuScript : MonoBehaviour
{
    private Button playButton;
    private Button optionsButton;
    private Button quitButton;
    
    void Awake() {
        Debug.developerConsoleVisible = true;

        var ui = GetComponent<UIDocument>().rootVisualElement;

        playButton = ui.Q<Button>("PlayButton");
        optionsButton = ui.Q<Button>("OptionsButton");
        quitButton = ui.Q<Button>("QuitButton");
    }

    void OnEnable() {
        playButton.RegisterCallback<ClickEvent>(PlayButtonClicked);
        optionsButton.RegisterCallback<ClickEvent>(OptionsButtonClicked);
        quitButton.RegisterCallback<ClickEvent>(QuitButtonClicked);
    }

    void OnDisable() {
        playButton.UnregisterCallback<ClickEvent>(PlayButtonClicked);
    }

    void PlayButtonClicked(ClickEvent evt) {
        Debug.Log($"Button {playButton.name} clicked!");
        SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }

    void OptionsButtonClicked(ClickEvent evt) {
        Debug.Log($"Button {optionsButton.name} clicked!");
    }

    void QuitButtonClicked(ClickEvent evt) {
        Debug.Log($"Button {quitButton.name} clicked!");
        Application.Quit();
    }
}
