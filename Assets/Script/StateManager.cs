using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { Menu, Game }

public class StateManager : MonoBehaviour
{
    public static StateManager Instance;

    // Flag to determine if a new game was started (used when reloading the scene)
    public static bool newGameStarted = false;

    [Header("UI References")]
    // Panel for the menu (pause/intro) UI.
    public GameObject pauseScreenUI;
    // Panel for the in-game UI (player stats, etc.).
    public GameObject gamePanel;
    // Reference to the Audio Toggle Button (assign via Inspector)
    public Button audioToggleButton;
    // Reference to the dedicated ESC button (assign via Inspector)
    public GameObject escButton;
    // UI Text for showing academy info (or any other in-game info).
    public TextMeshProUGUI academyInfoText;

    [Header("End Game UI")]
    // Panel for the win screen.
    public GameObject winScreen;
    // Panel for the lose screen.
    public GameObject loseScreen;

    public GameState currentState;

    // This flag indicates if the game has been started at least once.
    private bool hasGameStarted = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // Uncomment if you want this to persist across scene loads:
            // DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        // Ensure the dedicated ESC button is hidden initially.
        if (escButton != null)
        {
            escButton.SetActive(false);
        }

        if (newGameStarted)
        {
            // After a scene reload, resume the game.
            newGameStarted = false;
            SetState(GameState.Game);
            hasGameStarted = true;
            UpdateEscButtonVisibility();
        }
        else
        {
            // Start in Menu state.
            SetState(GameState.Menu);
        }
    }

    void Update()
    {
        // Allow toggling via ESC key only if the game has been started.
        if (hasGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Toggles between Game and Menu states.
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Game)
        {
            SetState(GameState.Menu);
        }
        else if (currentState == GameState.Menu)
        {
            SetState(GameState.Game);
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Menu:
                Time.timeScale = 0f; // Pause the game.
                if (pauseScreenUI != null)
                    pauseScreenUI.SetActive(true);
                if (gamePanel != null)
                    gamePanel.SetActive(false);
                // Also hide win/lose panels.
                if (winScreen != null)
                    winScreen.SetActive(false);
                if (loseScreen != null)
                    loseScreen.SetActive(false);
                break;

            case GameState.Game:
                Time.timeScale = 1f; // Resume the game.
                if (pauseScreenUI != null)
                    pauseScreenUI.SetActive(false);
                if (gamePanel != null)
                    gamePanel.SetActive(true);
                break;
        }
    }

    // Called when clicking the NEW GAME button.
    public void StartNewGame()
    {
        if (!hasGameStarted)
        {
            // Initial startup: simply resume the already loaded game.
            SetState(GameState.Game);
            hasGameStarted = true;
            UpdateEscButtonVisibility();
        }
        else
        {
            // In subsequent uses, restart the game by reloading the scene.
            newGameStarted = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // Called when clicking the EXIT GAME button.
    public void ExitGame()
    {
        Application.Quit();
        // Note: Application.Quit() does not work in the editor.
    }

    // Called when clicking the AUDIO TOGGLE button.
    public void ToggleAudio()
    {
        // Toggle the pause state of the AudioListener.
        AudioListener.pause = !AudioListener.pause;

        // Update the button color based on the audio state.
        if (audioToggleButton != null)
        {
            Color targetColor = AudioListener.pause ? Color.gray : Color.white;
            audioToggleButton.image.color = targetColor;
        }
    }

    /// <summary>
    /// Updates the visibility of the dedicated ESC button.
    /// It is only active if the game has been started.
    /// </summary>
    private void UpdateEscButtonVisibility()
    {
        if (escButton != null)
        {
            escButton.SetActive(hasGameStarted);
        }
    }

    /// <summary>
    /// Shows the win screen, pauses the game, and returns to the menu after a delay.
    /// </summary>
    public void ShowWinScreen()
    {
        if (winScreen != null)
            winScreen.SetActive(true);
        if (loseScreen != null)
            loseScreen.SetActive(false);
        Time.timeScale = 0f;
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    /// <summary>
    /// Shows the lose screen, pauses the game, and returns to the menu after a delay.
    /// </summary>
    public void ShowLoseScreen()
    {
        if (loseScreen != null)
            loseScreen.SetActive(true);
        if (winScreen != null)
            winScreen.SetActive(false);
        Time.timeScale = 0f;
        StartCoroutine(ReturnToMenuAfterDelay());
    }

    /// <summary>
    /// Waits a few seconds, hides the win/lose panels, and then returns the game to the Menu state.
    /// </summary>
    private IEnumerator ReturnToMenuAfterDelay()
    {
        // Wait for 3 seconds in real time (since timeScale is 0)
        yield return new WaitForSecondsRealtime(2f);
        // Hide win/lose panels before returning to the menu.
        if (winScreen != null)
            winScreen.SetActive(false);
        if (loseScreen != null)
            loseScreen.SetActive(false);
        Time.timeScale = 1f; // Resume time
        SetState(GameState.Menu);
    }
}
