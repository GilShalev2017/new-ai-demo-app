namespace Server.Models.AiJobs
{
    namespace Server.Models.AiJobs
    {
        public enum RuleRecurrenceEnum
        {
            None = 0,
            Once,
            Recurring,
            Continuous
        }

        public static class DayOfWeekHelper
        {
            public static int ToBinary(DayOfWeek dayOfWeek)
            {
                return dayOfWeek switch
                {
                    DayOfWeek.Monday => 0b1000000,
                    DayOfWeek.Tuesday => 0b0100000,
                    DayOfWeek.Wednesday => 0b0010000,
                    DayOfWeek.Thursday => 0b0001000,
                    DayOfWeek.Friday => 0b0000100,
                    DayOfWeek.Saturday => 0b0000010,
                    DayOfWeek.Sunday => 0b0000001,
                    _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek))
                };
            }
        }

        public class Rule
        {
            public RuleRecurrenceEnum Recurrence { get; set; } = RuleRecurrenceEnum.Once;

            // 7 bits: Mon → Sun
            public int Days { get; set; } = 0b1111111;

            public bool IsActiveOnDay(DateTime dateTimeToCheck)
            {
                if (Days == 0) // never
                    return false;

                int dayBit = DayOfWeekHelper.ToBinary(dateTimeToCheck.DayOfWeek);

                if ((dayBit & Days) == 0)
                    return false;

                return true;
            }
        }
    }
}
