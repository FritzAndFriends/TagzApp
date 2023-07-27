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
		newMessage.innerHTML = `
					<span class="author">${content.authorDisplayName}:  <i class="bi bi-${content.provider.toLowerCase()}"></i></span>
					<span class="time">${new Date(content.timestamp).toLocaleString(undefined, { day: 'numeric', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>
					<span class="content">${content.text}</span>`;
		newMessage.addEventListener("DOMNodeInserted", function (ev) {
			window.Masonry.resizeGridItem(newMessage);
		}, false);
		taggedContent.appendChild(newMessage);

	}

	const t = {

		ListenForTags: async function (tags) {

			var tagCsv = encodeURI(tags);

			connection = new signalR.HubConnectionBuilder()
				.withUrl(`/messages?t=${tagCsv}`)
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