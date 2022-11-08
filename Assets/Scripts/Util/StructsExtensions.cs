using UnityEngine;
using System.Collections;

public static class StructsExtensions {
	
	public static Color Set(this Color color, float? r = null, float? g = null, float? b = null, float? a = null){
		Color newColor = color;
		
		newColor.r = r ?? color.r;
		newColor.g = g ?? color.g;
		newColor.b = b ?? color.b;
		newColor.a = a ?? color.a;
		
		return newColor;
	}

	public static Vector3 Set3(this Vector3 vector, float? x = null, float? y = null, float? z = null){
		Vector3 newVector = vector;

		newVector.x = x ?? vector.x;
		newVector.y = y ?? vector.y;
		newVector.z = z ?? vector.z;

		return newVector;
	}

	public static Vector2 Set2(this Vector2 vector, float? x = null, float? y = null){
		Vector3 newVector = vector;
		
		newVector.x = x ?? vector.x;
		newVector.y = y ?? vector.y;
		
		return newVector;
	}

	public static Vector3 Clamp3(this Vector3 vector, float? minX = null, float? maxX = null, float? minY = null, float? maxY = null, float? minZ = null, float? maxZ = null) {
		Vector3 newVector = vector;

		newVector.x = Mathf.Max(minX ?? float.MinValue, newVector.x);
		newVector.x = Mathf.Min(maxX ?? float.MaxValue, newVector.x);

		newVector.y = Mathf.Max(minY ?? float.MinValue, newVector.y);
		newVector.y = Mathf.Min(maxY ?? float.MaxValue, newVector.y);

		newVector.z = Mathf.Max(minZ ?? float.MinValue, newVector.z);
		newVector.z = Mathf.Min(maxZ ?? float.MaxValue, newVector.z);

		return newVector;
	}

	public static Vector2 Clamp2(this Vector2 vector, float? minX = null, float? maxX = null, float? minY = null, float? maxY = null) {
		Vector2 newVector = vector;

		newVector.x = Mathf.Max(minX ?? float.MinValue, newVector.x);
		newVector.x = Mathf.Min(maxX ?? float.MaxValue, newVector.x);

		newVector.y = Mathf.Max(minY ?? float.MinValue, newVector.y);
		newVector.y = Mathf.Min(maxY ?? float.MaxValue, newVector.y);

		return newVector;
	}
}