using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BugHouse.Utils.Extensions
{
    public static class HelperExtensions
    {
        private static CultureInfo _culture = CultureInfo.InvariantCulture;
        public static byte[] ToBytes(this string value)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(value);
            return originalBytes;
        }
        public static string ToBase64(this byte[] value)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException(nameof(value));
            return Convert.ToBase64String(value);
        }
        public static byte[] ToBase64Bytes(this string value)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(value);
            return originalBytes;
        }

        public static string ToBase64(this string value)
        {
            byte[] originalBytes = Encoding.UTF8.GetBytes(value);
            return originalBytes.ToBase64();
        }

        public static byte[] ToBase64ToBytes(this string values)
        {
            if (string.IsNullOrEmpty(values))
                throw new ArgumentNullException(nameof(values));

            return Convert.FromBase64String(values);
        }
        public static string ToBase64ToString(this string values)
        {
            if (string.IsNullOrEmpty(values))
                throw new ArgumentNullException(nameof(values));

            byte[] bytes = Convert.FromBase64String(values);
            return Encoding.UTF8.GetString(bytes);
        }


        public static string ToDisplayName(this Enum enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString()).First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return String.IsNullOrWhiteSpace(value);
        }
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            if (value == null)
                return true;

            if (value is ICollection<T> collection)
                return collection.Count == 0;

            if (value is ICollection nonGenericCollection)
                return nonGenericCollection.Count == 0;

            return !value.GetEnumerator().MoveNext();
        }

        public static bool IsNullOrEmpty<T>(this HashSet<T> value)
        {
            if (value == null)
                return true;

            if (value is ICollection<T> collection)
                return collection.Count == 0;

            if (value is ICollection nonGenericCollection)
                return nonGenericCollection.Count == 0;

            return !value.GetEnumerator().MoveNext();
        }


        public static bool IsNull(this object value)
        {
            return value == null;
        }

        #region Converçoes Int
        public static int ToInt(this string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }

        }
        public static int ToInt(this object value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }

        }
        public static int? ToIntNullable(this string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }

        }
        public static int? ToIntNullable(this object value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }

        }
        #endregion


        #region Converçoes Decimal
        public static decimal ToDecimal(this string value)
        {
            try
            {
                value = value.Replace(",", ".");
                return Convert.ToDecimal(value, _culture);
            }
            catch
            {
                return 0;
            }

        }
        public static decimal ToDecimal(this object value)
        {
            try
            {
                return Convert.ToDecimal(value, _culture);
            }
            catch
            {
                return 0;
            }

        }
        public static decimal? ToDecimalNulable(this string value)
        {
            try
            {
                value = value.Replace(",",".");
                return Convert.ToDecimal(value, _culture);
            }
            catch
            {
                return null;
            }

        }
        public static decimal? ToDecimalNulable(this object value)
        {
            try
            {
                return Convert.ToDecimal(value, _culture);
            }
            catch
            {
                return null;
            }

        }
        #endregion

        #region Converçoes Boolean
        public static bool ToBoolean(this object value)
        {
            try
            {
                if (value is int x)
                    return Convert.ToBoolean(x);
                else if (value is string s)
                    if (s.ToIntNullable() != null)
                        return Convert.ToBoolean(s.ToInt());
                    else
                        return Convert.ToBoolean(s);
                else
                    return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }
        public static bool ToBoolean(this string value)
        {
            try
            {
                if (value.ToIntNullable() != null)
                    return Convert.ToBoolean(value.ToInt());
                else
                    return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        public static bool? ToBooleanNulable(this object value)
        {
            try
            {
                if (value is int x)
                    return Convert.ToBoolean(x);
                else if (value is string s)
                    if (s.ToIntNullable() != null)
                        return Convert.ToBoolean(s.ToInt());
                    else
                        return Convert.ToBoolean(s);
                else
                    return Convert.ToBoolean(value);
            }
            catch
            {
                return null;
            }
        }
        public static bool? ToBooleanNulable(this string value)
        {
            try
            {
                if (value.ToIntNullable() != null)
                    return Convert.ToBoolean(value.ToInt());
                else
                    return Convert.ToBoolean(value);
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Converçoes Json

        public static string ToSerialize(this object value)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
        public static T ToDeserialize<T>(this string value)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default(T);
            }
        }
        #endregion


        #region Conversao DateTime
        public static bool IsNullOrEnpty(this DateTime? value)
        {
            if (value is null)
                return true;

            if (!value.HasValue)
                return true;

            if (value == DateTime.MinValue)
                return true;
            return false;
        }

        public static DateTime ToDateDecimal(this decimal value)
        {
            try
            {
                var dataCompleta = value.ToString();

                int dia = dataCompleta.Substring(6, 2).ToInt();
                int mes = dataCompleta.Substring(4, 2).ToInt();
                int ano = dataCompleta.Substring(0, 4).ToInt();

                return new DateTime(ano, mes, dia);
            }
            catch (Exception ex)
            {
                var dataCompleta = value.ToString();
                int dia = dataCompleta.Substring(6, 2).ToInt();
                int mes = dataCompleta.Substring(4, 2).ToInt();
                int ano = dataCompleta.Substring(0, 4).ToInt();
                return new DateTime(ano, mes, 1);
            }
        }

        public static decimal ToDecimalDate(this DateTime? value)
        {
            return value.Value.ToString("yyyyMMdd").ToDecimal();
        }
        public static DateTime ToDateTime(this string value)
        {
            try
            {
                var result = Convert.ToDateTime(value);
                return result;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
        public static DateTime ToDateTime(this object value)
        {
            try
            {
                var result = Convert.ToDateTime(value);
                return result;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime? ToDateTimeNulable(this string value)
        {
            try
            {
                var result = Convert.ToDateTime(value);
                return result;
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? ToDateTimeNulable(this object value)
        {
            try
            {
                var result = Convert.ToDateTime(value);
                return result;
            }
            catch
            {
                return null;
            }
        }

        #endregion


        #region Linq Extensions
        public static Expression<Func<T, bool>> Combine<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            if (left == null && right == null) throw new ArgumentException("At least one argument must not be null");
            if (left == null) return right;
            if (right == null) return left;

            var parameter = Expression.Parameter(typeof(T), "p");
            var combined = new ParameterReplacer(parameter).Visit(Expression.OrElse(left.Body, right.Body));
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            readonly ParameterExpression parameter;

            internal ParameterReplacer(ParameterExpression parameter)
            {
                this.parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return parameter;
            }
        }
    }

    #endregion

}

