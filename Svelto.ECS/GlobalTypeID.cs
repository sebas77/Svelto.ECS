#if UNITY_ECS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Unity.Burst;

namespace Svelto.ECS
{
    public class GlobalTypeID
    {
        internal static uint NextID<T>()
        {
            return (uint) (Interlocked.Increment(ref value) - 1);
        }

        static GlobalTypeID()
        {
            value = 0;
        }

        static int value;
    }
    
    static class EntityComponentID<T>
    {
        internal static readonly SharedStatic<uint> ID = SharedStatic<uint>.GetOrCreate<GlobalTypeID, T>();
    }
    
    interface IFiller 
    {
        void FillFromByteArray(EntityComponentInitializer init, NativeBag buffer);
    }
    
    static class UnmanagedTypeExtensions
    {
        private static Dictionary<Type, bool> cachedTypes =
            new Dictionary<Type, bool>();

        public static bool IsUnManaged<T>() { return typeof(T).IsUnManaged(); }

        public static bool IsUnManaged(this Type t)
        {
            var result = false;
            
            if (cachedTypes.ContainsKey(t))
                return cachedTypes[t];
            else if (t.IsPrimitive || t.IsPointer || t.IsEnum)
                    result = true;
                else if (t.IsGenericType || !t.IsValueType)
                        result = false;
                    else
                        result = t.GetFields(BindingFlags.Public | 
                                             BindingFlags.NonPublic | BindingFlags.Instance)
                                  .All(x => x.FieldType.IsUnManaged());
            cachedTypes.Add(t, result);
            return result;
        }
    }
    
    delegate void ForceUnmanagedCast<T>(EntityComponentInitializer init, NativeBag buffer) where T : struct, IEntityComponent;

    class Filler<T>: IFiller where T : struct, IEntityComponent
    {
        static readonly ForceUnmanagedCast<T> _action;

        static Filler()
        {
            var method = typeof(Trick).GetMethod(nameof(Trick.ForceUnmanaged)).MakeGenericMethod(typeof(T));
            _action = (ForceUnmanagedCast<T>) Delegate.CreateDelegate(typeof(ForceUnmanagedCast<T>), method);
        }
        
        //it's an internal interface
        void IFiller.FillFromByteArray(EntityComponentInitializer init, NativeBag buffer)
        {
            DBC.ECS.Check.Require(UnmanagedTypeExtensions.IsUnManaged<T>() == true, "invalid type used");

            _action(init, buffer);
        }
        
        static class Trick
        {    
            public static void ForceUnmanaged<U>(EntityComponentInitializer init, NativeBag buffer) where U : unmanaged, IEntityComponent
            {
                var component = buffer.Dequeue<U>();

                init.Init(component);
            }
        }
    }

    static class EntityComponentIDMap
    {
        static readonly FasterList<IFiller> TYPE_IDS = new FasterList<IFiller>();

        internal static void Register<T>(IFiller entityBuilder) where T : struct, IEntityComponent
        {
            var location = EntityComponentID<T>.ID.Data = GlobalTypeID.NextID<T>();
            TYPE_IDS.AddAt(location, entityBuilder);
        }
        
        internal static IFiller GetTypeFromID(uint typeId)
        {
            return TYPE_IDS[typeId];
        }
    }
}
#endif