using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackTower : Tower
{
    // Start is called before the first frame update
    public override void Start()
    {
        BulletSpeed(3f);
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
    public override void ShootTarget()
    {
        base.ShootTarget();
    }
}
