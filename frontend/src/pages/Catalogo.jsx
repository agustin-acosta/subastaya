import { useState, useEffect } from "react";
import { obtenerSubastas } from "../api/subastasApi";
import SubastaCard from "../components/SubastaCard";

function Catalogo() {
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

    if (cargando) return <div className="container"><p>Cargando subastas...</p></div>;
    if (error) return <div className="container"><p>Error: {error}</p></div>;

    return (
        <div className="container">
            <h1>Catálogo de Subastas</h1>
            {subastas.length === 0 ? (
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