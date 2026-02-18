window.EventDisplay = (() => {
	const SCROLL_SPEED_MS = 30;
	const SCROLL_INCREMENT_PX = 1;
	const PAUSE_AT_BOTTOM_MS = 3000;
	const PAUSE_ON_NEW_CONTENT_MS = 2000;

	let scrollInterval = null;
	let isPaused = false;
	let scrollContainer = null;
	let mutationObserver = null;

	function startAutoScroll() {
		scrollContainer = document.getElementById('taggedContent');
		if (!scrollContainer) {
			console.warn('EventDisplay: #taggedContent not found, retrying in 500ms');
			setTimeout(startAutoScroll, 500);
			return;
		}

		// Setup MutationObserver to detect new content
		mutationObserver = new MutationObserver(() => {
			pauseScroll(PAUSE_ON_NEW_CONTENT_MS);
		});

		mutationObserver.observe(scrollContainer, {
			childList: true,
			subtree: false
		});

		// Start the scroll loop
		scrollInterval = setInterval(() => {
			if (isPaused) return;

			const atBottom = scrollContainer.scrollTop + scrollContainer.clientHeight >= scrollContainer.scrollHeight - 5;

			if (atBottom) {
				pauseScroll(PAUSE_AT_BOTTOM_MS);
				setTimeout(() => {
					scrollContainer.scrollTo({ top: 0, behavior: 'smooth' });
				}, PAUSE_AT_BOTTOM_MS);
			} else {
				scrollContainer.scrollTop += SCROLL_INCREMENT_PX;
			}
		}, SCROLL_SPEED_MS);
	}

	function pauseScroll(durationMs) {
		isPaused = true;
		setTimeout(() => {
			isPaused = false;
		}, durationMs);
	}

	function setupPage() {
		if (window.Masonry && window.Masonry.setupPage) {
			window.Masonry.setupPage();
		}
	}

	function cleanup() {
		if (scrollInterval) {
			clearInterval(scrollInterval);
			scrollInterval = null;
		}
		if (mutationObserver) {
			mutationObserver.disconnect();
			mutationObserver = null;
		}
	}

	return {
		startAutoScroll,
		setupPage,
		cleanup
	};
})();
