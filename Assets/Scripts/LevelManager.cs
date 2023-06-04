using Assets.Scripts;
using Assets.Scripts.Enemys;
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
    
    private Queue<Enemy> _spawnedEnemiesQueue = new Queue<Enemy>();
    //Thời gian còn lại cho mỗi lần Enemy xuất hiện tiếp theo
    private float _runningSpawnDelay;

    //Danh sách chứa các đối tượng Bullet đã được tạo ra
    private List<Bullet> _spawnedBullets = new List<Bullet>();

    public bool IsOver { get; private set; }

    //[SerializeField] private float _spawnDelay = 5f;
    //[SerializeField] private int _maxEnemiesInScene = 5;
    //[SerializeField] private int _maxLives = 3;
    //[SerializeField] private int _totalEnemy = 15;

    //4 đối tượng UI để hiển thị trong game
    [SerializeField] private GameObject _panel;
    [SerializeField] private Text _statusInfo;
    [SerializeField] private Text _livesInfo;
    [SerializeField] private Text _totalEnemyInfo;
    [SerializeField] private Text _totalPoint;

    //Số lượng Enemy còn được phép đi qua map và số lượng Enemy còn lại để tiêu diệt
    private int _currentLives;
    private int _enemyCounter;
    private int _enemyPoint;

    //Set Data IO
    static ConfigurationData configurationData;
    #region Set Properties
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
    #endregion

    // Set giá trị cho _currentLives, _enemyCounter bằng _maxLives, _totalEnemy
    //Tạo các đối tượng TowerUI
    private void Start()
    {
        configurationData = new ConfigurationData();
        Debug.Log("1: " + _spawnDelay);
        Debug.Log("2: " + _maxEnemiesInScene);
        Debug.Log("3: " + _maxLives);
        Debug.Log("4: " + _totalEnemy);

        SetCurrentLives (_maxLives);
        SetTotalEnemy (_totalEnemy);
        SetTotalPoint (0);
        InstantiateAllTowerUI ();
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

        if (IsOver)
        {
            return;
        }

        if (_spawnedEnemiesQueue.Count < _maxEnemiesInScene)
        {
            _runningSpawnDelay -= Time.unscaledDeltaTime;
            if (_runningSpawnDelay <= 0f)
            {
                //Tạo ra một đối tượng Enemy mới và set giá trị cho _runningSpawnDelay = _spawnDelay = 5f
                SpawnEnemy();
                _runningSpawnDelay = _spawnDelay;
            }
        }


        //Duyệt các đối tượng Tower trong danh sách _spawnedTowers
        //Check và thực hiện các họat động của đối tượng Tower
        foreach (Tower tower in _spawnedTowers)
        {
            //Check Enemy gần nhất
            tower.CheckNearestEnemy(_spawnedEnemiesQueue);
            //Tìm kiếm Enemy
            tower.SeekTarget();
            //Bắn Enemy
            tower.ShootTarget();
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

            //Tính toán và so sánh khoảng cách giữa vị trí hiện tại và vị trí mục tiêu có < 0.1f?
            //Nếu đúng thì Enemy đã đạt đến vị trí mục tiêu
            if (Vector2.Distance (enemy.transform.position, enemy.TargetPosition) < 0.1f)
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

    //Tạo ra các đối tượng Enemy trong trò chơi
    private void SpawnEnemy ()
    {
        //Log
        Debug.Log("Run Spawn");
        //Set lại số lượng Enemy
        SetTotalEnemy (--_enemyCounter);
        //_enemyCounter < 0 thì tất cả Enemy được tạo ra
        if (_enemyCounter < 0)
        {
            //check xem tất cả Enemy bị đánh bại hết chưa?
            bool isAllEnemyDestroyed = _spawnedEnemiesQueue.FirstOrDefault(e => e.gameObject.activeSelf) == null;
            //isAllEnemyDestroyed = true thì game kết thúc và return
            if (isAllEnemyDestroyed)
            {
                SetGameOver (true);
            }
            return;
        }

        //Chọn 1 số ngẫu nhiên để chọn 1 Enemy ngẫu nhiên từ mảng Enemy _enemyPrefabs
        int randomIndex = Random.Range (0, _enemyPrefabs.Length);
        //check xem tên của của Enemy có chứa số randomIndex + 1 hay không?
        string enemyIndexString = (randomIndex + 1).ToString ();
        //Tìm Enemy không hoạt động và có tên chứa enemyIndexString trong _spawnedEnemiesQueue rồi gán cho newEnemyObj
        GameObject newEnemyObj = _spawnedEnemiesQueue.FirstOrDefault(e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString))?.gameObject;
        //check tìm thấy Enemy có tên chứa enemyIndexString hay không
        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate (_enemyPrefabs[randomIndex].gameObject);
        }
        //Tạo 1 Enemy mới từ newEnemyObj
        Enemy newEnemy = newEnemyObj.GetComponent<Enemy> ();
        //check xem newEnemyObj có trong _spawnedEnemiesQueue hay chưa
        if (!_spawnedEnemiesQueue.Contains (newEnemy))
        {
            _spawnedEnemiesQueue.Enqueue(newEnemy);
        }
        //set newEnemy thành vị trí đầu của mảng _enemyPaths
        newEnemy.transform.position = _enemyPaths[0].position;
        //set vị trí mục tiêu của đối tượng newEnemy thành vị trí thứ 2 trong mảng _enemyPaths
        newEnemy.SetTargetPosition (_enemyPaths[1].position);
        //set chỉ số đường đi hiện tại của đối tượng newEnemy thành 1.
        newEnemy.SetCurrentPathIndex (1);
        newEnemy.gameObject.SetActive (true);
    }

    //Vẽ đường đi
    private void OnDrawGizmos ()
    {
        //duyệt qua các điểm trong mảng _enemyPaths
        for (int i = 0; i < _enemyPaths.Length - 1; i++)
        {
            //set màu cho đường đi
            Gizmos.color = Color.cyan;
            //vẽ 1 đường đi từ vị trí i đến i + 1
            Gizmos.DrawLine (_enemyPaths[i].position, _enemyPaths[i + 1].position);
        }
    }

    //Tạo đạn và sử dụng lại
    public Bullet GetBulletFromPool (Bullet prefab)
    {
        //tìm 1 đối tượng đạn không hoạt động và có tên chứa prefab.name gán cho newBulletObj
        GameObject newBulletObj = _spawnedBullets.Find (b => !b.gameObject.activeSelf && b.name.Contains (prefab.name))?.gameObject;
        //check không tìm thấy đạn thì tạo mới 1 đạn khác trong prefabs và gán lại cho newBulletObj
        if (newBulletObj == null)
        {
            newBulletObj = Instantiate (prefab.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet> ();
        //check _spawnedBullets đã chứa đối tượng newBullet hay chưa
        if (!_spawnedBullets.Contains (newBullet))
        {
            //add newBullet vào _spawnedBullets
            _spawnedBullets.Add (newBullet);
        }

        return newBullet;
    }

    //Kích hoạt giảm máu quái
    public void ExplodeAt (Vector2 point, float radius, int damage)
    {
        //duyệt tất cả Enemy trong _spawnedEnemiesQueue
        foreach (Enemy enemy in _spawnedEnemiesQueue)
        {
            //check enemy có hoạt động hay không
            if (enemy.gameObject.activeSelf)
            {
                //check nếu khoảng cách giữa vị trí của enemy và điểm nổ nhỏ hơn hoặc bằng bán kính
                if (Vector2.Distance (enemy.transform.position, point) <= radius)
                {
                    //giảm máu bằng với damege được truyền vào
                    enemy.ReduceEnemyHealth (damage);
                }
            }
        }
    }

    //giảm số lượng enemy được phép vượt qua map
    public void ReduceLives (int value)
    {
        SetCurrentLives (_currentLives - value);
        if (_currentLives <= 0)
        {
            SetGameOver (false);
        }
    }

    //số lượng enemy còn lại được qua map
    public void SetCurrentLives (int currentLives)
    {
        _currentLives = Mathf.Max (currentLives, 0);
        _livesInfo.text = $"Lives: {_currentLives}";
    }

    //số lượng enemy còn lại xuất hiện
    public void SetTotalEnemy (int totalEnemy)
    {
        _enemyCounter = totalEnemy;
        _totalEnemyInfo.text = $"Total Enemy: {Mathf.Max (_enemyCounter, 0)}";
    }

    //Số lượng điểm
    public void SetTotalPoint(int totalPoint)
    {
        _enemyPoint = totalPoint;
        if (_totalPoint == null)
        {
            Debug.Log("_totalPoint is null");
        }
        _totalPoint.text = $"Total Point: {Mathf.Max(_enemyPoint, 0)}";
    }

    //set gameover
    public void SetGameOver (bool isWin)
    {
        Enemy point = new Enemy();
        int resultPoint = point.ResultPoint();
        Debug.Log("Result: " + resultPoint);
        configurationData.SaveToFile(resultPoint);

        IsOver = true;
        _statusInfo.text = isWin ? "You Win!" : "You Lose!";
        _panel.gameObject.SetActive (true);
    }
}
