using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int _bulletPower; //Sức mạnh đạn
    private float _bulletSpeed; //Tốc độ đạn
    private float _bulletSplashRadius; //Xác định vùng ảnh hưởng đạn

    private Enemy _targetEnemy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // FixedUpdate adalah update yang lebih konsisten jeda pemanggilannya
    // cocok digunakan jika karakter memiliki Physic (Rigidbody, dll)
    //Gọi liên tục với tuần suất cố định
    private void FixedUpdate ()
    {
        if (LevelManager.Instance.IsOver)//Kiểm tra trò chơi kết thúc 
        {
            return;
        }

        if (_targetEnemy != null) //Đạn chạy theo mục tiêu
        {
            if (!_targetEnemy.gameObject.activeSelf)//Mục tiêu bị hủy, hủy đạn
            {
                gameObject.SetActive (false);
                _targetEnemy = null;
                return;
            }
            //Di chuyển đạn tới mục tiêu
            transform.position = Vector3.MoveTowards (transform.position, _targetEnemy.transform.position, _bulletSpeed * Time.fixedDeltaTime);
            //Tính toán hướng quay của đạn theo mục tiêu
            Vector3 direction = _targetEnemy.transform.position - transform.position;
            float targetAngle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, targetAngle - 90f));
        }
    }

    //Gọi khi va chạm quái và đạn
    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (_targetEnemy == null)
        {
            return;
        }
        
        if (collision.gameObject.Equals (_targetEnemy.gameObject))//Kiểm tra va chạm có phải mục tiêu không
        {
            gameObject.SetActive (false);//Hủy đạn

            if (_bulletSplashRadius > 0f)//Quáy chết nổ
            {
                LevelManager.Instance.ExplodeAt (transform.position, _bulletSplashRadius, _bulletPower);
            }

            // Giảm máu quái
            else
            {
                _targetEnemy.ReduceEnemyHealth (_bulletPower);
            }
            _targetEnemy = null;
        }
    }

    //Set giá trị các thuộc tính
    public void SetProperties (int bulletPower, float bulletSpeed, float bulletSplashRadius)
    {
        _bulletPower = bulletPower;
        _bulletSpeed = bulletSpeed;
        _bulletSplashRadius = bulletSplashRadius;
    }

    //Set mục tiêu
    public void SetTargetEnemy (Enemy enemy)
    {
        _targetEnemy = enemy;
    }
}
