using System;

[Serializable]
public class DateTimeData
{
    public long ticks;

    public DateTimeData(DateTime dateTime)
    {
        ticks = dateTime.Ticks;
    }

    public DateTime ToDateTime()
    {
        return new DateTime(ticks);
    }
}
