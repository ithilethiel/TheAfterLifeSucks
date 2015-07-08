using UnityEngine;
using System.Collections;

public enum SoulState {
	IN_HEAVEN,
	IN_HELL,
	IN_BETWEEN
}

public class Soul : MonoBehaviour {

	Rigidbody2D rigidBody;										// para agregar fuerzas al "alma" en si
	[HideInInspector] new public Transform transform;			// nuevo y cacheado Transform (evita el GetComponent interno)

	public Transform topBg, bottomBg;							// forma de ubicar donde empieza y termina el fondo (no tengo forma de saber su alto si no)
	public Rigidbody2D backgroundRigidbody;						// usado para argregarle fuerzas
	Transform backgroundTransform;								// referencia a la posicion en cada vuelta del loop
	float backgroundOriginalY;									// referencia al "cero" original
	float backgroundHeight; 									// alto del escenario (para saber cuando mover el fondo y cuando el alma)
	Vector2 bottomLeftCoords, topRightCoords;					// referencias a los margenes de la pantalla (para saber cuando gana alguien / cuando vuelve a IN_BETWEEN)
	public float screenHeight; 									//FIXME mover a GAME! PowerUpSpawner lo necesita, pero no deberia estar aca!

	[HideInInspector] public float normalizedSoulValue = 0f;	// posicion entre cielo (1) e infierno (-1)
	float heavenValue, relativeSoulValue;						// posicion absoluta del tope del fondo (cielo) | posicion absoluta del alma previa a ser normalizada
	SoulState soulState = SoulState.IN_BETWEEN;					// en que porcion de espacio se encuentra

	Player[] players;											// referencia a ambos jugadores


	void Awake(){
		CacheInternalReferences();
	}

	void Start(){
		CacheExternalReferences();
		CalculateFixedValues();
		Align();
	}

	void Update(){
		CalculateSuckingForces();
	}

	void LateUpdate () {
		UpdateSoulStateForTracking();
	}


	void CacheInternalReferences(){
		transform = this.GetComponent<Transform>();
		rigidBody = this.GetComponent<Rigidbody2D>();

		backgroundTransform = backgroundRigidbody.gameObject.transform;
		backgroundOriginalY = backgroundTransform.position.y;
	}

	void CacheExternalReferences(){
		players = Game.players;

		bottomLeftCoords = Game.screen.bottomLeft;
		topRightCoords = Game.screen.topRight;
	}

	void CalculateFixedValues(){
		backgroundHeight = topBg.position.y - bottomBg.position.y;
		screenHeight = topRightCoords.y - bottomLeftCoords.y;
		heavenValue = backgroundOriginalY + backgroundHeight * 0.5f - 0.4f;
		// hellValue = originalY - bgHeight * 0.5f + 0.4f;
	}

	void Align(){
		transform.position = Game.screen.middle;
	}

	void CalculateSuckingForces(){
		for(int i=0; i<players.Length;++i){
			PlayerStats playerStats = players[i].stats;

			if(!playerStats.canSuck)
				continue;
			// Debug.Log(players[i].celestialAlignment + " , force: " + playerStats.suckingPower);

			float forceX, forceY;
			Vector2 dist = playerStats.transform.position - transform.position;
			
			// Calculo el "angulo" entre el alma y la aspiradora. Dot 1/-1 estoy en linea recta perfecta. Dot 0 esta a un costado.
			float dotAngle = Vector2.Dot(playerStats.transform.up.normalized, dist.normalized);
			if(dotAngle == 0){
				return;						// ya estoy en un extremo de la pantalla 								
			}else if(dotAngle < 0){
				dotAngle = -dotAngle;		// angulo absoluto con la recta imaginaria del up del player, sin importar de que lado de ella este
			}
			
			// Calculo la fuerza en x
			forceX = dist.x * dotAngle * dotAngle;
			
			// Calculo la fuerza en y
			float dotRange = 1-playerStats.angleThreshold;									// de 0 (el dot minimo en el que deja de aplicar fuerza) a 1 (linea recta perfecta)
			float normalizedDotAngle = (dotAngle - playerStats.angleThreshold)/dotRange; 	// calculo que valor representa el dot actual en la escala 0-1 del dotRange
			float suckingPower = (playerStats.isSucking) ? playerStats.suckingPower : 0;
			forceY = suckingPower * normalizedDotAngle/dist.y;
			
			// Creo la fuerza con ambos componentes (x, y) y se la agrego al alma
			Vector2 suckForce = new Vector2(forceX, forceY);
			AddSuckingForce(suckForce);
		}
	}

	public void AddSuckingForce(Vector2 force){
		if(soulState != SoulState.IN_BETWEEN)
			rigidBody.AddForce(force);
		else
			backgroundRigidbody.AddForce(force * -1);
	}


	void UpdateSoulStateForTracking(){

		// Actualizo la info de donde esta el alma en relacion a cielo/infierno
		relativeSoulValue = (backgroundOriginalY - backgroundTransform.position.y) - (0 - transform.position.y);
		normalizedSoulValue = relativeSoulValue / heavenValue;

		switch (soulState){
		case SoulState.IN_BETWEEN:
			// Alcanzo el cielo o el infierno?
			if(backgroundTransform.position.y >= backgroundOriginalY + backgroundHeight * 0.5f - screenHeight * 0.5f){
				backgroundRigidbody.velocity = Vector2.zero;
				soulState = SoulState.IN_HELL;
			}else if(backgroundTransform.position.y <= backgroundOriginalY - backgroundHeight * 0.5f + screenHeight * 0.5f){
				backgroundRigidbody.velocity = Vector2.zero;
				soulState = SoulState.IN_HEAVEN;
			}
			break;
		case SoulState.IN_HEAVEN:
			// Volvio a ser chupado hasta el medio? O llego hasta el tope y gano el cielo? // FIXME que compare el x con la aspiradora
			if(transform.position.y < topRightCoords.y - screenHeight * 0.5f){
				soulState = SoulState.IN_BETWEEN;
				transform.position = new Vector3(transform.position.x, 0, 0);
			}else if(Mathf.Abs(topRightCoords.y - transform.position.y) <= 0.5f){
				GameOver(CelestialAlignment.HEAVEN);
			}
			break;
		case SoulState.IN_HELL:
			// Volvio a ser chupado hasta el medio? O llego hasta el tope y gano el infierno? // FIXME que compare el x con la aspiradora
			if(transform.position.y > bottomLeftCoords.y + screenHeight * 0.5f){
				soulState = SoulState.IN_BETWEEN;
				transform.position = new Vector3(transform.position.x, 0, 0);
			}else if(Mathf.Abs(bottomLeftCoords.y - transform.position.y) <= 0.5f){
				GameOver(CelestialAlignment.HELL);
			}
			break;
		}		
	}

	void GameOver(CelestialAlignment winner){
		Debug.LogError("GameOver, gano: " + winner.ToString()); // FIXME poner en game manager o ver que onda
		Application.LoadLevel(0);
	}

//
//	void LateUpdate () {
//		if(transform.position.y >= topRightCoords.y - margin){
//			rigidBody.velocity = Vector2.zero;
//			outOfBounds = true;
//			transform.position = new Vector3(transform.position.x, topRightCoords.y, 0);
//		}else if (transform.position.y <= bottomLeftCoords.y + margin){
//			rigidBody.velocity = Vector2.zero;
//			outOfBounds = true;
//			transform.position = new Vector3(transform.position.x, bottomLeftCoords.y, 0);
//		}
//	}
}
