/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXRigidbodyExtensions.cs
	
	Allows to use a Vector3 for the upwardsModifier 
	on a rigidbody AddExplosionForce
	
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;
 

public static class PPFXRigidbodyExtensions : object {
    
    public static void AddExplosionForce (this Rigidbody body, float explosionForce, Vector3 explosionRadiusCenter, float explosionRadius) 
    {
        AddExplosionForce(body, explosionForce, explosionRadiusCenter, explosionRadius, new Vector3(0F, 0F, 0F));
    }
    
    public static void AddExplosionForce (this Rigidbody body, float explosionForce, Vector3 explosionRadiusCenter, float explosionRadius, Vector3 explosionOriginPoint) 
    {
        AddExplosionForce(body, explosionForce, explosionRadiusCenter, explosionRadius, explosionOriginPoint, ForceMode.Force);
    }
   
    public static void AddExplosionForce ( 
        this Rigidbody body
        , float explosionForce
        , Vector3 explosionRadiusCenter
        , float explosionRadius
        , Vector3 explosionOriginPoint // this is the oposite from upwardsModifier
        , ForceMode mode
    )
    {
        if (Vector3.Distance(body.transform.position, explosionRadiusCenter) <= explosionRadius) 
        {
            Vector3 force = (body.transform.position - (explosionRadiusCenter + explosionOriginPoint));
            body.AddForce(force * (explosionForce/5), mode); // 5 came from experimentation
        }
    }
}