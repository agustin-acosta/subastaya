import { useEffect, useState } from "react";
import { obtenerUsuarios } from "../api/subastasApi";
import { UserContext } from "./UserContextDefinition";

export function UserProvider({ children }) {
    const [usuarios, setUsuarios] = useState([]);
    const [currentUserId, setCurrentUserId] = useState(null);

    useEffect(() => {
        obtenerUsuarios().then((data) => {
            setUsuarios(data);
            if (data.length > 0) setCurrentUserId(data[0].id);
        });
    }, []);

    const currentUser = usuarios.find((u) => u.id === currentUserId) || null;

    return (
        <UserContext.Provider
            value={{ usuarios, currentUser, currentUserId, setCurrentUserId }}
        >
            {children}
        </UserContext.Provider>
    );
}