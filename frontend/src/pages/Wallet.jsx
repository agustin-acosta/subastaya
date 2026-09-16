import { useState, useEffect } from "react";
import { obtenerBalance, depositar, obtenerMovimientos } from "../api/subastasApi";
import { useUser } from "../context/useUser";

const ETIQUETAS_TIPO = {
    Deposito: "Depósito",
    Retencion: "Retención",
    Liberacion: "Liberación",
    Pago: "Pago",
    Cobro: "Cobro",
};

function Wallet() {
    const { sesion } = useUser();
    const [balance, setBalance] = useState(null);
    const [movimientos, setMovimientos] = useState([]);
    const [monto, setMonto] = useState("");
    const [mensaje, setMensaje] = useState(null);
    const [tick, setTick] = useState(0);

    useEffect(() => {
        if (!sesion) return;
        obtenerBalance().then(setBalance);
        obtenerMovimientos().then(setMovimientos).catch(() => { });
    }, [sesion, tick]);

    async function handleDepositar(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            await depositar(Number(monto));
            setMensaje({ tipo: "exito", texto: "Depósito realizado." });
            setMonto("");
            setTick((t) => t + 1);
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    if (!sesion) return <div className="container"><p>Iniciá sesión para ver tu billetera.</p></div>;

    return (
        <div className="container">
            <h1>Mi Billetera</h1>
            <p className="card-muted">{sesion.email}</p>

            {balance && (
                <div className="wallet-grid">
                    <div className="wallet-stat">
                        <div className="valor">${balance.saldoTotal.toLocaleString()}</div>
                        <div className="label">Saldo Total</div>
                    </div>
                    <div className="wallet-stat">
                        <div className="valor">${balance.saldoRetenido.toLocaleString()}</div>
                        <div className="label">Retenido en garantía</div>
                    </div>
                    <div className="wallet-stat">
                        <div className="valor">${balance.saldoDisponible.toLocaleString()}</div>
                        <div className="label">Disponible</div>
                    </div>
                </div>
            )}

            <div className="panel" style={{ maxWidth: 400 }}>
                <h3>Cargar saldo</h3>
                <form onSubmit={handleDepositar}>
                    <div className="form-group">
                        <label>Monto a depositar</label>
                        <input
                            type="number"
                            value={monto}
                            onChange={(e) => setMonto(e.target.value)}
                            min="1"
                            required
                        />
                    </div>
                    <button type="submit" className="btn btn-primary btn-block">
                        Depositar
                    </button>
                </form>
                {mensaje && (
                    <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>
                )}
            </div>

            <div className="panel" style={{ marginTop: 24 }}>
                <h3>Historial de movimientos</h3>
                {movimientos.length === 0 ? (
                    <p className="card-muted">Todavía no tenés movimientos.</p>
                ) : (
                    <table style={{ width: "100%", borderCollapse: "collapse" }}>
                        <thead>
                            <tr style={{ textAlign: "left", borderBottom: "1px solid var(--border, #ccc)" }}>
                                <th style={{ padding: "8px 4px" }}>Fecha</th>
                                <th style={{ padding: "8px 4px" }}>Tipo</th>
                                <th style={{ padding: "8px 4px" }}>Monto</th>
                                <th style={{ padding: "8px 4px" }}>Subasta</th>
                            </tr>
                        </thead>
                        <tbody>
                            {movimientos.map((m, i) => (
                                <tr key={i} style={{ borderBottom: "1px solid var(--border, #eee)" }}>
                                    <td style={{ padding: "8px 4px" }}>{new Date(m.fecha).toLocaleString()}</td>
                                    <td style={{ padding: "8px 4px" }}>{ETIQUETAS_TIPO[m.tipo] ?? m.tipo}</td>
                                    <td style={{ padding: "8px 4px" }}>${m.monto.toLocaleString()}</td>
                                    <td style={{ padding: "8px 4px" }}>
                                        {m.subastaId ? (
                                            <a href={`/subastas/${m.subastaId}`}>#{m.subastaId}</a>
                                        ) : (
                                            "-"
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
        </div>
    );
}

export default Wallet;