using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{
    static class EntityView<T> where T: IEntityStruct, new()
    {
        internal static readonly FasterList<KeyValuePair<Type, ActionCast<T>>> cachedFields;

        static EntityView()
        {
            cachedFields = new FasterList<KeyValuePair<Type, ActionCast<T>>>();
                
            var type = typeof(T);

            var fields = type.GetFields(BindingFlags.Public |
                                        BindingFlags.Instance);
    
            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var field = fields[i];

                ActionCast<T> setter = FastInvoke<T>.MakeSetter(field);
                    
                cachedFields.Add(new KeyValuePair<Type, ActionCast<T>>(field.FieldType, setter));
            }
        }

        internal static void InitCache()
        {}
        
        internal static void BuildEntityView(EGID ID, out T entityView) 
        {
            entityView = new T { ID = ID };
        }
    }
}