using TagzApp.LocationMigration;

namespace TagzApp.LocationMigration;

/// <summary>
/// Simple test class to demonstrate location validation
/// Run this with: dotnet run -- --test
/// </summary>
public static class LocationValidationTest
{
	public static void RunTests()
	{
		Console.WriteLine("=== Location Validation Test Results ===\n");

		// Test cases based on the examples you provided
		var testCases = new[]
		{
            // Invalid examples (should be filtered out)
            ("no dotnet-dump", false),
						("same", false),
						("we're getting set", false),
						("it's only a code party if fritz is", false),
						("async await task", false),
						("npm install", false),
						("git commit", false),
						("function method", false),
						("debug build", false),
						("hello world", false),
						("let me check", false),
						("what is this", false),
						("file.cs", false),
						("123456", false),
						("==", false),
						("a", false),
            
            // Valid location examples (should pass)
            ("Paris", true),
						("New York", true),
						("San Francisco", true),
						("Tokyo", true),
						("London", true),
						("Toronto", true),
						("Sydney", true),
						("California", true),
						("Texas", true),
						("United States", true),
						("Canada", true),
						("Germany", true),
						("Chicago", true),
						("Seattle", true),
						("Boston", true),
						("Miami", true),
						("Denver", true),
						("Portland", true),
						("Vancouver", true),
						("Montreal", true),
						("Birmingham", true),
						("Manchester", true),
						("Glasgow", true),
						("Dublin", true),
						("Stockholm", true),
						("Copenhagen", true),
						("Amsterdam", true),
            
            // Edge cases
            ("North Carolina", true), // Contains "north"
            ("South Beach", true),    // Contains "beach"
            ("Mountain View", true),  // Contains "mountain"
            ("River City", true),     // Contains "river" and "city"
            ("Saint Paul", true),     // Contains "saint"
            ("New Orleans", true),    // Contains "new"
            ("Little Rock", true),    // Contains "little"
        };

		int passed = 0;
		int failed = 0;

		foreach (var (testLocation, expectedValid) in testCases)
		{
			bool actualValid = SimpleGeocoder.IsValidLocation(testLocation);
			string result = actualValid == expectedValid ? "✅ PASS" : "❌ FAIL";

			Console.WriteLine($"{result} '{testLocation}' -> Expected: {expectedValid}, Got: {actualValid}");

			if (actualValid == expectedValid)
				passed++;
			else
				failed++;
		}

		Console.WriteLine($"\n=== Summary ===");
		Console.WriteLine($"Total tests: {testCases.Length}");
		Console.WriteLine($"Passed: {passed}");
		Console.WriteLine($"Failed: {failed}");
		Console.WriteLine($"Success rate: {(double)passed / testCases.Length * 100:F1}%");

		if (failed > 0)
		{
			Console.WriteLine($"\n⚠️  Some tests failed. You may want to adjust the validation rules in SimpleGeocoder.IsValidLocation()");
		}
		else
		{
			Console.WriteLine($"\n🎉 All tests passed! Location validation is working correctly.");
		}
	}
}
