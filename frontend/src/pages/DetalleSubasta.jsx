import { useState, useEffect } from "react";
import { obtenerSubastaPorId, ofertar, obtenerUsuarios } from "../api/subastasApi";

function DetalleSubasta({ subastaId }) {
    const [subasta, setSubasta] = useState(null);
    const [cargando, setCargando] = useState(true);
    const [error, setError] = useState(null);
    const [tick, setTick] = useState(0);

    const [usuarios, setUsuarios] = useState([]);
    const [compradorId, setCompradorId] = useState("");
    const [monto, setMonto] = useState("");
    const [mensajeOferta, setMensajeOferta] = useState(null);

    useEffect(() => {
        let cancelado = false;

        obtenerUsuarios()
            .then((data) => {
                if (!cancelado) {
                    setUsuarios(data);
                    if (data.length > 0) {
                        setCompradorId(data[0].id);
                    }
                }
            })
            .catch(() => { });

        return () => {
            cancelado = true;
        };
    }, []);

    useEffect(() => {
        let cancelado = false;

        async function cargar() {
            try {
                const data = await obtenerSubastaPorId(subastaId);
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

        return () => {
            cancelado = true;
        };
    }, [subastaId, tick]);

    useEffect(() => {
        const intervalo = setInterval(() => {
            setTick((t) => t + 1);
        }, 2500);

        return () => clearInterval(intervalo);
    }, []);

    async function handleOfertar(e) {
        e.preventDefault();
        setMensajeOferta(null);

        try {
            await ofertar(subastaId, Number(compradorId), Number(monto));
            setMensajeOferta({ tipo: "exito", texto: "¡Oferta registrada!" });
            setMonto("");
            setTick((t) => t + 1);
        } catch (err) {
            setMensajeOferta({ tipo: "error", texto: err.message });
        }
    }

    if (cargando) return <p>Cargando...</p>;
    if (error) return <p>Error: {error}</p>;
    if (!subasta) return <p>La subasta no existe.</p>;

    const montoActual = subasta.pujaActualMonto ?? subasta.precioBase;

    return (
        <div>
            <h2>{subasta.titulo}</h2>
            <p>{subasta.descripcion}</p>
            <p>Categoría: {subasta.categoriaNombre}</p>
            <p>Vendedor: {subasta.vendedorNombre}</p>
            <p>
                <strong>Monto actual: ${montoActual}</strong>
            </p>
            <p>Cierra: {new Date(subasta.fechaFin).toLocaleString()}</p>
            <p>Estado: {subasta.estado}</p>
            <p>Cantidad de ofertas: {subasta.cantidadPujas}</p>

            {subasta.estado === "Activa" && usuarios.length > 0 && (
                <form onSubmit={handleOfertar}>
                    <h3>Ofertar</h3>
                    <label>
                        Comprador:
                        <select
                            value={compradorId}
                            onChange={(e) => setCompradorId(e.target.value)}
                        >
                            {usuarios.map((u) => (
                                <option key={u.id} value={u.id}>
                                    {u.email}
                                </option>
                            ))}
                        </select>
                    </label>
                    <br />
                    <label>
                        Monto:
                        <input
                            type="number"
                            value={monto}
                            onChange={(e) => setMonto(e.target.value)}
                            min={montoActual + subasta.incrementoMinimo}
                            required
                        />
                    </label>
                    <br />
                    <button type="submit">Ofertar</button>
                </form>
            )}

            {mensajeOferta && (
                <p style={{ color: mensajeOferta.tipo === "error" ? "red" : "green" }}>
                    {mensajeOferta.texto}
                </p>
            )}
        </div>
    );
}

export default DetalleSubasta;