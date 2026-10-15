# Minimal Chrome DevTools Protocol helper for testing the Web build in a
# separate, non-throttled Chrome profile (the in-app browser pane throttles
# requestAnimationFrame while hidden, which makes Unity run at ~1 fps).
#
#   python cdp.py launch <url> [--headful]  start Chrome (own profile, port 9333)
#   python cdp.py log <file>                 follow console output into <file> (runs until killed)
#   python cdp.py shot <file.png>            screenshot of the page
#   python cdp.py click <x> <y>              mouse click in CSS pixels
#   python cdp.py tap <x> <y>                touch tap (no mouse events)
#   python cdp.py eval <js>                  evaluate JS, print JSON result
#   python cdp.py goto <url>                 navigate
#   python cdp.py close                      close Chrome
import base64, json, os, subprocess, sys, time, urllib.request
import websocket

PORT = 9334
# Chrome would not open a second debugging instance next to the user's running
# Chrome on this machine; Edge (same Chromium engine) does.
CHROME = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
PROFILE = os.environ.get("PEGLIN_PROFILE") or os.path.join(os.environ.get("TEMP", "."), "peglin-edge-profile")


def page_ws():
    for _ in range(50):
        try:
            tabs = json.load(urllib.request.urlopen(f"http://127.0.0.1:{PORT}/json"))
            for t in tabs:
                if t.get("type") == "page":
                    return t["webSocketDebuggerUrl"]
        except Exception:
            pass
        time.sleep(0.2)
    raise SystemExit("no Chrome page on port %d" % PORT)


class Session:
    def __init__(self):
        self.ws = websocket.create_connection(page_ws(), timeout=120, suppress_origin=True)
        self.n = 0

    def call(self, method, **params):
        self.n += 1
        self.ws.send(json.dumps({"id": self.n, "method": method, "params": params}))
        while True:
            msg = json.loads(self.ws.recv())
            if msg.get("id") == self.n:
                if "error" in msg:
                    raise RuntimeError(msg["error"])
                return msg.get("result", {})


def fmt_arg(a):
    if "value" in a:
        return str(a["value"])
    return a.get("description", a.get("type", ""))


def main():
    cmd, args = sys.argv[1], sys.argv[2:]
    if cmd == "launch":
        flags = [CHROME, f"--remote-debugging-port={PORT}", f"--user-data-dir={PROFILE}",
                 "--no-first-run", "--no-default-browser-check", "--remote-allow-origins=*",
                 "--disable-background-timer-throttling", "--disable-renderer-backgrounding",
                 "--disable-backgrounding-occluded-windows", "--window-size=1280,800",
                 "--enable-unsafe-swiftshader", "--autoplay-policy=no-user-gesture-required"]
        if "--headful" not in args:
            flags += ["--headless=new", "--use-angle=d3d11", "--enable-gpu"]
        flags.append(args[0])
        subprocess.Popen(flags, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        page_ws()
        print("launched")
    elif cmd == "log":
        s = Session()
        s.call("Runtime.enable")
        s.call("Log.enable")
        s.ws.settimeout(None)
        with open(args[0], "a", encoding="utf-8") as f:
            while True:
                msg = json.loads(s.ws.recv())
                m, p = msg.get("method"), msg.get("params", {})
                line = None
                if m == "Runtime.consoleAPICalled":
                    line = "[%s] %s" % (p["type"], " ".join(fmt_arg(a) for a in p.get("args", [])))
                elif m == "Runtime.exceptionThrown":
                    d = p["exceptionDetails"]
                    line = "[exception] %s %s" % (d.get("text", ""), d.get("exception", {}).get("description", ""))
                elif m == "Log.entryAdded":
                    e = p["entry"]
                    line = "[%s/%s] %s %s" % (e["source"], e["level"], e["text"], e.get("url", ""))
                if line:
                    f.write(time.strftime("%H:%M:%S ") + line.rstrip() + "\n")
                    f.flush()
    elif cmd == "shot":
        s = Session()
        data = s.call("Page.captureScreenshot", format="png")["data"]
        open(args[0], "wb").write(base64.b64decode(data))
        print(args[0])
    elif cmd == "click":
        s = Session()
        x, y = float(args[0]), float(args[1])
        for t in ("mouseMoved", "mousePressed", "mouseReleased"):
            s.call("Input.dispatchMouseEvent", type=t, x=x, y=y, button="left", clickCount=1)
            time.sleep(0.05)
        print("clicked", x, y)
    elif cmd == "move":
        Session().call("Input.dispatchMouseEvent", type="mouseMoved", x=float(args[0]), y=float(args[1]))
        print("moved", args[0], args[1])
    elif cmd == "tap":
        # touch-only input, as on a phone (no mouse events are sent)
        s = Session()
        s.call("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=5)
        x, y = float(args[0]), float(args[1])
        s.call("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
        time.sleep(0.08)
        s.call("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
        print("tapped", x, y)
    elif cmd == "eval":
        s = Session()
        r = s.call("Runtime.evaluate", expression=" ".join(args), returnByValue=True, awaitPromise=True)
        print(json.dumps(r.get("result", {}).get("value", r), ensure_ascii=False))
    elif cmd == "goto":
        Session().call("Page.navigate", url=args[0])
        print("navigated")
    elif cmd == "close":
        try:
            Session().call("Browser.close")
        except Exception:
            pass
        print("closed")


if __name__ == "__main__":
    main()
