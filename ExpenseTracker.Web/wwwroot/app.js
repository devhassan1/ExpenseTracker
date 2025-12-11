
//// Simple frontend for your minimal API (same origin)

//// Store & get JWT
//const tokenKey = "expense.jwt";
//const getToken = () => localStorage.getItem(tokenKey);
//const setToken = (t) => localStorage.setItem(tokenKey, t || "");
//const clearToken = () => localStorage.removeItem(tokenKey);

//// Helpers
//const $ = (sel) => document.querySelector(sel);
//const out = (id, content) => { $(id).textContent = typeof content === "string" ? content : JSON.stringify(content, null, 2); };
//const log = (...args) => {
//    const msg = args.map(a => (typeof a === "string" ? a : JSON.stringify(a))).join(" ");
//    const prev = $("#console-out").textContent;
//    $("#console-out").textContent = prev + (prev ? "\n" : "") + msg;
//    console.log(...args);
//};

//// Tabs
//document.querySelectorAll("nav button").forEach(btn => {
//    btn.addEventListener("click", () => {
//        const tab = btn.dataset.tab;
//        document.querySelectorAll(".tab").forEach(t => t.classList.remove("active"));
//        $(`#tab-${tab}`).classList.add("active");
//    });
//});

//// Decode JWT
//function decodeJwt(token) {
//    try {
//        const [, payload] = token.split(".");
//        const json = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
//        return JSON.parse(json);
//    } catch { return null; }
//}

//// Auth header
//const authHeader = () => {
//    const t = getToken();
//    return t ? { "Authorization": `Bearer ${t}` } : {};
//};

//// Default date ranges
//function setDefaultDates() {
//    const today = new Date();
//    const past = new Date(Date.now() - 30 * 24 * 3600 * 1000);

//    const fmt = (d) => d.toISOString().substring(0, 10);

//    // Add
//    $("#add-expense-form input[name='date']").value = fmt(today);

//    // List
//    $("#list-expenses-form input[name='from']").value = fmt(past);
//    $("#list-expenses-form input[name='to']").value = fmt(today);

//    // Export
//    $("#export-form input[name='from']").value = fmt(past);
//    $("#export-form input[name='to']").value = fmt(today);
//}

//// RAW JSON template for AddExpenseRequest (adjust to your DTO)
//const defaultAddJson = {
//    // ⚠️ Replace keys to match your real AddExpenseRequest
//    date: new Date().toISOString(),
//    amount: 500,
//    currency: "PKR",
//    categoryId: 1,
//    tags: ["food", "lunch"],
//    note: "Team lunch",
//    forUserId: null
//};
//$("#raw-add-json").value = JSON.stringify(defaultAddJson, null, 2);

//// LOGIN
//$("#login-form").addEventListener("submit", async (e) => {
//    e.preventDefault();
//    const fd = new FormData(e.currentTarget);
//    const body = {
//        username: fd.get("username"),
//        password: fd.get("password")
//    };
//    try {
//        const res = await fetch("/api/auth/login", {
//            method: "POST",
//            headers: { "Content-Type": "application/json" },
//            body: JSON.stringify(body)
//        });
//        if (!res.ok) {
//            out("#token-preview", `(login failed) HTTP ${res.status}`);
//            out("#claims-preview", "");
//            log("Login failed:", res.status, await res.text());
//            return;
//        }
//        const data = await res.json();
//        setToken(data.token);
//        out("#token-preview", data.token);
//        const claims = decodeJwt(data.token);
//        out("#claims-preview", claims || "(unable to decode)");
//        log("Logged in successfully");
//        // Switch to list tab
//        document.querySelector("nav button[data-tab='list']").click();
//    } catch (err) {
//        out("#token-preview", String(err));
//        log("Login error:", err);
//    }
//});

//$("#logout-btn").addEventListener("click", () => {
//    clearToken();
//    out("#token-preview", "(not logged in)");
//    out("#claims-preview", "(no claims)");
//    log("Logged out");
//});

