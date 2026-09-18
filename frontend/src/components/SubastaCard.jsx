import { useState } from "react";
import { Link } from "react-router-dom";
import CountdownTimer from "./CountdownTimer";

const ICONOS = {
    Tecnología: "💻",
    Coleccionables: "🎴",
    Indumentaria: "👕",
    Vehículos: "🚗",
};

function SubastaCard({ subasta }) {
    const [imagenRota, setImagenRota] = useState(false);
    const monto = subasta.pujaActualMonto ?? subasta.precioBase;
    const badgeClass = `badge badge-${subasta.estado.toLowerCase()}`;
    const mostrarImagen = Boolean(subasta.urlImagen) && !imagenRota;
    const hayPujas = subasta.pujaActualMonto != null;
    
    const cantidadPujas = subasta.cantidadPujas ?? 0;

    return (
        <Link to={`/subastas/${subasta.id}`} className="card">
            <div className="card-image">
                {mostrarImagen ? (
                    <img src={subasta.urlImagen} alt={subasta.titulo} onError={() => setImagenRota(true)} />
                ) : (
                    ICONOS[subasta.categoriaNombre] || "📦"
                )}
                <span className="card-pujas-badge">
                    {cantidadPujas} {cantidadPujas === 1 ? "puja" : "pujas"}
                </span>
            </div>
            <div className="card-body">
                <span className={badgeClass}>{subasta.estado}</span>
                <div className="card-title">{subasta.titulo}</div>
                <div className="card-muted">{subasta.categoriaNombre}</div>
                <div className="card-precio-fila">
                    <span className="card-precio-label">{hayPujas ? "Puja actual" : "Precio base"}</span>
                    <div className="card-price">${monto.toLocaleString()}</div>
                </div>
                {subasta.estado === "Activa" && (
                    <div className="card-tiempo-fila">
                        <span className="card-tiempo-label">Cierra en</span>
                        <CountdownTimer fechaFin={subasta.fechaFin} />
                    </div>
                )}
            </div>
        </Link>
    );
}

export default SubastaCard;