﻿(function () {

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
		const newMessageTime = new Date(content.timestamp);
		newMessage.innerHTML = `
					<span class="author">${content.authorDisplayName}:  <i class="bi bi-${content.provider.toLowerCase()}"></i></span>
					<span class="time" data-time="${newMessageTime.toISOString()}">${newMessageTime.toLocaleString(undefined, { day: 'numeric', month: 'numeric', year: 'numeric', hour: '2-digit', minute: '2-digit' })}</span>
					<span class="content">${content.text}</span>`;
		newMessage.addEventListener("DOMNodeInserted", function (ev) {
			window.Masonry.resizeGridItem(newMessage);
		}, false);
		newMessage.addEventListener("click", function (ev) {

			var el = ev.target.closest('article');

			connection.invoke("SendMessageToOverlay", window.TagzApp.Tags[0], el.getAttribute("data-provider"), el.getAttribute("data-providerid"));

		});

		const newest = getDateFromElement(taggedContent.firstElementChild);
		const oldest = getDateFromElement(taggedContent.lastElementChild);
		if (taggedContent.childElementCount === 0 || (newest && (newest <= newMessageTime))) {
			taggedContent.prepend(newMessage);
		} else if (oldest && (oldest > newMessageTime)) {
			taggedContent.append(newMessage);
		} else {
			const times = [...taggedContent.children].map(article => getDateFromElement(article));
			const index = getIndexForValue(times, newMessageTime);
			taggedContent.insertBefore(newMessage, taggedContent.children[index]);
		}

	}

	function getDateFromElement(el) {
		const span = el?.querySelector('.time');
		if (span === undefined || span === null) return false;

		const date = new Date(span.dataset.time);
		if (isNaN(date)) return false;

		return date;
	}

	function getIndexForValue(list, value) {
		let low = 0;
        let high = list.length - 1;

        while (low <= high) {
			let mid = (low + high) >>> 1;
			let guess = list[mid];

			if (guess === value) return mid;

			if (guess > value) {
				low = mid + 1;
			} else {
				high = mid - 1;
			}
		}

        return low;
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