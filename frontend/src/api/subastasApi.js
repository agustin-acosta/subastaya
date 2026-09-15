const API_BASE_URL = "https://localhost:7094/api";

export async function obtenerSubastas(pagina = 1, tamanoPagina = 10) {
    const response = await fetch(
        `${API_BASE_URL}/subastas?pagina=${pagina}&tamanoPagina=${tamanoPagina}`
    );

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

export async function ofertar(subastaId, compradorId, monto) {
    const response = await fetch(`${API_BASE_URL}/subastas/${subastaId}/pujas`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ compradorId, monto }),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo registrar la oferta";
        throw new Error(mensaje);
    }

    return data;
}

export async function obtenerUsuarios() {
    const response = await fetch(`${API_BASE_URL}/usuarios`);

    if (!response.ok) {
        throw new Error("No se pudieron obtener los usuarios");
    }

    return response.json();
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
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo crear la subasta";
        throw new Error(mensaje);
    }

    return data;
}

export async function obtenerBalance(usuarioId) {
    const response = await fetch(`${API_BASE_URL}/wallet/balance?usuarioId=${usuarioId}`);

    if (!response.ok) {
        throw new Error("No se pudo obtener el balance");
    }

    return response.json();
}

export async function depositar(usuarioId, monto) {
    const response = await fetch(`${API_BASE_URL}/wallet/deposit`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ usuarioId, monto }),
    });

    const data = await response.json().catch(() => null);

    if (!response.ok) {
        const mensaje = data?.error || "No se pudo depositar";
        throw new Error(mensaje);
    }

    return data;
}
