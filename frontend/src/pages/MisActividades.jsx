import { useState, useEffect } from "react";
import { Link } from "react-router-dom";
import { obtenerMisPublicaciones, obtenerMisPujas } from "../api/subastasApi";
import { useUser } from "../context/useUser";
import SubastaCard from "../components/SubastaCard";

function MisActividades() {
    const { sesion } = useUser();
    const [publicaciones, setPublicaciones] = useState([]);
    const [pujas, setPujas] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        if (!sesion) return;
        let cancelado = false;
        Promise.all([obtenerMisPublicaciones(), obtenerMisPujas()])
            .then(([datosPublicaciones, datosPujas]) => {
                if (!cancelado) {
                    setPublicaciones(datosPublicaciones);
                    setPujas(datosPujas);
                }
            })
            .catch((err) => {
                if (!cancelado) setError(err.message);
            })
            .finally(() => {
                if (!cancelado) setCargando(false);
            });
        return () => { cancelado = true; };
    }, [sesion]);

    if (!sesion) return <div className="container"><p>Iniciá sesión para ver tus actividades.</p></div>;
    if (cargando) return <div className="container"><p>Cargando...</p></div>;
    if (error) return <div className="container"><p>Error: {error}</p></div>;

    return (
        <div className="container">
            <h1>Mis Actividades</h1>

            <h2 style={{ marginTop: 24 }}>Mis Publicaciones</h2>
            {publicaciones.length === 0 ? (
                <p className="card-muted">Todavía no publicaste ninguna subasta.</p>
            ) : (
                    <div className="catalogo-grid">
                    {publicaciones.map((subasta) => (
                        <SubastaCard key={subasta.id} subasta={subasta} />
                    ))}
                </div>
            )}

            <h2 style={{ marginTop: 32 }}>Mis Pujas</h2>
            {pujas.length === 0 ? (
                <p className="card-muted">Todavía no ofertaste en ninguna subasta.</p>
            ) : (
                    <div className="catalogo-grid">
                    {pujas.map((p) => (
                        <Link key={p.subastaId} to={`/subastas/${p.subastaId}`} className="card">
                            <div className="card-body">
                                <span className={`badge badge-${p.estado.toLowerCase()}`}>{p.estado}</span>
                                <div className="card-title">{p.titulo}</div>
                                <div className="card-muted">Precio actual: ${p.precioActual.toLocaleString()}</div>
                                <div className="card-muted">Mi mejor oferta: ${p.miMejorOferta.toLocaleString()}</div>
                                {p.estado === "Activa" && (
                                    p.estoyLiderando ? (
                                        <div className="alert alert-exito" style={{ marginTop: 8 }}>Liderando</div>
                                    ) : (
                                        <div className="alert alert-error" style={{ marginTop: 8 }}>Superado</div>
                                    )
                                )}
                                {p.estado === "Finalizada" && (
                                    p.estoyLiderando ? (
                                        <div className="alert alert-exito" style={{ marginTop: 8 }}>¡Ganaste!</div>
                                    ) : (
                                        <div className="alert alert-error" style={{ marginTop: 8 }}>No ganaste</div>
                                    )
                                )}
                            </div>
                        </Link>
                    ))}
                </div>
            )}
        </div>
    );
}

export default MisActividades;