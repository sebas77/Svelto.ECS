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
        public static Action<CastedType, object> MakeSetter<CastedType>(FieldInfo field) 
        {
            if (field.FieldType.IsInterface == true && field.FieldType.IsValueType == false)
            {
                return new Action<CastedType, object>((target, value) => field.SetValue(target, value));
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported EntityView field (must be an interface and a class)");
        }
#elif !NETFX_CORE
        public static Action<CastedType, object> MakeSetter<CastedType>(FieldInfo field) 
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

                return new Action<CastedType, object>((target, value) => m.CreateDelegate(typeof(Action<T, object>)).DynamicInvoke(target, value));
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported EntityView field (must be an interface and a class)");
        }
#else
        public static Action<CastedType, object> MakeSetter<CastedType>(FieldInfo field) 
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                ParameterExpression targetExp = Expression.Parameter(typeof(T), "target");
                ParameterExpression valueExp = Expression.Parameter(field.FieldType, "value");

                MemberExpression fieldExp = Expression.Field(targetExp, field);
                BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

                var setter = Expression.Lambda(assignExp, targetExp, valueExp).Compile();

                return new Action<CastedType, object>((target, value) => setter.DynamicInvoke(target, value));
            }

            throw new ArgumentException("<color=orange>Svelto.ECS</color> unsupported EntityView field (must be an interface and a class)");
        }
#endif
    }
}