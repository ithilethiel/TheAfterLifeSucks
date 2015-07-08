using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpSpawner : MonoBehaviour {
	float minDistBetweenPowerUps = 1f;													
	public int maxPowerUpsOnScreen = 5;

	public GameObject[] PowerUps_Hell;							// Solo para asignar los prefabs del cielo en el Inspector
	public GameObject[] PowerUps_Heaven;						// Solo para asignar los prefabs del infierno en el Inspector

	PowerUpsPool[] pooledHellPowerUps, pooledHeavenPowerUps;	// Pool de powerUps disponibles para cielo e infierno

	Transform parent;											// Bajo que Transform se instancian los powerUps

	Player heavenPlayer, hellPlayer;
	IEnumerator spawnRoutine;									// Referencia a la corrutina de spawneo de powerUps
	bool spawnActive = true;
	List<int> posibleTypesIndices = new List<int>();			// Lista de indices de powerUps que podrian lanzarse en cada ciclo

	List<PowerUp> powerUpsOnScreen = new List<PowerUp>();		// Inlcuye solo los recien spawneados y aun no recolectados por nadie

	void Awake(){
		CacheInternalReferences();
	}

	void Start(){
		CacheExternalReferences();
		CreatePowerUpPools();
		StartSpawning();
	}

	void CacheInternalReferences(){
		Game.spawner = this;
		parent = this.GetComponent<Transform>();
	}

	void CacheExternalReferences(){
		for(int i=0; i<Game.players.Length;++i){
			if(Game.players[i].celestialAlignment == CelestialAlignment.HEAVEN)
				heavenPlayer = Game.players[i];
			else
				hellPlayer = Game.players[i];
		}
	}

	void CreatePowerUpPools(){
		pooledHellPowerUps = new PowerUpsPool[PowerUps_Hell.Length];
		for(int i=0;i<PowerUps_Hell.Length;++i){
			pooledHellPowerUps[i] = new PowerUpsPool(hellPlayer.inventory.maxAmountOfEach, PowerUps_Hell[i], parent);
		}

		pooledHeavenPowerUps = new PowerUpsPool[PowerUps_Heaven.Length];
		for(int i=0;i<PowerUps_Heaven.Length;++i){
			pooledHeavenPowerUps[i] = new PowerUpsPool(heavenPlayer.inventory.maxAmountOfEach, PowerUps_Heaven[i], parent);
		}
	}

	void StartSpawning(){
		spawnRoutine = SpawnCycle();
		StartCoroutine(spawnRoutine);
	}

	IEnumerator SpawnCycle(){
		Soul soul;
		float waitSeconds = 0;

		while(spawnActive){
			soul = Game.soul;

			if(soul.normalizedSoulValue > 0.4f){
				// va ganando cielo, ayuda al infierno
				Spwan(CelestialAlignment.HELL, soul);
			}else if(soul.normalizedSoulValue < -0.4f){
				// va ganando infierno, ayuda al cielo
				Spwan(CelestialAlignment.HEAVEN, soul);
			}else{
				// mas o menos empatados por el centro
				if(Random.value >= 0.5f){
					Spwan(CelestialAlignment.HEAVEN, soul);
				}else{
					Spwan(CelestialAlignment.HELL, soul);
				}
			}
			Debug.Log("Wait seconds");
			yield return new WaitForSeconds(Random.Range(0.8f, 2.5f));
		}
		yield break;
	}

	void Spwan(CelestialAlignment alignment, Soul soul){

		// Ya hay en el escenario el maximo permitido?
		if(powerUpsOnScreen.Count >= maxPowerUpsOnScreen)
			return;

		// De que pool voy a sacar powerUps
		PowerUpsPool[] pool = pooledHeavenPowerUps;
		int alignmentCorrection = 1;
		
		if(alignment == CelestialAlignment.HELL){
			pool = pooledHellPowerUps;
			alignmentCorrection = -1;
		}
		
		// Que powerUp lanzar
		int powerUpTypeIndex = -1;
		
		for(int i=0; i<pool.Length;++i){
			float randomNum = Random.value;
			if(pool[i].hasAvailableInstances && randomNum <= pool[i].spawnChances){
				powerUpTypeIndex = i;
				posibleTypesIndices.Add(i);
			}
		}
		
		if(posibleTypesIndices.Count != 0){
			powerUpTypeIndex = posibleTypesIndices[Mathf.FloorToInt(Random.Range(0f, posibleTypesIndices.Count -1))];
			posibleTypesIndices.Clear();
		}
		
		if(powerUpTypeIndex == -1)
			return;
		
		// Donde lanzarlo
		float xPos, yPos;
		
		// Horizontalmente
		xPos = HorizontalPos(soul);
		
		// Verticalmente
		bool tryOtherPos = true;							// me sirve una posicion tal, o tengo que revisar a ver si pisa algo?
		
		yPos = -soul.normalizedSoulValue + (0.2f * alignmentCorrection);
		yPos = (yPos > 0) ? Random.Range(0, yPos) : Random.Range(yPos, 0);
		yPos *= Game.soul.screenHeight;
		
		if(powerUpsOnScreen.Count == 0){
			tryOtherPos = false;							// no hay nada mas en el escenario, me sirve cualquiera
			Debug.Log("<color=green>Primero</color>");
		}
		
		int loops = 0;
		while(loops <= 5 && tryOtherPos){
			for(int i = 0; i<powerUpsOnScreen.Count;++i){
				if(Lanes.AlmostEquals(yPos, powerUpsOnScreen[i].transform.position.y, minDistBetweenPowerUps) && 
				   Lanes.PosToLane(xPos) == Lanes.PosToLane(powerUpsOnScreen[i].transform.position.x)){
					tryOtherPos = true;
					Debug.Log("<color=blue>Hay algo cerca!</color>");
					break;
				}else
					tryOtherPos = false;
			}
			
			if(tryOtherPos){
				yPos = -soul.normalizedSoulValue + (0.2f * alignmentCorrection);
				yPos = (yPos > 0) ? Random.Range(0, yPos) : Random.Range(yPos, 0);
				yPos *= Game.soul.screenHeight;
				yPos += (0.25f*loops*alignmentCorrection);
			}else{
				Debug.Log("<color=green>Found one!</color>");
			}
			++loops;
		}
		
		if(tryOtherPos)
			xPos = HorizontalPos(soul, xPos);
		
		Vector3 powerUpPosition = new Vector3(xPos, yPos, 0f);
		PowerUp p = pool[powerUpTypeIndex].GetInstance();
		p.InitializeAt(powerUpPosition);
		powerUpsOnScreen.Add(p);
		Debug.Log("<color=white>Spawn</color>");
	}
	
	float HorizontalPos(Soul soul, float? avoid = null){
		float xPos;
		int soulLane = Lanes.PosToLane(soul.transform.position.x);
		if(avoid != null){
			do{
				xPos = Lanes.RandomLanePos();
			}while(Lanes.PosToLane(xPos) == soulLane || Lanes.PosToLane(xPos) == avoid);
		}else{
			do{
				xPos = Lanes.RandomLanePos();
			}while(Lanes.PosToLane(xPos) == soulLane);
		}
		return xPos;
	}

	public int GetPoolIndexForType(PowerUp p){
		// Que indice en su pool es el tipo este??
		int index = 0;
		PowerUpsPool[] pool = pooledHeavenPowerUps;
		if(p.celestialAlignment == CelestialAlignment.HELL)
			pool = pooledHellPowerUps;

		for(int i = 0; i < pool.Length; ++i){
			if(pool[i].type == p.typeName){
				index = i;
				break;
			}
		}
		return index;
	}


	public void RemoveFromScreen(PowerUp p){
		if(powerUpsOnScreen.Contains(p))
			powerUpsOnScreen.Remove(p);
	}

	public void ReturnToPool(PowerUp p){
		Debug.Log("Return to pool");
		p.Finalize();
		switch(p.celestialAlignment){
		case CelestialAlignment.HELL:
			pooledHellPowerUps[GetPoolIndexForType(p)].ReturnInstance(p);
			break;
		case CelestialAlignment.HEAVEN:
			pooledHeavenPowerUps[GetPoolIndexForType(p)].ReturnInstance(p);
			break;
		}
		RemoveFromScreen(p);
	}

	void LateUpdate(){

		// spawnear algo? que espawnear y donde?

		// el y lo tomo de soul.normalizedPosY
		// el x de donde estoy, donde esta soul y donde esta el otro player 

		/*
		 * Deberia ver cuantas cosas spawneadas sin juntar hay? Tienen fade si no las agarras??
		 * 
		 * Si estoy perdiendo quiero tener algo mas o menos cerca en x (e y?)
		 * a menos que ambos players estemos lejos del soul -> en ese caso quiero algo cerca de soul
		 * 
		 * Si esta el soul en el centro Y (empate)
		 * quiero poner cosas hacia el lado que mas carriles libres tenga - idealmente opuesto a mi enemigo - lejos del soul
		 * 
		 * Si estoy ganando - cosas mas lejos en x e y y lejos del soul
		 * alguna del enemigo entre yo y soul quizas...
		 */
	}
}

