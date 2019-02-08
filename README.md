# Svelto Entity Component System 2.7
=====================================

Real Entity-Component-System for c#. Enables to write encapsulated, decoupled, maintainable, highly efficient, data oriented, cache friendly, multi-threaded, code without pain. Although the framework is platform agnostic (compatible with .net 3.5, .net 4.5 and .net standard 2.0), it comes with several Unity extensions. 

## Why using Svelto.ECS with Unity?

_Svelto.ECS wasn't born just from the needs of a large team, but also as result of years of reasoning behind software engineering applied to game development(*). Compared to Unity.ECS the main goals and reasons for Svelto.ECS to exist are different enough to justify its on going development (plus Svelto is platform agnostic, so it has been written with portability in mind). Svelto.ECS hasn't been built just to develop faster code, it has been built to help develop better code. Performance gain is one of the benefits in using Svelto.ECS, as ECS in general is a great way to write cache-friendly code, but the main reasons why Svelto.ECS has been written orbit around the shifting of paradigm from Object Oriented Programming, the consequent improvement of the code design and maintainability, the approachability by junior programmers that won't need to worry too much about the architecture and can focus on the solution of the problems thanks to the rigid directions that the framework gives. Svelto.ECS is the result of years of iteration of the ECS paradigm applied to real game development with the intent to be "junior coder proof"._

## Examples

* **Survival Example (Unity)**: https://github.com/sebas77/Svelto-ECS-Example (including article)
* **Vanilla Example (.Net Standard/.Net Core)**: https://github.com/sebas77/Svelto.ECS.Examples.Vanilla (including article)

## Users Examples (software made by Svelto users)

* **Gungi (Unity)**: https://github.com/grovemaster/Unity3D-Game-App

## Articles

**Framework articles:**

* http://www.sebaslab.com/svelto-2-7-whats-new-and-best-practices/ (shows what's changed since 2.5)
* http://www.sebaslab.com/svelto-ecs-2-5-and-allocation-0-code/ (shows what's changed since 2.0)
* http://www.sebaslab.com/svelto-ecs-2-0-almost-production-ready/ (shows what's changed since 1.0)
* http://www.sebaslab.com/ecs-1-0/
* http://www.sebaslab.com/learning-svelto-ecs-by-example-the-unity-survival-example/
* http://www.sebaslab.com/learning-svelto-ecs-by-example-the-vanilla-example/
* http://www.sebaslab.com/svelto-ecs-svelto-tasks-to-write-data-oriented-cache-friendly-multi-threaded-code-in-unity/

**Theory related articles (please read them to understand why I think ECS is great to write maintainable code):**

* http://www.sebaslab.com/the-quest-for-maintainable-code-and-the-path-to-ecs/
* http://www.sebaslab.com/ioc-container-for-unity3d-part-1/
* http://www.sebaslab.com/ioc-container-for-unity3d-part-2/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-i-dependency-injection/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-ii-inversion-of-control/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iii-entity-component-systems/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iv-dependency-inversion-principle/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-v-drifting-away-from-ioc-containers/

Note: I included the IoC articles just to show how I shifted over the years from using an IoC container to use an ECS framework and the rationale behind its adoption.

**Svelto users articles (note they may use an outdated version of Svelto):**

* https://eagergames.wordpress.com/category/ecs/ (Dario Oliveri)
* https://blogs.msdn.microsoft.com/uk_faculty_connection/2018/05/08/entity-component-system-in-unity-a-tutorial/ (Lee Stott)

_If you write an article about Svelto, please let me know, I will add it here!_

**The perfect companion for Svelto.ECS is Svelto.Tasks to run the logic of the Systems even on other threads!**

* https://github.com/sebas77/Svelto.Tasks

**Unity official forum thread:**

* https://forum.unity.com/threads/open-source-svelto-ecs-lightweight-entity-component-system-for-c-and-unity.502163/

**Official Chat**

* https://discord.gg/3qAdjDb 

**NED-Studio Svelto ECS inspector (WIP)**

* https://github.com/NED-Studio/LGK.Inspector

## How to clone

The folders Svelto.ECS, Svelto.Tasks and Svelto.Common, where present, are submodules pointing to the relavite repositories. If you find them empty, you need to update them through the submodule command. Check some instructions here: https://github.com/sebas77/Svelto.ECS.Vanilla.Example/wiki

**Note: don't beat yourself up if you find Svelto.ECS hard to use at first. The framework is very light in features, but it forces the use of a new coding paradigm and shifting code paradigm is hard! I will try to clarify all the concepts writing more and more articles**

## In case of bugs

Best option is to fork and clone https://github.com/sebas77/Svelto.ECS.Tests, add a new test to reproduce the problem and request a pull. Then open a github, I come here pretty often :). Also feel free to contact me on twitter or leave comments on the blog!

## [The Github wiki page](https://github.com/sebas77/Svelto.ECS/wiki)

It needs love and as far as I understood, anyone can edit it. Feel free to do so if you have a good understanding of Svelto!

## I like the project, how can I help?

Hey thanks a lot for considering this. You can help in two ways. First one is with the documentation, updating the wiki or writing your own articles. The second one is helping me coding new features.
Svelto.ECS has all the features needed to make a game with the ECS pattern, but the design can very likely still improved and tweaked, but I hope I won't need to add any major features anymore or undergo any major refactoring (that happened already several times since I wrote this paragraph). I'd love to have some visual tools, at least for Unity, but that probably I won't have the time to develop them. What I have in mind is to use reflection or static analisis to analyze the code and visualize the engines, entities and maybe their relations. The Context are also designed to be able to be substituted with config files, so it would be theoretically possible to generate EnginesRoots, add Engines, inject Sequencers automatically instead to do it manually in the Composition Root. 

## FAQ

https://github.com/sebas77/Svelto.ECS/wiki/FAQ

## Svelto Framework is used to develop the following products(*):

![Robocraft](https://i.ytimg.com/vi/JGr1Em2Ip-c/maxresdefault.jpg)
![Robocraft Infinity](https://news.xbox.com/en-us/wp-content/uploads/Robocraft_Hero-hero.jpg)
![Cardlife](https://i.ytimg.com/vi/q2jaUZjnNyg/maxresdefault.jpg)

*if you want your products made with Svelto here, just send me an email or whatever, I'll be super happy to add them.

**_Note: Dear Svelto Users : Although I am committed to help you and write articles as much as I can, I will never be able to keep all the documentation up to date. If you are a happy svelto user and you want to contribute, please feel free to update the github wiki! 🙏👊_**
