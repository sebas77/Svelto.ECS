// using System.Threading.Tasks;
// using NUnit.Framework;
// using Svelto.Common;
// using Svelto.ECS.DataStructures;
//
// namespace Svelto.ECS.Tests.Common.DataStructures
// {
//     [TestFixture]
//     public class ThreadSafeNativeBagTest
//     {
//         [Test]
//         public void TestByteReallocWorks()
//         {
//             var threadNativeBag = new ThreadSafeNativeBag(Allocator.Persistent);
//             
//             Parallel.Invoke(() =>
//                             {
//                                 for (int i = 0; i < 100; i++)
//                                 {
//                                     threadNativeBag.Enqueue((int)1);
//                                 }
//                             }
//                             ,  // close first Action
//                              () =>
//                              {
//                                  for (int i = 0; i < 100; i++)
//                                  {
//                                      threadNativeBag.Enqueue((int)2);
//                                  }
//                              }
//                             , //close second Action
//                             
//                             () =>
//                             {
//                                 for (int i = 0; i < 100; i++)
//                                 {
//                                     threadNativeBag.Enqueue(3);
//                                 }
//                             } //close third Action
//             ); //close parallel.invoke
//             
//             // for (int i = 0; i < 100; i++)
//             // {
//             //     threadNativeBag.Enqueue(1);
//             // }
//
//             int oneCount = 0, twoCount = 0, threeCount = 0;
//             
//             while (threadNativeBag.count > 0)
//             {
//                 var value = threadNativeBag.Dequeue<int>();
//
//                 switch (value)
//                 {
//                     case 1:
//                         oneCount++;
//                         break;
//                     case 2:
//                         twoCount++;
//                         break;
//                     case 3:
//                         threeCount++;
//                         break;
//                 }
//             }
//             
//             Assert.That(oneCount, Is.EqualTo(100));
//             Assert.That(twoCount, Is.EqualTo(100));
//             Assert.That(threeCount, Is.EqualTo(100));
//             
//             threadNativeBag.Dispose();
//         }
//     }
// }