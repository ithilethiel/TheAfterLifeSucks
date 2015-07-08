using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpsPool {
	public string type {get; private set;}
	Stack<PowerUp> poolStack;

	public PowerUpsPool(int size, GameObject prefab, Transform parent = null) {
		poolStack = new Stack<PowerUp>();
		size*= 2;

		for(int i = 0; i <= size; ++i){
			GameObject go = GameObject.Instantiate(prefab,Vector3.zero, Quaternion.identity) as GameObject;
			go.transform.parent = parent;
			poolStack.Push(go.GetComponent<PowerUp>());
			if(i == 0)
				type = poolStack.Peek().typeName;
			go.SetActive(false);
			go = null;
		}
	}

	public float spawnChances {
		get {
			if(poolStack.Count > 0)
				return poolStack.Peek().probability;
			else
				return 0;
		}
	}

	public bool hasAvailableInstances {
		get {return poolStack.Count > 0;}
	}

	public PowerUp GetInstance(){
		return poolStack.Pop();
	}

//	public PowerUp GetFreeInstance(){
//		if(usableShit.Count == 0)
//			return null;
//		usableShit.Peek().gameObject.SetActive(true);
//		return usableShit.Pop();
//	}

	public void ReturnInstance(PowerUp p){
		poolStack.Push(p);
	}
}
