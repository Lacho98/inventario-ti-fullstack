// src/api.js
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5276";

async function apiGet(path) {
    const resp = await fetch(`${API_BASE_URL}${path}`);
    if (!resp.ok) throw new Error(`Error ${resp.status}`);
    return resp.json();
}

async function apiPost(path, body) {
    const resp = await fetch(`${API_BASE_URL}${path}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });
    if (!resp.ok) {
        const text = await resp.text();
        throw new Error(text || `Error ${resp.status}`);
    }
    return resp.json();
}

async function apiPut(path, body) {
    const resp = await fetch(`${API_BASE_URL}${path}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
    });
    if (!resp.ok) {
        const text = await resp.text();
        throw new Error(text || `Error ${resp.status}`);
    }
    return resp.status === 204 ? null : resp.json();
}

export async function getTiposEquipos() {
    return apiGet("/api/equipos/tipos");
}

export function getEquipos() {
    return apiGet("/api/equipos");
}

export function crearEquipo(payload) {
    return apiPost("/api/equipos", payload);
}

export function getSolicitudes() {
    return apiGet("/api/solicitudes");
}

export function crearSolicitud(payload) {
    return apiPost("/api/solicitudes", payload);
}

export function getPropuestaOptima(solicitudId) {
    return apiGet(`/api/solicitudes/${solicitudId}/propuesta-optima`);
}

export function getSolicitudDetalle(solicitudId) {
    return apiGet(`/api/solicitudes/${solicitudId}`);
}

export function getRoles() {
    return apiGet("/api/roles");
}

export function actualizarEstadoSolicitud(id, estado) {
    return apiPut(`/api/solicitudes/${id}/estado`, { estado });
}


