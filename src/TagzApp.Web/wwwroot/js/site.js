(function () {
	var connection;

	const ModerationState = {
		Pending: 0,
		Approved: 1,
		Rejected: 2,
	};

	var paused = false;
	var rolloverPause = false;
	var pauseQueue = [];
	var pauseTimeout;
	const waterfallMaxEntries = 100;
	const moderationMaxEntries = 500;

	const taggedContent = document.getElementById('taggedContent');
	const observer = new MutationObserver(function (mutationsList, observer) {
		for (const mutation of mutationsList) {
			if (mutation.type !== 'childList') continue;
			for (const node of mutation.addedNodes) {
				const imgCardTop = node.querySelector('.card-img-top');
				if (imgCardTop !== null) {
					imgCardTop.addEventListener(
						'load',
						function (ev) {
							window.Masonry.resizeGridItem(node);
						},
						false,
					);
					imgCardTop.addEventListener(
						'error',
						function (ev) {
							window.Masonry.resizeGridItem(node);
						},
						false,
					);
				} else {
					window.Masonry.resizeGridItem(node);
				}
			}
		}
	});
	const observerConfig = {
		childList: true,
	};

	if (taggedContent !== null) {
		observer.observe(taggedContent, observerConfig);
	}

	async function start() {
		try {
			await connection.start();
			console.log('SignalR Connected.');
		} catch (err) {
			console.log(err);
			setTimeout(start, 5000);
		}

		connection.onclose(async () => {
			await start();
		});
	}

	function FormatMessage(content, additionalClass, onclick, onmouseenter) {
		if (taggedContent.querySelector('.spinner-border')) {
			taggedContent.querySelector('.spinner-border').remove();
		}

		if (
			document.querySelector("[data-providerid='" + content.providerId + "']")
		)
			return;

		const newMessage = document.createElement('article');

		if (additionalClass) {
			if (additionalClass.constructor === Array) {
				for (const cssClass of additionalClass) {
					newMessage.classList.add(cssClass);
				}
			} else {
				newMessage.classList.add(additionalClass);
			}
		}

		newMessage.setAttribute('data-url', content.sourceUri);
		newMessage.setAttribute('data-provider', content.provider);
		newMessage.setAttribute('data-providerid', content.providerId);
		const newMessageTime = new Date(content.timestamp);
		newMessage.setAttribute('data-timestamp', newMessageTime.toISOString());
		newMessage.innerHTML = `
		<img class="ProfilePicture" src="${content.authorProfileImageUri}" alt="${
			content.authorDisplayName
		}" />
		<div class="byline">
			<div class="author">${content.authorDisplayName} <i class="autoMod"></i></div>
			<div class="authorUserName" title="${content.authorUserName}">${
				content.authorUserName
			}</div>
		</div>
		<i class="provider bi ${MapProviderToIcon(content.provider)}"></i>
		<div class="time">${newMessageTime.toLocaleString(undefined, {
			day: 'numeric',
			month: 'long',
			year: 'numeric',
			hour: 'numeric',
			minute: '2-digit',
		})}<div class="autoModReason"></div></div>

		<div class="content">${FormatContextWithEmotes(content)}</div>`;

		if (content.previewCard) {
			const tag =
				content.previewCard.imageUri.split('.').pop() == 'mp4'
					? "video muted='muted' controls='controls' autoplay"
					: 'img onerror="this.onerror=null; this.style.display=\'none\';"';
			newMessage.innerHTML += `
				<div class="contentcard">
					<${tag} src="${content.previewCard.imageUri}" class="card-img-top" alt="${content.previewCard.altText}" />
				</div>
			`;
		}

		if (onclick) {
		} else {
			newMessage.addEventListener('click', function (ev) {
				var el = ev.target.closest('article');

				connection.invoke(
					'SendMessageToOverlay',
					window.TagzApp.Tags[0],
					el.getAttribute('data-provider'),
					el.getAttribute('data-providerid'),
				);

				// Pause updates
				paused = true;
				FormatPauseButton();

				// Format Modal
				let modalProfilePic = document.querySelector('.modal-header img');
				modalProfilePic.src = content.authorProfileImageUri;
				modalProfilePic.alt = content.authorDisplayName;

				document.querySelector('.modal-header .author').innerText =
					content.authorDisplayName;

				let modalAuthorUserName = document.querySelector(
					'.modal-header .authorUserName',
				);
				modalAuthorUserName.setAttribute('title', content.authorUserName);
				modalAuthorUserName.innerText = content.authorUserName;

				let modalProvider = document.querySelector('.modal-header .bi');
				modalProvider.setAttribute('class', '');
				modalProvider.classList.add(
					'provider',
					'bi',
					`${MapProviderToIcon(content.provider)}`,
				);

				document.querySelector(
					'.modal-header .time',
				).innerText = `${newMessageTime.toLocaleString(undefined, {
					day: 'numeric',
					month: 'long',
					year: 'numeric',
					hour: 'numeric',
					minute: '2-digit',
				})}`;

				let modalBody = (document.querySelector('.modal-body').innerHTML =
					content.text);

				if (
					content.previewCard &&
					content.previewCard.imageUri.trim() != 'about:blank'
				) {
					const tag =
						content.previewCard.imageUri.split('.').pop() == 'mp4'
							? "video muted='muted' controls='controls' autoplay"
							: 'img onerror="this.onerror=null; this.parentElement.style.display = \'none\';"';
					document.querySelector('.modal-body').innerHTML += `
				<div class="contentcard">
					<${tag} src="${content.previewCard.imageUri}" class="card-img-top" alt="${content.previewCard.altText}" />
				</div>
			`;
				}

				let modalWindow = new bootstrap.Modal(
					document.getElementById('contentModal'),
				);

				// NOTE: Let's not immediately turn off pause coming back from a modal
				//document.getElementById('contentModal').addEventListener('hide.bs.modal', function (ev) {
				//	paused = false;
				//	FormatPauseButton();
				//	ResumeFromPause();
				//});

				modalWindow.show();
			});
		}

		if (onmouseenter) {
			newMessage.addEventListener('mouseenter', onmouseenter);
		}

		const newest = getDateFromElement(taggedContent.firstElementChild);
		const oldest = getDateFromElement(taggedContent.lastElementChild);
		if (newest === null || newest <= newMessageTime) {
			taggedContent.prepend(newMessage);
		} else if (oldest !== null && oldest > newMessageTime) {
			taggedContent.append(newMessage);
		} else {
			const times = [...taggedContent.children].map((article) =>
				getDateFromElement(article),
			);
			const index = getIndexForValue(times, newMessageTime);
			taggedContent.insertBefore(newMessage, taggedContent.children[index]);
		}

		// Remove oldest message if we're over the max
		if (
			document.querySelector('.currentModerators') == null &&
			taggedContent.children.length > waterfallMaxEntries
		) {
			taggedContent.lastElementChild.remove();
		} else if (
			document.querySelector('.currentModerators') != null &&
			taggedContent.children.length > moderationMaxEntries
		) {
			taggedContent.lastElementChild.remove();
		}
	}

	function FormatMessageForModeration(content) {
		var moreClasses = ['moderation'];
		if (content.state == ModerationState.Approved)
			moreClasses.push('status-approved');
		if (content.state == ModerationState.Rejected)
			moreClasses.push('status-rejected');
		if (
			content.state == ModerationState.Rejected &&
			content.moderator == 'AZURE-CONTENTSAFETY'
		)
			moreClasses.push('status-automod');
		if (
			content.state == ModerationState.Rejected &&
			content.moderator != 'AZURE-CONTENTSAFETY'
		)
			moreClasses.push('status-humanmod');

		FormatMessage(
			content,
			moreClasses,
			showModerationPanel,
			showModerationPanel,
		);

		if (
			content.state == ModerationState.Rejected &&
			content.moderator == 'AZURE-CONTENTSAFETY'
		) {
			var card = document.querySelector(
				`[data-providerid='${content.providerId}']`,
			);
			FormatAutomodReason(content, card);
		}
	}

	function FormatAutomodReason(content, card) {
		content.reason = content.reason.replace('2', 'Low');
		content.reason = content.reason.replace('4', 'Medium');
		content.reason = content.reason.replace('6', 'High');
		content.reason = content.reason.replace('.', '');
		card.querySelector(
			'.autoModReason',
		).innerText = `AI Reason ( ${content.reason} )`;
	}

	function MapProviderToIcon(provider) {
		var cssClass = provider.toLowerCase().trim();
		switch (cssClass) {
			case 'twitter':
				cssClass = 'twitter-x';
				break;
			case 'youtube-chat':
				cssClass = 'youtube';
				break;
			default:
				break;
		}

		return 'bi-' + cssClass;
	}

	function FormatContextWithEmotes(content) {
		var text = content.text;
		if (!content.emotes) return text;

		var toReplace = [];

		for (var e in content.emotes) {
			var emote = content.emotes[e];
			var emoteUrl = emote.imageUrl;

			var emoteName = text
				.substring(emote.pos, emote.length + emote.pos + 1)
				.trim();
			var emoteHtml = `<img class="emote" src="${emoteUrl}"  />`;
			toReplace.push({ name: emoteName, html: emoteHtml });
		}

		for (var r in toReplace) {
			var item = toReplace[r];
			text = text.replace(item.name, item.html);
		}

		return text;
	}

	function ApproveMessage(content) {
		var card = document.querySelector(
			`[data-providerid='${content.providerId}']`,
		);

		if (card) {
			card.classList.remove('status-rejected');
			card.classList.remove('status-automod');
			card.classList.add('status-approved');
		}
	}

	function RejectMessage(content) {
		var card = document.querySelector(
			`[data-providerid='${content.providerId}']`,
		);

		if (card) {
			card.classList.remove('status-approved');
			card.classList.remove('status-automod');
			card.classList.add('status-rejected');
		}

		if (content.moderator == 'AZURE-CONTENTSAFETY') {
			card.classList.add('status-automod');
			FormatAutomodReason(content, card);
		} else {
			card.classList.add('status-humanmod');
		}
	}

	function PauseOnRollover(ev) {
		// pause updates
		window.clearTimeout(pauseTimeout);
		if (!paused) {
			paused = true;
			rolloverPause = true;
			FormatPauseButton();
		}

		ev.srcElement.addEventListener('mouseleave', function (ev) {
			// resume updates if we mouse out
			if (rolloverPause) {
				pauseTimeout = window.setTimeout(() => {
					paused = false;
					rolloverPause = false;
					FormatPauseButton();
					ResumeFromPause();
				}, 1500);
			}
		});
	}

	function showModerationPanel(ev) {
		var hovered = ev.target.closest('article');
		if (hovered.querySelector('#moderationAction')) return;

		// pause updates
		window.clearTimeout(pauseTimeout);
		if (!paused) {
			paused = true;
			rolloverPause = true;
			FormatPauseButton();
		}

		var hoverPanel = document
			.getElementById('moderationAction')
			.cloneNode(true);
		hoverPanel.style.display = '';
		hoverPanel.classList.add('active_panel');

		hoverPanel
			.querySelector('i.approve')
			.addEventListener('click', function (ev) {
				connection.invoke(
					'SetStatus',
					hovered.getAttribute('data-provider'),
					hovered.getAttribute('data-providerid'),
					ModerationState.Approved,
				);
				hoverPanel.remove();
				hovered.classList.remove('status-rejected');
				hovered.classList.add('status-approved');
			});

		hoverPanel
			.querySelector('i.reject')
			.addEventListener('click', function (ev) {
				connection.invoke(
					'SetStatus',
					hovered.getAttribute('data-provider'),
					hovered.getAttribute('data-providerid'),
					ModerationState.Rejected,
				);
				hoverPanel.remove();
				hovered.classList.remove('status-approved');
				hovered.classList.add('status-rejected');
			});

		hovered.insertBefore(hoverPanel, hovered.firstElementChild);

		hoverPanel.addEventListener('mouseleave', function (ev) {
			// resume updates if we mouse out
			if (rolloverPause) {
				pauseTimeout = window.setTimeout(() => {
					paused = false;
					rolloverPause = false;
					FormatPauseButton();
					ResumeFromPause();
				}, 1500);
			}

			// cleanup the moderation overlay
			ev.target.closest('#moderationAction').remove();
			var panels = document.querySelectorAll('.active_panel');
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

	function AddModerator(moderator) {
		var moderatorList = document.querySelector('.currentModerators');

		var newMod = document.createElement('img');
		newMod.id = 'moderator-' + moderator.email;
		newMod.title = moderator.displayName;
		newMod.src = moderator.avatarImageSource;

		moderatorList.appendChild(newMod);
	}

	function ConfigurePauseButton() {
		var pauseButton = document.getElementById('pauseButton');
		pauseButton.addEventListener('click', function (ev) {
			paused = !paused;
			FormatPauseButton();
			if (!paused) ResumeFromPause();
		});
	}

	function FormatPauseButton() {
		if (paused) {
			pauseButton.classList.remove('bi-pause-circle-fill');
			pauseButton.classList.add('bi-play-circle-fill');
		} else {
			pauseButton.classList.remove('bi-play-circle-fill');
			pauseButton.classList.add('bi-pause-circle-fill');
		}
	}

	function AddMessageToPauseQueue(content) {
		pauseQueue.push(content);
	}

	function ResumeFromPause() {
		// for each element in pauseQueue, call FormatMessage, then clear the queue
		pauseQueue.forEach(function (content) {
			if (document.querySelector('.currentModerators')) {
				FormatMessageForModeration(content);
			} else {
				FormatMessage(content, null, null, PauseOnRollover);
			}
		});

		pauseQueue = [];
	}

	const t = {
		Tags: [],

		MapProviderToIconClass: function (provider) {
			return MapProviderToIcon(provider);
		},

		ListenForWaterfallContent: async function (tags) {
			var tagCsv = encodeURI(tags);
			t.Tags = tags.split(',');

			connection = new signalR.HubConnectionBuilder()
				.withUrl(`/messages?t=${tagCsv}`)
				.withAutomaticReconnect()
				.configureLogging(signalR.LogLevel.Information)
				.build();

			connection.on('NewWaterfallMessage', (content) => {
				if (paused) {
					AddMessageToPauseQueue(content);
					return;
				}
				FormatMessage(content, null, null, PauseOnRollover);
			});

			connection.on('RemoveMessage', (provider, providerId) => {
				var item = document.querySelector(`[data-providerid='${providerId}']`);
				if (item) item.remove();

				// Remove item from pauseQueue if it's loaded
				pauseQueue = pauseQueue.filter(function (content) {
					return content.providerId != providerId;
				});
			});

			// Start the connection.
			await start();

			ConfigurePauseButton();

			connection
				.invoke('GetExistingContentForTag', tags)
				.then(function (result) {
					result.forEach(function (content) {
						FormatMessage(content, null, null, PauseOnRollover);
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

			connection.on('NewWaterfallMessage', (content) => {
				if (paused) {
					AddMessageToPauseQueue(content);
					return;
				}

				FormatMessageForModeration(content);
			});

			connection.on('NewApprovedMessage', (content) => {
				if (!paused) {
					ApproveMessage(content);
				} else {
					// Find item in the pauseQueue and set its state to ModerationState.Approved
					pauseQueue.forEach(function (item) {
						if (item.providerId == content.providerId) {
							item.state = ModerationState.Approved;
						}
					});
				}
			});

			connection.on('NewRejectedMessage', (content) => {
				if (!paused) {
					RejectMessage(content);
				} else {
					// find item in the pauseQueue and set its state to ModerationState.Rejected
					pauseQueue.forEach(function (item) {
						if (item.providerId == content.providerId) {
							item.state = ModerationState.Rejected;
						}
					});
				}
			});

			connection.on('NewModerator', (moderator) => {
				AddModerator(moderator);
			});

			connection.on('RemoveModerator', (moderatorEmail) => {
				document.getElementById('moderator-' + moderatorEmail).remove();
			});

			// Start the connection.
			await start();

			ConfigurePauseButton();

			connection.invoke('GetContentForTag', tag).then(function (result) {
				result.forEach(function (content) {
					FormatMessageForModeration(content);
				});
				window.Masonry.resizeAllGridItems();
			});
		},
	};

	window.TagzApp = window.TagzApp || t;
})();
