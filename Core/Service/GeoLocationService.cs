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
using Android.Gms.Maps.Model;
using Xamarin.Geolocation;

namespace PhotoMapper.Core.Service
{
	public class GeoLocationService : IGeoLocationService
	{
		private Context _context;

		public GeoLocationService(Context context)
		{
			if (context == null)
				throw new ArgumentNullException("context");

			_context = context;
		}

		#region GeoSearch

		private Geocoder _geocoder;
		protected Geocoder Geocoder
		{
			get { return _geocoder ?? (_geocoder = new Geocoder(_context)); }
		}

		public IList<Address> GeoSearch(string searchAddress, int maxResults)
		{
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			return Geocoder.GetFromLocationName(searchAddress, maxResults);
		}

		public Address GeoSearch(string searchAddress)
		{
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			IList<Address> addresses = GeoSearch(searchAddress, 1);
			if (addresses != null)
				return addresses.FirstOrDefault();

			return null;
		}

		public async Task<IList<Address>> GeoSearchAsync(string searchAddress, int maxResults)
		{
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			IList<Address> addresses = await Geocoder.GetFromLocationNameAsync(searchAddress, maxResults);
			return addresses;
		}

		public async Task<Address> GeoSearchAsync(string searchAddress)
		{
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			IList<Address> addresses = await GeoSearchAsync(searchAddress, 1);
			if (addresses != null)
				return addresses.FirstOrDefault();

			return null;
		}

		#endregion

		#region GeoLocate

		private Geolocator _geolocator;
		protected Geolocator Geolocator
		{
			get { return _geolocator ?? (_geolocator = new Geolocator(_context) { DesiredAccuracy = GeoAccuracy }); }
		}

		private double _geoAccuracy = 50;
		public double GeoAccuracy
		{
			get { return _geoAccuracy; }
			set
			{
				_geoAccuracy = value;
				Geolocator.DesiredAccuracy = value;
			}
		}

		public bool IsGeolocationEnabled
		{
			get { return Geolocator.IsGeolocationEnabled; }
		}

		public async Task<LatLng> GetCurrentLocationAsync()
		{
			LatLng location = null;

			if (Geolocator.IsGeolocationEnabled)
			{
				Position position = await Geolocator.GetPositionAsync(timeout: 10000);
				location = position == null ? null : new LatLng(position.Latitude, position.Longitude);
			}

			return location;
		}

		#endregion
	}
}

