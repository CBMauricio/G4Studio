using System;
using System.Collections.Generic;

namespace G4Studio.Models
{
    public class AlarmFeedback
    {
        public DateTime Date { get; set; }
        public int Type { get; set; }

        public AlarmFeedback()
            : this(new DateTime(), 0)
        { }

        public AlarmFeedback(DateTime date, int type)
        {
            Date = date;
            Type = type;
        }

        public static List<AlarmFeedback> GetAlarmFeedbacks(DateTime initDate, long nMessagesPerDay, double workingHours)
        {
            List<AlarmFeedback> alarmFeedbacks = new List<AlarmFeedback>();

            nMessagesPerDay = Math.Max(2, nMessagesPerDay);
            double incMinutes = workingHours * 60 / (nMessagesPerDay - 1);

            for (int i = 0; i < nMessagesPerDay; i++)
            {
                DateTime date = initDate.AddMinutes(incMinutes * i);
                int alarmType = GetAlarmTypeByHour(date.Hour);

                alarmFeedbacks.Add(new AlarmFeedback() { Date = date, Type = alarmType });
            }

            return alarmFeedbacks;
        }



        private static int GetAlarmTypeByHour(int hour)
        {
            if (hour < 3) return 0;
            if (hour < 6) return 2;

            return 1;
        }
    }
}
