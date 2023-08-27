/*=========================================================
	PARTICLE PRO FX volume one 
	PPFXChainReactionInspector.cs
	
	Chain reaction custom inspector
	
	(c) 2014
=========================================================*/

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PPFXChainReaction))]
public class PPFXChainReactionInspector : Editor 
{
    
    private PPFXChainReaction CRScript; 
    
    void Awake(){
    	CRScript = (PPFXChainReaction)target;
    }
    
    public override void OnInspectorGUI()
    {
               
        EditorGUILayout.Space();
               
        CRScript.destroyLastPrefab = EditorGUILayout.Toggle("Destroy last prefab", CRScript.destroyLastPrefab);
    	
    	EditorGUILayout.Space();
    	
    	if(GUILayout.Button("+ add prefab"))
		{
		    AddNewPrefab();
		}
    	
    	for(int i = 0; i < CRScript.objects.Count; i ++ ){
    		
    		EditorGUILayout.BeginHorizontal("Box");
				
				EditorGUILayout.BeginVertical();
				
					EditorGUILayout.LabelField(i.ToString());
		    		CRScript.objects[i] = (GameObject)EditorGUILayout.ObjectField("prefab:",CRScript.objects[i], typeof(GameObject), true); 
	    			CRScript.cloneTime[i] = EditorGUILayout.FloatField("spawn time:",CRScript.cloneTime[i]);	
    				CRScript.clonePosition[i] = EditorGUILayout.Vector3Field("position:", CRScript.clonePosition[i]);

		    	
		    	if(GUILayout.Button("-"))
		    	{
		    		CRScript.objects.RemoveAt(i);
		    		CRScript.cloneTime.RemoveAt(i);
		    		CRScript.clonePosition.RemoveAt(i);
		    	}
		    	
		    	EditorGUILayout.EndVertical();
		    	
    		EditorGUILayout.EndHorizontal();
    		
    	}
    }
    
    void AddNewPrefab()
    {
    	GameObject _tmp = null;
    	CRScript.objects.Add(_tmp);
    	CRScript.cloneTime.Add(0f);
    	CRScript.clonePosition.Add(new Vector3(0,0,0));
    }
}
#endif