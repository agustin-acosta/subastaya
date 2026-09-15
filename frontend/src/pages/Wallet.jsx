import { useState, useEffect } from "react";
import { obtenerBalance, depositar } from "../api/subastasApi";
import { useUser } from "../context/UserContext";

function Wallet() {
    const { currentUser, currentUserId } = useUser();
    const [balance, setBalance] = useState(null);
    const [monto, setMonto] = useState("");
    const [mensaje, setMensaje] = useState(null);
    const [tick, setTick] = useState(0);

    useEffect(() => {
        if (!currentUserId) return;
        obtenerBalance(currentUserId).then(setBalance);
    }, [currentUserId, tick]);

    async function handleDepositar(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            await depositar(currentUserId, Number(monto));
            setMensaje({ tipo: "exito", texto: "Depósito realizado." });
            setMonto("");
            setTick((t) => t + 1);
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    if (!currentUser) return <div className="container"><p>Cargando...</p></div>;

    return (
        <div className="container">
            <h1>Mi Billetera</h1>
            <p className="card-muted">{currentUser.email}</p>

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
        </div>
    );
}

export default Wallet;