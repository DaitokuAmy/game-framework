/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXPhysicForce.cs
	
	Add Rigidbody force to explosions.
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXPhysicForce : MonoBehaviour {
	
	public float radius = 10f;
	public float force = 10f;
	public float delay = 0.2f;
	
	Collider[] colliders;
	
	
	void Start () 
	{
		colliders = Physics.OverlapSphere(this.transform.position, radius);
		
		StartCoroutine(Explode());
	}
	
	IEnumerator Explode()
	{
		yield return new WaitForSeconds(delay);
		
		for(int i = 0; i < colliders.Length; i ++)
		{
			var _rb = colliders[i].GetComponent<Rigidbody>();
			
			if(_rb != null)
			{
				colliders[i].GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius, new Vector3(Random.Range(-3, 3),Random.Range(-3, 3),Random.Range(-3, 3)),ForceMode.Impulse);
			}
		}
		
		yield return null;
	}

}
