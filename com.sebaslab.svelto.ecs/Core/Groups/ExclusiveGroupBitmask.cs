namespace Svelto.ECS
{
    public enum ExclusiveGroupBitmask : byte
    {
        NONE = 0,
        DISABLED_BIT = 0b00000001,
        UNUSED_BIT_2 = 0b00000010,
        UNUSED_BIT_3 = 0b00000100,
        UNUSED_BIT_4 = 0b00001000,
        UNUSED_BIT_5 = 0b00010000,
        UNUSED_BIT_6 = 0b00100000,
        UNUSED_BIT_7 = 0b01000000,
        UNUSED_BIT_8 = 0b10000000,
    }
}