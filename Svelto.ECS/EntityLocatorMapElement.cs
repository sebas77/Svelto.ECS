using System;

namespace Svelto.ECS
{
    internal struct EntityLocatorMapElement
    {
        internal EGID egid;
        internal uint version;

        internal EntityLocatorMapElement(EGID egid)
        {
            this.egid = egid;
            version = 0;
        }
    }
}