import { useState, useEffect } from "react";
import { obtenerSubastas } from "../api/subastasApi";

function Catalogo({ onSeleccionar }) {
    const [subastas, setSubastas] = useState([]);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        obtenerSubastas()
            .then((data) => {
                setSubastas(data.items);
                setCargando(false);
            })
            .catch((err) => {
                setError(err.message);
                setCargando(false);
            });
    }, []);

    if (cargando) return <p>Cargando subastas...</p>;
    if (error) return <p>Error: {error}</p>;

    return (
        <div>
            <h2>Catálogo de Subastas</h2>
            <ul>
                {subastas.map((subasta) => (
                    <li key={subasta.id}>
                        <button onClick={() => onSeleccionar(subasta.id)}>
                            {subasta.titulo} — {subasta.categoriaNombre} — $
                            {subasta.pujaActualMonto ?? subasta.precioBase} —{" "}
                            {subasta.estado}
                        </button>
                    </li>
                ))}
            </ul>
        </div>
    );
}

export default Catalogo;