//// ADD EXPENSE — guided form
//$("#add-expense-form").addEventListener("submit", async (e) => {
//    e.preventDefault();
//    const fd = new FormData(e.currentTarget);
//    const payload = {
//        // ⚠️ Map to your DTO keys if different:
//        date: fd.get("date"),
//        amount: fd.get("amount") ? Number(fd.get("amount")) : null,
//        currency: fd.get("currency") || null,
//        categoryId: fd.get("categoryId") ? Number(fd.get("categoryId")) : null,
//        tags: (fd.get("tagsCsv") || "").split(",").map(s => s.trim()).filter(Boolean),
//        note: fd.get("note") || null,
//        forUserId: fd.get("forUserId") ? Number(fd.get("forUserId")) : null
//    };
//    try {
//        const res = await fetch("/api/expenses", {
//            method: "POST",
//            headers: { "Content-Type": "application/json", ...authHeader() },
//            body: JSON.stringify(payload)
//        });
//        const text = await res.text();
//        out("#add-response", res.ok ? text : `HTTP ${res.status}\n${text}`);
//        log("Add expense response:", res.status, text);
//    } catch (err) {
//        out("#add-response", String(err));
//        log("Add expense error:", err);
//    }
//});

//// ADD EXPENSE — raw JSON
//$("#send-raw-add").addEventListener("click", async () => {
//    let payload;
//    try { payload = JSON.parse($("#raw-add-json").value); }
//    catch (err) { alert("Invalid JSON: " + err); return; }

//    try {
//        const res = await fetch("/api/expenses", {
//            method: "POST",
//            headers: { "Content-Type": "application/json", ...authHeader() },
//            body: JSON.stringify(payload)
//        });
//        const text = await res.text();
//        out("#add-response", res.ok ? text : `HTTP ${res.status}\n${text}`);
//        log("Add (raw) response:", res.status, text);
//    } catch (err) {
//        out("#add-response", String(err));
//        log("Add (raw) error:", err);
//    }
//});

//// LIST EXPENSES
//$("#list-expenses-form").addEventListener("submit", async (e) => {
//    e.preventDefault();
//    const fd = new FormData(e.currentTarget);
//    const params = new URLSearchParams({
//        from: fd.get("from"),
//        to: fd.get("to")
//    });
//    const forUserId = fd.get("forUserId");
//    if (forUserId) params.set("forUserId", String(forUserId));

//    try {
//        const res = await fetch(`/api/expenses?${params.toString()}`, {
//            headers: { ...authHeader() }
//        });
//        const text = await res.text();
//        if (!res.ok) {
//            out("#list-json", `HTTP ${res.status}\n${text}`);
//            log("List failed:", res.status, text);
//            return;
//        }
//        const data = JSON.parse(text);
//        out("#list-json", data);
//        const tbody = $("#list-table tbody");
//        tbody.innerHTML = "";
//        (Array.isArray(data) ? data : []).forEach(item => {
//            const tr = document.createElement("tr");
//            const td = (v) => {
//                const el = document.createElement("td");
//                el.textContent = v ?? "";
//                return el;
//            };
//            tr.appendChild(td(item.date ?? item.occurredOn ?? "")); // guess common keys
//            tr.appendChild(td(item.amount));
//            tr.appendChild(td(item.categoryName ?? item.categoryId));
//            tr.appendChild(td(Array.isArray(item.tags) ? item.tags.join(", ") : ""));
//            tr.appendChild(td(item.note ?? ""));
//            tbody.appendChild(tr);
//        });
//        log("Listed expenses:", data?.length ?? 0);
//    } catch (err) {
//        out("#list-json", String(err));
//        log("List error:", err);
//    }
//});

//// EXPORT
//$("#export-form").addEventListener("submit", async (e) => {
//    e.preventDefault();
//    const fd = new FormData(e.currentTarget);
//    const params = new URLSearchParams({
//        from: fd.get("from"),
//        to: fd.get("to"),
//        format: fd.get("format")
//    });
//    const forUserId = fd.get("forUserId");
//    if (forUserId) params.set("forUserId", String(forUserId));

//    try {
//        const res = await fetch(`/api/reports/export?${params.toString()}`, {
//            headers: { ...authHeader() }
//        });
//        if (!res.ok) {
//            const text = await res.text();
//            out("#export-status", `HTTP ${res.status}\n${text}`);
//            log("Export failed:", res.status, text);
//            return;
//        }
//        const blob = await res.blob();
//        const url = URL.createObjectURL(blob);

