import { useState, useEffect } from "react";
import { useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { obtenerSubastas, obtenerCategorias } from "../api/subastasApi";
import SubastaCard from "../components/SubastaCard";

const ESTADOS = ["Programada", "Activa", "Finalizada", "Desierta"];
const FILTROS_VACIOS = { estado: "", categoriaId: "", precioMin: "", precioMax: "", busqueda: "", ordenarPor: "" };

function Catalogo() {
    const location = useLocation();
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    
    const busquedaUrl = searchParams.get("busqueda") || "";

    const filtrosDeBusquedaUrl = { ...FILTROS_VACIOS, busqueda: busquedaUrl };

    const [subastas, setSubastas] = useState([]);
    const [categorias, setCategorias] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);
    const [filtros, setFiltros] = useState(filtrosDeBusquedaUrl);
    const [mostrarExitoEliminacion, setMostrarExitoEliminacion] = useState(Boolean(location.state?.eliminada));

    const [busquedaYaAplicada, setBusquedaYaAplicada] = useState(busquedaUrl);
    if (busquedaUrl !== busquedaYaAplicada) {
        setBusquedaYaAplicada(busquedaUrl);
        setFiltros(filtrosDeBusquedaUrl);
    }

    useEffect(() => {
        if (location.state?.eliminada) {
            window.history.replaceState({}, "");
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        obtenerCategorias().then(setCategorias).catch(() => { });
    }, []);

    useEffect(() => {
        buscar(filtrosDeBusquedaUrl);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [busquedaUrl]);

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
        
        if (busquedaUrl) {
            navigate("/", { replace: true });
        }
    }

    return (
        <div className="container" id="catalogo">
            {mostrarExitoEliminacion && (
                <div className="alert alert-exito" style={{ marginBottom: 16 }}>
                    Subasta eliminada correctamente.
                    <button
                        type="button"
                        onClick={() => setMostrarExitoEliminacion(false)}
                        className="alert-cerrar"
                    >
                        cerrar
                    </button>
                </div>
            )}

            <div className="page-header">
                <span className="page-kicker">Explorá</span>
                <h1>Catálogo de Subastas</h1>
                <p className="page-subtitulo">
                    {busquedaUrl
                        ? <>Resultados para "<strong>{busquedaUrl}</strong>"</>
                        : "Encontrá algo único entre las subastas activas, próximas y finalizadas."}
                </p>
                <div className="page-divisor"></div>
            </div>

            <form onSubmit={handleSubmit} className="panel" style={{ marginBottom: 24 }}>
                <div className="filtros-grid">
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
                <div className="acciones-fila">
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
                <div className="catalogo-grid">
                    {subastas.map((subasta) => (
                        <SubastaCard key={subasta.id} subasta={subasta} />
                    ))}
                </div>
            )}
        </div>
    );
}

export default Catalogo;