import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useUser } from "../context/useUser";

function Login() {
    const { login } = useUser();
    const navigate = useNavigate();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [mensaje, setMensaje] = useState(null);
    const [cargando, setCargando] = useState(false);

    async function handleSubmit(e) {
        e.preventDefault();
        setMensaje(null);
        setCargando(true);
        try {
            await login(email, password);
            navigate("/");
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        } finally {
            setCargando(false);
        }
    }

    return (
        <div className="container" style={{ maxWidth: 400 }}>
            <h1>Iniciar sesión</h1>
            <form onSubmit={handleSubmit} className="panel">
                <div className="form-group">
                    <label>Email</label>
                    <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
                </div>
                <div className="form-group">
                    <label>Contraseña</label>
                    <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
                </div>
                <button type="submit" className="btn btn-primary btn-block" disabled={cargando}>
                    {cargando ? "Ingresando..." : "Ingresar"}
                </button>
                {mensaje && <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>}
            </form>
        </div>
    );
}

export default Login;