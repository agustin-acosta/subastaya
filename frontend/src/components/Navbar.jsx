import { Link } from "react-router-dom";
import { useUser } from "../context/useUser";

function Navbar() {
    const { sesion, logout } = useUser();

    return (
        <div className="navbar">
            <div className="navbar-inner">
                <Link to="/" className="brand">SubastaYa</Link>
                <div className="nav-links">
                    <Link to="/">Catálogo</Link>
                    {sesion && <Link to="/crear">Publicar subasta</Link>}
                    {sesion && <Link to="/mis-actividades">Mis actividades</Link>}
                    {sesion && <Link to="/wallet">Mi billetera</Link>}
                    {sesion ? (
                        <>
                            <span className="card-muted">{sesion.nombre}</span>
                            <button className="btn btn-primary" onClick={logout}>Cerrar sesión</button>
                        </>
                    ) : (
                        <Link to="/login">Iniciar sesión</Link>
                    )}
                </div>
            </div>
        </div>
    );
}

export default Navbar;