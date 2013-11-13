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
using Xamarin.Geolocation;
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
		private LatLng _markerLocation = null;
		private string _imageToLocatePath;

		#region Services

		private IGeoLocationService _geoLocationService = null;
		public IGeoLocationService GeoLocationService
		{
			get { return _geoLocationService ?? (_geoLocationService = new GeoLocationService(this)); }
			set { _geoLocationService = value; }
		}

		private IImageService _imageService = null;
		public IImageService ImageService
		{
			get { return _imageService ?? (_imageService = new ImageService(this)); }
			set { _imageService = value; }
		}

		#endregion

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
			Button goToCurrentLocationButton = FindViewById<Button>(Resource.Id.GoToCurrentLocationButton);
			goToCurrentLocationButton.Click += (object sender, EventArgs e) =>
			{
				HandleGoToCurrentLocation();
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

			Button selectLocationButton = FindViewById<Button>(Resource.Id.SelectLocationButton);
			selectLocationButton.Visibility = ViewStates.Invisible;
			selectLocationButton.Click += (object sender, EventArgs e) => 
			{
				HandleSelectLocation();
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

		private async void HandleGoToCurrentLocation()
		{
			GoogleMap map = GetMapFromFragment(Resource.Id.PhotoMapFragment);
			if (map != null)
			{
				LatLng currentLocation = await GeoLocationService.GetCurrentLocationAsync();
				if (currentLocation != null)
				{
					map.ZoomToLocation(currentLocation, ZoomLevel);
					map.SetMarker(currentLocation, "Current Location");
				}
			}
		}

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

		private void HandleSelectLocation()
		{
			if (string.IsNullOrWhiteSpace(_imageToLocatePath))
				throw new ApplicationException("Image to locate not identified.");
			if (_markerLocation == null)
				throw new ApplicationException("Location not marked on map.");

			if (!ImageService.SetImageLocation(_imageToLocatePath, _markerLocation))
				this.DisplayMessage(Resource.String.SetImageLocationFailedTitle, Resource.String.SetImageLocationFailedMessage);
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
				new AlertDialog.Builder(this)
					.SetTitle(Resource.String.NoExifGeoDataInImageTitle)
					.SetMessage(Resource.String.NoExifGeoDataInImagePrompt)
					.SetPositiveButton(Resource.String.Okay, (object sender, DialogClickEventArgs e) =>
					{
						PlaceMovableImageMarker(map, imagePath, zoom);
					})
					.SetNegativeButton(Resource.String.Cancel, (object sender, DialogClickEventArgs e) =>
					{
					})
					.Show();
			}
		}

		private async void PlaceMovableImageMarker(GoogleMap map, string imagePath, float zoom)
		{
			if (GeoLocationService.IsGeolocationEnabled)
			{
				LatLng currentLocation = await GeoLocationService.GetCurrentLocationAsync();
				if (currentLocation != null)
				{
					_imageToLocatePath = imagePath;
					_markerLocation = currentLocation;

					map.ZoomToLocation(currentLocation, zoom);
					map.SetMovableMarker(currentLocation, Path.GetFileName(imagePath), MarkerDragEndHandler);
					Button selectLocationButton = FindViewById<Button>(Resource.Id.SelectLocationButton);
					selectLocationButton.Visibility = ViewStates.Visible;
				}
			}
			else
			{
				this.DisplayMessage(Resource.String.NoGeoEnabledTitle, Resource.String.NoGeoEnabledMessage);
			}
		}

		private void MarkerDragEndHandler(object sender, GoogleMap.MarkerDragEndEventArgs e)
		{
			_markerLocation = e.P0.Position;
		}
	}
}

