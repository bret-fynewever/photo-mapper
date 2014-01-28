using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using Android.Locations;

namespace PhotoMapper.Core.Service
{
	public interface IGeoLocationService
	{
		#region GeoSearch

		IList<Address> GeoSearch(string searchAddress, int maxResults);
		Address GeoSearch(string searchAddress);

		Task<IList<Address>> GeoSearchAsync(string searchAddress, int maxResults);
		Task<Address> GeoSearchAsync(string searchAddress);

		#endregion

		#region GeoLocate

		double GeoAccuracy { get; set; }
		bool IsGeolocationEnabled { get; }
		Task<LatLng> GetCurrentLocationAsync();

		#endregion
	}
}

