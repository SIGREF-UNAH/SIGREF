import { InboxOutlined } from "@ant-design/icons";

export const EmptyState = ({ 
  title = "No hay datos disponibles",
  description = "No se encontraron registros para el período seleccionado",
  icon: Icon = InboxOutlined,
  iconColor = "#94a3b8"
}) => {
  return (
    <div className="flex flex-col items-center justify-center py-12 px-4">
      <div className="bg-gray-100 p-6 rounded-full mb-4">
        <Icon style={{ fontSize: "48px", color: iconColor }} />
      </div>
      <h3 className="text-lg font-semibold text-gray-700 mb-2">
        {title}
      </h3>
      <p className="text-sm text-gray-500 text-center max-w-md">
        {description}
      </p>
    </div>
  );
};