//        // Try to infer filename
//        const cd = res.headers.get("Content-Disposition");
//        let filename = "export.dat";
//        const m = cd && /filename="?([^"]+)"?/.exec(cd);
//        if (m && m[1]) filename = m[1];

//        const a = document.createElement("a");
//        a.href = url;
//        a.download = filename;
//        a.style.display = "none";
//        document.body.appendChild(a);
//        a.click();
//        URL.revokeObjectURL(url);
//        document.body.removeChild(a);

//        out("#export-status", `Downloaded: ${filename}`);
//        log("Export success:", filename);
//    } catch (err) {
//        out("#export-status", String(err));
//        log("Export error:", err);
//    }
//});

//// Initialize
//setDefaultDates();
//const t = getToken();
//if (t) {
//    out("#token-preview", t);
//    out("#claims-preview", decodeJwt(t) || "(unable to decode)");
//}
    //const apiBaseUrl = '/api';
    //const currentUserId = null; // set after loading users for a logged-in session, or hardcode for testing

    //    // Utility
    //    const byId = id => document.getElementById(id);
    //    const setMsg = (id, text, err = false) => {
    //        const el = byId(id);
    //el.textContent = text;
    //el.className = err ? 'error' : 'muted';
    //    };

    //async function getJSON(url) {
    //        const r = await fetch(url);
    //if (!r.ok) throw new Error(await r.text());
    //return r.json();
    //    }
    //async function postJSON(url, body) {
    //        const r = await fetch(url, {
    //    method: 'POST',
    //headers: {'Content-Type': 'application/json' },
    //body: JSON.stringify(body)
    //        });
    //if (!r.ok) throw new Error(await r.text());
    //return r.json();
    //    }

    //// Populate dropdowns
    //async function loadOrganizations(selectIds) {
    //        const orgs = await getJSON(`${apiBaseUrl}/organizations`);
    //for (const id of selectIds) {
    //            const sel = byId(id);
    //sel.innerHTML = '';
    //for (const org of orgs) {
    //                const opt = document.createElement('option');
    //opt.value = org.id;
    //opt.textContent = org.name;
    //sel.appendChild(opt);
    //            }
    //        }
    //// Also fill parent selector with (none) retained
    //const parentSel = byId('org-parent');
    //if (parentSel) {
    //            for (const org of orgs) {
    //                const opt = document.createElement('option');
    //opt.value = org.id;
    //opt.textContent = org.name;
    //parentSel.appendChild(opt);
    //            }
    //        }
    //return orgs;
    //    }

    //async function loadRoles() {
    //        const roles = await getJSON(`${apiBaseUrl}/roles`);
    //const sel = byId('user-role');
    //sel.innerHTML = '';
    //for (const role of roles) {
    //            const opt = document.createElement('option');
    //opt.value = role.id;
    //opt.textContent = role.name;
    //sel.appendChild(opt);
    //        }
    //    }

    //async function loadUsers(orgId, selectId) {
    //        const users = await getJSON(`${apiBaseUrl}/users?orgId=${orgId}`);
    //const sel = byId(selectId);
    //sel.innerHTML = '';
    //for (const u of users) {
    //            const opt = document.createElement('option');
    //opt.value = u.id;
    //opt.textContent = `${u.name} (${u.email || 'no email'})`;
    //sel.appendChild(opt);
    //        }
    //return users;
    //    }

    //async function loadCategories(orgId, selectId) {
    //        const cats = await getJSON(`${apiBaseUrl}/categories?orgId=${orgId}&includeGlobal=true`);
    //const sel = byId(selectId);
    //sel.innerHTML = '<option value="">(None)</option>';
    //for (const c of cats) {
    //            const opt = document.createElement('option');
    //opt.value = c.id;
    //opt.textContent = `${c.label}${c.organizationId ? '' : ' (Global)'}`;
    //sel.appendChild(opt);
    //        }
    //return cats;
    //    }

    //async function loadTags(orgId, selectId) {
    //        const tags = await getJSON(`${apiBaseUrl}/tags?orgId=${orgId}&includeGlobal=true`);
    //const sel = byId(selectId);
    //sel.innerHTML = '';
    //for (const t of tags) {
    //            const opt = document.createElement('option');
    //opt.value = t.id;
    //opt.textContent = `${t.label}${t.organizationId ? '' : ' (Global)'}`;
    //sel.appendChild(opt);
    //        }
    //return tags;
    //    }

    //// Auto-tags preview: show CATEGORY_TAG mappings for selected category
    //async function loadAutoTagsPreview(categoryId, orgId) {
    //        const container = byId('exp-auto-tags');
    //container.innerHTML = '';
    //if (!categoryId) return;

    //// naive call: backend could support /category-tags?categoryId=...
    //const mappings = await getJSON(`${apiBaseUrl}/category-tags?categoryId=${categoryId}`);
    //for (const m of mappings) {
    //            const chip = document.createElement('span');
    //chip.className = 'chip';
    //chip.textContent = `Auto-tag: ${m.tagLabel}`;
    //container.appendChild(chip);
    //        }
    //    }

    //    // ======= Form handlers =======
    //    // Organizations
    //    byId('form-org').addEventListener('submit', async (e) => {
    //    e.preventDefault();
    //try {
    //            const name = byId('org-name').value.trim();
    //const parentId = byId('org-parent').value || null;
    //await postJSON(`${apiBaseUrl}/organizations`, {name, parentId});
    //setMsg('org-msg', 'Organization created.');
    //await loadOrganizations(['user-org', 'cat-org', 'tag-org', 'exp-org']);
    //        } catch (err) {
    //    setMsg('org-msg', err.message, true);
    //        }
    //    });

    //    // Users
    //    byId('form-user').addEventListener('submit', async (e) => {
    //    e.preventDefault();
    //try {
    //            const body = {
    //    organizationId: Number(byId('user-org').value),
    //roleId: Number(byId('user-role').value),
    //name: byId('user-name').value.trim(),
    //email: byId('user-email').value.trim() || null,
    //passwordHash: byId('user-passhash').value.trim(),
    //isActive: Number(byId('user-active').value)
    //            };
    //await postJSON(`${apiBaseUrl}/users`, body);
    //setMsg('user-msg', 'User created.');
    //// Refresh users for selected org in other forms
    //const orgId = byId('user-org').value;
    //await loadUsers(orgId, 'cat-owner');
    //await loadUsers(orgId, 'tag-owner');
    //await loadUsers(orgId, 'exp-user');
    //await loadUsers(orgId, 'exp-created-by');
    //        } catch (err) {
    //    setMsg('user-msg', err.message, true);
    //        }
    //    });

    //    // Categories
    //    byId('form-category').addEventListener('submit', async (e) => {
    //    e.preventDefault();
    //try {
    //            const orgId = byId('cat-org').value || null;
    //const body = {
    //    organizationId: orgId ? Number(orgId) : null,
    //ownerUserId: byId('cat-owner').value ? Number(byId('cat-owner').value) : null,
    //label: byId('cat-label').value.trim(),
    //isPredefined: Number(byId('cat-predef').value),
    //isActive: Number(byId('cat-active').value)
    //            };
    //await postJSON(`${apiBaseUrl}/categories`, body);
    //setMsg('cat-msg', 'Category created.');
    //await loadCategories(byId('exp-org').value, 'exp-category');
    //        } catch (err) {
    //    setMsg('cat-msg', err.message, true);
    //        }
    //    });

    //    // Tags
    //    byId('form-tag').addEventListener('submit', async (e) => {
    //    e.preventDefault();
    //try {
    //            const orgId = byId('tag-org').value || null;
    //const body = {
    //    organizationId: orgId ? Number(orgId) : null,
    //ownerUserId: byId('tag-owner').value ? Number(byId('tag-owner').value) : null,
    //label: byId('tag-label').value.trim(),
    //isPredefined: Number(byId('tag-predef').value),
    //isActive: Number(byId('tag-active').value)
    //            };
    //await postJSON(`${apiBaseUrl}/tags`, body);
    //setMsg('tag-msg', 'Tag created.');
    //await loadTags(byId('exp-org').value, 'exp-tags');
    //        } catch (err) {
    //    setMsg('tag-msg', err.message, true);
    //        }
    //    });

    //    // Expenses: cascading selects and submit
    //    byId('exp-org').addEventListener('change', async () => {
    //        const orgId = byId('exp-org').value;
    //await loadUsers(orgId, 'exp-user');
    //await loadUsers(orgId, 'exp-created-by');
    //await loadCategories(orgId, 'exp-category');
    //await loadTags(orgId, 'exp-tags');
    //byId('exp-auto-tags').innerHTML = '';
    //    });

    //    byId('exp-category').addEventListener('change', async () => {
    //        const categoryId = byId('exp-category').value || null;
    //const orgId = byId('exp-org').value || null;
    //await loadAutoTagsPreview(categoryId, orgId);
    //    });

    //    byId('form-expense').addEventListener('submit', async (e) => {
    //    e.preventDefault();
    //try {
    //            const orgId = Number(byId('exp-org').value);
    //const userId = Number(byId('exp-user').value);
    //const createdBy = Number(byId('exp-created-by').value);
    //const categoryId = byId('exp-category').value ? Number(byId('exp-category').value) : null;
    //const amount = Number(byId('exp-amount').value);
    //const currency = byId('exp-currency').value.trim().toUpperCase();
    //const description = byId('exp-desc').value.trim() || null;

    //// Convert datetime-local to ISO
    //const txnLocal = byId('exp-txn-date').value;
    //if (!txnLocal) throw new Error('Transaction date is required');
    //const txnDate = new Date(txnLocal);
    //if (isNaN(txnDate)) throw new Error('Invalid transaction date');

    //// Multi-select tags
    //const tagSel = byId('exp-tags');
    //            const tagIds = Array.from(tagSel.selectedOptions).map(o => Number(o.value));

    //// Basic client validations aligned with your checks
    //if (currency.length !== 3) throw new Error('Currency must be 3 characters');

    //const payload = {
    //    organizationId: orgId,
    //userId,
    //createdByUserId: createdBy,
    //categoryId,
    //amount,
    //currency,
    //description,
    //txnDate: txnDate.toISOString(),
    //tagIds
    //            };
    //await postJSON(`${apiBaseUrl}/expenses`, payload);
    //setMsg('exp-msg', 'Expense created.');
    //byId('form-expense').reset();
    //byId('exp-auto-tags').innerHTML = '';
    //        } catch (err) {
    //    setMsg('exp-msg', err.message, true);
    //        }
    //    });

    //// Initial boot
    //(async function init() {
    //        try {
    //    await loadOrganizations(['user-org', 'cat-org', 'tag-org', 'exp-org']);
    //await loadRoles();
    //// Default select first org to drive cascading loads
    //const org1 = byId('user-org').value;
    //await loadUsers(org1, 'cat-owner');
    //await loadUsers(org1, 'tag-owner');
    //await loadUsers(byId('exp-org').value, 'exp-user');
    //await loadUsers(byId('exp-org').value, 'exp-created-by');
    //await loadCategories(byId('exp-org').value, 'exp-category');
    //await loadTags(byId('exp-org').value, 'exp-tags');
    //// Pre-fill txn date to now (local)
    //const now = new Date();
    //            const pad = n => String(n).padStart(2, '0');
    //const local = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`;
    //            byId('exp-txn-date').value = local;
    //        } catch (err) {
    //            console.error(err);
    //        }
