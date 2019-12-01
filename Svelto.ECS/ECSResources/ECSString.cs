using System;

namespace Svelto.ECS.Experimental
{
    [Serialization.DoNotSerialize]
    public struct ECSString:IEquatable<ECSString>
    {
        uint id;

        public ECSString(string newText)
        {
            id = ResourcesECSDB<string>.ToECS(newText);
        }

        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString.id);
        }
        
        public void Set(string newText)
        {
            if (id != 0)
                ResourcesECSDB<string>.resources(id) = newText;
            else
                id = ResourcesECSDB<string>.ToECS(newText);
        }

        public bool Equals(ECSString other)
        {
            return other.id == id;
        }

        public override string ToString()
        {
            return ResourcesECSDB<string>.FromECS(id);
        }
    }
}