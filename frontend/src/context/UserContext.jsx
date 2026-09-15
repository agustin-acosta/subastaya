import { useState } from "react";
import { login as loginRequest } from "../api/subastasApi";
import { UserContext } from "./UserContextDefinition";

function leerSesionGuardada() {
    try {
        const guardado = localStorage.getItem("sesion");
        return guardado ? JSON.parse(guardado) : null;
    } catch {
        return null;
    }
}

export function UserProvider({ children }) {
    const [sesion, setSesion] = useState(leerSesionGuardada);

    async function login(email, password) {
        const resultado = await loginRequest(email, password);
        const nuevaSesion = {
            token: resultado.token,
            usuarioId: resultado.usuarioId,
            nombre: resultado.nombre,
            email: resultado.email,
        };
        localStorage.setItem("sesion", JSON.stringify(nuevaSesion));
        setSesion(nuevaSesion);
    }

    function logout() {
        localStorage.removeItem("sesion");
        setSesion(null);
    }

    return (
        <UserContext.Provider value={{ sesion, login, logout }}>
            {children}
        </UserContext.Provider>
    );
}