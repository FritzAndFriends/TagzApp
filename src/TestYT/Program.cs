using Google.Apis.Services;
using Google.Apis.YouTube.v3;

await GetYouTubeVideos();

async Task GetYouTubeVideos()
{
	var youtubeService = new YouTubeService(new BaseClientService.Initializer()
	{
		ApiKey = "AIzaSyD-zImnv2v_RMqUhYb4YJnzO6vKfIqaUR8",
		ApplicationName = "Test Fritz"
	});
	var searchListRequest = youtubeService.Search.List("snippet");
	searchListRequest.Q = ".NET Conf 2024 - Day";
	searchListRequest.ChannelId = "UCvtT19MZW8dq5Wwfu6B0oxw";
	//searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Upcoming | SearchResource.ListRequest.EventTypeEnum.Live;
	searchListRequest.Type = "live";
	searchListRequest.MaxResults = 50;
	// Call the search.list method to retrieve results matching the specified query term.
	var searchListResponse = searchListRequest.Execute();
	List<string> videos = new List<string>();
	List<string> channels = new List<string>();
	List<string> playlists = new List<string>();
	// Add each result to the appropriate list, and then display the lists of
	// matching videos, channels, and playlists.
	foreach (var searchResult in searchListResponse.Items)
	{
		channels.Add(string.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id));

		if (searchResult.Id?.VideoId is null) continue;

		// Get live chat ID
		var videoRequest = youtubeService.Videos.List("liveStreamingDetails");
		videoRequest.Id = searchResult.Id.VideoId;
		var videoResponse = await videoRequest.ExecuteAsync();
		foreach (var video in videoResponse.Items)
		{
			var liveStreamingDetails = video.LiveStreamingDetails;
			if (liveStreamingDetails != null && liveStreamingDetails.ActiveLiveChatId != null)
			{
				Console.WriteLine($"Live Chat ID: {liveStreamingDetails.ActiveLiveChatId}");
			}
		}
	}

	Console.WriteLine(string.Format("Channels:\n{0}\n", string.Join("\n", channels)));
	//Console.WriteLine(string.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));
}
