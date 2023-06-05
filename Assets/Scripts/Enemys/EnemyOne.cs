using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace Assets.Scripts.Enemys
{
    using UnityEngine;
    public class EnemyOne : Enemy
    {
        public EnemyOne(int maxHealth, float moveSpeed) : base(maxHealth, moveSpeed)
        {
            MaxHealth= maxHealth;
            MoveSpeed= moveSpeed;
        }

        public override void ReduceEnemyHealth(int damage)
        {
            Debug.Log("EnemyOne is taking damage!");
            base.ReduceEnemyHealth(damage);
        }
    }
}
