using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //OnTriggerEnter2D sẽ được gọi để kiểm tra xem có thể đặt tháp lên vị trí đó hay không
    private void OnTriggerEnter2D(Collider2D collision)
    {
        LevelManager levelManager = FindObjectOfType<LevelManager>();

        var total = levelManager.getMoney();

        if (_placedTower != null) //Đã tồn tại
        {
            return;
        }
        Tower tower = collision.GetComponent<Tower>();
     
        if (total>=50 && tower != null) //Chưa tồn tại set mới
        {
                tower.SetPlacePosition(transform.position);
                _placedTower = tower;
                levelManager.SetMinusMoney(50);

        }
    }

    //OnTriggerExit2D sẽ được gọi để xóa tháp khỏi vị trí đó và đặt biến _placedTower về null. 
    private void OnTriggerExit2D (Collider2D collision)
    {
        if (_placedTower == null)
        {
            return;
        }
        _placedTower.SetPlacePosition (null);
        _placedTower = null;
    }
}
