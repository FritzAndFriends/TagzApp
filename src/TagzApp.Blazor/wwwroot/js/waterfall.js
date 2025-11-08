'use strict';
(function () {
	const messages = [];
	let pauseButtonRef = null;
	let pauseTimeout = null;
	let rollOverPause = false;
	let isPaused = false;
	let taggedContent = null;

	async function PauseNewContentForDuration(duration) {
		if (!rollOverPause && !isPaused) {
			window.clearTimeout(pauseTimeout);
			rollOverPause = true;
			isPaused = true;
			pauseButtonRef.invokeMethodAsync('SetPauseState', true);

			pauseTimeout = window.setTimeout(() => {
				rollOverPause = false;
				isPaused = false;
				pauseButtonRef.invokeMethodAsync('SetPauseState', false);
			}, duration);
		}
	}

	let cursorProviderId = null;
	function HandleMoveCursor(key) {
		let height = 0;
		try {
			height = document.querySelector('article .bi').offsetHeight;
		} catch {
			return;
		}

		switch (key) {
			//case 'h':
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
						let currentMsg = messages.find((msg) => msg.id == cursorProviderId);
						currentMsg.message.invokeMethodAsync('SetCursorFocus', false);
						cursorProviderId = above.getAttribute('data-providerid');

						break;
					}
				}
				PauseNewContentForDuration(5000);

				break;
			//case 'l':
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
						let currentMsg = messages.find((msg) => msg.id == cursorProviderId);
						currentMsg.message.invokeMethodAsync('SetCursorFocus', false);
						cursorProviderId = above.getAttribute('data-providerid');
						break;
					}
				}
				PauseNewContentForDuration(5000);

				break;
			//case 'k':
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
							let currentMsg = messages.find(
								(msg) => msg.id == cursorProviderId,
							);
							currentMsg.message.invokeMethodAsync('SetCursorFocus', false);
							cursorProviderId = above.getAttribute('data-providerid');
							break;
						}

						// dont scroll up if we are at the top of the page
						if (taggedContent.scrollTop != 0) taggedContent.scrollBy(0, -72);
					}
				}

				PauseNewContentForDuration(5000);
				break;
			//case 'j':
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
							let currentMsg = messages.find(
								(msg) => msg.id == cursorProviderId,
							);
							currentMsg.message.invokeMethodAsync('SetCursorFocus', false);
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

				PauseNewContentForDuration(5000);
				break;
			//	case 'Enter':
			//		if (document.querySelector('.active_panel')) {
			//			RemoveActivePanel();
			//		} else {
			//			// bring up the moderation panel
			//			const card = document.querySelector(
			//				`[data-providerid='${cursorProviderId}']`,
			//			);
			//			if (card) showModerationPanel({ target: card });
			//		}
			//		break;
		}

		console.log('CursorProviderId: ' + cursorProviderId);

		// Set the new cursor position
		if (
			cursorProviderId != null &&
			document.querySelector(`[data-providerid='${cursorProviderId}']`)
		) {
			let currentMsg = messages.find((msg) => msg.id == cursorProviderId);
			currentMsg.message.invokeMethodAsync('SetCursorFocus', true);
		}
	}

	var waterfallUi = {
		setupWaterfall: function () {
			const floatingHeader = document.getElementById('floatingHeader');

			document
				.getElementById('headerButton')
				.addEventListener('click', function () {
					window.setTimeout(() => {
						floatingHeader.addEventListener('mouseleave', (el) => {
							floatingHeader.classList.remove('scrollIn');
						});
						floatingHeader.addEventListener('click', (el) => {
							floatingHeader.classList.remove('scrollIn');
						});
					}, 3000);
					floatingHeader.classList.add('scrollIn');
				});
		},

		FixEmbedImage: function (img) {
			var theArticle = img.closest('[data-provider]');

			if (theArticle == null) return;

			if (
				theArticle.dataset.provider == 'TWITTER' &&
				!img.src.toString().includes('d.fxtwitter.com')
			) {
				img.src =
					theArticle.dataset.url.replace('twitter.com', 'd.fxtwitter.com') +
					'.jpg';
			} else {
				img.parentElement.style.display = 'none';
				if (img.parentElement.parentElement.classList.contains('showPreview')) {
					img.parentElement.parentElement.classList.remove('showPreview');
					img.parentElement.parentElement.classList.add('show');
				}
			}
		},

		AddMessage: function (id, message) {
			messages.push({ id: id, message: message });
		},

		RegisterPauseButton: function (buttonRef) {
			pauseButtonRef = buttonRef;
		},

		ConfigureKeyboardSupport: function () {
			taggedContent = document.getElementById('taggedContent');

			window.onkeydown = function (e) {
				// Pause/Resume
				if (e.key === 'p') {
					if (pauseButtonRef) {
						pauseButtonRef.invokeMethodAsync('PauseClicked');
						return;
					}
				} else if (e.key === 'y' || e.key === 'n') {
					if (e.key == 'y') {
						if (document.querySelector('.active_panel') == null) return;

						var thisCard = document.querySelector(
							`[data-providerid='${cursorProviderId}']`,
						);
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
					} else {
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
					}
				} else {
					// move cursor to next article in the correct direction
					HandleMoveCursor(e.key);
				}
			};
		},

		PauseNewContent: function (duration) {
			PauseNewContentForDuration(duration);
		},

		SetPauseState: function (paused) {
			isPaused = paused;
			rollOverPause = false;
		},

		ResumeContent: function () {
			if (rollOverPause) {
				window.clearTimeout(pauseTimeout);
				isPaused = false;
				rollOverPause = false;
				pauseButtonRef.invokeMethodAsync('SetPauseState', false);
			}
		},

		Messages: messages,

		MuteVideo: function (video) {
			var video = document.getElementById(video);
			video.muted = true;
		},

		SetTitle: function (newTitle) {
			document.title = newTitle;
		},

		// Detect and mark rightmost column cards for moderation panel positioning
		UpdateRightmostCards: function () {
			const taggedContent = document.getElementById('taggedContent');
			if (!taggedContent) return;

			const articles = Array.from(taggedContent.querySelectorAll('article'));
			if (articles.length === 0) return;

			// Remove existing rightmost class from all articles
			articles.forEach(article => article.classList.remove('rightmost-column'));

			// Group articles by their vertical position (top coordinate)
			const rows = {};
			articles.forEach(article => {
				const rect = article.getBoundingClientRect();
				const top = Math.round(rect.top);
				if (!rows[top]) rows[top] = [];
				rows[top].push({ article, rect });
			});

			// For each row, mark the rightmost article
			Object.values(rows).forEach(row => {
				if (row.length > 0) {
					// Find the article with the maximum right coordinate
					const rightmost = row.reduce((max, current) => 
						current.rect.right > max.rect.right ? current : max
					);
					rightmost.article.classList.add('rightmost-column');
				}
			});
		},
	};

	window.WaterfallUi = waterfallUi;

	// Update rightmost cards on load and resize
	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', () => {
			setTimeout(() => waterfallUi.UpdateRightmostCards(), 100);
		});
	} else {
		setTimeout(() => waterfallUi.UpdateRightmostCards(), 100);
	}

	let resizeTimeout;
	window.addEventListener('resize', () => {
		clearTimeout(resizeTimeout);
		resizeTimeout = setTimeout(() => waterfallUi.UpdateRightmostCards(), 250);
	});

	// Also update when new content is added (use MutationObserver)
	const observer = new MutationObserver(() => {
		clearTimeout(resizeTimeout);
		resizeTimeout = setTimeout(() => waterfallUi.UpdateRightmostCards(), 100);
	});

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', () => {
			const taggedContent = document.getElementById('taggedContent');
			if (taggedContent) {
				observer.observe(taggedContent, { childList: true, subtree: false });
			}
		});
	} else {
		const taggedContent = document.getElementById('taggedContent');
		if (taggedContent) {
			observer.observe(taggedContent, { childList: true, subtree: false });
		}
	}
})();
