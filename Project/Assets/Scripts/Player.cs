using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure; // Required in C#

public class Player : MonoBehaviour {
	public GUIImages images;

	public PlayerStats stats;
	public CelestialAlignment celestialAlignment;
	public PlayerIndex playerIndex;
	public PlayerInput playerInput;

	public List<PowerUp> powerUpsInEffect;

	new Transform transform;
	Rigidbody2D rigidBody;

	float originalSuckPower;
	bool nextPressed, prevPressed = false;

	public Inventory inventory;

//	GameManager gameManager;
//	Soul soul;
	Vector2 bottomLeftCoords, topRightCoords;

	void Awake(){
		CacheInternalReferences();
	}

	void Start () {
//		CacheExternalReferences();
		SetBounds();
		Align();
	}
	
	void Update () {
		CheckInput();
		UpdateVacuumPosition();
//		SuckSoul();
//		SuckPowerUps();
	}

//	void FixedUpdate(){
//		UpdateCoolDowns();
//	}

	void CacheInternalReferences(){
		playerInput = new PlayerInput(playerIndex);
		originalSuckPower = stats.suckingPower;
		transform = this.GetComponent<Transform>();
		rigidBody = this.GetComponent<Rigidbody2D>();
		inventory = new Inventory(images);
		powerUpsInEffect = new List<PowerUp>();
	}

	void CacheExternalReferences(){
//		soul = Game.soul;
//		gameManager = Game.gameManager;
	}

	void SetBounds(){
		bottomLeftCoords = Game.screen.bottomLeft;
		topRightCoords = Game.screen.topRight;
	}

	void Align(){
		if(celestialAlignment == CelestialAlignment.HEAVEN){
			// Ubicar en top de la pantalla y rotar 180 grados para que este orientado hacia abajo
			transform.position = new Vector3(Game.screen.topMiddle.x, Game.screen.topMiddle.y, -1f);
			transform.localRotation = new Quaternion(0f, 0f, 180f, 0f);
		}else{
			// Ubicar en bottom
			transform.position = new Vector3(Game.screen.bottomMiddle.x, Game.screen.bottomMiddle.y, -1f);
		}
		stats.transform = transform;
	}

	/*
	void SuckSoul(){

		float forceX, forceY;
		Vector2 dist = transform.position - soul.transform.position;

		// Calculo el "angulo" entre el alma y la aspiradora. Dot 1/-1 estoy en linea recta perfecta. Dot 0 esta a un costado.
		float dotAngle = Vector2.Dot(transform.up.normalized, dist.normalized);
		if(dotAngle == 0){
			// ya estoy en un extremo de la pantalla 
			return;										
		}else if(dotAngle < 0){
			// angulo absoluto con la recta imaginaria del up del player, sin importar de que lado de ella este
			dotAngle = -dotAngle;
		}

		// Calculo la fuerza en x
		forceX = dist.x * dotAngle * dotAngle;

		// Calculo la fuerza en y
		float dotRange = 1-angleThreshold;	// va de 0 (el dot minimo en el que deja de aplicar fuerza) a 1 (linea recta perfecta)
		float normalizedDotAngle = (dotAngle - angleThreshold)/dotRange; // calculo que valor representa el dot actual en la escala 0-1 del angularRange
		forceY = suckingPower * normalizedDotAngle/dist.y;

		// Creo la fuerza con ambos componentes (x, y) y se la agrego al alma
		Vector2 suckForce = new Vector2(forceX, forceY);
		soul.AddSuckingForce(suckForce);
	}
	*/

	public void RegisterNewEffect(PowerUp p){
		// FIXME logica de stackeo de efectos, deberia ir aca??? PASAR A QEUE
		// FIXME otra opcion puede ser que no se stackeen los del mismo tipo??? || que stackeen menor efecto???
		if(powerUpsInEffect.Count > 2){
			Debug.Log("Habia tres? " + powerUpsInEffect.Count);
			powerUpsInEffect[0].RemoveEffectTo(this);
		}
		powerUpsInEffect.Add(p);
	}

	public void UnregisterEffect(PowerUp p){
		powerUpsInEffect.Remove(p);
	}

