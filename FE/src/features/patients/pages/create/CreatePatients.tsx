import { useState } from "react";
import CreateFormPatient from "../../components/ui/FormPatients";
import { Link } from "react-router-dom";

const CreatePatients = () => {
  const [activeButton, setActiveButton] = useState<"listar" | "crear" | null>(
    "crear"
  );

  // Función para manejar el clic en un botón
  const handleButtonClick = (button: "listar" | "crear") => {
    setActiveButton(button);
  };
  return (
    <div className=" min-h-screen bg-white">
      <main className="flex flex-col gap-6 p-6">
        {/* Header con título a la izquierda y menú a la derecha */}
        <div className="flex justify-between items-center">
          {/* Título */}
          <h1 className="text-3xl font-bold text-gray-900">
            Gestión de Pacientes
          </h1>

          {/* Menú de opciones */}
          <div className="flex items-center gap-3">
            <Link
              to="/patients/list"
              onClick={() => handleButtonClick("listar")}
              className={`px-4 py-2 transition-colors cursor-pointer ${
                activeButton === "listar"
                  ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                  : "text-[#163C65]"
              }`}
            >
              Listar Pacientes
            </Link>

            <Link
              to="/patients/create"
              onClick={() => handleButtonClick("crear")}
              className={`px-4 py-2 transition-colors cursor-pointer ${
                activeButton === "crear"
                  ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                  : "text-[#163C65]"
              }`}
            >
              Crear Paciente
            </Link>
          </div>
        </div>

        {/* Formulario */}
        <div className="w-full">
          <CreateFormPatient />
        </div>
      </main>
    </div>
  );
};

export default CreatePatients;
