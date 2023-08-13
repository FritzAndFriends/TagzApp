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
		const newMessageTime = new Date(content.timestamp);
		newMessage.setAttribute("data-timestamp", newMessageTime.toISOString());
		newMessage.innerHTML = `
		<img class="ProfilePicture" src="${content.authorProfileImageUri}" alt="${content.authorDisplayName}" />
		<div class="byline">
			<div class="author">${content.authorDisplayName}</div>
			<div class="authorUserName">${content.authorUserName}</div>
		</div>
		<i class="provider bi bi-${content.provider.toLowerCase()}"></i>
		<div class="time">${newMessageTime.toLocaleString(undefined, { day: 'numeric', month: 'long', year: 'numeric', hour: 'numeric', minute: '2-digit' })}</div>

		<div class="content">${content.text}</div>`;

		if (content.previewCard) {
			newMessage.innerHTML += `
				<div class="contentcard">
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

			var el = ev.target.closest('article');

			connection.invoke("SendMessageToOverlay", window.TagzApp.Tags[0], el.getAttribute("data-provider"), el.getAttribute("data-providerid"));

			// Format Modal
			let modalTitle = document.getElementById("modalTitle");
			modalTitle.innerHTML = `
            ${content.authorDisplayName}: 
            <i class="bi bi-${content.provider.toLowerCase()}"></i>
        `;

			let modalBody = document.querySelector(".modal-body").innerHTML = content.text;
			let modalTime = document.querySelector(".modal-time").textContent = new Date(content.timestamp).toLocaleString(undefined, {
				day: 'numeric',
				month: 'numeric',
				year: 'numeric',
				hour: '2-digit',
				minute: '2-digit'
			});

			let modalWindow = new bootstrap.Modal(document.getElementById("contentModal"));
			modalWindow.show();

		});

		const newest = getDateFromElement(taggedContent.firstElementChild);
		const oldest = getDateFromElement(taggedContent.lastElementChild);
		if (newest === null || (newest <= newMessageTime)) {
			taggedContent.prepend(newMessage);
		} else if (oldest !== null && (oldest > newMessageTime)) {
			taggedContent.append(newMessage);
		} else {
			const times = [...taggedContent.children].map(article => getDateFromElement(article));
			const index = getIndexForValue(times, newMessageTime);
			taggedContent.insertBefore(newMessage, taggedContent.children[index]);
		}

	}

	function getDateFromElement(el) {
		const timestamp = el?.dataset.timestamp;
		if (timestamp === undefined || timestamp === null) return null;

		const date = new Date(timestamp);
		if (isNaN(date)) return null;

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
