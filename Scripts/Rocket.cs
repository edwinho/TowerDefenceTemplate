using UnityEngine;
using System.Collections;

namespace TowerDefenceTemplate
{

	public class Rocket : MonoBehaviour
	{

		[HideInInspector] public GameObject Target;

		[HideInInspector] public float Damage;

		private bool Exploded;

		private GameManager gameManager;

	
		public GameObject Body;

		public EllipsoidParticleEmitter Explosion;

		public ParticleSystem Smoke;

		public int RocketSpeed;


		void Start(){
			gameManager = GameObject.FindObjectOfType<GameManager> ();

			if (gameManager == null)
				enabled = false;
		}

		void Update ()
		{
			Fly ();
			if (Target != null) AimAtTarget ();
		}

		void Fly(){
			transform.position += transform.forward * Time.deltaTime * RocketSpeed;
		}

		void AimAtTarget ()
		{
			Quaternion TargetRotation = Quaternion.LookRotation (Target.transform.position - transform.position);
			transform.rotation = Quaternion.RotateTowards (transform.rotation, TargetRotation, Time.deltaTime * 1000);
			if (Vector3.Distance (transform.position, Target.transform.position) < 3 && !Exploded) Explode ();
		}

		void GetParentingBack ()
		{
			Explosion.transform.parent = transform;
			Explosion.transform.position = transform.position;
			Smoke.Play ();
			Body.SetActive (true);
			gameObject.SetActive (false);
			Exploded = false;
		}

		void Explode ()
		{
			Target.transform.SendMessage ("GetDamage", Damage);
			Exploded = true;
			gameManager.RocketExplosionSound.Play ();
			Explosion.Emit ();
			Explosion.transform.parent = null;
			Smoke.Stop ();
			Target = null;
			Invoke ("GetParentingBack", 0.9f);
			Body.SetActive (false);
		}

	}
}