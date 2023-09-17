# Svelto.ECS C# Entity Component System framework

Real ECS framework for c\#. Enables to write encapsulated, decoupled, maintainable, highly efficient, data oriented, cache friendly, code without pain. Although the framework is platform agnostic \(compatible with c\# 7 and above and .net standard 2.0 and above\), it comes with several Unity extensions.

## Svelto.ECS in pills
Svelto.ECS is easy to start with, but full of tricks for expert users. The hardest problem to overcome is usually to shift mentality from OOP programming to ECS programming rather than using the framework itself. If you would like to read an ECS faq, you can check this article: https://github.com/SanderMertens/ecs-faq

### Svelto.ECS at glance

simplest setup:

```csharp
    public class SimpleContext
    {
        //the group where the entity will be built in
        public static ExclusiveGroup group0 = new ExclusiveGroup(); 

        public SimpleContext()
        {
            var simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            //Build Svelto Entities and Engines container, called EnginesRoot
            _enginesRoot = new EnginesRoot(simpleSubmissionEntityViewScheduler);

            var entityFactory   = _enginesRoot.GenerateEntityFactory();

            //Add an Engine to the enginesRoot to manage the SimpleEntities
            var behaviourForEntityClassEngine = new BehaviourForEntityClassEngine();
            _enginesRoot.AddEngine(behaviourForEntityClassEngine);

            //build a new Entity with ID 0 in group0
            entityFactory.BuildEntity<SimpleEntityDescriptor>(new EGID(0, ExclusiveGroups.group0));

            //submit the previously built entities to the Svelto database
            simpleSubmissionEntityViewScheduler.SubmitEntities();

            //as Svelto doesn't provide an engine ticking system, it's the user's responsibility to
            //update engines 
            behaviourForEntityClassEngine.Update();
        }
        
        readonly EnginesRoot _enginesRoot;
   }
```

your first entity descriptor:

```csharp
    public struct EntityComponent : IEntityComponent
    {
        public int  counter;
    }

    class SimpleEntityDescriptor : GenericEntityDescriptor<EntityComponent>
    {}
```

your first engine executing entities behaviours:

```csharp
    public class BehaviourForEntityClassEngine : IQueryingEntitiesEngine
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Update()
        {
            var (components, count) = entitiesDB.QueryEntities<EntityComponent>(ExclusiveGroups.group0);

            for (var i = 0; i < count; i++)
                components[i].counter++;
        }
    }
```

learn more about svelto on the Wiki page: https://github.com/sebas77/Svelto.ECS/wiki 

## Svelto.ECS philosophy
Svelto.ECS wasn't born just from the needs of a large team, but also as a result of years of reasoning behind software engineering applied to game development. Svelto.ECS hasn't been created just to write faster code, it has been designed to help developing better code. Performance gains is just one of the benefits in using Svelto.ECS, as ECS is a great way to write cache-friendly code. Svelto.ECS has been developed with the idea of ECS being a paradigm and not just a pattern, helping the user to shift away from Object Oriented Programming with consequent improvements on the code design and code maintainability. Svelto.ECS is the result of years of iteration of the ECS paradigm applied to real game development with the intent to be as foolproof as possible. Svelto.ECS has been designed to be used by a medium-size/large team working on long term projects where the cost of maintainability is relevant.

## Is it Archetype? Is it Sparse set? No it's something else :O
**Svelto.ECS** is loosely based on the **Archetype** idea. The main difference compared to any other Archetype-based model is that Svelto Archetypes are static, meaning that users cannot add or remove components at runtime. There are many design reasons behind this decision, including the fact that users are often not aware of the costs of structural changes.

While other frameworks typically limit user freedom to avoid exposing flaws in the archetype-based concept, Svelto.ECS introduces the concept of **groups**. This is simply an explicit way for the user to define sets of entities and iterate through them as quickly as possible.

**GroupCompounds** build on this idea by allowing users to change the "state"/"set"/"group" according to tags that serve effectively as adjective or state identifiers.

