// Connection handler for TagzApp custom error UI
let connectionHandler = null;

export function initializeConnectionHandler(dotnetReference) {
	connectionHandler = dotnetReference;

	// Monitor Blazor connection state
	if (window.Blazor) {
		// For Blazor Server
		window.Blazor.addEventListener('reconnecting', () => {
			console.log('Blazor: Reconnecting...');
			if (connectionHandler) {
				connectionHandler.invokeMethodAsync(
					'OnConnectionStateChanged',
					'connecting',
					1,
				);
			}
		});

		window.Blazor.addEventListener('reconnected', () => {
			console.log('Blazor: Reconnected');
			if (connectionHandler) {
				connectionHandler.invokeMethodAsync(
					'OnConnectionStateChanged',
					'connected',
				);
			}
		});

		window.Blazor.addEventListener('disconnected', () => {
			console.log('Blazor: Disconnected');
			if (connectionHandler) {
				connectionHandler.invokeMethodAsync(
					'OnConnectionStateChanged',
					'disconnected',
				);
			}
		});
	}

	// Monitor SignalR connections if available
	if (window.tagzAppSignalR) {
		window.tagzAppSignalR.forEach((connection) => {
			connection.onreconnecting(() => {
				console.log('SignalR: Reconnecting...');
				if (connectionHandler) {
					connectionHandler.invokeMethodAsync(
						'OnConnectionStateChanged',
						'connecting',
						1,
					);
				}
			});

			connection.onreconnected(() => {
				console.log('SignalR: Reconnected');
				if (connectionHandler) {
					connectionHandler.invokeMethodAsync(
						'OnConnectionStateChanged',
						'connected',
					);
				}
			});

			connection.onclose(() => {
				console.log('SignalR: Connection closed');
				if (connectionHandler) {
					connectionHandler.invokeMethodAsync(
						'OnConnectionStateChanged',
						'disconnected',
					);
				}
			});
		});
	}

	// Listen for online/offline events
	window.addEventListener('online', () => {
		console.log('Network: Online');
		// Attempt to reconnect when network comes back
		setTimeout(() => {
			if (connectionHandler) {
				connectionHandler.invokeMethodAsync(
					'OnConnectionStateChanged',
					'connecting',
					1,
				);
			}
		}, 1000);
	});

	window.addEventListener('offline', () => {
		console.log('Network: Offline');
		if (connectionHandler) {
			connectionHandler.invokeMethodAsync(
				'OnConnectionStateChanged',
				'disconnected',
			);
		}
	});
}

export function attemptReconnection() {
	console.log('Manual reconnection attempt');

	// Try to trigger Blazor reconnection
	if (window.Blazor && window.Blazor.reconnect) {
		try {
			window.Blazor.reconnect();
		} catch (e) {
			console.warn('Could not trigger Blazor reconnect:', e);
		}
	}

	// Try to reconnect SignalR connections
	if (window.tagzAppSignalR) {
		window.tagzAppSignalR.forEach(async (connection) => {
			try {
				if (connection.state === 'Disconnected') {
					await connection.start();
				}
			} catch (e) {
				console.warn('Could not reconnect SignalR:', e);
			}
		});
	}

	// Fallback: reload if nothing else works
	setTimeout(() => {
		if (connectionHandler) {
			connectionHandler.invokeMethodAsync(
				'OnConnectionStateChanged',
				'disconnected',
			);
		}
	}, 5000);
}

// Global error handler for unhandled JavaScript errors
window.addEventListener('error', (event) => {
	console.error('Global JavaScript error:', event.error);

	// Don't show connection errors for minor script issues
	if (event.error && event.error.name !== 'ChunkLoadError') {
		return;
	}

	if (connectionHandler) {
		// For chunk load errors, suggest a page refresh
		connectionHandler.invokeMethodAsync(
			'OnConnectionStateChanged',
			'disconnected',
		);
	}
});

// Global promise rejection handler
window.addEventListener('unhandledrejection', (event) => {
	console.error('Unhandled promise rejection:', event.reason);

	// Don't show errors for non-connection related issues
	if (
		event.reason &&
		typeof event.reason === 'string' &&
		(event.reason.includes('fetch') || event.reason.includes('connection'))
	) {
		if (connectionHandler) {
			connectionHandler.invokeMethodAsync(
				'OnConnectionStateChanged',
				'disconnected',
			);
		}
	}
});
