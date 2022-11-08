using UnityEngine;
using System.Collections.Generic;

namespace Baviux {

// Inspired by https://bitbucket.org/Unity-Technologies/ui/src/0155c39e05ca5d7dcc97d9974256ef83bc122586/UnityEngine.UI/EventSystem/ExecuteEvents.cs?at=5.2&fileviewer=file-view-default
public static class MessageSystem {
	public delegate void MessageFunction<T>(T handler);
	public delegate R MessageFunctionWithReturnValue<T,R>(T handler);

	private static ObjectPool<List<Component>> componentListPool = new ObjectPool<List<Component>>(null, list => list.Clear());
	private static ObjectPool<List<IMessageHandler>> handlerListPool = new ObjectPool<List<IMessageHandler>>(null, list => list.Clear());
	private static ObjectPool<List<Transform>> transformListPool = new ObjectPool<List<Transform>>(null, list => list.Clear());

	public static bool SendMessage<T>(this GameObject go, MessageFunction<T> functor, bool includeChildren = false, bool includeDisabled = false) where T : IMessageHandler{
		List<IMessageHandler> handlers = handlerListPool.Get();

		go.GetMessageHandlers<T>(handlers, includeChildren, includeDisabled);
		int handlerCount = handlers.Count;
		for(int i=0; i<handlerCount; i++){
			functor((T)handlers[i]);
		}

		handlerListPool.Release(handlers);

		return handlerCount > 0;
	}

	public static bool SendMessage<T, R>(this GameObject go, MessageFunctionWithReturnValue<T, R> functor, List<R> results) where T : IMessageHandler{
		results.Clear();

		List<IMessageHandler> handlers = handlerListPool.Get();

		go.GetMessageHandlers<T>(handlers);
		int handlerCount = handlers.Count;
		for(int i=0; i<handlerCount; i++){
			R result = functor((T)handlers[i]);
			results.Add(result);
		}

		handlerListPool.Release(handlers);

		return handlerCount > 0;
	}

	public static R SendMessageToFirst<T, R>(this GameObject go, MessageFunctionWithReturnValue<T, R> functor) where T: IMessageHandler{
		R result;
		
		IMessageHandler handler = GetFirstMessageHandler<T>(go);
		
		if (handler != null){
			result = functor((T)handler);
		}
		else{
			result = default(R);
		}

		return result;
	}

	public static bool SendMessageUpwards<T>(this GameObject root, MessageFunction<T> functor) where T : IMessageHandler{
		bool result = false;

		List<Transform> transformList = transformListPool.Get();

		root.GetTransformChain(transformList);

		for (int i=0, size=transformList.Count; i<size; i++){
			result |= transformList[i].gameObject.SendMessage<T>(functor);
		}

		transformListPool.Release(transformList);

		return result;
	}

	public static void GetMessageHandlers<T>(this GameObject go, List<IMessageHandler> results, bool includeChildren = false, bool includeDisabled = false) where T : IMessageHandler {
		if (!go.activeInHierarchy)
			return;

		results.Clear();

		List<Component> componentList = componentListPool.Get();

		if (includeChildren){
			go.GetComponentsInChildren(componentList);
		}
		else{
			go.GetComponents(componentList);
		}

		for (int i=0, size=componentList.Count; i<size; i++){
			if (ShouldSendToComponent<T>(componentList[i], includeDisabled)){
				results.Add(componentList[i] as IMessageHandler);
			}
		}

		componentListPool.Release(componentList);
	}

	public static T GetFirstMessageHandler<T>(this GameObject go) where T : IMessageHandler {
		if (!go.activeInHierarchy)
			return default(T);

		IMessageHandler handler = null;

		List<Component> componentList = componentListPool.Get();

		go.GetComponents(componentList);
		for (int i=0, size=componentList.Count; i<size; i++){
			if (ShouldSendToComponent<T>(componentList[i])){
				handler = componentList[i] as IMessageHandler;
				break;
			}
		}

		componentListPool.Release(componentList);

		return (T)handler;
	}

	public static void GetTransformChain(this GameObject root, List<Transform> transformChain){
		transformChain.Clear();

		Transform t = root.transform;
		while(t != null){
			transformChain.Add(t);
			t = t.parent;
		}
	}

	private static bool ShouldSendToComponent<T>(Component component, bool includeDisabled = false) where T : IMessageHandler{
		var valid = component is T;
		if (!valid){
			return false;
		}

		if (!includeDisabled) {
			var behaviour = component as Behaviour;
			if (behaviour != null){
				// Logger.Write(component.name + " " + typeof(T) + " " + behaviour.GetType() + " " + behaviour.isActiveAndEnabled);
				return behaviour.isActiveAndEnabled;
			}
		}

		return true;
	}

}

}