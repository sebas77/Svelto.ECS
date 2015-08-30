using UnityEngine;
using Svelto.Context;
using System.Collections;

public class UnityContext:MonoBehaviour
{}

public class UnityContext<T>: UnityContext where T:class, ICompositionRoot, IUnityContextHierarchyChangedListener, new()
{
	virtual protected void Awake()
	{
		Init();
	}

	void Init()
	{
		_applicationRoot = new T();
		
		MonoBehaviour[] behaviours = transform.GetComponentsInChildren<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            var component = behaviours[i];

            if (component != null)
                _applicationRoot.OnMonobehaviourAdded(component);
        }

        Transform[] children = transform.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];

            if (child != null)
                 _applicationRoot.OnGameObjectAdded(child.gameObject);
        }
    }
	
	void OnDestroy()
	{
		FrameworkDestroyed();
	}

	void Start()
	{
		if (Application.isPlaying == true)
			StartCoroutine(WaitForFrameworkInitialization());
	}

	IEnumerator WaitForFrameworkInitialization()
	{
		yield return new WaitForEndOfFrame(); //let's wait until next frame, so we are sure that all the awake and starts are called
		
		_applicationRoot.OnContextInitialized();
	}
	
	void FrameworkDestroyed()
	{
		_applicationRoot.OnContextDestroyed();
	}
	
	private T _applicationRoot = null;
}
