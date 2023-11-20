(function () {
	var connection;

	const ModerationState = {
		Pending: 0,
		Approved: 1,
		Rejected: 2,
	};

	const ApprovalFilter = {
		All: '-1',
		Approved: '1',
		Rejected: '2',
		Unmoderated: '0',
	};

	var tagCsv = '';
	var paused = false;
	var rolloverPause = false;
	var pauseQueue = [];
	var pauseTimeout;
	var approvedFilterStatus = ApprovalFilter.All;
	var providerFilter = [];
	var cursorProviderId = null;
	var currentModal = null;
	var modalWindow = null;

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

	function decodeHtml(html) {
		var txt = document.createElement('textarea');
		txt.innerHTML = html;
		return txt.value;
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

	function RemoveActivePanel() {
		if (document.querySelector('.active_panel')) {
			// remove the active panel
			document.querySelector('.active_panel').remove();
		}
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
		}" onerror="this.src='/img/user.jpg';" />
		<div class="byline">
			<div class="author">${content.authorDisplayName} <i class="autoMod"></i></div>
			<div class="authorUserName" title="${content.authorUserName}">${
				content.authorUserName
			}</div>
		</div>
		<i class="provider bi ${MapProviderToIcon(content.provider)}"></i>
		<div class="time"><div>${newMessageTime.toLocaleString(undefined, {
			day: 'numeric',
			month: 'short',
			year: 'numeric',
			hour: 'numeric',
			minute: '2-digit',
		})}</div><div class="autoModReason"></div></div>

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
			newMessage.addEventListener('touchend', (ev) => {
				window.setTimeout(() => onclick(ev), 100);
			});
		} else {
			newMessage.addEventListener('click', function (ev) {
				var el = ev.target.closest('article');

				if (currentModal == content.providerId) return;
				currentModal = content.providerId;

				if (modalWindow) modalWindow.hide();

				modalWindow = new bootstrap.Modal(
					document.getElementById('contentModal'),
				);

				// modalWindow.modal('hide');

				connection.invoke(
					'SendMessageToOverlay',
					window.TagzApp.Tags[0],
					el.getAttribute('data-provider'),
					el.getAttribute('data-providerid'),
				);

				// Pause updates
				paused = true;
				FormatPauseButton();

				// modal window is face down to start
				document.querySelector('.modal-inner').classList.remove('flip');

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

				document.querySelector('.modal-header .time').innerText =
					`${newMessageTime.toLocaleString(undefined, {
						day: 'numeric',
						month: 'long',
						year: 'numeric',
						hour: 'numeric',
						minute: '2-digit',
					})}`;

				let modalBody = (document.querySelector('.modal-body').innerHTML =
					FormatContextWithEmotes(content));

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

				// NOTE: Let's not immediately turn off pause coming back from a modal
				//document.getElementById('contentModal').addEventListener('hide.bs.modal', function (ev) {
				//	paused = false;
				//	FormatPauseButton();
				//	ResumeFromPause();
				//});

				modalWindow.show();

				window.setTimeout(() => {
					document.querySelector('.modal-inner').classList.add('flip');
				}, mouseSpinDelay);
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
		card.querySelector('.autoModReason').innerText =
			`AI Reason ( ${content.reason} )`;
	}

	function MapProviderToIcon(provider) {
		var cssClass = provider.toLowerCase().trim();
		switch (cssClass) {
			case 'twitter':
				cssClass = 'twitter-x';
				break;
			case 'website':
				cssClass = 'globe2';
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
		var text = decodeHtml(content.text);
		if (!content.emotes) return text;

		var toReplace = [];

		for (var e in content.emotes) {
			var emote = content.emotes[e];
			var emoteUrl = emote.imageUrl;

			var emoteName = text
				.substring(emote.pos, emote.length + emote.pos + 1)
				.trim();
			var emoteHtml = `<img class="emote" src="${emoteUrl}"  />`;
			// console.log(
			// 	`Formatting text: '${text}' with emote at ${emote.pos}, with length ${emote.length} and found text ${emoteName}`,
			// );
			toReplace.push({ name: emoteName, html: emoteHtml });
		}

		for (var r in toReplace) {
			var item = toReplace[r];
			// console.log(`Replacing ${item.name} with ${item.html}`);
			text = text.replace(item.name, item.html);
		}

		return text;
	}

	function LoadAdditionalContentForFilters() {
		// only proceed if less than 20 article elements are visible
		if (
			document.querySelectorAll('article:not([style*="display: none"])')
				.length < 20
		)
			return;

		// use the SignalR connection to call the server and get the additional content
		connection
			.invoke(
				'GetFilteredContentByTag',
				tagCsv,
				providerFilter,
				approvedFilterStatus,
			)
			.then(function (result) {
				console.log(
					`Received ${result.length} additional messages from server`,
				);
				result.forEach(function (content) {
					FormatMessageForModeration(content);
				});
				window.Masonry.resizeAllGridItems();
			});
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
				}, 500);
			}
		});
	}

	function showModerationPanel(ev) {
		var hovered = ev.target.closest('article');

		// Remove all moderationAction elements inside of articles
		var panels = document.querySelectorAll('article #moderationAction');
		panels.forEach(function (panel) {
			panel.remove();
		});

		if (hovered == null || hovered.querySelector('#moderationAction')) return;

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
				let approveFunc = function () {
					connection.invoke(
						'SetStatus',
						hovered.getAttribute('data-provider'),
						hovered.getAttribute('data-providerid'),
						ModerationState.Approved,
					);
					hoverPanel.remove();
					hovered.classList.remove('status-rejected');
					hovered.classList.add('status-approved');
				};

				if (hovered.classList.contains('status-rejected')) {
					// Confirm that we are flipping this
					swal({
						title: 'Are you sure?',
						text: 'This message was previously rejected. Are you sure you want to approve it?',
						icon: 'warning',
						buttons: true,
						dangerMode: true,
					}).then((willApprove) => {
						if (willApprove) {
							approveFunc();
						}
					});
				} else {
					approveFunc();
				}
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
				hovered.classList.add('status-humanmod');
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
				}, 500);
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
		// Don't double add the moderator
		if (document.getElementById('moderator-' + moderator.email)) return;

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

	function DisableContextMenu() {
		document.addEventListener('contextmenu', ev => ev.preventDefault());
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

	function HandleKeyPress(ev) {
		let height = 0;
		try {
			height = document.querySelector('article .bi').offsetHeight;
		} catch {
			return;
		}

		function PauseNewContentFor5Seconds() {
			window.clearTimeout(pauseTimeout);
			if (!paused) {
				paused = true;
				rolloverPause = true;
				FormatPauseButton();
			}

			pauseTimeout = window.setTimeout(() => {
				paused = false;
				rolloverPause = false;
				FormatPauseButton();
				ResumeFromPause();
			}, 5000);
		}

		switch (ev.key) {
			case 'p':
				paused = !paused;
				FormatPauseButton();
				if (!paused) ResumeFromPause();
				break;
			// for h,j,k,l and the arrow keys start navigating the cursor
			case 'h':
			case 'ArrowLeft':
				// move the cursor left
				if (cursorProviderId == null) {
					cursorProviderId =
						taggedContent.firstElementChild.getAttribute('data-providerid');
				} else {
					const thisOne = document.querySelector(
						`[data-providerid='${cursorProviderId}']`,
					);
					const rect = thisOne.getBoundingClientRect();

					var above = null;
					for (var j = 0; j < 20; j++) {
						// Look for an ARTICLE to the left of the current cursor position, if not found, look 10 pixels lower for an article

						const aboveElements = document.elementsFromPoint(
							rect.x - height,
							rect.y + j * 10,
						);

						// inspect the elements in aboveElements and assign the variable above to the first article element
						for (var i = 0; i < aboveElements.length; i++) {
							if (aboveElements[i].tagName == 'ARTICLE') {
								above = aboveElements[i];
								break;
							}
						}

						if (above != null && above.tagName == 'ARTICLE') break;
					}

					// check if above is an article element
					if (above && above.tagName == 'ARTICLE') {
						document
							.querySelector(`[data-providerid='${cursorProviderId}']`)
							.classList.remove('keyboard-cursor');
						cursorProviderId = above.getAttribute('data-providerid');
					}
				}
				RemoveActivePanel();
				PauseNewContentFor5Seconds();

				break;
			case 'l':
			case 'ArrowRight':
				// move the cursor right
				if (cursorProviderId == null) {
					cursorProviderId =
						taggedContent.firstElementChild.getAttribute('data-providerid');
				} else {
					const thisOne = document.querySelector(
						`[data-providerid='${cursorProviderId}']`,
					);
					const rect = thisOne.getBoundingClientRect();

					var above = null;
					for (var j = 0; j < 20; j++) {
						const aboveElements = document.elementsFromPoint(
							rect.x + rect.width + height,
							rect.y + j * 10,
						);

						// inspect the elements in aboveElements and assign the variable above to the first article element
						for (var i = 0; i < aboveElements.length; i++) {
							if (aboveElements[i].tagName == 'ARTICLE') {
								above = aboveElements[i];
								break;
							}
						}

						if (above != null && above.tagName == 'ARTICLE') break;
					}

					// check if above is an article element
					if (above && above.tagName == 'ARTICLE') {
						document
							.querySelector(`[data-providerid='${cursorProviderId}']`)
							.classList.remove('keyboard-cursor');
						cursorProviderId = above.getAttribute('data-providerid');
					}
				}
				RemoveActivePanel();
				PauseNewContentFor5Seconds();

				break;
			case 'k':
			case 'ArrowUp':
				// move the cursor up
				if (cursorProviderId == null) {
					cursorProviderId =
						taggedContent.firstElementChild.getAttribute('data-providerid');
				} else {
					for (var i = 0; i < 5; i++) {
						const thisOne = document.querySelector(
							`[data-providerid='${cursorProviderId}']`,
						);
						const rect = thisOne.getBoundingClientRect();
						const aboveElements = document.elementsFromPoint(
							rect.x,
							rect.y - height,
						);

						// inspect the elements in aboveElements and assign the variable above to the first article element
						var above = null;
						for (var i = 0; i < aboveElements.length; i++) {
							if (aboveElements[i].tagName == 'ARTICLE') {
								above = aboveElements[i];
								break;
							}
						}

						// check if above is an article element
						if (above && above.tagName == 'ARTICLE') {
							document
								.querySelector(`[data-providerid='${cursorProviderId}']`)
								.classList.remove('keyboard-cursor');
							cursorProviderId = above.getAttribute('data-providerid');
							break;
						}

						document.getElementById('taggedContent').scrollBy(0, -72);
					}
				}

				document
					.querySelector(`[data-providerid='${cursorProviderId}']`)
					.classList.add('keyboard-cursor');

				RemoveActivePanel();
				PauseNewContentFor5Seconds();
				break;
			case 'j':
			case 'ArrowDown':
				// move the cursor down
				if (cursorProviderId == null) {
					cursorProviderId =
						taggedContent.firstElementChild.getAttribute('data-providerid');
				} else {
					for (var i = 0; i < 5; i++) {
						const thisOne = document.querySelector(
							`[data-providerid='${cursorProviderId}']`,
						);
						const rect = thisOne.getBoundingClientRect();
						const aboveElements = document.elementsFromPoint(
							rect.x,
							rect.y + rect.height + height,
						);

						// inspect the elements in aboveElements and assign the variable above to the first article element
						var above = null;
						for (var i = 0; i < aboveElements.length; i++) {
							if (aboveElements[i].tagName == 'ARTICLE') {
								above = aboveElements[i];
								break;
							}
						}

						// check if above is an article element
						if (above && above.tagName == 'ARTICLE') {
							document
								.querySelector(`[data-providerid='${cursorProviderId}']`)
								.classList.remove('keyboard-cursor');
							cursorProviderId = above.getAttribute('data-providerid');
							break;
						}

						try {
							let taggedContent = document.getElementById('taggedContent');
							let tcHeight = taggedContent.scrollHeight;
							if (
								taggedContent.scrollTop + taggedContent.offsetHeight + height >
								tcHeight
							)
								return;
							taggedContent.scrollBy(0, height);
						} catch (ex) {
							break;
						}
					}
				}

				RemoveActivePanel();
				PauseNewContentFor5Seconds();
				break;
			case 'Enter':
				if (document.querySelector('.active_panel')) {
					RemoveActivePanel();
				} else {
					// bring up the moderation panel
					const card = document.querySelector(
						`[data-providerid='${cursorProviderId}']`,
					);
					if (card) showModerationPanel({ target: card });
				}
				break;
			case 'y':
				if (document.querySelector('.active_panel') == null) return;

				var thisCard = document.querySelector(
					`[data-providerid='${cursorProviderId}']`,
				);

				// Approve the current message
				let approveFunc = function () {
					connection.invoke(
						'SetStatus',
						thisCard.getAttribute('data-provider'),
						thisCard.getAttribute('data-providerid'),
						ModerationState.Approved,
					);
					thisCard.classList.remove('status-rejected');
					thisCard.classList.add('status-approved');
				};

				if (thisCard.classList.contains('status-rejected')) {
					// Confirm that we are flipping this
					swal({
						title: 'Are you sure?',
						text: 'This message was previously rejected. Are you sure you want to approve it?',
						icon: 'warning',
						buttons: true,
						dangerMode: true,
					}).then((willApprove) => {
						if (willApprove) {
							approveFunc();
						}
					});
				} else {
					approveFunc();
				}

				break;
			case 'n':
				if (document.querySelector('.active_panel') == null) return;

				let rejectCard = document.querySelector(
					`[data-providerid='${cursorProviderId}']`,
				);

				connection.invoke(
					'SetStatus',
					rejectCard.getAttribute('data-provider'),
					rejectCard.getAttribute('data-providerid'),
					ModerationState.Rejected,
				);
				rejectCard.classList.remove('status-approved');
				rejectCard.classList.add('status-rejected');
				rejectCard.classList.add('status-humanmod');

				break;
		}

		// Set the new cursor position
		if (
			cursorProviderId != null &&
			document.querySelector(`[data-providerid='${cursorProviderId}']`)
		) {
			document
				.querySelector(`[data-providerid='${cursorProviderId}']`)
				.classList.add('keyboard-cursor');
		}
	}

	const t = {
		Tags: [],

		ActivateKeyboardNavigation: function () {
			window.onkeydown = HandleKeyPress;
		},

		MapProviderToIconClass: function (provider) {
			return MapProviderToIcon(provider);
		},

		FormatContentWithEmotes: function (content) {
			return FormatContextWithEmotes(content);
		},

		FilterByApprovalStatus: function (status) {
			let taggedContent = document.getElementById('taggedContent');
			approvedFilterStatus = status;
			switch (status) {
				case ApprovalFilter.All:
					taggedContent.classList.remove('filter-approvedOnly');
					taggedContent.classList.remove('filter-rejectedOnly');
					taggedContent.classList.remove('filter-needsModeration');
					break;
				case ApprovalFilter.Approved:
					taggedContent.classList.add('filter-approvedOnly');
					taggedContent.classList.remove('filter-rejectedOnly');
					taggedContent.classList.remove('filter-needsModeration');
					break;
				case ApprovalFilter.Rejected:
					taggedContent.classList.remove('filter-approvedOnly');
					taggedContent.classList.add('filter-rejectedOnly');
					taggedContent.classList.remove('filter-needsModeration');
					break;
				case ApprovalFilter.Unmoderated:
					taggedContent.classList.remove('filter-approvedOnly');
					taggedContent.classList.remove('filter-rejectedOnly');
					taggedContent.classList.add('filter-needsModeration');
					break;
			}

			LoadAdditionalContentForFilters();
		},

		ListenForWaterfallContent: async function (tags) {
			tagCsv = encodeURI(tags);
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

			// Listen for the DisplayOverlay event and display the modal for the selected message
			connection.on('DisplayOverlay', (content) => {
				var item = document.querySelector(
					`[data-providerid='${content.providerId}']`,
				);
				if (item) {
					item.click();
				}
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

			DisableContextMenu();
		},

		ListenForModerationContent: async function (tag) {
			tagCsv = encodeURI(tag);

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

			connection.invoke('GetCurrentModerators').then(function (result) {
				result.forEach(function (moderator) {
					AddModerator(moderator);
				});
				window.Masonry.resizeAllGridItems();
			});
		},

		InitializeProviderFilter: function (providers) {
			providerFilter = providers;
		},

		ToggleProviderFilter: function (provider) {
			// console.log(`Before Toggle: ${providerFilter} -- toggling ${provider}`);

			if (providerFilter.includes(provider)) {
				providerFilter = providerFilter.filter(function (item) {
					return item != provider;
				});

				var style = document.getElementById(`providerFilter-${provider}`);
				if (style) style.remove();

				// Add a css rule to the page
				var style = document.createElement('style');
				style.setAttribute('id', `providerFilter-${provider}`);
				style.innerHTML = `article[data-provider='${provider}'] { display: none!important; }`;
				document.head.appendChild(style);
			} else {
				providerFilter.push(provider);
				// Remove the css rule from the page
				var style = document.getElementById(`providerFilter-${provider}`);
				if (style) style.remove();

				var style = document.createElement('style');
				style.setAttribute('id', `providerFilter-${provider}`);
				style.innerHTML = `article[data-provider='${provider}'] { display: grid; }`;
				document.head.appendChild(style);
			}

			if (providerFilter.length > 0) LoadAdditionalContentForFilters();
		},
	};

	window.TagzApp = window.TagzApp || t;
})();
