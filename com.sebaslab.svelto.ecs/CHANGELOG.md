# Svelto.ECS Changelog
All notable changes to this project will be documented in this file. Changes are listed in random order of importance.

## [3.5.2] - 07-2024

* Minor serialization code improvements
* Remove legacy filters once for all
* breaking change: GetOrCreate*Filter, Create*Filter, Get*Filter don't return by ref anymore to fix reported bug
* references are now updated at the end of the submission frame so they are accessible inside callbacks

## [3.5.1] - 01-2024

* Remove UnityEntitySubmissionScheduler, it was never needed, the user can use the standard EntitySubmissionScheduler and tick it manually
* Dropped the idea to specialise EntitiesSubmissionScheduler. In hindsight it was never necessary.
* Added better support for range exclusive groups, now they are correctly registered in the group hash map
* Removed annoying Group compound/tag {type} is not sealed warning
* Merged Cuyi's workaround to be able to query compound groups in abstract engine. Never had the time to implement a better solution
* It is now possible again to add an entity multiple times inside a filter (it will be overriden)
* Fixed issue https://github.com/sebas77/Svelto.ECS/issues/123
* Fixed issue https://github.com/sebas77/Svelto.ECS/issues/122
* Fixed issue https://github.com/sebas77/Svelto.ECS/issues/121
* AddEngine now adds engines contained in a GroupEngine to the EnginesGroup optionally

## [3.5.0] - 09-2023

* Introduced Serialization namespace for the serialization code
* Unity: dropped 2020 support, minimum requirement is no 2021.3
* Unity DOTS: added CreateDOTSToSveltoSyncEngine method in SveltoOnDOTSEnginesGroup
* Refactor: split NB/MB struct from their internal logic that must be used only by the framework. Eventually NB and MB structs must be ref, as they are not supposed to be held (they may become invalid over the time). However due to the current DOTS patterns this is not possible. In future a sentinel pattern will allow to lease these buffers with the assumption that they can't be modified while held (and if a modification happens an exception will throw)
 * Improved managed EGIDMultiMapper. A MultiMapper can improve components fetching performance
 * Renamed IDisposableEngine interface to IDisposableEngine
 * added EntityReference Exists method to validate it against a given entity database
 * BUG FIXED: IReactOnDisposeEx callbacks were not correctly called
 * BUG FIXED: fixed serious bug that would pass wrong entities indices to the moveTO callback under specific conditions
 * Added Group Range functionality to GroupCompound
 * Added Offset to GroupCompound to know the index of a given group compared to the starting group ID of the compound range (check MiniExample 9, Groupsonly for example)
 * range and bitmask can be now set only in GroupTag and be inherited by GroupCompounds. GroupCompound bitmasks will be the OR of the group tags bitmasks, while the range will be the larger of the group tags ranges.
 * entity filters enumerator do not iterate anymore filters belonging to disabled groups
 * remove operations can now be executed in the same frame of a swap. a Remove will always supersed as Swap operation
 * engines added to a GreoupEngine are automatically added to the enginesgroup 
 

## [3.4.6] - 05-2023

* SveltoOnDOTS bug fixes/improvements
* Comments and code cleanup

## [3.4.4] - 04-2023

* refactored internal datastructures 
* added IReactOnDisposeEx interface
* added code to warmup all the entity descriptors at startup to avoid first time allocations when an entitydescriptor is used for the very first time
* added the option to iterate transient filters per component like it already happens with persistent filters. Transient filters are tracked optionally. 
* fixed huge bug in the filter enumerator, truly surprised this never showed up

## [3.4.2] - 03-2023

* removed static caches used in performance critical paths as they were causing unexpected performance issues (the fetching of static data is slower than i imagined)
* add Native prefix in front of the native memory utilities method names
* largely improved the console logger system
* minor improvements to the platform profiler structs
* improvements to the ThreadSafeObjectPool class (some refactoring too)
* added several datastructures previously belonging to Svelto.ECS
* all the FastClear methods are gone. The standard clear method now is aware of the type used and will clear it in the fastest way possible
* MemClear is added in case memory needs to be cleared explicitly
* added new SveltoStream, Unmanaged and Managed stream classes, their use case will be documented one day
* renamed the Svelto.Common.DataStructures namespace to Svelto.DataStructures
* added FixedTypedArray* methods. Fixed size arrays embedded in structs are now possible
* FasterList extension to convert to Span and ByteSpan
* Fix reported bugs
* Minor Svelto Dictionary improvements
* Added ValueContainer, a simple int, Tvalue dictionary based on sparse set. It has very specific use cases at the moment. Mainly to be used for the new ECS OOP Abstraction resoruce manager
* Added IReactOnSubmissionStarted interface

### SveltoOnDOTS changes

