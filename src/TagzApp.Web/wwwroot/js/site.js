(function () {

	var connection;

	const ModerationState = {
		Pending		: 0,
		Approved	: 1,
		Rejected	: 2
	}

	const taggedContent = document.getElementById("taggedContent");
	const observer = new MutationObserver(function (mutationsList, observer) {
		for (const mutation of mutationsList) {
			if (mutation.type !== 'childList') continue;
			for (const node of mutation.addedNodes) {
				const imgCardTop = node.querySelector(".card-img-top");
				if (imgCardTop !== null) {
					imgCardTop.addEventListener("load", function (ev) {
						window.Masonry.resizeGridItem(node);
					}, false);
					imgCardTop.addEventListener("error", function (ev) {
						window.Masonry.resizeGridItem(node);
					}, false);
				} else {
					window.Masonry.resizeGridItem(node);
				}
            }
		}
	});
	const observerConfig = {
		childList: true
	};

	observer.observe(taggedContent, observerConfig);
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

		if (document.querySelector("[data-providerid='" + content.providerId + "']")) return;

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
			<div class="authorUserName" title="${content.authorUserName}">${content.authorUserName}</div>
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

		newMessage.addEventListener("click", function (ev) {

			var el = ev.target.closest('article');

			connection.invoke("SendMessageToOverlay", window.TagzApp.Tags[0], el.getAttribute("data-provider"), el.getAttribute("data-providerid"));

			// Format Modal
			let modalProfilePic = document.querySelector(".modal-header img");
			modalProfilePic.src = content.authorProfileImageUri;
			modalProfilePic.alt = content.authorDisplayName;

			document.querySelector(".modal-header .author").innerText = content.authorDisplayName;

			let modalAuthorUserName = document.querySelector(".modal-header .authorUserName");
			modalAuthorUserName.setAttribute("title", content.authorUserName);
			modalAuthorUserName.innerText = content.authorUserName;

			let modalProvider = document.querySelector(".modal-header .bi");
			modalProvider.setAttribute("class", "");
			modalProvider.classList.add('provider', 'bi', `bi-${content.provider.toLowerCase()}`);

			document.querySelector(".modal-header .time").innerText = `${newMessageTime.toLocaleString(undefined, { day: 'numeric', month: 'long', year: 'numeric', hour: 'numeric', minute: '2-digit' })}`;

			let modalBody = document.querySelector(".modal-body").innerHTML = content.text;

			if (content.previewCard) {
				document.querySelector(".modal-body").innerHTML += `
				<div class="contentcard">
					<img src="${content.previewCard.imageUri}" class="card-img-top" alt="${content.previewCard.altText}" />
				</div>
			`
			}

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

	function FormatMessageForModeration(content) {

		if (taggedContent.querySelector(".spinner-border")) {
			taggedContent.querySelector(".spinner-border").remove();
		}

		if (document.querySelector("[data-providerid='" + content.providerId + "']")) return;

		const newMessage = document.createElement("article");
		newMessage.classList.add("moderation");

		if (content.state == ModerationState.Approved) newMessage.classList.add("status-approved");
		if (content.state == ModerationState.Rejected) newMessage.classList.add("status-rejected");

		newMessage.setAttribute("data-url", content.sourceUri);
		newMessage.setAttribute("data-provider", content.provider);
		newMessage.setAttribute("data-providerid", content.providerId);
		const newMessageTime = new Date(content.timestamp);
		newMessage.setAttribute("data-timestamp", newMessageTime.toISOString());
		newMessage.innerHTML = `
		<img class="ProfilePicture" src="${content.authorProfileImageUri}" alt="${content.authorDisplayName}" />
		<div class="byline">
			<div class="author">${content.authorDisplayName}</div>
			<div class="authorUserName" title="${content.authorUserName}">${content.authorUserName}</div>
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

		newMessage.addEventListener("mouseenter", showModerationPanel);
		newMessage.addEventListener("click", showModerationPanel);		// for touch-screen support

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

	function showModerationPanel(ev) {

		var hovered = ev.target.closest('article');
		if (hovered.querySelector("#moderationAction")) return;

		var hoverPanel = document.getElementById("moderationAction").cloneNode(true);
		hoverPanel.style.display = "";
		hoverPanel.classList.add("active_panel");

		hoverPanel.querySelector("i.approve").addEventListener("click", function (ev) {
			connection.invoke("SetStatus", hovered.getAttribute("data-provider"), hovered.getAttribute("data-providerid"), ModerationState.Approved);
			hoverPanel.remove();
			hovered.classList.remove("status-rejected");
			hovered.classList.add("status-approved");
		});

		hoverPanel.querySelector("i.reject").addEventListener("click", function (ev) {
			connection.invoke("SetStatus", hovered.getAttribute("data-provider"), hovered.getAttribute("data-providerid"), ModerationState.Rejected);
			hoverPanel.remove();
			hovered.classList.remove("status-approved");
			hovered.classList.add("status-rejected");
		});

		hovered.insertBefore(hoverPanel, hovered.firstElementChild);

		hoverPanel.addEventListener("mouseleave", function (ev) {

			ev.target.closest("#moderationAction").remove();
			var panels = document.querySelectorAll(".active_panel");
			panels.forEach(function (panel) {
				panel.remove();
			});


		});

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

		ListenForWaterfallContent: async function (tags) {

			var tagCsv = encodeURI(tags);
			t.Tags = tags.split(",");

			connection = new signalR.HubConnectionBuilder()
				.withUrl(`/messages?t=${tagCsv}`)
				.withAutomaticReconnect()
				.configureLogging(signalR.LogLevel.Information)
				.build();

			connection.on("NewWaterfallMessage", (content) => {
				FormatMessage(content);
			});

			connection.on("RemoveMessage", (provider, providerId) => {

				var item = document.querySelector(`[data-providerid='${providerId}']`);
				if (item) item.remove();

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

		ListenForModerationContent: async function (tag) {

			var tagCsv = encodeURI(tag);

			connection = new signalR.HubConnectionBuilder()
				.withUrl(`/mod?t=${tagCsv}`)
				.withAutomaticReconnect()
				.configureLogging(signalR.LogLevel.Information)
				.build();

			connection.on("NewWaterfallMessage", (content) => {
				FormatMessageForModeration(content);
			});

			// Start the connection.
			await start();

			connection.invoke("GetContentForTag", tag)
				.then(function (result) {

					result.forEach(function (content) {
						FormatMessageForModeration(content);
					});
					window.Masonry.resizeAllGridItems();
				});

		},

	};

	window.TagzApp = window.TagzApp || t;

})();
