(function () {


	var waterfallUi = {

		setupWaterfall: function () {

			const floatingHeader = document.getElementById("floatingHeader");

			document.getElementById("headerButton").addEventListener("click", function () {
				window.setTimeout(() => {
					floatingHeader.addEventListener("mouseleave", (el) => {
						floatingHeader.classList.remove("scrollIn");
					});
					floatingHeader.addEventListener("click", (el) => {
						floatingHeader.classList.remove("scrollIn");
					});
				}, 3000);
				floatingHeader.classList.add("scrollIn");
			});

		}

	};

	window.WaterfallUi = waterfallUi;

})();
