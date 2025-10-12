# Reddit AMA Provider - User Configuration Guide

## Overview

The Reddit AMA Provider allows TagzApp to monitor a specific Reddit post (such as an AMA thread) for new comments in real-time. This is perfect for live events, Q&A sessions, product launches, and community discussions where you want to display Reddit activity on TagzApp's waterfall display.

## Prerequisites

- TagzApp admin access
- A specific Reddit post URL you want to monitor
- The Reddit post must be publicly accessible (not in a private subreddit)

## Step-by-Step Configuration

### 1. Access Provider Configuration

1. Log in to TagzApp as an administrator
2. Navigate to **Admin** → **Providers** → **Reddit AMA**
3. You'll see the Reddit AMA configuration panel

### 2. Basic Setup

#### Enable the Provider
- Check the **"Enable Provider"** checkbox
- This activates the Reddit AMA monitoring service

#### Configure the Reddit Post
1. **Find your Reddit post URL**
   - Example: `https://reddit.com/r/IAmA/comments/abc123/i_am_a_software_developer_ama/`
   - Copy the complete URL from your browser

2. **Paste the URL**
   - Paste the full Reddit URL into the **"Reddit Post URL"** field
   - TagzApp will automatically parse the URL and fill in:
     - **Subreddit**: (e.g., "IAmA")
     - **Post ID**: (e.g., "abc123") 
     - **Post Title**: (fetched automatically)

### 3. Advanced Settings

#### Refresh Rate
- **Refresh Interval**: How often TagzApp checks for new comments (15-300 seconds)
- **Recommended**: 30 seconds for active AMAs, 60+ seconds for slower discussions
- **Note**: Lower values check more frequently but use more API calls

#### Comment Filtering
- **Max Comments Per Check**: Limits how many comments to fetch each time (10-500)
- **Include Replies**: Whether to show replies to comments (recommended: Yes)
- **Hide Deleted Comments**: Filter out [deleted] and [removed] comments (recommended: Yes)
- **Minimum Comment Score**: Hide heavily downvoted comments (recommended: -10)

#### User Management
- **Blocked Authors**: List usernames (one per line) to exclude from the feed
- **Use for**: Blocking spam accounts, bots, or inappropriate users

### 4. Test Your Configuration

1. Click **"Test Connection"** to verify:
   - Reddit post exists and is accessible
   - API connection is working
   - Comments can be fetched successfully

2. **Expected Results**:
   - ✅ "Connection successful, found X comments"
   - ❌ "Post not found" - Check your URL
   - ❌ "Subreddit is private" - Use a public subreddit

### 5. Save and Activate

1. Click **"Save Configuration"**
2. The provider will start monitoring within 30 seconds
3. Check the **Provider Status** panel to confirm it's running

## Using the Provider

### Creating a TagzApp "Hashtag"

The Reddit AMA provider doesn't use traditional hashtags. Instead, you create an event identifier:

1. **Go to TagzApp main page**
2. **Search for a custom tag**, such as:
   - `"myama2024"` - For your specific AMA event
   - `"productlaunch"` - For a product announcement thread
   - `"liveqa"` - For a Q&A session
   - `"conference2024"` - For a conference discussion thread

3. **All comments from your monitored Reddit thread** will appear under this tag

### Viewing the Results

Comments will appear in TagzApp's waterfall display with:
- **Author**: Reddit username
- **Content**: Full comment text
- **Timestamp**: When the comment was posted
- **Link**: Direct link to the comment on Reddit
- **Score**: Reddit upvote/downvote score (if enabled)
- **OP Badge**: Special indicator for comments from the original poster

## Common Use Cases

### 1. Live AMA Events
**Scenario**: You're hosting an "Ask Me Anything" session

**Setup**:
1. Create your AMA post on Reddit (r/IAmA or relevant subreddit)
2. Configure TagzApp to monitor that specific post
3. Create a hashtag like `"myama2024"`
4. Display TagzApp on OBS/streaming software

**Result**: Questions appear in real-time during your live stream

### 2. Product Launch Q&A
**Scenario**: Announcing a new product with Reddit discussion

**Setup**:
1. Post product announcement on relevant subreddit
2. Monitor the thread for feedback and questions
3. Use hashtag like `"productlaunch"`