Entities can change state moving between sets swapping them in groups explcitly, rather than changing archetype.

## Why using Svelto.ECS with Unity?
Svelto.ECS doens't use a traditional archetype model like DOTS ECS does. The novel hybrid approach based on groups instead than archetypes has been designed to allow the user to take the right decision when it comes to states management. Svelto.ECS doesn't allow archetypes to change dynamically, the user cannot add or remove components after the entity is created. Handling entities states with components can quickly lead to very intensive structural changes operations, so groups have been introduced to avoid the wild explosion of states permutations and let the user see explicitly the cost of their decisions.

Filters have been added to make the handling of multiple states even more flexible by adding a new dimension to subset creation. Using filters it's possible to subset groups while avoiding structural changes (that happens when changing groups in Svelto.ECS too). DOTS engineers also realised this and consequentially introduced the new *Enableable components* which are less flexible than the Svelto.ECS filters as they implicitly tie the subsets to enabled/disabled components, while Filters in Svelto.ECS don't.

Thanks to the explicit use of Groups and Filters, the Svelto user is able to find the right trade off to handle entities states.

_Svelto.ECS is lean. It hasn't been designed to move a whole engine such as Unity from OOP to ECS, therefore it doesn't suffer from unjustifiable complexity overhead to try to solve problems that often are not linked to gameplay development. Svelto.ECS is fundamentally feature complete at this point of writing and new features in new versions are more nice to have than fundamental._

### Unity Compatibility
Svelto.ECS is partially compatible with Unity 2019.3.x cycle as long as it's not used with any DOTS package (including collections). It is compatible with all the versions of Unity from 2020 and above.

### Svelto.ECS is fully compatible with DOTS Burst and Jobs.
Svelto.ECS is designed to take full advantange of the DOTS modules and to use specifically DOTS ECS as an engine library, through the (optional) SveltoOnDOTS wrapper. 

## Why using Svelto.ECS without Unity?
There are so many c# game engines out there (Stride, Flax, Monogame, FlatRedBall, Evergine, UnrealCLR, UniEngine just to mention some) and Svelto.ECS is compatible with all of them! 

## Performance considerations
Aside from resizing the database absolutely when necessary, all the Svelto operations are memory allocation free. Some containers may need to be preallocated (and then disposed) but those are already advanced scenarios. When using pure ECS (no EntityViewComponents) components are stored in native collections across all the platforms, which means gaining some performance from losing the managed memory checks. With pure ECS, iterating components is automatically cache-friendly.

_Note: Svelto.ECS has a ton of allocating run-time checks in debug, so if you want to profile you need to profile a release version or use PROFILE_SVELTO define_

## If you decide to use Svelto.ECS
Svelto.ECS is an Open Source Project provided as it is, no support is guaranteed other than the help given on the Svelto Discord channel. Issues will be fixed when possible. If you decide to adopt Svelto.ECS, it's assumed you are willing to partecipate to the development of the product if necessary.

## Official Examples (A.K.A. where is the documentation?)
Documentation is costly to mantain so check the highly documented and simple mini-examples. Please study them all regardless the platform you intend to use Svelto with.

First of all please check the wiki page:

https://github.com/sebas77/Svelto.ECS/wiki

After that, you can get all the help you need from the official chat:

**Official Discord Server \(join to get help from me for free!\)**

