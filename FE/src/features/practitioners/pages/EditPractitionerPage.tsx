import EditPractitionerForm from "../components/ui/EditPractitionerForm";

export const EditPractitionerPage = () => {

  return (
    <div className="min-h-screen bg-white">
        <main className="p-6">
            {/* Header */}
        <div className="relative mb-6">
          <div className="flex justify-between items-center relative z-10">
            <h1 className="text-3xl font-bold text-general">
              Gestión de Empleados
            </h1>

            {/* Opciones */}
            <div className="flex items-center gap-3 relative">
              <div className="absolute bottom-0 h-px bg-gray-300 z-0 left-0 right-0"></div>
            </div>
          </div>
        </div>

        {/* Edit Employee Form */}
        <EditPractitionerForm />

        </main>
      
    </div>
  )
}