**Result**: Real-time community reactions and questions

### 3. Conference Session Discussion
**Scenario**: Live coding session with Reddit for questions

**Setup**:
1. Create discussion thread for your session
2. Monitor for audience questions
3. Use hashtag matching your session name

**Result**: Live Q&A integration with your presentation

### 4. Community Event Coverage
**Scenario**: Major announcement or community event

**Setup**:
1. Find or create the main discussion thread
2. Monitor for community reactions
3. Display reactions during live coverage

**Result**: Real-time community sentiment and discussion

## Troubleshooting

### Common Issues

#### "Post not found" Error
- **Cause**: Invalid Reddit URL or deleted post
- **Solution**: Verify the URL is correct and the post still exists

#### "Subreddit is private" Error
- **Cause**: The subreddit requires membership to view
- **Solution**: Use a public subreddit or gain access to the private one

#### No Comments Appearing
- **Possible Causes**:
  - Provider is disabled
  - Post has no new comments
  - Comments are being filtered out
  - TagzApp hashtag doesn't match
- **Solutions**:
  - Check provider status in admin panel
  - Verify the Reddit thread has activity
  - Review filtering settings
  - Ensure you're searching for the correct hashtag in TagzApp

#### Comments Appear Slowly
- **Cause**: Refresh interval is too high
- **Solution**: Lower the refresh interval (minimum 15 seconds)

#### Too Many Comments
- **Cause**: Very active thread overwhelming the display
- **Solutions**:
  - Increase minimum comment score filter
  - Reduce max comments per poll
  - Add blocked authors for spam accounts

### Rate Limiting
Reddit limits API calls to prevent abuse:
- **Limit**: ~60 requests per minute
- **TagzApp Response**: Automatically increases polling interval if rate limited
- **Your Action**: Consider increasing refresh interval for very active threads

### Performance Tips
- **For Active AMAs**: 30-second refresh, score filter of 0 or higher
- **For Slower Discussions**: 60+ second refresh, include all comments
- **For High-Traffic Events**: Increase min score filter, block spam accounts
- **For Moderated Events**: Use blocked authors list for inappropriate users

## Best Practices

### Before Your Event
1. **Test the setup** with the actual Reddit thread
2. **Configure filters** to match your content quality needs
3. **Prepare moderation** by identifying potential blocked users
4. **Document your hashtag** for participants and viewers

### During Your Event
1. **Monitor the provider status** to ensure it's running
2. **Watch for spam** and add blocked authors as needed
3. **Adjust filters** if too many/few comments are appearing
4. **Have backup plan** in case of Reddit API issues

### After Your Event
1. **Keep the provider running** if you want to capture follow-up discussion
2. **Archive the configuration** for future similar events
3. **Review performance** and adjust settings for next time

## Security and Privacy

### Data Handling
- TagzApp only accesses **publicly available** Reddit comments
- **No authentication** required - only public data is used
- **Minimal storage** - comments are displayed and then archived normally

### Privacy Considerations
- All Reddit usernames and comments are **already public**
- TagzApp displays them **as-is** from Reddit
- **Moderation tools** available to filter inappropriate content

### Terms of Service
- Respects Reddit's **API rate limits** and terms of service
- **No automated voting** or posting - read-only access
- Suitable for **commercial and non-commercial** use

---

## Getting Help

### Documentation
- **Admin Guide**: See TagzApp admin documentation
- **General Usage**: Check TagzApp user manual
- **API Issues**: Review TagzApp logs in admin panel

### Support Channels
- **GitHub Issues**: Technical problems and feature requests
- **Community Discord**: Setup help and usage questions
- **Documentation**: Additional guides and examples

### Common Questions
**Q: Can I monitor multiple Reddit threads?**
A: Currently, each provider instance monitors one thread. You can run multiple providers for multiple threads.

**Q: Does this work with private subreddits?**
A: No, only public subreddits and posts are supported.

**Q: How much does this cost?**
A: Reddit's API is free for read-only access. No additional costs beyond TagzApp hosting.

**Q: Can I moderate the comments?**
A: Yes, use TagzApp's built-in moderation tools plus the provider's blocking features.

---

*Need more help? Check the TagzApp documentation or ask in the community Discord!*