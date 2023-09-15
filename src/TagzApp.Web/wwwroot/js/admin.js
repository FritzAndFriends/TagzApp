(function () {

	const userCustomization = {
		WaterfallEditor: null
	}

	const a = {

		ConfigureUserCustomization: function () {

			userCustomization.WaterfallEditor = new SimpleMDE({ element: document.getElementById("WaterfallHeaderMarkdown")})

		}

	};

	window.Admin = window.Admin || a;

})();
