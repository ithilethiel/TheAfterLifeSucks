using UnityEngine;
using System.Collections;

public class Cloud : PowerUp {
	private const string CLOGTYPE = "CLOUD";

	public Cloud (){
		this.typeName = CLOGTYPE;
	}

	public override IEnumerator ApplyEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Nublado!</color>");
		yield return new WaitForSeconds(effectDuration);
		RemoveEffectTo(enemy);
		yield break;
	}
	
	public override void RemoveEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Desnublado!</color>");
		DeletePowerUp(enemy);
	}
}