//})();

// app.js - shared utilities & API wiring

// wwwroot/js/app.js

// app.js

const apiBaseUrl = '/api';

const byId = id => document.getElementById(id);
const setMsg = (id, text, err = false) => {
    const el = byId(id);
    if (!el) return;
    el.textContent = text;
    el.className = err ? 'error' : 'muted';
};

// read JWT and attach to fetch
function getAuthHeader() {
    const token = localStorage.getItem('jwt');
    return token ? { 'Authorization': 'Bearer ' + token } : {};
}

async function getJSON(url) {
    const r = await fetch(url, { headers: { ...getAuthHeader() } });
    if (!r.ok) {
        const txt = await r.text();
        // Optional UX: guide on 401
        if (r.status === 401) setMsg('auth-msg', 'Please login to continue', true);
        throw new Error(txt || r.statusText);
    }
    const ct = r.headers.get('content-type') || '';
    if (!ct.includes('application/json')) {
        throw new Error(`Expected JSON, got ${ct}`);
    }
    return r.json();
}

async function postJSON(url, body) {
    const r = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...getAuthHeader() },
        body: JSON.stringify(body)
    });
    if (!r.ok) {
        const txt = await r.text();
        throw new Error(txt || r.statusText);
    }
    try {
        return await r.json();
    } catch {
        return null; // some endpoints return 201 with no body
    }
}

