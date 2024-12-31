/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXAutodestruct.cs
	
	Simple auto destruct script. Destroys particle system
	after duration + lifetime is over.
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;

public class PPFXAutodestruct : MonoBehaviour {
	
	ParticleSystem ps;
	
	void Start () {
		ps = this.GetComponent<ParticleSystem>();
	 	if(ps)
	 	{         
		 	#if UNITY_5_5_OR_NEWER
			 	var _m = ps.main;
			 	var _t = _m.duration + _m.startLifetime.constantMin;
			 	if (!_m.loop){
				 	Destroy(this.gameObject, _t);
			 	}
		 	#else	 	
	            if (!ps.loop) {
			        Destroy(this.gameObject, ps.duration + ps.startLifetime);
			    }
		    #endif
        }
	}
	
	public void DestroyPSystem(GameObject _ps)
	{
		ParticleSystem _pss = _ps.GetComponent<ParticleSystem>();
		
		
	 	#if UNITY_5_5_OR_NEWER
			var _m = _pss.main;
			var _t = _m.duration + _m.startLifetime.constantMin;
			Destroy(_ps, _t);
		#else
			Destroy(_ps, _pss.duration + _pss.startLifetime);
		#endif	
	}
}
