namespace Svelto.ECS
{
    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static MyExclusiveGroup1 = new ExclusiveGroup();
    /// }
    /// </summary>
    
    public class ExclusiveGroup
    {
        internal const int StandardEntitiesGroup = int.MaxValue;

        public ExclusiveGroup()
        {
            _id       =  _globalId;
            _globalId += 1;
        }

        /// <summary>
        /// Use this constructor to reserve N groups
        /// </summary>
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