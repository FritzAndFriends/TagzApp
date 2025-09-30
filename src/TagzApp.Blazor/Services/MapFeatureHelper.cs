using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TagzApp.Blazor.Services;

public class MapFeatureHelper
{

	internal const string SourceName = "TagzApp.InteractiveMap";

	internal static ActivitySource Source = new ActivitySource(SourceName);
	internal static Meter MapMeter = new Meter(SourceName, "1.0.0");
	internal static Histogram<double> MapLookupCount = MapMeter.CreateHistogram<double>("TagzApp.InteractiveMap.Lookup", unit: "Milliseconds", description: "Geocoder requests");


}
