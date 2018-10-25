using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct DynamicEntityDescriptorInfo<TType>:IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        public DynamicEntityDescriptorInfo(FasterList<IEntityBuilder> extraEntities) : this()
        {
            DBC.ECS.Check.Require(extraEntities.Count > 0,
                                  "don't use a DynamicEntityDescriptorInfo if you don't need to use extra EntityViews");

            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length = defaultEntities.Length;

            entitiesToBuild = new IEntityBuilder[length + extraEntities.Count + 1];

            Array.Copy(defaultEntities,      0, entitiesToBuild, 0,      length);
            Array.Copy(extraEntities.ToArrayFast(), 0, entitiesToBuild, length, extraEntities.Count);

            var _builder = new EntityBuilder<EntityInfoView>
            {
                _initializer = new EntityInfoView { entitiesToBuild = entitiesToBuild }
            };
            entitiesToBuild[entitiesToBuild.Length - 1] = _builder;
        }

        public IEntityBuilder[] entitiesToBuild { get; private set; }
    }
    
    public struct EntityInfoView : IEntityStruct
    {
        public EGID ID { get; set; }
        
        public IEntityBuilder[] entitiesToBuild;
    }
}