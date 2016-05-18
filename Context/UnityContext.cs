using System.Collections;
using Svelto.Context;
using UnityEngine;

public abstract class UnityContext:MonoBehaviour
{
    protected abstract void OnAwake();

    void Awake()
    {
        OnAwake();
    }
}

public class UnityContext<T>: UnityContext where T:class, ICompositionRoot, new()
{
    protected override void OnAwake()
    {
        _applicationRoot = new T();

        _applicationRoot.OnContextCreated(this);
    }

    void OnDestroy()
    {
        _applicationRoot.OnContextDestroyed();
    }

    void Start()
    {
        if (Application.isPlaying == true)
            StartCoroutine(WaitForFrameworkInitialization());
    }

    IEnumerator WaitForFrameworkInitialization()
    {
        //let's wait until the end of the frame, so we are sure that all the awake and starts are called
        yield return new WaitForEndOfFrame();

        _applicationRoot.OnContextInitialized();
    }

    T _applicationRoot;
}
