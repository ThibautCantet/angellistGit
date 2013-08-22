using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tools.Extensions
{
    public static class DateTimeExtensions
	{
		/// <summary>
		/// Convert a DateTime to the amount a second ellapsed since UNIX epoch
		/// </summary>
		/// <param name="date">The DateTime to convert</param>
		/// <returns>The amount a second ellapsed since UNIX epoch</returns>
		public static int ToTimestamp(this DateTime date)
		{
			TimeSpan t = (date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			int timestamp = (int)t.TotalSeconds;

			return timestamp;
		}

		/// <summary>
		/// Give the UTC DateTime based on a timestamp (epoch : 1970-01-01)
		/// </summary>
		/// <param name="timestamp">The timestamp to convert</param>
		/// <returns>The UTC DateTime</returns>
		public static DateTime FromTimestampToDateTime(this int timestamp)
		{
			DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return date.AddSeconds(timestamp);
		}

		/// <summary>
		/// Return the current age of a person based on its birth date. Prefer the DateTimeOffset version 
		/// </summary>
		/// <param name="birthDate">The person birthdate</param>
		/// <returns>The age</returns>
		public static int GetAgeFromBirthDate(this DateTime birthDate)
		{
			DateTime now = DateTime.UtcNow;
			DateTime birthDateUtc = birthDate.ToUniversalTime();

			int adjust = now.Month > birthDateUtc.Month || (now.Month == birthDateUtc.Month && now.Day >= birthDateUtc.Day) ? 0 : 1;
			return now.Year - birthDateUtc.Year - adjust;
		}

		/// <summary>
		/// Convert a DateTime to the amount a second ellapsed since UNIX epoch
		/// </summary>
		/// <param name="date">The DateTime to convert</param>
		/// <returns>The amount a second ellapsed since UNIX epoch</returns>
		public static int ToTimestamp(this DateTimeOffset date)
		{
			TimeSpan t = (date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero));
			int timestamp = (int)t.TotalSeconds;

			return timestamp;
		}

		/// <summary>
		/// Give the UTC DateTime based on a timestamp (epoch : 1970-01-01)
		/// </summary>
		/// <param name="timestamp">The timestamp to convert</param>
		/// <returns>The UTC DateTime</returns>
		public static DateTimeOffset FromTimestampToDateTimeOffset(this int timestamp)
		{
			DateTimeOffset date = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
			return date.AddSeconds(timestamp);
		}

		/// <summary>
		/// Return the current age of a person based on its birth date 
		/// </summary>
		/// <param name="birthDate">The person birthdate</param>
		/// <returns>The age</returns>
		public static int GetAgeFromBirthDate(this DateTimeOffset birthDate)
		{
			DateTimeOffset now = DateTime.UtcNow;
			DateTimeOffset birthDateUtc = birthDate.ToUniversalTime();

			int adjust = now.Month > birthDateUtc.Month || (now.Month == birthDateUtc.Month && now.Day >= birthDateUtc.Day) ? 0 : 1;
			return now.Year - birthDateUtc.Year - adjust;
		}

        public static bool IsSameDay(this DateTime date, DateTime compareTo)
        {
            return date.Day == compareTo.Day && date.Month == compareTo.Month && date.Year == compareTo.Year;
        }

		/// <summary>
		/// Keep a TimeSpan only to the second
		/// </summary>
		/// <param name="time">The base time</param>
		/// <returns>The new TimeSpan HH:MM:SS</returns>
		public static TimeSpan RoundToSecond(this TimeSpan time)
		{
			long ms;
			Math.DivRem(time.Ticks, (long)10E6, out ms);
			return new TimeSpan(time.Ticks - ms);
		}
    }
}
