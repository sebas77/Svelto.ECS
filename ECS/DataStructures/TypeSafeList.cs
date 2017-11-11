using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface ITypeSafeList
    {
        void Clear();
        void AddRange(ITypeSafeList nodeListValue);

        ITypeSafeList Create();
        ITypeSafeDictionary CreateIndexedDictionary();
        void AddToIndexedDictionary(ITypeSafeDictionary nodesDic);
    }

    public class TypeSafeFasterList<T> : FasterList<T>, ITypeSafeList
    {
        public TypeSafeFasterList()
        {
        }

        public void AddRange(ITypeSafeList nodeListValue)
        {
            AddRange(nodeListValue as FasterList<T>);
        }

        public ITypeSafeList Create()
        {
            return new TypeSafeFasterList<T>();
        }

        public ITypeSafeDictionary CreateIndexedDictionary()
        {
            return new TypeSafeDictionary<int, T>();
        }

        public void AddToIndexedDictionary(ITypeSafeDictionary nodesDic)
        {
            var dic = nodesDic as TypeSafeDictionary<int, T>;

            var buffer = NoVirt.ToArrayFast(this);

            for (int i = 0; i < Count; i++)
            {
                T node = buffer[i];

                dic[(node as NodeWithID).ID] = node;
            }
        }
    }
}
