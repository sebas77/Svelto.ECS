using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// DynamicEntityDescriptor can be used to add entity views to an existing EntityDescriptor that act as flags,
    /// at building time.
    /// This method allocates, so it shouldn't be abused
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct DynamicEntityDescriptor<TType> : IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        internal DynamicEntityDescriptor(bool isExtendible) : this()
        {
            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length = defaultEntities.Length;

            _entitiesToBuild = new IEntityBuilder[length + 1];

            Array.Copy(defaultEntities, 0, _entitiesToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            _entitiesToBuild[length] = new EntityBuilder<EntityStructInfoView>
            (
                new EntityStructInfoView
                {
                    entitiesToBuild = _entitiesToBuild
                }
            );
        }

        public DynamicEntityDescriptor(IEntityBuilder[] extraEntityBuilders) : this()
        {
            var extraEntitiesLength = extraEntityBuilders.Length;

            _entitiesToBuild = Construct(extraEntitiesLength, extraEntityBuilders,
                EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild);
        }

        public DynamicEntityDescriptor(FasterList<IEntityBuilder> extraEntityBuilders) : this()
        {
            var extraEntities = extraEntityBuilders.ToArrayFast();
            var extraEntitiesLength = extraEntityBuilders.Count;

            _entitiesToBuild = Construct(extraEntitiesLength, extraEntities,
                EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild);
        }

        public void ExtendWith<T>() where T : IEntityDescriptor, new()
        {
            var newEntitiesToBuild = EntityDescriptorTemplate<T>.descriptor.entitiesToBuild;

            _entitiesToBuild = Construct(newEntitiesToBuild.Length, newEntitiesToBuild, _entitiesToBuild);
        }
        
        public void ExtendWith(IEntityBuilder[] extraEntities)
        {
            _entitiesToBuild = Construct(extraEntities.Length, extraEntities, _entitiesToBuild);
        }

        static IEntityBuilder[] Construct(int extraEntitiesLength, IEntityBuilder[] extraEntities,
            IEntityBuilder[] startingEntities)
        {
            IEntityBuilder[] localEntitiesToBuild;

            if (extraEntitiesLength == 0)
            {
                localEntitiesToBuild = startingEntities;
                return localEntitiesToBuild;
            }

            var defaultEntities = startingEntities;
            var length = defaultEntities.Length;

            var index = SetupSpecialEntityStruct(defaultEntities, out localEntitiesToBuild, extraEntitiesLength);

            Array.Copy(extraEntities, 0, localEntitiesToBuild, length, extraEntitiesLength);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            localEntitiesToBuild[index] = new EntityBuilder<EntityStructInfoView>
            (
                new EntityStructInfoView
                {
                    entitiesToBuild = localEntitiesToBuild
                }
            );

            return localEntitiesToBuild;
        }

        static int SetupSpecialEntityStruct(IEntityBuilder[] defaultEntities, out IEntityBuilder[] entitiesToBuild,
            int extraLenght)
        {
            int length = defaultEntities.Length;
            int index = -1;

            for (var i = 0; i < length; i++)
            {
                //the special entity already exists
                if (defaultEntities[i].GetEntityType() == EntityBuilderUtilities.ENTITY_STRUCT_INFO_VIEW)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                index = length + extraLenght;
                entitiesToBuild = new IEntityBuilder[index + 1];
            }
            else
                entitiesToBuild = new IEntityBuilder[length + extraLenght];

            Array.Copy(defaultEntities, 0, entitiesToBuild, 0, length);

            return index;
        }


        public IEntityBuilder[] entitiesToBuild => _entitiesToBuild;

        IEntityBuilder[] _entitiesToBuild;
    }
}