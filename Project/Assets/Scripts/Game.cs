using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {

	public static GameManager gameManager;
	public static Player[] players;
	public static int hellPlayerIndex, heavenPlayerIndex;
	public static Soul soul;
	public static Bounds screen;
	public static PowerUpSpawner spawner;

	public float bgSize;
	public float viewSize;
	public float gameScreenSize;
	
	void Awake () {
		Screen.SetResolution(1080,1920,false);
		SetScreenBounds();
		SetBackgroundSize();
		CacheReferences();
	}

	void CacheReferences(){
		// FIXME asegurarme de que solo lea dos? Quizas Tag "HellPlayer" y tag "HeavenPlayer"?
		GameObject[] gos = GameObject.FindGameObjectsWithTag("Player");
		if(gos.Length > 0){
			players = new Player[gos.Length];
			for(int i=0;i<gos.Length;++i){
				players[i] = gos[i].GetComponent<Player>();
				if(players[i].celestialAlignment == CelestialAlignment.HEAVEN){
					heavenPlayerIndex = i;
				}else{
					hellPlayerIndex = i;
				}
			}
		}
		soul = GameObject.FindGameObjectWithTag("Soul").GetComponent<Soul>();
		gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
	}

	void SetScreenBounds(){
		screen = new Bounds();
		Camera cam = Camera.main;
		screen.bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0f, 0f));
		screen.bottomMiddle = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f));
		screen.topRight = cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
		screen.topMiddle = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
		screen.middle = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
	}

	void SetBackgroundSize(){
//		bgSize = topBg.position.y - bottomBg.position.y;
//		viewSize = screen.topMiddle.y - screen.bottomMiddle.y;
//		gameScreenSize = bgSize + viewSize;
	}
}

public class Bounds {
	public Vector2 topRight, topMiddle, bottomLeft, bottomMiddle, middle;
}
