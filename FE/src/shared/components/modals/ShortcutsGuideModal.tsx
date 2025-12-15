import { Modal } from "antd";
import { 
  useAbility, 
  getFilteredShortcutSections 
} from "../../../config";

interface ShortcutsGuideModalProps {
  open: boolean;
  onClose: () => void;
}

export const ShortcutsGuideModal = ({
  open,
  onClose,
}: ShortcutsGuideModalProps) => {
  const ability = useAbility();
  
  // Obtener secciones filtradas por abilities
  const shortcutSections = getFilteredShortcutSections(ability);
  
  // Si no hay shortcuts disponibles, mostrar mensaje
  if (shortcutSections.length === 0) {
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
        width={600}
        centered
      >
        <div className="text-center py-8">
          <p className="text-gray-600">
            No tienes permisos para acceder a los atajos de teclado disponibles.
          </p>
        </div>
      </Modal>
    );
  }

  // Calcular el número de columnas basado en la cantidad de secciones
  const gridCols = shortcutSections.length <= 3 
    ? `grid-cols-${shortcutSections.length}` 
    : "grid-cols-3";

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
      <div className={`grid ${gridCols} gap-6`}>
        {shortcutSections.map((section, index) => (
          <div key={index} className="border rounded-lg p-4 bg-gray-50">
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
        ))}
      </div>
    </Modal>
  );
};