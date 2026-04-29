
using AutoMapper;

namespace Backend.Mapping;

public sealed class WeekdayCodesToMaskConverter : IValueConverter<string[], int>
{
    public int Convert(string[] sourceMember, ResolutionContext context) =>
        WeekdayMaskConverter.FromCodes(sourceMember ?? Array.Empty<string>());
}

public sealed class MaskToWeekdayCodesConverter : IValueConverter<int, string[]>
{
    public string[] Convert(int sourceMember, ResolutionContext context) =>
        WeekdayMaskConverter.ToCodes(sourceMember);
}

public static class WeekdayMaskConverter
{
    public static int FromCodes(IEnumerable<string> codes)
    {
        var mask = WeekdayMask.None;

        foreach (var c in codes.Select(x => x.Trim().ToUpperInvariant()))
        {
            mask |= c switch
            {
                "MONDAY" => WeekdayMask.Monday,
                "TUESDAY" => WeekdayMask.Tuesday,
                "WEDNESDAY" => WeekdayMask.Wednesday,
                "THURSDAY" => WeekdayMask.Thursday,
                "FRIDAY" => WeekdayMask.Friday,
                "SATURDAY" => WeekdayMask.Saturday,
                "SUNDAY" => WeekdayMask.Sunday,
                _ => throw new ArgumentException($"Invalid weekday code: {c}")
            };
        }

        return (int)mask;
    }

    public static string[] ToCodes(int mask)
    {
        var m = (WeekdayMask)mask;
        var list = new List<string>(7);

        if (m.HasFlag(WeekdayMask.Monday)) list.Add("MONDAY");
        if (m.HasFlag(WeekdayMask.Tuesday)) list.Add("TUESDAY");
        if (m.HasFlag(WeekdayMask.Wednesday)) list.Add("WEDNESDAY");
        if (m.HasFlag(WeekdayMask.Thursday)) list.Add("THURSDAY");
        if (m.HasFlag(WeekdayMask.Friday)) list.Add("FRIDAY");
        if (m.HasFlag(WeekdayMask.Saturday)) list.Add("SATURDAY");
        if (m.HasFlag(WeekdayMask.Sunday)) list.Add("SUNDAY");

        return list.ToArray();
    }
}