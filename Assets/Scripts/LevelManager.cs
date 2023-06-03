using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    //Tạo 1 đối tượng singleton tồn tại duy nhất trong game
    private static LevelManager _instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }
            return _instance;
        }
    }

    //Tạo transform của đối tượng cha cho các TowerUI
    [SerializeField] private Transform _towerUIParent;
    //GameObject mẫu để tạo các đối tượng TowerUI
    [SerializeField] private GameObject _towerUIPrefab;
    //Một mảng chứa các đối tượng Tower prefabs
    [SerializeField] private Tower[] _towerPrefabs;

    //Danh sách chứa các đối tượng Tower được tạo ra
    private List<Tower> _spawnedTowers = new List<Tower>();

    //Một mảng chứa các đối tượng Enemy prefabs
    [SerializeField] private Enemy[] _enemyPrefabs;
    //Một mảng chứa các Transform đại diện cho các đường đi của Enemy
    [SerializeField] private Transform[] _enemyPaths;
    //Thời gian trễ giữa mỗi lần Enemy xuất hiện
    [SerializeField] private float _spawnDelay = 5f;

    //Danh sách chứa các đối tượng Enemy đã được tạo ra
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    //Thời gian còn lại cho mỗi lần Enemy xuất hiện tiếp theo
    private float _runningSpawnDelay;

    //Danh sách chứa các đối tượng Bullet đã được tạo ra
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    public bool IsOver { get; private set; }

    //Số lượng Enemy đi qua map tối đa là thua
    [SerializeField] private int _maxLives = 3;
    //Số lượng Enemy cần tiêu diệt
    [SerializeField] private int _totalEnemy = 15;

    //4 đối tượng UI để hiển thị trong game
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;

    //Số lượng Enemy còn được phép đi qua map và số lượng Enemy còn lại để tiêu diệt
    private int _currentLives;
    private int _enemyCounter;

    // Set giá trị cho _currentLives, _enemyCounter bằng _maxLives, _totalEnemy
    //Tạo các đối tượng TowerUI
    private void Start()
    {
        SetCurrentLives(_maxLives);
        SetTotalEnemy(_totalEnemy);
        InstantiateAllTowerUI();
    }

    //Được call mỗi frame trong game
    //Quản lý việc xuất hiện Enemy và điều khiển hoạt động của các đối tượng Tower và Enemy
    private void Update()
    {
        //Ấn R để load lại game
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        //Check xem game kết thúc hay chưa
        if (IsOver)
        {
            return;
        }


        _runningSpawnDelay -= Time.unscaledDeltaTime;
        if (_runningSpawnDelay <= 0f)
        {
            //Tạo ra một đối tượng Enemy mới và set giá trị cho _runningSpawnDelay = _spawnDelay = 5f
            SpawnEnemy();
            _runningSpawnDelay = _spawnDelay;
        }

        //Duyệt các đối tượng Tower trong danh sách _spawnedTowers
        //Check và thực hiện các họat động của đối tượng Tower
        foreach (Tower tower in _spawnedTowers)
        {
            //Check Enemy gần nhất
            tower.CheckNearestEnemy(_spawnedEnemies);
            //Tìm kiếm Enemy
            tower.SeekTarget();
            //Bắn Enemy
            tower.ShootTarget();
        }

        //Duyệt qua các đối tượng trong danh sách _spawnedEnemies
        //Check và thực hiện các hoạt động của mỗi đối tượng Enemy
        foreach (Enemy enemy in _spawnedEnemies)
        {
            //Check đối tượng Enemy có đang hoạt động hay không?
            if (!enemy.gameObject.activeSelf)
            {
                _spawnedEnemiesQueue.Dequeue();
                continue;
            }

            //Tính toán và so sánh khoảng cách giữa vị trí hiện tại và vị trí mục tiêu có < 0.1f?
            //Nếu đúng thì Enemy đã đạt đến vị trí mục tiêu
            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                //Chuyển đến vị trí tiếp theo trên đường đi của Enemy
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex + 1);
                //Check giá trị CurrentPathIndex của Enemy có nhỏ hơn số lượng phần tử trong mảng _enemyPaths?
                if (enemy.CurrentPathIndex < _enemyPaths.Length)
                {
                    //Set giá trị mới cho TargetPosition của Enemy dựa trên vị trí trong mảng _enemyPaths tương ứng với CurrentPathIndex
                    enemy.SetTargetPosition(_enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    //Enemy đi quá map call đến ReduceLives(1) để trừ đi 1 lượt Enemy đi qua map
                    ReduceLives(1);
                    //Enemy đi qua map thì mất đi
                    enemy.gameObject.SetActive(false);
                }
            }

            //Enemy vẫn chưa đạt đến vị trí mục tiêu
            else
            {
                //MoveToTarget() được gọi để di chuyển Enemy đến vị trí mục tiêu
                enemy.MoveToTarget();
            }

            _spawnedEnemiesQueue.Enqueue(enemy); // Enqueue enemy vừa được xử lý vào cuối queue
            _spawnedEnemiesQueue.Dequeue(); // Dequeue enemy đầu tiên để xử lý enemy tiếp theo trong queue
        }
    }

    //Tạo ra UI cho các đối tượng Tower
    private void InstantiateAllTowerUI()
    {
        foreach (Tower tower in _towerPrefabs)
        {
            //Tạo ra những bản sao clone của _towerUIPrefab và đặt trong _towerUIParent
            //_towerUIParent là một đối tượng cha cho UI Tower, nơi các UI Tower con sẽ được sắp xếp
            GameObject newTowerUIObj = Instantiate(_towerUIPrefab.gameObject, _towerUIParent);
            //Get các component TowerUI từ bản sao UI Tower được tạo ra
            TowerUI newTowerUI = newTowerUIObj.GetComponent<TowerUI>();
            newTowerUI.SetTowerPrefab(tower);
            //Set name của UI Tower theo name của đối tượng Tower
            newTowerUI.transform.name = tower.name;
        }
    }

    //Đăng ký các đối tượng Tower được tạo ra để LevelManager điều khiển quản lý
    public void RegisterSpawnedTower(Tower tower)
    {
        //Add vào danh sách _spawnedTowers
        _spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        Debug.Log("Run Spawn");
        SetTotalEnemy (--_enemyCounter);
        if (_enemyCounter < 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemiesQueue.FirstOrDefault(e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver(true);
            }
            return;
        }

        int randomIndex = Random.Range (0, _enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString ();
        GameObject newEnemyObj = _spawnedEnemiesQueue.FirstOrDefault(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate(_enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy> ();
        if (!_spawnedEnemiesQueue.Contains (newEnemy))
        {
            _spawnedEnemiesQueue.Enqueue(newEnemy);
        }

        newEnemy.transform.position = _enemyPaths[0].position;
        newEnemy.SetTargetPosition(_enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }

    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos()
    {
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

    public Bullet GetBulletFromPool(Bullet prefab)
    {
        GameObject newBulletObj = _spawnedBullets.Find(b => !b.gameObject.activeSelf && b.name.Contains(prefab.name))?.gameObject;
        if (newBulletObj == null)
        {
            newBulletObj = Instantiate(prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(newBullet))
        {
            _spawnedBullets.Add(newBullet);
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
                if (Vector2.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        SetCurrentLives(_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver(false);
        }
    }

    public void SetCurrentLives(int currentLives)
    {
        // Mathf.Max fungsi nya adalah mengambil angka terbesar
        // sehingga _currentLives di sini tidak akan lebih kecil dari 0
        _currentLives = Mathf.Max(currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    public void SetTotalEnemy(int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max(_enemyCounter, 0)}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;
        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive(true);
    }
}
