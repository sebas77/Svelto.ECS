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
    
    static class EntityView<T> where T: IEntityData, new()
    {
        internal static T BuildEntityView(EGID ID) 
        {
            if (FieldCache<T>.list.Count == 0)
            {
                var type = typeof(T);

                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.Instance);

                for (int i = fields.Length - 1; i >= 0; --i)
                {
                    var field = fields[i];

                    CastedAction<T> setter = FastInvoke<T>.MakeSetter(field);
                    
                    FieldCache<T>.list.Add(new KeyValuePair<Type, CastedAction<T>>(field.FieldType, setter));
                }
            }

            return new T { ID = ID };
        }

        //check if I can remove W
        internal static class FieldCache<W>
        {
            internal static readonly FasterList<KeyValuePair<Type, CastedAction<T>>> list
                = new FasterList<KeyValuePair<Type, CastedAction<T>>>();
        }
    }
}

