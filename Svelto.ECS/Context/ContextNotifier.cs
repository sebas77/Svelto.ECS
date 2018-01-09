using System;
using System.Collections.Generic;
using WeakReferenceI = Svelto.DataStructures.WeakReference<Svelto.Context.IWaitForFrameworkInitialization>;
using WeakReferenceD = Svelto.DataStructures.WeakReference<Svelto.Context.IWaitForFrameworkDestruction>;

namespace Svelto.Context
{
    public class ContextNotifier : IContextNotifer
    {
        public ContextNotifier()
        {
            _toInitialize = new List<WeakReferenceI>();
            _toDeinitialize = new List<WeakReferenceD>();
        }

        public void AddFrameworkDestructionListener(IWaitForFrameworkDestruction obj)
        {
            if (_toDeinitialize != null)
                _toDeinitialize.Add(new WeakReferenceD(obj));
            else
                throw new Exception("An object is expected to be initialized after the framework has been deinitialized. Type: " + obj.GetType());
        }

        public void AddFrameworkInitializationListener(IWaitForFrameworkInitialization obj)
        {
            if (_toInitialize != null)
                _toInitialize.Add(new WeakReferenceI(obj));
            else
                throw new Exception("An object is expected to be initialized after the framework has been initialized. Type: " + obj.GetType());
        }

        /// <summary>
        /// A Context is meant to be deinitialized only once in its timelife
        /// </summary>
        public void NotifyFrameworkDeinitialized()
        {
            for (var i = _toDeinitialize.Count - 1; i >= 0; --i)
                try
                {
                    var obj = _toDeinitialize[i];
                    if (obj.IsAlive)
                        obj.Target.OnFrameworkDestroyed();
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }

            _toDeinitialize = null;
        }

        /// <summary>
        /// A Context is meant to be initialized only once in its timelife
        /// </summary>
        public void NotifyFrameworkInitialized()
        {
            for (var i = _toInitialize.Count - 1; i >= 0; --i)
                try
                {
                    var obj = _toInitialize[i];
                    if (obj.IsAlive)
                        obj.Target.OnFrameworkInitialized();
                }
                catch (Exception e)
                {
                    Utility.Console.LogException(e);
                }

            _toInitialize = null;
        }

        List<WeakReferenceD> _toDeinitialize;
        List<WeakReferenceI> _toInitialize;
    }
}
