import { useState, useEffect } from "react";

const DISTANCIA_DESVANECIDO = 220;

function Hero() {
    const [progreso, setProgreso] = useState(0);

    useEffect(() => {
        let pedidoPendiente = false;

        function calcularProgreso() {
            const scrollActual = window.scrollY;
            const nuevoProgreso = Math.min(scrollActual / DISTANCIA_DESVANECIDO, 1);
            setProgreso(nuevoProgreso);
            pedidoPendiente = false;
        }

        function manejarScroll() {
            if (!pedidoPendiente) {
                window.requestAnimationFrame(calcularProgreso);
                pedidoPendiente = true;
            }
        }

        window.addEventListener("scroll", manejarScroll);
        return () => window.removeEventListener("scroll", manejarScroll);
    }, []);

    const estiloDesvanecido = {
        opacity: 1 - progreso,
        transform: `scale(${1 - progreso * 0.12})`,
    };

    return (
        <section className="hero" style={estiloDesvanecido}>
            <p className="hero-eslogan">El Futuro de las Subastas Digitales</p>
            <a href="#catalogo" className="btn btn-primary btn-hero">
                Explorar Subastas
            </a>
        </section>
    );
}

export default Hero;