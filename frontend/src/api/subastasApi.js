export const API_BASE_URL = "https://localhost:7094/api";
export const HUB_URL = API_BASE_URL.replace(/\/api$/, "") + "/hubs/subastas";

function obtenerToken() {
    try {
        const sesion = JSON.parse(localStorage.getItem("sesion"));
        return sesion?.token ?? null;
    } catch {
        return null;
    }
}

function headersConToken(extra = {}) {
    const token = obtenerToken();
    return token ? { ...extra, Authorization: `Bearer ${token}` } : extra;
}

export async function login(email, password) {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo iniciar sesión";
        throw new Error(mensaje);
    }

    return data;
}

export async function obtenerSubastas(filtros = {}, pagina = 1, tamanoPagina = 10) {
    const params = new URLSearchParams({ pagina, tamanoPagina });
    if (filtros.estado) params.set("estado", filtros.estado);
    if (filtros.categoriaId) params.set("categoriaId", filtros.categoriaId);
    if (filtros.precioMin) params.set("precioMin", filtros.precioMin);
    if (filtros.precioMax) params.set("precioMax", filtros.precioMax);
    if (filtros.busqueda) params.set("busqueda", filtros.busqueda);
    if (filtros.ordenarPor) params.set("ordenarPor", filtros.ordenarPor);

    const response = await fetch(`${API_BASE_URL}/subastas?${params.toString()}`);

    if (!response.ok) {
        throw new Error("No se pudieron obtener las subastas");
    }

    return response.json();
}

export async function obtenerSubastaPorId(id) {
    const response = await fetch(`${API_BASE_URL}/subastas/${id}`);

    if (response.status === 404) {
        return null;
    }

    if (!response.ok) {
        throw new Error("No se pudo obtener la subasta");
    }

    return response.json();
}
export async function obtenerPujas(subastaId) {
    const response = await fetch(`${API_BASE_URL}/subastas/${subastaId}/pujas`);

    if (!response.ok) {
        throw new Error("No se pudo obtener el historial de ofertas");
    }

    return response.json();
}
export async function obtenerMovimientos() {
    const response = await fetch(`${API_BASE_URL}/wallet/movimientos`, {
        headers: headersConToken(),
    });

    if (!response.ok) {
        throw new Error("No se pudo obtener el historial de movimientos");
    }

    return response.json();
}
export async function obtenerMisPublicaciones() {
    const response = await fetch(`${API_BASE_URL}/subastas/mis-publicaciones`, {
        headers: headersConToken(),
    });

    if (!response.ok) {
        throw new Error("No se pudieron obtener tus publicaciones");
    }

    return response.json();
}

export async function obtenerMisPujas() {
    const response = await fetch(`${API_BASE_URL}/subastas/mis-pujas`, {
        headers: headersConToken(),
    });

    if (!response.ok) {
        throw new Error("No se pudieron obtener tus pujas");
    }

    return response.json();
}
export async function ofertar(subastaId, monto) {
    const response = await fetch(`${API_BASE_URL}/subastas/${subastaId}/pujas`, {
        method: "POST",
        headers: headersConToken({ "Content-Type": "application/json" }),
        body: JSON.stringify({ monto }),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo registrar la oferta";
        throw new Error(mensaje);
    }

    return data;
}

export async function obtenerCategorias() {
    const response = await fetch(`${API_BASE_URL}/categorias`);

    if (!response.ok) {
        throw new Error("No se pudieron obtener las categorías");
    }

    return response.json();
}

export async function crearSubasta(dto) {
    const response = await fetch(`${API_BASE_URL}/subastas`, {
        method: "POST",
        headers: headersConToken({ "Content-Type": "application/json" }),
        body: JSON.stringify(dto),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo crear la subasta";
        throw new Error(mensaje);
    }

    return data;
}

export async function modificarSubasta(id, dto) {
    const response = await fetch(`${API_BASE_URL}/subastas/${id}`, {
        method: "PUT",
        headers: headersConToken({ "Content-Type": "application/json" }),
        body: JSON.stringify(dto),
    });

    if (!response.ok) {
        const data = await response.json().catch(() => null);
        const mensaje = data?.error || "No se pudo modificar la subasta";
        throw new Error(mensaje);
    }
}

export async function eliminarSubasta(id) {
    const response = await fetch(`${API_BASE_URL}/subastas/${id}`, {
        method: "DELETE",
        headers: headersConToken(),
    });

    if (!response.ok) {
        const data = await response.json().catch(() => null);
        const mensaje = data?.error || "No se pudo eliminar la subasta";
        throw new Error(mensaje);
    }
}

export async function obtenerBalance() {
    const response = await fetch(`${API_BASE_URL}/wallet/balance`, {
        headers: headersConToken(),
    });

    if (!response.ok) {
        throw new Error("No se pudo obtener el balance");
    }

    return response.json();
}

export async function depositar(monto) {
    const response = await fetch(`${API_BASE_URL}/wallet/deposit`, {
        method: "POST",
        headers: headersConToken({ "Content-Type": "application/json" }),
        body: JSON.stringify({ monto }),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo depositar";
        throw new Error(mensaje);
    }

    return data;
}