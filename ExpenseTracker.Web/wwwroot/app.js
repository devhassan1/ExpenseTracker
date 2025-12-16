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
    const resp = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ Username: username, Password: password })
    });

    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }

    const data = await resp.json();
    localStorage.setItem('jwt', data.token);
    return data;
}

async function authRegister({ name, email, password, roleId, parentUserId }) {
    const resp = await fetch('/api/auth/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Name: name,
            Email: email,
            Password: password,
            RoleId: roleId,
            ParentUserId: parentUserId
        })
    });

    if (!resp.ok) {
        const txt = await resp.text();
        throw new Error(txt || resp.statusText);
    }

    return await resp.json();
}


// --- DECODE JWT ------------------------------------------------------

function decodeJwt(token) {
    try {
        const payload = token.split('.')[1];
        const json = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
        const obj = JSON.parse(decodeURIComponent(escape(json)));

        return {
            userId:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ||
                obj.sub,
            username:
                obj['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
                obj.name,
            roles: (() => {
                const r =
                    obj['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
                    obj.roles;
                return Array.isArray(r) ? r : r ? [r] : [];
            })(),
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

    const url = `${apiBaseUrl}/users${params.toString() ? '?' + params : ''}`;

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
        opt.value = u.id;
        opt.textContent = u.name ?? u.username ?? `User ${u.id}`;
        sel.appendChild(opt);
    }

    return users;
}
async function loadTags(selectId = 'exp-tags', opts = { includeGlobal: true }) {
    const select = document.getElementById(selectId);
    if (!select) {
        console.warn(`loadTags: element #${selectId} not found;
            skipping render.`);
        return [];
    }
    const q = new URLSearchParams();
    if (opts?.label)
        q.append('label', opts.label);
    if (opts?.includeGlobal !== undefined)
        q.append('includeGlobal', String(opts.includeGlobal));
    let tags = [];
    try {
        tags = await getJSON(`/api/tags ? ${q.toString()}`);
    } catch (err) {
        console.error('loadTags failed:', err);
        setMsg('tags-msg', 'Could not load tags', true);
    }
    select.innerHTML = '';
    (Array.isArray(tags) ? tags : []).forEach(
        t => {
            const opt = document.createElement('option');
            opt.value = t.id;
            opt.textContent = t.label;
            select.appendChild(opt);
        });
    return tags;
}
