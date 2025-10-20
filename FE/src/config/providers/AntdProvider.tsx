import { ProConfigProvider } from "@ant-design/pro-components";
import { App as AntApp, ConfigProvider } from "antd";
import esES from "antd/es/locale/es_ES"; // idioma español
import type { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

export const AntdProvider = ({ children }: Props) => {
  return (
    <ConfigProvider
      locale={esES}
      theme={{
        token: {
          fontFamily: "'Inter', sans-serif",
        },
      }}
    >
      <ProConfigProvider dark={false}>
        <AntApp>{children}</AntApp>
      </ProConfigProvider>
    </ConfigProvider>
  );
};
