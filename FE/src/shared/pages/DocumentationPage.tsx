import {
  FileTextOutlined,
  FilePdfOutlined,
  FileWordOutlined,
  FileExcelOutlined,
  DownloadOutlined,
  EyeOutlined,
} from "@ant-design/icons";
import { Card, Typography, Space } from "antd";
import { documents } from "../store";

const { Title, Text, Paragraph } = Typography;

export const DocumentationPage = () => {
  // Obtener icono del documento
  const getIcon = (type: string) => {
    switch (type) {
      case "pdf":
        return <FilePdfOutlined className="text-5xl text-red-500" />;
      case "word":
        return <FileWordOutlined className="text-5xl text-blue-600" />;
      case "excel":
        return <FileExcelOutlined className="text-5xl text-green-600" />;
      default:
        return <FileTextOutlined className="text-5xl text-gray-500" />;
    }
  };

  // Abrir en nueva pestaña
  const handleView = (url: string) => {
    window.open(url, "_blank");
  };

  // Descargar el documento
  const handleDownload = (url: string, title: string) => {
    // Simular descarga
    const link = document.createElement("a");
    link.href = url;
    link.download = title;
    link.click();
  };

  return (
    <div>
      <div className="mb-8 text-center">
        <Title level={2} className="mb-2">
          Documentación del Proyecto
        </Title>
        <Text type="secondary" className="text-base">
          Accede a todos los documentos, informes y manuales relacionados con el
          proyecto
        </Text>
      </div>

      <div className="flex items-center justify-center">
        {documents.map((doc) => (
          <div className="w-80" key={doc.id}>
            <Card
              className="primary-card"
              actions={[
                <div
                  key="view"
                  onClick={() => handleView(doc.url)}
                  className="flex items-center justify-center cursor-pointer hover:text-blue-500 transition-colors"
                >
                  <EyeOutlined className="mr-1" />
                  <span>Ver</span>
                </div>,
                <div
                  key="download"
                  onClick={() => handleDownload(doc.url, doc.title)}
                  className="flex items-center justify-center cursor-pointer hover:text-green-500 transition-colors"
                >
                  <DownloadOutlined className="mr-1" />
                  <span>Descargar</span>
                </div>,
              ]}
            >
              <Space direction="vertical" size="middle" className="w-full">
                <div className="text-center">
                  <div className="mb-4">{getIcon(doc.type)}</div>
                  <Title level={5} className="mb-2 line-clamp-2">
                    {doc.title}
                  </Title>
                </div>

                <Paragraph
                  type="secondary"
                  ellipsis={{ rows: 3 }}
                  className="text-sm mb-4 text-center"
                >
                  {doc.description}
                </Paragraph>

                <div className="flex justify-between items-center text-xs text-gray-500 pt-2 border-t border-gray-200">
                  <span>{doc.size}</span>
                  <span>{doc.date}</span>
                </div>
              </Space>
            </Card>
          </div>
        ))}
      </div>
    </div>
  );
};
