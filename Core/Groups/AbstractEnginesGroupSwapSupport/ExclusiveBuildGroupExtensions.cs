using System;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    //Hi! I've been following your discussion on discord and have just a few questions on how to use this snippet properly:
    //
    //Did I get the idea right: this snippet allows us to swap group tags (from current to target) even if target combination of tags not declared in groups class (like GameGroups.cs in Doofuses iteration 3 example)? If so:
    //On discord you wrote:
    //The specialized engines define the swaps like this:
    //GroupCompound<SHIP, AI>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, AI>.BuildGroup);
    //GroupCompound<SHIP, PLAYER>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, PLAYER>.BuildGroup);
    //
    //And any engine can do this:
    //var targetGroup = egid.groupID.GetSwapTag<SUNK_SHIP, SHIP>();
    //_functions.SwapEntityGroup<ShipEntityDescriptor>(egid, targetGroup);
    //
    //by specialized / any did you mean not abstract / abstract or something else?
    //
    //When _removeTransitions, _addTransitions, _swapTransitions should be filled?
    //@jlreymendez
    //Author
    //jlreymendez commented on Aug 10, 2020
    //Hey!
    //
    //1- Yeah, it would allow you to swap to targets that are not directly declared in a group class. In reality you are declaring them when you declare the SetTagSwap, SetTagAddition and SetTagRemoval.
    //
    //2- Yes, correct by specialized I mean something not abstract, an engine that knows all the tags that apply to an entity. Thus it would allow you to create a comprehensive list of the swaps like you see in that code. Remember that when you do SetTagSwap<SHIP, SUNK_SHIP> the reverse will also be declared automatically.
    //
    //This is the latest from that code you shared:
    //
    //
    // // Register possible transitions.
    // GroupCompound<SHIP, AI, PIRATE>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, AI, PIRATE>.BuildGroup);
    // GroupCompound<SHIP, AI, MERCHANT>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, AI, MERCHANT>.BuildGroup);
    // GroupCompound<SHIP, AI, NORMAL>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, AI, NORMAL>.BuildGroup);
    //
    // GroupCompound<SHIP, PLAYER>.BuildGroup.SetTagSwap<SHIP, SUNK_SHIP>(GroupCompound<SUNK_SHIP, PLAYER>.BuildGroup);
    //
    //3- You would fill them at initialization time, I did it in the Ready() of the engine before entering the game loop. But you could do it in the constructor of the engine. Wherever you find it appropriate before you need to start swapping or changing tags.
    
    public static class ExclusiveBuildGroupExtensions
    {
        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _removeTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _addTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        internal static FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>
            _swapTransitions =
                new FasterDictionary<ExclusiveGroupStruct, FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>>();

        public static void SetTagAddition<T>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_addTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions             = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _addTransitions[@group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagRemoval<T>(target, group, false);
            }
        }

        public static void SetTagRemoval<T>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true) where T : GroupTag<T>
        {
            if (_removeTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions                = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _removeTransitions[@group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(T));
            transitions[type] = target;

            if (setReverse)
            {
                SetTagAddition<T>(target, group, false);
            }
        }

        public static void SetTagSwap<TRemove, TAdd>
            (this ExclusiveBuildGroup group, ExclusiveBuildGroup target, bool setReverse = true)
            where TRemove : GroupTag<TRemove> where TAdd : GroupTag<TAdd>
        {
            if (_swapTransitions.TryGetValue(@group, out var transitions) == false)
            {
                transitions             = new FasterDictionary<RefWrapper<Type>, ExclusiveBuildGroup>();
                _swapTransitions[group] = transitions;
            }

            var type = new RefWrapper<Type>(typeof(TAdd));
            transitions[type] = target;

            // To avoid needing to check if the group already has the tag when swapping (prevent ecs exceptions).
            // The current groups adds the removed tag pointing to itself.
            type              = new RefWrapper<Type>(typeof(TRemove));
            transitions[type] = group;

            if (setReverse)
            {
                SetTagSwap<TAdd, TRemove>(target, group, false);
            }
        }
    }
}