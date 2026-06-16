import { ModalForm, type ProFormInstance } from "@ant-design/pro-components";
import { useEffect } from "react";
import PractitionerRoleForm from "./PractitionerRoleForm";
type SelectOption = { label: string; value: string };

type PractitionerRoleModalProps = {
  title?: string;
  open: boolean;
  orgOptions: SelectOption[];
  locationOptions: SelectOption[];
  initialValues?: any;
  formRef?: React.RefObject<ProFormInstance | null>;
  onOpenChange: (open: boolean) => void;
  onSubmit: (values: any) => Promise<boolean>;
};

export default function PractitionerRoleModal({
  title,
  open,
  orgOptions,
  locationOptions,
  initialValues,
  formRef,
  onOpenChange,
  onSubmit,
}: PractitionerRoleModalProps) {
  useEffect(() => {
    if (formRef?.current && initialValues) {
      formRef.current.setFieldsValue(initialValues);
    }
  }, [initialValues, formRef]);

  return (
    <ModalForm
      title={title}
      open={open}
      onOpenChange={onOpenChange}
      formRef={formRef}
      modalProps={{ destroyOnClose: true }}
      initialValues={initialValues}
      onFinish={onSubmit}
    >
      <PractitionerRoleForm
        orgOptions={orgOptions}
        locationOptions={locationOptions}
      />
    </ModalForm>
  );
}