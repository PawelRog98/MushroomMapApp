namespace MushroomMapApp.Features.Jobs.Abstraction;

[AttributeUsage(AttributeTargets.Class)]
public class RecurringJobAttribute : Attribute
{
    public string JobName { get; }
    public string CronExpression { get; }
    public string Queue { get; set;  } = "default";
    public RecurringJobAttribute(string jobName, string cronExpression)
    {
        JobName = jobName;
        CronExpression = cronExpression;
    }
}