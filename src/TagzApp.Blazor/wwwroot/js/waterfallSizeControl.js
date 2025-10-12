'use strict';
(function () {
	const waterfallSizeControl = {
		setTileWidth: function (width) {
			const taggedContent = document.getElementById('taggedContent');
			if (taggedContent) {
				// Update the grid-template-columns to use the new minimum width
				taggedContent.style.gridTemplateColumns = `repeat(auto-fill, minmax(${width}px, 1fr))`;
				
				// Trigger a resize of all grid items after changing column widths
				if (window.Masonry && window.Masonry.resizeAllGridItems) {
					// Small delay to let the grid reflow
					setTimeout(() => {
						window.Masonry.resizeAllGridItems();
					}, 100);
				}
			}
		},

		setModalScale: function (scalePercent) {
			// Update CSS custom property for modal scaling
			document.documentElement.style.setProperty('--modal-scale', scalePercent / 100);
			
			// Apply scale to modal dialog
			const modalDialog = document.querySelector('.modal-dialog');
			if (modalDialog) {
				modalDialog.style.transform = `scale(${scalePercent / 100})`;
				modalDialog.style.transformOrigin = 'center center';
			}

			// Apply scale to waterfall modal specifically
			const waterfallModal = document.querySelector('.waterfall-modal-content');
			if (waterfallModal) {
				waterfallModal.style.fontSize = `${scalePercent}%`;
			}

			// Apply scale to overlay display
			const overlayDisplay = document.getElementById('overlayDisplay');
			if (overlayDisplay) {
				overlayDisplay.style.transform = `scale(${scalePercent / 100})`;
				overlayDisplay.style.transformOrigin = 'center center';
			}

			const portraitOverlayDisplay = document.getElementById('portraitOverlayDisplay');
			if (portraitOverlayDisplay) {
				portraitOverlayDisplay.style.transform = `scale(${scalePercent / 100})`;
				portraitOverlayDisplay.style.transformOrigin = 'center center';
			}
		},

		getTileWidth: function () {
			const taggedContent = document.getElementById('taggedContent');
			if (taggedContent) {
				const style = window.getComputedStyle(taggedContent);
				const gridTemplate = style.getPropertyValue('grid-template-columns');
				// Extract minmax value from the grid template
				const match = gridTemplate.match(/minmax\((\d+)px,/);
				if (match) {
					return parseInt(match[1]);
				}
			}
			return 275; // Default
		},

		getModalScale: function () {
			const scale = getComputedStyle(document.documentElement)
				.getPropertyValue('--modal-scale');
			return scale ? parseFloat(scale) * 100 : 100;
		}
	};

	window.WaterfallSizeControl = waterfallSizeControl;
})();
