import { useState, useEffect } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { obtenerSubastaPorId, obtenerPujas, ofertar, eliminarSubasta, HUB_URL } from "../api/subastasApi";
import { useUser } from "../context/useUser";
import CountdownTimer from "../components/CountdownTimer";

function DetalleSubasta() {
    const { id } = useParams();
    const navigate = useNavigate();
    const location = useLocation();
    const { sesion } = useUser();

    const [subasta, setSubasta] = useState(null);
    const [pujas, setPujas] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);

    const [monto, setMonto] = useState("");
    const [mensaje, setMensaje] = useState(null);
    const [mostrarExitoCreacion, setMostrarExitoCreacion] = useState(Boolean(location.state?.creada));
    const [imagenRota, setImagenRota] = useState(false);

    useEffect(() => {
        if (location.state?.creada) {
            window.history.replaceState({}, "");
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    useEffect(() => {
        let cancelado = false;
        async function cargar() {
            try {
                const [datosSubasta, datosPujas] = await Promise.all([
                    obtenerSubastaPorId(id),
                    obtenerPujas(id),
                ]);
                if (!cancelado) {
                    setSubasta(datosSubasta);
                    setPujas(datosPujas);
                    setCargando(false);
                }
            } catch (err) {
                if (!cancelado) {
                    setError(err.message);
                    setCargando(false);
                }
            }
        }
        cargar();
        return () => { cancelado = true; };
    }, [id]);

    useEffect(() => {
        const subastaId = Number(id);
        const connection = new HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();

        connection.on("NuevaPuja", (notificacion) => {
            setPujas((prev) => [notificacion, ...prev]);
            setSubasta((prev) => prev ? {
                ...prev,
                pujaActualMonto: notificacion.monto,
                cantidadPujas: prev.cantidadPujas + 1,
            } : prev);
        });

        connection.on("ExtensionTiempo", ({ nuevaFechaFin }) => {
            setSubasta((prev) => prev ? { ...prev, fechaFin: nuevaFechaFin } : prev);
        });

        connection.on("CambioEstado", ({ nuevoEstado }) => {
            setSubasta((prev) => prev ? { ...prev, estado: nuevoEstado } : prev);
        });

        connection.onreconnected(() => {
            connection.invoke("UnirseASubasta", subastaId).catch((err) =>
                console.error("No se pudo volver a unirse al grupo tras reconectar:", err)
            );
        });

        connection.start()
            .then(() => connection.invoke("UnirseASubasta", subastaId))
            .catch((err) => console.error("No se pudo conectar al Hub de subastas:", err));

        return () => {
            if (connection.state === "Connected") {
                connection.invoke("SalirDeSubasta", subastaId).catch(() => { });
            }
            connection.stop();
        };
    }, [id]);

    async function handleOfertar(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            await ofertar(id, Number(monto));
            setMensaje({ tipo: "exito", texto: "¡Oferta registrada!" });
            setMonto("");
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    async function handleEliminar() {
        if (!window.confirm("¿Seguro que querés eliminar esta subasta?")) return;
        try {
            await eliminarSubasta(id);
            navigate("/", { state: { eliminada: true } });
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    if (cargando) return <div className="container"><p>Cargando...</p></div>;
    if (error) return <div className="container"><p>Error: {error}</p></div>;
    if (!subasta) return <div className="container"><p>La subasta no existe.</p></div>;

    const montoActual = subasta.pujaActualMonto ?? subasta.precioBase;
    const minimo = montoActual + subasta.incrementoMinimo;
    const badgeClass = `badge badge-${subasta.estado.toLowerCase()}`;

    const esDueno = sesion && Number(sesion.usuarioId) === subasta.vendedorId;
    const puedeEditar = esDueno && subasta.cantidadPujas === 0;
    const esMia = (p) => p.esMia ?? (Boolean(sesion) && p.compradorId === Number(sesion?.usuarioId));

    const misPujas = pujas.filter(esMia);
    const lider = pujas.length > 0 ? pujas.reduce((max, p) => (p.monto > max.monto ? p : max), pujas[0]) : null;
    const estoyLiderando = Boolean(sesion && lider && esMia(lider));
    const fuiSuperado = Boolean(sesion && !estoyLiderando && misPujas.length > 0);

    return (
        <div className="container">
            {mostrarExitoCreacion && (
                <div className="alert alert-exito" style={{ marginBottom: 16 }}>
                    ¡Subasta publicada con éxito!
                    <button
                        type="button"
                        onClick={() => setMostrarExitoCreacion(false)}
                        className="alert-cerrar"
                    >
                        cerrar
                    </button>
                </div>
            )}

            <div className="detalle-grid">
                <div>
                    <div className="hero-image">
                        {subasta.urlImagen && !imagenRota ? (
                            <img
                                src={subasta.urlImagen}
                                alt={subasta.titulo}
                                onError={() => setImagenRota(true)}
                            />
                        ) : (
                            "📦"
                        )}
                    </div>
                    <h1 style={{ marginTop: 16 }}>{subasta.titulo}</h1>
                    <span className={badgeClass}>{subasta.estado}</span>
                    <p style={{ marginTop: 12, color: "var(--text-muted)" }}>
                        {subasta.descripcion}
                    </p>
                    <p className="card-muted">Categoría: {subasta.categoriaNombre}</p>
                    <p className="card-muted">Vendedor: {subasta.vendedorNombre}</p>
                    <p className="card-muted">Ofertas realizadas: {subasta.cantidadPujas}</p>

                    {puedeEditar && (
                        <div className="acciones-fila">
                            <button type="button" className="btn btn-primary" onClick={() => navigate(`/subastas/${id}/editar`)}>
                                Editar
                            </button>
                            <button type="button" className="btn btn-danger" onClick={handleEliminar}>
                                Eliminar
                            </button>
                        </div>
                    )}
                </div>

                <div className="panel">
                    <div className="card-muted">Monto actual</div>
                    <div className="monto-actual">${montoActual.toLocaleString()}</div>

                    {subasta.estado === "Activa" && (
                        <>
                            <p>
                                Cierra en: <CountdownTimer fechaFin={subasta.fechaFin} />
                            </p>

                            {sesion && (
                                <>
                                    {estoyLiderando && (
                                        <div className="alert alert-exito" style={{ marginTop: 8 }}>
                                            🏆 Estás liderando esta subasta
                                        </div>
                                    )}
                                    {fuiSuperado && (
                                        <div className="alert alert-error" style={{ marginTop: 8 }}>
                                            ⚠️ Fuiste superado, ¡mejorá tu oferta!
                                        </div>
                                    )}

                                    <form onSubmit={handleOfertar}>
                                        <div className="form-group">
                                            <label>Monto (mínimo ${minimo.toLocaleString()})</label>
                                            <input
                                                type="number"
                                                value={monto}
                                                onChange={(e) => setMonto(e.target.value)}
                                                min={minimo}
                                                required
                                            />
                                        </div>
                                        <button type="submit" className="btn btn-primary btn-block">
                                            Ofertar
                                        </button>
                                    </form>
                                </>
                            )}

                            {!sesion && (
                                <p className="card-muted">Iniciá sesión para poder ofertar.</p>
                            )}
                        </>
                    )}

                    {subasta.estado === "Finalizada" && estoyLiderando && (
                        <div className="alert alert-exito" style={{ marginTop: 8 }}>
                            🎉 ¡Felicitaciones! Ganaste esta subasta por ${montoActual.toLocaleString()}.
                        </div>
                    )}

                    {subasta.estado === "Finalizada" && esDueno && (
                        <div className="alert alert-exito" style={{ marginTop: 8 }}>
                            ✅ Tu subasta se vendió por ${montoActual.toLocaleString()}.
                        </div>
                    )}

                    {subasta.estado === "Desierta" && (
                        <div className="alert alert-error" style={{ marginTop: 8 }}>
                            Esta subasta finalizó sin ofertas.
                        </div>
                    )}

                    {subasta.estado !== "Activa" && (
                        <p className="card-muted">Esta subasta ya no acepta ofertas.</p>
                    )}

                    {mensaje && (
                        <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>
                    )}
                </div>
            </div>

            <div className="panel" style={{ marginTop: 24 }}>
                <h3>Historial de ofertas</h3>
                {pujas.length === 0 ? (
                    <p className="card-muted">Todavía no hay ofertas para esta subasta.</p>
                ) : (
                    <div className="tabla-wrap">
                        <table className="tabla">
                            <thead>
                                <tr>
                                    <th>Postor</th>
                                    <th>Monto</th>
                                    <th>Fecha y hora</th>
                                </tr>
                            </thead>
                            <tbody>
                                {pujas.map((p, i) => (
                                    <tr key={i}>
                                        <td>
                                            {esMia(p) ? "Vos" : p.compradorSeudonimo}
                                        </td>
                                        <td>${p.monto.toLocaleString()}</td>
                                        <td>{new Date(p.fechaPuja).toLocaleString()}</td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
}

export default DetalleSubasta;