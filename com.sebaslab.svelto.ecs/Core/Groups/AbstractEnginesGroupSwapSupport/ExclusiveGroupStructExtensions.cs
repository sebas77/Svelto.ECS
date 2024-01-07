using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public static class ExclusiveGroupStructExtensions
    {
        public static ExclusiveBuildGroup RemoveTag<T>(this ExclusiveGroupStruct group) where T : GroupTag<T>
        {
            if (ExclusiveBuildGroupExtensions._removeTransitions.TryGetValue(@group, out var transitions))
            {
                var type = new RefWrapper<Type>(typeof(T));
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No remove transition found for type "
                .FastConcat(typeof(T).ToString())
                .FastConcat(" in group ").FastConcat(@group.ToString())
            );
        }

        public static ExclusiveBuildGroup AddTag<T>(this ExclusiveGroupStruct group) where T : GroupTag<T>
        {
            if (ExclusiveBuildGroupExtensions._addTransitions.TryGetValue(group, out var transitions))
            {
                var type = new RefWrapper<Type>(typeof(T));
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No add transition found for type "
                .FastConcat(typeof(T).ToString())
                .FastConcat(" in group ").FastConcat(@group.ToString())
            );
        }

        public static ExclusiveBuildGroup SwapTag<TTarget>(this ExclusiveGroupStruct group)
            where TTarget : GroupTag<TTarget>
        {
            var type =  new RefWrapper<Type>(typeof(TTarget));
            if (ExclusiveBuildGroupExtensions._swapTransitions.TryGetValue(@group, out var transitions))
            {
                if (transitions.TryGetValue(type, out var result))
                {
                    return result;
                }
            }

            throw new ECSException("No swap transition found for type "
                .FastConcat(typeof(TTarget).ToString())
                .FastConcat(" in group ").FastConcat(@group.ToString())
            );
        }
    }
}