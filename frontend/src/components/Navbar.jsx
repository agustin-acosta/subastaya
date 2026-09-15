import { Link } from "react-router-dom";
import { useUser } from "../context/useUser";

function Navbar() {
    const { usuarios, currentUserId, setCurrentUserId } = useUser();

    return (
        <div className="navbar">
            <div className="navbar-inner">
                <Link to="/" className="brand">SubastaYa</Link>
                <div className="nav-links">
                    <Link to="/">Catálogo</Link>
                    <Link to="/crear">Publicar subasta</Link>
                    <Link to="/wallet">Mi billetera</Link>
                    <select
                        className="user-select"
                        value={currentUserId || ""}
                        onChange={(e) => setCurrentUserId(Number(e.target.value))}
                    >
                        {usuarios.map((u) => (
                            <option key={u.id} value={u.id}>
                                {u.email}
                            </option>
                        ))}
                    </select>
                </div>
            </div>
        </div>
    );
}

export default Navbar;