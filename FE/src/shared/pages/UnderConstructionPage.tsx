import { Result, Button } from "antd";
import { HomeOutlined, LeftOutlined, CodeOutlined } from "@ant-design/icons";
import { useNavigate } from "react-router-dom";

export const UnderConstructionPage = () => {
  const navigate = useNavigate();

  return (
    <div className="flex items-center justify-center h-100">
      <div>
        <Result
          icon={<CodeOutlined className="text-8xl" style={{color: "#163C65"}} />}
          title={
            <span className="text-2xl font-semibold">
              Página en Construcción
            </span>
          }
          subTitle={
            <span className="text-gray-600 text-center">
              Esta página está actualmente en desarrollo. <br/>
              Pronto estará disponible con nuevas funcionalidades.
            </span>
          }
          extra={[
            <Button
              style={{ 
                backgroundColor: '#163C65',
                borderColor: '#163C65',
                color: '#fff'
              }}
              type="primary"
              key="back"
              icon={<LeftOutlined />}
              onClick={() => navigate(-1)}
              size="large"
            ></Button>,
            <Button
              style={{ 
                backgroundColor: '#163C65',
                borderColor: '#163C65',
                color: '#fff'
              }}
              key="home"
              icon={<HomeOutlined />}
              onClick={() => navigate("/")}
              size="large"
              className="mr-2"
            ></Button>,
          ]}
        />
      </div>
    </div>
  );
};
