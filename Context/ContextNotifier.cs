using Svelto.DataStructures;
using System;
using System.Collections.Generic;

namespace Svelto.Context
{
    class ContextNotifier : IContextNotifer
    {
        public ContextNotifier()
        {
            _toInitialize = new List<WeakReference<IWaitForFrameworkInitialization>>();
            _toDeinitialize = new List<WeakReference<IWaitForFrameworkDestruction>>();
        }

        public void AddFrameworkDestructionListener(IWaitForFrameworkDestruction obj)
        {
            if (_toDeinitialize != null)
                _toDeinitialize.Add(new WeakReference<IWaitForFrameworkDestruction>(obj));
            else
                throw new Exception("An object is expected to be initialized after the framework has been deinitialized. Type: " + obj.GetType());
        }

        public void AddFrameworkInitializationListener(IWaitForFrameworkInitialization obj)
        {
            if (_toInitialize != null)
                _toInitialize.Add(new WeakReference<IWaitForFrameworkInitialization>(obj));
            else
                throw new Exception("An object is expected to be initialized after the framework has been initialized. Type: " + obj.GetType());
        }

        /// <summary>
        /// A Context is meant to be deinitialized only once in its timelife
        /// </summary>
        public void NotifyFrameworkDeinitialized()
        {
            for (int i = _toDeinitialize.Count - 1; i >= 0; --i)
            {
                try
                {
                    var obj = _toDeinitialize[i];
                    if (obj.IsAlive == true)
                        (obj.Target as IWaitForFrameworkDestruction).OnFrameworkDestroyed();
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }
            }

            _toDeinitialize = null;
        }

        /// <summary>
        /// A Context is meant to be initialized only once in its timelife
        /// </summary>
        public void NotifyFrameworkInitialized()
        {
            for (int i = _toInitialize.Count - 1; i >= 0; --i)
            {
                try
                {
                    var obj = _toInitialize[i];
                    if (obj.IsAlive == true)
                        (obj.Target as IWaitForFrameworkInitialization).OnFrameworkInitialized();
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }
            }

            _toInitialize = null;
        }

        List<WeakReference<IWaitForFrameworkDestruction>> _toDeinitialize;
        List<WeakReference<IWaitForFrameworkInitialization>> _toInitialize;
    }
}
