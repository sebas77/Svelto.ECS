using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    static class ComponentTypeMap
    {
        static readonly ConcurrentDictionary<Type, ComponentID> _componentTypeMap = new ConcurrentDictionary<Type, ComponentID>();
        static readonly ConcurrentDictionary<ComponentID, Type> _reverseComponentTypeMap = new ConcurrentDictionary<ComponentID, Type>();

        public static void Add(Type type, ComponentID idData)
        {
            _componentTypeMap.TryAdd(type, idData);
            _reverseComponentTypeMap.TryAdd(idData, type);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ComponentID FetchID(Type type)
        {
            if (_componentTypeMap.TryGetValue(type, out var index) == false)
            {
                //if warming up is working correctly, this should never happen
                  var componentType = typeof(ComponentTypeID<>).MakeGenericType(type);
                 System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(componentType.TypeHandle);
                 return _componentTypeMap[type];
            }
            
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type FetchType(ComponentID id)
        {
            return _reverseComponentTypeMap[id];
        }
    }
}