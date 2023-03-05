using System.Runtime.InteropServices;

namespace Svelto.ECS.Hybrid
{
    /// <summary>
    /// ValueReference is the only way to store a reference inside an Implementor. To stop any abuse
    /// the reference must be an implementor and converted back to an implementor.
    /// The OOP abstraction layer that knows about the implementor than can cast it to the real type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ValueReference<T> : IValueReferenceInternal where T:class
    {
        public ValueReference(T obj) { _pointer = GCHandle.Alloc(obj, GCHandleType.Normal); }

        public T ConvertAndDispose<W>(W implementer) where W:IImplementor 
        {
            var pointerTarget = _pointer.Target;
            _pointer.Free();
            return (T)pointerTarget;
        }

        public bool isDefault => _pointer.IsAllocated == false;
        
        GCHandle    _pointer;
    }

    // Used to validate the use of this struct on the component builder check fields.
    interface IValueReferenceInternal {}
}