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
    const [menuAbierto, setMenuAbierto] = useState(false);
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
        setMenuAbierto(false);
    }

    function cerrarMenu() {
        setMenuAbierto(false);
    }

    function handleLogout() {
        cerrarMenu();
        logout();
    }

    return (
        <div className={`navbar${scrolleado ? " navbar-scrolled" : ""}`}>
            <div className="navbar-inner">
                <Link to="/" className="brand" onClick={cerrarMenu}>
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

                <button
                    type="button"
                    className="navbar-hamburguesa"
                    aria-label={menuAbierto ? "Cerrar menú" : "Abrir menú"}
                    aria-expanded={menuAbierto}
                    onClick={() => setMenuAbierto((abierto) => !abierto)}
                >
                    {menuAbierto ? "✕" : "☰"}
                </button>

                <div className={`nav-links${menuAbierto ? " nav-links-abierto" : ""}`}>
                    <NavLink to="/" end className={claseLink} onClick={cerrarMenu}>Catálogo</NavLink>
                    {sesion && <NavLink to="/crear" className={claseLink} onClick={cerrarMenu}>Publicar subasta</NavLink>}
                    {sesion && <NavLink to="/mis-actividades" className={claseLink} onClick={cerrarMenu}>Mis actividades</NavLink>}
                    {sesion && <NavLink to="/wallet" className={claseLink} onClick={cerrarMenu}>Mi billetera</NavLink>}
                    {sesion ? (
                        <>
                            <span className="card-muted">{sesion.nombre}</span>
                            <button className="btn btn-primary" onClick={handleLogout}>Cerrar sesión</button>
                        </>
                    ) : (
                        <NavLink to="/login" className={claseLink} onClick={cerrarMenu}>Iniciar sesión</NavLink>
                    )}
                </div>
            </div>
        </div>
    );
}

export default Navbar;