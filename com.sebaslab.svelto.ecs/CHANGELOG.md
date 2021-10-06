# Changelog
All notable changes to this project will be documented in this file. Changes are listed in random order of importance.

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

### Fixed

* bumped dependency of Svelto.Common due to an important fix there.

## [3.1.2]

### Changed

* improved async entity submission code (still experimental)
* improved native entity operations debug info

## [3.1.1]

### Changed

* SubmissionEngine didn't need the EntityManager property, so it has been removed

## [3.1.0]

### Changed

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

### Fixed

