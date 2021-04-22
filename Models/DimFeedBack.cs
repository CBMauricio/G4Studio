using G4Studio.Utils;
using System;
using System.Collections.Generic;

namespace G4Studio.Models
{
    public class DimFeedback
    {
        public DateTime Date { get; set; }
        public int DimLevel { get; set; }
        public double ACPower { get; set; }
        public double ACCurrent { get; set; }
        public double ACCPowerFactor { get; set; }
        public double EnergyConsumption { get; set; }

        public DimFeedback() : this(new DateTime(), 0, 0, 0, 0, 0)
        { }

        public DimFeedback(DateTime date, int dimLevel, double energyConsumption, double acPower, double acCurrent, double acPowerFactor)
        {
            Date = date;
            DimLevel = dimLevel;
            EnergyConsumption = energyConsumption;
            ACPower = acPower;
            ACCurrent = acCurrent;
            ACCPowerFactor = acPowerFactor;
        }

        public static DimFeedback GetDimFeedback(DateTime initDate)
        {
            return new DimFeedback() { Date = initDate, DimLevel = 100, EnergyConsumption = 0, ACPower = 85.8, ACCurrent = 0.379, ACCPowerFactor = 1 };
        }

        public static List<DimFeedback> GetDimFeedbacks(DateTime initDate, double latitude, double longitude, bool usesTimeZoneHandler)
        {
            List<DimFeedback> dimFeedbacks = new List<DimFeedback>();

            switch (initDate.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                    dimFeedbacks.AddRange(GetDimFeedbackForWeekend(initDate, latitude, longitude, usesTimeZoneHandler));
                    break;
                case DayOfWeek.Sunday:
                    dimFeedbacks.AddRange(GetDimFeedbackForWeekend(initDate, latitude, longitude, usesTimeZoneHandler));
                    break;
                default:
                    dimFeedbacks.AddRange(GetDimFeedbackForWeekdays(initDate, latitude, longitude, usesTimeZoneHandler));
                    break;
            }

            return dimFeedbacks;
        }

        private static List<DimFeedback> GetDimFeedbackForWeekend(DateTime initDate, double latitude, double longitude, bool usesTimeZoneHandler)
        {
            DateTime sunset = usesTimeZoneHandler ? TimeZoneHandler.Sunset(latitude, longitude, initDate) : new DateTime(initDate.Year, initDate.Month, initDate.Day, 18, 01, 00);
            DateTime midnight = new DateTime(initDate.Year, initDate.Month, initDate.Day, 00, 00, 00).AddDays(1);
            DateTime six = new DateTime(initDate.Year, initDate.Month, initDate.Day, 06, 00, 00).AddDays(1);
            DateTime sunrise = usesTimeZoneHandler ? TimeZoneHandler.Sunrise(latitude, longitude, initDate.AddDays(1)) : new DateTime(initDate.Year, initDate.Month, initDate.Day, 7, 01, 00).AddDays(1);

            DateTime date3 = six <= sunrise ? six : sunrise;
            DateTime date4 = six <= sunrise ? sunrise : six;

            double hoursFromSunsetTillMidnight = (midnight - sunset).TotalMinutes / 60;
            double hoursFromMidnightTillDate3 = (date3 - midnight).TotalMinutes / 60;
            double hoursFromDate3TillDate4 = (date4 - date3).TotalMinutes / 60;

            List<DimFeedback> dimFeedbacks = new List<DimFeedback>();            

            dimFeedbacks.Add(new DimFeedback() { Date = sunset, DimLevel = 100, EnergyConsumption = 0, ACPower = 85.8, ACCurrent = 0.379, ACCPowerFactor = 1 });            
            dimFeedbacks.Add(new DimFeedback() { Date = midnight, DimLevel = 70, EnergyConsumption = Math.Round(0.0858 * hoursFromSunsetTillMidnight, 2), ACPower = 85.8 * 0.7, ACCurrent = 0.379 * 0.7, ACCPowerFactor = 0.379 * 0.7 });
            dimFeedbacks.Add(new DimFeedback() { Date = date3, DimLevel = 100, EnergyConsumption = Math.Round(0.0858 * 0.7 * hoursFromMidnightTillDate3, 2), ACPower = 85.8, ACCurrent = 0.379, ACCPowerFactor = 1.0 });
            dimFeedbacks.Add(new DimFeedback() { Date = date4, DimLevel = 0, EnergyConsumption = Math.Round(0.0858 * hoursFromDate3TillDate4, 2), ACPower = 0.0, ACCurrent = 0.0, ACCPowerFactor = 0.0 });

            return dimFeedbacks;
        }

