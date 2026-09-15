import { Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import Catalogo from "./pages/Catalogo";
import DetalleSubasta from "./pages/DetalleSubasta";
import Wallet from "./pages/Wallet";
import CrearSubasta from "./pages/CrearSubasta";
import Login from "./pages/Login";

function App() {
    return (
        <>
            <Navbar />
            <Routes>
                <Route path="/" element={<Catalogo />} />
                <Route path="/subastas/:id" element={<DetalleSubasta />} />
                <Route path="/wallet" element={<Wallet />} />
                <Route path="/crear" element={<CrearSubasta />} />
                <Route path="/login" element={<Login />} />
            </Routes>
        </>
    );
}

export default App;