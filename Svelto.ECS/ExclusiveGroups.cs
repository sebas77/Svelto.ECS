namespace Svelto.ECS
{
    public class ExclusiveGroup
    {
        internal const int StandardEntitiesGroup = int.MaxValue;

        public ExclusiveGroup()
        {
            _id       =  _globalId;
            _globalId += 1;
        }

        public ExclusiveGroup(int range)
        {
            _id       =  _globalId;
            _globalId += range;
        }
        
        public static explicit operator int(ExclusiveGroup group) // explicit byte to digit conversion operator
        {
            return group._id;
        }

        readonly int _id;
        static   int _globalId;
    }
}