using UnityEngine;
using System.Collections;

public class Clog : PowerUp {
	private const string CLOGTYPE = "CLOG";

	public Clog (){
		this.typeName = CLOGTYPE;
	}

	public override IEnumerator ApplyEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Tapado por " + effectDuration +" segs</color>");
		enemy.stats.canSuck = false;
		yield return new WaitForSeconds(effectDuration);
		RemoveEffectTo(enemy);
		yield break;
	}
	
	public override void RemoveEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Se destapo!</color>");
		enemy.stats.canSuck = true;
		DeletePowerUp(enemy);
	}
}
