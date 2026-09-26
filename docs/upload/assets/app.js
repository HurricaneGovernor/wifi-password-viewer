/* Shared script for every language version.
   Links, texts and SEO tags are baked into the HTML by build.mjs;
   this file only adds behaviour. */
(function () {
  const L = JSON.parse(document.getElementById("l10n").textContent);
  const repo = document.documentElement.dataset.repo || "";

  /* Live star count; keeps the fallback if the repo is not set or the API is unavailable */
  if (repo && !/^USER\//.test(repo)) {
    fetch("https://api.github.com/repos/" + repo)
      .then(r => (r.ok ? r.json() : null))
      .then(d => {
        if (d && d.stargazers_count >= 200) {
          const n = d.stargazers_count.toLocaleString(document.documentElement.lang);
          document.querySelectorAll("[data-stars]").forEach(el => (el.textContent = n));
        }
      })
      .catch(() => {});
  }

  /* Language menu: close on outside click and Escape */
  const lang = document.querySelector(".lang");
  if (lang) {
    document.addEventListener("click", e => { if (!lang.contains(e.target)) lang.open = false; });
    document.addEventListener("keydown", e => {
      if (e.key === "Escape" && lang.open) { lang.open = false; lang.querySelector("summary").focus(); }
    });
  }

  /* Hero demo: a working copy of the app's network list (app UI stays in English, like the real app) */
  const nets = [
    { name: "MyHome_5G",       note: "Connected, 5 GHz",  sec: "WPA2-Personal", pw: "sunflower-2024!", on: true },
    { name: "Office_WiFi",     note: "Saved 2 days ago",  sec: "WPA3-Personal", pw: "Qx7mRt92pLa#" },
    { name: "Grandma_House",   note: "Saved 3 days ago",  sec: "WPA2-Personal", pw: "rosegarden1957" },
    { name: "CoffeeShop_Free", note: "Open network",      sec: "Open",          pw: null },
    { name: "Hotel_Guest_204", note: "Saved 1 month ago", sec: "WPA2-Personal", pw: "welcome204" }
  ];
  const list = document.querySelector(".nets");
  if (!list) return;
  const dots = "•".repeat(10);
  const svg = id => `<svg class="icon"><use href="#${id}"/></svg>`;

  nets.forEach(n => {
    const row = document.createElement("div");
    row.className = "net" + (n.on ? " is-connected" : "");
    row.innerHTML = `
      <div class="net-name">
        <svg viewBox="0 0 24 24" fill="currentColor"><use href="#i-wifi"/></svg>
        <span><b>${n.name}</b><small>${n.note}</small></span>
      </div>
      <span class="tag">${n.sec}</span>
      <span class="pw ${n.pw ? "hidden-pw" : "none"}">${n.pw ? dots : "No password"}</span>
      <span class="net-actions">
        ${n.pw ? `<button class="ibtn eye" type="button" aria-pressed="false" aria-label="${L.show} ${n.name}">${svg("i-eye-closed")}</button>` : ""}
        ${n.pw ? `<button class="ibtn cp" type="button" aria-label="${L.copy} ${n.name}">${svg("i-copy")}</button>` : ""}
      </span>`;
    list.appendChild(row);

    if (!n.pw) return;
    const pwEl = row.querySelector(".pw");
    const eye = row.querySelector(".eye");
    const cp = row.querySelector(".cp");

    const set = shown => {
      eye.setAttribute("aria-pressed", shown);
      eye.setAttribute("aria-label", (shown ? L.hide : L.show) + " " + n.name);
      eye.innerHTML = svg(shown ? "i-eye" : "i-eye-closed");
      pwEl.textContent = shown ? n.pw : dots;
      pwEl.classList.toggle("hidden-pw", !shown);
      pwEl.classList.remove("reveal");
      if (shown) { void pwEl.offsetWidth; pwEl.classList.add("reveal"); }
    };
    eye.addEventListener("click", () => set(eye.getAttribute("aria-pressed") !== "true"));
    cp.addEventListener("click", () => {
      if (navigator.clipboard) navigator.clipboard.writeText(n.pw).catch(() => {});
      cp.classList.add("copied");
      cp.innerHTML = svg("i-check");
      cp.setAttribute("aria-label", L.copied);
      setTimeout(() => {
        cp.classList.remove("copied");
        cp.innerHTML = svg("i-copy");
        cp.setAttribute("aria-label", L.copy + " " + n.name);
      }, 1400);
    });

    // One orchestrated moment: the connected network reveals itself on load
    if (n.on) setTimeout(() => set(true), 900);
  });
})();
