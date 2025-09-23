// SignalR connection registration for TagzApp custom error UI
export function registerSignalRConnection(connection) {
    if (!window.tagzAppSignalR) {
        window.tagzAppSignalR = [];
    }
    
    if (!window.tagzAppSignalR.includes(connection)) {
        window.tagzAppSignalR.push(connection);
        console.log('SignalR connection registered for error monitoring');
    }
}

export function unregisterSignalRConnection(connection) {
    if (window.tagzAppSignalR) {
        const index = window.tagzAppSignalR.indexOf(connection);
        if (index > -1) {
            window.tagzAppSignalR.splice(index, 1);
            console.log('SignalR connection unregistered from error monitoring');
        }
    }
}
