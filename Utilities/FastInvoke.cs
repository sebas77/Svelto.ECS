using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Svelto.Utilities
{
    //https://stackoverflow.com/questions/321650/how-do-i-set-a-field-value-in-an-c-sharp-expression-tree/321686#321686
    public static class FastInvoke<T> where T : class
    {
        public static Action<T, object> MakeSetter(FieldInfo field) 
        {
            DynamicMethod m = new DynamicMethod("setter", typeof(void), new Type[] {typeof(T), typeof(object)});
            ILGenerator cg = m.GetILGenerator();

            // arg0.<field> = arg1
            cg.Emit(OpCodes.Ldarg_0);
            cg.Emit(OpCodes.Ldarg_1);
            cg.Emit(OpCodes.Stfld, field);
            cg.Emit(OpCodes.Ret);

            return (Action<T, object>) m.CreateDelegate(typeof(Action<T, object>));;
        }
    }
}