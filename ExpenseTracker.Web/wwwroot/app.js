
const apiBaseUrl = '/api';

const byId = id => document.getElementById(id);

const setMsg = (id, text, err = false) => {
    const el = byId(id);
    if (!el) return;
    el.textContent = text;
    el.className = err ? 'error' : 'muted';
};

// --- AUTH HEADER ----------------------------------------------------

function getAuthHeader() {
    const token = localStorage.getItem('jwt');
    if (!token || token.split('.').length !== 3) return {};
    return { 'Authorization': 'Bearer ' + token };
}

// --- GENERIC GET/POST JSON ------------------------------------------

async function getJSON(url) {
    const r = await fetch(url, { headers: { ...getAuthHeader() } });

    if (!r.ok) {
        if (r.status === 401) {
            setMsg('auth-msg', 'Please log in to continue', true);
        }
        const txt = await r.text();
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
        return null;
    }
}

// --- AUTH HELPERS ----------------------------------------------------

async function authLogin({ username, password }) {
    const resp = await fetch(`${apiBaseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        // Server expects PascalCase? If yes, keep as below:
        body: JSON.stringify({ Username: username, Password: password })
        // If your model binder prefers camelCase, use: { username, password }
    });

    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }

    const data = await resp.json();
    if (!data?.token || typeof data.token !== 'string') {
        throw new Error('Login did not return a token.');
    }
    localStorage.setItem('jwt', data.token);
    return data;
}

async function authRegister({ username, password }) {
    const resp = await fetch(`${apiBaseUrl}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: username, Password: password })
    });

    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }

    const data = await resp.json();
    if (data?.token) {
        localStorage.setItem('jwt', data.token);
    }
    return data;
}

// --- DECODE JWT ------------------------------------------------------

function decodeJwt(token) {
    try {
        const payload = token.split('.')[1];
        const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');

        // Decode Base64 with proper handling
        const json = atob(base64);
        // Some environments need UTF-8 handling; try both
        let obj;
        try {
            obj = JSON.parse(decodeURIComponent(escape(json)));
        } catch {
            obj = JSON.parse(json);
        }

        const roleClaim =
            obj['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
            obj['roles'] ?? obj['role'];

        return {
            userId:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
                obj.sub,
            username:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                obj.name,
            roles: Array.isArray(roleClaim) ? roleClaim : roleClaim ? [roleClaim] : [],
            raw: obj
        };
    } catch {
        return {};
    }
}

// --- LOAD USERS ------------------------------------------------------

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

    sel.innerHTML = '<option value="">-- Select user --</option>';

    for (const u of Array.isArray(users) ? users : []) {
        const opt = document.createElement('option');
        opt.value = String(u.id ?? '');
        opt.textContent = u.name ?? u.username ?? `User ${u.id ?? ''}`;
        sel.appendChild(opt);
    }

    return users;
}

// --- LOAD TAGS (fixed URL, safe rendering) ---------------------------

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
        // FIX: remove spaces and build proper query
        const url = `${apiBaseUrl}/tags${q.toString() ? '?' + q.toString() : ''}`;
        tags = await getJSON(url);
    } catch (err) {
        console.error('loadTags failed:', err);
        setMsg('tags-msg', 'Could not load tags', true);
    }

    select.innerHTML = '';
    (Array.isArray(tags) ? tags : []).forEach(t => {
        const opt = document.createElement('option');
        opt.value = String(t.id ?? '');
        opt.textContent = String(t.label ?? '');
        select.appendChild(opt);
    });

    return tags;
}

// --- OPTIONAL: expose to global if your page scripts call them -------

window.loadTags = loadTags;
window.loadUsers = loadUsers;
window.authLogin = authLogin;
window.authRegister = authRegister;
window.getJSON = getJSON;
window.postJSON = postJSON;
window.decodeJwt = decodeJwt;
window.setMsg
