namespace Molten
{
    public static class EnumHelper
    {
        public static T GetLastValue<T>() where T : struct, IConvertible
        {
            Type t = typeof(T);

            if (t.IsEnum == false)
                throw new InvalidOperationException("Type must be an enum");

            return Enum.GetValues(t).Cast<T>().Last();
        }
    }
}
