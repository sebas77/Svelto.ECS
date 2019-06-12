namespace Svelto.ECS.Experimental
{
    public struct ECSString
    {
        internal uint id;

        ECSString(uint toEcs)
        {
            id = toEcs;
        }

        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString.id);
        }
        
        public static implicit operator ECSString(string text)
        {
            return new ECSString(ResourcesECSDB<string>.ToECS(text));
        }
    }
}