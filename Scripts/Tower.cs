using UnityEngine;
using System.Collections;

namespace TowerDefenceTemplate
{
	
	public class Tower : MonoBehaviour
	{
		public enum TowerType
		{
			LightGun,
			RocketLauncher,
			FlameThrower
		};

		public TowerType towerType;

        public int
            Cost;

		[HideInInspector] public int Level;

		public int[]
			Range,
            UpgradeCost,
            Damage;

		public string TowerName;

		public GameObject 
			Body,
			Head;

		public EllipsoidParticleEmitter 
			Fire1,
			Fire2,
			Flame;

		private float Timer;

		private GameManager gameManager;

		private GameObject CurrentTarget;

		public Transform 
			GunPoint1, 
			GunPoint2,
			DetectCircle;


		void Start ()
		{
			gameManager = GameObject.FindObjectOfType<GameManager> ();
			DetectCircle.localScale = new Vector3 (Range [Level], DetectCircle.localScale.y, Range [Level]);

			if (gameManager == null) {
				Debug.LogError ("GameManager not found!");
				enabled = false;
			}
		}

		void OnMouseUp ()
		{
			DetectCircle.gameObject.SetActive (true);
			gameManager.ActivateTowerControl (this);
		}


		void Update ()
		{
			if (transform.parent == null)
				return;

			float _detectRange = DetectCircle.localScale.x / 2;

			Timer = Mathf.MoveTowards (Timer, 0, Time.deltaTime);

			if (CurrentTarget == null)
				SearchTarget ();
			
			if (CurrentTarget != null) {
				Vector3 ProjectionOnGround = new Vector3 (CurrentTarget.transform.position.x, transform.position.y, CurrentTarget.transform.position.z);
				if (Vector3.Distance (transform.position, ProjectionOnGround) > _detectRange || CurrentTarget.tag == "Dead") {
					CurrentTarget = null;
					return;
				}
				Vector3 ThisPosition = transform.position;
				Quaternion TargetBodyRotation = Quaternion.LookRotation (new Vector3 (CurrentTarget.transform.position.x, ThisPosition.y, CurrentTarget.transform.position.z) - transform.position, transform.up);
				Quaternion TargetHeadRotation = Quaternion.LookRotation (CurrentTarget.transform.position - Head.transform.position, transform.up);
				Body.transform.rotation = Quaternion.RotateTowards (Body.transform.rotation, TargetBodyRotation, Time.deltaTime * 100);
				Head.transform.rotation = Quaternion.RotateTowards (Head.transform.rotation, TargetHeadRotation, Time.deltaTime * 50);

				if (Quaternion.Angle (Head.transform.rotation, TargetHeadRotation) < 2) {
					if (towerType == TowerType.LightGun)
						AttackingLightGun ();
					if (towerType == TowerType.RocketLauncher)
						AttackingRocketLauncher ();
					if (towerType == TowerType.FlameThrower)
						AttackingFlameThrower ();
				}



			} else {
				if (towerType == TowerType.FlameThrower) StopFlaming ();
			}


		}

		public void Upgrade ()
		{
			Level++;
			DetectCircle.localScale = new Vector3 (Range [Level], DetectCircle.localScale.y, Range [Level]);
		}

		void SearchTarget ()
		{
			foreach (var item in gameManager.SpawnedEnemies) {
				Vector3 ProjectionOnGround = new Vector3 (item.transform.position.x, transform.position.y, item.transform.position.z);

				if ((item.tag == "Plane" && towerType != TowerType.RocketLauncher) || item.tag == "Dead") continue;

                if (Vector3.Distance(transform.position, ProjectionOnGround) < DetectCircle.localScale.x / 2)
                {
                    CurrentTarget = item;
                    break;
                }
			}

		}

		void StopFlaming ()
		{
			Flame.emit = false;
			gameManager.FlameThrowerSound.Stop ();
		}

		void AttackingLightGun ()
		{
			if (Timer == 0) {
				Timer = 0.3f;
				ShootLightGun (GunPoint1);
				ShootLightGun (GunPoint2);
				Fire1.Emit ();
				Fire2.Emit ();
				gameManager.LightGunShootSound.Play ();
			}
		}


		void ShootLightGun (Transform gunpoint)
		{
			Bullet _tempbul;
			for (int i = 0; i < gameManager.BulletsPool.Count; i++)
				if (!gameManager.BulletsPool [i].gameObject.activeSelf) {
					_tempbul = gameManager.BulletsPool [i];
					_tempbul.target = CurrentTarget;
					_tempbul.Damage = Damage [Level];
					_tempbul.gameObject.SetActive (true);
					_tempbul.transform.position = gunpoint.position;
					_tempbul.transform.rotation = gunpoint.rotation;
					break;
				}
			
			int random = Random.Range (0, 20);

			if (random == 1) gameManager.RicochetSound1.Play ();
			if (random == 10) gameManager.RicochetSound2.Play ();
			if (random == 20) gameManager.RicochetSound3.Play ();
		}

		void AttackingRocketLauncher ()
		{
			if (Timer == 0) {
				Timer = 2;
				StartCoroutine (ShootRocketLauncher ());
			}

		}

		IEnumerator ShootRocketLauncher ()
		{
			Rocket _temproc;

			for (int i = 0; i < gameManager.RocketsPool.Count; i++)
				if (!gameManager.RocketsPool [i].gameObject.activeSelf) {
					_temproc = gameManager.RocketsPool [i];
					_temproc.Damage = Damage [Level];
					_temproc.Target = CurrentTarget;
					_temproc.gameObject.SetActive (true);
					_temproc.transform.position = GunPoint1.position;
					_temproc.transform.rotation = GunPoint1.rotation;
					gameManager.RocketLaunchSound.Play ();
					Fire1.Emit ();
					break;
				}

			yield return new WaitForSeconds (0.3f);

			for (int i = 0; i < gameManager.RocketsPool.Count; i++)
				if (!gameManager.RocketsPool [i].gameObject.activeSelf) {
					_temproc = gameManager.RocketsPool [i];
					_temproc.Damage = Damage [Level];
					_temproc.Target = CurrentTarget;
					_temproc.gameObject.SetActive (true);
					_temproc.transform.position = GunPoint2.position;
					_temproc.transform.rotation = GunPoint2.rotation;
					gameManager.RocketLaunchSound.Play ();
					Fire2.Emit ();
					break;
				}

		}


		void AttackingFlameThrower ()
		{
			Flame.emit = true;
			CurrentTarget.SendMessage ("GetDamage", Damage [Level] * Time.deltaTime);
            CurrentTarget.SendMessage("SetSpeedMulti", 0.1f);
            if (!gameManager.FlameThrowerSound.isPlaying) gameManager.FlameThrowerSound.Play ();
		}
	}



}