using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 1;
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private SpriteRenderer _healthBar; //Sprite object
    [SerializeField] private SpriteRenderer _healthFill; //Sprite máu

    private int _currentHealth; //Máu hiện tại

    public Vector3 TargetPosition { get; private set; } //Vị trí cần đến
    public int CurrentPathIndex { get; private set; } //Chỉ số đường đi hiện tại

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Tạo mỗi lần tạo hoạt ảnh cho đối tượng
    // Fungsi ini terpanggil sekali setiap kali menghidupkan game object yang memiliki script ini
    private void OnEnable ()
    {
        _currentHealth = _maxHealth; //Set máu tối đa
        _healthFill.size = _healthBar.size; //Set máu = spite
    }

    //Di chuyển đối tượng Enemy đến vị trí đích (TargetPosition) với tốc độ di chuyển (_moveSpeed) đã xác định.
    public void MoveToTarget ()
    {
        transform.position = Vector3.MoveTowards (transform.position, TargetPosition, _moveSpeed * Time.deltaTime);
    }

    //Thiết lập vị trí đích (TargetPosition) cho đối tượng Enemy và xoay đối tượng Enemy
    //để hướng về vị trí đích. Trong phương thức này, _healthBar
    //được gỡ bỏ khỏi đối tượng Enemy để không bị xoay cùng với đối tượng Enemy.
    public void SetTargetPosition (Vector3 targetPosition)
    {
        TargetPosition = targetPosition;
        _healthBar.transform.parent = null;

        // Mengubah rotasi dari enemy
        Vector3 distance = TargetPosition - transform.position;
        if (Mathf.Abs (distance.y) > Mathf.Abs (distance.x))
        {
            // Menghadap atas
            if (distance.y > 0)
            {
                transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, 90f));
            }

            // Menghadap bawah
            else
            {
                transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, -90f));
            }
        }
        else
        {
            // Menghadap kanan (default)
            if (distance.x > 0)
            {
                transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, 0f));
            }

            // Menghadap kiri
            else
            {
                transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, 180f));
            }
        }
        _healthBar.transform.parent = transform;
    }

    // Menandai indeks terakhir pada path
    //Thiết lập chỉ số củađường đi hiện tại (CurrentPathIndex) mà đối tượng Enemy đang đi trên.
    public void SetCurrentPathIndex (int currentIndex)
    {
        CurrentPathIndex = currentIndex;
    }

    //Giảm số máu của đối tượng Enemy khi bị tấn công bằng một lượng sát thương (damage) đã xác định.
    //Nếu số máu của đối tượng Enemy giảm xuống dưới 0, đối tượng Enemy sẽ bị đánh bại
    //(gameObject.SetActive(false)) và thanh máu (_healthFill) sẽ được thiết lập để hiển thị đầy đủ.
    ///Nếu số máu của đối tượng Enemy vẫn còn, thanh máu sẽ được cập nhật lại để hiển thị phần máu còn lại.
    public void ReduceEnemyHealth (int damage)
    {
        _currentHealth -= damage;
        AudioPlayer.Instance.PlaySFX ("hit-enemy");

        if (_currentHealth <= 0)
        {
            _currentHealth = 0;
            gameObject.SetActive (false);
            AudioPlayer.Instance.PlaySFX ("enemy-die");
        }

        float healthPercentage = (float) _currentHealth / _maxHealth;
        _healthFill.size = new Vector2 (healthPercentage * _healthBar.size.x, _healthBar.size.y);
    }
}
