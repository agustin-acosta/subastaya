import { useState, useEffect } from "react";
import { obtenerSubastas, obtenerCategorias } from "../api/subastasApi";
import SubastaCard from "../components/SubastaCard";

const ESTADOS = ["Programada", "Activa", "Finalizada", "Desierta"];
const FILTROS_VACIOS = { estado: "", categoriaId: "", precioMin: "", precioMax: "", ordenarPor: "" };

function Catalogo() {
    const [subastas, setSubastas] = useState([]);
    const [categorias, setCategorias] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);
    const [filtros, setFiltros] = useState(FILTROS_VACIOS);

    useEffect(() => {
        obtenerCategorias().then(setCategorias).catch(() => { });
    }, []);

    // Carga inicial: no llamamos setCargando(true) acá porque ya arranca en true.
    // Los setState solo ocurren dentro de then/catch/finally, es decir, como
    // reacción a que la promesa se resuelve, no de forma síncrona en el efecto.
    useEffect(() => {
        let cancelado = false;
        obtenerSubastas(FILTROS_VACIOS)
            .then((data) => {
                if (!cancelado) setSubastas(data.items);
            })
            .catch((err) => {
                if (!cancelado) setError(err.message);
            })
            .finally(() => {
                if (!cancelado) setCargando(false);
            });
        return () => { cancelado = true; };
    }, []);

    async function buscar(filtrosActuales) {
        setCargando(true);
        setError(null);
        try {
            const data = await obtenerSubastas(filtrosActuales);
            setSubastas(data.items);
        } catch (err) {
            setError(err.message);
        } finally {
            setCargando(false);
        }
    }

    function handleFiltroChange(campo, valor) {
        setFiltros((f) => ({ ...f, [campo]: valor }));
    }

    function handleSubmit(e) {
        e.preventDefault();
        buscar(filtros);
    }

    function handleLimpiar() {
        setFiltros(FILTROS_VACIOS);
        buscar(FILTROS_VACIOS);
    }

    return (
        <div className="container">
            <h1>Catálogo de Subastas</h1>

            <form onSubmit={handleSubmit} className="panel" style={{ marginBottom: 24 }}>
                <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(160px, 1fr))", gap: 12 }}>
                    <div className="form-group">
                        <label>Estado</label>
                        <select value={filtros.estado} onChange={(e) => handleFiltroChange("estado", e.target.value)}>
                            <option value="">Todos</option>
                            {ESTADOS.map((e) => <option key={e} value={e}>{e}</option>)}
                        </select>
                    </div>
                    <div className="form-group">
                        <label>Categoría</label>
                        <select value={filtros.categoriaId} onChange={(e) => handleFiltroChange("categoriaId", e.target.value)}>
                            <option value="">Todas</option>
                            {categorias.map((c) => <option key={c.id} value={c.id}>{c.nombre}</option>)}
                        </select>
                    </div>
                    <div className="form-group">
                        <label>Precio mínimo</label>
                        <input type="number" min="0" value={filtros.precioMin} onChange={(e) => handleFiltroChange("precioMin", e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label>Precio máximo</label>
                        <input type="number" min="0" value={filtros.precioMax} onChange={(e) => handleFiltroChange("precioMax", e.target.value)} />
                    </div>
                    <div className="form-group">
                        <label>Ordenar por</label>
                        <select value={filtros.ordenarPor} onChange={(e) => handleFiltroChange("ordenarPor", e.target.value)}>
                            <option value="">Más recientes</option>
                            <option value="tiempoRestante">Menor tiempo restante</option>
                            <option value="mayorPuja">Mayor puja actual</option>
                        </select>
                    </div>
                </div>
                <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                    <button type="submit" className="btn btn-primary">Aplicar filtros</button>
                    <button type="button" className="btn" onClick={handleLimpiar}>Limpiar</button>
                </div>
            </form>

            {cargando ? (
                <p className="card-muted">Cargando subastas...</p>
            ) : error ? (
                <p className="card-muted">Error: {error}</p>
            ) : subastas.length === 0 ? (
                <p className="card-muted">No hay subastas para mostrar.</p>
            ) : (
                <div
                    className="catalogo-grid"
                    style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))", gap: 16 }}
                >
                    {subastas.map((subasta) => (
                        <SubastaCard key={subasta.id} subasta={subasta} />
                    ))}
                </div>
            )}
        </div>
    );
}

export default Catalogo;