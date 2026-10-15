# Serve a Unity Web build folder for local play testing.
#   python serve-web.py <build-folder> [port] [bind-address]
# Bind 127.0.0.1 (default) for this PC only; a LAN address lets a phone on the same Wi-Fi connect.
import functools, http.server, socketserver, sys

folder = sys.argv[1]
port = int(sys.argv[2]) if len(sys.argv) > 2 else 8760
bind = sys.argv[3] if len(sys.argv) > 3 else "127.0.0.1"


class Handler(http.server.SimpleHTTPRequestHandler):
    extensions_map = {**http.server.SimpleHTTPRequestHandler.extensions_map,
                      ".wasm": "application/wasm", ".unityweb": "application/octet-stream"}

    def end_headers(self):
        self.send_header("Cache-Control", "no-cache")
        super().end_headers()


socketserver.ThreadingTCPServer.allow_reuse_address = True
with socketserver.ThreadingTCPServer((bind, port), functools.partial(Handler, directory=folder)) as s:
    print(f"serving {folder} on http://{bind}:{port}/")
    s.serve_forever()
