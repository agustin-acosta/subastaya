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