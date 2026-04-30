
using AutoMapper;
using Backend.Models;

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

        foreach (var c in codes.Select(x => x.Trim()))
        {
            mask |= c switch
            {
                "Monday" => WeekdayMask.Monday,
                "Tuesday" => WeekdayMask.Tuesday,
                "Wednesday" => WeekdayMask.Wednesday,
                "Thursday" => WeekdayMask.Thursday,
                "Friday" => WeekdayMask.Friday,
                "Saturday" => WeekdayMask.Saturday,
                "Sunday" => WeekdayMask.Sunday,
                _ => throw new ArgumentException($"Invalid weekday code: {c}")
            };
        }

        return (int)mask;
    }

    public static string[] ToCodes(int mask)
    {
        var m = (WeekdayMask)mask;
        var list = new List<string>(7);

        if (m.HasFlag(WeekdayMask.Monday)) list.Add("Monday");
        if (m.HasFlag(WeekdayMask.Tuesday)) list.Add("Tuesday");
        if (m.HasFlag(WeekdayMask.Wednesday)) list.Add("Wednesday");
        if (m.HasFlag(WeekdayMask.Thursday)) list.Add("Thursday");
        if (m.HasFlag(WeekdayMask.Friday)) list.Add("Friday");
        if (m.HasFlag(WeekdayMask.Saturday)) list.Add("Saturday");
        if (m.HasFlag(WeekdayMask.Sunday)) list.Add("Sunday");

        return list.ToArray();
    }

    public static int FromDayOfWeek(DayOfWeek dayOfWeek)
    {
        var mask = WeekdayMask.None;

        mask |= dayOfWeek switch
        {
            DayOfWeek.Monday => WeekdayMask.Monday,
            DayOfWeek.Tuesday => WeekdayMask.Tuesday,
            DayOfWeek.Wednesday => WeekdayMask.Wednesday,
            DayOfWeek.Thursday => WeekdayMask.Thursday,
            DayOfWeek.Friday => WeekdayMask.Friday,
            DayOfWeek.Saturday => WeekdayMask.Saturday,
            DayOfWeek.Sunday => WeekdayMask.Sunday,
            _ => throw new ArgumentException($"Invalid dayOfWeek code: {dayOfWeek}")
        };

        return (int)mask;
    }

    public static List<DayOfWeek> ToDaysOfWeek(int mask)
    {
        var m = (WeekdayMask)mask;
        var list = new List<DayOfWeek>();

        if (m.HasFlag(WeekdayMask.Monday)) list.Add(DayOfWeek.Monday);
        if (m.HasFlag(WeekdayMask.Tuesday)) list.Add(DayOfWeek.Tuesday);
        if (m.HasFlag(WeekdayMask.Wednesday)) list.Add(DayOfWeek.Wednesday);
        if (m.HasFlag(WeekdayMask.Thursday)) list.Add(DayOfWeek.Thursday);
        if (m.HasFlag(WeekdayMask.Friday)) list.Add(DayOfWeek.Friday);
        if (m.HasFlag(WeekdayMask.Saturday)) list.Add(DayOfWeek.Saturday);
        if (m.HasFlag(WeekdayMask.Sunday)) list.Add(DayOfWeek.Sunday);

        return list;
    }
}