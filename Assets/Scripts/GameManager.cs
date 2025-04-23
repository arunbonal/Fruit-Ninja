using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Blade blade;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text comboText;
    
    // Game over UI elements
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button restartButton;
    
    // Power-up UI elements
    [SerializeField] private GameObject powerUpIndicator;
    [SerializeField] private Text powerUpText;
    [SerializeField] private Image powerUpDurationBar;
    
    // Pause menu elements
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    public int score { get; private set; } = 0;
    private bool isPaused = false;
    
    // Combo system variables
    private int comboCount = 0;
    private float comboTimeWindow = 1.0f;
    private float lastSliceTime = 0f;
    private Coroutine comboCoroutine;
    
    // Power-up system variables
    private bool isSlowMotionActive = false;
    private bool isScoreMultiplierActive = false;
    private bool isFruitFrenzyActive = false;
    private float scoreMultiplier = 1f;
    private float normalTimeScale = 1f;
    private Coroutine powerUpCoroutine;

    private void Awake()
    {
        if (Instance != null) {
            DestroyImmediate(gameObject);
        } else {
            Instance = this;
        }
        
        // Add listeners to buttons
        if (restartButton != null) {
            restartButton.onClick.AddListener(NewGame);
        }
        
        if (resumeButton != null) {
            resumeButton.onClick.AddListener(TogglePause);
        }
        
        if (mainMenuButton != null) {
            mainMenuButton.onClick.AddListener(NewGame);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void Start()
    {
        NewGame();
    }
    
    private void Update()
    {
        // Check for pause input
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    private void NewGame()
    {
        // Ensure the game is not paused when starting a new game
        if (isPaused) {
            TogglePause();
        }
        
        Time.timeScale = 1f;
        normalTimeScale = 1f;

        ClearScene();

        blade.enabled = true;
        spawner.enabled = true;

        score = 0;
        comboCount = 0;
        scoreText.text = score.ToString();
        
        if (comboText != null) {
            comboText.text = "";
            comboText.gameObject.SetActive(false);
        }
        
        // Hide UI panels
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(false);
        }
        
        if (pausePanel != null) {
            pausePanel.SetActive(false);
        }
        
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(false);
        }
        
        // Reset power-up states
        isSlowMotionActive = false;
        isScoreMultiplierActive = false;
        isFruitFrenzyActive = false;
        scoreMultiplier = 1f;
    }
    
    public void TogglePause()
    {
        isPaused = !isPaused;
        
        if (isPaused)
        {
            // Save current time scale in case a power-up is active
            normalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            
            // Disable the blade and show pause menu
            blade.enabled = false;
            
            if (pausePanel != null) {
                pausePanel.SetActive(true);
            }
        }
        else
        {
            // Restore time scale and enable blade
            Time.timeScale = normalTimeScale;
            blade.enabled = true;
            
            if (pausePanel != null) {
                pausePanel.SetActive(false);
            }
        }
    }

    private void ClearScene()
    {
        Fruit[] fruits = FindObjectsOfType<Fruit>();

        foreach (Fruit fruit in fruits) {
            Destroy(fruit.gameObject);
        }

        Bomb[] bombs = FindObjectsOfType<Bomb>();

        foreach (Bomb bomb in bombs) {
            Destroy(bomb.gameObject);
        }
        
        PowerUp[] powerUps = FindObjectsOfType<PowerUp>();
        
        foreach (PowerUp powerUp in powerUps) {
            Destroy(powerUp.gameObject);
        }
    }

    public void IncreaseScore(int points)
    {
        // Combo system logic
        float currentTime = Time.time;
        if (currentTime - lastSliceTime < comboTimeWindow) {
            comboCount++;
            
            // Apply combo bonus (more points for higher combos)
            int comboBonus = Mathf.Min(comboCount, 5); // Cap combo bonus at 5x
            points *= comboBonus;
            
            // Show combo text
            if (comboText != null) {
                comboText.text = comboCount + "x COMBO!";
                comboText.gameObject.SetActive(true);
            }
            
            // Reset combo countdown
            if (comboCoroutine != null) {
                StopCoroutine(comboCoroutine);
            }
            comboCoroutine = StartCoroutine(ResetComboAfterDelay());
        } else {
            comboCount = 1;
            if (comboText != null) {
                comboText.gameObject.SetActive(false);
            }
        }
        
        lastSliceTime = currentTime;
        
        // Apply score multiplier if active
        if (isScoreMultiplierActive) {
            points = Mathf.RoundToInt(points * scoreMultiplier);
        }
        
        // Add points to score
        score += points;
        scoreText.text = score.ToString();

        float hiscore = PlayerPrefs.GetFloat("hiscore", 0);

        if (score > hiscore)
        {
            hiscore = score;
            PlayerPrefs.SetFloat("hiscore", hiscore);
        }
    }
    
    private IEnumerator ResetComboAfterDelay() {
        yield return new WaitForSeconds(comboTimeWindow);
        comboCount = 0;
        if (comboText != null) {
            comboText.gameObject.SetActive(false);
        }
    }

    public void Explode()
    {
        blade.enabled = false;
        spawner.enabled = false;

        StartCoroutine(ExplodeSequence());
    }

    private IEnumerator ExplodeSequence()
    {
        float elapsed = 0f;
        float duration = 0.5f;

        // Fade to white
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.clear, Color.white, t);

            Time.timeScale = 1f - t;
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);
        
        // Show game over screen instead of immediately starting a new game
        ShowGameOver();

        elapsed = 0f;

        // Fade back in
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.white, Color.clear, t);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }
    
    private void ShowGameOver()
    {
        // Update final score and high score texts
        if (finalScoreText != null) {
            finalScoreText.text = "Score: " + score;
        }
        
        if (highScoreText != null) {
            float highScore = PlayerPrefs.GetFloat("hiscore", 0);
            highScoreText.text = "High Score: " + Mathf.RoundToInt(highScore);
        }
        
        // Show game over panel
        if (gameOverPanel != null) {
            gameOverPanel.SetActive(true);
        }
    }
    
    // Power-up activation methods
    public void ActivateSlowMotion(float duration)
    {
        // Stop any existing power-up coroutine
        if (powerUpCoroutine != null) {
            StopCoroutine(powerUpCoroutine);
        }
        
        // Start new power-up
        powerUpCoroutine = StartCoroutine(SlowMotionPowerUp(duration));
    }
    
    public void ActivateScoreMultiplier(float duration)
    {
        // Stop any existing power-up coroutine
        if (powerUpCoroutine != null) {
            StopCoroutine(powerUpCoroutine);
        }
        
        // Start new power-up
        powerUpCoroutine = StartCoroutine(ScoreMultiplierPowerUp(duration));
    }
    
    public void ActivateFruitFrenzy(float duration)
    {
        // Stop any existing power-up coroutine
        if (powerUpCoroutine != null) {
            StopCoroutine(powerUpCoroutine);
        }
        
        // Start new power-up
        powerUpCoroutine = StartCoroutine(FruitFrenzyPowerUp(duration));
    }
    
    private IEnumerator SlowMotionPowerUp(float duration)
    {
        // Set up power-up
        isSlowMotionActive = true;
        normalTimeScale = Time.timeScale;
        Time.timeScale = 0.5f;
        
        // Update UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(true);
        }
        
        if (powerUpText != null) {
            powerUpText.text = "SLOW MOTION";
        }
        
        // Show duration
        float timer = duration;
        while (timer > 0)
        {
            // Don't count down while paused
            if (!isPaused) {
                if (powerUpDurationBar != null) {
                    powerUpDurationBar.fillAmount = timer / duration;
                }
                
                timer -= Time.unscaledDeltaTime;
            }
            yield return null;
        }
        
        // Reset time scale if not paused
        if (!isPaused) {
            Time.timeScale = normalTimeScale;
        }
        isSlowMotionActive = false;
        
        // Hide UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(false);
        }
    }
    
    private IEnumerator ScoreMultiplierPowerUp(float duration)
    {
        // Set up power-up
        isScoreMultiplierActive = true;
        scoreMultiplier = 2f;
        
        // Update UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(true);
        }
        
        if (powerUpText != null) {
            powerUpText.text = "2X POINTS";
        }
        
        // Show duration
        float timer = duration;
        while (timer > 0)
        {
            // Don't count down while paused
            if (!isPaused) {
                if (powerUpDurationBar != null) {
                    powerUpDurationBar.fillAmount = timer / duration;
                }
                
                timer -= Time.unscaledDeltaTime;
            }
            yield return null;
        }
        
        // Reset multiplier
        isScoreMultiplierActive = false;
        scoreMultiplier = 1f;
        
        // Hide UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(false);
        }
    }
    
    private IEnumerator FruitFrenzyPowerUp(float duration)
    {
        // Set up power-up
        isFruitFrenzyActive = true;
        
        // Temporarily modify spawner settings to create fruit frenzy
        float originalMinDelay = spawner.minSpawnDelay;
        float originalMaxDelay = spawner.maxSpawnDelay;
        float originalBombChance = spawner.bombChance;
        
        spawner.minSpawnDelay = 0.1f;
        spawner.maxSpawnDelay = 0.3f;
        spawner.bombChance = 0.01f;
        
        // Update UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(true);
        }
        
        if (powerUpText != null) {
            powerUpText.text = "FRUIT FRENZY!";
        }
        
        // Show duration
        float timer = duration;
        while (timer > 0)
        {
            // Don't count down while paused
            if (!isPaused) {
                if (powerUpDurationBar != null) {
                    powerUpDurationBar.fillAmount = timer / duration;
                }
                
                timer -= Time.unscaledDeltaTime;
            }
            yield return null;
        }
        
        // Reset spawner settings
        spawner.minSpawnDelay = originalMinDelay;
        spawner.maxSpawnDelay = originalMaxDelay;
        spawner.bombChance = originalBombChance;
        isFruitFrenzyActive = false;
        
        // Hide UI
        if (powerUpIndicator != null) {
            powerUpIndicator.SetActive(false);
        }
    }
}
