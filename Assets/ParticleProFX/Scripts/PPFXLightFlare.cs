/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXLightFlare.cs
	
	Fades light intensity to 0f
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXLightFlare : MonoBehaviour {

	public Light li;
	public float fadeTime = 0f;
	
	
	float t = 0f;
	
	
	void Start(){
		StartCoroutine(Fade());
	}
	
	IEnumerator Fade(){
		t = 0.0f;
		
		while (t < fadeTime){
			t += Time.deltaTime;
			li.intensity = Mathf.Lerp(1f, 0f, t / fadeTime);
			yield return null;
		}
	}
}
