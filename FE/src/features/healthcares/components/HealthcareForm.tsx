import { useState, useEffect } from "react";
import {
  Input,
  InputNumber,
  Button,
  Table,
  Select,
  Switch,
  Tag,
  Form,
} from "antd";
import type { ColumnsType } from "antd/es/table";
import type { FormikProps } from "formik";
import type { HealthcareDto, LocationDto, OrganizationDto } from "../../../api/models";

const { TextArea } = Input;

interface HealthcareFormProps {
  formik: FormikProps<HealthcareDto>;
  organizations: OrganizationDto[];
  locations: LocationDto[];
  submitButtonText?: string;
  isPending?: boolean;
  onCancel: () => void;
}

export const HealthcareForm = ({
  formik,
  organizations,
  locations,
  submitButtonText = "Crear servicio",
  isPending = false,
  onCancel,
}: HealthcareFormProps) => {
  const [searchText, setSearchText] = useState("");
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([]);
  const [selectedLocations, setSelectedLocations] = useState<LocationDto[]>([]);

  // Inicializar ubicaciones seleccionadas desde formik
  useEffect(() => {
    if (formik.values.location && formik.values.location.length > 0) {
      const locationIds = formik.values.location.map((loc) =>
        loc.reference?.split("/")[1]
      );
      const currentIds = selectedRowKeys.join(",");
      const newIds = locationIds.join(",");

      if (currentIds !== newIds) {
        setSelectedRowKeys(locationIds.filter((id): id is string => typeof id === "string"));
        const selectedLocs = locations.filter((loc: LocationDto) =>
          locationIds.includes(loc.id || "")
        );
        setSelectedLocations(selectedLocs);
      }
    }
  }, [formik.values.location, locations]);

  // Filtro de búsqueda
  const filteredLocations = locations.filter((location) =>
    location.name.toLowerCase().includes(searchText.toLowerCase())
  );

  // Columnas de la tabla
  const columns: ColumnsType<LocationDto> = [
    {
      title: "Nombre",
      dataIndex: "name",
      key: "name",
    },
    {
      title: "Estado",
      dataIndex: "status",
      key: "status",
      render: (status: string) => {
        const color = status === "Active" ? "green" : "default";
        const text = status === "Active" ? "Activo" : "Inactivo";
        return <Tag color={color}>{text}</Tag>;
      },
    },
  ];

  // Selección de ubicaciones
  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[], selectedRows: LocationDto[]) => {
      setSelectedRowKeys(newSelectedRowKeys as string[]);
      setSelectedLocations(selectedRows);
      formik.setFieldValue(
        "location",
        selectedRows.map((loc) => ({
          reference: `Location/${loc.id}`,
          display: loc.name,
        }))
      );
    },
  };

  // Selección de organización
  const handleOrganizationChange = (value: string) => {
    const selectedOrg = organizations.find((org) => org.id === value);
    if (selectedOrg) {
      formik.setFieldValue("providedBy", {
        reference: `Organization/${selectedOrg.id}`,
        display: selectedOrg.name,
      });
    }
  };

  // Extraer el ID de la organización para el Select
  const organizationId = formik.values.providedBy?.reference
    ? formik.values.providedBy.reference.split("/")[1]
    : undefined;

  return (
    <Form
      layout="vertical"
      onFinish={formik.handleSubmit}
      className="grid grid-cols-1 lg:grid-cols-2 gap-6"
    >
      {/* Columna Izquierda */}
      <div className="space-y-4">
        {/* Nombre */}
        <Form.Item
          label="Nombre"
          required
          validateStatus={
            formik.touched.name && formik.errors.name ? "error" : undefined
          }
          help={formik.touched.name && formik.errors.name}
        >
          <Input
            name="name"
            placeholder="Ej. Radiografía Dental"
            size="large"
            value={formik.values.name || ""}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            disabled={isPending}
          />
        </Form.Item>

        {/* Abreviatura */}
        <Form.Item
          label="Abreviatura"
          required
          validateStatus={
            formik.touched.abbreviation && formik.errors.abbreviation
              ? "error"
              : undefined
          }
          help={formik.touched.abbreviation && formik.errors.abbreviation}
        >
          <Input
            name="abbreviation"
            placeholder="Ej. RD"
            size="large"
            value={formik.values.abbreviation || ""}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            disabled={isPending}
          />
        </Form.Item>

        {/* Costo */}
        <Form.Item
          label="Costo"
          validateStatus={
            formik.touched.cost && formik.errors.cost ? "error" : undefined
          }
          help={formik.touched.cost && formik.errors.cost}
        >
          <InputNumber
            placeholder="Ej. 300.00"
            size="large"
            style={{ width: "100%" }}
            min={0}
            step={0.01}
            precision={2}
            prefix="L."
            value={formik.values.cost}
            onChange={(value) => formik.setFieldValue("cost", value || 0)}
            onBlur={() => formik.setFieldTouched("cost", true)}
            disabled={isPending}
          />
        </Form.Item>

        {/* Descripción */}
        <Form.Item
          label="Descripción"
          validateStatus={
            formik.touched.comment && formik.errors.comment ? "error" : undefined
          }
          help={formik.touched.comment && formik.errors.comment}
        >
          <TextArea
            name="comment"
            rows={4}
            placeholder="Ej. El servicio de radiografía es de alta calidad."
            value={formik.values.comment || ""}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            disabled={isPending}
          />
        </Form.Item>

        {/* Activo */}
        <Form.Item label="Activo" valuePropName="checked">
          <Switch
            checked={formik.values.active}
            onChange={(checked) => formik.setFieldValue("active", checked)}
            checkedChildren="Sí"
            unCheckedChildren="No"
            disabled={isPending}
          />
        </Form.Item>
      </div>

      {/* Columna Derecha */}
      <div className="space-y-4">
        {/* Organización */}
        <Form.Item
          label="Organización"
          validateStatus={
            formik.touched.providedBy && formik.errors.providedBy
              ? "error"
              : undefined
          }
          help={
            formik.touched.providedBy && formik.errors.providedBy
              ? typeof formik.errors.providedBy === "string"
                ? formik.errors.providedBy
                : "Debe seleccionar una organización"
              : undefined
          }
        >
          <Select
            placeholder="Seleccione una organización"
            size="large"
            style={{ width: "100%" }}
            value={organizationId}
            onChange={handleOrganizationChange}
            onBlur={() => formik.setFieldTouched("providedBy", true)}
            options={organizations.map((org) => ({
              label: org.name,
              value: org.id,
            }))}
            disabled={isPending}
          />
        </Form.Item>

        {/* Ubicaciones */}
        <Form.Item label="Seleccione los módulos o áreas disponibles">
          <Input.Search
            placeholder="Buscar"
            allowClear
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            className="mb-3"
            disabled={isPending}
          />

          <div className="border border-gray-300 rounded-lg overflow-hidden">
            <Table
              rowSelection={{
                type: "checkbox",
                ...rowSelection,
              }}
              columns={columns}
              dataSource={filteredLocations}
              rowKey="id"
              pagination={{
                pageSize: 10,
                showSizeChanger: false,
                showTotal: (total, range) =>
                  `${range[0]}-${range[1]} de ${total}`,
              }}
              size="small"
            />
          </div>

          {selectedLocations.length === 0 && (
            <div className="text-amber-600 text-sm mt-2">
              ⚠️ Recomendado: Seleccione al menos una ubicación
            </div>
          )}
        </Form.Item>
      </div>

      {/* Botones */}
      <div className="lg:col-span-2 flex justify-end gap-3 mt-4">
        <Button
          type="primary"
          size="large"
          onClick={onCancel}
          className="px-8"
          disabled={isPending}
          style={{
            backgroundColor: "red",
            color: "white",
          }}
        >
          Cancelar
        </Button>
        <Button
          type="primary"
          size="large"
          htmlType="submit"
          className="px-8"
          loading={isPending}
          disabled={isPending}
          style={{
            backgroundColor: "green",
            color: "white",
          }}
        >
          {submitButtonText}
        </Button>
      </div>
    </Form>
  );
};
