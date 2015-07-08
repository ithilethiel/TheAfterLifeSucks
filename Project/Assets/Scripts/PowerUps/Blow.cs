using UnityEngine;
using System.Collections;

public class Blow : PowerUp {
	private const string CLOGTYPE = "BLOW";

	public Blow (){
		this.typeName = CLOGTYPE;
	}

	public override IEnumerator ApplyEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Sopla por " + effectDuration +" segs</color>");
		enemy.stats.suckingPower *= -1f;
		effectWasApplied = true;
		yield return new WaitForSeconds(effectDuration);
		RemoveEffectTo(enemy);
		yield break;
	}

	public override void RemoveEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Dejo de soplar!</color>");
		if(effectWasApplied)
			enemy.stats.suckingPower *= -1f;
		effectWasApplied = false;
		DeletePowerUp(enemy);
	}
}