        private static List<DimFeedback> GetDimFeedbackForWeekdays(DateTime initDate, double latitude, double longitude, bool usesTimeZoneHandler)
        {
            DateTime sunset = usesTimeZoneHandler ?  TimeZoneHandler.Sunset(latitude, longitude, initDate) : new DateTime(initDate.Year, initDate.Month, initDate.Day, 18, 01, 00);
            DateTime twentytwo = new DateTime(initDate.Year, initDate.Month, initDate.Day, 22, 00, 00);
            DateTime five = new DateTime(initDate.Year, initDate.Month, initDate.Day, 05, 00, 00).AddDays(1);
            DateTime sunrise = usesTimeZoneHandler ?  TimeZoneHandler.Sunrise(latitude, longitude, initDate.AddDays(1)) : new DateTime(initDate.Year, initDate.Month, initDate.Day, 7, 01, 00).AddDays(1);

            DateTime date3 = five <= sunrise ? five : sunrise;
            DateTime date4 = five <= sunrise ? sunrise : five;

            double hoursFromSunsetTillTwentyTwo = (twentytwo - sunset).TotalMinutes / 60;
            double hoursFromTwentyTwoTillDate3 = (date3 - twentytwo).TotalMinutes / 60;
            double hoursFromDate3TillDate4 = (date4 - date3).TotalMinutes / 60;

            List<DimFeedback> dimFeedbacks = new List<DimFeedback>();

            dimFeedbacks.Add(new DimFeedback() { Date = sunset, DimLevel = 100, EnergyConsumption = 0, ACPower = 85.8, ACCurrent = 0.379, ACCPowerFactor = 1 });
            dimFeedbacks.Add(new DimFeedback() { Date = twentytwo, DimLevel = 70, EnergyConsumption = Math.Round(0.0858 * hoursFromSunsetTillTwentyTwo, 2), ACPower = 85.8 * 0.7, ACCurrent = 0.379 * 0.7, ACCPowerFactor = 0.379 * 0.7 });
            dimFeedbacks.Add(new DimFeedback() { Date = date3, DimLevel = 100, EnergyConsumption = Math.Round(0.0858 * 0.7 * hoursFromTwentyTwoTillDate3, 2), ACPower = 85.8, ACCurrent = 0.379, ACCPowerFactor = 1.0 });
            dimFeedbacks.Add(new DimFeedback() { Date = date4, DimLevel = 0, EnergyConsumption = Math.Round(0.0858 * hoursFromDate3TillDate4, 2), ACPower = 0.0, ACCurrent = 0.0, ACCPowerFactor = 0.0 });

            return dimFeedbacks;
        }

        private static int GetDimLevelByHour(int hour)
        {
            if (hour < 3) return 75;
            if (hour < 6) return 50;
            if (hour < 18) return 0;
            if (hour < 21) return 25;

            return 100;
        }

        public static double GetTotalConsumptionByNDays(int dayOfTheYear)
        {
            double totalConsumption = 0;

            for (int i = 0; i <= dayOfTheYear; i++)
            {
                totalConsumption += GetDimLevelIncByWeekDay(i % 7);
            }

            return totalConsumption;
        }

        private static double GetDimLevelIncByWeekDay(int day)
        {
            double inc;

            switch (day)
            {
                case 0:
                    inc = 1.68;
                    break;
                case 1:
                    inc = 1.63;
                    break;
                case 2:
                    inc = 1.31;
                    break;
                case 3:
                    inc = 1.49;
                    break;
                case 4:
                    inc = 1.73;
                    break;
                case 7:
                    inc = 1.53;
                    break;
                default:
                    inc = 1.18;
                    break;
            }

            return inc;
        }
    }
}
