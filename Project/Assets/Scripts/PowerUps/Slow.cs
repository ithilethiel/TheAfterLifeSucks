using UnityEngine;
using System.Collections;

public class Slow : PowerUp {
	private const string CLOGTYPE = "SLOW";

	public Slow (){
		this.typeName = CLOGTYPE;
	}

	public override IEnumerator ApplyEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Lenteja por " + effectDuration +" segs</color>");
		enemy.stats.moveSpeed *= 0.4f;
		yield return new WaitForSeconds(effectDuration);
		RemoveEffectTo(enemy);
		yield break;
	}
	
	public override void RemoveEffectTo(Player enemy){
		Debug.Log("<color="+Utils.GetInverseColor(celestialAlignment)+">Rapido de nuevo!</color>");
		enemy.stats.moveSpeed *= 2.5f;
		DeletePowerUp(enemy);
	}
}
