(function () {
	const userCustomization = {
		WaterfallEditor: null,
	};

	const a = {
		ConfigureUserCustomization: function () {
			userCustomization.WaterfallEditor = new SimpleMDE({
				element: document.getElementById('WaterfallHeaderMarkdown'),
				toolbar: [
					'bold',
					'italic',
					'|',
					'heading',
					'heading-smaller',
					'heading-bigger',
					'|',
					'heading-1',
					'heading-2',
					'heading-3',
					'|',
					'side-by-side',
					'fullscreen',
					'|',
					'guide',
				],
			});
		},
	};

	window.Admin = window.Admin || a;
})();
