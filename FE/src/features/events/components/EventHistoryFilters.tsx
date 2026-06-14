import { Button, Col, DatePicker, Form, Input, Row, Select, Space } from "antd";
import { SearchOutlined } from "@ant-design/icons";
import type { FormInstance } from "antd";
import type { Dayjs } from "dayjs";
import type { Dispatch, SetStateAction } from "react"; 
import type { EventHistoryFormValues } from "../types"; 
import { HTTP_METHODS, ACTIONS } from "../constants";

const { RangePicker } = DatePicker;
const { Option } = Select;

interface Props {
  form: FormInstance;
  formValues: EventHistoryFormValues;
  setFormValues: Dispatch<SetStateAction<EventHistoryFormValues>>; 
  onSearch: () => void;
  onClear: () => void;
}

export const EventHistoryFilters = ({ form, formValues, setFormValues, onSearch, onClear }: Props) => {
  
  const update = (patch: Partial<EventHistoryFormValues>) =>
    setFormValues((prev) => ({ ...prev, ...patch }));

  return (
    <Form layout="vertical" form={form}>
      <Row gutter={16}>
        <Col xs={24} sm={12} md={6} lg={5}>
          <Form.Item label="Nombre de Usuario">
            <Input
              placeholder="Buscar por usuario..."
              value={formValues.userName}
              onChange={(e) => update({ userName: e.target.value })}
              onPressEnter={onSearch}
              allowClear
            />
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={5}>
          <Form.Item label="User ID">
            <Input
              placeholder="Buscar por ID..."
              value={formValues.userId}
              onChange={(e) => update({ userId: e.target.value })}
              onPressEnter={onSearch}
              allowClear
            />
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={4}>
          <Form.Item label="Acción">
            <Select
              placeholder="Seleccionar tipo"
              value={formValues.action}
              onChange={(value) => update({ action: value })}
              allowClear
            >
              {ACTIONS.map((a) => (
                <Option key={a} value={a}>
                  {a.charAt(0).toUpperCase() + a.slice(1)}
                </Option>
              ))}
            </Select>
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={4}>
          <Form.Item label="HTTP Method">
            <Select
              placeholder="Método HTTP"
              value={formValues.httpMethod}
              onChange={(value) => update({ httpMethod: value })}
              allowClear
            >
              {HTTP_METHODS.map((m) => (
                <Option key={m} value={m}>
                  {m}
                </Option>
              ))}
            </Select>
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={8} lg={6}>
          <Form.Item label="Rango de Fechas">
            <RangePicker
              showTime
              format="YYYY-MM-DD HH:mm:ss"
              value={formValues.dateRange}
              onChange={(dates) => update({ dateRange: dates ? (dates as [Dayjs, Dayjs]) : null })}
              style={{ width: "100%" }}
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} sm={12} md={6} lg={5}>
          <Form.Item label="Tipo de Recurso">
            <Input
              placeholder="Ej: Patient, Observation..."
              value={formValues.resourceType}
              onChange={(e) => update({ resourceType: e.target.value })}
              onPressEnter={onSearch}
              allowClear
            />
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={5}>
          <Form.Item label="Trace ID">
            <Input
              placeholder="Trace ID..."
              value={formValues.traceId}
              onChange={(e) => update({ traceId: e.target.value })}
              onPressEnter={onSearch}
              allowClear
            />
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={4}>
          <Form.Item label="Dirección IP">
            <Input
              placeholder="IP..."
              value={formValues.ipAddress}
              onChange={(e) => update({ ipAddress: e.target.value })}
              onPressEnter={onSearch}
              allowClear
            />
          </Form.Item>
        </Col>
        <Col xs={24} sm={12} md={6} lg={4}>
          <Form.Item label="Resultado">
            <Select
              placeholder="Éxito/Fallo"
              value={formValues.success}
              onChange={(value) => update({ success: value })}
              allowClear
            >
              <Option value={true}>Exitoso</Option>
              <Option value={false}>Fallido</Option>
            </Select>
          </Form.Item>
        </Col>
        <Col
          xs={24} sm={24} md={8} lg={6}
          style={{ display: "flex", alignItems: "flex-end", justifyContent: "flex-start" }}
        >
          <Form.Item label=" " colon={false}>
            <Space>
              <Button type="primary" icon={<SearchOutlined />} onClick={onSearch}>
                Buscar
              </Button>
              <Button onClick={onClear}>Limpiar</Button>
            </Space>
          </Form.Item>
        </Col>
      </Row>
    </Form>
  );
};