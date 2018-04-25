using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace TowerDefenceTemplate{

public class MainMenu : MonoBehaviour {

	public GameObject 
		Menu,
		LevelSelecter;

	public void SelectLevel(){
		LevelSelecter.SetActive (true);
		Menu.SetActive (false);
	}

		public void LoadLevel1(){
			SceneManager.LoadScene ("Level1");
		}

		public void LoadLevel2(){
			SceneManager.LoadScene ("Level2");
		}

		public void LoadLevel3(){
			SceneManager.LoadScene ("Level3");
		}
}

}