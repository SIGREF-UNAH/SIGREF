import { Modal } from "antd";
import { ProtectedComponent } from "../ProtectedComponent";

interface ShortcutsGuideModalProps {
  open: boolean;
  onClose: () => void;
}

export const ShortcutsGuideModal = ({
  open,
  onClose,
}: ShortcutsGuideModalProps) => {
  const shortcutSections = [
    {
      title: "Gestión de Fondos",
      roles: ["admin", "cashier", "auditor"],
      shortcuts: [
        { keys: "Ctrl + F", description: "Control de fondos" },
        { keys: "Ctrl + FI", description: "Generar ingreso" },
        { keys: "Ctrl + FC", description: "Cierre de caja" },
        { keys: "Ctrl + FH", description: "Historial de cierres" },
      ],
    },
    {
      title: "Gestión de Servicios",
      roles: ["admin", "cashier", "auditor"],
      shortcuts: [
        { keys: "Ctrl + S", description: "Listar servicios" },
        { keys: "Ctrl + SC", description: "Crear servicio" },
      ],
    },
    {
      title: "Gestión de Organizaciones",
      roles: ["admin", "ti"],
      shortcuts: [
        { keys: "Ctrl + O", description: "Listar organizaciones" },
        { keys: "Ctrl + OC", description: "Crear organización" },
      ],
    },
    {
      title: "Gestión de Ubicaciones",
      roles: ["admin", "ti"],
      shortcuts: [
        { keys: "Ctrl + U", description: "Listar ubicaciones" },
        { keys: "Ctrl + UC", description: "Crear ubicación" },
      ],
    },
    {
      title: "Gestión de Reportes",
      roles: ["admin"],
      shortcuts: [
        { keys: "Ctrl + R", description: "Control de reportes" },
        { keys: "Ctrl + RC", description: "Generar reporte" },
        { keys: "Ctrl + RH", description: "Historial de reportes" },
      ],
    },
    {
      title: "Gestión de Empleados",
      roles: ["admin", "ti", "auditor"],
      shortcuts: [
        { keys: "Ctrl + E", description: "Listar empleados" },
        { keys: "Ctrl + EC", description: "Crear empleado" },
      ],
    },
    {
      title: "Gestión de Pacientes",
      roles: ["admin", "cashier"],
      shortcuts: [
        { keys: "Ctrl + P", description: "Listar pacientes" },
        { keys: "Ctrl + PC", description: "Crear paciente" },
      ],
    },
    {
      title: "Gestión de Eventos/Logs",
      roles: ["admin", "ti", "auditor"],
      shortcuts: [
        { keys: "Ctrl + L", description: "Control de eventos/logs" },
      ],
    },
  ];

  return (
    <Modal
      title={
        <div className="text-xl text-center font-semibold text-general">
          Guía de Atajos del Teclado
        </div>
      }
      open={open}
      onCancel={onClose}
      footer={null}
      width={1100}
      centered
    >
      <div className="grid grid-cols-3 gap-6 p-4">
        {shortcutSections.map((section, index) => (
          <ProtectedComponent key={index} allowedRoles={section.roles}>
            <div className="border rounded-lg p-4 bg-gray-50">
              <h3 className="font-semibold text-base mb-3 text-general border-b pb-2">
                {section.title}
              </h3>
              <div className="space-y-2">
                {section.shortcuts.map((shortcut, idx) => (
                  <div key={idx} className="flex justify-between items-center">
                    <span className="text-sm text-gray-700">
                      {shortcut.description}
                    </span>
                    <kbd className="px-2 py-1 text-xs font-semibold text-gray-800 bg-white border border-gray-300 rounded shadow-sm">
                      {shortcut.keys}
                    </kbd>
                  </div>
                ))}
              </div>
            </div>
          </ProtectedComponent>
        ))}
      </div>
    </Modal>
  );
};