using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Slot {
	public Stack<PowerUp> powerUps = new Stack<PowerUp>();
	public int quantity {
		get{return powerUps.Count;}
	}
	int maxAmountOfEach;
	public string typeName;
	public Sprite typeIcon;

	public bool inCoolDown = false;
	float coolDown;

	public Slot(PowerUp p, int maxAmountOfEach) {
		powerUps.Push(p);
		typeName = p.typeName;
		typeIcon = p.icon;
		coolDown = p.typeCoolDown;
		this.maxAmountOfEach = maxAmountOfEach;
	}

	public void Add(PowerUp p){
		if(quantity < maxAmountOfEach){
			powerUps.Push(p);
		}else{
			p.DeletePowerUp();
		}
	}

	public PowerUp GetAndRemoveLast(){
		if(quantity > 0){
			return powerUps.Pop();
		}
		return null;
	}
}

public class Inventory {

	public GUIImages images;
	public int maxAmountOfEach {get; private set;}

	public Inventory(GUIImages g, int maxAmountOfEach = 3){
		images = g;
		this.maxAmountOfEach = maxAmountOfEach;
		// FIXME quizas el maxAmountOfEach debe ir en los powerUps???
	}

	Dictionary<string, int> indexByType = new Dictionary<string, int>();
	List<Slot> slots = new List<Slot>();
	int currentIndex = -1;

	public void Add(PowerUp powerUp){
		if(indexByType.ContainsKey(powerUp.typeName)){
			// si es de un tipo repetido -> lo sumo al slot que ya existia
			slots[indexByType[powerUp.typeName]].Add(powerUp);
		}else{
			// si es de un tipo que no tenia -> nuevo slot
			currentIndex++;
			indexByType.Add(powerUp.typeName, currentIndex);
			slots.Insert(currentIndex, new Slot(powerUp, maxAmountOfEach));
		}
		UpdateDisplay();
	}

	public string GetSelectedType(){
		return slots[currentIndex].typeName;
	}

	public PowerUp GetAndRemoveSelected(){
		if(slots.Count == 0)
			return null;

		PowerUp p = slots[currentIndex].GetAndRemoveLast();

		if(slots[currentIndex].quantity == 0){
			// Quedaba uno solo y ahora hay 0 -> borrar el tipo del inventario
			indexByType.Remove(slots[currentIndex].typeName);
			slots[currentIndex] = null;
			slots.RemoveAt(currentIndex); //queda limpio lo de adentro???
			currentIndex = IndexAfterRemovingSlot();
		}

		UpdateDisplay();
		return p;
	}

	public void SelectPrev(){
		Debug.Log("Pre");
		if(slots.Count == 0)
			return;
		if(currentIndex +1 >= slots.Count){
			currentIndex = 0;
		}else{
			currentIndex++;
		}
		UpdateDisplay();
	}

	public void SelectNext(){
		Debug.Log("Next");
		if(slots.Count == 0)
			return;
		if(currentIndex == 1){
			currentIndex = 0;
		}else if(currentIndex == 0){
			currentIndex =  slots.Count-1;
		}else{
			currentIndex--;
		}
		UpdateDisplay();
	}

	int IndexAfterRemovingSlot(){
		if(slots.Count ==  1 || currentIndex == 1){
			return 0;
		}else if(currentIndex == 0){
			return slots.Count-1;
		}else{
			return currentIndex - 1;
		}
	}

	int IndexToLeft(){
		if(slots.Count <=  1)
			return -1;
		if(currentIndex == 0){
			return  slots.Count-1;
		}else if(currentIndex == 1){
			return 0;
		}else{
			return currentIndex - 1;
		}
	}

	int IndexToRight(){
		if(slots.Count <= 2)
			return -1;
		if(currentIndex +1 < slots.Count){
			return currentIndex + 1;
		}else{
			return 0;
		}
	}

	void UpdateDisplay(){
		images.previous.sprite = (IndexToLeft() == -1) ? null : slots[IndexToLeft()].typeIcon;
		images.current.sprite = (currentIndex == -1) ? null : slots[currentIndex].typeIcon;
		images.next.sprite = (IndexToRight() == -1) ? null : slots[IndexToRight()].typeIcon;
	}
}

[System.Serializable]
public class GUIImages {
	public Image previous, current, next;
}
