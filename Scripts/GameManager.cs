using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace TowerDefenceTemplate
{

	[System.Serializable]
	public class Wave
	{
		public Transform[] Waypoints;
		public Transform SpawnPoint;
		public GameObject 
			EnemyPrefab,
			ArrowPoint;
		public Sprite EnemiesIcon;
		public int 
			Speed,
			Amount,
			Interval,
			Health,
			Damage,
			Award;
		[HideInInspector] 
		public int Spawned;
	}

	public class GameManager : MonoBehaviour
	{
		private UnityEngine.EventSystems.EventSystem _eventSystem;

		[HideInInspector] public UI_Controller _UI_Controller;

		[HideInInspector] public int CurrentWaveIndex;

		[HideInInspector] public List<GameObject> 
			SpawnedEnemies,
			BuiltTowers;

		private GameObject[] TowerPoints;

		private float Timer;

		private int BaseHealth = 100;

		private bool GameEnded;

		private Tower TowerToEdit;

		[HideInInspector] public Dictionary<int,Bullet> BulletsPool;
		[HideInInspector] public  Dictionary<int,Rocket> RocketsPool;

		private GameObject 
			RocketsPoolParent,
			BulletsPoolParent,
			TempTower,
			TowerToBuild;

		//UI
		[HideInInspector] public RectTransform BaseHealthBar;
		[HideInInspector] public Text 
			MoneyMessage, 
			WaveNumber, 
			WaveUpperNumber,
			MoneyText,
			EnemiesLeftText,
			WaveTimer,
			NameText,
			DamageText,
			RangeText,
			LevelText,
			UpgradeButtonText,
            HealthValue,
			SellButtonText;

		[HideInInspector] public Image 
			EnemiesIcon;

		[HideInInspector] public Button 
			ToMenuButton, 
			RestartButton;

		[HideInInspector] 
		public GameObject
			RightPanel,
			PauseMenu,
			UpgradeButton;

		[Header ("Camera boundaries")]
		public float X_Min;
		public float X_Max;
		public float Z_Min;
		public float Z_Max;

		public Wave[] Waves;
		public int Money;

		[Header ("Sounds")]
		public AudioSource BuildSound;
		public AudioSource BaseDamageSound;
		public AudioSource ErrorSound;
		public AudioSource ExplosionSound;
		public AudioSource LevelUpSound;
		public AudioSource LightGunShootSound;
		public AudioSource RicochetSound1;
		public AudioSource RicochetSound2;
		public AudioSource RicochetSound3;
		public AudioSource RocketLaunchSound;
		public AudioSource RocketExplosionSound;
		public AudioSource FlameThrowerSound;
		public AudioSource JeepSound;
		public AudioSource TankSound;
		public AudioSource PlaneSound;
		public AudioSource SellSound;

		[Header ("Prefabs")]
		public GameObject RocketLauncherPrefab;
		public GameObject LightGunPrefab;
		public GameObject FlameThrowerPrefab;
		public GameObject HealthBarPrefab;

		public GameObject 
			BulletPrefab,
			RocketPrefab;

        private float MapMoveDistance = 1.0f;

		void Start ()
		{
			if (Waves.Length > 0) {
				if (Waves [CurrentWaveIndex].ArrowPoint != null)
					Waves [CurrentWaveIndex].ArrowPoint.SetActive (true);
			} else {
				Debug.LogError ("Set up the GameManager waves!");
				enabled = false;
			}

			if (_UI_Controller == null) {
				Debug.LogError ("UI Canvas not found! GameManager is disabled.");
				enabled = false;
				return;
			}

			TowerPoints = GameObject.FindGameObjectsWithTag ("TowerPoint");
			_eventSystem = _UI_Controller._eventSystem;

			CreateBulletsPool ();
			CreateRocketsPool ();

			Timer = 5;

			PauseMenu.SetActive (false);

			MoneyText.text = Money.ToString () + "$";
            HealthValue.text = "HP: " + BaseHealth.ToString();

			WaveUpperNumber.text = "Wave " + (CurrentWaveIndex + 1).ToString () + "/" + Waves.Length.ToString ();
		}


		void OnDrawGizmos ()
		{
			foreach (var wave in Waves) {
				for (int i = 1; i < wave.Waypoints.Length; i++)
					Gizmos.DrawLine (wave.Waypoints [i].position, wave.Waypoints [i - 1].position);
			}
		}

		public void CreateBulletsPool ()
		{
			BulletsPool = new Dictionary<int,Bullet> ();
			BulletsPoolParent = new GameObject ("BulletsPool");

			for (int i = 0; i < 30 * TowerPoints.Length; i++) {
				GameObject _newbullet = Instantiate (BulletPrefab);
				BulletsPool [i] = _newbullet.GetComponent<Bullet> ();
				BulletsPool [i].gameObject.SetActive (false);	
				BulletsPool [i].transform.parent = BulletsPoolParent.transform;
			}
		}

		public void CreateRocketsPool ()
		{
			RocketsPool = new Dictionary<int, Rocket> ();
			RocketsPoolParent = new GameObject ("RocketsPool");

			for (int i = 0; i < 5 * TowerPoints.Length; i++) {
				GameObject _newRocket = Instantiate (RocketPrefab);
				RocketsPool [i] = _newRocket.GetComponent<Rocket> ();
				RocketsPool [i].gameObject.SetActive (false);
				RocketsPool [i].transform.parent = RocketsPoolParent.transform;
			}
		}

		public void SpendMoney (int moneyAmount)
		{
			Money -= moneyAmount;
			MoneyText.text = Money.ToString () + "$";
		}

		public void AddMoney (int moneyAmount)
		{
			Money += moneyAmount;
			MoneyText.text = Money.ToString () + "$";
		}

		void Update ()
		{
			if (GameEnded)
				return;

			Timer = Mathf.MoveTowards (Timer, 0, Time.deltaTime);

			DoNavigation ();
			ControlMoneyMessageColor (false);
			CountEnemiesLeft ();
			CountTimeTillWaveStart ();
			CountEnemiesSpawnerTimer ();
			PositioningTower ();
			CreatingTower ();

			if (Input.GetKeyDown ("mouse 0") && !_eventSystem.IsPointerOverGameObject () && TowerToEdit != null)
				DeactivateTowerControl ();
		}

		void DoNavigation ()
		{
			if (_eventSystem.IsPointerOverGameObject ())
				return;

			if (Input.GetKey ("mouse 0") && TempTower == null)
				Camera.main.transform.position += new Vector3 (-Input.GetAxis ("Mouse X"), 0, -Input.GetAxis ("Mouse Y"));

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                Camera.main.transform.position += new Vector3(-MapMoveDistance, 0, 0);

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                Camera.main.transform.position += new Vector3(MapMoveDistance, 0, 0);

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                Camera.main.transform.position += new Vector3(0, 0, MapMoveDistance);

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                Camera.main.transform.position += new Vector3(0, 0, -MapMoveDistance);

            Camera.main.transform.position = new Vector3 (
				Mathf.Clamp (Camera.main.transform.position.x, X_Min, X_Max),
				Camera.main.transform.position.y,
				Mathf.Clamp (Camera.main.transform.position.z, Z_Min, Z_Max));
		}

		void CountEnemiesSpawnerTimer ()
		{
			if (Timer == 0) {
				WaveNumber.gameObject.SetActive (false);
				if (Waves [CurrentWaveIndex].ArrowPoint != null)
					Waves [CurrentWaveIndex].ArrowPoint.SetActive (false);
				Timer = Waves [CurrentWaveIndex].Interval;
				EnemiesIcon.overrideSprite = Waves [CurrentWaveIndex].EnemiesIcon;
				int _spawned = Waves [CurrentWaveIndex].Spawned;
				int _amount = Waves [CurrentWaveIndex].Amount;

				if (_spawned < _amount)
					SpawnEnemy ();
				else if (SpawnedEnemies.Count == 0) {
					JeepSound.Stop ();
					TankSound.Stop ();
					PlaneSound.Stop ();
					Timer = 5;
					WaveNumber.gameObject.SetActive (true);
					if (CurrentWaveIndex < (Waves.Length - 1)) {
						CurrentWaveIndex += 1;
						WaveUpperNumber.text = "Wave " + (CurrentWaveIndex + 1).ToString () + "/" + Waves.Length.ToString ();
						Waves [CurrentWaveIndex].ArrowPoint.SetActive (true);
						WaveNumber.text = "Wave " + (CurrentWaveIndex + 1).ToString ();

					} else
						YouWon ();
				}
			}
		}


		void CountTimeTillWaveStart ()
		{
			if (SpawnedEnemies.Count == 0)
				WaveTimer.text = "0:0" + ((int)Timer).ToString ();
		}

		void CountEnemiesLeft ()
		{
			int Killed, EnemiesLeft;

			Killed = Waves [CurrentWaveIndex].Spawned - SpawnedEnemies.Count;
			EnemiesLeft = Waves [CurrentWaveIndex].Amount - Killed;
			EnemiesLeftText.text = EnemiesLeft.ToString ();
		}

		void ControlMoneyMessageColor (bool ShowMessage)
		{
			Color _color = MoneyMessage.color;
			_color.a = Mathf.MoveTowards (_color.a, 0, Time.deltaTime);
			if (ShowMessage)
				_color.a = 1;
			MoneyMessage.color = _color;
		}

		public void ThrowMoneyMessage ()
		{
			ControlMoneyMessageColor (true);
			ErrorSound.Play ();
		}

		public void UpgradeTower ()
		{
			if (TowerToEdit.UpgradeCost [TowerToEdit.Level] > Money) {
				ThrowMoneyMessage ();
				return;
			}

			SpendMoney (TowerToEdit.UpgradeCost[TowerToEdit.Level]);
            TowerToEdit.Upgrade();
            if (TowerToEdit.Level == TowerToEdit.Damage.Length - 1)
				UpgradeButton.SetActive (false);
			else
				UpgradeButton.SetActive (true);
			LevelUpSound.Play ();
			UpdateTowerInfo ();
		}

		public void SellTower ()
		{
            int extra_cost = 0;
            if (TowerToEdit.Level > 0)
            {
                extra_cost = TowerToEdit.UpgradeCost[TowerToEdit.Level - 1] / 2;
            }
            AddMoney (TowerToEdit.Cost / 2 + extra_cost);
			Destroy (TowerToEdit.gameObject);
			SellSound.Play ();
			DeactivateTowerControl ();
		}

		void UpdateTowerInfo ()
		{
			UpgradeButtonText.text = "Upgrade " + TowerToEdit.UpgradeCost[TowerToEdit.Level].ToString () + "$";
            int extra_cost = 0;
            if (TowerToEdit.Level > 0)
            {
                extra_cost = TowerToEdit.UpgradeCost[TowerToEdit.Level - 1] / 2;
            }
            SellButtonText.text = "Sell " + (TowerToEdit.Cost / 2 + extra_cost).ToString () + "$";
			NameText.text = TowerToEdit.TowerName;
			LevelText.text = "Level " + (TowerToEdit.Level + 1).ToString ();
			DamageText.text = "Damage " + TowerToEdit.Damage [TowerToEdit.Level].ToString ();
			RangeText.text = "Range " + TowerToEdit.Range [TowerToEdit.Level].ToString ();
		}

		void YouLose ()
		{
			ToMenuButton.gameObject.SetActive (true);
			RestartButton.gameObject.SetActive (true);
			GameEnded = true;
			if (_UI_Controller != null)
				_UI_Controller.GameEnd ();
			WaveNumber.gameObject.SetActive (true);
			WaveNumber.text = "You lose!";

		}

		void YouWon ()
		{
			ToMenuButton.gameObject.SetActive (true);
			RestartButton.gameObject.SetActive (true);
			GameEnded = true;
			if (_UI_Controller != null)
				_UI_Controller.GameEnd ();
			WaveNumber.gameObject.SetActive (true);
			WaveTimer.gameObject.SetActive (false);
			WaveNumber.text = "You won!";
		}

		public void ActivateTowerControl (Tower tower)
		{
			RightPanel.SetActive (true);
			TowerToEdit = tower;
			if (TowerToEdit.Level == TowerToEdit.Damage.Length - 1)
				UpgradeButton.SetActive (false);
			else
				UpgradeButton.SetActive (true);
			UpdateTowerInfo ();
		}

		public void DeactivateTowerControl ()
		{
			RightPanel.SetActive (false);
			if (TowerToEdit != null)
				TowerToEdit.DetectCircle.gameObject.SetActive (false);
			TowerToEdit = null;

		}

		public void DamageBase ()
		{
			BaseHealth -= Waves [CurrentWaveIndex].Damage;
            if (BaseHealth < 0) BaseHealth = 0;
            HealthValue.text = "HP: " + BaseHealth.ToString();
            BaseDamageSound.Play ();
			BaseHealthBar.localScale = new Vector3 (BaseHealth / 100f, 1, 1);
			if (BaseHealth <= 0) {
				YouLose ();
				BaseHealth = 100;
			}
		}

		public void BackToMenu ()
		{
			SceneManager.LoadScene ("Menu");
		}


		public void RestartLevel ()
		{
			SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
		}

		public void BuildingModeOn (string type)
		{
			if (type == "lightgun")
				TowerToBuild = LightGunPrefab;
			if (type == "rocketlauncher")
				TowerToBuild = RocketLauncherPrefab;
			if (type == "flamethrower")
				TowerToBuild = FlameThrowerPrefab;
			
			TempTower = Instantiate (TowerToBuild);

		}

		void PositioningTower ()
		{
			if (TempTower == null)
				return;
			
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if (Physics.Raycast (ray, out hit)) {
				TempTower.transform.position = hit.point;
			}
		}

		void CreatingTower ()
		{
			if (TempTower == null || TowerPoints.Length == 0)
				return;

			GameObject ClosestTowerPoint = TowerPoints [0];
			Vector3 ClosestTowerPointPosition = TowerPoints [0].transform.position;

			foreach (var towerpoint in TowerPoints)
				if (Vector3.Distance (TempTower.transform.position, towerpoint.transform.position) < Vector3.Distance (ClosestTowerPointPosition, TempTower.transform.position)) {
					ClosestTowerPoint = towerpoint;
					ClosestTowerPointPosition = towerpoint.transform.position;
				}
					
			if (Input.GetKeyUp ("mouse 0")) {
				if (Money < TempTower.GetComponent<Tower> ().Cost) {
					ThrowMoneyMessage ();
					Destroy (TempTower);
					return;
				}

				if (Vector3.Distance (TempTower.transform.position, ClosestTowerPointPosition) > 5
				    || ClosestTowerPoint.transform.childCount > 0) {
					Destroy (TempTower);
					ErrorSound.Play ();
					return;
				}
				SpendMoney (TempTower.GetComponent<Tower> ().Cost);
				BuiltTowers.Add (TempTower);
				float _verticalOffset = ClosestTowerPoint.AddComponent<BoxCollider> ().bounds.size.y;
				Destroy (ClosestTowerPoint.GetComponent<BoxCollider> ());
				SphereCollider _tempCollider = TempTower.AddComponent<SphereCollider> ();
				_tempCollider.radius = 5;
				TempTower.transform.position = ClosestTowerPointPosition + new Vector3 (0, _verticalOffset, 0);
				TempTower.transform.parent = ClosestTowerPoint.transform;
				TempTower = null;
				BuildSound.Play ();


			}
		}

		public void Pause ()
		{
			Time.timeScale = 0;
		}

		public void UnPause ()
		{
			Time.timeScale = 1;
		}

		public void Fast ()
		{
			Time.timeScale = 3;
		}



		void SpawnEnemy ()
		{
			Waves [CurrentWaveIndex].Spawned += 1;
			GameObject _newEnemy = Instantiate (Waves [CurrentWaveIndex].EnemyPrefab, Waves [CurrentWaveIndex].SpawnPoint.position, Waves [CurrentWaveIndex].SpawnPoint.rotation) as GameObject;
			GameObject _healthBar = _newEnemy.GetComponent<Enemy> ().HealthBar = Instantiate (HealthBarPrefab) as GameObject;
			_healthBar.transform.SetParent (_UI_Controller.transform);
			SpawnedEnemies.Add (_newEnemy);
		}
	}
}