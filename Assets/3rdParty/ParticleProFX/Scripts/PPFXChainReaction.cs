/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXChainReaction.cs
	
	Create chain reactions with multiple prefabs.
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PPFXChainReaction : MonoBehaviour {
	
	public List<GameObject> objects = new List<GameObject>();
	public List<float> cloneTime = new List<float>();
	public List<Vector3> clonePosition = new List<Vector3>();
	
	public bool destroyLastPrefab = false;
	
	GameObject container; //store current instantiated prefab
	float duration;
	bool isLooping = false;
	int currentIndex = 0;
	
	void Start () 
	{
		StartCoroutine(CloneIndex(currentIndex));	
	}

	
	IEnumerator CloneIndex(int _inx)
	{
		yield return new WaitForSeconds(cloneTime[_inx]);
		
		if(destroyLastPrefab)
		{
			Destroy(container);
		}
		
		container = Instantiate(objects[_inx], transform.position + clonePosition[_inx], objects[_inx].transform.rotation)as GameObject;
		container.transform.parent = this.transform;
		
		ParticleSystem _ps = container.GetComponent<ParticleSystem>();
		
		if (_ps != null)
		{
			#if UNITY_5_5_OR_NEWER
				duration = _ps.main.duration;
				isLooping = _ps.main.loop;
			#else
				duration = _ps.duration;
				isLooping = _ps.loop;
			#endif
			
			StartCoroutine(Wait(cloneTime[_inx]));	
		}
		yield return null;
	}
	
	IEnumerator Wait(float _time)
	{
		yield return new WaitForSeconds(_time);
		
		currentIndex ++;
		
		if(currentIndex < objects.Count)
		{
			StartCoroutine(CloneIndex(currentIndex));
		}
		else
		{
			yield return new WaitForSeconds(duration);
			
			if(!isLooping)
			{
				Destroy(this.gameObject);
			}
		}
	}

}
