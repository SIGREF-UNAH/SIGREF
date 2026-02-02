import { useCreateSerie } from "../hooks";
import { PageContainer } from "@ant-design/pro-components";
import { Card, Button } from "antd";
import { ArrowLeftOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router";
import { SeriesForm } from "../components";

export const SeriesCreatePage = () => {
  const { handleFinish, isPending } = useCreateSerie();
  const navigate = useNavigate();

  return (
    <PageContainer
      header={{
        title: "Crear Nueva Serie",
        subTitle: "Complete el formulario para crear una nueva serie",
      }}
      extra={[
        <Button
          key="back"
          icon={<ArrowLeftOutlined />}
          onClick={() => navigate("/series")}
        >
          Volver al listado
        </Button>,
      ]}
    >
      <Card>
        <SeriesForm mode="create" onFinish={handleFinish} loading={isPending} />
      </Card>
    </PageContainer>
  );
};