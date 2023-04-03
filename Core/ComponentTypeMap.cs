using System;
using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class ComponentTypeMap
    {
        static readonly FasterDictionary<RefWrapper<Type>, ComponentID> _componentTypeMap = new FasterDictionary<RefWrapper<Type>, ComponentID>();
        static readonly FasterDictionary<ComponentID, Type> _reverseComponentTypeMap = new FasterDictionary<ComponentID, Type>();

        public static void Add(Type type, ComponentID idData)
        {
            _componentTypeMap.Add(type, idData);
            _reverseComponentTypeMap.Add(idData, type);
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