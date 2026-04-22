// Terminal JavaScript interop
(function () {
    let ws = null;
    let terminal = null;

    window.initTerminal = async function (token) {
        // Load xterm.js dynamically if not already loaded
        if (typeof Terminal === 'undefined') {
            await loadScript('https://cdn.jsdelivr.net/npm/xterm@5.3.0/lib/xterm.js');
            await loadScript('https://cdn.jsdelivr.net/npm/xterm@5.3.0/lib/xterm-fit.js');
            await loadStyle('https://cdn.jsdelivr.net/npm/xterm@5.3.0/css/xterm.css');
        }

        terminal = new Terminal({
            cursorBlink: true,
            fontSize: 14,
            fontFamily: 'Consolas, "Courier New", monospace',
            theme: {
                background: '#1e1e1e',
                foreground: '#ffffff',
                cursor: '#ffffff'
            }
        });

        terminal.open(document.getElementById('terminal'));
        terminal.fit();

        // Connect to WebSocket
        const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
        const host = window.location.hostname;
        const port = window.location.port || (window.location.protocol === 'https:' ? '443' : '80');
        const wsUrl = `ws://${window.location.hostname}:1408/ws/terminal?token=${token}`;

        ws = new WebSocket(wsUrl);

        ws.onopen = () => {
            terminal.write('Connected to MrBur Terminal\r\n\r\n');
            terminal.write('Welcome! You are now connected to your VPS.\r\n\r\n');
            connected = true;
        };

        ws.onclose = (event) => {
            terminal.write('\r\n\r\n[Connection closed]\r\n');
            connected = false;
        };

        ws.onerror = (error) => {
            terminal.write('\r\n[WebSocket error]\r\n');
        };

        ws.onmessage = (event) => {
            terminal.write(event.data);
        };

        terminal.onData((data) => {
            if (ws && ws.readyState === WebSocket.OPEN) {
                ws.send(data);
            }
        });

        // Handle resize
        window.addEventListener('resize', () => {
            if (terminal) {
                terminal.fit();
            }
        });
    };

    async function loadScript(src) {
        return new Promise((resolve, reject) => {
            const script = document.createElement('script');
            script.src = src;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });
    }

    async function loadStyle(href) {
        return new Promise((resolve, reject) => {
            const link = document.createElement('link');
            link.rel = 'stylesheet';
            link.href = href;
            link.onload = resolve;
            link.onerror = reject;
            document.head.appendChild(link);
        });
    }
})();