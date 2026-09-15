import { useState, useEffect } from "react";
import { useParams } from "react-router-dom";
import { obtenerSubastaPorId, ofertar } from "../api/subastasApi";
import { useUser } from "../context/useUser";
import CountdownTimer from "../components/CountdownTimer";

function DetalleSubasta() {
    const { id } = useParams();
    const { usuarios, currentUserId, setCurrentUserId } = useUser();

    const [subasta, setSubasta] = useState(null);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);
    const [tick, setTick] = useState(0);

    const [monto, setMonto] = useState("");
    const [mensaje, setMensaje] = useState(null);

    useEffect(() => {
        let cancelado = false;
        async function cargar() {
            try {
                const data = await obtenerSubastaPorId(id);
                if (!cancelado) {
                    setSubasta(data);
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
    }, [id, tick]);

    useEffect(() => {
        const intervalo = setInterval(() => setTick((t) => t + 1), 2500);
        return () => clearInterval(intervalo);
    }, []);

    async function handleOfertar(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            await ofertar(id, Number(currentUserId), Number(monto));
            setMensaje({ tipo: "exito", texto: "¡Oferta registrada!" });
            setMonto("");
            setTick((t) => t + 1);
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

    return (
        <div className="container">
            <div className="detalle-grid">
                <div>
                    <div className="hero-image">📦</div>
                    <h1 style={{ marginTop: 16 }}>{subasta.titulo}</h1>
                    <span className={badgeClass}>{subasta.estado}</span>
                    <p style={{ marginTop: 12, color: "var(--text-muted)" }}>
                        {subasta.descripcion}
                    </p>
                    <p className="card-muted">Categoría: {subasta.categoriaNombre}</p>
                    <p className="card-muted">Vendedor: {subasta.vendedorNombre}</p>
                    <p className="card-muted">Ofertas realizadas: {subasta.cantidadPujas}</p>
                </div>

                <div className="panel">
                    <div className="card-muted">Monto actual</div>
                    <div className="monto-actual">${montoActual.toLocaleString()}</div>

                    {subasta.estado === "Activa" && (
                        <>
                            <p>
                                Cierra en: <CountdownTimer fechaFin={subasta.fechaFin} />
                            </p>

                            <form onSubmit={handleOfertar}>
                                <div className="form-group">
                                    <label>Ofertar como</label>
                                    <select
                                        value={currentUserId || ""}
                                        onChange={(e) => setCurrentUserId(Number(e.target.value))}
                                    >
                                        {usuarios.map((u) => (
                                            <option key={u.id} value={u.id}>{u.email}</option>
                                        ))}
                                    </select>
                                </div>
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

                    {subasta.estado !== "Activa" && (
                        <p className="card-muted">Esta subasta ya no acepta ofertas.</p>
                    )}

                    {mensaje && (
                        <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>
                    )}
                </div>
            </div>
        </div>
    );
}

export default DetalleSubasta;