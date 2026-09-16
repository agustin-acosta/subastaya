import { useState, useEffect, useRef } from "react";

const SEGUNDOS_URGENTE = 60;

function formatear(ms) {
    if (ms <= 0) return "Finalizada";
    const totalSeg = Math.floor(ms / 1000);
    const dias = Math.floor(totalSeg / 86400);
    const horas = Math.floor((totalSeg % 86400) / 3600);
    const min = Math.floor((totalSeg % 3600) / 60);
    const seg = totalSeg % 60;
    const pad = (n) => String(n).padStart(2, "0");
    return `${dias}d ${pad(horas)}h ${pad(min)}m ${pad(seg)}s`;
}

function reproducirAlerta() {
    try {
        const contexto = new (window.AudioContext || window.webkitAudioContext)();
        const oscilador = contexto.createOscillator();
        const ganancia = contexto.createGain();
        oscilador.type = "sine";
        oscilador.frequency.value = 880;
        ganancia.gain.setValueAtTime(0.15, contexto.currentTime);
        oscilador.connect(ganancia);
        ganancia.connect(contexto.destination);
        oscilador.start();
        oscilador.stop(contexto.currentTime + 0.25);
    }
    catch {
        //
    }
}

function CountdownTimer({ fechaFin }) {
    const [restante, setRestante] = useState(null);

    const yaAlertoRef = useRef(false);

    useEffect(() => {
        function calcular() {
            const nuevoRestante = new Date(fechaFin).getTime() - Date.now();
            setRestante(nuevoRestante);

            const esUrgente = nuevoRestante > 0 && nuevoRestante <= SEGUNDOS_URGENTE * 1000;
            if (esUrgente && !yaAlertoRef.current) {
                yaAlertoRef.current = true;
                reproducirAlerta();
            } else if (!esUrgente) {
                yaAlertoRef.current = false;
            }
        }

        calcular();
        const id = setInterval(calcular, 1000);
        return () => clearInterval(id);
    }, [fechaFin]);

    if (restante === null) {
        return <span className="countdown">...</span>;
    }

    const urgente = restante > 0 && restante <= SEGUNDOS_URGENTE * 1000;

    return (
        <span className={`countdown ${urgente ? "urgente" : ""}`}>
            {formatear(restante)}
        </span>
    );
}

export default CountdownTimer;