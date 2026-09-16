import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { crearSubasta, obtenerCategorias } from "../api/subastasApi";

function CrearSubasta() {
    const navigate = useNavigate();
    const [categorias, setCategorias] = useState([]);
    const [mensaje, setMensaje] = useState(null);

    const [form, setForm] = useState({
        categoriaId: "",
        titulo: "",
        descripcion: "",
        urlImagen: "",
        precioBase: "",
        incrementoMinimo: "",
        fechaInicio: "",
        fechaFin: "",
    });

    useEffect(() => {
        obtenerCategorias().then((cats) => {
            setCategorias(cats);
            if (cats.length > 0) {
                setForm((f) => ({ ...f, categoriaId: cats[0].id }));
            }
        });
    }, []);

    function handleChange(campo, valor) {
        setForm((f) => ({ ...f, [campo]: valor }));
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            const resultado = await crearSubasta({
                categoriaId: Number(form.categoriaId),
                titulo: form.titulo,
                descripcion: form.descripcion,
                urlImagen: form.urlImagen,
                precioBase: Number(form.precioBase),
                incrementoMinimo: Number(form.incrementoMinimo),
                fechaInicio: new Date(form.fechaInicio).toISOString(),
                fechaFin: new Date(form.fechaFin).toISOString(),
            });
            navigate(`/subastas/${resultado.id}`, { state: { creada: true } });
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    return (
        <div className="container" style={{ maxWidth: 600 }}>
            <h1>Publicar nueva subasta</h1>
            <form onSubmit={handleSubmit} className="panel">
                <div className="form-group">
                    <label>Título</label>
                    <input value={form.titulo} onChange={(e) => handleChange("titulo", e.target.value)} required />
                </div>
                <div className="form-group">
                    <label>Descripción</label>
                    <textarea rows="3" value={form.descripcion} onChange={(e) => handleChange("descripcion", e.target.value)} required />
                </div>
                <div className="form-group">
                    <label>URL de imagen (opcional)</label>
                    <input value={form.urlImagen} onChange={(e) => handleChange("urlImagen", e.target.value)} />
                </div>
                <div className="form-group">
                    <label>Categoría</label>
                    <select value={form.categoriaId} onChange={(e) => handleChange("categoriaId", e.target.value)}>
                        {categorias.map((c) => <option key={c.id} value={c.id}>{c.nombre}</option>)}
                    </select>
                </div>
                <div className="form-group">
                    <label>Precio base</label>
                    <input type="number" value={form.precioBase} onChange={(e) => handleChange("precioBase", e.target.value)} min="1" required />
                </div>
                <div className="form-group">
                    <label>Incremento mínimo</label>
                    <input type="number" value={form.incrementoMinimo} onChange={(e) => handleChange("incrementoMinimo", e.target.value)} min="1" required />
                </div>
                <div className="form-group">
                    <label>Fecha de inicio</label>
                    <input type="datetime-local" value={form.fechaInicio} onChange={(e) => handleChange("fechaInicio", e.target.value)} required />
                </div>
                <div className="form-group">
                    <label>Fecha de fin</label>
                    <input type="datetime-local" value={form.fechaFin} onChange={(e) => handleChange("fechaFin", e.target.value)} required />
                </div>
                <button type="submit" className="btn btn-primary btn-block">Publicar</button>
                {mensaje && <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>}
            </form>
        </div>
    );
}

export default CrearSubasta;