using System;
using System.Collections.Generic;

namespace Svelto.ECS
{
    interface IEntitySerializationFactory
    {
        EntityInitializer BuildEntity(EGID egid, IComponentBuilder[] componentsToBuild, Type descriptorType,
            IEnumerable<object> implementors = null,
            [System.Runtime.CompilerServices.CallerMemberName] string caller = null);
    }
}