namespace Svelto.ECS.Experimental
{
    public struct ECSString
    {
        internal uint id;
        
        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString.id);
        }
    }
}