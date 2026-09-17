import { Link, NavLink } from "react-router-dom";
import { useUser } from "../context/useUser";

function claseLink({ isActive }) {
    return isActive ? "nav-link-activo" : undefined;
}

function Navbar() {
    const { sesion, logout } = useUser();

    return (
        <div className="navbar">
            <div className="navbar-inner">
                <Link to="/" className="brand">
                    <span className="brand-icon">🔨</span>
                    Subasta<span className="brand-accent">Ya</span>
                </Link>
                <div className="nav-links">
                    <NavLink to="/" end className={claseLink}>Catálogo</NavLink>
                    {sesion && <NavLink to="/crear" className={claseLink}>Publicar subasta</NavLink>}
                    {sesion && <NavLink to="/mis-actividades" className={claseLink}>Mis actividades</NavLink>}
                    {sesion && <NavLink to="/wallet" className={claseLink}>Mi billetera</NavLink>}
                    {sesion ? (
                        <>
                            <span className="card-muted">{sesion.nombre}</span>
                            <button className="btn btn-primary" onClick={logout}>Cerrar sesión</button>
                        </>
                    ) : (
                        <NavLink to="/login" className={claseLink}>Iniciar sesión</NavLink>
                    )}
                </div>
            </div>
        </div>
    );
}

export default Navbar;