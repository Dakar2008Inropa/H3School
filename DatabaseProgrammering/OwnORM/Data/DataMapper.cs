using System.Data;
using System.Globalization;
using System.Reflection;

namespace OwnORM.Data
{
    public static class DataMapper
    {
        public static T MapRecord<T>(IDataRecord record) where T : new()
        {
            if (record == null)
                throw new ArgumentException(nameof(record));

            T instance = new T();

            Dictionary<string, int> columns = GetColumnOrdinals(record);

            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite)
                    continue;

                string name = prop.Name.ToLower(CultureInfo.InvariantCulture);
                if (!columns.ContainsKey(name))
                    continue;

                int ordinal = columns[name];
                object raw = record.GetValue(ordinal);

                object value = CoercValue(prop.PropertyType, raw);

                prop.SetValue(instance, value);
            }

            return instance;
        }

        private static Dictionary<string, int> GetColumnOrdinals(IDataRecord record)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < record.FieldCount; i++)
            {
                string name = record.GetName(i);
                dict[name] = i;
            }
            return dict;
        }

        private static object CoercValue(Type targetType, object raw)
        {
            if (raw == null || raw is DBNull)
            {
                if (targetType == typeof(string))
                    return string.Empty;

                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);

                return null;
            }

            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsEnum)
                return Enum.ToObject(underlying, raw);

            if (underlying == typeof(Guid))
                return raw is Guid ? raw : Guid.Parse(raw.ToString());

            if (underlying == typeof(DateTime))
                return raw is DateTime ? raw : DateTime.Parse(raw.ToString(), CultureInfo.InvariantCulture);

            if (underlying == typeof(bool))
                return Convert.ToBoolean(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(int))
                return Convert.ToInt32(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(long))
                return Convert.ToInt64(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(decimal))
                return Convert.ToDecimal(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(double))
                return Convert.ToDouble(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(float))
                return Convert.ToSingle(raw, CultureInfo.InvariantCulture);

            if (underlying == typeof(string))
                return raw.ToString();

            return Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
        }
    }
}