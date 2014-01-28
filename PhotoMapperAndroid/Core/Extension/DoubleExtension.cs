using System;

namespace PhotoMapperAndroid.Core.Extension
{
	public static class DoubleExtension
	{
		public static string ToDMS(this double coordinate)
		{
			double degreesCoordinate = Math.Abs(coordinate);
			int degrees = (int)degreesCoordinate;

			double minutesCoordinate = (degreesCoordinate - degrees) * 60;
			int minutes = (int)minutesCoordinate;

			int seconds = (int)((minutesCoordinate - minutes) * 60000);

			return degrees.ToString() + "/1," + minutes.ToString() + "/1," + seconds.ToString() + "/1000";
		}

		public static string ToLatitudeReference(this double latitude)
		{
			return latitude < 0d ? "S" : "N";
		}

		public static string ToLongitudeReference(this double longitude)
		{
			return longitude < 0d ? "W" : "E";
		}
	}
}
