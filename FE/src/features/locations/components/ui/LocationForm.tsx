import useLocationForm from "../../hooks/useLocationForm";
import FormInput from "./FormInput";
import { BiChevronDown } from "react-icons/bi";
import { BsBuilding, BsGeoAltFill, BsPersonFill } from "react-icons/bs";
import { MdOutlineAddLocationAlt } from "react-icons/md";
import { FaCheck } from "react-icons/fa";

export default function LocationForm() {
  const { formData, setField, handleSubmit, isSubmitting } = useLocationForm();

  return (
    <div className="bg-[#FAFAFA] rounded-lg border-2 border-[#D9D9D9] p-6">
      {/* Header */}
      <div className="flex items-center gap-3 mb-8">
        <MdOutlineAddLocationAlt className="w-10 h-10 text-blue-500" />
        <span className="text-xl font-semibold text-[#333333]">Crear Ubicación</span>
      </div>

      <form onSubmit={handleSubmit} className="space-y-8">
        <div className="max-w-[105rem] mx-auto">
          {/* Información Básica */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsBuilding className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">Información Básica</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-4">
              <div>
                <FormInput label="Identificador" placeholder="Ej. Juan" value={formData.identifier} onChange={(v) => setField("identifier", v)} />
              </div>
              <div>
                <FormInput label="Nombre de la ubicación" placeholder="Ej. Sala de emergencias" value={formData.locationName} onChange={(v) => setField("locationName", v)} />
              </div>
              <div>
                <FormInput label="Alias" placeholder="Ej. Emergencias, ER" value={formData.alias} onChange={(v) => setField("alias", v)} />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-4">
              <div>
                <label className="block text-base font-medium text-[#616161] mb-2">Estado</label>
                <div className="relative">
                  <select value={formData.state} onChange={(e) => setField("state", e.target.value)} className="w-full px-3 py-2 bg-[#fff] font-normal border border-gray-200 rounded-md text-[#616161] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none">
                    <option value="">Seleccionar estado</option>
                    <option value="activo">Activo</option>
                    <option value="inactivo">Inactivo</option>
                  </select>
                  <BiChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[#616161] pointer-events-none" />
                </div>
              </div>

              <div>
                <label className="block text-base font-medium text-[#616161] mb-2">Modo</label>
                <div className="relative">
                  <select value={formData.mode} onChange={(e) => setField("mode", e.target.value)} className="w-full px-3 py-2 bg-[#fff] font-normal border border-gray-200 rounded-md text-[#616161] focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent appearance-none">
                    <option value="">Seleccionar modo</option>
                    <option value="operativo">Operativo</option>
                    <option value="mantenimiento">Mantenimiento</option>
                  </select>
                  <BiChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-[#616161] pointer-events-none" />
                </div>
              </div>

              <div>
                <FormInput label="Tipo de función" placeholder="Ej. Cuarto de emergencias" value={formData.functionType} onChange={(v) => setField("functionType", v)} />
              </div>
            </div>

            <div>
              <label className="block text-base font-medium text-[#616161] mb-2">Descripción</label>
              <textarea placeholder="Descripción adicional de la ubicación" value={formData.description} onChange={(e) => setField("description", e.target.value)} rows={2} className="w-full px-3 py-2 bg-[#D9D9D9] font-normal border border-gray-200 rounded-md text-[#616161] placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none" />
            </div>
          </section>

          <hr className="border-[#000] my-8" />

          {/* Dirección física */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsGeoAltFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">Dirección física</span>
            </div>

            <div className="mb-4">
              <FormInput label="Dirección" placeholder="Ej. Avenida principal 123" value={formData.address} onChange={(v) => setField("address", v)} />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32 mb-4">
              <div>
                <FormInput label="Ciudad" placeholder="Ej. Ciudad" value={formData.city} onChange={(v) => setField("city", v)} />
              </div>
              <div>
                <FormInput label="Estado/Provincia" placeholder="Ej. Provincia" value={formData.stateProvince} onChange={(v) => setField("stateProvince", v)} />
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <div>
                <FormInput label="Código postal" placeholder="Ej. 12345" value={formData.postalCode} onChange={(v) => setField("postalCode", v)} />
              </div>
              <div>
                <FormInput label="País" placeholder="Ej. Honduras" value={formData.country} onChange={(v) => setField("country", v)} />
              </div>
            </div>
          </section>

          <hr className="border-[#000] my-8" />

          {/* Información de contacto */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsPersonFill className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">Información de contacto</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <div>
                <FormInput label="Nombre de contacto" placeholder="Ej. Dr. Juan" value={formData.contactName} onChange={(v) => setField("contactName", v)} />
              </div>
              <div>
                <FormInput label="Teléfono" placeholder="Ej. 9878-8967" value={formData.phone} onChange={(v) => setField("phone", v)} />
              </div>
              <div>
                <FormInput label="Correo electrónico" placeholder="Ej. ever@me.gmail" type="email" value={formData.email} onChange={(v) => setField("email", v)} />
              </div>
            </div>
          </section>

          <hr className="border-[#000] my-8" />

          {/* Organización y jerarquía */}
          <section>
            <div className="flex items-center gap-3 mb-6">
              <BsBuilding className="w-8 h-8 text-blue-500" />
              <span className="text-lg font-semibold text-[#333333]">Organización y jerarquía</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-8 md:gap-16 lg:gap-32">
              <div>
                <FormInput label="Organización responsable" placeholder="Ej. Hospital nacional" value={formData.responsibleOrganization} onChange={(v) => setField("responsibleOrganization", v)} />
              </div>
              <div>
                <FormInput label="Parte de (Ubicación padre)" placeholder="Ej. Edificio principal" value={formData.parentLocation} onChange={(v) => setField("parentLocation", v)} />
              </div>
            </div>
          </section>

          {/* Botón de envío */}
          <div className="flex justify-end pt-10 pb-2">
            <button type="submit" disabled={isSubmitting} className="px-6 py-2 bg-green-500 hover:bg-green-600 text-white font-medium rounded-md transition-colors duration-200 flex items-center gap-2 disabled:opacity-60">
              <FaCheck className="w-4 h-4" /> {isSubmitting ? "Creando..." : "Crear Ubicación"}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}