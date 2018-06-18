# Svelto Entity Component System 2.5
=====================================

Real Entity-Component-System for c#. Enables to write encapsulated, decoupled, maintainable, highly efficient, data oriented, cache friendly, multi-threaded, code without pain. Although the framework is platform agnostic (compatible with .net 3.5, .net 4.5 and .net standard 2.0), it comes with several Unity extensions. 

## Why using Svelto.ECS with Unity?

_Svelto.ECS wasn’t born just from the needs of a large team using Unity on several projects, but also as result of years of reasoning behind software engineering applied to game development (*) and that’s why the main Svelto.ECS goals are currently different than the ones set by Unity ECS. They may converge one day, but at the moment they are different enough to justify the on going development of Svelto.ECS. The biggest difference is that Svelto.ECS primary goal is not performance. Performance gain is just one of the benefits in using Svelto.ECS, as the ECS design in general helps to write cache-friendly code, albeit the main reasons why Svelto.ECS has been written orbit around the shifting of paradigm from Object Oriented Programming, the consequent improvement of the code design and maintainability, the approachability by junior programmers that won’t need to worry too much about the architecture and can focus on the solution of the problems thanks to the rigid directions that the framework gives. Svelto.ECS 2.5 is the result of years of iterating the ECS design applied to real game development, with the intent to be "junior code proof"._

You can find a working example to learn how to use the framework here:

## Examples

* **Survival Example (Unity)**: https://github.com/sebas77/Svelto-ECS-Example (including article)
* **Vanilla Example (.Net Standard/.Net Core)**: https://github.com/sebas77/Svelto.ECS.Examples.Vanilla (including article)

## Articles

**Svelto users articles:**

* https://eagergames.wordpress.com/category/ecs/ (Dario Oliveri)
* https://blogs.msdn.microsoft.com/uk_faculty_connection/2018/05/08/entity-component-system-in-unity-a-tutorial/ (Lee Stott)

**Framework articles:**

* http://www.sebaslab.com/svelto-ecs-2-5-and-allocation-0-code/ (shows what's changed compared to 2.0)
* http://www.sebaslab.com/svelto-ecs-2-0-almost-production-ready/ (shows what's changed compared to 1.0)
* http://www.sebaslab.com/ecs-1-0/
* http://www.sebaslab.com/learning-svelto-ecs-by-example-the-unity-survival-example/
* http://www.sebaslab.com/learning-svelto-ecs-by-example-the-vanilla-example/
* http://www.sebaslab.com/svelto-ecs-svelto-tasks-to-write-data-oriented-cache-friendly-multi-threaded-code-in-unity/ (since I don't like the example much, I didn't update the code, but the article is still valid)

**Theory related articles:**

* http://www.sebaslab.com/ioc-container-for-unity3d-part-1/
* http://www.sebaslab.com/ioc-container-for-unity3d-part-2/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-i-dependency-injection/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-ii-inversion-of-control/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iii-entity-component-systems/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-iv-dependency-inversion-principle/
* http://www.sebaslab.com/the-truth-behind-inversion-of-control-part-v-drifting-away-from-ioc-containers/

Note: I included the IoC articles just to show how I shifted over the years from using an IoC container to use an ECS framework and the rationale behind its adoption.

**The perfect companion for Svelto.ECS is Svelto.Tasks to run the logic of the Systems even on other threads!**

* https://github.com/sebas77/Svelto.Tasks

**Unity official forum thread:**

* https://forum.unity.com/threads/open-source-svelto-ecs-lightweight-entity-component-system-for-c-and-unity.502163/

**Official Chat**

* https://gitter.im/Svelto-ECS/Lobby

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
Svelto.ECS 2.5 has all the features currently needed to make a game with the ECS pattern. The design can very likely still improved and tweaked, but I hope I won't need to add any major feature anymore or undergo any major refactoring. The tools I'd like to have, but that probably I won't have the time to develop, are visual tools. What I have in mind is to use reflection to analyze the assemblies and visualize with a visual tool the engines, entities and maybe their relations. The Context are also designed to be able to be substituted with config files, so it would be theoretically possible to generate EnginesRoots, add Engines, inject Sequencers automatically instead to do it manually in the CompositionRoot.

## FAQ

https://github.com/sebas77/Svelto.ECS/wiki/FAQ

## Svelto Framework is used to develop the following products(*):

![Robocraft](https://i.ytimg.com/vi/JGr1Em2Ip-c/maxresdefault.jpg)
![Robocraft Infinity](https://news.xbox.com/en-us/wp-content/uploads/Robocraft_Hero-hero.jpg)
![Robocraft Royale](https://static.altchar.com/live/media/images/950x633_ct/7707_Robocraft_Royale_2bc6bb8ceab8ce0a1568fb37bd826b3f.jpg)!
![Cardlife](https://i.ytimg.com/vi/q2jaUZjnNyg/maxresdefault.jpg)

*if you want your products made with Svelto here, just send me an email or whatever, I'll be super happy to add them.

**_Note: Dear Svelto Users : Although I am committed to help you and write articles as much as I can, I will never be able to keep all the documentation up to date. If you are a happy svelto user and you want to contribute, please feel free to update the github wiki! 🙏👊_**