// authentication helper
async function authLogin({ username, password }) {
    // backend expects LoginRequest { Username, Password }
    const resp = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: username, Password: password })
    });
    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }
    return resp.json(); // { token, token_type, expires_in }
}

// decode JWT to simple object (no validation, only payload)
function decodeJwt(token) {
    try {
        const payload = token.split('.')[1];
        const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
        // escape is deprecated; using decodeURIComponent(escape(json)) for legacy safety
        const obj = JSON.parse(decodeURIComponent(escape(json)));
        return {
            userId:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
                obj['sub'] ||
                obj['nameid'] ||
                obj['nameIdentifier'] ||
                Number(obj['sub']) || obj['name'],
            username:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                obj['unique_name'] ||
                obj['name'],
            roles: (() => {
                const r =
                    obj['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                    obj['roles'] ||
                    obj['role'];
                if (!r) return [];
                return Array.isArray(r) ? r : [r];
            })(),
            raw: obj
        };
    } catch {
        return {};
    }
}

// utility to export a report (binary response)
async function exportReport({ from, to, forUserId = null, format = 'csv' }) {
    const q = new URLSearchParams();
    if (from) q.append('from', from);
    if (to) q.append('to', to);
    if (forUserId) q.append('forUserId', forUserId);
    q.append('format', format);

    const url = `/api/reports/export?${q.toString()}`;
    const resp = await fetch(url, { headers: { ...getAuthHeader() } });
    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }
    const blob = await resp.blob();
    const ext = format === 'excel' ? 'xlsx' : (format === 'pdf' ? 'pdf' : 'csv');
    const filename = `expenses_${from || 'all'}_${to || 'all'}.${ext}`;
    const a = document.createElement('a');
    const urlObj = URL.createObjectURL(blob);
    a.href = urlObj;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(urlObj);
}