* update to DOTS 1.0 (but still compatible with 0.51, although slower)
* Deprecated the use of EntityCommandBuffer since was very slow
* added faster batched DOTS operations, new DOTS creation patterns introduced (old one still compatible as long as EntityCommandBuffer was not used)
* ISveltoOnDOTSSubmission interface exists only to allow the user to submit entities On DOTS explicitly, use this instead of 
* SveltoOnDOTSHandleCreationEngine is no more, you want to use ISveltoOnDOTSStructuralEngine and its DOTSOperations instead wherever EntityManager was used before
* ISveltoOnDOTSStructuralEngine is no more, you want to use ISveltoOnDOTSStructuralEngine and its DOTSOperations instead
* in all the case above, if you were relying on Update you probably want to use OnPostSubmission instead
* DOTSOperations new AddJobToComplete method will allow to register jobs from inside ISveltoOnDOTSStructuralEngines that will be completed at the end of the submission

## [3.3.2] - 04-06-2022

* Internal refactoring to support future features. Currently it may translate to a small performance boost
* IEntityComponent and IEntityViewComponent now implements _IInternalEntityComponent. This shouldn't affect existing code
* Improve thread-safety of entity building
* Fixed serious bug that affected the integrity of the EntityIDs values during RemoveEX callbacks
* The point above may result in a performance boost in the Filters updates during submission
* Code is again 2019 compatible (this may have been broken for a while)
* Fix a crash wit the EntityCollection deconstruction while trying to deconstruct an empty collection
* Breaking: EntityFilterCollection GetGroupFilter will throw an exception if the filter doesn't exist. Use GetOrCreateGroupFilter in case.
* Breaking: LocatorMap has been renamed to EntityReferenceMap
* Breaking: GetEntityLocatorMap has been renamed to GetEntityReferenceMap


## [3.3.1] - 26-04-2022

* Fixed serious bug that would affect the new IReactOnRemoveEx callbacks

## [3.3.0] - 11-04-2022

* INeedEGID and INeedEntityReference interfaces are not deprecated, but still available for backwards compatibility through the define SLOW_SVELTO_SUBMISSION
* There are some minor breaking changes, you may need to rename a bunch of methods calls
* Drastically improved Submission phase performance
* All the IReactOn interfaces are now replaced by much faster IReacOn\*Ex interfaces. Use those instead.
* QueryEntities methods now optionally return also an array of Entity IDs that you can reference like a component (this supersedes INeedEGID)
* Completely reworked and way more powerful filter API. The old one has been renamed to Legacy and left for backward compatibility
* NativeEGIDMultiMapper doesn't need to be created every submission anymore. It can be created permanently and disposed when not used anymore (some caveats with it)
* Improved Serialization system
* Improved SveltoOnDots system
* Tons of other improvements and bug fixes

## [3.2.5]

* refactor and improved NativeBag and UnsafeBlob. This fix a previously known crash with Unity IL2CPP

## [3.2.0]

* Improved checks on Svelto rules for the declaration of components and view components. This set of rules is not final yet (ideally one day they should be moved to static analyzers)
* Introduce the concept of Entity Reference. It's a very light weight identifier to keep track of entities EGID that can change dynamically (EGIDs change when groups are swapped), Entity References never change. The underlying code will be optimised even further in future.
* Introduced the concept of Disabled Group. Once a group is marked as disabled, queries will always ignore it.
* Merged DispatchOnSet and DispatchOnChange and renamed to ReactiveValue. This class will be superseded by better patterns in future.
* Added FindGroups with 4 components
* Improved QueryGroups interface
* Improved DynamicEntityDescriptor interface
* Improved ExtendibleEntityDescriptor interface
* Improved Native memory support
* Improved Svelto and Unity DOTS integration
* Improved and fixed Serialization code
* Ensure that the creation of static groups is deterministic (GroupHashMap)

## [3.1.3]

* bumped dependency of Svelto.Common due to an important fix there.

## [3.1.2]

* improved async entity submission code (still experimental)
* improved native entity operations debug info

## [3.1.1]

* SubmissionEngine didn't need the EntityManager property, so it has been removed

## [3.1.0]

* rearrange folders structures for clarity
* added DoubleEntitiesEnumerator, as seen in MiniExample 4, to allow a double iteration of the same group skipping
  already checked tuples
* reengineered the behaviour of WaitForSubmissionEnumerator
* removed redudant SimpleEntitySubmissionSchedulerInterface interface
* renamed BuildGroup in to ExclusiveBuildGroup
* renamed EntityComponentInitializer to EntityInitializer
* Entity Submission now can optionally be time sliced (based on number of entities to submit per slice)
* working on the Unity extension Submission Engine, still WIP
* added the possibility to hold a reference in a EntityViewComponent. This reference cannot be accesses as an object,
  but can be converted to the original object in OOP abstract layers
* renamed NativeEntityComponentInitializer to NativeEntityInitializer



