using UnityEngine;
using System.Collections;

public enum PowerUpState {
	IN_PLAY,
	COLLECTED,
	COLLECTED_BY_ENEMY,
	TROWN,
	DODGED,
	SELF_DESTROY
}

public class PowerUp : MonoBehaviour {
	public string typeName {get; protected set;}
	public CelestialAlignment celestialAlignment;
	public float probability;
	float selfDestroyTime = 9f; 											// TODO hacer publica y que varie segun el tipo
	float timeOnInitialization;
	public float effectDuration;
	public IEnumerator effectsInPlay;
	protected bool effectWasApplied = false;
	public float typeCoolDown = 0f;
	public Sprite icon;
	float angleThreshold = 0.98f;

	new Transform transform;
	Rigidbody2D rigidBody;
	protected PowerUpState state = PowerUpState.IN_PLAY;

	int playerIndex, enemyIndex;
	Player[] players; 														//TODO reemplazar por player y enemigo!

	PowerUpSpawner spawner;
	
	void Awake(){
		CacheInternalReferences();
	}

	void Start(){
		CacheExternalReferences();
	}

	void Update(){
		switch(state){
		case PowerUpState.IN_PLAY:
			UpdateSelfDestroyTimer();
			InPlayUpdate();
			break;
		case PowerUpState.TROWN:
			ThrownUpdate();
			break;
		}
	}

	void LateUpdate(){
		switch(state){
		case PowerUpState.IN_PLAY:
			CheckForCollections();
			break;
		case PowerUpState.TROWN:
			CheckForEnemyCollection();
			break;
		}
	}


	void CacheInternalReferences(){
		transform = this.GetComponent<Transform>();
		rigidBody = this.GetComponent<Rigidbody2D>();
		icon = this.GetComponent<SpriteRenderer>().sprite;
	}

	void CacheExternalReferences(){
		players = Game.players;
		for(int i=0; i<players.Length;++i){
			if(players[i].celestialAlignment == this.celestialAlignment)
				playerIndex = i;
			else
				enemyIndex = i;
		}
		spawner = Game.spawner;
	}

	protected virtual void InPlayUpdate(){
		for(int i=0; i<players.Length;++i){
			PlayerStats playerStats = players[i].stats;
			
			float forceX, forceY;
			Vector2 dist = playerStats.transform.position - transform.position;
			
			// Calculo el "angulo" entre el alma y la aspiradora. Dot 1/-1 estoy en linea recta perfecta. Dot 0 esta a un costado.
			float dotAngle = Vector2.Dot(playerStats.transform.up.normalized, dist.normalized);
			if(dotAngle == 0){
				// ya estoy en un extremo de la pantalla 
				return;										
			}else if(dotAngle < 0){
				// angulo absoluto con la recta imaginaria del up del player, sin importar de que lado de ella este
				dotAngle = -dotAngle;
			}
			
			if(dotAngle < this.angleThreshold){
				continue;
			}
			
			float alignmentFactor;
			if(i == enemyIndex)
				alignmentFactor = 0.6f;
			else
				alignmentFactor = 1.8f;
			
			// Calculo la fuerza en y
			float dotRange = 1-angleThreshold;	// va de 0 (el dot minimo en el que deja de aplicar fuerza) a 1 (linea recta perfecta)
			float normalizedDotAngle = (dotAngle - angleThreshold)/dotRange; // calculo que valor representa el dot actual en la escala 0-1 del angularRange
			float suckingPower = (playerStats.isSucking) ? playerStats.suckingPower : 0;
			forceY = (suckingPower * normalizedDotAngle/dist.y) * alignmentFactor;
			
			// Calculo la fuerza en x
			forceX = dist.x * normalizedDotAngle;
			forceX *= 0.1f;
			
			// Creo la fuerza con ambos componentes (x, y) y se la agrego al alma
			Vector2 suckForce = new Vector2(forceX, forceY);
			rigidBody.AddForce(suckForce);
		}
	}

