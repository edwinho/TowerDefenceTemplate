using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace TowerDefenceTemplate
{

	public class UI_Controller : MonoBehaviour
	{
		private GameManager gameManager;

		private GameObject SelectedTower;


		public UnityEngine.EventSystems.EventSystem _eventSystem;

		public Image EnemiesIcon;

		public RectTransform 
			BaseHealthBar,
			TowerControl;
	
		public Button 
			ToMenuButton,
			RestartButton;

		public GameObject 
			RightPanel,
			PauseMenu,
			ContinueButton,
			InGameButtons,
			UpgradeButton;

		public Text 
			MoneyMessage,
			MoneyText,
			WaveNumber,
			WaveTimer,
			EnemiesLeftText, 
			UpgradeButtonText,
			SellButtonText,
			LevelText,
			RangeText,
			DamageText,
			NameText,
            HealthValue,
			WaveUpperNumber;


		void Awake ()
		{
			gameManager = GameObject.FindObjectOfType<GameManager> ();

			if (gameManager == null) {
				Debug.LogError ("GameManager not found!");
				enabled = false;
				return;
			}

			SendInstances ();
		}

		void SendInstances ()
		{
			gameManager._UI_Controller = this;
			gameManager.MoneyMessage = MoneyMessage;
			gameManager.MoneyText = MoneyText;
			gameManager.BaseHealthBar = BaseHealthBar;
			gameManager.WaveNumber = WaveNumber;
			gameManager.EnemiesLeftText = EnemiesLeftText;
			gameManager.WaveTimer = WaveTimer;
			gameManager.ToMenuButton = ToMenuButton;
			gameManager.RestartButton = RestartButton;
			gameManager.EnemiesIcon = EnemiesIcon;
			gameManager.NameText = NameText;
			gameManager.DamageText = DamageText;
			gameManager.RangeText = RangeText;
			gameManager.LevelText = LevelText;
			gameManager.UpgradeButtonText = UpgradeButtonText;
			gameManager.SellButtonText = SellButtonText;
			gameManager.RightPanel = RightPanel;
			gameManager.WaveUpperNumber = WaveUpperNumber;
			gameManager.PauseMenu = PauseMenu;
			gameManager.UpgradeButton = UpgradeButton;
            gameManager.HealthValue = HealthValue;

        }


		public void GameEnd ()
		{
			InGameButtons.SetActive (false);
			WaveNumber.gameObject.SetActive (true);
			ContinueButton.SetActive (false);
			PauseMenuShow ();
		}


		public void CreateTower (string type)
		{
			if (gameManager == null)
				return;
			gameManager.BuildingModeOn (type);
		}

		public void PauseMenuShow ()
		{
			Pause ();
			PauseMenu.SetActive (true);
		}


		public void Pause ()
		{
			if (gameManager == null)
				return;
			gameManager.Pause ();
		}

		public void Play ()
		{
			if (gameManager == null)
				return;
			gameManager.UnPause ();
			PauseMenu.SetActive (false);
		}

		public void Fast ()
		{
			if (gameManager == null)
				return;
			PauseMenu.SetActive (false);
			gameManager.Fast ();
		}

		public void UpgradeTower ()
		{
			gameManager.UpgradeTower ();
		}

		public void SellTower ()
		{
			gameManager.SellTower ();
		}

		public void BackToMenu ()
		{
			if (gameManager == null)
				return;
			gameManager.BackToMenu ();
		}

		public void RestartLevel ()
		{
			if (gameManager == null)
				return;
			Play ();
			gameManager.RestartLevel ();
		}


	}
}