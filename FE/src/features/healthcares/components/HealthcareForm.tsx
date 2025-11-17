import { useState, useEffect } from "react";
import { Table, Input, Tag } from "antd";
import type { ColumnsType } from "antd/es/table";
import type { CreateHealthcareDto, HealthcareDto, LocationDto, OrganizationDto } from "../../../api/models";
import {
  ProForm,
  ProFormText,
  ProFormTextArea,
  ProFormDigit,
  ProFormSelect,
  ProFormSwitch,
} from "@ant-design/pro-components";
import { HealthcareExtensionsUrls } from "../../../shared/constants";

interface HealthcareFormProps {
  initialValues?: Partial<HealthcareDto>;
  organizations: OrganizationDto[];
  locations: LocationDto[];
  onFinish: (values: CreateHealthcareDto) => Promise<void>;
  submitButtonText?: string;
  isPending?: boolean;
  onCancel: () => void;
}

export const HealthcareForm = ({
  initialValues,
  organizations,
  locations,
  submitButtonText = "Crear servicio",
  isPending = false,
  onFinish,
  onCancel,
}: HealthcareFormProps) => {
  const [searchText, setSearchText] = useState("");
  const [selectedRowKeys, setSelectedRowKeys] = useState<string[]>([]);
  const [selectedLocations, setSelectedLocations] = useState<LocationDto[]>([]);

  // Inicializar ubicaciones seleccionadas desde initialValues
  useEffect(() => {
    if (initialValues?.location && initialValues.location.length > 0) {
      const locationIds = initialValues.location
        .map((loc) => loc.reference?.split("/")[1])
        .filter((id): id is string => typeof id === "string");
      
      setSelectedRowKeys(locationIds);
      
      const selectedLocs = locations.filter((loc: LocationDto) =>
        locationIds.includes(loc.id || "")
      );
      setSelectedLocations(selectedLocs);
    }
  }, [initialValues?.location, locations]);

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
      render: (status) => {
        const normalized = status.toLowerCase();
        if (normalized === "active") return <Tag color="green">✓ Activo</Tag>;
        if (normalized === "suspended") return <Tag color="orange">⚠︎ Suspendido</Tag>;
        if (normalized === "inactive") return <Tag color="red">✗ Inactivo</Tag>; 
        return "-";
      },
    },
  ];

  // Selección de ubicaciones
  const rowSelection = {
    selectedRowKeys,
    onChange: (newSelectedRowKeys: React.Key[], selectedRows: LocationDto[]) => {
      setSelectedRowKeys(newSelectedRowKeys as string[]);
      setSelectedLocations(selectedRows);
    },
  };

  // Manejar submit del formulario
  const handleFinish = async (values: any) => {
    // Construir el objeto HealthcareDto
    const healthcareData: CreateHealthcareDto = {
      ...values,
      location: selectedLocations.map((loc) => ({
        reference: `Location/${loc.id}`,
        display: loc.name,
      })),
      providedBy: values.providedBy
        ? {
            reference: `Organization/${values.providedBy}`,
            display: organizations.find((org) => org.id === values.providedBy)?.name || "",
          }
        : undefined,
    };

    await onFinish(healthcareData);
  };

  // Extraer el ID de la organización para el Select
  const organizationId = initialValues?.providedBy?.reference
    ? initialValues.providedBy.reference.split("/")[1]
    : undefined;

  return (
    <ProForm
      layout="vertical"
      onFinish={handleFinish}
      submitter={{
        render: (_) => (
          <div className="flex justify-end gap-3 mt-4">
            <button
              type="button"
              onClick={onCancel}
              disabled={isPending}
              className="px-8 py-2 text-white bg-red-500 hover:bg-red-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={isPending}
              className="px-8 py-2 text-white bg-green-500 hover:bg-green-600 rounded-lg disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isPending ? "Procesando..." : submitButtonText}
            </button>
          </div>
        ),
      }}
      grid
      initialValues={{
        name: initialValues?.name || "",
        abbreviation: initialValues?.extension?.find(ext => 
          ext.url === HealthcareExtensionsUrls.abbreviation // validar que la url sea correcta
        )?.valueString || "",
        cost: initialValues?.extension?.find(ext => 
          ext.url === HealthcareExtensionsUrls.cost // validar que la url sea correcta
        )?.valueDecimal || 0,
        comment: initialValues?.comment || "",
        active: initialValues?.active ?? true,
        providedBy: organizationId,
      }}
    >
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 w-full">
        {/* Columna Izquierda */}
        <div className="space-y-4">
          {/* Nombre */}
          <ProFormText
            name="name"
            label="Nombre"
            placeholder="Ej. Radiografía Dental"
            rules={[
              { required: true, message: "El nombre es requerido" },
              { min: 3, message: "El nombre debe tener al menos 3 caracteres" },
            ]}
            fieldProps={{
              size: "large",
              disabled: isPending,
            }}
          />

          {/* Abreviatura */}
          <ProFormText
            name="abbreviation"
            label="Abreviatura"
            placeholder="Ej. RD"
            rules={[
              { required: true, message: "La abreviatura es requerida" },
              { max: 10, message: "La abreviatura no puede tener más de 10 caracteres" },
            ]}
            fieldProps={{
              size: "large",
              disabled: isPending,
            }}
          />

          {/* Costo */}
          <ProFormDigit
            name="cost"
            label="Costo"
            placeholder="Ej. 300.00"
            min={0}
            fieldProps={{
              size: "large",
              precision: 2,
              prefix: "L.",
              disabled: isPending,
              style: { width: "100%" },
            }}
          />

          {/* Descripción */}
          <ProFormTextArea
            name="comment"
            label="Descripción"
            placeholder="Ej. El servicio de radiografía es de alta calidad."
            fieldProps={{
              rows: 4,
              disabled: isPending,
            }}
          />

          {/* Activo */}
          <ProFormSwitch
            name="active"
            label="Activo"
            checkedChildren="Sí"
            unCheckedChildren="No"
            fieldProps={{
              disabled: isPending,
            }}
          />
        </div>

        {/* Columna Derecha */}
        <div className="space-y-4">
          {/* Organización */}
          <ProFormSelect
            name="providedBy"
            label="Organización"
            placeholder="Seleccione una organización"
            options={organizations.map((org) => ({
              label: org.name,
              value: org.id,
            }))}
            fieldProps={{
              size: "large",
              disabled: isPending,
            }}
          />

          {/* Ubicaciones */}
          <div>
            <label className="block mb-2 text-sm font-medium">
              Seleccione las ubicaciones o áreas donde se ofrece el servicio
            </label>
            
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
          </div>
        </div>
      </div>
    </ProForm>
  );
};