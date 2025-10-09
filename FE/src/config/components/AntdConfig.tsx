import { ConfigProvider } from "antd";
import esES from "antd/es/locale/es_ES"; // idioma español
import type { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

export const AntdConfig = ({ children }: Props) => {
  return (
    <ConfigProvider
      locale={esES}
      theme={{
        token: {
          fontFamily: "'Inter', sans-serif",
        },
      }}
    >
      {/* TODO: Implementar ProProvider */}
      {children}
    </ConfigProvider>
  );
};
