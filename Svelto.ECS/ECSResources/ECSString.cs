using System;

namespace Svelto.ECS.Experimental
{
    [Serialization.DoNotSerialize]
    public struct ECSString:IEquatable<ECSString>
    {
        uint _id;

        public ECSString(string newText)
        {
            _id = ResourcesECSDB<string>.ToECS(newText);
        }
        
        ECSString(uint id)
        {
            _id = id;
        }

        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString._id);
        }
        
        public void Set(string newText)
        {
            if (_id != 0)
                ResourcesECSDB<string>.resources(_id) = newText;
            else
                _id = ResourcesECSDB<string>.ToECS(newText);
        }

        public ECSString Copy()
        {
            DBC.ECS.Check.Require(_id != 0, "copying not initialized string");
            
            var id = ResourcesECSDB<string>.ToECS(ResourcesECSDB<string>.resources(_id));
            
            return new ECSString(id);
        }

        public bool Equals(ECSString other)
        {
            return other._id == _id;
        }

        public override string ToString()
        {
            return ResourcesECSDB<string>.FromECS(_id);
        }
    }
}