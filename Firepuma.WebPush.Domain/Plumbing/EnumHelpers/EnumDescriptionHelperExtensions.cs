using System.ComponentModel;
using System.Globalization;

namespace Firepuma.WebPush.Domain.Plumbing.EnumHelpers;

public static class EnumDescriptionHelperExtensions
{
    public static string? GetEnumDescriptionOrNull<T>(this T e) where T : Enum, IConvertible
    {
        var type = e.GetType();
        var values = Enum.GetValues(type);

        foreach (int val in values)
        {
            var enumName = type.GetEnumName(val);
            if (val == e.ToInt32(CultureInfo.InvariantCulture) && enumName != null)
            {
                var memInfo = type.GetMember(enumName);

                if (memInfo[0]
                        .GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                {
                    return descriptionAttribute.Description;
                }
            }
        }

        return null;
    }
}