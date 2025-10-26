import { Link } from "react-router-dom";
import { useState } from "react";
import EditLocation from "../components/ui/EditFormulario";

 export default function EditLocations() {
   const [activeButton, setActiveButton] = useState<"listar" | "crear" | "editar" | null>("editar");
   
     // Función para manejar el clic en un botón
     const handleButtonClick = (button: "listar" | "crear" | "editar") => {
       setActiveButton(button);
      };
  return (
    <div className="min-h-screen bg-white">
      <main className="p-6"> 
       {/* Header */}
        <div className="relative mb-6">
          <div className="flex justify-between items-center relative z-10">
            <h1 className="text-3xl font-bold text-[#333333]">Gestión de Ubicaciones</h1>

            {/* Opciones */}
            <div className="flex items-center gap-3 relative">
              <Link
                to="/locations/list"
                onClick={() => handleButtonClick("listar")}
                className={`px-4 py-2 transition-colors z-10 cursor-pointer ${
                  activeButton === "listar"
                    ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                    : "text-[#163C65]"
                }`}
              >
                Listar Ubicaciones
              </Link>

              <Link
                to="/locations/create"
                onClick={() => handleButtonClick("crear")}
                className={`px-4 py-2 relative z-10 cursor-pointer ${
                  activeButton === "crear"
                    ? "border border-gray-300 bg-transparent text-[#7BA2D4] rounded-t-md"
                    : "text-[#163C65]"
                }`}
              >
                Crear Ubicación
              </Link>

              <div className="absolute bottom-0 h-[1px] bg-gray-300 z-0 left-0 right-0"></div>
            </div>
          </div>
        </div>
        {/* Formulario de gestión de ubicaciones */}
        <EditLocation />
      </main>
    </div>
      
  )
}

