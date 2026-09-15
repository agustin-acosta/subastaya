import { useContext } from "react";
import { UserContext } from "./UserContextDefinition";

export function useUser() {
    return useContext(UserContext);
}