* [https://discord.gg/3qAdjDb](https://discord.gg/3qAdjDb) 

but don't forget to check the FAQ AKA Discussions first (all the FAQ like questions will be redirected to Discussions)

https://github.com/sebas77/Svelto.ECS/discussions

## Official Articles
**Theory related articles (from the most recent to the oldest, read from the oldest if you are new to it):**

* [OOP abstraction layer in an ECS-centric application](https://www.sebaslab.com/oop-abstraction-layer-in-a-ecs-centric-application/)  \(this article is important for starters!\)
* [ECS abstraction layers and module encapsulation](https://www.sebaslab.com/ecs-abstraction-layers-and-modules-encapsulation/)
* [The Quest for Maintainable Code and The Path to ECS](http://www.sebaslab.com/the-quest-for-maintainable-code-and-the-path-to-ecs/)
* [The truth behind Inversion of Control – Part V – Entity Component System design to achieve true Inversion of Flow Control](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-v-drifting-away-from-ioc-containers/)
* [The truth behind Inversion of Control – Part IV – Dependency Inversion Principle](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iv-dependency-inversion-principle/)
* [The truth behind Inversion of Control – Part III – Entity Component System Design](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iii-entity-component-systems/)
* [The truth behind Inversion of Control – Part II – Inversion of Control](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-ii-inversion-of-control/)
* [The truth behind Inversion of Control – Part I – Dependency Injection](http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-i-dependency-injection/)
* [Inversion of Control with Unity – part 2](http://www.sebaslab.com/ioc-container-for-unity3d-part-2/)
* [Inversion of Control with Unity – part 1](http://www.sebaslab.com/ioc-container-for-unity3d-part-1/)

**Practical articles**

* [Svelto ECS 3.4 internals: How to integrate ComputeSharp](https://www.sebaslab.com/svelto-ecs-3-4-internals-how-to-integrate-computesharp/)
* [Svelto.ECS 3.4 – Svelto On DOTS ECS update](https://www.sebaslab.com/svelto-ecs-3-4-svelto-on-dots-ecs-update/)
* [Svelto.ECS 3.3 and the new Filters API](https://www.sebaslab.com/svelto-ecs-3-3-and-the-new-filters-api/)
* [What’s new in Svelto.ECS 3.0](https://www.sebaslab.com/whats-new-in-svelto-ecs-3-0/)
* [Svelto MiniExample 7: Stride Engine demo](https://www.sebaslab.com/svelto-miniexample-7-stride-engine-demo/)
* [Svelto Mini Examples: The Unity Survival Example](https://www.sebaslab.com/the-new-svelto-ecs-survival-mini-example/)
* [Svelto Mini (Unity) Examples: Doofuses Must Eat](https://www.sebaslab.com/svelto-mini-examples-doofuses-must-eat/)
* [Learning Svelto.ECS by example – The Vanilla Example](http://www.sebaslab.com/learning-svelto-ecs-by-example-the-vanilla-example/)
* [Porting a boid simulation from UnityECS/Jobs to Svelto.ECS/Tasks](https://www.sebaslab.com/porting-a-boid-simulation-from-unityecs-to-svelto-ecs/)
* [Svelto.ECS+Tasks to write Data Oriented, Cache Friendly, Multi-Threaded code](http://www.sebaslab.com/svelto-ecs-svelto-tasks-to-write-data-oriented-cache-friendly-multi-threaded-code-in-unity/)

Note: I included the IoC articles just to show how I shifted over the years from using an IoC container to use an ECS framework and the rationale behind its adoption.

## Users Generated Content \(I removed all the outdated articles, so this is a call for new ones!\)
* [A Beginner’s Guide to Svelto.ECS (3.0) with Unity by Jiheh Ritterling](https://jiheh.medium.com/a-beginners-guide-to-svelto-ecs-3-0-with-unity-e9dbc88a2145)

**Svelto Community Extensions**

* [***Svelto.ECS inspector***](https://github.com/akrogame/svelto-ecs-inspector)
* [***Svelto.ECS.Schema - Schema and State Machine extensions for Svelto.ECS***](https://github.com/cathei/Svelto.ECS.Schema)
* [***Being able to swap entities between a subset of compound tags to another subset of compound tags***](https://gist.github.com/jlreymendez/c2f441aaf6ac7b5f233ecd990314e9cc)
* [***Automatic way to control svelto engines order without having to pass in a string using attributes***](https://gist.github.com/dragonslaya84/88e6bb998eda8fe4ee912f01d67feec9)


## How to clone the repository:
The folders Svelto.ECS and Svelto.Common, where present, are submodules pointing to the relative repositories. If you find them empty, you need to update them through the submodule command. Check some instructions here: https://github.com/sebas77/Svelto.ECS.Vanilla.Example/wiki

## Svelto distributed as Unity Package through OpenUPM [![openupm](https://img.shields.io/npm/v/com.sebaslab.svelto.ecs?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.sebaslab.svelto.ecs/)

or just install the package that comes from the link https://package-installer.glitch.me/v1/installer/OpenUPM/com.sebaslab.svelto.ecs?registry=https%3A%2F%2Fpackage.openupm.com

this is shown in this example too: https://github.com/sebas77/Svelto.MiniExamples/tree/master/UPM-Integration/UPM

## Svelto distributed as Nuget
I am not a Nuget expert, but thanks to our contributors, Svelto.ECS can be found at https://www.nuget.org/packages/Svelto.ECS/

the Hello World example uses the nuget package directly: https://github.com/sebas77/Svelto.MiniExamples/tree/master/Example5-Net-HelloWorld

## In case of bugs
Best option is to fork and clone [https://github.com/sebas77/Svelto.ECS.Tests](https://github.com/sebas77/Svelto.ECS.Tests), add new tests to reproduce the problem and request a pull. I will then fix the issue. Also feel free to contact me on Discord.

## Unity Installation Note:

if you are installing Svelto.ECS manually and not through OUPM, you need to copy the projecs folders under the Package folder like:

<img width="512" alt="image" src="https://github.com/sebas77/Svelto.ECS/assets/945379/73d02526-f8f6-4aff-88d8-4bd3cdd9deeb">

and add this into your manifest:
```
,
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "org.nuget.system.runtime.compilerservices.unsafe"
      ]
    }
  ]
```

looking like:

```
{
  "dependencies": {
...
  },
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "org.nuget.system.runtime.compilerservices.unsafe"
      ]
    }
  ]
}

```

## I like the project, how can I help?
Hey, thanks a lot for considering this. You can help in several ways. The simplest is to talk about Svelto.ECS and spread the word, the more we are, the better it is for the community. Then you can help with the documentation, updating the wiki or writing your own articles. Svelto.ECS has all the features needed to make a game with the ECS pattern, but some areas are lacking: *A visual debugger and more unit tests are needed*. Other platforms other than Unity could get some love too: Stride Game, Godot, monogame, FNA or whatever supports c#. Porting to other languages, especially c++, would be awesome but probably pointless. Please check the lane dedicated to the community tasks list here: https://github.com/users/sebas77/projects/3 and let me know if you want to take something on!

## Svelto Framework is used to develop the following products\(\*\):
![Toy Trains](https://github.com/sebas77/Svelto.ECS/assets/945379/282494a8-1c0a-43be-8fb7-b2edddcf5938)
![Beyond These Stars](https://user-images.githubusercontent.com/945379/235711883-eff208d3-9e1f-45b7-90e5-83c2d7a0a805.png)
![Robocraft](https://user-images.githubusercontent.com/945379/225630614-20dbdf8e-2d7f-48d5-8e04-39e6d43a43ab.png)
![Techblox](https://user-images.githubusercontent.com/945379/123062411-65ee3600-d404-11eb-8dca-d30c28ed909d.png)
![Gamecraft](https://user-images.githubusercontent.com/945379/163145452-3e8d959a-1453-4373-8010-38bb7717f79e.png)
![Robocraft Infinity](https://user-images.githubusercontent.com/945379/163145385-7635f193-b69b-4508-a391-f41a3331122c.png)
![Cardlife](https://user-images.githubusercontent.com/945379/163145315-9ea85b13-48e1-42f3-b97b-3d2c7564a0ea.png)
![HoleIo](https://user-images.githubusercontent.com/945379/163145100-31039e0c-9604-4298-8ace-89f92b294e06.png)

\*If you want your products made with Svelto here, just send me an email or whatever, I'll be super happy to add them.

_**Note: Dear Svelto Users : Although I am committed to help you and write articles as much as I can, I will never be able to keep all the documentation up to date. If you are a happy svelto user and you want to contribute, please feel free to update the github wiki! 🙏👊**_


