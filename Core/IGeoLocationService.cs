using Android.Locations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoMapper.Core
{
	public interface IGeoLocationService
	{
		IList<Address> GeoSearch(string searchAddress, int maxResults);
		Address GeoSearch(string searchAddress);

		Task<IList<Address>> GeoSearchAsync(string searchAddress, int maxResults);
		Task<Address> GeoSearchAsync(string searchAddress);
	}
}

