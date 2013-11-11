using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using Android.Media;
using Xamarin.Media;
using PhotoMapper.Core.Service;
using PhotoMapper.Core.Extension;

namespace PhotoMapper
{
	[Activity(Label = "PhotoMap")]			
	public class PhotoMap : Activity
	{
		private const int ZoomLevel = 15;
		private const int SelectImageCode = 1000;
		private IGeoLocationService _geoLocationService;

		public IGeoLocationService GeoLocationService
		{
			get { return _geoLocationService ?? (_geoLocationService = new GeoLocationService(this)); }
			set { _geoLocationService = value; }
		}

		private IImageService _imageService;

		public IImageService ImageService
		{
			get { return _imageService ?? (_imageService = new ImageService(this)); }
			set { _imageService = value; }
		}

		#region Overrides

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.PhotoMap);

			// Configure the map.
			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map != null)
			{
				map.MapType = GoogleMap.MapTypeNormal;
				map.UiSettings.CompassEnabled = true;
				map.UiSettings.RotateGesturesEnabled = true;
				map.UiSettings.ScrollGesturesEnabled = true;
				map.UiSettings.TiltGesturesEnabled = true;
				map.UiSettings.ZoomControlsEnabled = true;
				map.UiSettings.ZoomGesturesEnabled = true;
			}

			// Attach event handlers to controls.
			Button goToMinneapolisButton = FindViewById<Button>(Resource.Id.GoToMinneapolisButton);
			goToMinneapolisButton.Click += (object sender, EventArgs e) =>
			{
				HandleGoToMinneapolis();
			};

			Button goToAddressButton = FindViewById<Button>(Resource.Id.GoToAddressButton);
			goToAddressButton.Click += (object sender, EventArgs e) =>
			{
				HandleGoToAddress();
			};
			
			Button mapImageButton = FindViewById<Button>(Resource.Id.MapImageButton);
			mapImageButton.Click += (object sender, EventArgs e) =>
			{
				HandleMapImage();
			};
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
		{
			base.OnActivityResult(requestCode, resultCode, intent);

			if (resultCode == Result.Canceled || intent == null)
				return;

			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map == null)
				return;

			switch (requestCode)
			{
				case SelectImageCode:
					MapImage(map, ImageService.GetImagePath(intent.Data), ZoomLevel);
					break;
				default:
					break;
			}
		}

		#endregion

		#region Button Handlers

		private void HandleGoToAddress()
		{
			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map != null)
			{
				var inputControl = new EditText(this);

				new AlertDialog.Builder(this)
					.SetTitle(Resource.String.AddressSearchTitle)
					.SetMessage(Resource.String.AddressSearchMessage)
					.SetView(inputControl)
					.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) =>
					{
						if (!string.IsNullOrWhiteSpace(inputControl.Text))
							ZoomToAddress(map, inputControl.Text, ZoomLevel);
					})
					.SetNegativeButton(Resource.String.Cancel, (object sender, DialogClickEventArgs e) =>
					{
					})
					.Show();
			}
		}

		private void HandleGoToMinneapolis()
		{
			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map != null)
			{
				var location = new LatLng(44.9833, -93.2667);   // Minneapolis latitude / longitude
				map.ZoomToLocation(location, ZoomLevel);
				map.SetMarker(location, "Downtown Minneapolis");
			}
		}

		private void HandleMapImage()
		{
			var picker = new MediaPicker(this);
			if (!picker.PhotosSupported)
			{
				this.DisplayMessage(Resource.String.NoDeviceImageSupportTitle, Resource.String.NoDeviceImageSupportMessage);
				return;
			}

			var imageIntent = new Intent();
			imageIntent.SetType("image/*");
			imageIntent.SetAction(Intent.ActionGetContent);
			StartActivityForResult(Intent.CreateChooser(imageIntent, "Select Image"), SelectImageCode);
		}

		#endregion

		private GoogleMap GetMapFromFragment(int mapFragmentId)
		{
			MapFragment mapFragment = (MapFragment)FragmentManager.FindFragmentById(mapFragmentId);
			return mapFragment.Map;
		}

		private async void ZoomToAddress(GoogleMap map, string searchAddress, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (string.IsNullOrWhiteSpace(searchAddress))
				throw new ArgumentNullException("searchAddress");

			Address address = await GeoLocationService.GeoSearchAsync(searchAddress);
			if (address != null)
			{
				var location = new LatLng(address.Latitude, address.Longitude);
				map.ZoomToLocation(location, zoom);
				map.SetMarker(location, address.GetAddressLine(0) + " " + address.GetAddressLine(1));
			}
			else // Location not found...
			{
				this.DisplayMessage(Resource.String.AddressNotFoundTitle, Resource.String.AddressNotFoundMessage);
			}
		}

		private void MapImage(GoogleMap map, string imagePath, float zoom)
		{
			if (map == null)
				throw new ArgumentNullException("map");
			if (string.IsNullOrWhiteSpace(imagePath))
				throw new ArgumentNullException("imagePath");

			LatLng location = ImageService.GetImageLocation(imagePath);
			if (location != null)
			{
				map.ZoomToLocation(location, zoom);
				map.SetMarker(location, Path.GetFileName(imagePath));
			}
			else // No EXIF geo data present in image...
			{
//				DisplayMessage(Resource.String.NoExifGeoDataInImageTitle, Resource.String.NoExifGeoDataInImageMessage);
				new AlertDialog.Builder(this)
					.SetTitle(Resource.String.NoExifGeoDataInImageTitle)
					.SetMessage(Resource.String.NoExifGeoDataInImagePrompt)
					.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) =>
					{
						LatLng currentLocation = GetCurrentLocation();
						if (currentLocation != null)
						{
							map.ZoomToLocation(currentLocation, zoom);
							map.SetMovableMarker(currentLocation, Path.GetFileName(imagePath), MarkerDragEndHandler);
						}
					})
					.SetNegativeButton(Resource.String.Cancel, (object sender, DialogClickEventArgs e) =>
					{
					})
					.Show();
			}
		}

		private LatLng GetCurrentLocation()
		{
			LocationManager locationManager = GetSystemService(Context.LocationService) as LocationManager;

			// TODO

			LatLng location = null;
			return location;
		}

		private void MarkerDragEndHandler(object sender, GoogleMap.MarkerDragEndEventArgs e)
		{
			// TODO
		}
	}
}

