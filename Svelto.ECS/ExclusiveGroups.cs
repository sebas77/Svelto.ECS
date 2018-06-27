namespace Svelto.ECS
{
    public class ExclusiveGroups
    {
        internal const int StandardEntity = int.MaxValue;

        public ExclusiveGroups()
        {
            _id = _globalId++;
        }
        
        public static explicit operator int(ExclusiveGroups group) // explicit byte to digit conversion operator
        {
            return group._id;
        }

        readonly int _id;
        static int _globalId;
    }
}