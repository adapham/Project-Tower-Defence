﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    // Tower Component
    [SerializeField] private SpriteRenderer _towerPlace;
    [SerializeField] private SpriteRenderer _towerHead;
    [SerializeField] GameObject levelUp;

    // Tower Properties
    [SerializeField] static int _shootPower = 2;
    [SerializeField] private float _shootDistance = 1f;
    [SerializeField] private float _shootDelay = 5f;
    [SerializeField] private float _bulletSpeed = 1f;
    [SerializeField] private float _bulletSplashRadius = 0f;

    [SerializeField] private Bullet _bulletPrefab;
    
    private float _runningShootDelay;
    private Enemy _targetEnemy;
    private Quaternion _targetRotation;
    public Button myButton;


    //Được sử dụng để lưu vị trí sẽ bị chiếm giữ khi tháp đang được kéo
    public Vector2? PlacePosition { get; private set; }
    protected void BulletSpeed(float bulletSpeed)
    {
        _bulletSpeed=bulletSpeed;
    }
    // Start is called before the first frame update
    public virtual void Start()
    {
        myButton.onClick.AddListener(OnButtonClick);        
    }
    public void OnButtonClick()
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();
        levelManager.SetMinusMoney(20);

        _shootPower += 1;
        Debug.Log(_shootPower);
        
    }
    // Update is called once per frame
    public virtual void Update()
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();

        var total = levelManager.getMoney();
        Debug.Log(total);
        if (total >= 20)
        {
            levelUp.SetActive(true);
        }
        else
        {
            levelUp.SetActive(false);
        }
    }

    //Chức năng lấy sprite trên Tower Head
    public Sprite GetTowerHeadIcon ()
    {
        return _towerHead.sprite;
    }

    public void SetPlacePosition(Vector2? newPosition)
    {
        PlacePosition = newPosition;
    }

    public void LockPlacement ()
    {
        transform.position = (Vector2) PlacePosition;
    }

    // Thay đổi thứ tự trong lớp trên tháp đang được kéo
    public void ToggleOrderInLayer (bool toFront)
    {
        int orderInLayer = toFront ? 2 : 0;
        _towerPlace.sortingOrder = orderInLayer;
        _towerHead.sortingOrder = orderInLayer;
    }

    // Kiểm tra kẻ thù gần đó
    public void CheckNearestEnemy (Queue<Enemy> enemies)
    {
        if (_targetEnemy != null)
        {
            if (!_targetEnemy.gameObject.activeSelf || Vector3.Distance (transform.position, _targetEnemy.transform.position) > _shootDistance)
            {
                _targetEnemy = null;
            }
            else
            {
                return;
            }
        }

        float nearestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;
        
        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance (transform.position, enemy.transform.position);
            if (distance > _shootDistance)
            {
                continue;
            }
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        _targetEnemy = nearestEnemy;
    }

    // Bắn kẻ thù đã được lưu làm mục tiêu
    public virtual void ShootTarget ()
    {
        if (_targetEnemy == null)
        {
            return;
        }

        _runningShootDelay -= Time.unscaledDeltaTime;
        if (_runningShootDelay <= 0f)
        {
            bool headHasAimed = Mathf.Abs (_towerHead.transform.rotation.eulerAngles.z - _targetRotation.eulerAngles.z) < 10f;
            if (!headHasAimed)
            {
                return;
            }
            LevelManager levelManager = FindObjectOfType<LevelManager>();
            Bullet bullet = levelManager.GetBulletFromPool(_bulletPrefab);
            bullet.transform.position = transform.position;
            
            bullet.SetProperties (_shootPower, _bulletSpeed, _bulletSplashRadius);
            Debug.Log(_shootPower + "dsadas");
            bullet.SetTargetEnemy (_targetEnemy);
            bullet.gameObject.SetActive (true);
            _runningShootDelay = _shootDelay;
        }
    }

    // Làm cho tháp luôn nhìn vào kẻ thù
    public void SeekTarget ()
    {
        if (_targetEnemy == null)
        {
            return;
        }
        Vector3 direction = _targetEnemy.transform.position - transform.position;
        float targetAngle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
        _targetRotation = Quaternion.Euler (new Vector3 (0f, 0f, targetAngle - 90f));
        _towerHead.transform.rotation = Quaternion.RotateTowards (_towerHead.transform.rotation, _targetRotation, Time.deltaTime * 180f);
    }
}
