(function () {

	const m = {

		resizeGridItem: function (item) {
			grid = document.getElementById("taggedContent");
			rowHeight = parseInt(window.getComputedStyle(grid).getPropertyValue('grid-auto-rows'));
			rowGap = parseInt(window.getComputedStyle(grid).getPropertyValue('grid-row-gap'));
			var imageSize = (item.querySelector('.contentcard')?.getBoundingClientRect().height ?? 0) + (item.querySelector('.contentcard') ? 6 : 0);
			rowSpan = Math.ceil((item.querySelector('.content').getBoundingClientRect().height + imageSize + 100 + rowGap) / (rowHeight + rowGap));
			item.style.gridRowEnd = "span " + rowSpan;
		},

		resizeAllGridItems: function () {
			allItems = document.querySelectorAll("article");
			for (x = 0; x < allItems.length; x++) {
				m.resizeGridItem(allItems[x]);
			}
		}

	};

	window.Masonry = m;

})();