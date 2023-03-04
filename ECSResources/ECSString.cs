using System;
using System.Runtime.InteropServices;

namespace Svelto.ECS.ResourceManager
{
    /// <summary>
    /// Todo: the entityDB should be aware of the ECSString and recycle it on entity removal
    /// </summary>
    [Serialization.DoNotSerialize]
    [StructLayout(LayoutKind.Explicit)]
    public struct ECSString:IEquatable<ECSString>
    {
        [FieldOffset(0)] uint _id;
        [FieldOffset(4)] uint _versioning;
        [FieldOffset(0)] long _realID;

        public ECSString(string newText):this()
        {
            _id = ResourcesECSDB<string>.ToECS(newText);
        }

        ECSString(uint id):this()
        {
            _id = id;
        }

        public static implicit operator string(ECSString ecsString)
        {
            return ResourcesECSDB<string>.FromECS(ecsString._id);
        }

        /// <summary>
        /// Note: Setting null String could be a good way to signal a disposing of the ID so that
        /// it can be recycled.
        /// Zero id must be a null string
        /// </summary>
        /// <param name="newText"></param>
        public void Set(string newText)
        {
            if (_id != 0)
            {
                if (ResourcesECSDB<string>.resources(_id).Equals(newText) == false)
                {
                    ResourcesECSDB<string>.resources(_id) = newText;
                        
                    _versioning++;                        
                }
            }
            else
                _id = ResourcesECSDB<string>.ToECS(newText);
        }

        public ECSString Copy()
        {
            DBC.ECS.Check.Require(_id != 0, "copying not initialized string");
            
            var id = ResourcesECSDB<string>.ToECS(ResourcesECSDB<string>.resources(_id));
            
            return new ECSString(id);
        }

        public override string ToString()
        {
            return ResourcesECSDB<string>.FromECS(_id);
        }

        public bool Equals(ECSString other)
        {
            return _realID == other._realID;
        }

        public static bool operator==(ECSString options1, ECSString options2)
        {
            return options1._realID == options2._realID;
        }

        public static bool operator!=(ECSString options1, ECSString options2)
        {
            return options1._realID != options2._realID;
        }

        public override bool Equals(object obj)
        {
            throw new NotSupportedException(); //this is on purpose
        }

        public override int GetHashCode()
        {
            return _realID.GetHashCode();
        }
    }
}