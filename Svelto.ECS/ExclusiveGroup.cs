using System;
using System.Collections.Generic;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    /// <summary>
    /// Exclusive Groups guarantee that the GroupID is unique.
    ///
    /// The best way to use it is like:
    ///
    /// public static class MyExclusiveGroups //(can be as many as you want)
    /// {
    ///     public static ExclusiveGroup MyExclusiveGroup1 = new ExclusiveGroup();
    ///
    ///     public static ExclusiveGroup[] GroupOfGroups = { MyExclusiveGroup1, ...}; //for each on this!
    /// }
    /// </summary>
    ///

    ///use this like:
    /// public class TriggersGroup : ExclusiveGroup<TriggersGroup> {}
    public abstract class NamedExclusiveGroup<T>:ExclusiveGroup
    {
        public static ExclusiveGroup Group = new ExclusiveGroup();
        public static string         name  = typeof(T).FullName;

        public NamedExclusiveGroup() { }

        public NamedExclusiveGroup(string recognizeAs) : base(recognizeAs)
        {}

        public NamedExclusiveGroup(ushort range) : base(range)
        {}
    }

    public class ExclusiveGroup
    {
        public ExclusiveGroup()
        {
            _group = ExclusiveGroupStruct.Generate();
        }

        public ExclusiveGroup(string recognizeAs)
        {
            _group = ExclusiveGroupStruct.Generate();

            _serialisedGroups.Add(recognizeAs, _group);
        }

        public ExclusiveGroup(ushort range)
        {
            _group = new ExclusiveGroupStruct(range);
#if DEBUG
            _range = range;
#endif
        }

        public static implicit operator ExclusiveGroupStruct(ExclusiveGroup group)
        {
            return group._group;
        }

        public static explicit operator uint(ExclusiveGroup group)
        {
            return group._group;
        }

        public static ExclusiveGroupStruct operator+(ExclusiveGroup a, uint b)
        {
#if DEBUG
            if (a._range == 0)
                throw new ECSException("adding values to a not ranged ExclusiveGroup");
            if (b >= a._range)
                throw new ECSException("Using out of range group");
#endif            
            return a._group + b;
        }

        readonly ExclusiveGroupStruct _group;

        //I use this as parameter because it must not be possible to pass null Exclusive Groups.
        public struct ExclusiveGroupStruct : IEquatable<ExclusiveGroupStruct>, IComparable<ExclusiveGroupStruct>,
                                IEqualityComparer<ExclusiveGroupStruct>
        {
            public static bool operator ==(ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
            {
                return c1.Equals(c2);
            }

            public static bool operator !=(ExclusiveGroupStruct c1, ExclusiveGroupStruct c2)
            {
                return c1.Equals(c2) == false;
            }

            public bool Equals(ExclusiveGroupStruct other)
            {
                return other._id == _id;
            }

            public int CompareTo(ExclusiveGroupStruct other)
            {
                return other._id.CompareTo(_id);
            }

            public bool Equals(ExclusiveGroupStruct x, ExclusiveGroupStruct y)
            {
                return x._id == y._id;
            }

            public int GetHashCode(ExclusiveGroupStruct obj)
            {
                return _id.GetHashCode();
            }

            internal static ExclusiveGroupStruct Generate()
            {
                ExclusiveGroupStruct groupStruct;

                groupStruct._id = _globalId;
                DBC.ECS.Check.Require(_globalId + 1 < ushort.MaxValue, "too many exclusive groups created");
                _globalId++;

                return groupStruct;
            }

            /// <summary>
            /// Use this constructor to reserve N groups
            /// </summary>
            internal ExclusiveGroupStruct(ushort range)
            {
                _id =  _globalId;
                DBC.ECS.Check.Require(_globalId + range < ushort.MaxValue, "too many exclusive groups created");
                _globalId += range;
            }

            internal ExclusiveGroupStruct(uint groupID)
            {
                _id = groupID;
            }

            public ExclusiveGroupStruct(byte[] data, uint pos)
            {
                _id = (uint)(
                    data[pos++]
                    | data[pos++] << 8
                    | data[pos++] << 16
                    | data[pos++] << 24
                );
                
                DBC.ECS.Check.Ensure(_id < _globalId, "Invalid group ID deserialiased");
            }

            public static implicit operator uint(ExclusiveGroupStruct groupStruct)
            {
                return groupStruct._id;
            }

            public static ExclusiveGroupStruct operator+(ExclusiveGroupStruct a, uint b)
            {
                var group = new ExclusiveGroupStruct();

                group._id = a._id + b;

                return group;
            }

            uint        _id;
            static uint _globalId;
        }

        public static ExclusiveGroupStruct Search(string holderGroupName)
        {
            if (_serialisedGroups.ContainsKey(holderGroupName) == false)
                throw new Exception("Named Group Not Found ".FastConcat(holderGroupName));

            return _serialisedGroups[holderGroupName];
        }

        static readonly Dictionary<string, ExclusiveGroupStruct> _serialisedGroups = new Dictionary<string,
            ExclusiveGroupStruct>();
#if DEBUG
        readonly ushort _range;
#endif        
    }
}

#if future
        public static void ConstructStaticGroups()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Assemblies or types aren't guaranteed to be returned in the same order,
            // and I couldn't find proof that `GetTypes()` returns them in fixed order either,
            // even for builds made with the exact same source code.
            // So will sort reflection results by name before constructing groups.
            var groupFields = new List<KeyValuePair<string, FieldInfo>>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = GetTypesSafe(assembly);

                foreach (Type type in types)
                {
                    if (type == null || !type.IsClass)
                    {
                        continue;
                    }

                    // Groups defined as static members in static classes
                    if (type.IsSealed && type.IsAbstract)
                    {
                        FieldInfo[] fields = type.GetFields();
                        foreach(var field in fields)
                        {
                            if (field.IsStatic && typeof(ExclusiveGroup).IsAssignableFrom(field.FieldType))
                            {
                                groupFields.Add(new KeyValuePair<string, FieldInfo>($"{type.FullName}.{field.Name}", field));
                            }
                        }
                    }
                    // Groups defined as classes
                    else if (type.BaseType != null
                             && type.BaseType.IsGenericType
                             && type.BaseType.GetGenericTypeDefinition() == typeof(ExclusiveGroup<>))
                    {
                        FieldInfo field = type.GetField("Group",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                        groupFields.Add(new KeyValuePair<string, FieldInfo>(type.FullName, field));
                    }
                }
            }

            groupFields.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

            for (int i = 0; i < groupFields.Count; ++i)
            {
                groupFields[i].Value.GetValue(null);
#if DEBUG
                var group = (ExclusiveGroup) groupFields[i].Value.GetValue(null);
                groupNames[(uint) group] = groupFields[i].Key;
#endif
            }
        }

        static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                Type[] types = assembly.GetTypes();

                return types;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

#if DEBUG
        static string[] groupNames = new string[ushort.MaxValue];
#endif
#endif