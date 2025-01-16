/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXRandomPosition.cs
	
	very simple random position script for 
	using it with the meteor script.
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXRandomPosition : MonoBehaviour {

	public Vector2 amplitude;
	public Vector2 turbulence;
	public bool xAxis;
	public bool yAxis;
	
	Transform target;
	float amp;
	float turb;
	
	void Start()
	{
		target = this.transform;
		
		amp = Random.Range(amplitude.x, amplitude.y);
		turb = Random.Range(turbulence.x, turbulence.y);
	}
	
	void Update()
	{
		
		//transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.time * speed);
		
		if (xAxis)
		{
			transform.Translate(Vector3.right * Mathf.Sin(turb * Time.time) * amp);
		}
		if (yAxis)
		{
			transform.Translate(Vector3.up * Mathf.Sin(turb * Time.time) * amp);
		}
		
		transform.LookAt(target.transform.position);
		
	}
	
}