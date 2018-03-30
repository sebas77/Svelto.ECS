using System;
using System.Collections.Generic;
using System.Reflection;
using Svelto.DataStructures;
using Svelto.Utilities;

namespace Svelto.ECS
{   
    //todo: can I remove the ID from the struct?
    
    public interface IEntityView
    {
        EGID ID { get; }
    }
    
    public interface IEntityStruct:IEntityView
    {
        new EGID ID { get; set; }
    }

    public class EntityView : IEntityView
    {
        public EGID ID { get { return _ID; } }

        internal FasterList<KeyValuePair<Type, CastedAction<EntityView>>> entityViewBlazingFastReflection;
        internal EGID _ID;
    }

    static class EntityView<T> where T: EntityView, new()
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

                    CastedAction<EntityView> setter = FastInvoke<T>.MakeSetter<EntityView>(field);
                    
                    FieldCache<T>.list.Add(new KeyValuePair<Type, CastedAction<EntityView>>(field.FieldType, setter));
                }
            }

            return new T { _ID = ID, entityViewBlazingFastReflection = FieldCache<T>.list };
        }

        //check if I can remove W
        static class FieldCache<W> where W:T
        {
            internal static readonly FasterList<KeyValuePair<Type, CastedAction<EntityView>>> list
                = new FasterList<KeyValuePair<Type, CastedAction<EntityView>>>();
        }
    }
}

