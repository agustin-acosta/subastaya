import { useState } from "react";
import Catalogo from "./pages/Catalogo";
import DetalleSubasta from "./pages/DetalleSubasta";

function App() {
    const [subastaSeleccionada, setSubastaSeleccionada] = useState(null);

    if (subastaSeleccionada) {
        return (
            <div>
                <button onClick={() => setSubastaSeleccionada(null)}>
                    ← Volver al catálogo
                </button>
                <DetalleSubasta subastaId={subastaSeleccionada} />
            </div>
        );
    }

    return (
        <div>
            <h1>SubastaYa</h1>
            <Catalogo onSeleccionar={setSubastaSeleccionada} />
        </div>
    );
}

export default App;