(function () {

	const messages = [];

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

		Messages : messages

	};

	window.WaterfallUi = waterfallUi;
})();
