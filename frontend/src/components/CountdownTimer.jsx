import { useState, useEffect } from "react";

function formatear(ms) {
    if (ms <= 0) return "Finalizada";
    const totalSeg = Math.floor(ms / 1000);
    const dias = Math.floor(totalSeg / 86400);
    const horas = Math.floor((totalSeg % 86400) / 3600);
    const min = Math.floor((totalSeg % 3600) / 60);
    const seg = totalSeg % 60;
    if (dias > 0) return `${dias}d ${horas}h`;
    if (horas > 0) return `${horas}h ${min}m`;
    return `${min}m ${seg}s`;
}

function CountdownTimer({ fechaFin }) {
    const [restante, setRestante] = useState(null);

    useEffect(() => {
        function calcular() {
            setRestante(new Date(fechaFin).getTime() - Date.now());
        }

        calcular();
        const id = setInterval(calcular, 1000);
        return () => clearInterval(id);
    }, [fechaFin]);

    if (restante === null) {
        return <span className="countdown">...</span>;
    }

    const urgente = restante > 0 && restante <= 60000;

    return (
        <span className={`countdown ${urgente ? "urgente" : ""}`}>
            {formatear(restante)}
        </span>
    );
}

export default CountdownTimer;