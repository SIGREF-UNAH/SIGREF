import { BellOutlined, CheckCircleOutlined, CloseCircleOutlined, InfoCircleOutlined, WarningOutlined } from "@ant-design/icons";
import { App } from "antd";
import type { ReactNode } from "react";

type MessageType = "success" | "error" | "warning" | "info" | "loading";

interface MessageOptions {
  content: string | ReactNode;
  duration?: number;
  icon?: ReactNode;
  key?: string;
  style?: React.CSSProperties;
  className?: string;
  onClose?: () => void;
}

interface NotificationOptions {
  title: string;
  description?: string | ReactNode;
  duration?: number;
  icon?: ReactNode;
  placement?: "topLeft" | "topRight" | "bottomLeft" | "bottomRight";
  onClick?: () => void;
  onClose?: () => void;
  btn?: ReactNode;
  key?: string;
  className?: string;
  style?: React.CSSProperties;
}

export const useMessage = () => {
  const { message, notification } = App.useApp();

  // Función generalizada para mensajes tipo toast
  const showMessage = (type: MessageType, options: MessageOptions) => {
    const { content, duration = 3, icon, key, style, className, onClose } = options;
    message.open({
      type,
      content,
      duration,
      icon,
      key,
      style,
      className,
      onClose,
    });
  };

  // Función generalizada para notificaciones
  const showNotification = (options: NotificationOptions) => {
    const {
      title,
      description,
      duration = 4.5,
      icon = <BellOutlined />,
      placement = "topRight",
      onClick,
      onClose,
      btn,
      key,
      className,
      style,
    } = options;

    notification.open({
      message: title,
      description,
      duration,
      icon,
      placement,
      onClick,
      onClose,
      btn,
      key,
      className,
      style,
    });
  };

  // Métodos rápidos con estilos predefinidos
  return {
    success: (content: string | ReactNode, duration?: number, options?: Partial<MessageOptions>) =>
      showMessage("success", {
        content,
        duration,
        icon: <CheckCircleOutlined style={{ color: "#52c41a" }} />,
        ...options,
      }),

    error: (content: string | ReactNode, duration?: number, options?: Partial<MessageOptions>) =>
      showMessage("error", {
        content,
        duration,
        icon: <CloseCircleOutlined style={{ color: "#ff4d4f" }} />,
        ...options,
      }),

    warning: (content: string | ReactNode, duration?: number, options?: Partial<MessageOptions>) =>
      showMessage("warning", {
        content,
        duration,
        icon: <WarningOutlined style={{ color: "#faad14" }} />,
        ...options,
      }),

    info: (content: string | ReactNode, duration?: number, options?: Partial<MessageOptions>) =>
      showMessage("info", {
        content,
        duration,
        icon: <InfoCircleOutlined style={{ color: "#1890ff" }} />,
        ...options,
      }),

    loading: (content: string | ReactNode, duration?: number, options?: Partial<MessageOptions>) =>
      showMessage("loading", {
        content,
        duration,
        ...options,
      }),

    // Notificaciones con tipado
    notifySuccess: (title: string, description?: string | ReactNode, options?: Partial<NotificationOptions>) =>
      showNotification({
        title,
        description,
        icon: <CheckCircleOutlined style={{ color: "#52c41a" }} />,
        ...options,
      }),

    notifyError: (title: string, description?: string | ReactNode, options?: Partial<NotificationOptions>) =>
      showNotification({
        title,
        description,
        icon: <CloseCircleOutlined style={{ color: "#ff4d4f" }} />,
        ...options,
      }),

    notifyWarning: (title: string, description?: string | ReactNode, options?: Partial<NotificationOptions>) =>
      showNotification({
        title,
        description,
        icon: <WarningOutlined style={{ color: "#faad14" }} />,
        ...options,
      }),

    notifyInfo: (title: string, description?: string | ReactNode, options?: Partial<NotificationOptions>) =>
      showNotification({
        title,
        description,
        icon: <InfoCircleOutlined style={{ color: "#1890ff" }} />,
        ...options,
      }),

    notify: showNotification,

    // Métodos de utilidad
    destroy: (key?: string) => {
      message.destroy(key);
      notification.destroy(key);
    },
    
    destroyAll: () => {
      message.destroy();
      notification.destroy();
    },

    // Acceso directo a las APIs de Ant Design
    raw: { message, notification },
  };
};