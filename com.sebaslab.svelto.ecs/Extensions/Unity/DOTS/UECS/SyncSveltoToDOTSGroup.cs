#if UNITY_ECS
using Unity.Entities;
using Unity.Jobs;

namespace Svelto.ECS.SveltoOnDOTS
{
    /// <summary>
    ///     HOW DOTS SYSTEMBASE DEPENDENCY SYSTEM WORKS:
    ///     EACH SYSTEMBASE DEPENDENCY REMEMBERS ONLY THE JOBHANDLES OF THE JOBS THAT TOUCH THE COMPONENTS ACCESSED
    ///     BY THE FOREACH. THEY WON'T AUTOMATICALLY CARRY OVER DEPENDENCIES OF JOBS TOUCHING OTHER COMPONENTS AS
    ///     THE DEPENDENCY IS OPTIMIZED TO CARRY ONLY WHAT IS OF INTEREST TO MAKE THE CURRENTLY ACCESSED COMPONENTS
    ///     WORK AS INTENDED.
    ///     HOWEVER THEY CARRY OVER EXTERNAL DEPENDENCIES COMBINED EXPLICITLY.
    /// </summary>
    // Tim Johansson  15 hours ago
    // To make sure I got this right, you have something like systemA.Update(); systemB.Update(); systemC.Update() and you want to do systemC.Dependency.Complete() in order to wait for all jobs scheduled by any of the three systems?
    // If so it will not work in all cases because the dependencies are forwarded based on what data you access like @cort says.
    // If for example systemA is accessing component 1 and nothing else, and no other system is accessing component 1, any job scheduled by systemA will not be included.
    // A bit more advanced, if systemB and systemC are both reading component 2 and do not share any other components, then the jobs from systemB will not be a dependency for systemC since readers of data do not have to wait for other readers. (edited) 
    //
    // Cort Stratton  15 hours ago
    // Tim is correct; the Dependency for systemC would not include jobs scheduled by systemA and systemB, if systemA and systemB do not share component dependencies with systemC. So completing systemC's dependency would not automatically complete ALL jobs from ALL previous systems. If that's your goal, you probably want something like EntityManager.CompleteAllJobs(), but that's a very expensive function and is mostly geared towards test code.
    //
    // Tim Johansson  1 hour ago
    // There are never any automatic dependencies between systems unless they access the same data, and there is no dependency if both are reading the data (unless there is a writer between them).
    // Whatever the final state of Dependency is in a system will be an input Dependency for later systems accessing the same data, there is no filtering so any combine dependency stored in Dependency will propagate assuming data is shared.
    // So if systemA reads C1 and writes C2 what will happen is that after systemA has run we?ll look at the final state of Dependency. Will register that any system writing C1 will have to depend on it, any system reading or writing C2 will have to depend on it.
    // When systemB runs it will check which dependencies are registered in the system for accessing the components it needs, and combine all of those. That is the input Dependency when the system starts.
    //
    // Tim Johansson  10 minutes ago
    // If you set Dependency that will be propagated the same way regardless of if it is a job you schedule or a combined dependency. We?ll only look at the final state and propagate that for all types you accessed. So there is no special propagation of it unless there is shared data between the systems
    // yes my previous (and last) question was not about components though, I am still just talking what if I want Dependency to care about external jobs?
    // a. would combining Dependency with external jobs guarantee that the external jobs are executed before the current SystemBase Jobs are executed
    // b. would the external jobs (handles) be carried to the next SystemBase or I have to combine the external job handles with the current Dependency before each SystemBase update?
    // I hope this is clear, it's really my last doubt :disappointed: (edited) 
    // New
    //
    // Tim Johansson  1 minute ago
    // a. Yes, as long as you combine before you schedule it will make the external jobs a dependency for everything you schedule (assuming you do not explicitly schedule with a different dependency). The Combine before Update in your sample would work.
    // b. Depends on what you mean by ?next? SystemBase. The external jobs will be carried forward the exact same way as the jobs you schedule. So if the external dependency is forwarded or not depends on what data the two systems are accessing, there is not special code that forwards the dependency unless they share data
    // Sebastiano Mandala  2 hours ago
    // I see so if the external job handle is not about components, but external datastructures, I have to combine it every time
    //
    // Sebastiano Mandala  2 hours ago
    // tthat is actually what I am doing so it would be fine
    //
    // Sebastiano Mandala  2 hours ago
    // I just wanted to know I wasn't being over defensive
    //
    // Tim Johansson  2 hours ago
    // Yes, you would have to do it every time
    public class SyncSveltoToDOTSGroup: UnsortedJobifiedEnginesGroup<SyncSveltoToDOTSEngine> { }

    public abstract partial class SyncSveltoToDOTSEngine: SystemBase, IJobifiedEngine
    {
        //The dependency returned is enough for the Svelto Engines running after this to take in consideration
        //the Systembase jobs. The svelto engines do not need to take in consideration the new dependencies created
        //by the World.Update because those are independent and are needed only by the next World.Update() jobs
        public JobHandle Execute(JobHandle inputDeps)
        {
            _inputDeps = inputDeps;

            Update(); //this complete the previous frame jobs so dependency cannot be modified atr this point

            return Dependency;
        }

        //TODO if this is correct must change SyncDOTSToSveltoGroup too
        protected sealed override void OnUpdate()
        {
            //SysteBase jobs that will use this Dependency will wait for inputDeps to be completed before to execute
            Dependency = JobHandle.CombineDependencies(Dependency, _inputDeps);

            OnSveltoUpdate();
        }

        protected abstract void OnSveltoUpdate();

        public abstract string name { get; }

        JobHandle _inputDeps;
    }
}
#endif