import { Link } from "react-router-dom";
import CountdownTimer from "./CountdownTimer";

const ICONOS = {
    Tecnología: "💻",
    Coleccionables: "🎴",
    Indumentaria: "👕",
    Vehículos: "🚗",
};

function SubastaCard({ subasta }) {
    const monto = subasta.pujaActualMonto ?? subasta.precioBase;
    const badgeClass = `badge badge-${subasta.estado.toLowerCase()}`;

    return (
        <Link to={`/subastas/${subasta.id}`} className="card">
            <div className="card-image">{ICONOS[subasta.categoriaNombre] || "📦"}</div>
            <div className="card-body">
                <span className={badgeClass}>{subasta.estado}</span>
                <div className="card-title">{subasta.titulo}</div>
                <div className="card-muted">{subasta.categoriaNombre}</div>
                <div className="card-price">${monto.toLocaleString()}</div>
                {subasta.estado === "Activa" && (
                    <CountdownTimer fechaFin={subasta.fechaFin} />
                )}
            </div>
        </Link>
    );
}

export default SubastaCard;