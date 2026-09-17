import { useState, useEffect } from "react";
import { Link, NavLink, useNavigate } from "react-router-dom";
import { useUser } from "../context/useUser";

function claseLink({ isActive }) {
    return isActive ? "nav-link-activo" : undefined;
}

function Navbar() {
    const { sesion, logout } = useUser();
    const [scrolleado, setScrolleado] = useState(false);
    const [busqueda, setBusqueda] = useState("");
    const navigate = useNavigate();

    useEffect(() => {
        function manejarScroll() {
            setScrolleado(window.scrollY > 10);
        }
        window.addEventListener("scroll", manejarScroll);
        return () => window.removeEventListener("scroll", manejarScroll);
    }, []);

    function handleBuscar(e) {
        e.preventDefault();
        const texto = busqueda.trim();
        navigate(texto ? `/?busqueda=${encodeURIComponent(texto)}` : "/");
    }

    return (
        <div className={`navbar${scrolleado ? " navbar-scrolled" : ""}`}>
            <div className="navbar-inner">
                <Link to="/" className="brand">
                    SubastaYa
                </Link>

                <form className="navbar-buscador" onSubmit={handleBuscar}>
                    <input
                        type="text"
                        placeholder="Buscar subastas..."
                        value={busqueda}
                        onChange={(e) => setBusqueda(e.target.value)}
                        aria-label="Buscar subastas"
                    />
                    <button type="submit" aria-label="Buscar">🔍</button>
                </form>

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