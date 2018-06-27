namespace Svelto.ECS
{
    public class ExclusiveGroup
    {
        internal const int StandardEntitiesGroup = int.MaxValue;

        public ExclusiveGroup()
        {
            _id = _globalId++;
        }
        
        public static explicit operator int(ExclusiveGroup group) // explicit byte to digit conversion operator
        {
            return group._id;
        }

        readonly int _id;
        static int _globalId;
    }
}