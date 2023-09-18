#if GODOT
using Godot;

namespace Svelto.ECS.Schedulers.Godot
{
    /// <summary>
    /// Unlike Unity, in godot Everything is a Node. Monobehaviour = Node, GameObject = Node etc.
    /// </summary>
    public partial class GodotScheduler : Node
    {   
        internal System.Action onTick;

        public override void _Process(double delta)
        {
           Routine();
        }

        public async void Routine()
        {
            while (true)
            {
                await ToSignal(GetTree(), "process_frame");
        
                onTick();
                
            }
        }
    }
}
#endif