/**
 * Populate a <select> with tags (multi-select friendly).
 * NOTE: Do NOT attach DOMContentLoaded here; call this from your page when ready.
 *
 * @param {string} selectId - element id of the select, e.g. 'exp-tags'
 * @param {object} opts - optional filters { label?: string, includeGlobal?: boolean }
 */
async function loadTags(selectId = 'exp-tags', opts = { includeGlobal: true }) {
    const select = document.getElementById(selectId);
    if (!select) {
        console.warn(`loadTags: element #${selectId} not found; skipping render.`);
        return [];
    }

    const q = new URLSearchParams();
    if (opts?.label) q.append('label', opts.label);
    if (opts?.includeGlobal !== undefined) q.append('includeGlobal', String(opts.includeGlobal));

    let tags = [];
    try {
        tags = await getJSON(`/api/tags?${q.toString()}`);
    } catch (err) {
        console.error('loadTags failed:', err);
        setMsg('tags-msg', 'Could not load tags', true);
    }

    // Render (multi-select; no placeholder needed)
    select.innerHTML = '';
    (Array.isArray(tags) ? tags : []).forEach(t => {
        const opt = document.createElement('option');
        opt.value = t.id;
        opt.textContent = t.label;
        select.appendChild(opt);
    });

    return tags;
}

/**
 * Populate a <select> with users; returns the array of users.
 * @param {number|null} orgId - optional organization id
 * @param {string} selectId - element id of select
 */
async function loadUsers(orgId, selectId) {
    const params = new URLSearchParams();
    if (orgId) params.append('orgId', orgId);
    const url = `${apiBaseUrl}/users${params.toString() ? '?' + params.toString() : ''}`;

    let users = [];
    try {
        users = await getJSON(url);
    } catch (err) {
        console.error('loadUsers failed:', err);
        setMsg('users-msg', 'Could not load users', true);
    }

    const sel = document.getElementById(selectId);
    if (!sel) return users;

    sel.innerHTML = '';
    const placeholder = document.createElement('option');
    placeholder.value = '';
    placeholder.textContent = '-- Select user --';
    sel.appendChild(placeholder);

    for (const u of Array.isArray(users) ? users : []) {
        const opt = document.createElement('option');
        opt.value = u.id;
        opt.textContent = `${u.name ?? u.username ?? ('User ' + u.id)}${u.email ? ` (${u.email})` : ''}`;
        sel.appendChild(opt);
    }
    return users;
}

