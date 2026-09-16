import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { obtenerSubastaPorId, modificarSubasta } from "../api/subastasApi";

function EditarSubasta() {
    const { id } = useParams();
    const navigate = useNavigate();
    const [cargando, setCargando] = useState(true);
    const [mensaje, setMensaje] = useState(null);

    const [form, setForm] = useState({
        titulo: "",
        descripcion: "",
        urlImagen: "",
        precioBase: "",
        incrementoMinimo: "",
        fechaFin: "",
    });

    useEffect(() => {
        obtenerSubastaPorId(id).then((subasta) => {
            if (!subasta) return;
            setForm({
                titulo: subasta.titulo,
                descripcion: subasta.descripcion,
                urlImagen: subasta.urlImagen ?? "",
                precioBase: subasta.precioBase,
                incrementoMinimo: subasta.incrementoMinimo,
                fechaFin: subasta.fechaFin.slice(0, 16),
            });
            setCargando(false);
        });
    }, [id]);

    function handleChange(campo, valor) {
        setForm((f) => ({ ...f, [campo]: valor }));
    }

    async function handleSubmit(e) {
        e.preventDefault();
        setMensaje(null);
        try {
            await modificarSubasta(id, {
                titulo: form.titulo,
                descripcion: form.descripcion,
                urlImagen: form.urlImagen,
                precioBase: Number(form.precioBase),
                incrementoMinimo: Number(form.incrementoMinimo),
                fechaFin: new Date(form.fechaFin).toISOString(),
            });
            navigate(`/subastas/${id}`);
        } catch (err) {
            setMensaje({ tipo: "error", texto: err.message });
        }
    }

    if (cargando) return <div className="container"><p>Cargando...</p></div>;

    return (
        <div className="container" style={{ maxWidth: 600 }}>
            <h1>Editar subasta</h1>
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
                    <label>Precio base</label>
                    <input type="number" value={form.precioBase} onChange={(e) => handleChange("precioBase", e.target.value)} min="1" required />
                </div>
                <div className="form-group">
                    <label>Incremento mínimo</label>
                    <input type="number" value={form.incrementoMinimo} onChange={(e) => handleChange("incrementoMinimo", e.target.value)} min="1" required />
                </div>
                <div className="form-group">
                    <label>Fecha de fin</label>
                    <input type="datetime-local" value={form.fechaFin} onChange={(e) => handleChange("fechaFin", e.target.value)} required />
                </div>
                <button type="submit" className="btn btn-primary btn-block">Guardar cambios</button>
                {mensaje && <div className={`alert alert-${mensaje.tipo}`}>{mensaje.texto}</div>}
            </form>
        </div>
    );
}

export default EditarSubasta;