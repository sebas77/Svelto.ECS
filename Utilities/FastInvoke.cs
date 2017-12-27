using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Svelto.Utilities
{
    //https://stackoverflow.com/questions/321650/how-do-i-set-a-field-value-in-an-c-sharp-expression-tree/321686#321686

    public static class FastInvoke<T> where T : class
    {
#if ENABLE_IL2CPP
        public static CastedAction<CastedType> MakeSetter<CastedType>(FieldInfo field) where CastedType:class
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                return new CastedAction<CastedType, T>(field.SetValue);
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
#elif !NETFX_CORE
        public static CastedAction<CastedType> MakeSetter<CastedType>(FieldInfo field) where CastedType:class
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                DynamicMethod m = new DynamicMethod("setter", typeof(void), new Type[] { typeof(T), typeof(object) });
                ILGenerator cg = m.GetILGenerator();

                // arg0.<field> = arg1
                cg.Emit(OpCodes.Ldarg_0);
                cg.Emit(OpCodes.Ldarg_1);
                cg.Emit(OpCodes.Stfld, field);
                cg.Emit(OpCodes.Ret);

                var del = m.CreateDelegate(typeof(Action<T, object>));

                return new CastedAction<CastedType, T>(del);
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
#else
        public static CastedAction<CastedType> MakeSetter<CastedType>(FieldInfo field) where CastedType:class
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                ParameterExpression targetExp = Expression.Parameter(typeof(T), "target");
                ParameterExpression valueExp = Expression.Parameter(typeof(object), "value");

                MemberExpression fieldExp = Expression.Field(targetExp, field);
                UnaryExpression convertedExp = Expression.TypeAs(valueExp, field.FieldType);
                BinaryExpression assignExp = Expression.Assign(fieldExp, convertedExp);

                Type type = typeof(Action<,>).MakeGenericType(new Type[] { typeof(T), typeof(object) });

                var setter = Expression.Lambda(type, assignExp, targetExp, valueExp).Compile();

                return new CastedAction<CastedType, T>(setter); 
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
#endif
    }

    public abstract class CastedAction<W> 
    {
        abstract public void Call(W target, object value);
    }

    public class CastedAction<W, T> : CastedAction<W> where W : class where T:class
    {
        Action<T, object> setter;

        public CastedAction(Delegate setter)
        {
            this.setter = (Action<T, object>)setter;
        }

        public CastedAction(Action<T, object> setter)
        {
            this.setter = setter;
        }

        override public void Call(W target, object value)
        {
            setter(target as T, value);
        }
    }
}