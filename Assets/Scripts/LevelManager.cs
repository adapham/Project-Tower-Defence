using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    // Fungsi Singleton
    private static LevelManager _instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager> ();
            }
            return _instance;
        }
    }

    [SerializeField] private Transform _towerUIParent;
    [SerializeField] private GameObject _towerUIPrefab;
    [SerializeField] private Tower[] _towerPrefabs;

    private List<Tower> _spawnedTowers = new List<Tower> ();

    [SerializeField] private Enemy[] _enemyPrefabs;
    [SerializeField] private Transform[] _enemyPaths;
    //[SerializeField] private float _spawnDelay = 5f;

    //[SerializeField] private int _maxEnemiesInScene = 5;
    private Queue<Enemy> _spawnedEnemiesQueue = new Queue<Enemy>();
    private float _runningSpawnDelay;

    private List<Bullet> _spawnedBullets = new List<Bullet> ();

    public bool IsOver { get; private set; }

    //[SerializeField] private int _maxLives = 3;
    //[SerializeField] private int _totalEnemy = 15;

    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    private int _currentLives;
    private int _enemyCounter;

    //Set Data IO
    // configuration data
    static ConfigurationData configurationData;
    public static float _spawnDelay
    {
        get
        {
            return configurationData.SpawnDelay;
        }
    }
    public static int _maxEnemiesInScene
    {
        get
        {
            return configurationData.MaxEnemiesInScene;
        }
    }
    static int _maxLives
    {
        get
        {
            return configurationData.MaxLives;
        }
    }
    static int _totalEnemy
    {
        get { 
            return configurationData.TotalEnemy;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        configurationData = new ConfigurationData();
        Debug.Log("1: " + _spawnDelay);
        Debug.Log("2: " + _maxEnemiesInScene);
        Debug.Log("3: " + _maxLives);
        Debug.Log("4: " + _totalEnemy);

        SetCurrentLives (_maxLives);
        SetTotalEnemy (_totalEnemy);
        InstantiateAllTowerUI ();

    }

    // Update is called once per frame
    private void Update()
    {
        // Jika menekan tombol R, fungsi restart akan terpanggil
        if (Input.GetKeyDown (KeyCode.R))
        {
            SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
        }

        if (IsOver)
        {
            return;
        }

        // Counter untuk spawn enemy dalam jeda waktu yang ditentukan
        // Time.unscaledDeltaTime adalah deltaTime yang independent, tidak terpengaruh oleh apapun kecuali game object itu sendiri,
        // jadi bisa digunakan sebagai penghitung waktu
        if (_spawnedEnemiesQueue.Count < _maxEnemiesInScene)
        {
            _runningSpawnDelay -= Time.unscaledDeltaTime;
            if (_runningSpawnDelay <= 0f)
            {
                SpawnEnemy();
                _runningSpawnDelay = _spawnDelay;
            }
        }


        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy (_spawnedEnemiesQueue);
            tower.SeekTarget ();
            tower.ShootTarget ();
        }

        // Xử lý các enemy trong queue
        int enemiesCount = _spawnedEnemiesQueue.Count;

        for (int i = 0; i < enemiesCount; i++)
        {
            Enemy enemy = _spawnedEnemiesQueue.Peek();
            if (!enemy.gameObject.activeSelf)
            {
                _spawnedEnemiesQueue.Dequeue();
                continue;
            }

            // Kenapa nilainya 0.1? Karena untuk lebih mentoleransi perbedaan posisi,
            // akan terlalu sulit jika perbedaan posisinya harus 0 atau sama persis
            if (Vector2.Distance (enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex (enemy.CurrentPathIndex + 1);
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    enemy.SetTargetPosition (_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    ReduceLives (1);
                    enemy.gameObject.SetActive (false);
                }
            }

            else
            {
                enemy.MoveToTarget ();
            }

            _spawnedEnemiesQueue.Enqueue(enemy); // Enqueue enemy vừa được xử lý vào cuối queue
            _spawnedEnemiesQueue.Dequeue(); // Dequeue enemy đầu tiên để xử lý enemy tiếp theo trong queue
        }
    }

    // Menampilkan seluruh Tower yang tersedia pada UI Tower Selection
    private void InstantiateAllTowerUI ()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate (_towerUIPrefab.gameObject, _towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI> ();
            newTowerUI.SetTowerPrefab (tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    // Mendaftarkan Tower yang di-spawn agar bisa dikontrol oleh LevelManager
    public void RegisterSpawnedTower (Tower tower)
    {
        _spawnedTowers.Add (tower);
    }

    private void SpawnEnemy ()
    {
        Debug.Log("Run Spawn");
        SetTotalEnemy (--_enemyCounter);
        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemiesQueue.FirstOrDefault(e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver (true);
            }
            return;
        }

        int randomIndex = Random.Range (0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString ();
        GameObject newEnemyObj = _spawnedEnemiesQueue.FirstOrDefault(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate (_enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy> ();
        if (!_spawnedEnemiesQueue.Contains (newEnemy))
        {
            _spawnedEnemiesQueue.Enqueue(newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition (_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex (1);
        newEnemy.gameObject.SetActive (true);
    }

    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos ()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine (_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

    public Bullet GetBulletFromPool (Bullet prefab)
    {
        GameObject newBulletObj = _spawnedBullets.Find (b => !b.gameObject.activeSelf && b.name.Contains (prefab.name))?.gameObject;
        if (newBulletObj == null)
        {
            newBulletObj = Instantiate (prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet> ();
        if (!_spawnedBullets.Contains (newBullet))
        {
            _spawnedBullets.Add (newBullet);
        }

        return newBullet;
    }

    //Kích hoạt giảm máu quái
    public void ExplodeAt (Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemiesQueue)
        {
            if (enemy.gameObject.activeSelf)
            {
                if (Vector2.Distance (enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth (damage);
                }
            }
        }
    }

    public void ReduceLives (int value)
    {
        SetCurrentLives (_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver (false);
        }
    }

    public void SetCurrentLives (int currentLives)
    {
        // Mathf.Max fungsi nya adalah mengambil angka terbesar
        // sehingga _currentLives di sini tidak akan lebih kecil dari 0
        _currentLives = Mathf.Max (currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy (int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max (_enemyCounter, 0)}";
    }

    public void SetGameOver (bool isWin)
    {
        IsOver = true;
        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive (true);
    }
}
