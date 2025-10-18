import { Modal } from "antd";
import { ProtectedComponent } from "../ProtectedComponent";
import { appShortcuts, type ShortcutSection } from "../../../config";

interface ShortcutsGuideModalProps {
  open: boolean;
  onClose: () => void;
}

export const ShortcutsGuideModal = ({
  open,
  onClose,
}: ShortcutsGuideModalProps) => {
  
  const shortcutSections: ShortcutSection[] = appShortcuts.reduce((sections, shortcut) => {
    const existingSection = sections.find(section => section.title === shortcut.category);
    
    const shortcutItem = {
      keys: shortcut.keys.split(', ')[0], // Tomar la primera combinación de teclas
      description: shortcut.description
    };

    if (existingSection) {
      existingSection.shortcuts.push(shortcutItem);
    } else {
      sections.push({
        title: shortcut.category,
        roles: shortcut.roles,
        shortcuts: [shortcutItem]
      });
    }
    
    return sections;
  }, [] as ShortcutSection[]);

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