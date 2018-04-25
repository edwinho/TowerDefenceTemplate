using UnityEngine;
using System.Collections;

namespace TowerDefenceTemplate{

public class Bullet : MonoBehaviour {

	[HideInInspector] public float Damage;

	[HideInInspector] public GameObject target;

	public int BulletSpeed;

	void Start () {
		Invoke ("Deactivate", 1);
	}
		
	void Update(){
		if (target!=null)
			if (Vector3.Distance (transform.position, target.transform.position) < 3) MakeDamage ();
			Fly ();
	}

		void Fly(){
			transform.position += transform.forward * Time.deltaTime * BulletSpeed;
		}

	void MakeDamage(){
		target.transform.SendMessage ("GetDamage", Damage);
		Deactivate ();
	}

	void Deactivate(){
		gameObject.SetActive (false);
	}


}
}