// Unit Converter frontend. Plain JavaScript, no build step: it only talks to the API in this same app.
(() => {
  "use strict";

  const API = "api/v1";
  const DEBOUNCE_MS = 250;
  const HISTORY_KEY = "unit-converter:history";
  const SELECTION_KEY = "unit-converter:selection";
  const HISTORY_LIMIT = 8;
  const HISTORY_SAVE_DELAY_MS = 1200;
  const DECIMAL_CHOICES = [0, 1, 2, 3, 4, 6, 8, 10, 12];
  const NUMBER_PATTERN = /^[-+]?(\d+(\.\d*)?|\.\d+)$/;

  // Sensible starting pair per category; falls back to the first two units.
  const DEFAULT_PAIRS = {
    length: ["m", "ft"],
    mass: ["kg", "lb"],
    temperature: ["°C", "°F"],
    volume: ["L", "gal"],
    area: ["m²", "ft²"],
    time: ["h", "min"],
    speed: ["km/h", "mph"],
    "data-storage": ["GB", "GiB"],
  };

  const el = {
    tabs: document.getElementById("category-tabs"),
    form: document.getElementById("converter-form"),
    value: document.getElementById("value"),
    from: document.getElementById("from-unit"),
    to: document.getElementById("to-unit"),
    swap: document.getElementById("swap"),
    decimals: document.getElementById("decimals"),
    result: document.getElementById("result"),
    resultValue: document.getElementById("result-value"),
    resultUnit: document.getElementById("result-unit"),
    resultFormula: document.getElementById("result-formula"),
    copy: document.getElementById("copy"),
    error: document.getElementById("error"),
    allTable: document.querySelector("#all-table tbody"),
    allCaption: document.getElementById("all-caption"),
    history: document.getElementById("history"),
    historyEmpty: document.getElementById("history-empty"),
    clearHistory: document.getElementById("clear-history"),
  };

  const state = {
    category: null,
    units: [],
    debounce: 0,
    historyTimer: 0,
    request: null,
    lastResult: null,
  };

  // ---------- Storage (best effort: private windows may block it) ----------

  const storage = {
    get(key, fallback) {
      try {
        const raw = localStorage.getItem(key);
        return raw ? JSON.parse(raw) : fallback;
      } catch {
        return fallback;
      }
    },
    set(key, value) {
      try {
        localStorage.setItem(key, JSON.stringify(value));
      } catch {
        /* ignore */
      }
    },
  };

  // ---------- API ----------

  async function api(path, options = {}) {
    const response = await fetch(`${API}/${path}`, {
      headers: { Accept: "application/json", ...(options.body ? { "Content-Type": "application/json" } : {}) },
      ...options,
    });
    const body = await response.json().catch(() => null);
    if (!response.ok) {
      throw new Error(problemMessage(body) ?? `Request failed (${response.status}).`);
    }
    return body;
  }

  // Turns an RFC 7807 problem response into one readable sentence.
  function problemMessage(problem) {
    if (!problem) return null;
    if (problem.errors) {
      return Object.values(problem.errors).flat().join(" ");
    }
    return problem.detail ?? problem.title ?? null;
  }

  // ---------- Formatting ----------

  function format(number, decimals) {
    return new Intl.NumberFormat("en-US", { maximumFractionDigits: decimals }).format(number);
  }

  function unitLabel(unit) {
    return unit.name === unit.symbol ? unit.symbol : `${unit.symbol} · ${unit.name}`;
  }

  // ---------- Rendering ----------

  function renderTabs(categories) {
    el.tabs.replaceChildren(
      ...categories.map((category) => {
        const tab = document.createElement("button");
        tab.type = "button";
        tab.className = "tab";
        tab.setAttribute("role", "tab");
        tab.dataset.key = category.key;
        tab.textContent = category.name;
        tab.setAttribute("aria-selected", "false");
        tab.addEventListener("click", () => selectCategory(category.key));
        return tab;
      }),
    );
  }

  function renderUnitOptions(select, selectedSymbol) {
    select.replaceChildren(
      ...state.units.map((unit) => {
        const option = new Option(unitLabel(unit), unit.symbol);
        option.selected = unit.symbol === selectedSymbol;
        return option;
      }),
    );
  }

  function renderResult(conversion, decimals) {
    el.result.classList.remove("is-stale");
    el.resultValue.textContent = format(conversion.result, decimals);
    el.resultUnit.textContent = conversion.to.name;
    el.resultFormula.textContent =
      `${format(conversion.value, 20)} ${conversion.from.symbol} = ${format(conversion.result, decimals)} ${conversion.to.symbol}`;
    el.copy.hidden = false;
  }

  function clearResult() {
    el.resultValue.textContent = "–";
    el.resultUnit.textContent = "";
    el.resultFormula.textContent = "";
    el.copy.hidden = true;
    state.lastResult = null;
  }

  function showError(message) {
    el.error.textContent = message;
    el.error.hidden = !message;
  }

  function renderAllUnits(batch, decimals) {
    const rows = batch.items
      .filter((item) => item.succeeded)
      .map(({ conversion }) => {
        const row = document.createElement("tr");
        row.classList.toggle("is-current", conversion.to.symbol === el.to.value);
        row.title = `Convert to ${conversion.to.name}`;
        row.addEventListener("click", () => {
          el.to.value = conversion.to.symbol;
          scheduleConvert(0);
        });

        const unitCell = document.createElement("td");
        const symbol = document.createElement("strong");
        symbol.textContent = conversion.to.symbol;
        const name = document.createElement("span");
        name.className = "unit-name";
        name.textContent = conversion.to.name;
        unitCell.append(symbol, name);

        const valueCell = document.createElement("td");
        valueCell.className = "num";
        valueCell.textContent = format(conversion.result, decimals);

        row.append(unitCell, valueCell);
        return row;
      });

    el.allTable.replaceChildren(...rows);
    const from = state.units.find((u) => u.symbol === el.from.value);
    el.allCaption.textContent = from ? `${el.value.value.trim()} ${from.symbol} in each unit` : "";
  }

  function renderHistory() {
    const history = storage.get(HISTORY_KEY, []);
    el.history.replaceChildren(
      ...history.map((entry) => {
        const item = document.createElement("li");
        const button = document.createElement("button");
        button.type = "button";
        button.textContent = `${entry.value} ${entry.from} → ${entry.result} ${entry.to}`;
        button.addEventListener("click", () => restore(entry));
        item.append(button);
        return item;
      }),
    );
    el.historyEmpty.hidden = history.length > 0;
    el.clearHistory.hidden = history.length === 0;
  }

  // ---------- Behaviour ----------

  async function selectCategory(key, preferred) {
    state.category = key;
    for (const tab of el.tabs.querySelectorAll(".tab")) {
      tab.setAttribute("aria-selected", String(tab.dataset.key === key));
    }

    try {
      state.units = await api(`categories/${encodeURIComponent(key)}/units`);
    } catch (error) {
      showError(error.message);
      return;
    }

    const symbols = state.units.map((u) => u.symbol);
    const [defaultFrom, defaultTo] = DEFAULT_PAIRS[key] ?? symbols;
    const pick = (wanted, fallback) => (symbols.includes(wanted) ? wanted : fallback);
    const from = pick(preferred?.from, pick(defaultFrom, symbols[0]));
    const to = pick(preferred?.to, pick(defaultTo, symbols[1] ?? symbols[0]));

    renderUnitOptions(el.from, from);
    renderUnitOptions(el.to, to);
    scheduleConvert(0);
  }

  function scheduleConvert(delay = DEBOUNCE_MS) {
    clearTimeout(state.debounce);
    el.result.classList.add("is-stale");
    state.debounce = setTimeout(convert, delay);
  }

  async function convert() {
    const raw = el.value.value.trim();
    const decimals = Number(el.decimals.value);

    if (!raw) {
      el.value.removeAttribute("aria-invalid");
      showError("");
      clearResult();
      el.allTable.replaceChildren();
      el.allCaption.textContent = "";
      return;
    }

    if (!NUMBER_PATTERN.test(raw)) {
      el.value.setAttribute("aria-invalid", "true");
      showError("Enter a number, for example 12 or -3.5.");
      return;
    }
    el.value.removeAttribute("aria-invalid");

    // Cancel any slower request still in flight so results never arrive out of order.
    state.request?.abort();
    const controller = new AbortController();
    state.request = controller;

    const query = new URLSearchParams({ value: raw, from: el.from.value, to: el.to.value, decimals: String(decimals) });

    try {
      const conversion = await api(`conversions?${query}`, { signal: controller.signal });
      showError("");
      renderResult(conversion, decimals);
      state.lastResult = conversion;
      saveSelection();
      // Only record a conversion once the user pauses, not every intermediate keystroke.
      clearTimeout(state.historyTimer);
      state.historyTimer = setTimeout(() => saveHistory(conversion, decimals, raw), HISTORY_SAVE_DELAY_MS);
      await loadAllUnits(raw, decimals, controller.signal);
    } catch (error) {
      if (error.name === "AbortError") return;
      el.result.classList.remove("is-stale");
      clearResult();
      el.allTable.replaceChildren();
      el.allCaption.textContent = "";
      showError(error.message);
    }
  }

  // Uses the batch endpoint to show the value in every unit of the category at once.
  async function loadAllUnits(value, decimals, signal) {
    const items = state.units.map((unit) => ({ value: Number(value), from: el.from.value, to: unit.symbol }));
    const batch = await api("conversions/batch", {
      method: "POST",
      body: JSON.stringify({ items, decimals }),
      signal,
    });
    renderAllUnits(batch, decimals);
  }

  function saveSelection() {
    storage.set(SELECTION_KEY, {
      category: state.category,
      from: el.from.value,
      to: el.to.value,
      decimals: el.decimals.value,
      value: el.value.value.trim(),
    });
  }

  function saveHistory(conversion, decimals, raw) {
    const entry = {
      category: conversion.category,
      value: format(conversion.value, 20),
      from: conversion.from.symbol,
      to: conversion.to.symbol,
      result: format(conversion.result, decimals),
      raw,
    };
    const history = storage
      .get(HISTORY_KEY, [])
      .filter((h) => !(h.raw === entry.raw && h.from === entry.from && h.to === entry.to));
    history.unshift(entry);
    storage.set(HISTORY_KEY, history.slice(0, HISTORY_LIMIT));
    renderHistory();
  }

  function restore(entry) {
    el.value.value = entry.raw;
    if (entry.category === state.category) {
      el.from.value = entry.from;
      el.to.value = entry.to;
      scheduleConvert(0);
    } else {
      selectCategory(entry.category, entry);
    }
  }

  // ---------- Wiring ----------

  function wireEvents() {
    el.form.addEventListener("submit", (event) => {
      event.preventDefault();
      scheduleConvert(0);
    });
    el.value.addEventListener("input", () => scheduleConvert());
    el.from.addEventListener("change", () => scheduleConvert(0));
    el.to.addEventListener("change", () => scheduleConvert(0));
    el.decimals.addEventListener("change", () => scheduleConvert(0));

    el.swap.addEventListener("click", () => {
      [el.from.value, el.to.value] = [el.to.value, el.from.value];
      scheduleConvert(0);
    });

    el.copy.addEventListener("click", async () => {
      if (!state.lastResult) return;
      try {
        await navigator.clipboard.writeText(String(state.lastResult.result));
        el.copy.textContent = "Copied";
        setTimeout(() => (el.copy.textContent = "Copy"), 1200);
      } catch {
        el.copy.textContent = "Copy failed";
      }
    });

    el.clearHistory.addEventListener("click", () => {
      storage.set(HISTORY_KEY, []);
      renderHistory();
    });
  }

  async function init() {
    const saved = storage.get(SELECTION_KEY, {});

    el.decimals.replaceChildren(...DECIMAL_CHOICES.map((d) => new Option(String(d), String(d))));
    el.decimals.value = DECIMAL_CHOICES.includes(Number(saved.decimals)) ? saved.decimals : "4";
    if (typeof saved.value === "string" && NUMBER_PATTERN.test(saved.value)) {
      el.value.value = saved.value;
    }

    wireEvents();
    renderHistory();

    try {
      const categories = await api("categories");
      renderTabs(categories);
      const keys = categories.map((c) => c.key);
      const start = keys.includes(saved.category) ? saved.category : keys.includes("length") ? "length" : keys[0];
      await selectCategory(start, saved.category === start ? saved : undefined);
    } catch (error) {
      el.tabs.replaceChildren();
      showError(`Could not reach the API: ${error.message}`);
    }
  }

  init();
})();
