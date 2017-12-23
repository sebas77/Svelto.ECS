using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{   
    public interface IEntityView
    {
        int ID { get; }
    }
    
    public interface IEntityStruct:IEntityView
    {
        new int ID { set; }
    }

    public abstract class EntityView : IEntityView
    {
        public int ID { get { return _ID; } }

        abstract internal KeyValuePair<Type, Action<EntityView, object>>[]
            EntityViewBlazingFastReflection(out int count);

        protected int _ID;
    }

    public class EntityView<T>: EntityView where T: EntityView
    {
        internal static TEntityViewType BuildEntityView<TEntityViewType>(int ID) where TEntityViewType: EntityView<T>, new() 
        {
            if (FieldCache.list.Count == 0)
            {
                var type = typeof(TEntityViewType);

                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Instance);

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];

                    Action<EntityView, object> setter = FastInvoke<EntityView>.MakeSetter(field);

                    FieldCache.Add(new KeyValuePair<Type, Action<EntityView, object>>(field.FieldType, setter));
                }
            }

            return new TEntityViewType { _ID = ID };
        }

        override internal KeyValuePair<Type, Action<EntityView, object>>[] 
            EntityViewBlazingFastReflection(out int count)
        {
            return FasterList<KeyValuePair<Type, Action<EntityView, object>>>.NoVirt.ToArrayFast(FieldCache.list, out count);
        }

        static class FieldCache
        {
            internal static void Add(KeyValuePair<Type, Action<EntityView, object>> setter)
            {
                list.Add(setter);
            }
            
            internal static readonly FasterList<KeyValuePair<Type, Action<EntityView, object>>> list = new FasterList<KeyValuePair<Type, Action<EntityView, object>>>();
        }
    }
}

