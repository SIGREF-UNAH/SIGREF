import {
  FileTextOutlined,
  FilePdfOutlined,
  FileWordOutlined,
  FileExcelOutlined,
  DownloadOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import { documents } from "../store";
import { Typography } from "antd";

const { Title, Text } = Typography;

export const DocumentationPage = () => {
  // Obtener icono del documento
  const getIcon = (type : string) => {
    switch (type) {
      case "pdf":
        return <FilePdfOutlined className="text-5xl text-red-500!" />;
      case "word":
        return <FileWordOutlined className="text-5xl text-blue-600!" />;
      case "excel":
        return <FileExcelOutlined className="text-5xl text-green-600!" />;
      default:
        return <FileTextOutlined className="text-5xl text-gray-500!" />;
    }
  };

  // Convertir URL de Google Drive para visualización
  const getViewUrl = (url : string) => {
    const match = url.match(/\/d\/([a-zA-Z0-9-_]+)/);
    if (match) {
      return `https://docs.google.com/document/d/${match[1]}/preview`;
    }
    return url;
  };

  // Convertir URL de Google Drive para descarga
  const getDownloadUrl = (url : string) => {
    const match = url.match(/\/d\/([a-zA-Z0-9-_]+)/);
    if (match) {
      return `https://docs.google.com/document/d/${match[1]}/export?format=docx`;
    }
    return url;
  };

  // Abrir en nueva pestaña
  const handleView = (url : string) => {
    window.open(getViewUrl(url), "_blank", "noopener,noreferrer");
  };

  // Descargar el documento
  const handleDownload = (url : string, title : string) => {
    const downloadUrl = getDownloadUrl(url);
    const link = document.createElement("a");
    link.href = downloadUrl;
    link.download = title;
    link.target = "_blank";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div>
      <div className="mb-8 text-center">
        <Title level={2} className="mb-2">
          Documentación del Sistema
        </Title>
        <Text type="secondary">
          Accede a todos los documentos, informes y manuales relacionados con el
          proyecto
        </Text>
      </div>

      <div className="flex gap-3 items-center justify-center">
        {documents.map((doc) => (
          <div className="w-80" key={doc.id}>
            <div className="primary-card bg-white rounded-lg shadow-md overflow-hidden">
              <div className="p-6">
                <div className="flex flex-col items-center space-y-4 w-full">
                  <div className="text-center">
                    <div className="mb-4">{getIcon(doc.type)}</div>
                    <h5 className="text-lg font-medium mb-2 line-clamp-2">
                      {doc.title}
                    </h5>
                  </div>

                  <p className="text-gray-500 text-sm mb-4 text-center line-clamp-3">
                    {doc.description}
                  </p>

                  <div className="flex justify-between items-center text-xs text-gray-500 pt-2 border-t border-gray-200 w-full">
                    <span>{doc.size}</span>
                    <span>{doc.date}</span>
                  </div>
                </div>
              </div>

              <div className="border-t border-gray-200 flex divide-x divide-gray-200">
                <div
                  onClick={() => handleView(doc.url)}
                  className="flex-1 flex items-center justify-center cursor-pointer hover:text-blue-500 hover:bg-gray-50 transition-colors py-3"
                >
                  <EyeOutlined className="mr-1" />
                  <span>Ver</span>
                </div>
                <div
                  onClick={() => handleDownload(doc.url, doc.title)}
                  className="flex-1 flex items-center justify-center cursor-pointer hover:text-green-500 hover:bg-gray-50 transition-colors py-3"
                >
                  <DownloadOutlined className="mr-1" />
                  <span>Descargar</span>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}