namespace BusinessClockApi.Services;

public class StandardBusinessClock(ISystemTime systemTime) : IProvideTheBusinessClock
{

    public bool IsOpen()
    {
        var now = systemTime.GetCurrent();
        var dayOfWeek = now.DayOfWeek;
        var hour = now.Hour;

        var openingTime = new TimeSpan(9, 0, 0);
        var closingTime = new TimeSpan(17, 0, 0);

        var isOpen = dayOfWeek switch
        {
            DayOfWeek.Sunday => false,
            DayOfWeek.Saturday => false,
            _ => hour >= openingTime.Hours && hour < closingTime.Hours
        };

        return isOpen;
    }
}

public interface ISystemTime
{
    DateTime GetCurrent();
}

public class SystemTime : ISystemTime
{
    public DateTime GetCurrent() { return DateTime.Now; }
}