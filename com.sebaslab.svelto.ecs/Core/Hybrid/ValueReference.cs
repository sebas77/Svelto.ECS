using System;
using System.Runtime.InteropServices;

namespace Svelto.ECS.Hybrid
{
    /// <summary>
    /// an ECS dirty trick to hold the reference to an object. this is component can be used in an engine
    /// managing an OOP abstraction layer. It's need is quite rare though! An example is found at
    /// https://github.com/sebas77/Svelto.MiniExamples/blob/master/Example6-Unity%20Hybrid-OOP%20Abstraction/Assets/Code/WithEntityViewComponent/Descriptors/TransformImplementor.cs
    /// All other uses must be considered an abuse
    /// as the object can be casted back to it's real type only by an OOP Abstraction Layer Engine:
    /// https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/ 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ValueReference<T> : IDisposable where T:class, IImplementor
    {
        static ValueReference()
        {
            DBC.ECS.Check.Require(typeof(T).IsInterface == true, "ValueReference type can be only pure interface implementing IImplementor");
        }

        public ValueReference(T obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public W Convert<W>(W implementer) where W:T
        {
            var pointerTarget = _pointer.Target;
            return (W)pointerTarget;
        }

        public void Dispose()
        {
            _pointer.Free();
        }
        
        GCHandle    _pointer;
    }
}