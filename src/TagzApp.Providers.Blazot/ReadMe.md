# Blazot Provider
API Documentation is available at https://developers.blazot.com/docs/api/v1

### Key Points
- The first thing you need to do is create a user account (<span style="color:orange">⚠️not company - read below</span>) at https://blazot.com. 
	- If you're creating an account for a company, that should be created as a "Site" after creating and logging in with your personal user account. If you use the desired company username as a user, it won't be available as a Site.
- The API key and Secret Auth Key is available in the user's Configuration page (top right menu) or Site's Configuration page after logging into https://blazot.com.
- The API keys then need added to the TagzApp app settings, ideally in your local secret store or Azure Key Vault.
- The Blazot provider uses those keys to fetch the access token and renews that token as needed. The Secret Auth Token should only be used to get the access token.
- If your keys or token are compromised, just regenerate your keys on the Blazot Configuration page.
- The current default rate limit is 5 requests per 15 minute window for users without a Blazot subscription. This will likely increase as the site grows, but there isn't a real need to make more requests than that at the moment.
