(function () {

	var connection;

	const taggedContent = document.getElementById("taggedContent");

	async function start() {
		try {
			await connection.start();
			console.log("SignalR Connected.");
		} catch (err) {
			console.log(err);
			setTimeout(start, 5000);
		}

		connection.onclose(async () => {
			await start();
		});

	};

	function FormatMessage(content) {

		if (taggedContent.querySelector(".spinner-border")) {
			taggedContent.querySelector(".spinner-border").remove();
		}

		const newMessage = document.createElement("article");
		newMessage.setAttribute("data-url", content.sourceUri);
		newMessage.setAttribute("data-provider", content.provider);
		newMessage.setAttribute("data-providerid", content.providerId);
		newMessage.innerHTML = `
					<span class="author">${content.authorDisplayName}:  <i class="bi bi-${content.provider.toLowerCase()}"></i></span>
					<span class="time">${new Date(content.timestamp).toLocaleString(undefined, { day: 'numeric', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>
					<span class="content">${content.text}</span>`;

		if (content.previewCard) {
			newMessage.innerHTML += `
				<div class="card">
					<img src="${content.previewCard.imageUri}" class="card-img-top" alt="${content.previewCard.altText}" />
				</div>
			`
		}

		if (content.previewCard) {
			newMessage.querySelector(".card-img-top").addEventListener("load", function (ev) {
				window.Masonry.resizeGridItem(newMessage);
			}, false);
		} else {
			newMessage.addEventListener("DOMNodeInserted", function (ev) {
				window.Masonry.resizeGridItem(newMessage);
			}, false);
		}
		newMessage.addEventListener("click", function (ev) {

			var el = ev.srcElement;
			while (el && el.tagName !== "ARTICLE") {
				el = el.parentNode;
			}

			connection.invoke("SendMessageToOverlay", window.TagzApp.Tags[0], el.getAttribute("data-provider"), el.getAttribute("data-providerid"));

		});
		taggedContent.appendChild(newMessage);

	}

	const t = {

		Tags: [],

		ListenForTags: async function (tags) {

			var tagCsv = encodeURI(tags);
			t.Tags = tags.split(",");

			connection = new signalR.HubConnectionBuilder()
				.withUrl(`/messages?t=${tagCsv}`)
				.withAutomaticReconnect()
				.configureLogging(signalR.LogLevel.Information)
				.build();

			connection.on("NewMessage", (content) => {
				FormatMessage(content);
			});

			// Start the connection.
			await start();

			connection.invoke("GetExistingContentForTag", tags)
				.then(function (result) {

					result.forEach(function (content) {
						FormatMessage(content);
					});
					window.Masonry.resizeAllGridItems();
				});

		},

	};

	window.TagzApp = window.TagzApp || t;

})();