	void UpdateVacuumPosition(){
		rigidBody.AddForce(Vector2.right * playerInput.Move * stats.moveSpeed);
		if(transform.position.x <= bottomLeftCoords.x){
			// se pasa de la izq
			rigidBody.velocity = Vector2.zero;
			transform.position = new Vector3(bottomLeftCoords.x, transform.position.y, 0);
		}else if(transform.position.x >= topRightCoords.x){
			// se pasa de la der
			rigidBody.velocity = Vector2.zero;
			transform.position = new Vector3(topRightCoords.x, transform.position.y, 0);
		}
		stats.transform = transform;
	}

	void CheckInput(){
		playerInput.UpdateInput();

		if(playerInput.A == UsefulButtonStates.Down){
			Debug.Log("<color="+Utils.GetColor(celestialAlignment)+">Parar de succionar, A</color>");
			stats.isSucking = false;
			stats.suckingPower = 0;
		}else if(playerInput.A == UsefulButtonStates.Up){
			stats.isSucking = true;
			stats.suckingPower = originalSuckPower;
		}

		if(playerInput.X == UsefulButtonStates.Down){
			try{
				Debug.Log("<color="+Utils.GetColor(celestialAlignment)+">Se arrojo un " + inventory.GetSelectedType()+"</color>");
				inventory.GetAndRemoveSelected().Throw();
//				inventory.RemoveSelected();
			}catch(System.ArgumentOutOfRangeException){
				Debug.Log("<color="+Utils.GetColor(celestialAlignment)+">No hay nada en el inventario</color>");
			}
		}

		if(playerInput.B == UsefulButtonStates.Down){
			Debug.Log("Soplar, B");
			stats.suckingPower *= -1f;
		}else if(playerInput.B == UsefulButtonStates.Up){
			Debug.Log("NO Soplar, B");
			stats.suckingPower *= -1f;
		}

		if(playerInput.Prev !=0)
		{
			if(prevPressed == false)
			{
				prevPressed = true;
				inventory.SelectPrev();
			}
		}
		if(playerInput.Prev == 0)
		{
			prevPressed = false;
		}


		if(playerInput.Next != 0)
		{
			if(nextPressed == false)
			{
				nextPressed = true;
				inventory.SelectNext();
			}
		}
		if(playerInput.Next == 0)
		{
			nextPressed = false;
		}
	}
}

public enum CelestialAlignment {
	HEAVEN,
	HELL
}

[System.Serializable]
public class PlayerStats {
	public float moveSpeed = 15f;
	public float suckingPower = 140f;
	public float angleThreshold = 0.97f;
	public bool canSuck = true;

	public bool isSucking = true;
	public Transform transform;
}

public class PlayerInput {
	[HideInInspector]
	public UsefulButtonStates A, B, X;
	[HideInInspector]
	public float Move, Prev, Next;

	PlayerIndex playerIndex;
	GamePadState state, prevState;
	bool stateWasInitialized = false;

	public PlayerInput(PlayerIndex index){
		playerIndex = index;
	}

	public void UpdateInput(){
		if(!stateWasInitialized){
			prevState = GamePad.GetState(playerIndex);
			stateWasInitialized = true;
		}else
			prevState = state;
		state = GamePad.GetState(playerIndex);

		A = GetState(state.Buttons.A, prevState.Buttons.A);
		B = GetState(state.Buttons.B, prevState.Buttons.B);
		X = GetState(state.Buttons.X, prevState.Buttons.X);

		Move = state.ThumbSticks.Left.X;
		Prev = state.Triggers.Left;
		Next = state.Triggers.Right;

	}

	UsefulButtonStates GetState(ButtonState curr, ButtonState prev){
		if(prev == ButtonState.Released && curr == ButtonState.Pressed){
			return UsefulButtonStates.Down;
		}else if(prev == ButtonState.Pressed && curr == ButtonState.Released){
			return UsefulButtonStates.Up;
		}else if(curr == ButtonState.Pressed){
			return UsefulButtonStates.Pressed;
		}
		return UsefulButtonStates.Released;
	}
}

public enum UsefulButtonStates {
	Down,
	Up,
	Pressed,
	Released
}