/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXClickMove.cs
	
	Can be used for Beam Prefabs.
	Moves particle beam to the clicked position.
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXClickMoveSmooth : MonoBehaviour {

	public float speed = 5f;
	public string tagName = "plane";
	
	//distance check
	float radius = 2.0f;
	float dist = 0f;	
	bool anim = false;
	
	
	void Update () 
	{
		//check if user has clicked
		if(Input.GetMouseButtonDown(0))
		{    
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	        RaycastHit hit;
	        if(Physics.Raycast(ray,out hit))
	        {
	        	//check if user has clicked on tagged object
	            if(hit.collider.tag==tagName)
	            {             
	                dist = Vector3.Distance(transform.position, hit.point);	
	                if(!anim){	
	                	anim = true;
		                StartCoroutine(Move(hit.point));
	            	}
	            }
	        }
	    }
	}
	
	
	IEnumerator Move(Vector3 _newPos)
	{

		_newPos = new Vector3(_newPos.x, 0.0f, _newPos.z);
    	
    	//move to new position
        while(dist > radius)
        { 
        	
        	float step = speed * Time.deltaTime;
        	Vector3 velocity = Vector3.zero;
    		transform.position = Vector3.SmoothDamp(transform.position,_newPos,ref velocity, step);
        	
        	dist = Vector3.Distance(transform.position, _newPos);	
        	
        	
        	yield return null;
        }
        
    	anim = false;
	}
}
