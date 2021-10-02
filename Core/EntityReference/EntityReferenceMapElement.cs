namespace Svelto.ECS.Reference
{
    struct EntityReferenceMapElement
    {
        internal EGID egid;
        internal uint version;

        internal EntityReferenceMapElement(EGID egid)
        {
            this.egid = egid;
            version = 0;
        }

        internal EntityReferenceMapElement(EGID egid, uint version)
        {
            this.egid = egid;
            this.version = version;
        }
    }
}