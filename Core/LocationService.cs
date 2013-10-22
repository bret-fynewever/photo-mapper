using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace PhotoMapper.Core
{
	public class LocationService
	{
		private Context _context;

		public LocationService(Context context)
		{
			_context = context;
		}

		public IList<Address> GeoSearch(string searchAddress, int maxResults)
		{
			Geocoder geocoder = new Geocoder(_context);
			return geocoder.GetFromLocationName(searchAddress, maxResults);
		}

		public async Task<IList<Address>> GeoSearchAsync(string searchAddress, int maxResults)
		{
			Geocoder geocoder = new Geocoder(_context);
			IList<Address> addresses = await geocoder.GetFromLocationNameAsync(searchAddress, maxResults);
			return addresses;
		}

		public Address GeoSearch(string searchAddress)
		{
			IList<Address> addresses = GeoSearch(searchAddress, 1);
			if (addresses != null && addresses.Count > 0)
				return addresses[0];

			return null;
		}

		public async Task<Address> GeoSearchAsync(string searchAddress)
		{
			IList<Address> addresses = await GeoSearchAsync(searchAddress, 1);
			if (addresses != null && addresses.Count > 0)
				return addresses[0];

			return null;
		}
	}
}

