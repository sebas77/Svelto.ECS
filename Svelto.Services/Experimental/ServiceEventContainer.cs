using System;
using System.Collections.Generic;

namespace Svelto.ServiceLayer.Experimental
{
    public abstract class ServiceEventContainer : IServiceEventContainer
    {
        public void Dispose()
        {
            foreach (var listener in _listeners)
            {
                listener.Dispose();
            }

            _listeners.Clear();
        }

        protected ServiceEventContainer()
        {
            //call all the AddRelation in the implementation if you wish
        }

        public void ListenTo<TListener, TDelegate>(TDelegate callBack)
            where TListener : class, IServiceEventListener<TDelegate> where TDelegate : Delegate
        {
            var concreteType = _registeredTypes[typeof(TListener)];
            var listener = (TListener)Activator.CreateInstance(concreteType);
            listener.SetCallback(callBack);
            _listeners.Add(listener);
        }

        protected void AddRelation<TInterface, TConcrete>() where TInterface : IServiceEventListenerBase
            where TConcrete : TInterface
        {
            _registeredTypes.Add(typeof(TInterface), typeof(TConcrete));
        }

        readonly List<IServiceEventListenerBase> _listeners = new List<IServiceEventListenerBase>();
        readonly Dictionary<Type, Type> _registeredTypes = new Dictionary<Type, Type>();
    }
}