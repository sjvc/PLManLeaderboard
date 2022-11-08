using UnityEngine;
using System.Collections.Generic;
using System;

/**
 * Esta clase es un ObjectPool para añdir objetos de cualquier tipo, instanciados a partir de un prefab.
 **/

namespace Baviux {

public class PrefabPool : MonoBehaviour {
	[HideInInspector][NonSerialized]
	public static PrefabPool instance;

	[System.Serializable]
	public class StartupPool{
		public int size;
		public GameObject prefab;
	}

	public StartupPool[] startupPools;

	protected Dictionary<GameObject, Stack<GameObject>> pooledObjects; // Key = Prefab, Value = Prefab Instances Stack
	protected Dictionary<GameObject, GameObject> retrievedObjects; // Key = Prefab Instance, Value = Prefab
	protected Dictionary<GameObject, List<GameObject>> allObjects;  // Key = Prefab, Value = Prefab Instances List

	public PrefabPool(){
		pooledObjects = new Dictionary<GameObject, Stack<GameObject>>();
		retrievedObjects = new Dictionary<GameObject, GameObject>();
		allObjects = new Dictionary<GameObject, List<GameObject>>();
	}

	void Awake(){
		instance = this;

		// Create startup pools
		if (startupPools != null){
			for (int s=0, size=startupPools.Length; s<size; s++){
				StartupPool startupPool = startupPools[s];
				for (int i=0; i<startupPool.size; i++){
					GameObject gObject = InstantiatePrefab(startupPool.prefab);
					gObject.SendMessage<IPrefabPoolItem>(f => f.OnInstantiatePoolItem());
					
					AddToPool(gObject, startupPool.prefab);
				}
			}
		}
	}

	private GameObject InstantiatePrefab(GameObject prefab, Transform parent = null) {
		GameObject gObject = (GameObject)Instantiate(prefab, parent ?? transform);

		if (!allObjects.ContainsKey(prefab)) {
			allObjects[prefab] = new List<GameObject>();
		}

		allObjects[prefab].Add(gObject);

		return gObject;
	}

	public GameObject Retrieve(GameObject prefab, Vector3 position, Quaternion? rotation = null, Vector3? localScale = null, Transform parent = null, bool instantiateInWorldSpace = true) {
		GameObject gObject = null;

		if (pooledObjects.ContainsKey(prefab) && pooledObjects[prefab].Count > 0){
			gObject = pooledObjects[prefab].Pop();
			
			if (instantiateInWorldSpace) {
				gObject.transform.position = position;
				gObject.transform.rotation = rotation ?? prefab.transform.localRotation; // Si cogemos la rotación del prefab, cogemos su rotación local
				gObject.transform.localScale = localScale ?? prefab.transform.localScale; // Aunque instanciemos en world space, la escala es relativa al padre
			}

			SetGameObjectActiveAndSetParent(gObject, true, parent, instantiateInWorldSpace); // worldPositionStays debe ser true si lo es instantiateInWorldSpace para que permanezca en el mismo sitio en el mundo después de establecer el padre

			if (!instantiateInWorldSpace) {
				gObject.transform.localPosition = position;
				gObject.transform.localRotation = rotation ?? prefab.transform.localRotation;
				gObject.transform.localScale = localScale ?? prefab.transform.localScale;
			}
		}
		else{
			gObject = InstantiatePrefab(prefab, parent);

			if (instantiateInWorldSpace) {
				gObject.transform.position = position;
				if (rotation.HasValue) gObject.transform.rotation = rotation.Value;
				if (localScale.HasValue) gObject.transform.localScale = localScale.Value;
			} else {
				gObject.transform.localPosition = position;
				if (rotation.HasValue) gObject.transform.localRotation = rotation.Value;
				if (localScale.HasValue) gObject.transform.localScale = localScale.Value;
			}

			gObject.SendMessage<IPrefabPoolItem>(f => f.OnInstantiatePoolItem());
		}

		retrievedObjects[gObject] = prefab;

		gObject.SendMessage<IPrefabPoolItem>(f => f.OnRetrievePoolItem());

		return gObject;
	}

	public void Recycle(GameObject gObject){
		GameObject prefab;
	
		if (retrievedObjects.TryGetValue(gObject, out prefab)){			
			AddToPool(gObject, prefab);

			retrievedObjects.Remove(gObject);
		}
		else {
			Destroy (gObject);
		}
	}

	public void RemovePrefabAndDestroyInstances(GameObject prefab) {
		if (allObjects.ContainsKey(prefab)) {
			List<GameObject> instances = allObjects[prefab];

			for (int i=0; i < instances.Count; i++) {
				retrievedObjects.Remove(instances[i]);
				Destroy(instances[i]);
			}

			if (pooledObjects.ContainsKey(prefab)) {
				pooledObjects.Remove(prefab);
			}

			allObjects.Remove(prefab);
		}
	}

	protected void AddToPool(GameObject gObject, GameObject prefab){
		gObject.SendMessage<IPrefabPoolItem>(f => f.OnRecyclePoolItem());
		
		SetGameObjectActiveAndSetParent(gObject, false, transform, false); // false para worldPositionStays para que no modifique ninguna propiedad del transform (no es necesario)

		if (!pooledObjects.ContainsKey(prefab)){
			pooledObjects[prefab] = new Stack<GameObject>();
		}
		pooledObjects[prefab].Push(gObject);
	}

	protected void SetGameObjectActiveAndSetParent(GameObject gObject, bool active, Transform parent, bool worldPositionStays) {
		gObject.transform.SetParent(parent, worldPositionStays);
		gObject.SetActive(active);
	}
}

public interface IPrefabPoolItem : IMessageHandler{
	void OnInstantiatePoolItem(); // Executed when a new instance is created
	void OnRetrievePoolItem();    // Executed when a instance is retrieved from the pool
	void OnRecyclePoolItem();	  // Executed when a instance is returned to the pool
}

}
