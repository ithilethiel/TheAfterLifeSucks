using UnityEngine;
using System.Collections;
//
//public enum Bounds {
//	TOP_RIGHT,
//	TOP_MIDDLE,
//	BOTTOM_LEFT,
//	BOTTOM_MIDDLE
//}
//
public static class Utils {
	public static string GetColor(CelestialAlignment a){
		if(a == CelestialAlignment.HEAVEN)
			return "blue";
		else
			return "red";
	}
	public static string GetInverseColor(CelestialAlignment a){
		if(a == CelestialAlignment.HEAVEN)
			return "red";
		else
			return "blue";
	}
//	public static Vector2 CalculateBounds(Bounds bounds, Camera cam = null){
//		if(cam == null)
//			cam = Camera.main;
//		switch(bounds){
//		case Bounds.BOTTOM_LEFT:
//			return cam.ViewportToWorldPoint(new Vector3(0, 0f, 0f));
//			break;
//		case Bounds.BOTTOM_MIDDLE:
//			cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, 0f));
//			break;
//		case Bounds.TOP_MIDDLE:
//			cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));
//			break;
//		case Bounds.TOP_RIGHT:
//			cam.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
//			break;
//		}
//		return Vector2.zero;
//	}
}