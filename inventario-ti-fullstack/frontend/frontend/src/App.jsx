import { useState, useEffect } from "react";
import {
    getEquipos,
    getSolicitudes,
    crearSolicitud,
    getPropuestaOptima,
    getRoles,
    getSolicitudDetalle,
    actualizarEstadoSolicitud,
    crearEquipo,
    getTiposEquipos,
} from "./api";
import "./App.css";

function App() {
    const [tab, setTab] = useState("equipos");

    return (
        <div className="app">
            <header className="app-header">
                <h1>Inventario y Solicitudes TI</h1>
                <nav className="nav-tabs">
                    <button
                        className={tab === "equipos" ? "active" : ""}
                        onClick={() => setTab("equipos")}
                    >
                        Inventario
                    </button>
                    <button
                        className={tab === "solicitudes" ? "active" : ""}
                        onClick={() => setTab("solicitudes")}
                    >
                        Solicitudes
                    </button>
                </nav>
            </header>

            <main className="app-main">
                {tab === "equipos" && <EquiposView />}
                {tab === "solicitudes" && <SolicitudesView />}
            </main>
        </div>
    );
}

/* ======================== INVENTARIO ======================== */

function EquiposView() {
    const [equipos, setEquipos] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const [filtroEstado, setFiltroEstado] = useState("");
    const [filtroTipo, setFiltroTipo] = useState("");

    const [mensaje, setMensaje] = useState("");

    const [modalNuevoAbierto, setModalNuevoAbierto] = useState(false);
    const [tipoEquipoNuevo, setTipoEquipoNuevo] = useState("");
    const [tiposEquipos, setTiposEquipos] = useState([]);
    const [modeloNuevo, setModeloNuevo] = useState("");
    const [serieNueva, setSerieNueva] = useState("");
    const [costoNuevo, setCostoNuevo] = useState("");
    const [especificacionesNuevas, setEspecificacionesNuevas] = useState("");
    const [errorModal, setErrorModal] = useState("");
    const [guardando, setGuardando] = useState(false);

    const cargarEquipos = () => {
        setLoading(true);
        getEquipos()
            .then(setEquipos)
            .catch((err) => setError(err.message || "Error al cargar equipos"))
            .finally(() => setLoading(false));
    };

    useEffect(() => {
        cargarEquipos();
    }, []);

    useEffect(() => {
        getTiposEquipos()
            .then(setTiposEquipos)
            .catch((err) => console.error("Error cargando tipos de equipo", err));
    }, []);


    const tiposUnicos = Array.from(new Set(equipos.map((e) => e.tipoEquipo)));

    const equiposFiltrados = equipos.filter((e) => {
        const coincideEstado =
            !filtroEstado || e.estado?.toLowerCase() === filtroEstado.toLowerCase();
        const coincideTipo =
            !filtroTipo || e.tipoEquipo?.toLowerCase() === filtroTipo.toLowerCase();
        return coincideEstado && coincideTipo;
    });

    const abrirModalNuevo = () => {
        setErrorModal("");
        setMensaje("");
        setTipoEquipoNuevo("");
        setModeloNuevo("");
        setSerieNueva("");
        setCostoNuevo("");
        setEspecificacionesNuevas("");
        setModalNuevoAbierto(true);
    };

    const cerrarModalNuevo = () => {
        if (guardando) return;
        setModalNuevoAbierto(false);
    };

    const handleCrearEquipo = async (e) => {
        e.preventDefault();
        setErrorModal("");
        setGuardando(false);

        const costoNum = parseFloat(costoNuevo);

        if (
            !tipoEquipoNuevo.trim() ||
            !modeloNuevo.trim() ||
            !serieNueva.trim() ||
            isNaN(costoNum) ||
            costoNum <= 0
        ) {
            setErrorModal(
                "Completa tipo, modelo, número de serie y un costo mayor a cero."
            );
            return;
        }

        const payload = {
            tipoEquipo: tipoEquipoNuevo.trim(),
            modelo: modeloNuevo.trim(),
            numeroSerie: serieNueva.trim(),
            costo: costoNum,
            especificaciones:
                especificacionesNuevas.trim() === ""
                    ? null
                    : especificacionesNuevas.trim(),
        };

        try {
            setGuardando(true);
            await crearEquipo(payload);
            setMensaje("Equipo registrado correctamente.");
            setModalNuevoAbierto(false);
            cargarEquipos();
        } catch (err) {
            setErrorModal(err.message || "Error al registrar equipo.");
        } finally {
            setGuardando(false);
        }
    };

    return (
        <>
            <section>
                <h2>Inventario de equipos</h2>

                {/* Botón para abrir modal */}
                <div style={{ textAlign: "right", marginBottom: "0.5rem" }}>
                    <button className="agregar-equipo" type="button" onClick={abrirModalNuevo}>
                        Agregar nuevo equipo 
                    </button>
                </div>

                {/* Mensaje general */}
                {mensaje && <p className="ok">{mensaje}</p>}
                {error && <p className="error">{error}</p>}

                {/* Filtros */}
                <div
                    style={{
                        display: "flex",
                        gap: "1rem",
                        flexWrap: "wrap",
                        justifyContent: "center",
                        margin: "1rem 0",
                    }}
                >
                    <div>
                        <label>
                            Estado:&nbsp;
                            <select
                                value={filtroEstado}
                                onChange={(e) => setFiltroEstado(e.target.value)}
                            >
                                <option value="">Todos</option>
                                <option value="disponible">Disponible</option>
                                <option value="asignado">Asignado</option>
                                <option value="baja">Baja</option>
                            </select>
                        </label>
                    </div>
                    <div>
                        <label>
                            Tipo de equipo:&nbsp;
                            <select
                                value={filtroTipo}
                                onChange={(e) => setFiltroTipo(e.target.value)}
                            >
                                <option value="">Todos</option>
                                {tiposUnicos.map((tipo) => (
                                    <option key={tipo} value={tipo}>
                                        {tipo}
                                    </option>
                                ))}
                            </select>
                        </label>
                    </div>
                </div>

                {loading && <p>Cargando...</p>}

                {!loading && !error && (
                    <table className="tabla">
                        <thead>
                            <tr>
                                <th>Id</th>
                                <th>Tipo</th>
                                <th>Modelo</th>
                                <th>No. Serie</th>
                                <th>Estado</th>
                                <th>Asignado a</th>
                                <th>Costo</th>
                            </tr>
                        </thead>
                        <tbody>
                            {equiposFiltrados.map((e) => (
                                <tr key={e.id}>
                                    <td>{e.id}</td>
                                    <td>{e.tipoEquipo}</td>
                                    <td>{e.modelo}</td>
                                    <td>{e.numeroSerie}</td>
                                    <td>{e.estado}</td>
                                    <td>
                                        {e.empleadoAsignado
                                            ? `${e.empleadoAsignado} — ${e.rolEmpleado}`
                                            : "—"}
                                    </td>
                                    <td>${e.costo?.toLocaleString("es-MX")}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </section>

            {/* Modal de nuevo equipo */}
            {modalNuevoAbierto && (
                <div className="modal-backdrop">
                    <div className="modal-content">
                        <h3>Nuevo equipo</h3>

                        <form className="form" onSubmit={handleCrearEquipo}>
                            <label>
                                Tipo de equipo:
                                <select
                                    value={tipoEquipoNuevo}
                                    onChange={(e) => setTipoEquipoNuevo(e.target.value)}
                                >
                                    <option value="">Seleccione un tipo</option>
                                    {tiposEquipos.map((tipo) => (
                                        <option key={tipo} value={tipo}>
                                            {tipo}
                                        </option>
                                    ))}
                                </select>
                            </label>

                            <label>
                                Modelo:
                                <input
                                    type="text"
                                    value={modeloNuevo}
                                    onChange={(e) => setModeloNuevo(e.target.value)}
                                    placeholder="Ej. Dell XPS 13"
                                />
                            </label>

                            <label>
                                Número de serie:
                                <input
                                    type="text"
                                    value={serieNueva}
                                    onChange={(e) => setSerieNueva(e.target.value)}
                                />
                            </label>

                            <label>
                                Costo:
                                <input
                                    type="number"
                                    min="0"
                                    step="0.01"
                                    value={costoNuevo}
                                    onChange={(e) => setCostoNuevo(e.target.value)}
                                />
                            </label>

                            <label>
                                Especificaciones (opcional):
                                <input
                                    type="text"
                                    value={especificacionesNuevas}
                                    onChange={(e) =>
                                        setEspecificacionesNuevas(e.target.value)
                                    }
                                    placeholder='Puedes colocar texto o JSON, ej: {"RAM":"16GB"}'
                                />
                            </label>

                            {errorModal && <p className="error">{errorModal}</p>}

                            <div className="modal-actions">
                                <button type="submit" disabled={guardando}>
                                    {guardando ? "Guardando..." : "Guardar"}
                                </button>
                                <button
                                    type="button"
                                    onClick={cerrarModalNuevo}
                                    disabled={guardando}
                                >
                                    Cancelar
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </>
    );
}



/* ======================== SOLICITUDES ======================== */


function SolicitudesView() {
    const [solicitudes, setSolicitudes] = useState([]);
    const [roles, setRoles] = useState([]);

    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const [nombre, setNombre] = useState("");

    const [lineasRol, setLineasRol] = useState([
        { id: Date.now(), rolId: "", cantidad: 1 },
    ]);

    const [mensaje, setMensaje] = useState("");

    const [modalAbierto, setModalAbierto] = useState(false);
    const [detalleSolicitud, setDetalleSolicitud] = useState(null);
    const [propuesta, setPropuesta] = useState(null);
    const [modalError, setModalError] = useState("");
    const [modalCargando, setModalCargando] = useState(false);

    const cargarSolicitudes = () => {
        setLoading(true);
        getSolicitudes()
            .then(setSolicitudes)
            .catch((err) => setError(err.message || "Error al cargar solicitudes"))
            .finally(() => setLoading(false));
    };

    const cargarRoles = () => {
        getRoles()
            .then(setRoles)
            .catch(() => {

            });
    };

    useEffect(() => {
        cargarSolicitudes();
        cargarRoles();
    }, []);

    const agregarLineaRol = () => {
        setLineasRol((prev) => [
            ...prev,
            { id: Date.now() + Math.random(), rolId: "", cantidad: 1 },
        ]);
    };

    const eliminarLineaRol = (id) => {
        setLineasRol((prev) => {
            if (prev.length === 1) {
                return [{ ...prev[0], rolId: "", cantidad: 1 }];
            }
            return prev.filter((l) => l.id !== id);
        });
    };

    const actualizarLineaRol = (id, campo, valor) => {
        setLineasRol((prev) =>
            prev.map((l) =>
                l.id === id
                    ? { ...l, [campo]: campo === "cantidad" ? Number(valor) : valor }
                    : l
            )
        );
    };

    const handleCrear = async (e) => {
        e.preventDefault();
        setMensaje("");
        setError("");

        const nombreTrim = nombre.trim();
        if (!nombreTrim) {
            setError("Captura un nombre para la solicitud.");
            return;
        }

        const lineasValidas = lineasRol.filter(
            (l) => l.rolId && Number(l.cantidad) > 0
        );

        if (lineasValidas.length === 0) {
            setError("Agrega al menos un rol con una cantidad válida.");
            return;
        }

        const rolesSolicitados = lineasValidas.map((l) => ({
            rolId: parseInt(l.rolId, 10),
            cantidad: parseInt(l.cantidad, 10),
        }));

        const payload = {
            nombreSolicitud: nombreTrim,
            rolesSolicitados,
        };

        try {
            await crearSolicitud(payload);
            setMensaje("Solicitud creada correctamente.");
            setNombre("");
            setLineasRol([{ id: Date.now(), rolId: "", cantidad: 1 }]);
            cargarSolicitudes();
        } catch (err) {
            setError(err.message || "Error al crear solicitud.");
        }
    };

    const abrirDetalleSolicitud = async (solicitud) => {
        setModalAbierto(true);
        setModalError("");
        setDetalleSolicitud(null);
        setPropuesta(null);
        setModalCargando(true);

        try {
            const detalle = await getSolicitudDetalle(solicitud.id);
            setDetalleSolicitud(detalle);

            if (detalle.estado?.toLowerCase() === "pendiente") {
                const prop = await getPropuestaOptima(solicitud.id);
                setPropuesta(prop);
            } else {
                setPropuesta(null);
            }
        } catch (err) {
            setModalError(err.message || "Error al obtener detalle/propuesta.");
        } finally {
            setModalCargando(false);
        }
    };



    const cerrarModal = () => {
        setModalAbierto(false);
        setDetalleSolicitud(null);
        setPropuesta(null);
        setModalError("");
    };

    const marcarComoResuelta = async () => {
        if (!detalleSolicitud) return;

        try {
            await actualizarEstadoSolicitud(detalleSolicitud.id, "resuelta");
            cerrarModal();
            cargarSolicitudes();
        } catch (err) {
            setModalError(err.message || "Error al actualizar el estado.");
        }
    };

    return (
        <>
            <section>
                <h2>Solicitudes de equipamiento</h2>

                <form className="form" onSubmit={handleCrear}>
                    <h3 style={{ textAlign: "center", marginTop: 0 }}>
                        Nueva solicitud
                    </h3>

                    <label>
                        Nombre de la solicitud:
                        <input
                            type="text"
                            value={nombre}
                            onChange={(e) => setNombre(e.target.value)}
                            placeholder="Ej. Equipo para proyecto X"
                        />
                    </label>

                    <div className="roles-dinamicos">
                        <p style={{ fontWeight: "bold", marginBottom: "0.5rem" }}>
                            Roles requeridos
                        </p>

                        <table className="tabla">
                            <thead>
                                <tr>
                                    <th>Rol</th>
                                    <th>Cantidad</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                {lineasRol.map((linea) => (
                                    <tr key={linea.id}>
                                        <td>
                                            <select
                                                value={linea.rolId}
                                                onChange={(e) =>
                                                    actualizarLineaRol(
                                                        linea.id,
                                                        "rolId",
                                                        e.target.value
                                                    )
                                                }
                                            >
                                                <option value="">
                                                    Selecciona un rol
                                                </option>
                                                {roles.map((r) => (
                                                    <option
                                                        key={r.id}
                                                        value={r.id}
                                                    >
                                                        {r.nombreRol}
                                                    </option>
                                                ))}
                                            </select>
                                        </td>
                                        <td>
                                            <input
                                                type="number"
                                                min="1"
                                                value={linea.cantidad}
                                                onChange={(e) =>
                                                    actualizarLineaRol(
                                                        linea.id,
                                                        "cantidad",
                                                        e.target.value
                                                    )
                                                }
                                            />
                                        </td>
                                        <td style={{ textAlign: "center" }}>
                                            <button
                                                type="button"
                                                onClick={() =>
                                                    eliminarLineaRol(linea.id)
                                                }
                                                style={{
                                                    border: "none",
                                                    background: "transparent",
                                                    cursor: "pointer",
                                                    fontSize: "1.1rem",
                                                    color: "#dc2626",
                                                }}
                                                title="Eliminar fila"
                                            >
                                                ✕
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>

                        <button
                            type="button"
                            onClick={agregarLineaRol}
                            style={{ marginTop: "0.5rem" }}
                        >
                            + Agregar
                        </button>
                    </div>

                    <button type="submit" style={{ marginTop: "1rem" }}>
                        Crear solicitud
                    </button>
                </form>

                {mensaje && <p className="ok">{mensaje}</p>}
                {error && <p className="error">{error}</p>}

                <h3 style={{ marginTop: "1.5rem", textAlign: "center" }}>
                    Solicitudes registradas
                </h3>
                {loading && <p>Cargando...</p>}

                {!loading && solicitudes.length === 0 && (
                    <p style={{ textAlign: "center" }}>No hay solicitudes.</p>
                )}

                {!loading && solicitudes.length > 0 && (
                    <table className="tabla">
                        <thead>
                            <tr>
                                <th>Id</th>
                                <th>Nombre</th>
                                <th>Fecha</th>
                                <th>Estado</th>
                            </tr>
                        </thead>
                        <tbody>
                            {solicitudes.map((s) => (
                                <tr
                                    key={s.id}
                                    style={{ cursor: "pointer" }}
                                    onClick={() => abrirDetalleSolicitud(s)}
                                >
                                    <td>{s.id}</td>
                                    <td>{s.nombreSolicitud}</td>
                                    <td>{new Date(s.fecha).toLocaleString()}</td>
                                    <td>{s.estado}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </section>

            {modalAbierto && (
                <div className="modal-backdrop">
                    <div className="modal-content">
                        <h3>Detalle de solicitud</h3>

                        {modalCargando && <p>Cargando detalle y propuesta...</p>}
                        {modalError && <p className="error">{modalError}</p>}

                        {detalleSolicitud && (
                            <>
                                <p>
                                    <strong>Id:</strong> {detalleSolicitud.id}
                                </p>
                                <p>
                                    <strong>Nombre:</strong>{" "}
                                    {detalleSolicitud.nombreSolicitud}
                                </p>
                                <p>
                                    <strong>Fecha:</strong>{" "}
                                    {new Date(
                                        detalleSolicitud.fecha
                                    ).toLocaleString()}
                                </p>
                                <p>
                                    <strong>Estado:</strong>{" "}
                                    {detalleSolicitud.estado}
                                </p>

                                <h4>Roles solicitados</h4>
                                <ul>
                                    {detalleSolicitud.roles.map((r) => (
                                        <li key={r.rolId}>
                                            {r.nombreRol} - {r.cantidadPuestos}{" "}
                                            puestos
                                        </li>
                                    ))}
                                </ul>
                            </>
                        )}

                        {detalleSolicitud &&
                            detalleSolicitud.estado === "pendiente" &&
                            propuesta && (
                                <>
                                    <h4>Propuesta óptima</h4>
                                <p>
                                    <strong>Mensaje:</strong> {propuesta.mensaje}
                                </p>
                                <p>
                                    <strong>Costo total estimado:</strong>{" "}
                                    $
                                    {propuesta.costoTotalEstimado?.toLocaleString(
                                        "es-MX"
                                    )}
                                </p>

                                <h5>Asignaciones por rol</h5>
                                {propuesta.asignaciones.length === 0 && (
                                    <p>No hay asignaciones.</p>
                                )}
                                {propuesta.asignaciones.map((a, idx) => (
                                    <div key={idx} className="card">
                                        <p>
                                            <strong>Rol:</strong> {a.rol} (puesto{" "}
                                            {a.puestoNumero})
                                        </p>
                                        {a.equipos.length === 0 ? (
                                            <p>Sin equipos asignados.</p>
                                        ) : (
                                            <ul>
                                                {a.equipos.map((eq) => (
                                                    <li key={eq.equipoId}>
                                                        #{eq.equipoId} -{" "}
                                                        {eq.tipoEquipo} {eq.modelo} ($
                                                        {eq.costo})
                                                    </li>
                                                ))}
                                            </ul>
                                        )}
                                    </div>
                                ))}

                                <h5>Faltantes</h5>
                                {propuesta.faltantes.length === 0 && (
                                    <p>No hay faltantes.</p>
                                )}
                                {propuesta.faltantes.length > 0 && (
                                    <ul className="error">
                                        {propuesta.faltantes.map((f, idx) => (
                                            <li key={idx}>
                                                Rol {f.rol} - {f.tipoEquipo}: faltan{" "}
                                                {f.cantidadFaltante}
                                            </li>
                                        ))}
                                    </ul>
                                )}
                            </>
                        )}

                        <div className="modal-actions">
                            {detalleSolicitud && detalleSolicitud.estado === "pendiente" && (
                                <>
                                    <button
                                        type="button"
                                        onClick={marcarComoResuelta}
                                        style={{
                                            backgroundColor: "#0a7d33",
                                            color: "white",
                                            opacity:
                                                propuesta && propuesta.faltantes && propuesta.faltantes.length > 0
                                                    ? 0.6
                                                    : 1,
                                            cursor:
                                                propuesta && propuesta.faltantes && propuesta.faltantes.length > 0
                                                    ? "not-allowed"
                                                    : "pointer",
                                        }}
                                        disabled={
                                            !!propuesta &&
                                            !!propuesta.faltantes &&
                                            propuesta.faltantes.length > 0
                                        }
                                    >
                                        Marcar como resuelta
                                    </button>

                                    {propuesta &&
                                        propuesta.faltantes &&
                                        propuesta.faltantes.length > 0 && (
                                            <span className="error" style={{ marginLeft: "0.5rem" }}>
                                                No se puede resolver: faltan equipos en la propuesta.
                                            </span>
                                        )}
                                </>
                            )}

                            {detalleSolicitud && detalleSolicitud.estado === "resuelta" && (
                                <p style={{ marginTop: "1rem" }}>
                                    Esta solicitud ya fue resuelta.
                                </p>
                            )}


                            <button type="button" onClick={cerrarModal}>
                                Cerrar
                            </button>
                        </div>

                    </div>
                </div>
            )}
        </>
    );
}


export default App;
