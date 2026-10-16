// Keep the DOM full-window while avoiding retina-sized, repeatedly reallocated
// drawing buffers in iPad Chrome. Safari/Android/desktop keep Unity defaults.
(() => {
  const ipad = /iPad/.test(navigator.userAgent) ||
    (/Macintosh|MacIntel/.test(navigator.userAgent + " " + navigator.platform) && navigator.maxTouchPoints > 1);
  const enabled = (ipad && /CriOS\//.test(navigator.userAgent)) ||
    new URLSearchParams(location.search).get("display") === "ipad-chrome";
  if (!enabled) return;
  config.matchWebGLToCanvasSize = false;
  config.devicePixelRatio = 1;
  config.webglContextAttributes = {
    preserveDrawingBuffer: true,
    powerPreference: "low-power"
  };

  let resizeTimer;
  function resizeBuffer() {
    const rect = canvas.getBoundingClientRect();
    if (rect.width < 1 || rect.height < 1 || document.hidden) return;
    const scale = Math.min(1, 1280 / Math.max(rect.width, rect.height));
    const width = Math.max(1, Math.round(rect.width * scale));
    const height = Math.max(1, Math.round(rect.height * scale));
    if (canvas.width !== width) canvas.width = width;
    if (canvas.height !== height) canvas.height = height;
  }
  function scheduleResize() {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(resizeBuffer, 250);
  }
  resizeBuffer();
  new ResizeObserver(scheduleResize).observe(canvas);
  // Reapply the budget if the desktop settings call Screen.SetResolution.
  new MutationObserver(scheduleResize).observe(canvas, {attributes: true, attributeFilter: ["width", "height"]});
  window.addEventListener("resize", scheduleResize);
  document.addEventListener("visibilitychange", scheduleResize);

  // The desktop game requests browser fullscreen on interaction. On this
  // platform CSS already fills the viewport; avoid a second display transition.
  for (const element of [canvas, document.getElementById("unity-fullscreen-container")]) {
    if (element.requestFullscreen) element.requestFullscreen = () => Promise.resolve();
    if (element.webkitRequestFullscreen) element.webkitRequestFullscreen = () => {};
  }

  canvas.addEventListener("webglcontextlost", () => {
    showBanner("畫面中斷，請重新整理後繼續。若持續發生，請將錯誤代碼「顯示-01」告訴我。", "error");
    console.error("[WebDisplay] context lost; mode=ipad-chrome");
  });
  console.info("[WebDisplay] iPad Chrome compatibility display enabled");
})();
