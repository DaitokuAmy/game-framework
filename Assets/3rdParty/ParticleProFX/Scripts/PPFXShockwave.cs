/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXShockwave.cs
	
	Animate a simple quad mesh for shockwave.
	
	axis orientation based on cameraFacing script by
	Neil Cartner (NCartner)
	
	(c) 2014
=========================================================*/
 
using UnityEngine;
using System.Collections;
 
public class PPFXShockwave : MonoBehaviour
{
	Camera referenceCamera;
 
	public enum Axis {up, down, left, right, forward, back};
	public bool reverseFace = false; 
	public Axis axis = Axis.up; 
	
	private float duration;
	public float scale;
	public bool loop;
	public bool lookAt;
	
	
	public Vector3 GetAxis (Axis refAxis)
	{
		switch (refAxis)
		{
			case Axis.down:
				return Vector3.down; 
			case Axis.forward:
				return Vector3.forward; 
			case Axis.back:
				return Vector3.back; 
			case Axis.left:
				return Vector3.left; 
			case Axis.right:
				return Vector3.right; 
		}
 
		// default is Vector3.up
		return Vector3.up; 		
	}
 
	void  Awake ()
	{
		// if no camera referenced, grab the main camera
		if (!referenceCamera)
			referenceCamera = Camera.main;
			
		StartCoroutine(StartAnimation());
	}
 
	void  Update ()
	{
		// rotates the object relative to the camera
		if(lookAt){
			Vector3 targetPos = transform.position + referenceCamera.transform.rotation * (reverseFace ? Vector3.forward : Vector3.back) ;
			Vector3 targetOrientation = referenceCamera.transform.rotation * GetAxis(axis);
			transform.LookAt (targetPos, targetOrientation);
		}
	}
	
	IEnumerator StartAnimation(){
		
		transform.localScale = new Vector3(0,0,0);
		transform.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1,1,1,1));
		
		
		StartCoroutine(Animate());
		
		yield return null;
	}
	
	IEnumerator Animate(){
		//scale shockwave and fade alpha
		float t = 0;

		while(t < 1f) {
			transform.localScale = new Vector3(Mathf.Lerp(0, scale, t/0.5f),Mathf.Lerp(0, scale,  t/0.5f),Mathf.Lerp(0, scale,  t/0.5f));
			transform.GetComponent<Renderer>().material.SetColor("_TintColor", Color.Lerp ( new Color(1, 1, 1, 1) , new Color(1, 1, 1, 0),  t/0.5f));
		    t += Time.deltaTime;
		    yield return null;
        }
		
	}
}