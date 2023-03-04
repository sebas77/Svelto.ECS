#if SLOW_SVELTO_SUBMISSION
namespace Svelto.ECS.Internal
{
    delegate void SetEGIDWithoutBoxingActionCast<T>(ref T target, EGID egid) where T : struct, _IInternalEntityComponent;
    delegate void SetReferenceWithoutBoxingActionCast<T>(ref T target, EntityReference egid) where T : struct, _IInternalEntityComponent;

    static class SetEGIDWithoutBoxing<T> where T : struct, _IInternalEntityComponent
    {
        public static readonly SetEGIDWithoutBoxingActionCast<T>      SetIDWithoutBoxing  = MakeSetter();
        public static readonly SetReferenceWithoutBoxingActionCast<T> SetRefWithoutBoxing = MakeSetterReference();

        public static void Warmup() { }

        static SetEGIDWithoutBoxingActionCast<T> MakeSetter()
        {
            if (ComponentBuilder<T>.HAS_EGID)
            {
#if !ENABLE_IL2CPP                
                var method = typeof(Trick).GetMethod(nameof(Trick.SetEGIDImpl)).MakeGenericMethod(typeof(T));
                return (SetEGIDWithoutBoxingActionCast<T>) System.Delegate.CreateDelegate(
                    typeof(SetEGIDWithoutBoxingActionCast<T>), method);
#else
             return (ref T target, EGID egid) =>
             {
                 var needEgid = (target as INeedEGID);
                 needEgid.ID = egid;
                 target      = (T) needEgid;
             };
#endif
            }

            return null;
        }
        
        static SetReferenceWithoutBoxingActionCast<T> MakeSetterReference()
        {
            if (ComponentBuilder<T>.HAS_REFERENCE)
            {
#if !ENABLE_IL2CPP                
                var method = typeof(Trick).GetMethod(nameof(Trick.SetEGIDImplRef)).MakeGenericMethod(typeof(T));
                return (SetReferenceWithoutBoxingActionCast<T>) System.Delegate.CreateDelegate(
                    typeof(SetReferenceWithoutBoxingActionCast<T>), method);
#else
             return (ref T target, EntityReference reference) =>
             {
                 var needEgid = (target as INeedEntityReference);
                 needEgid.selfReference = reference;
                 target                   = (T) needEgid;
             };
#endif
            }

            return null;
        }
        
        static class Trick
        {
            public static void SetEGIDImpl<U>(ref U target, EGID egid) where U : struct, INeedEGID
            {
                target.ID = egid;
            }
            
            public static void SetEGIDImplRef<U>(ref U target, EntityReference reference) where U : struct, INeedEntityReference
            {
                target.selfReference = reference;
            }
        }
    }
}
#endif