/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXCountdownDestruct.cs
	
	Destroys gameobject after certain amount of time
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXCountdownDestruct : MonoBehaviour {
	
	public float time;
	
	void Start()
	{
		StartCoroutine(StartCountdown());
	}
	
	IEnumerator StartCountdown()
	{
		yield return new WaitForSeconds (time);
		
		Destroy(this.gameObject);
	}
}
