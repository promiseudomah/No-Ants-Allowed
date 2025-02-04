using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Space, SerializeField] Player m_Player;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI m_WaveText;
    [SerializeField] private TextMeshProUGUI m_EnemiesKilledText;
    [SerializeField] private TextMeshProUGUI m_EnemiesEscapedText;
    [SerializeField] private TextMeshProUGUI m_EnemiesRemainingText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject youWinPanel;
    [SerializeField] private GameObject startPanel;

    [Header("Wave Progress UI")]
    [SerializeField] private Slider waveProgressBar;

    int m_TotalWaves;
    int m_CurrentWave;
    [HideInInspector] public bool isGameOver = false;
    int totalEnemiesSpawned = 0;
    int totalEnemiesToSpawn = 0;
    int m_EnemiesKilled = 0;
    int m_EnemiesEscaped = 0;
    bool isPaused = false;

    [HideInInspector] public UnityEvent OnGameStarted;

    EnemySpawner enemySpawner;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        enemySpawner = EnemySpawner.Instance;
        m_TotalWaves = enemySpawner.m_TotalWaves;
        m_CurrentWave = enemySpawner.m_CurrentWave;

        CalculateTotalEnemies();
        UpdateUI();

        startPanel.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){

            Menu();
        }
    }

    public void TrackEnemySpawn()
    {

        totalEnemiesSpawned ++;
        UpdateWaveProgress();
    }
    public void TrackEnemyKill()
    {

        m_EnemiesKilled++;
        UpdateUI();
    }

    public void TrackEnemyEscape()
    {

        m_EnemiesEscaped++;
        UpdateUI();
    }

    public void NextWave()
    {

        Invoke(nameof(LoadNextWave), 1f);
    }

    void LoadNextWave(){

        m_CurrentWave++;

        enemySpawner.StartGame();
        CalculateTotalEnemies();
        UpdateUI();
    }

    public void GameOver(bool won)
    {
        if(!isGameOver) StartCoroutine(DelayGameOver(won));
    }

    public void GameOverImmediate(bool won)
    {
        if(!isGameOver) StartCoroutine(DelayGameOver(won, 0));
    }

    IEnumerator DelayGameOver(bool won, float time = 1.5f){

        yield return new WaitForSeconds(time);
        isGameOver = true;

        if(won){

            youWinPanel.SetActive(true);
        }

        else{

            gameOverPanel.SetActive(true);
        }
        
        enemySpawner.StopAllCoroutines();
        enemySpawner.CancelInvoke();
        enemySpawner.KillAllAnts();
        
        enemySpawner.enabled = false;
        enabled = false;
    }


    void UpdateUI()
    {

        m_WaveText.text = $"Wave: {m_CurrentWave + 1}/{m_TotalWaves}";

        m_EnemiesKilledText.text = $"Ants killed: {m_EnemiesKilled}/{totalEnemiesToSpawn}";
        m_EnemiesEscapedText.text = $"Ants that escaped: {m_EnemiesEscaped}/{totalEnemiesToSpawn}";
        m_EnemiesRemainingText.text = $"Ants remaining: {Mathf.Abs(enemySpawner.m_EnemiesRemaining)}";

        UpdateWaveProgress();
    }

    void CalculateTotalEnemies()
    {
       
        totalEnemiesToSpawn += enemySpawner.GetEnemiesForWave();
        UpdateUI();
    }

    void UpdateWaveProgress()
    {
        Invoke(nameof(DelayUpdateWaveProgress), 1f);
    }

    void DelayUpdateWaveProgress(){

        waveProgressBar.value = (float)(m_EnemiesKilled + m_EnemiesEscaped) / totalEnemiesToSpawn * ((float)(m_CurrentWave + 1) / m_TotalWaves);

    }

    public void StartGame(){

        startPanel.SetActive(false);
        enemySpawner.StartGame();
        OnGameStarted.Invoke();
    }

    public void Menu(){

        SceneManager.LoadScene(0);
    }

    public void Retry(){

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Next()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int totalScenes = SceneManager.sceneCountInBuildSettings;

        if (currentSceneIndex + 1 < totalScenes)
        {

            SceneManager.LoadScene(currentSceneIndex + 1);
        }

        else{

            Menu();
        }
    }
}
