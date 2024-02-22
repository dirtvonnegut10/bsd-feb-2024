using BusinessClockApi.Services;
using NSubstitute;

namespace BusinessClockApi.UnitTests;
public class StandardBusinessClockTests
{
    [Fact]
    public void ClosedOnSaturday()
    {
        var fakeClock = Substitute.For<ISystemTime>();
        fakeClock.GetCurrent().Returns(new DateTime(2024, 2, 17, 10, 00, 00));

        var clock = new StandardBusinessClock(fakeClock);

        Assert.False(clock.IsOpen());
    }

    [Fact]
    public void ClosedOnSunday()
    {
        var fakeClock = Substitute.For<ISystemTime>();
        fakeClock.GetCurrent().Returns(new DateTime(2024, 2, 18, 10, 00, 00));

        var clock = new StandardBusinessClock(fakeClock);


        Assert.False(clock.IsOpen());
    }

    [Theory]
    [InlineData("2/20/2023 16:25:00")]
    [InlineData("2/20/2023 9:00:00")]
    public void WeAreOpen(string dateTime)
    {
        var dateToUse = DateTime.Parse(dateTime);
        var fakeClock = Substitute.For<ISystemTime>();
        fakeClock.GetCurrent().Returns(dateToUse);
        var clock = new StandardBusinessClock(fakeClock);
        Assert.True(clock.IsOpen());
    }

    [Theory]
    [InlineData("2/20/2023 17:00:00")]
    [InlineData("2/20/2023 8:59:59")]
    public void WeAreClosed(string dateTime)
    {
        var dateToUse = DateTime.Parse(dateTime);
        var fakeClock = Substitute.For<ISystemTime>();
        fakeClock.GetCurrent().Returns(dateToUse);
        var clock = new StandardBusinessClock(fakeClock);
        Assert.False(clock.IsOpen());
    }
}