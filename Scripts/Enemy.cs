using UnityEngine;
using System.Collections;

namespace TowerDefenceTemplate
{

	public class Enemy : MonoBehaviour
	{
		public enum EnemyType
		{
			Jeep,
			Tank,
			Plane
		};

		public EnemyType enemyType;

		public EllipsoidParticleEmitter Explosion;

		public float 
			Health,
			Speed,
			current_health;

		public GameObject HealthBar;

        // 加速减速状态
        private float SpeedMulti = 1f;

		private Transform CurrentWaypoint;

		private Transform[] Waypoints;

		private GameManager gameManager;

		private int 
			Award,
			WaypointIndex;

		private bool Dead;


		void Start ()
		{
			gameManager = GameObject.FindObjectOfType<GameManager> ();
			if (gameManager == null) {
				Debug.LogError ("GameManager not found!");
				enabled = false;
				return;
			}

			Speed = gameManager.Waves [gameManager.CurrentWaveIndex].Speed;
			Health = gameManager.Waves [gameManager.CurrentWaveIndex].Health;
			Award = gameManager.Waves [gameManager.CurrentWaveIndex].Award;
			Waypoints = gameManager.Waves [gameManager.CurrentWaveIndex].Waypoints;

			current_health = Health;
			HealthBar.transform.localScale = Vector3.one;

			CurrentWaypoint = Waypoints [WaypointIndex];

			if (enemyType == EnemyType.Jeep) gameManager.JeepSound.Play ();
			if (enemyType == EnemyType.Tank) gameManager.TankSound.Play ();
			if (enemyType == EnemyType.Plane) gameManager.PlaneSound.Play ();
		}

		void GetDamage (float damage)
		{
			if (Dead) return;
			current_health -= damage;
			HealthBar.transform.localScale = new Vector3 (current_health / Health, 1, 1);
			if (current_health <= 0 && !Dead) Die ();
		}

		void Moving ()
		{
			if (!Dead) {
				Quaternion TargetRotation = Quaternion.LookRotation (CurrentWaypoint.position - transform.position, transform.up);
				transform.rotation = Quaternion.RotateTowards (transform.rotation, TargetRotation, Time.deltaTime * 100 * Speed * SpeedMulti);
			}
            
			transform.position += transform.forward * Time.deltaTime * Speed * SpeedMulti;

			if (Vector3.Distance (transform.position, CurrentWaypoint.position) < 2f) {
				if ((Waypoints.Length - 1) > WaypointIndex)
					NextWaypoint ();
				else {
					gameManager.DamageBase ();
					Deactivating ();
				}
			}
		}
			
		IEnumerator SinkInGround ()
		{
			for (float f = 0; f < 2; f += Time.deltaTime) {
				transform.position += new Vector3 (0, -f / 5, 0);
				Speed *= (1 - f / 2);
				yield return null;
			}
		}

		IEnumerator FallDown ()
		{
			for (float f = 0; f < 2; f += Time.deltaTime) {
				transform.position += new Vector3 (0, -f / 5, 0);
				Speed += f / 2;
				yield return null;
			}
		}

		void NextWaypoint ()
		{
			WaypointIndex += 1;
			CurrentWaypoint = Waypoints [WaypointIndex];
		}

		void Update ()
		{
			Moving ();
			HealthBar.transform.position = Camera.main.WorldToScreenPoint (transform.position) + new Vector3 (-30, -20, 0);
		}

		void Deactivating ()
		{
			Destroy (gameObject);
			Destroy (HealthBar);

			for (int i = 0; i < gameManager.SpawnedEnemies.Count; i++)
				if (gameManager.SpawnedEnemies [i].Equals (gameObject))
					gameManager.SpawnedEnemies.RemoveAt (i);
			
		}


		void Die ()
		{
			Explosion.Emit ();
			if (enemyType != EnemyType.Plane) StartCoroutine (SinkInGround ());
			else StartCoroutine (FallDown ());
			HealthBar.transform.localScale = Vector3.zero;
			tag = "Dead";
			Dead = true;
			gameManager.ExplosionSound.Play ();
			gameManager.AddMoney (Award);
			Invoke ("Deactivating", 2);
		}

        void SetSpeedMulti(float speed_multi)
        {
            SpeedMulti = speed_multi;
        }
    }

}