	void CheckForCollections(){
		for(int i=0; i<players.Length;++i){
			if(Mathf.Abs(players[i].stats.transform.position.y - transform.position.y) <= 0.5f){
				if(i == playerIndex){
					CollectedBy(players[i]);
				}else{
					CollectedByEnemy(players[i]);
				}
			}
		}  
	}

	void CollectedBy(Player player){
		state = PowerUpState.COLLECTED;
		player.inventory.Add(this);
		spawner.RemoveFromScreen(this);
		gameObject.SetActive(false);
	}

	public virtual void Throw(){
		gameObject.SetActive(true);
		state = PowerUpState.TROWN;
		Soul soul = Game.soul;
		float xPos, yPos;
		PlayerStats playerStats = players[enemyIndex].stats;
		xPos = playerStats.transform.position.x;
		yPos = (playerStats.transform.position.y - soul.transform.position.y) * 0.25f;
		this.transform.position = new Vector3(xPos,yPos,0f);
	}

	protected virtual void ThrownUpdate(){
		// TODO logica de movimiento del power up al ser succionado solo por el enemigo
		PlayerStats playerStats = players[enemyIndex].stats;
		float suckingPower = (playerStats.isSucking) ? playerStats.suckingPower : 0;
		Vector2 suckForce = playerStats.transform.up * -suckingPower * 1.1f; //FIXME ver que onda con la escupida o si se estackean powerUps
		rigidBody.AddForce(suckForce);
	}

	void CheckForEnemyCollection(){
		if(Mathf.Abs(players[enemyIndex].stats.transform.position.y - transform.position.y) <= 0.5f){
			if(!players[enemyIndex].stats.isSucking){
				// Ya no estaba chupando???? //FIXME deberia acercarse el powerUp??
				Dodged();
				return;
			}
			if(Mathf.Abs(players[enemyIndex].stats.transform.position.x - transform.position.x) > 0.5f){
				// Paso la linea pero se movio en x y lo esquivo
				Dodged();
				return;
			}
			CollectedByEnemy(players[enemyIndex]);
			return;
		}
		if(Mathf.Abs(players[playerIndex].stats.transform.position.y - transform.position.y) <= 0.5f){
			// Fue soplado por el enemigo y paso la linea friendly y a la mier...
			Dodged();
			return;
		}
	}


	void UpdateSelfDestroyTimer(){
		if(Time.time - timeOnInitialization >= selfDestroyTime){
			state = PowerUpState.SELF_DESTROY;
			// TODO llamar corrutina de animacion o lo que sea | efectos en los segundos previos al destruir
			DeletePowerUp();
		}
	}

	void Dodged(){
		Debug.Log("<color="+Utils.GetInverseColor(this.celestialAlignment)+">Esquivado</color>");
		state = PowerUpState.DODGED;
		DeletePowerUp();
	}

	void CollectedByEnemy(Player enemy){
		state = PowerUpState.COLLECTED_BY_ENEMY;
		enemy.RegisterNewEffect(this);
		//ApplyEffectTo(enemy);
		effectsInPlay = ApplyEffectTo(enemy);
		StartCoroutine(effectsInPlay);
	}

	public virtual IEnumerator ApplyEffectTo(Player enemy){yield break;}
	public virtual void RemoveEffectTo(Player enemy){}
	public void DeletePowerUp(Player enemy = null){
//		Debug.Log("Destruite!!!!");
		if(enemy != null)
			enemy.UnregisterEffect(this);
		spawner.ReturnToPool(this);
//		Destroy(this.gameObject);
	}

	public void InitializeAt(Vector3 pos){
		gameObject.SetActive(true);
		transform.position = pos;
		state = PowerUpState.IN_PLAY;
		timeOnInitialization = Time.time;
	}

	public void Finalize(){
		gameObject.SetActive(false);
	}
}