public class Lanes {
	static float[] lanePositions = new float[5] {-2.65f, -1.4f, 0, 1.4f, 2.65f};
	
	public static int PosToLane(float pos){
		for(int i = 0; i < lanePositions.Length ; ++i){
			if(pos <= lanePositions[i] || i+1 == lanePositions.Length){
				// retorno el primer o ultimo lane si las posiciones exceden el min/max
				return i - (Mathf.FloorToInt(lanePositions.Length * 0.5f));						// en el array los index van de 0 a Length, pero lane 0 es Length/2
			}else{
				float midPointToNext = (lanePositions[i+1] - lanePositions[i]) * 0.5f;			// distancia entre un lane y el proximo
				midPointToNext += lanePositions[i];												// valor absouluto medio entre el centro de este lane y el del proximo
				if(pos <= midPointToNext)
					return i - (Mathf.FloorToInt(lanePositions.Length * 0.5f));
				continue;
			}
		}
		return 0;
	}
	
	public static float LaneToPos(int lane){
		try{
			return lanePositions[lane + Mathf.FloorToInt(lanePositions.Length * 0.5f)];
		}catch(System.IndexOutOfRangeException){
			Debug.LogWarning("No existe ese carril, te tiro un centro");
			return 0;
		}
	}
	
	public static float RandomLanePos(){
		return lanePositions[Mathf.FloorToInt(Random.Range(0f, (float)lanePositions.Length))];
	}
	
	public static bool AlmostEquals(float a, float b, float min)
	{
		return Mathf.Abs(a - b) <= min;
	}
}
