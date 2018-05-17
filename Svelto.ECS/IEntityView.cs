using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{   
    //todo: can I remove the ID from the struct?
    public interface IEntityData
    {
        EGID ID { get; set; }
    }
    
    public class EntityView : IEntityData
    {
        public EGID ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        EGID _ID;
    }

    public struct EntityInfoView : IEntityData
    {
        public EGID ID { get; set; }
        
        public IEntityViewBuilder[] entityViewsToBuild;
    }

    public static class EntityView<T> where T: IEntityData, new()
    {
        internal static void BuildEntityView(EGID ID, out T entityView) 
        {
            if (FieldCache.list == null)
            {
                FieldCache.list = new FasterList<KeyValuePair<Type, ActionRef<T>>>();
                
                var type = typeof(T);

                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Instance);
    
                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];

                    ActionRef<T> setter = FastInvoke<T>.MakeSetter(field);
                    
                    FieldCache.list.Add(new KeyValuePair<Type, ActionRef<T>>(field.FieldType, setter));
                }
            }

            entityView = new T { ID = ID };
        }

        public static class FieldCache
        {
            public static FasterList<KeyValuePair<Type, ActionRef<T>>> list;
        }
    }
}

